// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;

namespace System.Windows.Forms.TestUtilities
{
	public static class CommonTestHelperEx
	{
		public static IEnumerable<TestCaseData> GetBackColorTheoryData()
		{
			yield return new TestCaseData(Color.Red, Color.Red);
			yield return new TestCaseData(Color.Empty, Control.DefaultBackColor);
		}

		public static IEnumerable<TestCaseData> GetForeColorTheoryData()
		{
			yield return new TestCaseData(Color.Red, Color.Red);
			yield return new TestCaseData(Color.FromArgb(254, 1, 2, 3), Color.FromArgb(254, 1, 2, 3));
			yield return new TestCaseData(Color.White, Color.White);
			yield return new TestCaseData(Color.Black, Color.Black);
			yield return new TestCaseData(Color.Empty, Control.DefaultForeColor);
		}

		public static IEnumerable<TestCaseData> GetImageTheoryData()
		{
			yield return new TestCaseData(new Bitmap(10, 10));
			yield return new TestCaseData(null);
		}

		public static IEnumerable<TestCaseData> GetFontTheoryData()
		{
			yield return new TestCaseData(SystemFonts.MenuFont);
			yield return new TestCaseData(null);
		}

		public static IEnumerable<TestCaseData> GetTypeWithNullTheoryData()
		{
			yield return new TestCaseData(null);
			yield return new TestCaseData(typeof(int));
		}

		public static IEnumerable<TestCaseData> GetRightToLeftTheoryData()
		{
			yield return new TestCaseData(RightToLeft.Inherit, RightToLeft.No);
			yield return new TestCaseData(RightToLeft.Yes, RightToLeft.Yes);
			yield return new TestCaseData(RightToLeft.No, RightToLeft.No);
		}

		public static IEnumerable<TestCaseData> GetPaddingTheoryData()
		{
			yield return new TestCaseData(new Padding());
			yield return new TestCaseData(new Padding(1, 2, 3, 4));
			yield return new TestCaseData(new Padding(1));
			yield return new TestCaseData(new Padding(-1, -2, -3, -4));
		}

		public static IEnumerable<TestCaseData> GetPaddingNormalizedTheoryData()
		{
			yield return new TestCaseData(new Padding(), new Padding());
			yield return new TestCaseData(new Padding(1, 2, 3, 4), new Padding(1, 2, 3, 4));
			yield return new TestCaseData(new Padding(1), new Padding(1));
			yield return new TestCaseData(new Padding(-1, -2, -3, -4), Padding.Empty);
		}

		public static IEnumerable<TestCaseData> GetCursorTheoryData()
		{
			yield return new TestCaseData(null);
			//yield return new TestCaseData(new Cursor((IntPtr)1));
		}

		public static IEnumerable<TestCaseData> GetPaintEventArgsTheoryData()
		{
			yield return new TestCaseData(null);
			yield return new TestCaseData(new PaintEventArgs(new BGraphics(), Rectangle.Empty));
		}

		public static IEnumerable<TestCaseData> GetKeyEventArgsTheoryData()
		{
			yield return new TestCaseData(new KeyEventArgs(Keys.None));
			yield return new TestCaseData(new KeyEventArgs(Keys.Cancel));
		}

		public static IEnumerable<TestCaseData> GetKeyPressEventArgsTheoryData()
		{
			yield return new TestCaseData(null);
			yield return new TestCaseData(new KeyPressEventArgs('1'));
		}

		public static IEnumerable<TestCaseData> GetLayoutEventArgsTheoryData()
		{
			yield return new TestCaseData(null);
			//yield return new TestCaseData(new LayoutEventArgs(null, null));
			yield return new TestCaseData(new LayoutEventArgs(new Control(), "affectedProperty"));
		}

		public static IEnumerable<TestCaseData> GetMouseEventArgsTheoryData()
		{
			yield return new TestCaseData(null);
			yield return new TestCaseData(new MouseEventArgs(MouseButtons.Left, 1, 2, 3, 4));
			//yield return new TestCaseData(new HandledMouseEventArgs(MouseButtons.Left, 1, 2, 3, 4));
		}

		/*public static IEnumerable<TestCaseData> GetEditValueInvalidProviderTestData()
		{
			var nullServiceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
			nullServiceProviderMock
				.Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
				.Returns(null);
			var invalidServiceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
			invalidServiceProviderMock
				.Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
				.Returns(new object());
			var value = new object();
			return new TheoryData<IServiceProvider, object>
			{
				{ null, null },
				{ null, value },
				{ nullServiceProviderMock.Object, null },
				{ nullServiceProviderMock.Object, value },
				{ invalidServiceProviderMock.Object, null },
				{ invalidServiceProviderMock.Object, value }
			};
		}*/

		public static IEnumerable<TestCaseData> GetITypeDescriptorContextTestData()
		{
			yield return new TestCaseData(null);
			yield return new TestCaseData(new Mock<ITypeDescriptorContext>(MockBehavior.Strict).Object);
		}
	}
}
