//Work Time Automatic Control StopWatch use Google Spreadsheets to save your work information in the cloud.
//Copyright (C) 2013  Tomayly Dmitry
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see http://www.gnu.org/licenses/.
//
//Google APIs Client Library for .NET license: http://www.apache.org/licenses/LICENSE-2.0 .
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Extends aero effect on whole application window
	/// </summary>
	internal class GlassHelper
	{
		[DllImport("dwmapi.dll", PreserveSig = false)]
		static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

		[DllImport("dwmapi.dll", PreserveSig = false)]
		static extern bool DwmIsCompositionEnabled();

		internal struct MARGINS
		{
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;

			public MARGINS(Thickness t)
			{
				Left = (int)t.Left;
				Right = (int)t.Right;
				Top = (int)t.Top;
				Bottom = (int)t.Bottom;
			}
		}

		/// <summary>
		/// Extends aero effect on whole application window if it is possible
		/// </summary>
		/// <returns>true - if operation succeeded, false - oterwise</returns>
		/// <exception cref="InvalidOperationException"></exception>
		internal static bool ExtendGlassFrame(Window window, Thickness margin)
		{
			if ((Environment.OSVersion.Version.Major < 6) || !DwmIsCompositionEnabled())
				return false;

			var hwnd = new WindowInteropHelper(window).Handle;
			if (hwnd == IntPtr.Zero)
				throw new InvalidOperationException("The Window must be shown before extending glass.");

			// Set the background to transparent from both the WPF and Win32 perspectives
			window.Background = Brushes.Transparent;
			if (HwndSource.FromHwnd(hwnd) != null)
			{
				HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;
			}
			

			var margins = new MARGINS(margin);
			DwmExtendFrameIntoClientArea(hwnd, ref margins);

			return true;
		}
	}
}
