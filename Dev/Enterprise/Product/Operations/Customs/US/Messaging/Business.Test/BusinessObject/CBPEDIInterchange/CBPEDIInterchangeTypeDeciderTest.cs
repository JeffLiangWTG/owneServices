using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class CBPEDIInterchangeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestAppliedToEDIInterchange()
		{
			Enterprise.Messaging.Business.EDIInterchange interchange = Factory.New<Enterprise.Messaging.Business.EDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIMessage.ApplicationCodes.USCustomsExport;
			interchange.EI_InterchangeType = "DIR";
			interchange.EI_From = "WHO";
			interchange.EI_To = "WHO";
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AssertEquals(typeof(Enterprise.Messaging.Business.EDIInterchange), factory2.Load<Enterprise.Messaging.Business.EDIInterchange>(interchange.PK).GetType());
		}

		public void TestGetTypeForLoad()
		{
			CBPEDIInterchangeTypeDecider typeDecider = new CBPEDIInterchangeTypeDecider();
			Enterprise.Messaging.Business.EDIInterchange intercharge = Factory.New<Enterprise.Messaging.Business.EDIInterchange>();
			DataRow row = ((INeedRow)intercharge).Row;
			AssertEquals("Default Message", typeof(Enterprise.Messaging.Business.EDIInterchange), typeDecider.GetTypeForLoad(row, Factory));
			intercharge.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsExport;
			AssertEquals("Interchange", typeof(Enterprise.Messaging.Business.EDIInterchange), typeDecider.GetTypeForLoad(row, Factory));
			intercharge.EI_InterchangeType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertEquals("Interchange", typeof(Enterprise.Messaging.Business.EDIInterchange), typeDecider.GetTypeForLoad(row, Factory));
			intercharge.EI_InterchangeType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			AssertEquals("Interchange", typeof(CBPEDIInterchange), typeDecider.GetTypeForLoad(row, Factory));
			intercharge.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			AssertEquals("Interchange", typeof(CBPEDIInterchange), typeDecider.GetTypeForLoad(row, Factory));
			intercharge.EI_InterchangeType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			AssertEquals("Interchange", typeof(CBPEDIInterchange), typeDecider.GetTypeForLoad(row, Factory));
		}
	}
}
