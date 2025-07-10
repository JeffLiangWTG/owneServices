using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgMiscServ))]
	sealed class OrgMiscServTest : BusinessObjectWithCustomLabelsTestCase
	{
		#region Test Defaults

		public void TestDefaults()
		{
			OrganisationsDataRegistry.Instance.ConsigneeIncoTerm.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "FCA");
			OrganisationsDataRegistry.Instance.ConsignorIncoTerm.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CIF");

			var defaultOSMG = new DefaultOSMG();
			var osmgGroup = Factory.NewWithValidTestData<GlbGroup>();
			defaultOSMG.OrgSecurityGroup = osmgGroup.PK;

			OrganisationRegistry.Instance.OrgSecurityManagementGroupDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultOSMG);

			ReleaseTypes releaseTypes = FreightDataRegistry.Instance.ReleaseTypes.DefaultValue;
			releaseTypes.OriginalsNumber = 5;
			releaseTypes.CopiesNumber = 6;
			FreightDataRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseTypes);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgMiscServ newMiscServ = org.MiscServ;
			AssertNotNull("NewMiscServ not null", newMiscServ);

			AssertEquals("OM_EXExporterCategory", Constants.AccountsCategory.Standard, newMiscServ.OM_EXExporterCategory);
			AssertEquals("OM_RX_NKEXDefCurrency should not be defaulted - Organisations created in OS offices will have local currency of that office, which then defaults into customs invoice headers as the invoice currency, when the local currency should have been the invoice currency in majority of cases. Setting Default currency should only be by explicit data entry. See WI00022542", ZString.Empty, newMiscServ.OM_RX_NKEXDefCurrency);

			AssertEquals("OM_IMImporterCategory", Constants.AccountsCategory.Standard, newMiscServ.OM_IMImporterCategory);
			AssertEquals("OM_IMMergeCustomsInvoiceLinesBy", OrgConstants.MergeInvoiceLines.Default, newMiscServ.OM_IMMergeCustomsInvoiceLinesBy);
			AssertEquals("OM_EXMergeCustomsInvoiceLinesBy", OrgConstants.MergeInvoiceLines.Default, newMiscServ.OM_EXMergeCustomsInvoiceLinesBy);
			AssertEquals("OM_IMSendImportDocsTo", OrgConstants.SendDocsTo.Importer, newMiscServ.OM_IMSendImportDocsTo);
			AssertEquals("OM_IMSendSeaImportDocsTo", OrgConstants.SendDocsTo.Importer, newMiscServ.OM_IMSendSeaImportDocsTo);
			AssertEquals("OM_IMOriginalSeaBills", (byte)5, newMiscServ.OM_IMOriginalSeaBills);
			AssertEquals("OM_IMCopySeaBills", (byte)6, newMiscServ.OM_IMCopySeaBills);

			AssertEquals("OM_IMSeaDepotFreeDays", Env.Registry.CFSSeaFreightLCLStorageFreeDays, newMiscServ.OM_IMSeaDepotFreeDays);
			AssertEquals("OM_IMAirDepotFreeDays", Env.Registry.CFSAirFreightLCLStorageFreeDays, newMiscServ.OM_IMAirDepotFreeDays);
			AssertEquals("OM_IMDefaultWarehousePickOption", WhsPickOption.Codes.Auto, newMiscServ.OM_IMDefaultWarehousePickOption);
			AssertEquals("OM_WhsDefaultWarehousePickMode", WhsPickMode.Codes.AttributeSpecified, newMiscServ.OM_WhsDefaultWarehousePickMode);

			AssertEquals(OrgMiscServSchema.OM_FWAgentCategory.Name, Constants.AccountsCategory.Standard, newMiscServ.OM_FWAgentCategory);

			AssertEquals(OrgMiscServSchema.OM_CMClientSize.Name, "", newMiscServ.OM_CMClientSize);
			AssertEquals(OrgMiscServSchema.OM_CMGrowthOutlook.Name, "", newMiscServ.OM_CMGrowthOutlook);
			AssertEquals(OrgMiscServSchema.OM_CMOverallEffectOfClientOnAirfreightCosts.Name, "", newMiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts);
			AssertEquals(OrgMiscServSchema.OM_CMOverallEffectOfClientOnLCLCosts.Name, "", newMiscServ.OM_CMOverallEffectOfClientOnLCLCosts);
			AssertEquals(OrgMiscServSchema.OM_CMOverallEffectOfClientOnTEUCosts.Name, "", newMiscServ.OM_CMOverallEffectOfClientOnTEUCosts);
			AssertEquals(OrgMiscServSchema.OM_CMOverallEffectOfClientOnOtherCosts.Name, "", newMiscServ.OM_CMOverallEffectOfClientOnOtherCosts);
			AssertEquals(OrgMiscServSchema.OM_IMAutoPopulateOwnerRefWithOrderNums.Name, "DEF", newMiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums);
			AssertEquals("OM_CMDistanceCalculationProvider must default to DEF", DistanceCalculationConstants.Providers.DefaultFromRegistry, newMiscServ.OM_CMDistanceCalculationProvider);

			AssertEquals("OM_CITypeOfService", "", newMiscServ.OM_CITypeOfService);
			AssertEquals("OM_CISellingStyle", "", newMiscServ.OM_CISellingStyle);

			AssertEquals("OM_IMDefaultINCOTerm", "FCA", newMiscServ.OM_IMDefaultINCOTerm);
			AssertEquals("OM_EXDefaultIncoTerm", "CIF", newMiscServ.OM_EXDefaultIncoTerm);
			AssertEquals("UseTransactionCompanyAsPreferredPayment", true, newMiscServ.UseTransactionCompanyAsPreferredPayment);
			AssertEquals(osmgGroup.PK, newMiscServ.OM_GG_OrgSecurityGroup);
		}

		#endregion

		#region Decimals

		public void TestZDecimalsHaveCorrectDecimalPlacesOrgMiscServ()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			var globalList = new List<string>
			{
				nameof(miscServ.OM_ARGlobalCreditLimit),
				nameof(miscServ.ARGlobalCreditLimit)
			};

			var tester = new DecimalPlacesAttributeTester(miscServ);
			tester.CheckNonLocalCurrency(globalList, nameof(miscServ.GlobalCreditCurrencyDecimals), nameof(miscServ.OM_RX_NKARGlobalCreditCurrency), miscServ);
		}

		#endregion

		#region Related Business Objects

		public void TestAutoratingDateFiltering()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "NEW";
			var otherBranch = otherCompany.Branches.AddNew();
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Arrival;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var newFactory = new BusinessObjectFactory();
				var loadedOrganization = newFactory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals("Other Company: OM_AutoratingDateFiltering", RatingDateFilterTypes.Codes.Arrival, loadedOrganization.MiscServ.OM_AutoratingDateFiltering);

				loadedOrganization.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Departure;
				newFactory.Save();
			}

			AssertEquals("Current Company: OM_AutoratingDateFiltering", RatingDateFilterTypes.Codes.Departure, orgHeader.MiscServ.OM_AutoratingDateFiltering);
		}

		public void TestAirlineDetails()
		{
			miscServ.OM_RM_Airline = ZGuid.Empty;
			AssertNull(miscServ.Airline);

			RefAirline airline = Factory.NewWithValidTestData<RefAirline>();
			miscServ.OM_RM_Airline = airline.PK;
			AssertEquals(airline, miscServ.Airline);

			miscServ.OM_RM_Airline = new ZGuid();
			AssertNull(miscServ.Airline);
		}

		public void TestHeader()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			object x = org.MiscServ;
			Factory.Save();
			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();

			OrgMiscServ retrievedMiscServ = retrievingFactory.Load<OrgMiscServ>(org.MiscServ.PK);
			AssertNotNull(retrievedMiscServ.Header);
		}

		#region TestClientDocumentLogoNote

		public void TestClientDocumentLogoNote_IsCleared()
		{
			Assert("ClientDocumentLogo.IsEmpty", miscServ.ClientDocumentLogo.IsEmpty);
			miscServ.RunPreSaveValidation();
			Factory.Save();
			Assert("Should not have error", !miscServ.HasErrors);
			Assert("No Changes", !miscServ.HasChanges);

			miscServ.ClientDocumentLogo = BitmapToZBlob(new Bitmap(1, 1));
			Assert("Has Changes", miscServ.HasChanges);
			Assert("ClientDocumentLogo should not be empty", !miscServ.ClientDocumentLogo.IsEmpty);
			miscServ.RunPreSaveValidation();
			Assert("Should not have error", !miscServ.HasErrors);
		}

		public void TestClientDocumentLogoNote_UpdateSetsHasChanges()
		{
			// Setup a new logo for an organisation
			Assert("ClientDocumentLogo.IsEmpty", miscServ.ClientDocumentLogo.IsEmpty);
			miscServ.ClientDocumentLogo = ZBlob.Empty;
			miscServ.ClientDocumentLogo = BitmapToZBlob(new Bitmap(1, 1));
			Factory.Save();

			// Use a second factory to bypass the caching and force a db reload of BOs
			var otherFactory = new BusinessObjectFactory();
			var otherFactoryCompany = otherFactory.Load<OrgHeader>(company.PK);
			var otherFactoryMiscServ = otherFactoryCompany.MiscServ;

			// Try to modify the logo
			Assert("Precondition - Should NOT have changes.", !otherFactoryMiscServ.HasChanges);
			otherFactoryMiscServ.ClientDocumentLogo = BitmapToZBlob(new Bitmap(2, 2));
			Assert("Should have changes.", otherFactoryMiscServ.HasChanges);
			Assert("ClientDocumentLogo should NOT be empty.", !otherFactoryMiscServ.ClientDocumentLogo.IsEmpty);
			otherFactory.Save();

			// Try to clear the logo
			Assert("Precondition - Should NOT have changes.", !otherFactoryMiscServ.HasChanges);
			Assert("Precondition - ClientDocumentLogo should NOT be empty.", !otherFactoryMiscServ.ClientDocumentLogo.IsEmpty);
			var clientDocumentLogoNote = GetClientDocumentLogoNote(otherFactoryMiscServ);
			otherFactoryMiscServ.ClientDocumentLogo = null;
			Assert("Should have changes.", otherFactoryMiscServ.HasChanges);
			Assert("Initial logo should be deleted.", clientDocumentLogoNote.IsDeleted);
			Assert("ClientDocumentLogo should be empty.", otherFactoryMiscServ.ClientDocumentLogo.IsEmpty);

			// Try to modify the logo again without saving ( should not cause errors )
			otherFactoryMiscServ.ClientDocumentLogo = BitmapToZBlob(new Bitmap(3, 3));
			Assert("ClientDocumentLogo should NOT be empty.", !otherFactoryMiscServ.ClientDocumentLogo.IsEmpty);
			otherFactory.Save();
		}

		#endregion

		public void TestUNDGContact()
		{
			miscServ.OM_OC_EXDefaultDGContact = ZGuid.Empty;
			AssertNull(miscServ.UNDGContact);
			AssertEquals(true, miscServ.OM_EXDefaultDGContactPhoneUsed.IsEmpty);

			OrgContact contact = company.Contacts.AddNew();
			contact.OC_HomePhone = "123";
			miscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.HOM;
			miscServ.OM_OC_EXDefaultDGContact = contact.PK;
			AssertEquals(contact, miscServ.UNDGContact);
			AssertEquals(false, miscServ.OM_EXDefaultDGContactPhoneUsed.IsEmpty);

			miscServ.OM_OC_EXDefaultDGContact = new ZGuid();
			AssertNull(miscServ.UNDGContact);
		}

		#endregion

		#region Properties

		#region TestOM_EXMergeCustomsInvoiceLinesBy

		public void TestOM_EXMergeCustomsInvoiceLinesBy()
		{
			AssertEquals("Caption", "Merge Customs Invoice Lines By", DataBoundResourceStrings.GetDataForProperty(typeof(OrgMiscServ), nameof(OrgMiscServ.OM_EXMergeCustomsInvoiceLinesBy)).Caption);
		}

		#endregion

		#region Test_AuthorityToLeave

		public void Test_OM_CMAuthorityToLeave()
		{
			var excludeLogs = new List<StmALog>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgServ = org.MiscServ;
			AssertEquals("DEF", orgServ.OM_CMAuthorityToLeave);

			var logs = orgServ.Logs;
			Factory.Save();
			AssertEquals("Should be no logs, as DEF is default.", false, logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.Authorised.Code));

			orgServ.OM_CMAuthorityToLeave = "YES";
			orgServ.OM_CMAuthorityToLeave = "NO";
			orgServ.OM_CMAuthorityToLeave = "YES";
			Factory.Save();
			var latestLog = FindLatestWithType(logs, Events.AuthorisedCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("YES", orgServ.OM_CMAuthorityToLeave);
			AssertEquals(Events.AuthorisedCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			orgServ.OM_CMAuthorityToLeave = "DEF";
			Factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("DEF", orgServ.OM_CMAuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawnCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			orgServ.OM_CMAuthorityToLeave = "NO";
			Factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			AssertEquals("NO", orgServ.OM_CMAuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawnCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "NO", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);
		}

		public void Test_OM_ConsignorAuthorityToLeave()
		{
			var excludeLogs = new List<StmALog>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgServ = org.MiscServ;
			AssertEquals("DEF", orgServ.OM_ConsignorAuthorityToLeave);

			var logs = orgServ.Logs;
			Factory.Save();
			AssertEquals("Should be no logs, as DEF is default.", false, logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.Authorised.Code));

			orgServ.OM_ConsignorAuthorityToLeave = "YES";
			Factory.Save();
			var latestLog = FindLatestWithType(logs, Events.AuthorisedCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("YES", orgServ.OM_ConsignorAuthorityToLeave);
			AssertEquals(Events.AuthorisedCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			orgServ.OM_ConsignorAuthorityToLeave = "DEF";
			Factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("DEF", orgServ.OM_ConsignorAuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawnCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			orgServ.OM_ConsignorAuthorityToLeave = "NO";
			Factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			AssertEquals("NO", orgServ.OM_ConsignorAuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawnCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "NO", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);
		}

		public void Test_OM_ConsigneeAuthorityToLeave()
		{
			var excludeLogs = new List<StmALog>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgServ = org.MiscServ;
			AssertEquals("DEF", orgServ.OM_ConsigneeAuthorityToLeave);

			var logs = orgServ.Logs;
			Factory.Save();
			AssertEquals("Should be no logs, as DEF is default.", false, logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.Authorised.Code));

			orgServ.OM_ConsigneeAuthorityToLeave = "YES";
			Factory.Save();
			var latestLog = FindLatestWithType(logs, Events.AuthorisedCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("YES", orgServ.OM_ConsigneeAuthorityToLeave);
			AssertEquals(Events.AuthorisedCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			orgServ.OM_ConsigneeAuthorityToLeave = "DEF";
			Factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("DEF", orgServ.OM_ConsigneeAuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawnCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			orgServ.OM_ConsigneeAuthorityToLeave = "NO";
			Factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			AssertEquals("NO", orgServ.OM_ConsigneeAuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawnCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "NO", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);
		}

		StmALog FindLatestWithType(Logs logs, ZString eventType, params StmALog[] excludeLogs)
		{
			return logs.GetAllLogs().Cast<StmALog>().Where(l => !excludeLogs.Contains(l)).First(l => l.SL_SE_NKEvent == eventType);
		}

		[ExpectNoExceptions]
		public void TestIsAuthorisedToLeaveWithFallbacks()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertIsAuthorisedToLeaveWithFallback(org, org.MiscServ.OM_CMAuthorityToLeave);
			AssertIsAuthorisedToLeaveWithFallback(org, org.MiscServ.OM_ConsigneeAuthorityToLeave);
			AssertIsAuthorisedToLeaveWithFallback(org, org.MiscServ.OM_ConsignorAuthorityToLeave);
		}

		void AssertIsAuthorisedToLeaveWithFallback(OrgHeader org, ZString altOption)
		{
			var orgServ = org.MiscServ;

			altOption = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Should return true, as ATL has been set to Yes", true, orgServ.IsAuthorisedToLeaveWithFallback(altOption));

			altOption = AuthorityToLeaveOptions.Codes.NO;
			AssertEquals("Should return false, as ATL has been set to No", false, orgServ.IsAuthorisedToLeaveWithFallback(altOption));

			altOption = AuthorityToLeaveOptions.Codes.DEF;
			ObjectFactory.Get<ITransportRegistry>().AuthorityToLeave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Should return true, as it should fall back to the registry item which has been set to Yes", true, orgServ.IsAuthorisedToLeaveWithFallback(altOption));

			ObjectFactory.Get<ITransportRegistry>().AuthorityToLeave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Should return false, as it should fall back to the registry item which has been set to No", false, orgServ.IsAuthorisedToLeaveWithFallback(altOption));
		}

		#endregion

		#region LastUnactioned

		[TestDate(2014, 04, 28)]
		public void TestLastUnactioned()
		{
			var org1 = Factory.NewWithValidTestData<OrgMiscServ>();
			var org2 = Factory.NewWithValidTestData<OrgMiscServ>();

			var call1 = Factory.NewWithValidTestData<OrgSalesCall>();
			call1.OQ_CallDate = new ZDateTime(2014, 04, 27);
			call1.OQ_NextCall = new ZDateTime(2014, 04, 27);

			var call2 = Factory.NewWithValidTestData<OrgSalesCall>();
			call2.OQ_CallDate = ZDateTime.Empty;
			call2.OQ_NextCall = new ZDateTime(2014, 04, 16);

			var call3 = Factory.NewWithValidTestData<OrgSalesCall>();
			call3.OQ_CallDate = ZDateTime.Empty;
			call3.OQ_NextCall = new ZDateTime(2014, 04, 29);

			org1.Header.SalesCalls.Add(call1);
			org1.Header.SalesCalls.Add(call2);
			org1.Header.SalesCalls.Add(call3);

			var call4 = Factory.NewWithValidTestData<OrgSalesCall>();
			call4.OQ_CallDate = ZDateTime.Empty;
			call4.OQ_NextCall = new ZDateTime(2014, 04, 26);
			org2.Header.SalesCalls.Add(call4);

			Factory.Save();

			AssertEquals("Last Unactioned should be empty", org1.GetLastUnactionedCallDateSalesCall(), null);

			call1.OQ_CallDate = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("Last Unactioned should display incomplete action", org1.GetLastUnactionedCallDateSalesCall().OQ_NextCall, call1.OQ_NextCall);
		}

		#endregion

		#region TestOM_WhsIncludeClientInABCAnalysis

		public void TestOM_WhsIncludeClientInABCAnalysis()
		{
			miscServ.OM_WhsABCAnalysisEnabled = true;
			AssertEquals(true, miscServ.OM_WhsABCAnalysisEnabled);

			miscServ.OM_WhsABCAnalysisMethod = "";
			miscServ.OM_WhsABCAnalysisEnabled = false;
			AssertEquals("DEF", miscServ.OM_WhsABCAnalysisMethod);

			miscServ.OM_WhsABCAnalysisEnabled = true;
			miscServ.OM_WhsABCAnalysisMethod = "XXX";
			miscServ.OM_WhsABCAnalysisEnabled = false;
			AssertEquals("DEF", miscServ.OM_WhsABCAnalysisMethod);

			miscServ.OM_WhsABCAnalysisEnabled = true;
			miscServ.OM_WhsABCAnalysisMethod = "VLC";
			miscServ.OM_WhsABCAnalysisEnabled = false;
			AssertEquals("DEF", miscServ.OM_WhsABCAnalysisMethod);

			miscServ.OM_WhsABCAnalysisEnabled = true;
			miscServ.OM_WhsABCAnalysisPeriod = "";
			miscServ.OM_WhsABCAnalysisEnabled = false;
			AssertEquals("DEF", miscServ.OM_WhsABCAnalysisPeriod);

			miscServ.OM_WhsABCAnalysisEnabled = true;
			miscServ.OM_WhsABCAnalysisPeriod = "XXX";
			miscServ.OM_WhsABCAnalysisEnabled = false;
			AssertEquals("DEF", miscServ.OM_WhsABCAnalysisPeriod);

			miscServ.OM_WhsABCAnalysisEnabled = true;
			miscServ.OM_WhsABCAnalysisPeriod = "WKY";
			miscServ.OM_WhsABCAnalysisEnabled = false;
			AssertEquals("DEF", miscServ.OM_WhsABCAnalysisPeriod);
		}

		#endregion

		#region TestOM_WhsABCAnalysisMethod

		public void TestOM_WhsABCAnalysisMethod()
		{
			miscServ.OM_WhsABCAnalysisMethod = "VLC";
			AssertEquals("VLC", miscServ.OM_WhsABCAnalysisMethod);
		}

		#endregion

		#region TestOM_WhsABCAnalysisPeriod

		public void TestOM_WhsABCAnalysisPeriod()
		{
			miscServ.OM_WhsABCAnalysisPeriod = "WKY";
			AssertEquals("WKY", miscServ.OM_WhsABCAnalysisPeriod);
		}

		#endregion

		#region TestOM_WCG_CartonGroup

		public void TestOM_WCG_CartonGroup()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(OrgMiscServ), OrgMiscServSchema.Constants.OM_WCG_CartonGroup, false,
				la => la.ListDataSourceMember == "Lookups.CartonGroups");
		}

		#endregion

		#region TestDGPhoneNumber()

		public void TestDGPhoneNumber()
		{
			miscServ.OM_OC_EXDefaultDGContact = ZGuid.Empty;
			AssertEquals(ZString.Empty, miscServ.DGPhoneNumber);

			OrgContact contact = company.Contacts.AddNew();
			miscServ.OM_OC_EXDefaultDGContact = contact.PK;

			contact.OC_HomePhone = "123";
			contact.OC_Mobile = "456";
			contact.OC_OtherPhone = "789";
			contact.OC_Phone = "101";
			AssertPhoneNumber(PhoneTypeList.Codes.HOM, contact.OC_HomePhone);
			AssertPhoneNumber(PhoneTypeList.Codes.MOB, contact.OC_Mobile);
			AssertPhoneNumber(PhoneTypeList.Codes.OTH, contact.OC_OtherPhone);
			AssertPhoneNumber(PhoneTypeList.Codes.WRK, contact.OC_Phone);
			AssertPhoneNumber("1X1", "");
		}

		void AssertPhoneNumber(ZString phoneCode, ZString phoneNumber)
		{
			miscServ.OM_EXDefaultDGContactPhoneUsed = phoneCode;
			AssertEquals(phoneNumber, miscServ.DGPhoneNumber);
		}

		#endregion

		#region TestDGPhoneNumber_Formatted

		public void TestDGPhoneNumber_Formatted()
		{
			miscServ.OM_OC_EXDefaultDGContact = ZGuid.Empty;
			AssertEquals(ZString.Empty, miscServ.DGPhoneNumber_Formatted);

			OrgContact contact = company.Contacts.AddNew();
			company.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			miscServ.OM_OC_EXDefaultDGContact = contact.PK;

			contact.OC_HomePhone = "02 5555 9999";
			contact.OC_Mobile = "0420 019 999";
			contact.OC_OtherPhone = "02 3333 6666";
			contact.OC_Phone = "0420 011 022";
			AssertPhoneNumber_Formatted(PhoneTypeList.Codes.HOM, contact.OC_HomePhone_Formatted);
			AssertPhoneNumber_Formatted(PhoneTypeList.Codes.MOB, contact.OC_Mobile_Formatted);
			AssertPhoneNumber_Formatted(PhoneTypeList.Codes.OTH, contact.OC_OtherPhone_Formatted);
			AssertPhoneNumber_Formatted(PhoneTypeList.Codes.WRK, contact.OC_Phone_Formatted);
			AssertPhoneNumber_Formatted("1X1", "");
		}

		void AssertPhoneNumber_Formatted(ZString phoneCode, ZString phoneNumber)
		{
			miscServ.OM_EXDefaultDGContactPhoneUsed = phoneCode;
			AssertEquals(phoneNumber, miscServ.DGPhoneNumber_Formatted);
		}
		#endregion

		#region DistanceCalculation

		public void TestOM_CMDistanceCalculationVersion()
		{
			miscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.PCMiler;
			miscServ.OM_CMDistanceCalculationVersion = "99";

			AssertEquals("Precondition", "99", miscServ.OM_CMDistanceCalculationVersion);
			AssertEquals("Precondition", false, miscServ.OM_CMDistanceCalculationVersionInfo.ReadOnly);

			miscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.DefaultFromRegistry;
			AssertEquals("Version is cleared when no list with versions", "", miscServ.OM_CMDistanceCalculationVersion);

			foreach (CodeDescriptionPair pair in miscServ.OM_CMDistanceCalculationProvider_List)
			{
				miscServ.OM_CMDistanceCalculationProvider = pair.Code;
				AssertEquals("Version is not readonly only for PCMiler", pair.Code != DistanceCalculationConstants.Providers.PCMiler, miscServ.OM_CMDistanceCalculationVersionInfo.ReadOnly);
			}
		}

		public void TestOM_CMDistanceCalculationMethod()
		{
			miscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.PCMiler;
			miscServ.OM_CMDistanceCalculationMethod = DistanceCalculationConstants.CalculationMethods.PCMiler.Practical;

			AssertEquals("Precondition", DistanceCalculationConstants.CalculationMethods.PCMiler.Practical, miscServ.OM_CMDistanceCalculationMethod);
			AssertEquals("Precondition", false, miscServ.OM_CMDistanceCalculationMethodInfo.ReadOnly);

			miscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.DefaultFromRegistry;
			AssertEquals("Method is cleared when no list with methods", "", miscServ.OM_CMDistanceCalculationMethod);

			foreach (CodeDescriptionPair pair in miscServ.OM_CMDistanceCalculationProvider_List)
			{
				miscServ.OM_CMDistanceCalculationProvider = pair.Code;
				AssertEquals("Method is not readonly only for PCMiler", pair.Code != DistanceCalculationConstants.Providers.PCMiler, miscServ.OM_CMDistanceCalculationMethodInfo.ReadOnly);
			}
		}

		#endregion

		#region TestLastCostlist

		public void TestLastCostlist()
		{
			AssertEquals("OM_EXInvPriceFromLastCost_List", typeof(PopulateInvPriceFromLastCostList), miscServ.OM_EXInvPriceFromLastCost_List.GetType());
		}

		#endregion

		#region TestValidateOM_EXDefaultInvoicePriceFromProductLastCost

		public void TestValidateOM_EXDefaultInvoicePriceFromProductLastCost()
		{
			company.OH_IsConsignor = true;
			company.MiscServ.OM_EXDefaultInvoicePriceFromProductLastCost = "why";
			Assert("Validation should fail", company.MiscServ.OM_EXDefaultInvoicePriceFromProductLastCostInfo.HasErrors());

			company.MiscServ.OM_EXDefaultInvoicePriceFromProductLastCost = "NO";
			Assert("Validation should now pass", !company.MiscServ.OM_EXDefaultInvoicePriceFromProductLastCostInfo.HasErrors());
		}

		#endregion

		#region  TestOM_WhsPackingSlipOrderByList

		public void TestOM_WhsPackingSlipOrderByList()
		{
			AssertEquals(true, company.MiscServ.OM_WhsPackingSlipOrderBy_List.ContainsCode("DEF"));
			AssertEquals(true, company.MiscServ.OM_WhsPackingSlipOrderBy_List.ContainsCode(WhsPackingSlipOrderByList.Codes.ProductCode));
			AssertEquals(true, company.MiscServ.OM_WhsPackingSlipOrderBy_List.ContainsCode(WhsPackingSlipOrderByList.Codes.ProductDescription));
			AssertEquals(true, company.MiscServ.OM_WhsPackingSlipOrderBy_List.ContainsCode(WhsPackingSlipOrderByList.Codes.LineNo));
			AssertEquals(true, company.MiscServ.OM_WhsPackingSlipOrderBy_List.ContainsCode(WhsPackingSlipOrderByList.Codes.SameAsOnGrid));
		}

		#endregion

		#region TestOM_WhsDefaultWarehousePickMode

		public void TestOM_WhsDefaultWarehousePickMode()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals(false, org.MiscServ.OM_WhsDefaultWarehouseRollUp);

			org.MiscServ.OM_WhsDefaultWarehousePickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals(false, org.MiscServ.OM_WhsDefaultWarehouseRollUp);

			org.MiscServ.OM_WhsDefaultWarehouseRollUp = true;
			AssertEquals(true, org.MiscServ.OM_WhsDefaultWarehouseRollUp);

			org.MiscServ.OM_WhsDefaultWarehousePickMode = WhsPickMode.Codes.AttributeSpecified;
			AssertEquals(false, org.MiscServ.OM_WhsDefaultWarehouseRollUp);
		}

		#endregion

		#region TestPartAttributeRuleList

		public void TestPartAttributeRuleList()
		{
			var org = Factory.New<OrgHeader>();
			var list = org.MiscServ.PartAttributeRuleList;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
						PartAttributeTypeList.Codes.BatchNumber,
						PartAttributeTypeList.Codes.JulianBatchNumber,
						PartAttributeTypeList.Codes.Mandatory,
						PartAttributeTypeList.Codes.NonMandatory,
						PartAttributeTypeList.Codes.VIN,
				},
				list.GetAllCodes());
		}

		#endregion

		#region TestOM_IMAirCargoReportDefaultConsignee

		public void TestOM_IMAirCargoReportDefaultConsignee()
		{
			var miscServ = Factory.New<OrgMiscServ>();
			AssertEquals("Default value", ConsigneeDefaultOptionList.Codes.Default, miscServ.OM_IMAirCargoReportDefaultConsignee);
			AssertEquals("Caption", "ACR Consignee Default", DataBoundResourceStrings.GetDataForProperty(miscServ.OM_IMAirCargoReportDefaultConsigneeInfo).Caption);
		}

		#endregion

		#region TestOM_IMSeaCargoReportDefaultConsignee

		public void TestOM_IMSeaCargoReportDefaultConsignee()
		{
			var miscServ = Factory.New<OrgMiscServ>();
			AssertEquals("Default value", ConsigneeDefaultOptionList.Codes.Default, miscServ.OM_IMSeaCargoReportDefaultConsignee);
			AssertEquals("Caption", "SCR Consignee Default", DataBoundResourceStrings.GetDataForProperty(miscServ.OM_IMSeaCargoReportDefaultConsigneeInfo).Caption);
		}

		#endregion

		#region TestOM_IMPartAttribTypes

		public void TestOM_IMPartAttrib1Type()
		{
			TestOM_IMPartAttribTypeCore(OrgMiscServSchema.OM_IMPartAttrib1Type);
		}

		public void TestOM_IMPartAttrib2Type()
		{
			TestOM_IMPartAttribTypeCore(OrgMiscServSchema.OM_IMPartAttrib2Type);
		}

		public void TestOM_IMPartAttrib3Type()
		{
			TestOM_IMPartAttribTypeCore(OrgMiscServSchema.OM_IMPartAttrib3Type);
		}

		void TestOM_IMPartAttribTypeCore(SchemaStringColumn partAttributeTypeColumn)
		{
			var miscServ = Factory.New<OrgMiscServ>();
			AssertEquals("Precondition", true, ((ZString)miscServ[partAttributeTypeColumn]).IsEmpty);
			AssertEquals("Precondition", false, miscServ.OM_IMUsePackingDate);
			AssertEquals("Precondition", false, miscServ.OM_IMUseExpiryDate);

			miscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals("Setting Part Attribute Type to any type other than BAJ should not turn on Packing Date usage.", false, miscServ.OM_IMUsePackingDate);
			AssertEquals("Setting Part Attribute Type to any type other than BAJ should not turn on Expiry Date usage.", false, miscServ.OM_IMUseExpiryDate);

			miscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertEquals("Setting Part Attribute Type to Julian Batch Number should turn on Packing Date usage.", true, miscServ.OM_IMUsePackingDate);
			AssertEquals("Setting Part Attribute Type to Julian Batch Number should turn on Expiry Date usage.", true, miscServ.OM_IMUseExpiryDate);

			miscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals("Setting Part Attribute Type to any type other than BAJ should not turn off Packing Date usage if it was checked beforehand.", true, miscServ.OM_IMUsePackingDate);
			AssertEquals("Setting Part Attribute Type to any type other than BAJ should not turn off Expiry Date usage if it was checked beforehand.", true, miscServ.OM_IMUseExpiryDate);
		}

		#endregion

		#region TestTemoraryCreditLimitColumns

		[TestDate(2014, 02, 06, 07, 55, 00)]
		public void TestTemoraryCreditLimitColumns()
		{
			using (new MasterFilesTestHelper(Factory).GetUtcPlus8UserContext())
			{
				var expiryDateTime = ZDateTime.UtcNow.AddDays(1);

				// Getting Tests
				miscServ.Header.CompanyData.OB_ARCreditLimit = 103M;
				miscServ.Header.CompanyData.OB_ARTemporaryCreditLimitIncrease = 99M;
				miscServ.Header.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry = expiryDateTime;
				AssertEquals(99M, miscServ.ARTemporaryCreditLimitIncrease);
				AssertEquals(202M, miscServ.ARTemporaryCreditLimit);
				AssertEquals(expiryDateTime.AddHours(8), miscServ.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);

				// Setting Tests
				miscServ.ARTemporaryCreditLimitIncrease = 44M;
				AssertEquals(44M, miscServ.ARTemporaryCreditLimitIncrease);
				AssertEquals(44M, miscServ.Header.CompanyData.OB_ARTemporaryCreditLimitIncrease);
			}
		}

		#endregion

		#region TestVoyageRecyclingPeriodCode

		public void TestVoyageRecyclingPeriodCode()
		{
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_CRVoyageRecyclingPeriodInMonths = 6;
			AssertEquals("6", miscServ.VoyageRecyclingPeriodCode);

			miscServ.VoyageRecyclingPeriodCode = "XXX";
			AssertEquals(VoyageRecyclingPeriodList.Invalid, miscServ.OM_CRVoyageRecyclingPeriodInMonths);

			miscServ.VoyageRecyclingPeriodCode = VoyageRecyclingPeriodList.Codes.Default;
			AssertEquals(VoyageRecyclingPeriodList.Default, miscServ.OM_CRVoyageRecyclingPeriodInMonths);

			miscServ.VoyageRecyclingPeriodCode = VoyageRecyclingPeriodList.Codes.None;
			AssertEquals(new ZShort(0), miscServ.OM_CRVoyageRecyclingPeriodInMonths);

			miscServ.VoyageRecyclingPeriodCode = VoyageRecyclingPeriodList.Codes.TwelveMonths;
			AssertEquals(new ZShort(12), miscServ.OM_CRVoyageRecyclingPeriodInMonths);
		}

		#endregion

		#region TestFWRequiresElectronicBOLForNonDirectConsolCaption

		public void TestFWRequiresElectronicBOLForNonDirectConsolCaption()
		{
			AssertEquals("Caption should be set correctly", "Require Electronic Bill of Lading", miscServ.OM_FWRequiresElectronicBOLForNonDirectConsolInfo.HumanReadableName);
		}

		#endregion

		#region TestGS_PreferredPaymentCompany

		public void TestUseTransactionCompanyAsPreferredPayment_SettingToTrueClearsGS_GC_PreferredPaymentCompany()
		{
			miscServ.UseTransactionCompanyAsPreferredPayment = false;
			miscServ.OM_GC_CMPreferredPaymentCompany = GlbCompany.CurrentCompany.PK;

			miscServ.UseTransactionCompanyAsPreferredPayment = true;
			AssertEquals(ZGuid.Empty, miscServ.OM_GC_CMPreferredPaymentCompany);
		}

		public void TestIsGlobal_OnLoaded()
		{
			var miscServWithoutPreferredPaymentCompany = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServWithoutPreferredPaymentCompany.OM_GC_CMPreferredPaymentCompany = ZGuid.Empty;
			var miscServWithPreferredPaymentCompany = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServWithPreferredPaymentCompany.OM_GC_CMPreferredPaymentCompany = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			AssertEquals(true, otherFactory.Load<OrgMiscServ>(miscServWithoutPreferredPaymentCompany.PK).UseTransactionCompanyAsPreferredPayment);
			AssertEquals(false, otherFactory.Load<OrgMiscServ>(miscServWithPreferredPaymentCompany.PK).UseTransactionCompanyAsPreferredPayment);
		}

		public void TestCreateNewOrganisation_ShouldCreate_GlobalOnHoldEventLog()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			var logsBefore = org.MiscServ.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));

			org.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();

			var logsAfter = org.MiscServ.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModifiedCode));
			AssertEquals("Global Credit On Hold log must be added.", logsBefore.Length + 1, logsAfter.Length);
			Assert("New log is global credit on hold", logsAfter[0].SL_Reference.Contains("Global Credit On Hold"));
		}

		public void TestGS_GC_PreferredPaymentCompany_ReadOnly()
		{
			miscServ.UseTransactionCompanyAsPreferredPayment = true;
			AssertEquals(true, miscServ.OM_GC_CMPreferredPaymentCompanyInfo.ReadOnly);

			miscServ.UseTransactionCompanyAsPreferredPayment = false;
			AssertEquals(false, miscServ.OM_GC_CMPreferredPaymentCompanyInfo.ReadOnly);
		}

		#endregion

		#region TestOM_IMDisallowOrders

		public void TestOM_IMDisallowOrders()
		{
			var orgMiscServ = (OrgMiscServ)GetNewBusinessObject();
			AssertEquals("Default Value - OM_IMAllowOrders", true, orgMiscServ.OM_IMAllowOrders);
			AssertEquals("Default Value - OM_IMDisallowOrders ", false, orgMiscServ.OM_IMDisallowOrders);

			orgMiscServ.OM_IMDisallowOrders = true;
			AssertEquals("Setting OM_IMDisallowOrders changes OM_IMAllowOrders ", false, orgMiscServ.OM_IMAllowOrders);

			orgMiscServ.OM_IMAllowOrders = true;
			AssertEquals("Setting OM_IMAllowOrders changes OM_IMDisallowOrders", false, orgMiscServ.OM_IMDisallowOrders);
		}

		#endregion

		#region TestOM_CMClientCommenced

		public void TestOM_CMClientCommenced_ExistingOrgMiscServ()
		{
			AssertOM_CMClientCommenced(true);
		}

		public void TestOM_CMClientCommenced_NewOrgMiscServ()
		{
			AssertOM_CMClientCommenced(false);
		}

		void AssertOM_CMClientCommenced(bool saveBeforeSettingDate)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~test123~";
			org.OH_FullName = Guid.NewGuid().ToString();

			var agreement1 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement1.CA0_OH_Customer = org.PK;
			agreement1.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			agreement1.CA0_LastApprovedDateUtc = ZDateTime.Now;

			var agreement2 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement2.CA0_OH_Customer = org.PK;
			agreement2.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			agreement1.CA0_LastApprovedDateUtc = ZDateTime.Now;

			var agreement3 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement3.CA0_OH_Customer = org.PK;
			agreement3.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;
			agreement3.CA0_LastApprovedDateUtc = ZDateTime.Now;
			agreement3.Reverse();

			var agreement4 = Factory.NewWithValidTestData<OrgCommissionAgreement>().CreateDraft();
			agreement4.CA0_OH_Customer = org.PK;
			agreement4.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;

			var agreement5 = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			agreement5.CA0_OH_Customer = org.PK;
			agreement5.CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.ClientCommencedDate;

			agreement1.SendToCalculationQueue(ZDateTime.BrettsBirthday, true);
			agreement2.SendToCalculationQueue(ZDateTime.BrettsBirthday, true);
			agreement3.SendToCalculationQueue(ZDateTime.BrettsBirthday, true);
			agreement4.SendToCalculationQueue(ZDateTime.BrettsBirthday, true);
			agreement5.SendToCalculationQueue(ZDateTime.BrettsBirthday, true);

			var queue1 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement1.PK));
			AssertEquals(1, queue1.Length);
			var queue1PK = queue1[0].PK;

			var queue2 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement2.PK));
			AssertEquals(1, queue2.Length);
			var queue2PK = queue2[0].PK;

			var queue3 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement3.PK));
			AssertEquals(1, queue3.Length);
			var queue3PK = queue3[0].PK;

			var queue4 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement4.PK));
			AssertEquals(1, queue4.Length);
			var queue4PK = queue4[0].PK;

			var queue5 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement5.PK));
			AssertEquals(1, queue5.Length);
			var queue5PK = queue5[0].PK;

			var commDate = ZDateTime.Now;

			if (saveBeforeSettingDate)
			{
				Factory.Save();
			}

			org.MiscServ.OM_CMClientCommenced = commDate;
			Factory.Save();

			queue1 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement1.PK));
			AssertEquals(1, queue1.Length);

			AssertNotEquals(queue1PK, queue1[0].PK);
			AssertEquals(ZDateTime.MinSmallDateTimeValue, queue1[0].CAQ_MinimumInvoicePostedDate);

			queue2 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement2.PK));
			AssertEquals(1, queue2.Length);

			AssertEquals(queue2PK, queue2[0].PK);
			AssertEquals(ZDateTime.BrettsBirthday, queue2[0].CAQ_MinimumInvoicePostedDate);

			queue3 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement3.PK));
			AssertEquals(1, queue3.Length);

			AssertEquals(queue3PK, queue3[0].PK);
			AssertEquals(ZDateTime.BrettsBirthday, queue3[0].CAQ_MinimumInvoicePostedDate);

			queue4 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement4.PK));
			AssertEquals(1, queue4.Length);

			AssertEquals(queue4PK, queue4[0].PK);
			AssertEquals(ZDateTime.BrettsBirthday, queue4[0].CAQ_MinimumInvoicePostedDate);

			queue5 = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, agreement5.PK));
			AssertEquals(1, queue5.Length);

			AssertEquals(queue5PK, queue5[0].PK);
			AssertEquals(ZDateTime.BrettsBirthday, queue5[0].CAQ_MinimumInvoicePostedDate);
		}

		#endregion

		#region TestOM_IMPartAttribNames_Translatable

		public void TestOM_IMPartAttrib1Name_Translatable()
		{
			TestOM_IMPartAttribNames_TranslatableCore(OrgMiscServSchema.OM_IMPartAttrib1Name, "TestPartAttrib1Name", "测试属性1");
		}

		public void TestOM_IMPartAttrib2Name_Translatable()
		{
			TestOM_IMPartAttribNames_TranslatableCore(OrgMiscServSchema.OM_IMPartAttrib2Name, "TestPartAttrib2Name", "测试属性2");
		}

		public void TestOM_IMPartAttrib3Name_Translatable()
		{
			TestOM_IMPartAttribNames_TranslatableCore(OrgMiscServSchema.OM_IMPartAttrib3Name, "TestPartAttrib3Name", "测试属性3");
		}

		void TestOM_IMPartAttribNames_TranslatableCore(SchemaStringColumn partAttributeNameColumn, ZString partAttributeName, string chsTranslationString)
		{
			var orgMiscServ = (OrgMiscServ)GetNewBusinessObject();
			orgMiscServ[partAttributeNameColumn] = partAttributeName;
			AssertEquals($"Pre-condition: {partAttributeNameColumn.Name}.", partAttributeName, (ZString)orgMiscServ[partAttributeNameColumn]);

			string resKey = "";
			MultilingualString partAttribMultiLingualString = null;

			switch (partAttributeNameColumn.Name)
			{
				case "OM_IMPartAttrib1Name":
					partAttribMultiLingualString = orgMiscServ.OM_IMPartAttrib1NameMultilingual;
					resKey = orgMiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(orgMiscServ, partAttributeName).ResourceKey;
					break;
				case "OM_IMPartAttrib2Name":
					partAttribMultiLingualString = orgMiscServ.OM_IMPartAttrib2NameMultilingual;
					resKey = orgMiscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(orgMiscServ, partAttributeName).ResourceKey;
					break;
				case "OM_IMPartAttrib3Name":
					partAttribMultiLingualString = orgMiscServ.OM_IMPartAttrib3NameMultilingual;
					resKey = orgMiscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(orgMiscServ, partAttributeName).ResourceKey;
					break;
				default:
					break;
			}

			AssertEquals(partAttributeName, partAttribMultiLingualString);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, chsTranslationString));
				AssertEquals(chsTranslationString, partAttribMultiLingualString);
			}
		}

		#endregion

		#region TestOM_WhsPackageToleranceEnabled

		public void TestOM_WhsPackageToleranceEnabled()
		{
			miscServ.OM_WhsPackageToleranceEnabled = true;
			AssertEquals(true, miscServ.OM_WhsPackageToleranceEnabled);
			AssertEquals(false, miscServ.OM_WhsPackageWeightTolerancePercentInfo.ReadOnly);

			miscServ.OM_WhsPackageToleranceEnabled = false;
			AssertEquals(false, miscServ.OM_WhsPackageToleranceEnabled);
			AssertEquals(true, miscServ.OM_WhsPackageWeightTolerancePercentInfo.ReadOnly);
		}

		#endregion

		#region Sales

		public void TestOM_CMIndustryVerticalDescription()
		{
			var industryVerticalTypes = new CodeDescriptionBoolCollection(OrgMiscServSchema.OM_CMIndustryVertical.MaxLength);
			industryVerticalTypes.Add("AAA", (NoResString)"AAA Desc", true);
			industryVerticalTypes.Add("BBB", (NoResString)"BBB Desc", false);
			OrganisationsDataRegistry.Instance.IndustryVerticalTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, industryVerticalTypes);

			miscServ.OM_CMIndustryVertical = "AAA";
			AssertEquals("AAA Desc", miscServ.OM_CMIndustryVerticalDescription);

			miscServ.OM_CMIndustryVertical = "BBB";
			AssertEquals("BBB Desc", miscServ.OM_CMIndustryVerticalDescription);
		}

		public void TestOM_CMPeriodOfActivityDescription()
		{
			var periodOfActivities = new CodeDescriptionWithEnabledAndDefaultCollection(OrgMiscServSchema.OM_CMPeriodOfActivity.MaxLength);
			periodOfActivities.AddNew("AAA", (NoResString)"AAA Desc", true, true);
			periodOfActivities.AddNew("BBB", (NoResString)"BBB Desc", false, false);
			OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodOfActivities);

			miscServ.OM_CMPeriodOfActivity = "AAA";
			AssertEquals("AAA Desc", miscServ.OM_CMPeriodOfActivityDescription);

			miscServ.OM_CMPeriodOfActivity = "BBB";
			AssertEquals("BBB Desc", miscServ.OM_CMPeriodOfActivityDescription);
		}

		#endregion

		#region TestOM_WhsEnforceScanOfProductsWhenPackingTote

		public void TestOM_WhsEnforceScanOfProductsWhenPackingTote()
		{
			var orgMiscServ = (OrgMiscServ)GetNewBusinessObject();
			AssertEquals("Precondition: Default value is false.", false, orgMiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote);
			orgMiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote = true;
			Factory.Save();

			AssertEquals("Value is updated to true.", true, new BusinessObjectFactory().Load<OrgMiscServ>(orgMiscServ.PK).OM_WhsEnforceScanOfProductsWhenPackingTote);
		}

		#endregion

		#region Global Credit Limit

		public void TestOM_OH_ARGlobalCreditGroup_Set()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			var currencyCode = "EUR";

			Assert(miscServ.OM_OH_ARGlobalCreditGroup.IsEmpty);
			miscServ.OM_ARGlobalCreditLimit = 10000m;
			miscServ.OM_RX_NKARGlobalCreditCurrency = currencyCode;

			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Empty;
			AssertEquals(10000m, miscServ.OM_ARGlobalCreditLimit);
			AssertEquals(currencyCode, miscServ.OM_RX_NKARGlobalCreditCurrency);

			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Invalid;
			AssertEquals(10000m, miscServ.OM_ARGlobalCreditLimit);
			AssertEquals(currencyCode, miscServ.OM_RX_NKARGlobalCreditCurrency);

			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.PK;
			Assert(miscServ.OM_ARGlobalCreditLimit.IsEmpty);
			Assert(miscServ.OM_RX_NKARGlobalCreditCurrency.IsEmpty);

			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Empty;
			miscServ.OM_ARGlobalCreditLimit = 20000m;
			miscServ.OM_RX_NKARGlobalCreditCurrency = currencyCode;

			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.PK;
			Assert(miscServ.OM_ARGlobalCreditLimit.IsEmpty);
			Assert(miscServ.OM_RX_NKARGlobalCreditCurrency.IsEmpty);
			AssertEquals(this.miscServ.OM_ARGlobalCreditApproved, false);
			AssertEquals(this.miscServ.OM_ARGlobalOnCreditHold, false);
		}

		public void TestARGlobalCreditLimit()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_ARGlobalCreditLimit = 10m;
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			groupMiscServ.OM_ARGlobalCreditLimit = 20m;
			Factory.Save();

			miscServ.OM_ARGlobalCreditApproved = true;
			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Empty;
			AssertEquals(10m, miscServ.ARGlobalCreditLimit);
			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Invalid;
			AssertEquals(10m, miscServ.ARGlobalCreditLimit);

			miscServ.OM_ARGlobalCreditApproved = false;
			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Empty;
			AssertEquals(0m, miscServ.ARGlobalCreditLimit);
			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Invalid;
			AssertEquals(0m, miscServ.ARGlobalCreditLimit);

			groupMiscServ.OM_ARGlobalCreditApproved = true;
			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertEquals(20m, miscServ.ARGlobalCreditLimit);

			groupMiscServ.OM_ARGlobalCreditApproved = false;
			AssertEquals(0m, miscServ.ARGlobalCreditLimit);
		}

		public void TestARGlobalCreditApproved()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			Factory.Save();

			miscServ.OM_ARGlobalCreditApproved = true;
			groupMiscServ.OM_ARGlobalCreditApproved = false;

			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Empty;
			Assert(miscServ.ARGlobalCreditApproved);
			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Invalid;
			Assert(miscServ.ARGlobalCreditApproved);

			miscServ.OM_ARGlobalCreditApproved = false;
			groupMiscServ.OM_ARGlobalCreditApproved = true;
			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;

			Assert(miscServ.ARGlobalCreditApproved);

			groupMiscServ.OM_ARGlobalCreditApproved = false;
			AssertEquals(0m, miscServ.ARGlobalCreditLimit);
		}

		public void TestARGlobalCreditCurrencyCodeWithARGlobalCreditApproved()
		{
			AssertARGlobalCreditCurrencyCode(true, true);
			AssertARGlobalCreditCurrencyCode(false, false);
			AssertARGlobalCreditCurrencyCode(false, true);
			AssertARGlobalCreditCurrencyCode(true, false);
		}

		void AssertARGlobalCreditCurrencyCode(bool isARGlobalCreditApproved, bool isParentARGlobalCreditApproved)
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_RX_NKARGlobalCreditCurrency = "USD";
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			groupMiscServ.OM_RX_NKARGlobalCreditCurrency = "EUR";
			Factory.Save();

			miscServ.OM_ARGlobalCreditApproved = isARGlobalCreditApproved;
			groupMiscServ.OM_ARGlobalCreditApproved = isParentARGlobalCreditApproved;

			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Empty;
			AssertEquals("USD", miscServ.ARGlobalCreditCurrencyCode);
			AssertEquals("USD", miscServ.GlobalCreditCurrency.RX_Code);
			miscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Invalid;
			AssertEquals("USD", miscServ.ARGlobalCreditCurrencyCode);
			AssertEquals("USD", miscServ.GlobalCreditCurrency.RX_Code);

			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertEquals("EUR", miscServ.ARGlobalCreditCurrencyCode);
			AssertEquals("EUR", miscServ.GlobalCreditCurrency.RX_Code);
		}

		public void TestGlobalCreditCurrencyDecimals()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			miscServ.OM_RX_NKARGlobalCreditCurrency = "USD";
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			groupMiscServ.OM_RX_NKARGlobalCreditCurrency = "IDR";
			Factory.Save();

			AssertEquals(2, miscServ.GlobalCreditCurrencyDecimals);

			miscServ.OM_RX_NKARGlobalCreditCurrency = "ZZZ";
			AssertEquals(2, miscServ.GlobalCreditCurrencyDecimals);

			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertEquals(0, miscServ.GlobalCreditCurrencyDecimals);
		}

		public void TestLocalCreditLimitTotalAndIsInvalidGlobalCreditCurrencyOrMissingExRate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_ARCreditLimit = 500m;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";
			Factory.Save();

			Assert(!org.MiscServ.IsInvalidGlobalCreditCurrencyOrMissingExRate);

			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "EUR";

			Assert(org.MiscServ.IsInvalidGlobalCreditCurrencyOrMissingExRate);

			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			exchangeRate.RE_SellRate = 1.1m;
			exchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
			exchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));
			Factory.Save();

			Assert(!new BusinessObjectFactory().Load<OrgHeader>(org.PK).MiscServ.IsInvalidGlobalCreditCurrencyOrMissingExRate);
			AssertEquals("no other company, only current MiscServ local credit limit", 550m, org.MiscServ.LocalCreditLimitTotal);

			var tempCompany = Factory.NewWithValidTestData<GlbCompany>();
			var tempBranch = Factory.NewWithValidTestData<GlbBranch>();
			tempBranch.GB_GC = tempCompany.PK;
			Factory.Save();

			BusinessObjectFactory tempFactory;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, tempBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				tempFactory = new BusinessObjectFactory();
				var tempOrg = tempFactory.Load<OrgHeader>(org.PK);
				tempOrg.CompanyData.OB_IsDebtor = true;
				tempOrg.CompanyData.OB_ARCreditLimit = 200m;
				tempOrg.CompanyData.OB_ARCreditApproved = true;

				tempFactory.Save();
			}

			Assert(new BusinessObjectFactory().Load<OrgHeader>(org.PK).MiscServ.IsInvalidGlobalCreditCurrencyOrMissingExRate);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, tempBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var tempCurrency = tempFactory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
				var tempExchangeRate = tempCurrency.ExchangeRates.AddNew();
				tempExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
				tempExchangeRate.RE_SellRate = 2m;
				tempExchangeRate.RE_StartDate = new ZDateTime(ZDateTime.Today.AddMonths(-1));
				tempExchangeRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.AddMonths(1));

				tempFactory.Save();
			}

			var reloadOrg = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			Assert(!reloadOrg.MiscServ.IsInvalidGlobalCreditCurrencyOrMissingExRate);
			AssertEquals("other company limit is 400 (200x2) + current 550", 950m, reloadOrg.MiscServ.LocalCreditLimitTotal);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, tempBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var tempOrg = tempFactory.Load<OrgHeader>(org.PK);
				tempOrg.CompanyData.OB_ARCreditApproved = false;

				tempFactory.Save();
			}

			AssertEquals("other company org local credit limit not approved should not be in the total", 550m, new BusinessObjectFactory().Load<OrgHeader>(org.PK).MiscServ.LocalCreditLimitTotal);
		}

		#endregion

		#region TestOM_CIFinancialDetailsApplicableToDate

		[TestDate(2020, 12, 18)]
		public void TestOM_CIFinancialDetailsApplicableToDate()
		{
			Assert(miscServ.OM_CIFinancialDetailsApplicableToDateInfo.ReadOnly);

			AssertEquals(ZDate.Empty, miscServ.OM_CIFinancialDetailsApplicableToDate);

			miscServ.OM_CIFinancialDetailsApplicableFromDate = ZDate.Today.AddYears(-1).AddDays(1);
			AssertEquals("From Date : 19-DEC-2019, To Date : 18-DEC-2020 (Today)", ZDate.Today, miscServ.OM_CIFinancialDetailsApplicableToDate);

			miscServ.OM_CIFinancialDetailsApplicableFromDate = new ZDate(2019, 7, 1);
			AssertEquals("From Date : 01-JUL-2019, To Date : 30-JUN-2020", new ZDate(2020, 6, 30), miscServ.OM_CIFinancialDetailsApplicableToDate);

			miscServ.OM_CIFinancialDetailsApplicableFromDate = ZDate.Today.AddYears(-1).AddDays(2);
			AssertEquals("From Date : 20-DEC-2019, To Date : 19-DEC-2020 (cannot be in the future)", ZDate.Empty, miscServ.OM_CIFinancialDetailsApplicableToDate);
		}

		#endregion

		#endregion

		#region Lookups

		#region Charge Codes

		public void TestChargeCodes()
		{
			GlbCompany companyNotThisCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_DepartmentFilterList = "ALL";
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			AccChargeCode chargeCodeSpecificDepartment = Factory.New<AccChargeCode>();
			chargeCodeSpecificDepartment.AC_DepartmentFilterList = "FEA";
			chargeCodeSpecificDepartment.AC_GC = GlbCompany.CurrentCompany.PK;

			AccChargeCode chargeCodeInDifferentCompany = Factory.New<AccChargeCode>();
			chargeCodeInDifferentCompany.AC_DepartmentFilterList = "FEA";
			chargeCodeInDifferentCompany.AC_GC = companyNotThisCompany.PK;

			OrgHeader org = Factory.New<OrgHeader>();
			AccChargeCodeCollection result = org.MiscServ.ChargeCodes;
			result.Load();

			AssertCollectionContains(chargeCode, result);
			AssertCollectionContains(chargeCodeSpecificDepartment, result);
			AssertCollectionNotContains(chargeCodeInDifferentCompany, result);
		}

		#endregion

		#region Receivables

		public void TestOM_ARConsolidatedAccountingCategory_List()
		{
			Assert("OM_ARConsolidatedAccountingCategory_List.Count > 0", miscServ.OM_ARConsolidatedAccountingCategory_List.Count > 0);
		}

		public void TestConsolidatedAccountingCategoryClass()
		{
			miscServ.OM_ARConsolidatedAccountingCategory = string.Empty;
			miscServ.OM_APConsolidatedAccountingCategory = string.Empty;
			AssertEquals(string.Empty, miscServ.ConsolidatedAccountingCategoryClass);

			miscServ.OM_ARConsolidatedAccountingCategory = AccountsCategory.Unrelated;
			AssertEquals("AP and AR property have the same root property", AccountsCategory.Unrelated, miscServ.OM_APConsolidatedAccountingCategory);
			AssertEquals(ConsolidatedAccountingCategoryClassList.Codes.ThirdParty, miscServ.ConsolidatedAccountingCategoryClass);

			miscServ.OM_ARConsolidatedAccountingCategory = AccountsCategory.WhollyOwned;
			AssertEquals(ConsolidatedAccountingCategoryClassList.Codes.Intercompany, miscServ.ConsolidatedAccountingCategoryClass);
		}

		public void TestOM_ARCategory_List()
		{
			Assert("OM_ARCategory_List.Count > 0", miscServ.OM_ARCategory_List.Count > 0);
		}

		public void TestOM_ARCreditRating_List()
		{
			Assert("OM_ARCreditRating_List.Count > 0", miscServ.OM_ARCreditRating_List.Count > 0);
		}

		#endregion

		#region Consignor

		public void TestOM_EXDocumentAddressPreference_List()
		{
			Assert("OM_EXDocumentAddressPreference_List.Count == 3", miscServ.OM_EXDocumentAddressPreference_List.Count == 3);
			Assert("OM_EXDocumentAddressPreference_List should have option DOC", miscServ.OM_EXDocumentAddressPreference_List.ContainsCode("DOC"));
			Assert("OM_EXDocumentAddressPreference_List should have option OFC", miscServ.OM_EXDocumentAddressPreference_List.ContainsCode("OFC"));
			Assert("OM_EXDocumentAddressPreference_List should have option DLV", miscServ.OM_EXDocumentAddressPreference_List.ContainsCode("PIC"));
		}

		[TestDate(2010, 12, 1)]
		public void TestOM_EXDefaultIncoTerm_List()
		{
			string[] actualCodes = (from code in miscServ.OM_EXDefaultIncoTerm_List.Cast<CodeDescriptionPair>() select code.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Core.Constants.IncoTerms.Incoterms2000.Concat(domesticPaymentTermsCodes), actualCodes);
		}

		[TestDate(2011, 12, 1)]
		public void TestOM_EXDefaultIncoTerm_List2()
		{
			string[] actualCodes = (from code in miscServ.OM_EXDefaultIncoTerm_List.Cast<CodeDescriptionPair>() select code.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Core.Constants.IncoTerms.Incoterms2010.Concat(domesticPaymentTermsCodes), actualCodes);
		}

		public void TestOM_EXExporterCategory_List()
		{
			Assert("OM_EXExporterCategory_List.Count > 0", miscServ.OM_EXExporterCategory_List.Count > 0);
		}

		#endregion

		#region Consignee

		public void TestOM_IMDocumentAddressPreference_List()
		{
			Assert("OM_IMDocumentAddressPreference_List.Count == 3", miscServ.OM_IMAutoPopulateOwnerRefList.Count == 3);
			Assert("OM_IMDocumentAddressPreference_List should have option DOC", miscServ.OM_IMDocumentAddressPreference_List.ContainsCode("DOC"));
			Assert("OM_IMDocumentAddressPreference_List should have option OFC", miscServ.OM_IMDocumentAddressPreference_List.ContainsCode("OFC"));
			Assert("OM_IMDocumentAddressPreference_List should have option DLV", miscServ.OM_IMDocumentAddressPreference_List.ContainsCode("DLV"));
		}

		public void TestOM_IMMergeCustomsInvoiceLinesBy_List()
		{
			NewCompanyAndBranch("TW", "TWX");
			NewCompanyAndBranch("CN", "CNX");
			NewCompanyAndBranch("CA", "CAX");
			Factory.Save();

			Assert("OM_IMMergeCustomsInvoiceLinesBy_List.Count == 9", miscServ.OM_IMMergeCustomsInvoiceLinesBy_List.Count == 9);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var twCompany = Factory.New<OrgHeader>();
				var miscserv = twCompany.MiscServ;

				twCompany.OH_RL_NKClosestPort = "TWTPE";
				twCompany.OH_FullName = "TestCompany";
				twCompany.MainAddress.OA_Address1 = "TestAddress";
				twCompany.MainAddress.OA_RN_NKCountryCode = "TW";

				AssertEquals("NON, TRD, PNO, TRF, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);

				twCompany.MainAddress.OA_RN_NKCountryCode = "CA";
				AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);

				twCompany.MainAddress.OA_RN_NKCountryCode = "CN";
				AssertEquals("NON, NOP, TRF, TRD, PNO, PNP, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var caCompany = Factory.New<OrgHeader>();
				var miscserv = caCompany.MiscServ;

				caCompany.OH_RL_NKClosestPort = "CAXXX";
				caCompany.OH_FullName = "TestCompany";
				caCompany.MainAddress.OA_Address1 = "TestAddress";
				caCompany.MainAddress.OA_RN_NKCountryCode = "TW";

				AssertEquals("NON, TRD, PNO, TRF, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);

				caCompany.MainAddress.OA_RN_NKCountryCode = "CA";
				AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);

				caCompany.MainAddress.OA_RN_NKCountryCode = "CN";
				AssertEquals("NON, NOP, TRF, TRD, PNO, PNP, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var cnCompany = Factory.New<OrgHeader>();
				var miscserv = cnCompany.MiscServ;

				cnCompany.OH_RL_NKClosestPort = "CNSHA";
				cnCompany.OH_FullName = "TestCompany";
				cnCompany.MainAddress.OA_Address1 = "TestAddress";
				cnCompany.MainAddress.OA_RN_NKCountryCode = "TW";

				AssertEquals("NON, TRD, PNO, TRF, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);

				cnCompany.MainAddress.OA_RN_NKCountryCode = "CA";
				AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);

				cnCompany.MainAddress.OA_RN_NKCountryCode = "CN";
				AssertEquals("NON, NOP, TRF, TRD, PNO, PNP, DEF", miscserv.OM_IMMergeCustomsInvoiceLinesBy_List.CodesAsString);
			}
		}

		public void TestOM_IMSendImportDocsTo_List()
		{
			Assert("OM_IMSendImportDocsTo_List.Count == 3", miscServ.OM_IMSendImportDocsTo_List.Count == 3);
		}

		public void TestOM_IMImporterCategory_List()
		{
			Assert("OM_IMImporterCategory_List.Count > 0", miscServ.OM_IMImporterCategory_List.Count > 0);
		}

		[TestDate(2010, 12, 1)]
		public void TestOM_IMDefaultINCOTerm_List()
		{
			string[] actualCodes = (from code in miscServ.OM_IMDefaultINCOTerm_List.Cast<CodeDescriptionPair>() select code.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Core.Constants.IncoTerms.Incoterms2000.Concat(domesticPaymentTermsCodes), actualCodes);
		}

		[TestDate(2011, 12, 1)]
		public void TestOM_IMDefaultINCOTerm_List2()
		{
			string[] actualCodes = (from code in miscServ.OM_IMDefaultINCOTerm_List.Cast<CodeDescriptionPair>() select code.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(Core.Constants.IncoTerms.Incoterms2010.Concat(domesticPaymentTermsCodes), actualCodes);
		}

		readonly string[] domesticPaymentTermsCodes = new string[]
			{
				Core.Constants.DomesticPaymentTerms.Collect, Core.Constants.DomesticPaymentTerms.CollectThirdParty,
				Core.Constants.DomesticPaymentTerms.CollectCOD, Core.Constants.DomesticPaymentTerms.Prepaid
			};

		public void TestOM_IMPaymentMethod_List()
		{
			Assert("OM_IMPaymentMethod_List.Count > 0", miscServ.OM_IMPaymentMethod_List.Count > 0);
		}

		public void TestOM_IMAutoPopulateOwnerRef_List()
		{
			Assert("OM_IMAutoPopulateOwnerRef_List.Count == 3", miscServ.OM_IMAutoPopulateOwnerRefList.Count == 3);
			Assert("OM_IMAutoPopulateOwnerRef_List should have option Yes", miscServ.OM_IMAutoPopulateOwnerRefList.CodesAsString.Contains("YES"));
			Assert("OM_IMAutoPopulateOwnerRef_List should have option No", miscServ.OM_IMAutoPopulateOwnerRefList.CodesAsString.Contains("NO"));
			Assert("OM_IMAutoPopulateOwnerRef_List should have default option", miscServ.OM_IMAutoPopulateOwnerRefList.CodesAsString.Contains("DEF"));
		}

		#endregion

		#region WhsOrderFulfillmentRule

		public void TestOM_WhsOrderFulfillmentRule()
		{
			AssertEquals(WhsOrderFulfillmentRuleList.Codes.None, miscServ.OM_WhsOrderFulfillmentRule);
			miscServ.OM_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			AssertEquals(WhsOrderFulfillmentRuleList.Codes.All, miscServ.OM_WhsOrderFulfillmentRule);
		}

		#endregion

		#region WhsCheckPartWeightOrDimsOnReceive

		public void TestOM_WhsCheckPartWeightOrDimsOnReceive()
		{
			AssertEquals(ProductReceiveWeightOrDimsCheckTypeList.Codes.DoNotCheckWeightOrDims, miscServ.OM_WhsCheckPartWeightOrDimsOnReceive);
			miscServ.OM_WhsCheckPartWeightOrDimsOnReceive = ProductReceiveWeightOrDimsCheckTypeList.Codes.StockKeepingUnitOnly;
			AssertEquals(ProductReceiveWeightOrDimsCheckTypeList.Codes.StockKeepingUnitOnly, miscServ.OM_WhsCheckPartWeightOrDimsOnReceive);
		}

		public void TestWhsCheckPartWeightOrDimsOnReceive_CheckAttributes()
		{
			var propertyInfo = typeof(OrgMiscServ).GetProperty(nameof(OrgMiscServ.OM_WhsCheckPartWeightOrDimsOnReceive));
			var attributes = propertyInfo.GetCustomAttributes(false);

			AssertEquals(1, attributes.Length);
			AssertEquals("Lookups.ProductReceiveWeightOrDimsCheckTypeList", (attributes[0] as ListAttribute).ListDataSourceMember);
		}

		#endregion

		#region Forwarder

		public void TestOM_FWAgentCategory_List()
		{
			Assert("OM_FWAgentCategory_List.Count > 0", miscServ.OM_FWAgentCategory_List.Count > 0);
		}

		#endregion

		#region Carrier

		public void TestOM_CRCarrierCategory_List()
		{
			Assert("OM_CRCarrierCategory_List.Count > 0", miscServ.OM_CRCarrierCategory_List.Count > 0);
		}

		#endregion

		#region Services

		public void TestOM_SVServicesCategory_List()
		{
			Assert("OM_SVServicesCategory_List.Count > 0", miscServ.OM_SVServicesCategory_List.Count > 0);
		}

		#endregion

		#region Sales

		public void TestOM_CMSalesCategory_List()
		{
			Assert("OM_CMSalesCategory_List.Count > 0", miscServ.OM_CMSalesCategory_List.Count > 0);
		}

		public void TestOM_CMClientSize_List()
		{
			OrgMiscServ orgMiscServ = Factory.New<OrgMiscServ>();
			Assert("OM_CMClientSize_List.Count > 0", orgMiscServ.OM_CMClientSize_List.Count > 0);

			ReadOnlyCodeDescriptionPairList originalList = OrganisationsDataRegistry.Instance.ClientSizeList.Value;
			try
			{
				CodeDescriptionPairList newList = new CodeDescriptionPairList();
				newList.AddPair("TST", "Test");
				OrganisationsDataRegistry.Instance.ClientSizeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newList);

				orgMiscServ = Factory.New<OrgMiscServ>();
				AssertEquals("OM_CMClientSize_List.Count", 1, orgMiscServ.OM_CMClientSize_List.Count);

				newList.AddPair("ZUB", "Test2");
				newList.AddPair("AAA", "Test3");
				OrganisationsDataRegistry.Instance.ClientSizeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newList);

				orgMiscServ = Factory.New<OrgMiscServ>();
				AssertEquals("OM_CMClientSize_List.Count", 3, orgMiscServ.OM_CMClientSize_List.Count);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.ClientSizeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalList);
			}
		}

		public void TestOM_CMCompetitorActivity_List()
		{
			Assert("OM_CMCompetitorActivity_List.Count > 0", miscServ.OM_CMCompetitorActivity_List.Count > 0);
		}

		public void TestOM_CMGrowthOutlook_List()
		{
			Assert("OM_CMGrowthOutlook_List.Count > 0", miscServ.OM_CMGrowthOutlook_List.Count > 0);
		}

		public void TestOM_CMOverallEffectOfClient_List()
		{
			Assert("OM_CMOverallEffectOfClient_List.Count > 0", miscServ.OM_CMOverallEffectOfClient_List.Count > 0);
		}

		public void TestOM_CMClientRanking_List()
		{
			Assert("OM_CMClientRanking_List.Count > 0", miscServ.OM_CMClientRanking_List.Count > 0);
		}

		public void TestOM_CMOverallClientRelation_List()
		{
			Assert("OM_CMOverallClientRelation_List.Count > 0", miscServ.OM_CMOverallClientRelation_List.Count > 0);
		}

		public void TestOM_CMClientsDesireToRemain_List()
		{
			Assert("OM_CMClientsDesireToRemain_List.Count > 0", miscServ.OM_CMClientsDesireToRemain_List.Count > 0);
		}

		public void TestOM_CMEaseClientCanBePoached_List()
		{
			Assert("OM_CMEaseClientCanBePoached_List.Count > 0", miscServ.OM_CMEaseClientCanBePoached_List.Count > 0);
		}

		public void TestOM_CMAmountOfElectronicIntegration_List()
		{
			Assert("OM_CMAmountOfElectronicIntegration_List.Count > 0", miscServ.OM_CMAmountOfElectronicIntegration_List.Count > 0);
		}

		public void TestOM_CMSalesTerritory_List()
		{
			Assert("OM_CMSalesTerritory_List.Count > 0", miscServ.OM_CMSalesTerritory_List.Count > 0);
		}

		public void TestOM_CMIndustryVertical_List()
		{
			Assert("OM_CMIndustryVertical_List.Count > 0", miscServ.OM_CMIndustryVertical_List.Count > 0);
		}

		public void TestOM_CMIndustryVertical_ActiveList()
		{
			Assert("OM_CMIndustryVertical_ActiveList.Count > 0", miscServ.OM_CMIndustryVertical_ActiveList.Count > 0);
		}

		public void TestOM_CMPeriodOfActivity_List()
		{
			Assert("OM_CMPeriodOfActivity_List.Count > 0", miscServ.OM_CMPeriodOfActivity_List.Count > 0);
		}

		public void TestOM_CMPeriodOfActivity_ActiveList()
		{
			Assert("OM_CMPeriodOfActivity_ActiveList.Count > 0", miscServ.OM_CMPeriodOfActivity_ActiveList.Count > 0);
		}

		public void TestWarehouses()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			org2.OH_IsWarehouseClient = true;

			var warehouses = Factory.New<OrgHeader>().MiscServ.Warehouses;
			warehouses.Load();

			Assert("Org1 should not be in list", !warehouses.Contains(org1));
			Assert("Org2 should be in list", warehouses.Contains(org2));
		}

		#endregion

		#region Competitors

		public void TestOM_CITypeOfService_List()
		{
			Assert("OM_CITypeOfService_List.Count == 14", miscServ.OM_CITypeOfService_List.Count == 14);
		}

		public void TestOM_CISellingStyle_List()
		{
			Assert("OM_CISellingStyle_List.Count == 9", miscServ.OM_CISellingStyle_List.Count == 9);
		}

		public void TestOM_CICompetitorCategory_List()
		{
			Assert("OM_CICompetitorCategory_List.Count > 0", miscServ.OM_CICompetitorCategory_List.Count > 0);
		}

		public void TestOM_CICompetitiveRanking_List()
		{
			Assert("OM_CICompetitiveRanking_List.Count > 0", miscServ.OM_CICompetitiveRanking_List.Count > 0);
		}

		#endregion

		#region TestWarehousePickOptionList

		public void TestWarehousePickOptionList()
		{
			AssertEquals(3, miscServ.WarehousePickOptionList.Count);
			AssertEquals(true, miscServ.WarehousePickOptionList.ContainsCode(WhsPickOption.Codes.Auto));
			AssertEquals(true, miscServ.WarehousePickOptionList.ContainsCode(WhsPickOption.Codes.Manual));
			AssertEquals(true, miscServ.WarehousePickOptionList.ContainsCode(WhsPickOption.Codes.ManualWithAutoAllocate));
		}

		#endregion

		#region TestWarehousePickModeList

		public void TestWarehousePickModeList()
		{
			AssertEquals(2, miscServ.WarehousePickModeList.Count);
			AssertEquals(true, miscServ.WarehousePickModeList.ContainsCode(WhsPickMode.Codes.AttributeSpecified));
			AssertEquals(true, miscServ.WarehousePickModeList.ContainsCode(WhsPickMode.Codes.AttributeNeutral));
		}

		#endregion

		#region WhsOrderFulfillmentRuleList

		public void TestOM_WhsOrderFulfillmentRuleList()
		{
			Assert(miscServ.OM_WhsOrderFulfillmentRuleList.Count > 0);
		}

		#endregion

		#region UNDGContact_List

		public void TestUNDGContactList()
		{
			AssertNotNull(miscServ.UNDGContactList);
			AssertEquals(0, miscServ.UNDGContactList.Count);
		}

		#endregion

		#region Warehouse Unique Ref Stratefy List

		public void TestWhsUniqueRefStratefyList()
		{
			AssertNotNull(miscServ.WhsUniqueRefStrategyTypesList);
			AssertEquals(1, miscServ.WhsUniqueRefStrategyTypesList.Count);
			Assert(miscServ.WhsUniqueRefStrategyTypesList.ContainsCode(WhsUniqueRefStrategyTypeList.Codes.ReferecePlusDateTime));
		}

		#endregion

		#region PhoneTypesList

		public void TestPhoneTypesList()
		{
			AssertNotNull(miscServ.PhoneTypesList);
			AssertEquals(4, miscServ.PhoneTypesList.Count);
			Assert(miscServ.PhoneTypesList.ContainsCode(PhoneTypeList.Codes.HOM));
			Assert(miscServ.PhoneTypesList.ContainsCode(PhoneTypeList.Codes.MOB));
			Assert(miscServ.PhoneTypesList.ContainsCode(PhoneTypeList.Codes.OTH));
			Assert(miscServ.PhoneTypesList.ContainsCode(PhoneTypeList.Codes.WRK));
		}

		#endregion

		#region Putaway Algorithms

		public void TestPutawayAlgorithmList()
		{
			AssertEquals(typeof(WhsPutawayAlgorithmList), miscServ.PutawayAlgorithmList.GetType());
		}

		#endregion

		#region Pick Algorithms

		public void TestPickAlgorithmList()
		{
			AssertEquals(typeof(WhsPickAlgorithmList), miscServ.PickAlgorithmList.GetType());
		}

		#endregion

		#region DistanceCalculation

		public void TestOM_CMDistanceCalculationProvider_List()
		{
			Assert("OM_CMDistanceCalculationProvider_List.Count > 0", miscServ.OM_CMDistanceCalculationProvider_List.Count > 0);
			Assert("OM_CMDistanceCalculationProvider_List contains DEFault code", miscServ.OM_CMDistanceCalculationProvider_List.ContainsCode(DistanceCalculationConstants.Providers.DefaultFromRegistry));
			foreach (CodeDescriptionPair pair in DistanceCalculationLists.Instance.Providers)
			{
				Assert("OM_CMDistanceCalculationProvider_List contains all codes from the registry", miscServ.OM_CMDistanceCalculationProvider_List.ContainsCode(pair.Code));
			}
		}

		public void TestOM_CMDistanceCalculationVersion_List()
		{
			AssertNotNull("OM_CMDistanceCalculationVersion_List not null", miscServ.OM_CMDistanceCalculationVersion_List);
		}

		public void TestOM_CMDistanceCalculationMethod_List()
		{
			AssertNotNull("OM_CMDistanceCalculationMethod_List not null", miscServ.OM_CMDistanceCalculationMethod_List);
		}

		#endregion

		#endregion

		#region New Bound Properties

		public void TestAirlineFieldsReadOnlyForNotAirline()
		{
			AssertEquals(true, company.MiscServ.OM_RM_AirlineInfo.ReadOnly);

			company.OH_IsAirLine = true;
			AssertEquals(false, company.MiscServ.OM_RM_AirlineInfo.ReadOnly);

			company.OH_IsAirLine = false;
			AssertEquals(true, company.MiscServ.OM_RM_AirlineInfo.ReadOnly);
		}

		#endregion

		#region Relationships

		public void TestRelationships()
		{
			AssertNotNull("SingleOrgDebtor", miscServ.SingleOrgDebtor);
			AssertEquals("SingleOrgDebtor.ReadOnly", true, miscServ.SingleOrgDebtor.ReadOnly);
		}

		#endregion

		#region Calendar Reminders

		public void TestEstimatedCloseDateCalendarReminder()
		{
			Env.Registry.SetOrgAllowMixedCase(true);

			#region Test Data

			OrgHeader org = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("479520de-ba34-4339-b9ae-03ed90c5c864"));
			GlbStaff someStaff = Factory.NewWithValidTestData<GlbStaff>();
			someStaff.GS_EmailAddress = "test@example.com";
			someStaff.GS_FullName = "Someb ody";

			org.StaffAssignments.OverallSalesRep = someStaff.GS_Code;

			AssertNotNull(org.MiscServ.EstimatedCloseDateReminder.Recipients[0]);
			AssertEquals("Staff Name has been added to Recepient list", someStaff.GS_FullName, org.MiscServ.EstimatedCloseDateReminder.Recipients[0].Name);
			AssertEquals("Staff Name has been added to Recepient list", someStaff.GS_EmailAddress, org.MiscServ.EstimatedCloseDateReminder.Recipients[0].Email);

			org.OH_FullName = "Some Special Organisation";
			org.OH_Code = "SOMEORG1";

			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			string plainTextExpected =
@"This is a reminder that the Estimated Sales Close Date is approaching for the sales lead Some Special Organisation (SOMEORG1)

Client Address:
18 Henricks Avenue
My House
Newington NSW 2127
Office Email: mainorg@example.com
Office Phone: +61 2 9911 1199
";

			string htmlExpected =
@"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>This is a reminder that the Estimated Sales Close Date is approaching for the sales lead Some Special Organisation (<a href=""edient:Command=ShowEditForm&ControllerID=Organisation&BusinessEntityPK=479520de-ba34-4339-b9ae-03ed90c5c864" + @"&VersionNumber=" + new EnterpriseInformationRetriever().VersionNumber + @"&Hash=%2b7mh4ylQc5igNAh6FqQqdX6pro2teONbq"">SOMEORG1</a>)

Client Address:
18 Henricks Avenue
My House
Newington NSW 2127
Office Email: mainorg@example.com
Office Phone: +61 2 9911 1199
</BODY></HTML>";

			#endregion

			AssertEquals("Calendar reminder created with correct body", plainTextExpected, org.MiscServ.EstimatedCloseDateReminder.Body);
			AssertEquals("Calendar reminder created with correct body", htmlExpected, org.MiscServ.EstimatedCloseDateReminder.HtmlBody);
		}

		public void TestEstimatedCloseDateCalendarReminder_CancelAppointment()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@example.com";
			staff.GS_FullName = "sam";
			org.OH_FullName = "Organisation DDD";
			org.OH_Code = "DDDORG1";
			org.StaffAssignments.OverallSalesRep = staff.GS_Code;

			org.MiscServ.OM_CMEstimatedDateToClose = new ZDateTime(2012, 9, 22, 10, 0, 0);
			Factory.Save();
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("DTSTART:20120922T000000Z", email.Body);
			AssertContains("DTEND:20120922T000100Z", email.Body);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			org.MiscServ.OM_CMEstimatedDateToClose = ZDateTime.Empty;
			Factory.Save();
			AssertEquals(ReminderType.Cancellation, org.MiscServ.EstimatedCloseDateReminder.ReminderType);

			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotContains("DTSTART:<Invalid>", email.Body);
			AssertNotContains("DTEND:<Invalid>", email.Body);
			AssertContains("DTSTART:20120922T000000Z", email.Body);
			AssertContains("DTEND:20120922T000100Z", email.Body);
		}

		#endregion

		#region As Agent Name

		public void TestOM_FWAsAgentName_ReadOnly()
		{
			var misc = Factory.New<OrgMiscServ>();
			AssertEquals(true, misc.OM_FWAsAgentNameInfo.ReadOnly);

			misc.OM_FWAsAgentOption = misc.Lookups.AsAgentOptions[0].Code;
			AssertEquals(false, misc.OM_FWAsAgentNameInfo.ReadOnly);

			misc.OM_FWAsAgentOption = string.Empty;
			AssertEquals(true, misc.OM_FWAsAgentNameInfo.ReadOnly);
		}

		#endregion

		#region Sales Calls

		[TestDate(2006, 03, 03)]
		public void TestSalesCalls()
		{
			var org1 = Factory.NewWithValidTestData<OrgMiscServ>().Header;
			var org2 = Factory.NewWithValidTestData<OrgMiscServ>().Header;

			var call1 = org1.SalesCalls.AddNew();
			var call2 = org1.SalesCalls.AddNew();
			var call3 = org1.SalesCalls.AddNew();

			call1.OQ_CallDate = ZDateTime.Empty;
			call2.OQ_CallDate = ZDateTime.Empty;
			call3.OQ_CallDate = ZDateTime.Empty;

			call1.OQ_NextCall = new ZDateTime(2006, 03, 01);
			call2.OQ_NextCall = new ZDateTime(2006, 03, 15);
			call3.OQ_NextCall = new ZDateTime(2006, 04, 18);

			var call4 = org2.SalesCalls.AddNew();
			call4.OQ_CallDate = ZDateTime.Empty;
			call4.OQ_NextCall = new ZDateTime(2006, 3, 4);

			Factory.Save();

			AssertEquals("Follow up date should be earliest communication in the future without an actual date", call2.OQ_NextCallLocal, org1.MiscServ.FollowUpDate);

			call2.OQ_CallDate = new ZDateTime(2006, 02, 28);
			Factory.Save();
			AssertEquals("Follow up date should be earliest communication in the future without an actual date", call3.OQ_NextCallLocal, org1.MiscServ.FollowUpDate);

			call1.OQ_CallDate = new ZDateTime(2006, 02, 13);
			call3.OQ_CallDate = new ZDateTime(2006, 03, 03);
			Factory.Save();
			AssertEquals("All call dates have actual date", ZDateTime.Empty, org1.MiscServ.FollowUpDate);
		}

		public void TestLastCallDateSalesCall()
		{
			var org1 = Factory.NewWithValidTestData<OrgMiscServ>().Header;
			var org2 = Factory.NewWithValidTestData<OrgMiscServ>().Header;

			var call1 = org1.SalesCalls.AddNew();
			var call2 = org1.SalesCalls.AddNew();
			var call3 = org1.SalesCalls.AddNew();

			call1.OQ_CallDate = new ZDateTime(2006, 3, 1);
			call2.OQ_CallDate = new ZDateTime(2006, 3, 15);
			call3.OQ_CallDate = new ZDateTime(2006, 4, 18);

			var call4 = org2.SalesCalls.AddNew();
			call4.OQ_CallDate = ZDateTime.Empty;
			call4.OQ_CallDate = new ZDateTime(2006, 4, 20);

			Factory.Save();

			AssertEquals(call3.OQ_CallDate, org1.MiscServ.GetLastCallDateSalesCall().OQ_CallDate);
			AssertEquals(call4.OQ_CallDate, org2.MiscServ.GetLastCallDateSalesCall().OQ_CallDate);
		}

		#endregion

		#region ReadOnly Security

		public void TestCarrierServiceLevelCollectionReadOnly()
		{
			bool oldCarrierAppointedAgentPortsValue = Env.Security.OrgCarrierModify.IsAllowed;

			try
			{
				Env.Security.OrgCarrierModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.CarrierServiceLevels.ReadOnly);

				Env.Security.OrgCarrierModify.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.MiscServ.CarrierServiceLevels.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierAppointedAgentPortsValue;
			}
		}

		public void TestReadOnlySecurity()
		{
			bool oldCNRDetailsValue = Env.Security.OrgConsignorModifyDetails.IsAllowed;
			bool oldCNEDetailsValue = Env.Security.OrgConsigneeModifyDetails.IsAllowed;
			bool oldCNELandedCostingValue = Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed;
			bool oldWarehouseValue = Env.Security.OrgWarehouseModify.IsAllowed;
			bool oldCarrierValue = Env.Security.OrgCarrierModify.IsAllowed;
			bool oldServicesValue = Env.Security.OrgServicesModify.IsAllowed;
			bool oldForwarderValue = Env.Security.OrgForwarderModifyDetails.IsAllowed;
			bool oldSalesSummaryValue = Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed;
			bool oldSalesRelationshipValue = Env.Security.ClientIntelligenceModifyClientRelationship.IsAllowed;
			bool oldConfigGeneralValue = Env.Security.OrgConfigModifyGeneral.IsAllowed;
			bool oldPayablesCreditorDetails = Env.Security.OrgPayablesCreditorDetailsModify.IsAllowed;
			bool oldGlobalCreditValue = Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed;

			try
			{
				OrgInDB.OH_IsAirLine = ZBool.True;

				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsScanPackQtyAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsLabelPrintedOnClosePackageInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsAutoPackAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXExporterCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultIncoTermInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXJobRequireOrderTrackLinkInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAutoImpJobReferedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMEftHoldUntilPayAuthorisedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent1Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent2Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent3Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib1IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib2IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib3IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMInvoiceDetailReportSortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAirDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMSeaDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMSalesCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMTotalClientRevenueInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMAmountOfBusinessWonInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMDoesImportsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RH_NKCMMainExportCmdtyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientCommencedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnAirfreightCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnLCLCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnOtherCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnTEUCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnWarehousingCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallClientRelationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientsDesireToRemainInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMEaseClientCanBePoachedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMAmountOfElectronicIntegrationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallClientRelationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientsDesireToRemainInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMEaseClientCanBePoachedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMAmountOfElectronicIntegrationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CICapitalEmployedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CICompetitiveRankingInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CICompetitorCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CIEstimatedStaffThisCountryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CIEstimatedStaffThisLocationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CIOpportunitiesInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CIProfitInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CISellingStyleInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CIStrengthInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CIThreatsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CITurnoverInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CITypeOfServiceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CIWeaknessesInfo.ReadOnly);

				Env.Security.CompetitorIntelligenceModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CICapitalEmployedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CICompetitiveRankingInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CICompetitorCategoryInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CIEstimatedStaffThisCountryInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CIEstimatedStaffThisLocationInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.FollowUpDateInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CIOpportunitiesInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CIProfitInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CISellingStyleInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CIStrengthInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CIThreatsInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CITurnoverInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CITypeOfServiceInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CIWeaknessesInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsScanPackQtyAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsLabelPrintedOnClosePackageInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsAutoPackAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXExporterCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultIncoTermInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXJobRequireOrderTrackLinkInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAutoImpJobReferedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMEftHoldUntilPayAuthorisedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent1Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent2Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent3Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib1IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib2IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib3IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMInvoiceDetailReportSortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAirDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMSeaDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMSalesCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMTotalClientRevenueInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMAmountOfBusinessWonInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMDoesImportsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RH_NKCMMainExportCmdtyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientCommencedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnAirfreightCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnLCLCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnOtherCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnTEUCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnWarehousingCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallClientRelationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientsDesireToRemainInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMEaseClientCanBePoachedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMAmountOfElectronicIntegrationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallClientRelationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientsDesireToRemainInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMEaseClientCanBePoachedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMAmountOfElectronicIntegrationInfo.ReadOnly);

				Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMSalesCategoryInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMTotalClientRevenueInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMAmountOfBusinessWonInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.FollowUpDateInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMDoesImportsInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_RH_NKCMMainExportCmdtyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsScanPackQtyAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsLabelPrintedOnClosePackageInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsAutoPackAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXExporterCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultIncoTermInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXJobRequireOrderTrackLinkInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAutoImpJobReferedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMEftHoldUntilPayAuthorisedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent1Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent2Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent3Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib1IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib2IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib3IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMInvoiceDetailReportSortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAirDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMSeaDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientCommencedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnAirfreightCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnLCLCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnOtherCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnTEUCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnWarehousingCostsInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallClientRelationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientsDesireToRemainInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMEaseClientCanBePoachedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMAmountOfElectronicIntegrationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMOverallClientRelationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientsDesireToRemainInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMEaseClientCanBePoachedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMAmountOfElectronicIntegrationInfo.ReadOnly);

				Env.Security.ClientIntelligenceModifyClientRelationship.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMClientCommencedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnAirfreightCostsInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnLCLCostsInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnOtherCostsInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnTEUCostsInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMOverallEffectOfClientOnWarehousingCostsInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMOverallClientRelationInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMClientsDesireToRemainInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMEaseClientCanBePoachedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMAmountOfElectronicIntegrationInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMOverallClientRelationInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMClientsDesireToRemainInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMEaseClientCanBePoachedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CMAmountOfElectronicIntegrationInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsScanPackQtyAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsLabelPrintedOnClosePackageInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsAutoPackAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXExporterCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultIncoTermInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXJobRequireOrderTrackLinkInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAutoImpJobReferedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMEftHoldUntilPayAuthorisedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent1Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent2Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent3Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib1IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib2IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib3IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMInvoiceDetailReportSortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAirDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMSeaDepotFreeDaysInfo.ReadOnly);

				Env.Security.OrgConsignorModifyDetails.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_EXExporterCategoryInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_EXDefaultIncoTermInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_EXJobRequireOrderTrackLinkInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsScanPackQtyAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsLabelPrintedOnClosePackageInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IsAutoPackAllowedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAutoImpJobReferedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMEftHoldUntilPayAuthorisedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent1Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent2Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent3Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib1IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib2IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib3IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMInvoiceDetailReportSortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAirDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMSeaDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_MinimumShelfLifeAcceptedInfo.ReadOnly);

				Env.Security.OrgConsigneeModifyDetails.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IsScanPackQtyAllowedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IsLabelPrintedOnClosePackageInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IsAutoPackAllowedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_RS_NKIMDefaultServiceLevelInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMAutoImpJobReferedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMEftHoldUntilPayAuthorisedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_MinimumShelfLifeAcceptedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent1Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent2Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent3Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib1IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib2IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAttrib3IsKeyInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMInvoiceDetailReportSortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_EXDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMAirDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMSeaDepotFreeDaysInfo.ReadOnly);

				Env.Security.OrgForwarderModifyDetails.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_EXDocumentAddressPreferenceInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMAirDepotFreeDaysInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMSeaDepotFreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent1Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent2Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_LandedCostMarginPercent3Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);

				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_LandedCostMarginPercent1Info.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_LandedCostMarginPercent2Info.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_LandedCostMarginPercent3Info.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);

				Env.Security.OrgWarehouseModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_OC_EXDefaultDGContactInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMPartAttrib1NameInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMPartAttrib1TypeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMPartAttrib2NameInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMPartAttrib2TypeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMPartAttrib3NameInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMPartAttrib3TypeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMUseExpiryDateInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMUsePackingDateInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMDefaultWarehousePickOptionInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_WhsDefaultWarehousePickModeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMAttrib1IsKeyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMAttrib2IsKeyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMAttrib3IsKeyInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_IMInvoiceDetailReportSortInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);

				Env.Security.OrgCarrierModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_CRCarrierCategoryInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);

				Env.Security.OrgServicesModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", OrgInDB.MiscServ.OM_SVServicesCategoryInfo.ReadOnly);

				Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed = false;

				Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed = true;
			}
			finally
			{
				Env.Security.OrgConsignorModifyDetails.IsAllowed = oldCNRDetailsValue;
				Env.Security.OrgConsigneeModifyDetails.IsAllowed = oldCNEDetailsValue;
				Env.Security.OrgConsigneeModifyLandedCosting.IsAllowed = oldCNELandedCostingValue;
				Env.Security.OrgWarehouseModify.IsAllowed = oldWarehouseValue;
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierValue;
				Env.Security.OrgServicesModify.IsAllowed = oldServicesValue;
				Env.Security.OrgForwarderModifyDetails.IsAllowed = oldForwarderValue;
				Env.Security.ClientIntelligenceModifyClientSummary.IsAllowed = oldSalesSummaryValue;
				Env.Security.OrgConfigModifyGeneral.IsAllowed = oldConfigGeneralValue;
				Env.Security.ClientIntelligenceModifyClientRelationship.IsAllowed = oldSalesRelationshipValue;
				Env.Security.OrgPayablesCreditorDetailsModify.IsAllowed = oldPayablesCreditorDetails;
				Env.Security.OrgReceivablesGlobalCreditControl.IsAllowed = oldGlobalCreditValue;
			}
		}

		public void TestOM_APConsolidatedAccountingCategory_ReadOnly_NotNewItem()
		{
			ResetOrgInDB();
			TestOM_APConsolidatedAccountingCategory_ReadOnly_Senario
			(
				fOrgInDB.MiscServ
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_APConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access NOT Allowed - ReadOnly", orgMiscServ.OM_APConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access NOT Allowed - ReadOnly", orgMiscServ.OM_APConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access NOT Allowed - ReadOnly", orgMiscServ.OM_APConsolidatedAccountingCategory_ReadOnly)
			);
		}

		public void TestOM_APConsolidatedAccountingCategory_ReadOnly_NewItem()
		{
			TestOM_APConsolidatedAccountingCategory_ReadOnly_Senario
			(
				(OrgMiscServ)GetNewBusinessObject()
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_APConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_APConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_APConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_APConsolidatedAccountingCategory_ReadOnly)
			);
		}

		void TestOM_APConsolidatedAccountingCategory_ReadOnly_Senario(
			OrgMiscServ orgMiscServ
			, Action<OrgMiscServ> senarioFullSecurity
			, Action<OrgMiscServ> senarioLackSecurityOrgPayablesModify
			, Action<OrgMiscServ> senarioLackSecurityOrgModifyConsolidationCategory
			, Action<OrgMiscServ> senarioEmptySecurity)
		{
			Env.Security.OrgPayablesModify.IsAllowed = true;
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = true;
			senarioFullSecurity(orgMiscServ);

			Env.Security.OrgPayablesModify.IsAllowed = false;
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = true;
			senarioLackSecurityOrgPayablesModify(orgMiscServ);

			Env.Security.OrgPayablesModify.IsAllowed = true;
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = false;
			senarioLackSecurityOrgModifyConsolidationCategory(orgMiscServ);

			Env.Security.OrgPayablesModify.IsAllowed = false;
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = false;
			senarioEmptySecurity(orgMiscServ);
		}

		public void TestOM_ARConsolidatedAccountingCategory_ReadOnly_NotNewItem()
		{
			ResetOrgInDB();
			TestOM_ARConsolidatedAccountingCategory_ReadOnly_Senario(
				fOrgInDB.MiscServ
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_ARConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access NOT Allowed - ReadOnly", orgMiscServ.OM_ARConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access NOT Allowed - ReadOnly", orgMiscServ.OM_ARConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access NOT Allowed - ReadOnly", orgMiscServ.OM_ARConsolidatedAccountingCategory_ReadOnly)
			);
		}

		public void TestOM_ARConsolidatedAccountingCategory_ReadOnly_NewItem()
		{
			TestOM_ARConsolidatedAccountingCategory_ReadOnly_Senario(
				(OrgMiscServ)GetNewBusinessObject()
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_ARConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_ARConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_ARConsolidatedAccountingCategory_ReadOnly)
				, orgMiscServ => Assert("Access Allowed - NOT ReadOnly", !orgMiscServ.OM_ARConsolidatedAccountingCategory_ReadOnly)
			);
		}

		void TestOM_ARConsolidatedAccountingCategory_ReadOnly_Senario(
			OrgMiscServ orgMiscServ
			, Action<OrgMiscServ> senarioFullSecurity
			, Action<OrgMiscServ> senarioLackOrgReceivablesModify
			, Action<OrgMiscServ> senarioLackOrgModifyConsolidationCategory
			, Action<OrgMiscServ> senarioEmptySecurity)
		{
			Env.Security.OrgReceivablesModify.IsAllowed = true;
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = true;
			senarioFullSecurity(orgMiscServ);

			Env.Security.OrgReceivablesModify.IsAllowed = false;
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = true;
			senarioLackOrgReceivablesModify(orgMiscServ);

			Env.Security.OrgReceivablesModify.IsAllowed = true;
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = false;
			senarioLackOrgModifyConsolidationCategory(orgMiscServ);

			Env.Security.OrgReceivablesModify.IsAllowed = false;
			Env.Security.OrgModifyConsolidationCategory.IsAllowed = false;
			senarioEmptySecurity(orgMiscServ);
		}

		public void TestModifyRatingAndTariffsSecurity()
		{
			bool oldOrgDetailsModify = Env.Security.OrgDetailsModify.IsAllowed;
			bool oldOrgDetailsModifyRatingAndTariffs = Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModify.IsAllowed = false;

				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMDistanceCalculationProviderInfo.ReadOnly);
				OrgInDB.MiscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.PCMiler;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMDistanceCalculationMethodInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMDistanceCalculationVersionInfo.ReadOnly);

				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.MiscServ.OM_CMDistanceCalculationProviderInfo.ReadOnly);
				OrgInDB.MiscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.PCMiler;
				Assert("Access Disallowed - ReadOnly", OrgInDB.MiscServ.OM_CMDistanceCalculationMethodInfo.ReadOnly);
				Assert("Access Disallowed - ReadOnly", OrgInDB.MiscServ.OM_CMDistanceCalculationVersionInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModify.IsAllowed = oldOrgDetailsModify;
				Env.Security.OrgDetailsModifyRatingAndTariffs.IsAllowed = oldOrgDetailsModifyRatingAndTariffs;
			}
		}

		public void TestModifyWebSecurity()
		{
			bool oldOrgDetailsModify = Env.Security.OrgDetailsModify.IsAllowed;
			bool oldOrgDetailsModifyWeb = Env.Security.OrgDetailsModifyWebSecurity.IsAllowed;
			bool oldOrgDetailsNewWeb = Env.Security.OrgDetailsNewWebSecurity.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModify.IsAllowed = false;

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientPortalHomePageInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !Factory.NewWithValidTestData<OrgHeader>().MiscServ.OM_CMClientPortalHomePageInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.MiscServ.OM_CMClientPortalHomePageInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !Factory.NewWithValidTestData<OrgHeader>().MiscServ.OM_CMClientPortalHomePageInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = true;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
				Assert("Access Allowed - Not ReadOnly", !OrgInDB.MiscServ.OM_CMClientPortalHomePageInfo.ReadOnly);
				Assert("Access Disallowed - ReadOnly", Factory.NewWithValidTestData<OrgHeader>().MiscServ.OM_CMClientPortalHomePageInfo.ReadOnly);

				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = false;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;
				ResetOrgInDB();
				Assert("Access Disallowed - ReadOnly", OrgInDB.MiscServ.OM_CMClientPortalHomePageInfo.ReadOnly);
				Assert("Access Disallowed - ReadOnly", Factory.NewWithValidTestData<OrgHeader>().MiscServ.OM_CMClientPortalHomePageInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgDetailsModify.IsAllowed = oldOrgDetailsModify;
				Env.Security.OrgDetailsModifyWebSecurity.IsAllowed = oldOrgDetailsModifyWeb;
				Env.Security.OrgDetailsNewWebSecurity.IsAllowed = oldOrgDetailsNewWeb;
			}
		}

		public void TestChangingAirCarrierFlag()
		{
			bool oldFlagValue = Env.Security.OrgCarrierModifyAir.IsAllowed;
			OrgInDB.OH_IsAirLine = false;
			OrgInDB.OH_IsAirWholesaler = false;

			try
			{
				Env.Security.OrgCarrierModifyAir.IsAllowed = false;
				OrgInDB.OH_IsAirLine = true;
				AssertEquals("OrgAirlineMAWBStockManagementCollection should be read only", ZBool.True, OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);

				Env.Security.OrgCarrierModifyAir.IsAllowed = true;
				OrgInDB.OH_IsAirWholesaler = true;
				AssertEquals("OrgAirlineMAWBStockManagementCollection should not be read only", ZBool.False, OrgInDB.MiscServ.OM_RM_AirlineInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModifyAir.IsAllowed = oldFlagValue;
			}
		}

		public void TestChangingSeaCarrierFlag()
		{
			bool oldFlagValue = Env.Security.OrgCarrierModifySea.IsAllowed;
			OrgInDB.OH_IsShippingLine = false;
			OrgInDB.OH_IsSeaWholesaler = false;

			try
			{
				Env.Security.OrgCarrierModifySea.IsAllowed = false;
				OrgInDB.OH_IsShippingLine = true;
				AssertEquals("VoyageRecyclingPeriodCode should be read only", ZBool.True, OrgInDB.MiscServ.VoyageRecyclingPeriodCodeInfo.ReadOnly);

				Env.Security.OrgCarrierModifySea.IsAllowed = true;
				OrgInDB.OH_IsSeaWholesaler = true;
				AssertEquals("VoyageRecyclingPeriodCode should not be read only", ZBool.False, OrgInDB.MiscServ.VoyageRecyclingPeriodCodeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModifySea.IsAllowed = oldFlagValue;
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

		#region TestDBHits

		public void TestDBHitsForCommunicationDates()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgMiscServ = orgHeader.MiscServ;
			Factory.ResetDatabaseLoadCount();
			orgMiscServ.GetFollowUpDateSalesCall();
			orgMiscServ.GetLastUnactionedCallDateSalesCall();
			AssertDbHits(new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 0 },
				{ OrgSalesCallSchema.Constants.TableName, 2 }
			}, Factory);
		}

		#endregion

		#region Test ReadOnly Properties

		public void TestOU_WhsDefaultWarehouseRollUp_ReadOnly()
		{
			var org = Factory.New<OrgHeader>();
			AssertEquals(true, org.MiscServ.OM_WhsDefaultWarehouseRollUpInfo.ReadOnly);

			org.MiscServ.OM_WhsDefaultWarehousePickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals(false, org.MiscServ.OM_WhsDefaultWarehouseRollUpInfo.ReadOnly);
		}

		public void TestOM_GG_OrgSecurityGroup_ReadOnly()
		{
			Env.Security.GroupsManageOSMG.IsAllowed = true;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(false, org.MiscServ.OM_GG_OrgSecurityGroupInfo.ReadOnly);

			Env.Security.GroupsManageOSMG.IsAllowed = false;
			AssertEquals(true, org.MiscServ.OM_GG_OrgSecurityGroupInfo.ReadOnly);
		}

		public void TestOM_ARGlobalCreditLimit_ReadOnly()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();

			AssertEquals(false, miscServ.OM_ARGlobalCreditLimitInfo.ReadOnly);

			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertEquals(true, miscServ.OM_ARGlobalCreditLimitInfo.ReadOnly);
		}

		public void TestOM_RX_NKARGlobalCreditCurrency_ReadOnly()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();

			AssertEquals(false, miscServ.OM_RX_NKARGlobalCreditCurrencyInfo.ReadOnly);

			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertEquals(true, miscServ.OM_RX_NKARGlobalCreditCurrencyInfo.ReadOnly);
		}

		public void TestOM_ARGlobalCreditApproved_ReadOnly()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();

			AssertEquals(false, miscServ.OM_ARGlobalCreditApprovedInfo.ReadOnly);

			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertEquals(true, miscServ.OM_ARGlobalCreditApprovedInfo.ReadOnly);
		}

		public void TestOM_ARGlobalOnCreditHold_ReadOnly()
		{
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();

			AssertEquals(false, miscServ.OM_ARGlobalOnCreditHoldInfo.ReadOnly);

			miscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertEquals(true, miscServ.OM_ARGlobalOnCreditHoldInfo.ReadOnly);
		}

		#endregion

		#region IUniqueIndexFailureHandler

		public void TestSaveWithUniqueIndexViolation_ShouldDiscardCurrentDataAndPromptReSaveInfo()
		{
			var secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AAABBB";
			Factory.Save();

			var orgMiscServ = Factory.New<OrgMiscServ>();
			AssertNotNull(orgMiscServ);
			AssertEquals(false, orgMiscServ.IsInDatabase);
			orgMiscServ.OM_OH = org.PK;
			org.MiscServ = orgMiscServ;
			orgMiscServ.Header = org;

			var loadedOrg = secondFactory.Load<OrgHeader>(org.PK);
			var loadedOrgMiscServ = loadedOrg.MiscServ;
			AssertNotNull(loadedOrgMiscServ);
			AssertEquals(false, loadedOrgMiscServ.IsInDatabase);
			AssertNotEquals(orgMiscServ.PK, loadedOrgMiscServ.PK);
			secondFactory.Save();

			try
			{
				Factory.Save();
				Assert("Save should not have succeeded.", false);
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);

				AssertEquals("User should have been notified of the problem.", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("User should have been notified of the problem.", "While you were working, a constraint has been detected and corrected, please attempt to save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should repalce to the data in database", orgMiscServ.Header.MiscServ.PK, loadedOrgMiscServ.PK);
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		#endregion

		#region Save

		public void TestSaveWhenHeaderIsNull()
		{
			try
			{
				var miscServ = Factory.New<OrgMiscServ>();
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				AssertContains("Error Saving Record", ex.Message);
			}
		}

		#endregion

		#region GetReadOnlySecurity

		public void TestGetReadOnlySecurityWhenHeaderIsNull()
		{
			var orgMisc = Factory.New<OrgMiscServ>();

			AssertNull(orgMisc.Header);
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(false, orgMisc.OM_RN_NKEXDefaultCntryOfOriginInfo.ReadOnly);
			});
		}

		public void TestGetReadOnlySecurityWhenHeaderIsNotNull()
		{
			var orgMisc = Factory.NewWithValidTestData<OrgMiscServ>();
			Factory.Save();

			AssertNotNull(orgMisc.Header);

			var rawValue = Env.Security.OrgConsignorModifyDetails.IsAllowed;
			try
			{
				Env.Security.OrgConsignorModifyDetails.IsAllowed = false;
				AssertEquals(true, orgMisc.OM_RN_NKEXDefaultCntryOfOriginInfo.ReadOnly);

				Env.Security.OrgConsignorModifyDetails.IsAllowed = true;
				AssertEquals(false, orgMisc.OM_RN_NKEXDefaultCntryOfOriginInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConsignorModifyDetails.IsAllowed = rawValue;
			}
		}

		public void TestGetReadOnlySecurity_OM_IMSerialNumberIsKey()
		{
			var orgMisc = Factory.NewWithValidTestData<OrgMiscServ>();
			Factory.Save();

			var rawValue = Env.Security.OrgWarehouseModify.IsAllowed;
			try
			{
				Env.Security.OrgWarehouseModify.IsAllowed = false;
				AssertEquals(true, orgMisc.OM_IMSerialNumberIsKeyInfo.ReadOnly);

				Env.Security.OrgWarehouseModify.IsAllowed = true;
				AssertEquals(false, orgMisc.OM_IMSerialNumberIsKeyInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgWarehouseModify.IsAllowed = rawValue;
			}
		}

		public void TestGetReadOnlySecurity_OM_IMUseSerialNumber()
		{
			var orgMisc = Factory.NewWithValidTestData<OrgMiscServ>();
			Factory.Save();

			var rawValue = Env.Security.OrgWarehouseModify.IsAllowed;
			try
			{
				Env.Security.OrgWarehouseModify.IsAllowed = false;
				AssertEquals(true, orgMisc.OM_IMUseSerialNumberInfo.ReadOnly);

				Env.Security.OrgWarehouseModify.IsAllowed = true;
				AssertEquals(false, orgMisc.OM_IMUseSerialNumberInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgWarehouseModify.IsAllowed = rawValue;
			}
		}

		#endregion

		#region Implementation

		OrgHeader company;
		OrgMiscServ miscServ;
		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			company = Factory.New<OrgHeader>();
			miscServ = company.MiscServ;

			company.OH_RL_NKClosestPort = "USLAX";
			company.OH_FullName = "TestCompany";
			company.MainAddress.OA_Address1 = "TestAddress";
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}

		void NewCompanyAndBranch(string countryCode, string code)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_Code = code;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = code;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<OrgHeader>().MiscServ;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<OrgHeader>().MiscServ;
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			return ((OrgMiscServ)bO).Header;
		}

		// Find and return the hidden note business object
		HiddenStmNote GetClientDocumentLogoNote(OrgMiscServ noteHolder)
		{
			ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, "ClientDocumentLogo");
			filter.AddToFilter(StmNoteSchema.ST_ParentID, noteHolder.PK);
			filter.AddToFilter(StmNoteSchema.ST_Table, noteHolder.TableName);
			return noteHolder.Factory.LoadTop1<HiddenStmNote>(filter);
		}

		// Create a new ZBlob with valid image data
		ZBlob BitmapToZBlob(Bitmap image)
		{
			var ms = new System.IO.MemoryStream();
			image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

			return new ZBlob(ms.ToArray());
		}

		#endregion
	}
}
