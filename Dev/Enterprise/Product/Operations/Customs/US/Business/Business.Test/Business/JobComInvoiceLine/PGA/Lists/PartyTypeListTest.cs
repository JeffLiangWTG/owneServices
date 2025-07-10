using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class PartyTypeListTest : TestCaseWithFactory
	{
		public void TestGetListForPSTNotifyPartyList()
		{
			var fullList = new PartyTypeList();
			var list1 = PartyTypeList.GetListForPSTNotifyPartyList(Factory);
			var list2 = PartyTypeList.GetListForPSTNotifyPartyList(Factory);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 2, list1.Count);
			AssertEquals(PartyTypeList.Descriptions.CustomsBroker, list1.GetDescriptionFromCode(PartyTypeList.Codes.CustomsBroker));
			AssertEquals(PartyTypeList.Descriptions.Importer, list1.GetDescriptionFromCode(PartyTypeList.Codes.Importer));
		}

		public void TestGetListForPSTCertifyingIndividual()
		{
			var fullList = new PartyTypeList();
			var list1 = PartyTypeList.GetListForPSTCertifyingIndividual(Factory);
			var list2 = PartyTypeList.GetListForPSTCertifyingIndividual(Factory);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 3, list1.Count);
			AssertEquals(PartyTypeList.Descriptions.CustomsBroker, list1.GetDescriptionFromCode(PartyTypeList.Codes.CustomsBroker));
			AssertEquals(PartyTypeList.Descriptions.Importer, list1.GetDescriptionFromCode(PartyTypeList.Codes.Importer));
			AssertEquals(PartyTypeList.Descriptions.Shipper, list1.GetDescriptionFromCode(PartyTypeList.Codes.Shipper));
		}

		public void TestGetListForNHTSACertifyingIndividual()
		{
			var fullList = new PartyTypeList();
			var list1 = PartyTypeList.GetListForNHTSACertifyingIndividual(Factory);
			var list2 = PartyTypeList.GetListForNHTSACertifyingIndividual(Factory);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 3, list1.Count);
			AssertEquals(PartyTypeList.Descriptions.CustomsBroker, list1.GetDescriptionFromCode(PartyTypeList.Codes.CustomsBroker));
			AssertEquals(PartyTypeList.Descriptions.Importer, list1.GetDescriptionFromCode(PartyTypeList.Codes.Importer));
			AssertEquals(PartyTypeList.Descriptions.Owner, list1.GetDescriptionFromCode(PartyTypeList.Codes.Owner));
		}

		public void TestGetListForVNECertifyingIndividual()
		{
			var fullList = new PartyTypeList();
			var list1 = PartyTypeList.GetListForVNECertifyingIndividual(Factory);
			var list2 = PartyTypeList.GetListForVNECertifyingIndividual(Factory);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 3, list1.Count);
			AssertEquals(PartyTypeList.Descriptions.CustomsBroker, list1.GetDescriptionFromCode(PartyTypeList.Codes.CustomsBroker));
			AssertEquals(PartyTypeList.Descriptions.Importer, list1.GetDescriptionFromCode(PartyTypeList.Codes.Importer));
			AssertEquals(PartyTypeList.Descriptions.Owner, list1.GetDescriptionFromCode(PartyTypeList.Codes.Owner));
		}

		public void TestGetListForFWSCertifyingIndividual()
		{
			var fullList = new PartyTypeList();
			var list1 = PartyTypeList.GetListForFWSCertifyingIndividual(Factory);
			var list2 = PartyTypeList.GetListForFWSCertifyingIndividual(Factory);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals("Count", 3, list1.Count);
			AssertEquals(PartyTypeList.Descriptions.CustomsBroker, list1.GetDescriptionFromCode(PartyTypeList.Codes.CustomsBroker));
			AssertEquals(PartyTypeList.Descriptions.FWSImporter, list1.GetDescriptionFromCode(PartyTypeList.Codes.FWSImporter));
			AssertEquals(PartyTypeList.Descriptions.FWSForeignExporter, list1.GetDescriptionFromCode(PartyTypeList.Codes.FWSForeignExporter));
		}
	}
}
