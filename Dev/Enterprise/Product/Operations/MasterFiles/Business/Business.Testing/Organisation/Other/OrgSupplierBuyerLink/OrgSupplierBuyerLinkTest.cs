using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierBuyerLink))]
	public class OrgSupplierBuyerLinkTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<OrgSupplierBuyerLink>();
		}

		public void TestSupplierCorrectSpelling()
		{
			var buyer = Factory.New<OrgHeader>();
			var supplier = Factory.New<OrgHeader>();
			var link = Factory.New<OrgSupplierBuyerLink>();

			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;

			var initialShipmentExpectedReminder = link.InitialShipmentExpectedReminder;

			string expectedSuject = "Initial shipment expected for Buyer  / Supplier ";
			string expectedBody = @"Initial shipment expected for Buyer  / Supplier 

Buyer Contact Details

Contact: The Import Manager
Email: 
Fax: 
Phone: 

Supplier Contact Details

Contact: The Export Manager
Email: 
Fax: 
Phone: 

";

			AssertEquals("suject in Supplier", expectedSuject, initialShipmentExpectedReminder.Subject);
			AssertEquals("body in Supplier", expectedBody, initialShipmentExpectedReminder.Body);
		}

		#region TestDefaultValues

		public void TestDefaultValues()
		{
			OrgSupplierBuyerLink link = (OrgSupplierBuyerLink)GetNewBusinessObject();

			AssertEquals("OL_RX_NKDefaultCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, link.OL_RX_NKDefaultCurrency);
			AssertEquals("OL_SendImportDocsTo", "", link.OL_SendImportDocsTo);
			AssertEquals("UpdateShipmentDate", false, link.UpdateShipmentDate);
			AssertEquals("OL_RN_NKImporterCountry", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, link.OL_RN_NKImporterCountry);
			AssertEquals("should create 1 OrgSupBuyLinkTrnModes", 1, link.OrgSupBuyLinkTrnModes.Count);
		}

		#endregion

		#region Collections and Lists

		public void TestBuyerNotifyParties()
		{
			var contact1 = Link.Buyer.Contacts.AddNew();
			contact1.OC_IsActive = true;
			var contact2 = Link.Buyer.Contacts.AddNew();
			contact2.OC_IsActive = true;
			AssertEquals(2, Link.BuyerNotifyParties.Count);
			AssertEquals(true, Link.BuyerNotifyParties.Contains(contact1));
			AssertEquals(true, Link.BuyerNotifyParties.Contains(contact2));
			Link.Buyer.ContactsActive.Remove(contact2);
			AssertEquals(1, Link.BuyerNotifyParties.Count);
			AssertEquals(false, Link.BuyerNotifyParties.Contains(contact2));

			var newBuyer = Factory.New<OrgHeader>();
			Link.OL_OH_Buyer = newBuyer.PK;
			var newBuyerContact1 = newBuyer.Contacts.AddNew();
			newBuyerContact1.OC_IsActive = false;
			var newBuyerContact2 = newBuyer.Contacts.AddNew();
			newBuyerContact2.OC_IsActive = true;
			AssertEquals("There should only be active contacts", 1, Link.BuyerNotifyParties.Count);
			AssertEquals(false, Link.BuyerNotifyParties.Contains(contact1));
			AssertEquals(false, Link.BuyerNotifyParties.Contains(contact2));
			AssertEquals(true, Link.BuyerNotifyParties.Contains(newBuyerContact2));

			Link.OL_OH_Buyer = ZGuid.Empty;
			AssertEquals(0, Link.BuyerNotifyParties.Count);
		}

		public void TestSupplierNotifyParties()
		{
			OrgContact contact1 = Link.Supplier.Contacts.AddNew();
			AssertEquals(true, Link.SupplierNotifyParties.Contains(contact1));
			OrgContact contact2 = Link.Supplier.Contacts.AddNew();
			AssertEquals(true, Link.SupplierNotifyParties.Contains(contact2));
			Link.Supplier.Contacts.Remove(contact2);
			AssertEquals(false, Link.SupplierNotifyParties.Contains(contact2));

			OrgHeader newSupplier = Factory.New<OrgHeader>();
			Link.OL_OH_Supplier = newSupplier.PK;
			OrgContact newSupplierContact1 = newSupplier.Contacts.AddNew();
			AssertEquals(false, Link.SupplierNotifyParties.Contains(contact1));
			AssertEquals(false, Link.SupplierNotifyParties.Contains(contact2));
			AssertEquals(true, Link.SupplierNotifyParties.Contains(newSupplierContact1));

			Link.OL_OH_Supplier = ZGuid.Empty;
			Link.SupplierNotifyParties.Load();
			AssertEquals(0, Link.SupplierNotifyParties.Count);
		}

		public void TestBuyerCollection()
		{
			Link.Buyers.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_FullName, OrgNameForLoading));
			Assert("Buyers.Count > 0", Link.Buyers.Count > 0);
			bool result = true;
			foreach (OrgHeader org in Link.Buyers)
			{
				if (!org.OH_IsConsignee)
				{
					result = false;
					break;
				}
			}
			Assert("All organisations in Buyer collection are Consignee", result);
		}

		public void TestSupplierCollection()
		{
			Link.Suppliers.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_FullName, OrgNameForLoading));
			Assert("Suppliers.Count > 0", Link.Suppliers.Count > 0);
			bool result = true;
			foreach (OrgHeader org in Link.Suppliers)
			{
				if (!org.OH_IsConsignor)
				{
					result = false;
					break;
				}
			}
			Assert("All organisations in Buyer collection are Consignee", result);
		}

		#endregion

		#region Properties

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			var emptyLink = Factory.New<OrgSupplierBuyerLink>();
			AssertEquals("Supplier/Buyer Relationship", emptyLink.HumanReadableName);

			Link.Buyer.OH_Code = "Buyer";
			Link.Supplier.OH_Code = "Supplier";
			AssertEquals("Supplier/Buyer Relationship (Supplier - Buyer)", Link.HumanReadableName);
		}

		#endregion

		#region Test_OL_AuthorityToLeave

		public void Test_OL_AuthorityToLeave_Log()
		{
			AssertEquals("DEF", Link.OL_AuthorityToLeave);
			var excludeLogs = new List<StmALog>();
			var logs = Link.Logs;
			Factory.Save();
			AssertEquals("Should be no logs, as DEF is default.", false, logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.Authorised.Code));

			Link.OL_AuthorityToLeave = "YES";
			Factory.Save();
			var latestLog = FindLatestWithType(logs, Events.AuthorisedCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("YES", Link.OL_AuthorityToLeave);
			AssertEquals(Events.AuthorisedCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			Link.OL_AuthorityToLeave = "DEF";
			Factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			excludeLogs.Add(latestLog);
			AssertEquals("DEF", Link.OL_AuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawnCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "YES", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);

			Link.OL_AuthorityToLeave = "NO";
			Factory.Save();
			latestLog = FindLatestWithType(logs, Events.AuthorisationWithdrawnCode, excludeLogs.ToArray());
			AssertEquals("NO", Link.OL_AuthorityToLeave);
			AssertEquals(Events.AuthorisationWithdrawnCode, latestLog.Event.SE_Code);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			AssertEquals("Event Reference Parameter OLD", "DEF", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old]);
			AssertEquals("Event Reference Parameter NEW", "NO", latestLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New]);
		}

		StmALog FindLatestWithType(Logs logs, ZString eventType, params StmALog[] excludeLogs)
		{
			return logs.GetAllLogs().Cast<StmALog>().Where(l => !excludeLogs.Contains(l)).First(l => l.SL_SE_NKEvent == eventType);
		}

		#endregion

		public void TestOL_ValuationBasis()
		{
			Link.OL_ValuationBasis = "aaa";
			AssertEquals("OL_ValuationBasis should always be upper case", "AAA", Link.OL_ValuationBasis);
		}

		public void TestValuationBasisAndRelatedPartyForCA()
		{
			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Canada;
			Assert("OL_RelatedParty should be readonly for CA", Link.OL_RelatedPartyInfo.ReadOnly);
			Link.OL_ValuationBasis = "12";
			AssertEquals("OL_RelatedParty should be set to N when OL_ValuationBasis starts with 1 in CA", "N", Link.OL_RelatedParty);
			Link.OL_ValuationBasis = "23";
			AssertEquals("OL_RelatedParty should be set to Y when OL_ValuationBasis starts with 2 in CA", "Y", Link.OL_RelatedParty);
			Link.OL_ValuationBasis = "34";
			AssertEquals("OL_RelatedParty should be blank when OL_ValuationBasis starts with neither 1 nor 2 in CA", "", Link.OL_RelatedParty);
			Link.OL_ValuationBasis = "";
			AssertEquals("OL_RelatedParty should be blank when OL_ValuationBasis starts with neither 1 nor 2 in CA", "", Link.OL_RelatedParty);

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			Assert("OL_RelatedParty should not be readonly for other countries", !Link.OL_RelatedPartyInfo.ReadOnly);
			Link.OL_ValuationBasis = "12";
			AssertEquals("OL_RelatedParty should not be defaulted for other countries", "", Link.OL_RelatedParty);
		}

		public void TestImporterCountryHasValuationBasisList()
		{
			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			AssertEquals("ImporterCountryHasValuationBasisList", true, Link.ImporterCountryHasValuationBasisList);

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Afghanistan;
			AssertEquals("ImporterCountryHasValuationBasisList", false, Link.ImporterCountryHasValuationBasisList);
		}

		public void TestImporterCountryAndAddInfo()
		{
			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			Link.OL_AddInfo = "TEST ADDINFO";

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("Add Info should be empty when Importer Country has changed", "TEST ADDINFO", Link.OL_AddInfo);
			Link.OL_AddInfo = "<AddInfo><FirstSale>Y</FirstSale><NAFTAReconIndicator>Y</NAFTAReconIndicator><OtherReconIndicator>V9</OtherReconIndicator></AddInfo>";

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Afghanistan;
			Link.Factory.Save();
			AssertEquals("should not lost any data", Link.OL_AddInfo, "<AddInfo><FirstSale>Y</FirstSale><NAFTAReconIndicator>Y</NAFTAReconIndicator><OtherReconIndicator>V9</OtherReconIndicator></AddInfo>");

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates;
		}

		[ExpectNoExceptions]
		public void TestRemovingLinkWithNewUnsavedPackPivotDoesntThrowExceptionOnSave()
		{
			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			var packType = Factory.NewWithValidTestData<RefPackType>();
			var packPivot = Factory.New<OrgBuyerSupplierLinkPackPivot>();
			packPivot.Q0_OL = Link.PK;
			packPivot.Q0_F3 = packType.PK;
			Link.PackPivots.Add(packPivot);
			Link.Delete();

			Factory.Save();
		}

		public void TestUniqueIndexFailureHandler()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "ORG1";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "ORG2";

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();
			var buyerInAnotherFactory = factory1.Load<OrgHeader>(buyer.PK);
			var supplierInAnotherFactory = factory2.Load<OrgHeader>(supplier.PK);

			var link1 = buyerInAnotherFactory.SupplierLinks.AddNew();
			link1.OL_RN_NKImporterCountry = "AU";
			link1.OL_OH_Supplier = supplier.PK;

			var link2 = supplierInAnotherFactory.BuyerLinks.AddNew();
			link2.OL_RN_NKImporterCountry = "AU";
			link2.OL_OH_Buyer = buyer.PK;

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("Should have caused save exception due to unique index violation");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertEquals("The value of Supplier Name + Buyer Organization + Importer/Buyer Country must be unique on Supplier/Buyer Relationship. The duplicate values are: (Buyer: ORG1, Supplier: ORG2, Country: AU).", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestOL_BuyingCommissionPercentage_Caption()
		{
			var orgSupplierBuyerLink = Factory.New<OrgSupplierBuyerLink>();
			var resData = DataBoundResourceStrings.GetDataForProperty(orgSupplierBuyerLink.OL_BuyingCommissionPercentageInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Buying Commission Percentage", resData.Caption);
				AssertEquals("ShortCaption", "BCM", resData.ShortCaption);
			});
		}

		#endregion

		#region CodeLists

		public void TestOL_ValuationBasis_ListForAustralia()
		{
			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			Assert("OL_ValuationBasis_List.Count > 0", Link.OL_ValuationBasis_List.Count > 0);
		}

		public void TestOL_ValuationBasis_ListForSouthAfrica()
		{
			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals("Type", typeof(Customs.ZA.ValuationCodeList), Link.OL_ValuationBasis_List.GetType());
		}

		public void TestOL_ValuationBasis_ListForNetherlands()
		{
			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Netherlands;
			AssertEquals("Type", typeof(Customs.EU.ValuationMethodList), Link.OL_ValuationBasis_List.GetType());
		}

		public void TestOL_RelatedParty_List()
		{
			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			AssertEquals(Factory.GetCachedValue<Customs.RelatedPartyList>(), Link.OL_RelatedParty_List);

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(Factory.GetCachedValue<Customs.RelatedIndicatorList>(), Link.OL_RelatedParty_List);

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.NewZealand;
			AssertEquals(Factory.GetCachedValue<Customs.NZ.RelatedPartyList>(), Link.OL_RelatedParty_List);
			Assert("NZ list must have code 'R' for Related but not affecting price", Link.OL_RelatedParty_List.ContainsCode(Customs.NZ.RelatedPartyList.Codes.RelatedDoesNotAffectPrice));

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Afghanistan;
			AssertEquals(Factory.GetCachedValue<Customs.RelatedPartyList>(), Link.OL_RelatedParty_List);

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Factory.GetCachedValue<Customs.RelatedPartyList>(), Link.OL_RelatedParty_List);

			Link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Brazil;
			AssertEquals(Factory.GetCachedValue<Customs.BR.RelatedIndicatorList>(), Link.OL_RelatedParty_List);
		}

		public void TestOL_ValuationBasisListWhenRelatedIndicatorChanged()
		{
			Link.OL_RN_NKImporterCountry = Constants.CountryCodes.SouthAfrica;
			Link.OL_RelatedParty = Customs.RelatedIndicatorList.Codes.Exempt;
			AssertEquals("should be clear out when related indicator is changed to E", ZString.Empty, Link.OL_ValuationBasis);
		}

		public void TestOL_SendImportDocsTo_List()
		{
			Assert("OL_SendImportDocsTo_List.Count > 0", Link.OL_SendImportDocsTo_List.Count > 0);
		}

		public void TestRequiredDocumentsParentTableCode()
		{
			var document = Link.RequiredDocuments.AddNew();
			AssertEquals(document.EQ_ParentTableCode, OrgSupplierBuyerLinkSchema.Constants.Prefix);
		}

		public void TestGetDefaultINCOTermWithPlaceAndMode()
		{
			var linkMode = Link.OrgSupBuyLinkTrnModes.AddNew();

			linkMode.PF_TransportMode = Core.Constants.TransportModes.Air;

			linkMode.PF_IncoTerm = "EXW";
			linkMode.PF_IncoTermPlace = "here";
			linkMode.PF_IncoTermMode = OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS;

			var test = OrgSupplierBuyerLink.GetDefaultINCOTermWithPlaceAndMode(Link.Supplier, Link.Buyer, Link.OL_RN_NKImporterCountry, Core.Constants.TransportModes.Air, "");

			AssertEquals("EXW", test.incoterm);
			AssertEquals("here", test.incotermPlace);
			AssertEquals("THS", test.incotermMode);
		}

		#endregion

		#region Implementation

		OrgSupplierBuyerLink Link;
		OrgHeader Buyer;
		OrgHeader Supplier;
		OrgHeader Organisation;
		const string OrgNameForLoading = "OrgNameForLoading";

		protected override void SetUp()
		{
			base.SetUp();
			Organisation = Factory.New<OrgHeader>();
			Organisation.OH_Code = "gfdasd";
			Link = Organisation.SupplierLinks.AddNew();

			Buyer = Factory.New<OrgHeader>();
			Buyer.OH_FullName = OrgNameForLoading;
			Buyer.OH_RL_NKClosestPort = "AUSYD";
			Buyer.OH_IsConsignee = true;
			Buyer.OH_IsConsignor = false;
			Link.OL_OH_Buyer = Buyer.PK;

			Supplier = Factory.New<OrgHeader>();
			Supplier.OH_FullName = OrgNameForLoading;
			Supplier.OH_RL_NKClosestPort = "AUSYD";
			Supplier.OH_IsConsignor = true;
			Supplier.OH_IsConsignee = false;
			Link.OL_OH_Supplier = Supplier.PK;
		}

		protected OrgHeader SetupOrg(ZString uNLOCO)
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.OH_RL_NKClosestPort = uNLOCO;
			org.OH_Code = "fsrwewd";
			return org;
		}

		#endregion
	}
}
