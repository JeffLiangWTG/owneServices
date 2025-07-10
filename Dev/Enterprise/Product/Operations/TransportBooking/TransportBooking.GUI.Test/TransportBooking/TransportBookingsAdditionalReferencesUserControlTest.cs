using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportBookings.GUI.Testing
{
	sealed class TransportBookingsAdditionalReferencesUserControlTest : TestCaseWithFactory
	{
		public void TestDisplayDetailPanel()
		{
			var defaultValueAttribute = (DefaultValueAttribute)Attribute.GetCustomAttribute(typeof(TransportBookingsAdditionalReferencesUserControl).GetProperty("DisplayDetailPanel"), typeof(DefaultValueAttribute));
			AssertEquals("Requires default value or the public property will serialize incorrectly to false in the designer.", true, defaultValueAttribute.Value);

			using (var additionalReferencesControl = new TransportBookingsAdditionalReferencesUserControl())
			{
				AssertEquals(true, additionalReferencesControl.Controls.Find("numberDetailsPanel", true)[0].Visible);
				AssertEquals(true, additionalReferencesControl.DisplayDetailPanel);

				additionalReferencesControl.DisplayDetailPanel = false;
				AssertEquals(false, additionalReferencesControl.Controls.Find("numberDetailsPanel", true)[0].Visible);

				additionalReferencesControl.DisplayDetailPanel = true;
				AssertEquals(true, additionalReferencesControl.Controls.Find("numberDetailsPanel", true)[0].Visible);
			}
		}
	}
}
