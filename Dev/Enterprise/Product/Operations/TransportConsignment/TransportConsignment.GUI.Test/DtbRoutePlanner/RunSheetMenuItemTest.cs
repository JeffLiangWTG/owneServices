using System;
using System.Windows.Forms;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public abstract class RunSheetMenuItemTest : TestCase
	{
		#region TestProperties

		public void TestProperties()
		{
			var item = GetNewRunSheetMenuItem(null);
			AssertEquals(ToolStripItemImageScaling.None, item.ImageScaling);

			AssertEquals("Precondition - should not be empty.", false, GetExpectedCaption().IsEmpty);
			AssertEquals(GetExpectedCaption(), item.CaptionResourceString.Caption);

			AssertEquals("Precondition - should not be empty.", false, GetExpectedName().IsEmpty);
			AssertEquals(GetExpectedName(), item.Name);
		}

		protected abstract ZString GetExpectedCaption();
		protected abstract ZString GetExpectedName();

		#endregion

		#region TestClick

		public void TestClick()
		{
			bool clickFired = false;
			var item = GetNewRunSheetMenuItem((sender, e) =>
			{
				clickFired = true;
			});

			item.PerformClick();
			AssertEquals(true, clickFired);
		}

		#endregion

		#region Implementation

		protected abstract RunSheetMenuItem GetNewRunSheetMenuItem(EventHandler clickEventHandler);

		#endregion
	}
}
