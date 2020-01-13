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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenLock
{
  using System.Diagnostics;
  using static NativeMethods;

  public partial class FrmMain : Form
  {
    private IntPtr hookHandleId;
    private HookProc hookDelegate;
    private EventHandler unLocking;

    private CursorManager cursorManager = new CursorManager();
    private KeyState keyState = new KeyState();
    private List<FrmScreen> screenCollection = new List<FrmScreen>();

    public FrmMain()
    {
      this.InitializeComponent();
      this.unLocking += this.OnUnlocking;
    }

    private void OnUnlocking()
    {
      this.unLocking?.Invoke(this, new EventArgs());
    }

    private void OnUnlocking(object sender, EventArgs e)
    {
      if (this.hookHandleId != IntPtr.Zero)
      {
        UnhookWindowsHookEx(this.hookHandleId);
        Trace.WriteLine("キーボードフックを解除しました (2)", "KeyboardHook");
      }

      this.timerCursorBlocker.Stop();
      this.ScreenClean();
      this.cursorManager.Show();
      this.keyState.Reset();
      this.Show();
    }

    private bool KeyboardHook()
    {
      try
      {
        if (this.hookHandleId != IntPtr.Zero)
        {
          UnhookWindowsHookEx(this.hookHandleId);
          Trace.WriteLine("キーボードフックを解除しました (1)", "KeyboardHook");
        }

        var hMod = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]);
        this.hookDelegate = new HookProc(this.HookCallback);
        this.hookHandleId = SetWindowsHookEx(WM_KEYBOARD_LL, this.hookDelegate, hMod, 0);
        if (this.hookHandleId == IntPtr.Zero)
        {
          var errorCode = Marshal.GetLastWin32Error();
          throw new Win32Exception(errorCode);
        }
        Trace.WriteLine("キーボードフックを開始しました (1)", "KeyboardHook");
        return true;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      return false;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);
      if (this.hookHandleId != IntPtr.Zero)
      {
        UnhookWindowsHookEx(this.hookHandleId);
      }
    }

    private int HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode < 0)
      {
        return CallNextHookEx(this.hookHandleId, nCode, wParam, lParam);
      }

      Trace.WriteLine("キーを検知", "Callback");
      var os = Environment.OSVersion;
      var kbd = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
      if ((os.Version.Major == 6 && os.Version.Minor >= 2) || os.Version.Major > 6)
      {
        if (kbd.vkCode == VK_LSHIFT || kbd.vkCode == VK_RSHIFT)
        {
          this.keyState.Shift = true;
        }
        if (kbd.vkCode == VK_LCONTROL || kbd.vkCode == VK_RCONTROL)
        {
          this.keyState.Ctrl = true;
        }
        if (kbd.vkCode == VK_LMENU || kbd.vkCode == VK_RMENU)
        {
          this.keyState.Alt = true;
        }

        // Whether a hot key was pressed.
        // Shift + Ctrl + Alt + Delete
        if (this.keyState.IsHotKeyPressed())
        {
          if (kbd.vkCode == VK_DELETE)
          {
            this.keyState.Delete = true;
            this.OnUnlocking();
            return CallNextHookEx(this.hookHandleId, nCode, wParam, lParam);
          }
        }
      }
      else
      {
        // not working ?
        this.keyState.Shift = this.IsKeyPressed(VK_LSHIFT);
        this.keyState.Ctrl = this.IsKeyPressed(VK_LCONTROL);
        this.keyState.Alt = this.IsKeyPressed(VK_LMENU);

        // Whether a hot key was pressed.
        // Shift + Ctrl + Alt + Delete
        if (this.keyState.IsHotKeyPressed())
        {
          if (this.IsKeyPressed(VK_DELETE))
          {
            this.keyState.Delete = true;
            this.OnUnlocking();
            return CallNextHookEx(this.hookHandleId, nCode, wParam, lParam);
          }
        }
      }
      return 1;
    }

    private void ButtonBlocker_Click(object sender, EventArgs e)
    {
      try
      {
        if (this.KeyboardHook())
        {
          this.Hide();
          this.cursorManager.Hide();
          this.ScreenBlocker();
          if (this.timerCursorBlocker.Enabled)
          {
            this.timerCursorBlocker.Stop();
          }
          this.timerCursorBlocker.Start();
        }
        else
        {
          MessageBox.Show("キーボードフックに失敗しました。(win32)", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public bool IsKeyPressed(int keyCode)
    {
      return (GetAsyncKeyState(keyCode) & 0x8000) > 0;
    }

    public void ScreenBlocker()
    {
      this.screenCollection.Clear();
      Screen.AllScreens.ToList().ForEach(s =>
      {
        var form = new FrmScreen()
        {
          Bounds = s.Bounds
        };
        form.Show(this);
        this.screenCollection.Add(form);
      });
    }

    public void ScreenClean() => this.screenCollection.ForEach(s => { s.Close(); });

    public void SetWindowOpacity(double opacity)
    {
      this.Opacity = opacity;
      this.screenCollection.ForEach(s => s.Opacity = opacity);
    }

    private void TimerCursorBlocker_Tick(object sender, EventArgs e)
    {
      this.cursorManager.LeftMost();
    }

    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      MessageBox.Show($"バージョン: {Application.ProductVersion}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
  }
}
