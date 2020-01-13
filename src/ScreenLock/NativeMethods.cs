/*
 * Copyright (c) 2020 Coreizer
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ScreenLock
{
  public class NativeMethods
  {
    public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SetWindowsHookEx(int hookid, HookProc pfnhook, IntPtr hMod, int threadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhook);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    public extern static int ShowCursor(bool bShow);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    [ResourceExposure(ResourceScope.None)]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    [ResourceExposure(ResourceScope.None)]
    public static extern bool GetCursorPos([In, Out] POINT pt);

    public const int WM_KEYBOARD_LL = 13;
    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;
    public const int WM_SYSKEYDOWN = 0x0104;
    public const int WM_SYSKEYUP = 0x0105;

    public const int VK_LSHIFT = 0xA0;
    public const int VK_RSHIFT = 0xA1;

    public const int VK_LCONTROL = 0xA2;
    public const int VK_RCONTROL = 0xA3;

    public const int VK_LMENU = 0xA4;
    public const int VK_RMENU = 0xA5;

    public const int VK_DELETE = 0x2E;

    [StructLayout(LayoutKind.Sequential)]
    public class KBDLLHOOKSTRUCT
    {
      public uint vkCode;
      public uint scanCode;
      public LLKHF flags;
      public uint time;
      public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class POINT
    {
      public int X;
      public int Y;

      public POINT() { }

      public POINT(int x, int y)
      {
        this.X = x;
        this.Y = y;
      }
    }

    [Flags]
    public enum LLKHF : uint
    {
      LLKHF_EXTENDED = 0x01,
      LLKHF_LOWER_IL_INJECTED = 0x00000010,
      LLKHF_INJECTED = 0x10,
      LLKHF_ALTDOWN = 0x20,
      LLKHF_UP = 0x80
    }
  }
}
