using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgsEvaluatedForCreditControl))]
	sealed class OrgsEvaluatedForCreditControlTest : ChargeGroupSettingTest
	{
		public void TestHasOrganizationTypes()
		{
			foreach (CodeDescriptionPair jobType in JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes())
			{
				BizObj.JobType = jobType.Code;
				AssertEquals(BizObj.JobType + " has OrganisationTYpes", BizObj.OrganizationTypeList.Count > 0, BizObj.HasOrganizationTypes);
			}
		}

		public void TestHasAllDebtorsIsInSyncWithOrganizationTypeList()
		{
			foreach (CodeDescriptionPair jobType in JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes())
			{
				BizObj.JobType = jobType.Code;
				AssertEquals(BizObj.JobType + " has AllDebtors", BizObj.OrganizationTypeList.ContainsCode(OrgCodes.AllDebtors), BizObj.HasAllDebtors);
			}
		}

		public void TestINCOTermValues()
		{
			CodeDescriptionPairList expectedList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			expectedList.AddPair(INCOTermCodes.All, INCOTermDescriptions.All);

			CodeDescriptionPairList expectedDomesticList = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms);
			expectedDomesticList.AddPair(INCOTermCodes.All, INCOTermDescriptions.All);

			CodeDescriptionPairList expectedAllList = new CodeDescriptionPairList();
			expectedAllList.AddRange(expectedList);
			expectedAllList.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));

			foreach (CodeDescriptionPair jobType in JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes())
			{
				BizObj.JobType = jobType.Code;

				if (BizObj.JobType == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					Assert("Should see the dropdown list for these items", !BizObj.INCOTermInfo.ReadOnly);
					AssertContainsExactElementsInAnyOrder(expectedList, BizObj.INCOTermList);
				}
				else if (BizObj.JobType == JobInvoicingConsumerTypes.Shipment.Code
					|| jobType.Code == JobInvoicingConsumerTypes.QuotedBooking.Code)
				{
					Assert("Shouldn't see the INCO Term dropdown list", BizObj.INCOTermInfo.ReadOnly);

					BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Import;
					Assert("Should see the INCO Term dropdown list", !BizObj.INCOTermInfo.ReadOnly);
					AssertContainsExactElementsInAnyOrder(expectedList, BizObj.INCOTermList);

					BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Domestic;
					Assert("Should see the INCO Term dropdown list", !BizObj.INCOTermInfo.ReadOnly);
					AssertContainsExactElementsInAnyOrder(expectedDomesticList, BizObj.INCOTermList);

					BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.All;
					Assert("Should see the INCO Term dropdown list", !BizObj.INCOTermInfo.ReadOnly);
					AssertContainsExactElementsInAnyOrder(expectedAllList, BizObj.INCOTermList);

					BizObj.DirectionCode = "";
				}
				else
				{
					Assert("Shouldn't see the INCO Term dropdown list", BizObj.INCOTermInfo.ReadOnly);
				}
			}
		}

		public void TestFreightPaymentTermValues()
		{
			CodeDescriptionPairList expectedList = new CodeDescriptionPairList(OLookUpEditType.PaymentType);
			expectedList.Insert(0, new CodeDescriptionPair(FreightPaymentTermCodes.All, FreightPaymentTermDescriptions.All));

			foreach (CodeDescriptionPair jobType in JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes())
			{
				BizObj.JobType = jobType.Code;

				if (BizObj.JobType == JobInvoicingConsumerTypes.Brokerage.Code
					|| BizObj.JobType == JobInvoicingConsumerTypes.Shipment.Code
					|| BizObj.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code
					|| BizObj.JobType == JobInvoicingConsumerTypes.AgencyBillOfLading.Code
					|| BizObj.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code)
				{
					Assert("Shouldn't see the Freight Payment Term dropdown list", !BizObj.FreightPaymentTermInfo.ReadOnly);
					AssertContainsExactElementsInAnyOrder(expectedList, BizObj.FreightPaymentTermList);
				}
				else
				{
					Assert("Should see the Freight Payment Term dropdown list", BizObj.FreightPaymentTermInfo.ReadOnly);
				}
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#region Implementation

		new OrgsEvaluatedForCreditControl BizObj
		{
			get { return (OrgsEvaluatedForCreditControl)base.BizObj; }
			set { base.BizObj = value; }
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new OrgsEvaluatedForCreditControl();

			result.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.Mode = Core.Constants.TransportModes.Air;
			result.OrganizationType = OrgCodes.LocalClient;

			return result;
		}

		#endregion
	}

	sealed class OrgsEvaluatedForCCValidationTest : OrgsEvaluatedForCreditControlValidationTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new OrgsEvaluatedForCreditControl();
			}
		}

		protected override IRegistrySettingCollection GetNewBizObjCollection
		{
			get
			{
				return new OrgsEvaluatedForCreditControlCollection();
			}
		}

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";
			BizObj.OrganizationType = "CNE";

			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.OrganizationTypeInfo);
		}
	}

	sealed class OrgsEvaluatedForCCTest : OrgsEvaluatedForCreditControlLookupsTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new OrgsEvaluatedForCreditControl();
			}
		}
	}
}
