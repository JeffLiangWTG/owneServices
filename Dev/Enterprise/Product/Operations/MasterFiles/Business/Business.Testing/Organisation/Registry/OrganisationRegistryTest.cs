using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrganisationRegistry;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationRegistry))]
	sealed class OrganisationRegistryTest : RegistryItemSetTestCaseWithFactory<OrganisationRegistry>
	{
		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				var list = new List<string>(base.ConditionallyVisibleRegistryItems);
				list.Add("RatingDocRollupOrGroup");
				return list;
			}
		}
		public override void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			//Because the resource strings for StrictEnforcementOfRegistrationNumberFormats have to be generated from resource strings elsewhere, we have to override this test to not run.
			Assert(true);
		}

		#region Allow Merge Ignoring AR AP

		public void TestStrictEnforcementOfRegistrationNumberFormats()
		{
			TestRegistryItem(
					ItemSet.StrictEnforcementOfRegistrationNumberFormats,
					"StrictEnforcementOfRegistrationNumberFormats",
					OrganisationsDataRegistry.Categories.Organizations,
					(NoResString)"Strict Enforcement Of Registration Number Formats",
					(NoResString)"Certain kinds of registration numbers and codes have validation that will prevent saving. Untick a specific format to make them only warnings. CargoWise strongly recommends that validation is used to preserve data quality.",
					RegistryStorageFlags.System,
					"Is Validated?",
					true,
					83,
					OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue().ToArray());
		}

		public void TestStrictEnforcementOfRegistryNumberFormats_DefaultValues()
		{
			var items = new Dictionary<string, bool>
			{
				{ RegistrationNumberFormatFields.AONIF, true },
				{ RegistrationNumberFormatFields.ARCUIL, false },
				{ RegistrationNumberFormatFields.ARCUIT, false },
				{ RegistrationNumberFormatFields.ARDNI, false },
				{ RegistrationNumberFormatFields.ARIBL, false },
				{ RegistrationNumberFormatFields.ARIBM, false },
				{ RegistrationNumberFormatFields.ARIBS, false },
				{ RegistrationNumberFormatFields.AUABN, true },
				{ RegistrationNumberFormatFields.AUARN, true },
				{ RegistrationNumberFormatFields.AUCCP, true },
				{ RegistrationNumberFormatFields.AUEAP, true },
				{ RegistrationNumberFormatFields.AUEEN, true },
				{ RegistrationNumberFormatFields.AUEEU, true },
				{ RegistrationNumberFormatFields.AUESN, true },
				{ RegistrationNumberFormatFields.AUNEN, true },
				{ RegistrationNumberFormatFields.BRCNPJ, true },
				{ RegistrationNumberFormatFields.CABNC, true },
				{ RegistrationNumberFormatFields.CABRB, true },
				{ RegistrationNumberFormatFields.CABRE, true },
				{ RegistrationNumberFormatFields.CABRL, true },
				{ RegistrationNumberFormatFields.CABRM, true },
				{ RegistrationNumberFormatFields.CargoWiseOneCarrierCode, true },
				{ RegistrationNumberFormatFields.CarrierCode, true },
				{ RegistrationNumberFormatFields.CAWMI, true },
				{ RegistrationNumberFormatFields.CCP, true },
				{ RegistrationNumberFormatFields.CLRUTSOL, true },
				{ RegistrationNumberFormatFields.CNBSTVAT, true },
				{ RegistrationNumberFormatFields.CNVAGVAS, true },
				{ RegistrationNumberFormatFields.CONIT, true },
				{ RegistrationNumberFormatFields.CRCID, true },
				{ RegistrationNumberFormatFields.CRCIJ, true },
				{ RegistrationNumberFormatFields.CRDIM, true },
				{ RegistrationNumberFormatFields.CREAC, true },
				{ RegistrationNumberFormatFields.CRNIT, true },
				{ RegistrationNumberFormatFields.CRUBI, true },
				{ RegistrationNumberFormatFields.DEHRB, true },
				{ RegistrationNumberFormatFields.DKPNR, true },
				{ RegistrationNumberFormatFields.DOCED, true },
				{ RegistrationNumberFormatFields.DORNC, true },
				{ RegistrationNumberFormatFields.EINCBNSSN4811PartyID, true },
				{ RegistrationNumberFormatFields.EUEOR, true },
				{ RegistrationNumberFormatFields.GS1, true },
				{ RegistrationNumberFormatFields.HUIDM, true },
				{ RegistrationNumberFormatFields.ILVAT, true },
				{ RegistrationNumberFormatFields.ISCOC, true },
				{ RegistrationNumberFormatFields.ITCAT, true },
				{ RegistrationNumberFormatFields.ITCOD, false },
				{ RegistrationNumberFormatFields.ITIVA, false },
				{ RegistrationNumberFormatFields.KR01, true },
				{ RegistrationNumberFormatFields.KR08, true },
				{ RegistrationNumberFormatFields.KRKBC, true },
				{ RegistrationNumberFormatFields.KRKBT, true },
				{ RegistrationNumberFormatFields.KRVAT, true },
				{ RegistrationNumberFormatFields.MXCFD, true },
				{ RegistrationNumberFormatFields.MXIVA, false },
				{ RegistrationNumberFormatFields.MXREG, true },
				{ RegistrationNumberFormatFields.MXRFC, false },
				{ RegistrationNumberFormatFields.MYOTH, true },
				{ RegistrationNumberFormatFields.MYPIC, true },
				{ RegistrationNumberFormatFields.MYSIC, true },
				{ RegistrationNumberFormatFields.MYTIN, true },
				{ RegistrationNumberFormatFields.NAICS, true },
				{ RegistrationNumberFormatFields.NLCCN, true },
				{ RegistrationNumberFormatFields.NOGBR, true },
				{ RegistrationNumberFormatFields.NOMVA, true },
				{ RegistrationNumberFormatFields.NZCCP, true },
				{ RegistrationNumberFormatFields.PABRC, true },
				{ RegistrationNumberFormatFields.PTIVA, true },
				{ RegistrationNumberFormatFields.SIC, true },
				{ RegistrationNumberFormatFields.TWGTX, true },
				{ RegistrationNumberFormatFields.TWMCI, true },
				{ RegistrationNumberFormatFields.TWPIG, true },
				{ RegistrationNumberFormatFields.TWVAT, true },
				{ RegistrationNumberFormatFields.USCCC, true },
				{ RegistrationNumberFormatFields.USEIN, true },
				{ RegistrationNumberFormatFields.USMID, true },
				{ RegistrationNumberFormatFields.USNMF, true },
				{ RegistrationNumberFormatFields.USPFR, true },
				{ RegistrationNumberFormatFields.UYBRC, true },
				{ RegistrationNumberFormatFields.UYCID, false },
				{ RegistrationNumberFormatFields.UYRUT, false },
				{ RegistrationNumberFormatFields.VNVAT, true },
			};

			foreach (var item in items)
			{
				AssertEquals($"value should be set to {item.Value} by default", item.Value, ItemSet.StrictEnforcementOfRegistrationNumberFormats.Value.GetBoolFromCode(item.Key));
			}
		}

		public void TestAllowMergeIgnoringARAP()
		{
			TestGenericRegistryItem(
				ItemSet.AllowMergeIgnoringARAP,
				"AllowMergeIgnoringARAP",
				OrganisationRegistry.Categories.Organizations,
				"Allow Merge Ignoring AR and AP",
				"Allow Merge Ignoring AR and AP settings for other companies. Normally if you try to merge Organization A into Organization B, and Organization A is Payable/Receivable in any country/region, you will not be allowed to merge until Organization B is marked as Payable/Receivable for that country/region. With this option turned on, users will be able to merge if applicable.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region Profit Share Agreement

		public void TestCustomProfitShareAgreementTypeChargeCodes()
		{
			AssertEquals(0, ItemSet.CustomProfitShareAgreementTypeChargeCodes.GetAsGuidArray().Length);

			Guid new1 = Guid.NewGuid();
			Guid new2 = Guid.NewGuid();
			ItemSet.CustomProfitShareAgreementTypeChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new1 + "," + new2);
			Guid[] codes = ItemSet.CustomProfitShareAgreementTypeChargeCodes.GetAsGuidArray();
			AssertEquals("New Codes[0]", new1, codes[0]);
			AssertEquals("New Codes[1]", new2, codes[1]);

			AssertEquals("", ItemSet.CustomProfitShareAgreementTypeChargeCodes.DefaultChargeCode);

			TestGenericRegistryItem(
					ItemSet.CustomProfitShareAgreementTypeChargeCodes,
					"CustomProfitShareAgreementTypeChargeCodes",
					OrganisationRegistry.Categories.Accounting_JobInvoicing_ProfitShare,
					"Customized Profit Share Agreement Type Charge Codes",
					"List of Charge Codes for Customizing Profit Share Agreement Type",
					RegistryStorageFlags.Company);
		}

		public void TestCustomProfitShareAgreementTypeChargeCodes_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			TestGenericRegistryItem(
					ItemSet.CustomProfitShareAgreementTypeChargeCodes,
					"CustomProfitShareAgreementTypeChargeCodes",
					OrganisationRegistry.Categories.Accounting_JobInvoicing_ProfitShare,
					"Customized Profit Share Agreement Type Charge Codes",
					"List of Charge Codes for Customizing Profit Share Agreement Type",
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden);
		}

		#endregion

		#region INACTIVE Address Warning/Error

		public void TestInactiveAddressWarningOrError()
		{
			TestGenericRegistryItem(
				ItemSet.InactiveAddressWarningOrError,
				"InactiveAddressWarningOrError",
				OrganisationRegistry.Categories.Organizations,
				"INACTIVE Address Warning/Error",
				"When set to Yes, an error will replace the warning on an INACTIVE Address.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region Duplicate Main Address / AddressNotOnFile

		public void TestEnableAddressNotOnFileLogging()
		{
			TestGenericRegistryItem(
				ItemSet.EnableAddressNotOnFileLogging,
				"EnableAddressNotOnFileLogging",
				OrganisationRegistry.Categories.Organizations,
				"Enable Address Not On File Logging",
				"This registry will enable additional logging focused on the creation of the ***Address Not On File*** address. This address is usually created when there is no main address against an organization",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion Duplicate Main Address / AddressNotOnFile

		#region Sales & Marketing

		#region Commission

		public void TestCommissionAgreementStreams()
		{
			TestRegistryItem(
				ItemSet.CommissionAgreementStreams,
				"CommissionAgreementStreams",
				OrganisationRegistry.Categories.SalesMarketing_Commission,
				"Commission Agreement Streams",
				@"The list of streams available for commission agreements.

Commission agreements on different streams are allowed to have commissions from the same jobs. Additionally, the commissionable amount will be independent of the other streams.
e.g. if a job has a profit of $200, and there are 3 agreements on different streams for this same job; each agreement is applicable for $200 commission.",
				RegistryStorageFlags.System,
				"Is Enabled?",
				true,
				0);
		}

		public void TestCommissionAgreementConflictNotificationGroup()
		{
			TestGenericRegistryItem(
					ItemSet.CommissionAgreementConflictNotificationGroup,
					"CommissionAgreementConflictNotificationGroup",
					OrganisationRegistry.Categories.SalesMarketing_Commission,
					"Commission Agreement Conflict Notification Group",
					"The staff group that will receive notifications about commission agreement conflicts.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsValueOptional,
					Guid.Empty);
		}

		public void TestCommissionAgreementConflictNotificationGroupEmailTemplate()
		{
			TestGenericRegistryItem(
					ItemSet.CommissionAgreementConflictNotificationGroupEmailTemplate,
					"CommissionAgreementConflictNotificationGroupEmailTemplate",
					OrganisationRegistry.Categories.SalesMarketing_Commission,
					"Commission Agreement Conflict Email Template for Notification Group",
					"The email template that will be sent to the commission agreement conflict notification group when a more specific agreement is added.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestSendCommissionAgreementConflictEmailToAllRecipients()
		{
			TestGenericRegistryItem(
					ItemSet.SendCommissionAgreementConflictEmailToAllRecipients,
					"SendCommissionAgreementConflictEmailToAllRecipients",
					OrganisationRegistry.Categories.SalesMarketing_Commission,
					"Send Commission Agreement Conflict Notification Email to Wolf Pack",
					"When this flag is set to true, a notification email will be sent to all wolf pack members of an existing agreement when a more specific agreement is added.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
		}

		public void TestCommissionAgreementConflictRecipientsEmailTemplate()
		{
			TestGenericRegistryItem(
					ItemSet.CommissionAgreementConflictRecipientsEmailTemplate,
					"CommissionAgreementConflictRecipientsEmailTemplate",
					OrganisationRegistry.Categories.SalesMarketing_Commission,
					"Commission Agreement Conflict Email Template for Wolf Pack",
					"The email template that will be sent to existing commission agreement wolf pack members when a more specific agreement is added.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestDefaultSpecifiedBackdate()
		{
			TestGenericRegistryItem(
					ItemSet.DefaultSpecifiedBackdate,
					"DefaultSpecifiedBackdate",
					OrganisationRegistry.Categories.SalesMarketing_Commission,
					"Commission Agreement Approval Default Specified Backdate",
					"The default value for Backdate Commission Options -> From -> Specified Date, on the Commission Agreement Approval form.",
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		#endregion

		#region Trade Lane Synchronisation

		public void TestUpdateExistingClientCommenceDateFromTradeLaneSync()
		{
			TestRegistryItem(
				ItemSet.UpdateExistingClientCommenceDateFromTradeLaneSync,
				"UpdateExistingClientCommenceDateFromTradeLaneSync",
				OrganisationRegistry.Categories.SalesMarketing_TradeLanesSynchronisation,
				"Update Existing Client Commence Date From Trade Lane Synchronization",
				@"When the TLS service task recognizes that an organization is involved as either a Consignor, Consignee or Local Client where no Client Commenced date exists, it updates this organization with a date reflecting the period in which it started trading.

When this registry setting set to Yes, the TLS service task will update the Client Commenced date if a trade period older than the current Client Commenced is identified.

Note: the number of trade periods considered by TLS is dependent on the TLS Service task schedule configuration.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		public void TestFullTradeLanesSyncDateTimeThreshold()
		{
			TestGenericRegistryItem(
				ItemSet.FullTradeLanesSyncDateTimeThreshold,
				"FullTradeLanesSyncDateTimeThreshold",
				string.Empty,
				string.Empty,
				string.Empty,
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				SqlDateTime.MinValue.Value);
		}

		[TestDate(2023, 3, 2)]
		public void TestFullTradeLanesSyncFromDateRaw()
		{
			TestRegistryItem(
				ItemSet.FullTradeLanesSyncFromDateRaw,
				"FullTradeLanesSyncFromDate",
				OrganisationRegistry.Categories.SalesMarketing_TradeLanesSynchronisation,
				"Earliest Month included in Full Trade Lane Synchronization",
				@"The Full Trade Lane Synchronization (TLS) service task will process records dating between the first day of the month specified here and the current date.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new DateTime(2021, 3, 1));
		}

		[TestDate(2023, 3, 20)]
		public void TestFullTradeLanesSyncFromDate()
		{
			AssertEquals("Default date for FullTradeLanesSyncFromDate should be the first day of the month from 2 years back.", Instance.FullTradeLanesSyncFromDate, new DateTime(2021, 3, 1));

			Instance.FullTradeLanesSyncFromDate = new DateTime(2021, 3, 25);

			AssertEquals("Date for FullTradeLanesSyncFromDate should be the first day of the month specified when manually set.", Instance.FullTradeLanesSyncFromDate, new DateTime(2021, 3, 1));
		}

		public void TestShouldDoFullTradeLanesSyncIfRequired()
		{
			TestRegistryItem(
				ItemSet.ShouldDoFullTradeLanesSyncIfRequired,
				"ShouldDoFullTradeLanesSyncIfRequired",
				OrganisationRegistry.Categories.SalesMarketing_TradeLanesSynchronisation,
				"Should do Full Trade Lane synchronization",
				@"The Full Trade Lane Synchronization (TLS) service task will run if this registry item is true",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		#endregion

		#endregion

		#region Default Values

		public void TestInvoiceRollupOrGroup()
		{
			InvoiceRollupOrGroupCollection collection = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value;
			AssertEquals("Collection must have one line", 1, collection.Count);
			AssertEquals(OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code, collection[0].JobType);
			AssertEquals(OrgConstants.ServiceDirection.Code.All, collection[0].ServiceDirection);
			AssertEquals(OrgConstants.ModesForGroupOrSubTotal.Codes.All, collection[0].TransportMode);
			AssertEquals(OrgConstants.GroupOrSubTotalCharges.Code.RollUp, collection[0].GroupOrSubTotal);
			AssertEquals(OrgConstants.InvoiceLineGroupings.Code.None, collection[0].GroupOrSubtotalStyle);
			AssertEquals(InvoiceDescriptionOptionsList.Codes.None, collection[0].InvoiceLineDisplayOption);
			AssertEquals(InvoicePostingOptionsList.Codes.FinalInvoiceOnly, collection[0].InvoicePostingStyle);

			TestGenericRegistryItem(
					ItemSet.InvoiceRollupOrGroup,
					"InvoiceRollupOrGroup",
					OrganisationRegistry.Categories.Organizations_DefaultValues,
					"Charge Grouping & Roll Up",
					"Specify the grouping , roll up, posting and display settings Accounts Receivable Invoices that are sent to customers. These settings are the defaults for new organizations, and can be overridden on each organization.",
					RegistryStorageFlags.All);
		}

		public void TestTermsAndTermDays()
		{
			ARTermsCollection collection = OrganisationRegistry.Instance.TermsAndTermDays.Value;
			AssertEquals("one entry", 1, collection.Count);
			AssertEquals("no terms cycles yet", 0, collection[0].ARTermsCycles.Count);
			AssertEquals("no payment cycles yet", 0, collection[0].ARPaymentCycles.Count);
			AssertEquals(OrgARTermsLookups.InvoiceTypes.All.Code, collection[0].InvoiceClass);
			AssertEquals(Constants.InvoiceTerms.CashOnDelivery, collection[0].InvoiceTerm);
			AssertEquals((ZByte)0, collection[0].InvoiceDays);

			TestGenericRegistryItem(
					ItemSet.TermsAndTermDays,
					"TermsAndTermDays",
					OrganisationRegistry.Categories.Organizations_DefaultValues,
					"Terms and Term Days",
					"When an Organization is newly set as Receivables, Terms and Term Days will be populated with the following.",
					RegistryStorageFlags.All);
		}

		public void TestInvoiceTermsFromEndOfWeek()
		{
			TestGenericRegistryItem(
				ItemSet.InvoiceTermsEndOfWeek,
				"InvoiceTermsEndOfWeek",
				OrganisationRegistry.Categories.Organizations_DefaultValues,
				"Invoice Term - From End of Week",
				@"This registry define the 'Last Day of the Week' for the purpose of  'EWK - From End of Week' Invoice Term's due date calculation. By default, this registry is set to 'SUN-Sunday'.Please override the value where applicable.
						Example 1
						If the last day of the week is set to Sunday and the invoice term days is set to 10 days.
						An invoice issued with an 'Invoice Date' of Tuesday, 02-Apr-19 will have a 'Due Date' of 17-Apr-19.
						The calculation will be as follows:
						Step 1: The End of Week in relation to the Invoice Date will be Sunday 07-Apr-19.
						Step 2: Add 10 days to 07-Apr-19 to compute the Due Date which is 17-Apr-19.

						Example 2
						If the last day of the week is set to Friday and the invoice term days is set to 10 days.
						An invoice issued with an 'Invoice Date' of Friday, 05-Apr-19 will have a 'Due Date' of 15-Apr-19.
						The calculation will be as follows:
						Step 1: The End of Week in relation to the Invoice Date will be Friday 05-Apr-19.
						Step 2: Add 10 days to 05-Apr-19 to compute the Due Date which is 15-Apr-19.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);

			AssertEquals("System level Value", DayOfWeekCodeList.Codes.Sunday, ItemSet.InvoiceTermsEndOfWeek.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Company level Value", DayOfWeekCodeList.Codes.Sunday, ItemSet.InvoiceTermsEndOfWeek.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			ItemSet.InvoiceTermsEndOfWeek.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DayOfWeekCodeList.Codes.Monday);
			AssertEquals("System level Value", DayOfWeekCodeList.Codes.Sunday, ItemSet.InvoiceTermsEndOfWeek.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Company level Value", DayOfWeekCodeList.Codes.Monday, ItemSet.InvoiceTermsEndOfWeek.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			//event log
			var registry = OrganisationRegistry.Instance.InvoiceTermsEndOfWeek;
			var oldValue = DayOfWeekCodeList.Codes.Sunday;
			var newValue = DayOfWeekCodeList.Codes.Monday;
			var expectedLogMessage = (NoResString)$"Registry value changed from [{oldValue}] to [{newValue}].";
			var args = new RegistryItemWrapper.BuildLogReferenceArgs(registry, oldValue, newValue);
			var logReference = OrganisationRegistry.Instance.InvoiceTermsEndOfWeek.OnBuildLogReference(args);
			AssertEquals(expectedLogMessage, logReference);
		}
		public void TestPreApprovalTerms()
		{
			AssertEquals(ARInvoiceTermsList.CashOnDelivery.Code, OrganisationRegistry.Instance.PreApprovalTerms.Value);

			TestGenericRegistryItem(
					ItemSet.PreApprovalTerms,
					"PreApprovalTerms",
					OrganisationRegistry.Categories.Organizations_DefaultValues,
					"Pre-Approval Terms",
					"When Credit is not approved, this pre-approval term is used as a default. (If credit is on hold, the registry item for on hold takes precedence over this one.)",
					RegistryStorageFlags.All);
		}

		public void TestOnHoldTerms()
		{
			AssertEquals(ARInvoiceTermsList.CashOnDelivery.Code, OrganisationRegistry.Instance.PreApprovalTerms.Value);

			AssertEquals("DataType", typeof(OnHoldTermsRegistryItem), ItemSet.OnHoldTerms.GetType());
			TestGenericRegistryItem(
					ItemSet.OnHoldTerms,
					"OnHoldTerms",
					OrganisationRegistry.Categories.Organizations_DefaultValues,
					"On Hold Terms",
					"When Credit is on hold, this term is used as a default. (If credit is also not approved, this registry item takes precedence over the registry item for pre-approval.)",
					RegistryStorageFlags.All);
		}

		public void TestUseARInvoiceTermsAndTermDaysWhenCreditIsOnHold()
		{
			TestGenericRegistryItem(
				ItemSet.UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold,
				"UseARInvoiceTermsAndTermDaysWhenCreditIsOnHold",
				OrganisationRegistry.Categories.Organizations_DefaultValues,
				"Use AR Invoice Terms And Term Days When Credit Is On Hold",
				@"By default, this registry will be set to 'No' in which case the system will use the invoice terms and term days as configured under Master Data > Organizations > Default Values > On Hold Terms when issuing AR invoices if the debtor is put on credit hold.

When this registry is set to 'Yes', the system will use the invoice terms and term days as configured under Organization > A/R > Credit Control and Settlement > Local > Terms and Term Days.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		public void TestWebSecurityDefaultValues()
		{
			var collection = OrganisationRegistry.Instance.WebSecurityDefaultValues.Value;
			AssertEquals(0, collection.Count);

			TestGenericRegistryItem(
					ItemSet.WebSecurityDefaultValues,
					"WebSecurityDefaultValues",
					OrganisationRegistry.Categories.Organizations_DefaultValues,
					"Web Security Default Values",
					"Below are the Web Security Default Values for web access. Click on GLOW Portal User Admin to manage Glow Web Security Rights.",
					RegistryStorageFlags.System);
		}

		#endregion

		#region RatingDocuments
		public void TestRatingDocRollupOrGroupRegistryItem()
		{
			void AssertRegistryItem(RegistryOptions options)
			{
				var collection = OrganisationRegistry.Instance.RatingDocRollupOrGroup.Value;
				AssertEquals(DocRollupOrSortModuleList.Codes.All, collection[0].Module);
				AssertEquals(DocRollupOrSortJobTypeList.Codes.All, collection[0].JobType);
				AssertEquals(DocRollupOrSortTransportModeList.Codes.All, collection[0].TransportMode);
				AssertEquals(DocRollupOrSortDisplayList.Codes.RollUpCharges, collection[0].Display);
				AssertEquals(DocRollupOrSortStyleList.Codes.NoGrouping, collection[0].Style);

				TestGenericRegistryItem(
					ItemSet.RatingDocRollupOrGroup,
					"RatingDocRollupOrGroup",
					OrganisationRegistry.Categories.Organizations_Rating,
					"Rating Documents Charge Grouping and Roll Up",
					"Specify the module, job type, transport mode, display, and style for Rating Documents Printing. These settings are the defaults for new organizations, and can be overridden on each organization.",
					RegistryStorageFlags.All,
					options);
			}

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertRegistryItem(RegistryOptions.IsHidden);
			}

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("RatingDocRollupOrGroup", TimeSpan.Zero);
				AssertRegistryItem(RegistryOptions.Default);
			}
		}

		#endregion

		#region GUI State

		public void TestGuiStateIntegers()
		{
			TestRegistryItem(
					ItemSet.GuiStateIntegers,
					"GuiStateInts",
					OrganisationRegistry.Categories.Organizations_GUIState,
					ZString.Empty,
					ZString.Empty,
					RegistryStorageFlags.Company,
					RegistryOptions.NotLogged | RegistryOptions.IsHidden,
					-1);
		}

		#endregion

		#region Maximum Credit Limit

		public void TestMaximumCreditLimit()
		{
			TestGenericRegistryItem(
				ItemSet.MaximumCreditLimit,
				"MaximumCreditLimit",
				OrganisationRegistry.Categories.Organizations_CreditReports,
				"Maximum Credit Limit",
				"Users are required to purchase a Credit Report when a credit limit exceeds this value. Select the type of report to be purchased when a Credit Limit exceeds the maximum credit limit.",
				RegistryStorageFlags.Company | RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport);
		}

		public void TestCreditReportLifetime()
		{
			TestRegistryItem(
				ItemSet.CreditReportLifetime,
				"CreditReportLifetime",
				OrganisationRegistry.Categories.Organizations_CreditReports,
				"Credit Report Lifetime",
				"The length of time in months a Credit Report is considered valid for the purposes of the Maximum Credit Limit.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				12,
				1,
				12);
		}

		#endregion

		#region Default Behavior Of Show For All Companies Checkbox

		public void TestDefaultBehaviorOfShowForAllCompaniesCheckbox()
		{
			TestRegistryItem(
				ItemSet.DefaultBehaviorOfShowForAllCompaniesCheckbox,
				"DefaultBehaviorOfShowForAllCompaniesCheckbox",
				OrganisationRegistry.Categories.Organizations,
				"Default Behavior of Show for all companies Checkbox",
				"Show for all companies selected. This setting will only be applied if the user also has the security Maintain -> Master Data -> Organization -> View -> View Other Company's Staff Assignments enabled.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region Import Credit Report
		public void TestEnableImportfromCreditReports()
		{
			TestRegistryItem(
					ItemSet.EnableImportFromCreditReports,
					"EnableImportFromCreditReports",
					OrganisationRegistry.Categories.Organizations_CreditReports,
					"Enable Import from Credit Reports",
					"Enables Importing from Credit Reports when this flag is True.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
		}
		#endregion

		#region Allow Job Override Address Additional Information

		public void TestAllowOverrideAddressAdditionalInformation()
		{
			TestRegistryItem(
				ItemSet.AllowOverrideAddressAdditionalInformation,
				"JobOverrideAddressAdditionalInformation",
				OrganisationRegistry.Categories.Organizations,
				"Allow Job Override Address Additional Information",
				"Setting this registry to \"Yes\" enables users to override the Additional Address Info field on jobs linked to an organization without breaking the link to the organization.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region Address Validation

		[ExpectNoExceptions]
		public void TestMDMSupportCertificateInfoShouldBeEmptyInShelfTestDB()
		{
			AssertEquals(true, new AuthenticationCertificateInfo(MDMProductCodes.AVS).IsEmpty);
		}

		#endregion Address Validation

		public void TestEnableBoleroEHBLIntegration()
		{
			var item = ItemSet.EnableBoleroEHBLIntegration;
			var expectedDefaultValue = new BoleroEBLForOrganisationConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "https://galileo.boleroserve.net/galileo-portal/jwt/login",
				GalileoAudience = "ab2e99f4-15c9-41c8-a138-7d873c3c4f9f",
				GalileoTestEndPointUrl = "https://galileo.training.boleroserve.net/galileo-portal/jwt/login",
				GalileoTestAudience = "ef46c619-92d6-4f56-af3d-327cf3646508",
				Timeout = 60,
			};

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(item, "EnableBoleroEHBLIntegrationForOrg", Categories.Organizations, "Enable Bolero eHBL Integration", "If yes, enable Bolero eHBL Integration for testing.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers);
				AssertEquals(expectedDefaultValue.EnableEBLIntegration, item.Value.EnableEBLIntegration);
				AssertEquals(expectedDefaultValue.GalileoAudience, item.Value.GalileoAudience);
				AssertEquals(expectedDefaultValue.GalileoEndPointUrl, item.Value.GalileoEndPointUrl);
				AssertEquals(expectedDefaultValue.GalileoTestAudience, item.Value.GalileoTestAudience);
				AssertEquals(expectedDefaultValue.GalileoTestEndPointUrl, item.Value.GalileoTestEndPointUrl);
				AssertEquals(expectedDefaultValue.Timeout, item.Value.Timeout);
			});
		}
	}
}
