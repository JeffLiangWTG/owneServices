using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	abstract class MultiEmailColumnStyleTest : TestCaseWithFactory
	{
		public void TestReturnEditControl()
		{
			using (ColumnStyle)
			{
				AssertEquals("EditControl is AddressOverrideCombinationControl", ExpectedControlType, ColumnStyle.EditControl.GetType());

				var m = new Message();
				Assert(ColumnStyle.ShouldProcessCmdKey(ref m, Keys.Control | Keys.E));

				ColumnStyle.ReadOnly = true;
				Assert(!ColumnStyle.ShouldProcessCmdKey(ref m, Keys.Control | Keys.E));
			}
		}

		protected abstract MultiEmailColumnStyle ColumnStyle { get; }
		protected abstract Type ExpectedControlType { get; }
	}
}
