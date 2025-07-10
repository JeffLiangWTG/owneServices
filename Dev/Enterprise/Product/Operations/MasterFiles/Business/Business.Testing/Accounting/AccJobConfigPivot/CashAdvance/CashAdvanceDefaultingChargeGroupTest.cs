using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashAdvanceDefaultingChargeGroup))]
	sealed class CashAdvanceDefaultingChargeGroupTest : CashAdvanceDefaultingJobConfigPivotTest
	{
		public void TestReadOnly()
		{
			var companyConfig = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			companyConfig.CAC_ParentTableCode = ZString.Empty;
			var companyLevelPivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			companyLevelPivot.JCT_Code = ChargeCodeGroupList.Codes.Freight;
			companyLevelPivot.JCT_JCF_JobConfig = companyConfig.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var organizationConfig = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			organizationConfig.CAC_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			organizationConfig.CAC_ParentId = orgHeader.PK;
			organizationConfig.CAC_Ledger = LedgerTypes.AccountsReceivable;
			var orgLevelPivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			orgLevelPivot.JCT_Code = ChargeCodeGroupList.Codes.ShippingDisbursements;
			orgLevelPivot.JCT_JCF_JobConfig = organizationConfig.PK;

			Assert("Config outside of collection is always editable", !companyConfig.ReadOnly);
			Assert("Company Job Config is editable, so pivot is editable", !companyLevelPivot.ReadOnly);
			Assert("Config outside of collection is always editable", !organizationConfig.ReadOnly);
			Assert("Organization Job Config is editable, so pivot is editable", !orgLevelPivot.ReadOnly);

			var companyCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK);
			companyCollection.Add(companyConfig);
			Assert("Company level config in company collection is editable", !companyConfig.ReadOnly);
			Assert("Company level Charge Group pivot is editable", !companyLevelPivot.ReadOnly);
			companyCollection.RemoveAll();

			var organizationCollection = new AccCashAdvanceDefaultingConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, orgHeader.PK, LedgerTypes.AccountsReceivable);
			organizationCollection.Add(companyConfig);
			organizationCollection.Add(organizationConfig);
			Assert("Company level config in organization collection is read only", companyConfig.ReadOnly);
			Assert("Company level Charge Group pivot is read only", companyLevelPivot.ReadOnly);
			Assert("Organization level config in organization collection is editable", !organizationConfig.ReadOnly);
			Assert("Organization level Charge Group pivot is editable", !orgLevelPivot.ReadOnly);
			organizationCollection.RemoveAll();
		}

		public void TestHumanReadableNameCore()
		{
			var pivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			AssertEquals("Advance Payment Defaulting Charge Group", pivot.HumanReadableName);
		}

		public void TestIsDuplicate()
		{
			var bizo = Factory.New<CashAdvanceDefaultingChargeGroup>();
			bizo.JCT_Code = "FRT";
			var duplicate = Factory.New<CashAdvanceDefaultingChargeGroup>();
			duplicate.JCT_Code = "FRT";

			AssertNotEquals("Precondition: two different bizos", bizo.PK, duplicate.PK);
			Assert("Bizo should not be a duplicate of itself", !duplicate.IsDuplicateOf(duplicate));
			Assert("Duplicate should consider JCT_Code", duplicate.IsDuplicateOf(bizo));

			duplicate.JCT_Code = "BRK";
			Assert("Duplicate should consider JCT_Code", !duplicate.IsDuplicateOf(bizo));
		}

		public void TestChargeGroupDescription()
		{
			var pivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			AssertEquals(ZString.Empty, pivot.JCT_Code);
			AssertEquals(ZString.Empty, pivot.ChargeGroupDescription);

			pivot.JCT_Code = "BRK";
			AssertEquals("Customs Brokerage / Agency / Entry Fees", pivot.ChargeGroupDescription);
		}

		public void TestChargeGroups()
		{
			var pivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			var groupCodes = pivot.ChargeGroups.GetAllCodes();
			AssertEquals(38, groupCodes.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "BRK", "BON", "CLL", "CSH", "CST", "CDS", "DST", "FRT", "INS", "LOD", "LHR", "NJR", "NGC", "ORG", "OBR", "OBO", "SDS", "TRN", "TBC", "TDC", "TRC", "TDL", "TDU", "TRU", "UNL", "WAH", "WIN", "WOU", "WST", "CYI", "CYO", "CYS", "YRA", "YRE", "YTU", "MWO", "CGI", "CGO" }, groupCodes);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var configuration = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			configuration.CAC_ConfigType = JobConfiguration.TypeCodes.CashAdvanceDefaulting;
			configuration.CAC_GC = Env.CurrentCompanyPK;
			configuration.CAC_Ledger = LedgerTypes.AccountsReceivable;
			configuration.CAC_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			configuration.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			configuration.CAC_TransportMode = Core.Constants.TransportModes.Sea;
			configuration.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			var pivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			pivot.JCT_Code = ChargeCodeGroupList.Codes.Freight;
			pivot.JCT_JCF_JobConfig = configuration.PK;

			return pivot;
		}

		#endregion
	}
}
