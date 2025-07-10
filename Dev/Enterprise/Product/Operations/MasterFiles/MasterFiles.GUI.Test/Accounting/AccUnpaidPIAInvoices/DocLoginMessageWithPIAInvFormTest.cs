using System.Windows.Forms;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DocLoginMessageWithPIAInvForm))]
	sealed class DocLoginMessageWithPIAInvFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		class TestRelatedJobNumber : IRelatedJobNumber
		{
			public TestRelatedJobNumber(string[] jobNumber)
			{
				this.fJobNumber = jobNumber;
			}

			readonly string[] fJobNumber;

			public string[] JobNumber
			{
				get { return fJobNumber; }
			}
		}

		protected override Form GetFormToBashCore()
		{
			IRelatedJobNumber shipment = new TestRelatedJobNumber(new string[] { "S00009999" });
			return new DocLoginMessageWithPIAInvForm(shipment, "Message", Res.GetString("8875fd2e-ecc2-4fae-8056-f2c95da9f694", "Caption"));
		}
	}
}
