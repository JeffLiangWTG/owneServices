using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public partial class SpecialHandlingDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestConsolExportsSpecialHandlingCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var sh1 = consol.AWBSpecialHandlingItems.AddNew();
			sh1.JKH_Code = "ACT";

			var writer = new SpecialHandlingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
			var specialHandlingData = writer.GetDataObject(sh1);

			AssertEquals("ACT", specialHandlingData.Code);
			AssertEquals("Active Temperature Controlled System", specialHandlingData.Description);
		}
	}
}
