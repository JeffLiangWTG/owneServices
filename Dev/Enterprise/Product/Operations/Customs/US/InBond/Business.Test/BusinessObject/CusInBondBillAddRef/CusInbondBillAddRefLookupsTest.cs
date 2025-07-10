using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInbondBillAddRefLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalReferenceList()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			var additinalReference = bill.AdditionalReferences.AddNew();
			var list = additinalReference.Lookups.AdditionalReferenceList;
			AssertEquals(typeof(ReferenceQualifierList), list.GetType());
			AssertEquals(new ReferenceQualifierList().Count, list.Count);
			header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirContainer;
			moveHeader = header.MovementHeaders.AddNew();
			bill = header.Bills.AddNew();
			additinalReference = bill.AdditionalReferences.AddNew();
			list = additinalReference.Lookups.AdditionalReferenceList;
			AssertEquals(new ReferenceQualifierList().Count, list.Count);
			Assert(list.ContainsCode(ReferenceQualifierList.Codes.FEN));
			Assert(list.ContainsCode(ReferenceQualifierList.Codes.XC));
			Assert(list.ContainsCode(ReferenceQualifierList.Codes._2K));
			Assert(list.ContainsCode(ReferenceQualifierList.Codes.CSK));
			Assert(list.ContainsCode(ReferenceQualifierList.Codes.CUB));
		}
	}
}
