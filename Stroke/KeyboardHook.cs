using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Stroke
{
    public static class KeyboardHook
    {
        public enum KeyStates
        {
            None,
            Down,
            Up
        }

        public class KeyboardActionArgs
        {
            public readonly Keys Key;
            public readonly KeyStates KeyState;

            public KeyboardActionArgs(Keys key, KeyStates keyState)
            {
                Key = key;
                KeyState = keyState;
            }
        }

        public delegate bool KeyboardActionHandler(object sender, KeyboardActionArgs e);
        public static event KeyboardActionHandler KeyboardAction;
        public static bool Enable = false;

        private static API.HOOKPROC _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static void StartHook()
        {
            _hookID = SetHook(_proc);
            Enable = true;
        }

        public static void StopHook()
        {
            API.UnhookWindowsHookEx(_hookID);
            Enable = false;
        }

        private static IntPtr SetHook(API.HOOKPROC proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return API.SetWindowsHookEx(API.WH_KEYBOARD_LL, proc, API.GetModuleHandle(module.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || Enable == false)
            {
                return API.CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            API.KBDLLHOOKSTRUCT hookStruct = (API.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(API.KBDLLHOOKSTRUCT));

            Keys key = (Keys)hookStruct.vkCode;
            KeyStates keyState = KeyStates.None;

            switch ((API.KeyboardMessages)wParam)
            {
                case API.KeyboardMessages.WM_KEYDOWN:
                    {
                        keyState = KeyStates.Down;
                    }
                    break;
                case API.KeyboardMessages.WM_KEYUP:
                    {
                        keyState = KeyStates.Up;
                    }
                    break;
            }

            if (KeyboardAction(null, new KeyboardActionArgs(key, keyState)))
            {
                return (IntPtr)1;
            }

            return API.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

    }
}
