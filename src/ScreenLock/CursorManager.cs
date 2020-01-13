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
using System.Drawing;
using System.Runtime.InteropServices;

namespace ScreenLock
{
  using static NativeMethods;

  [ComVisible(true)]
  public sealed class CursorManager
  {
    private Point leftMost = new Point(0, 0);

    public Point Position {
      get {
        POINT p = new POINT();
        GetCursorPos(p);
        return new Point(p.X, p.Y);
      }
      set {
        SetCursorPos(value.X, value.Y);
      }
    }

    public Point CursorPosition {
      get;
      private set;
    }

    public void LeftMost()
    {
      this.Position = this.leftMost;
    }

    public Point SetCursorPosition {
      set {
        if (value == null) throw new ArgumentNullException(nameof(value));
        this.CursorPosition = value;
      }
    }

    public CursorManager()
    {
    }

    public CursorManager(Point position)
    {
      this.CursorPosition = position;
    }

    public void Show()
    {
      SetCursorPos(this.CursorPosition.X, this.CursorPosition.Y);
      ShowCursor(true);
    }

    public void Hide()
    {
      this.CursorPosition = this.Position;
      ShowCursor(false);
    }
  }
}
