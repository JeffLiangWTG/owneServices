using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class TestExporterSchemeControlUK : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestControlIsShown()
		{
			using (var testForm = new ZForm())
			{
				using (var testControl = new ExporterSchemeControlUK())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();

					var groupBox = testForm.Controls.Find("MajorExporterGroupBox", true).FirstOrDefault() as ZGroupBox;
					AssertNotNull(groupBox);
					AssertEquals("The details entered here do not affect the calculation of known status in the Shipment Inspection field.", groupBox.CaptionResourceString.Caption);
				}
			}
		}
	}
}
