using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	public class USOrgSupplierBuyerLinkAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUS_YesNoList()
		{
			AssertEquals(typeof(YesNoDefaultList), Lookups.YesNoList.GetType());
			Assert("Should contain No", Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.No));
			Assert("Should contain Yes", Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.Yes));
			Assert("Should not contain Default", !Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.Default));
		}

		public void TestOtherReconIssueList()
		{
			AssertEquals("OtherReconIssueList", typeof(ReconIssueCodeList), Lookups.OtherReconIssueList.GetType());
			AssertEquals("not existing ReconIssueCodeList.Codes.FTA", false, Lookups.OtherReconIssueList.ContainsCode(ReconIssueCodeList.Codes.FTA));
		}

		public void TestUltConsigneeTypeList()
		{
			AssertEquals("UltConsigneeTypeList", typeof(UltimateConsigneeTypeList), Lookups.UltConsigneeTypeList.GetType());
		}

		public void TestEntryTypes()
		{
			AssertEquals("CodeDescriptionPairList", typeof(CodeDescriptionPairList), Lookups.EntryTypes.GetType());
			Assert("Should contain ConsumptionFreeDutiable", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.ConsumptionFreeDutiable));
			Assert("Should contain ConsumptionQuotaVisa", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.ConsumptionQuotaVisa));
			Assert("Should contain ConsumptionADDCVD", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.ConsumptionADDCVD));
			Assert("Should contain ConsumptionFTZ", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.ConsumptionFTZ));
			Assert("Should contain ConsumptionADDCVDQuotaVisa", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa));
			Assert("Should contain InformalFreeDutiable", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.InformalFreeDutiable));
			Assert("Should contain InformalQuotaVisa", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.InformalQuotaVisa));
			Assert("Should contain Warehouse", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.Warehouse));
			Assert("Should contain ReWarehouse", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.ReWarehouse));
			Assert("Should contain TemporaryImportationBond", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.TemporaryImportationBond));
			Assert("Should contain WarehouseWithdrawalConsumption", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalConsumption));
			Assert("Should contain WarehouseWithdrawalQuota", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalQuota));
			Assert("Should contain WarehouseWithdrawalADDCVD", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			Assert("Should contain WarehouseWithdrawalADDCVDQuotaVisa", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa));
			Assert("Should contain DCASR", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.DCASR));
			Assert("Should contain GovernmentDutiable", Lookups.EntryTypes.ContainsCode(EntryTypeList.Codes.GovernmentDutiable));
		}

		USOrgSupplierBuyerLinkAddInfoLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					var link = Factory.New<OrgSupplierBuyerLink>();
					lookups = new USOrgSupplierBuyerLinkAddInfoLookups(new USOrgSupplierBuyerLinkAddInfo(link.GetAddInfo()));
				}
				return lookups;
			}
		}
		USOrgSupplierBuyerLinkAddInfoLookups lookups;
	}
}
