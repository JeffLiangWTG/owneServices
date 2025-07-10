using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class EmanifDocWrapperTest : TestCaseWithFactory
	{
		public void TestWrapperAtEmanifBillLevel()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_CustomsDischargePort = "TR0001-001";
			branch = Factory.NewWithValidTestData<GlbBranch>();
			company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			company.CompanyName = "TestName";
			company.Address1 = "Test adress 1";
			company.Address2 = "Test adress 2";
			company.Branches.Add(branch);
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BILL1";
			bill.ABL_ShipperName = "SHIPPERNAME";
			bill.ABL_ConsigneeName = "CONSIGNEENAME";
			bill.ABL_NotifyPartyName = "NOTIFYNAME";
			bill.ABL_RL_NKOrigin = "USLAX";
			bill.TransshipmentType = "4";
			Factory.Save();
			AsycudaManifestHeaderDocWrapper wrapperTR = (AsycudaManifestHeaderDocWrapper)AsycudaManifestHeaderDocWrapper.New(header, AsycudaManifestHeaderDocWrapper.TRManifestBillsforEMANIF, "");
			itemWrapper = new PreviousDeclarationsDocWrapper(wrapperTR, header.Bills[0], "PREV1");
			CombineAssertions("TR Properties Caption", () =>
			{
				AssertEquals("BILL1", itemWrapper.ABL_BillNumber);
				AssertEquals("SHIPPERNAME", itemWrapper.ABL_ShipperName);
				AssertEquals("CONSIGNEENAME", itemWrapper.ABL_ConsigneeName);
				AssertEquals("NOTIFYNAME", itemWrapper.ABL_NotifyPartyName);
				AssertEquals(ZString.Empty, itemWrapper.CompanyNameOfAgent);
				AssertEquals("USLAX", itemWrapper.ABL_RL_NKOrigin);
				AssertEquals("TR0001-001", itemWrapper.AMA_CustomsDischargePort);
				AssertEquals("H", itemWrapper.ContainerInformation);
				AssertEquals("PREV1", itemWrapper.CustomsEntryNumber);
				AssertEquals("E", itemWrapper.IsTransshipment);
			});
		}

		PreviousDeclarationsDocWrapper itemWrapper;
		AsycudaManifestHeader header;
		GlbCompany company;
		GlbBranch branch;
	}
}
