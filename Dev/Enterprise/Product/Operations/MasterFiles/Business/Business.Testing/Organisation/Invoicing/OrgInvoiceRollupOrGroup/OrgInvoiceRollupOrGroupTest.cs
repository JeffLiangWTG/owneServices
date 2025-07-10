using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgInvoiceRollupOrGroup))]
	sealed class OrgInvoiceRollupOrGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			AssertEquals("PG_InvoiceLineDisplayOption", OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode, InvoiceRollOrGroup.PG_InvoiceLineDisplayOption);
			AssertEquals("PG_InvoicePostingStyle", OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode, InvoiceRollOrGroup.PG_InvoicePostingStyle);
			AssertEquals("PG_RX_NKInvoicePostingCurrency", ZString.Empty, InvoiceRollOrGroup.PG_RX_NKInvoicePostingCurrency);
		}

		public void TestInvoicePostingStyle()
		{
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code).InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
			AssertEquals("Org has default value", OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode, InvoiceRollOrGroup.PG_InvoicePostingStyle);

			InvoiceRollOrGroup.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.DisbursementAndFinal;
			AssertEquals("Org has assigned value", InvoicePostingOptionsList.Codes.DisbursementAndFinal, InvoiceRollOrGroup.PG_InvoicePostingStyle);
		}

		public void TestInvoiceLineDisplayOptionIncludesExchangeRate()
		{
			AssertEquals("Code: None", false, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(InvoiceDescriptionOptionsList.Codes.None));
			AssertEquals("Code: All", false, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(InvoiceDescriptionOptionsList.Codes.All));
			AssertEquals("Code: AllExRate", true, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(InvoiceDescriptionOptionsList.Codes.AllExRate));
			AssertEquals("Code: Freight", false, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(InvoiceDescriptionOptionsList.Codes.Freight));
			AssertEquals("Code: FreightExRate", true, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(InvoiceDescriptionOptionsList.Codes.FreightExRate));
			AssertEquals("Code: FreightFOB", false, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(InvoiceDescriptionOptionsList.Codes.FreightFOB));
			AssertEquals("Code: FreightFOBExRate", true, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(InvoiceDescriptionOptionsList.Codes.FreightFOBExRate));
			AssertEquals("Code: NoneExRate", true, OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionIncludesExchangeRate(InvoiceDescriptionOptionsList.Codes.NoneExRate));
		}

		public void TestGetInvoicePostingCurrency()
		{
			OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code).InvoicePostingCurrency = "USD";
			AssertEquals("Org has default value", ZString.Empty, InvoiceRollOrGroup.PG_RX_NKInvoicePostingCurrency);

			InvoiceRollOrGroup.PG_RX_NKInvoicePostingCurrency = "EUR";
			AssertEquals("Org has assigned value", "EUR", InvoiceRollOrGroup.PG_RX_NKInvoicePostingCurrency);
		}

		public void TestGroupOrSubtotalStyleReadOnly()
		{
			var testInvoiceRollup = OrgInDB.CompanyData.InvoiceRollupOrGroups.AddNew();
			testInvoiceRollup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			var invoiceRollupOrGroupHelper = new InvoiceRollupOrGroupHelper(testInvoiceRollup);

			testInvoiceRollup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical;
			Assert(invoiceRollupOrGroupHelper.IsGroupOrSubTotalOnlyForCLCAndNOG(OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical));
			Assert("Not ReadOnly", !testInvoiceRollup.PG_GroupOrSubtotalStyleInfo.ReadOnly);

			testInvoiceRollup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.Sequence;
			Assert(invoiceRollupOrGroupHelper.IsGroupOrSubTotalOnlyForCLCAndNOG(OrgConstants.GroupOrSubTotalCharges.Code.Sequence));
			Assert("Not ReadOnly", !testInvoiceRollup.PG_GroupOrSubtotalStyleInfo.ReadOnly);

			testInvoiceRollup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.User;
			Assert(invoiceRollupOrGroupHelper.IsGroupOrSubTotalOnlyForCLCAndNOG(OrgConstants.GroupOrSubTotalCharges.Code.User));
			Assert("Not ReadOnly", !testInvoiceRollup.PG_GroupOrSubtotalStyleInfo.ReadOnly);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldARInvoiceValue = Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed;

			try
			{
				OrgInvoiceRollupOrGroup testInvoiceRollup = OrgInDB.CompanyData.InvoiceRollupOrGroups.AddNew();
				testInvoiceRollup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;

				Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testInvoiceRollup.PG_GroupOrSubTotalInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceRollup.PG_GroupOrSubtotalStyleInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceRollup.PG_JobTypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceRollup.PG_ServiceDirectionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceRollup.PG_TransportModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceRollup.PG_InvoiceLineDisplayOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceRollup.PG_InvoicePostingStyleInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testInvoiceRollup.PG_GroupOrSubTotalInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceRollup.PG_GroupOrSubtotalStyleInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceRollup.PG_JobTypeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceRollup.PG_ServiceDirectionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceRollup.PG_TransportModeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceRollup.PG_InvoiceLineDisplayOptionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceRollup.PG_InvoicePostingStyleInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyChargeGrouping.IsAllowed = oldARInvoiceValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoiceRollOrGroup;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return InvoiceRollOrGroup;
		}

		OrgInvoiceRollupOrGroup InvoiceRollOrGroup;
		OrgHeader Org;
		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.New<OrgHeader>();
			InvoiceRollOrGroup = Org.CompanyData.InvoiceRollupOrGroups.AddNew();
		}
	}
}
