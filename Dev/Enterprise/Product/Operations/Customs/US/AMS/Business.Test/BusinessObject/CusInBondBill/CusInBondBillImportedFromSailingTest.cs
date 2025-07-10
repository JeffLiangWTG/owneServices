using System.Linq;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondBillImportedFromSailingTest : LinkedSailingBillsImportedTest
	{
		public void TestImporting()
		{
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "AAA");
			AssertNotNull(bill);
			AssertEquals("SCAC", bill.B0_IssuerCode);
			AssertEquals("AUSYD", bill.B0_RL_NKPortOfLading);
			AssertEquals(1000m, bill.B0_Weight);
			AssertEquals("KG", bill.B0_WeightUQ);
			AssertEquals(11, bill.B0_ManifestQty);
			AssertEquals("PLT", bill.B0_ManifestUQ);
			AssertEquals(consignor.PK, bill.ForeignShipper.OrganisationPK);
			AssertEquals(consignee.PK, bill.Consignee.OrganisationPK);
			AssertEquals(notifyParty.PK, bill.NotifyParty1.OrganisationPK);
			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BBB");
			AssertNotNull(bill);
			AssertEquals(1m, bill.B0_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.B0_WeightUQ);
			AssertEquals(2m, bill.B0_Volume);
			AssertEquals(Core.Constants.Volume.CubicFeet, bill.B0_VolumeUQ);
			AssertEquals(3, bill.B0_ManifestQty);
			AssertEquals(Core.Constants.PkgUnit.Box, bill.B0_ManifestUQ);
			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "CCC");
			AssertNotNull(bill);
			AssertEquals(1m, bill.B0_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, bill.B0_WeightUQ);
			AssertEquals(2m, bill.B0_Volume);
			AssertEquals(Core.Constants.Volume.CubicFeet, bill.B0_VolumeUQ);
			AssertEquals(3, bill.B0_ManifestQty);
			AssertEquals(Core.Constants.PkgUnit.Box, bill.B0_ManifestUQ);
			bill = header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "BN001");
			AssertNotNull(bill);
			AssertEquals("SCAC", bill.B0_IssuerCode);
		}
	}
}
