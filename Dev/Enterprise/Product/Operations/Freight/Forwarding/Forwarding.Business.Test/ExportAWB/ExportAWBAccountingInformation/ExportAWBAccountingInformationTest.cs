using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBAccountingInformation))]
	sealed class ExportAWBAccountingInformationTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestAccountingCodes()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ExportAWBAccountingInformation accountingInformation = factory.New<ExportAWBAccountingInformation>();
			AssertEquals(OLookUpEditType.AWBAccountingCodes, accountingInformation.Lookups.AccountingCodes.LookupEditType);
		}

		public void TestIsSavedByFactory()
		{
			var accountingInformation = Factory.New<ExportAWBAccountingInformation>();
			AssertEquals("Object w/o header behaves normally", true, accountingInformation.IsSavedByFactory);
			accountingInformation.Delete();

			var aWBHeader = Factory.New<ShipmentExportAWBHeader>();
			accountingInformation = aWBHeader.AWBAccountingInformations.AddNew();
			aWBHeader.EH_ParentID = Factory.New<ForwardingShipment>().PK;
			aWBHeader.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Not overridden new AWB is never saved", false, accountingInformation.IsSavedByFactory);
			Factory.Save();
			AssertEquals(false, accountingInformation.IsInDatabase);

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			aWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			aWBHeader.EH_AreRateLinesOverridden = false;
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed and repopulated", false, accountingInformation.IsSavedByFactory);

			accountingInformation = aWBHeader.AWBAccountingInformations.AddNew();
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed", false, accountingInformation.IsSavedByFactory);

			aWBHeader.ForceSavingByFactory = true;
			AssertEquals("Forced AWB is always saved", true, accountingInformation.IsSavedByFactory);
			Factory.Save();
			AssertEquals(true, accountingInformation.IsInDatabase);

			aWBHeader.ForceSavingByFactory = false;
			AssertEquals("Once saved but not overridden is not saved next time", false, accountingInformation.IsSavedByFactory);

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("Saved when 'Override' is ticked", true, accountingInformation.IsSavedByFactory);
			Factory.Save();

			AssertEquals("Overridden is not saved if doesn't have changes", false, accountingInformation.IsSavedByFactory);

			accountingInformation.EA_Sequence = 3;
			AssertEquals("Overridden is saved when has changes", true, accountingInformation.IsSavedByFactory);
			Factory.Save();

			aWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals("Overridden is saved when 'Override' has changes and already in the database", true, accountingInformation.IsSavedByFactory);
			var accountingInformation2 = aWBHeader.AWBAccountingInformations.AddNew();
			Factory.Save();
			AssertEquals(true, accountingInformation.IsDeleted);
			AssertEquals(true, accountingInformation2.IsInDatabase);

			accountingInformation2.EA_Sequence = 4;
			AssertEquals("Not overridden and saved is not saved when has changes", false, accountingInformation2.IsSavedByFactory);
		}

		public void TestIsItalianRegistrationCode()
		{
			var accountingInformation = Factory.New<ExportAWBAccountingInformation>();
			accountingInformation.EA_InformationID = ZString.Empty;
			AssertEquals("Not Italian Code.", false, accountingInformation.IsItalianRegistrationCode);

			accountingInformation.EA_InformationID = ExportAWBAccountingInformationLookups.IssuedByIVA;
			AssertEquals("Is Italian Code.", true, accountingInformation.IsItalianRegistrationCode);

			accountingInformation.EA_InformationID = ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA;
			AssertEquals("Is Italian Code.", true, accountingInformation.IsItalianRegistrationCode);
		}

		public void TestIsSkippedOnMessaging()
		{
			var accountingInformation = Factory.New<ExportAWBAccountingInformation>();
			accountingInformation.EA_InformationID = ZString.Empty;
			AssertEquals("Record should NOT be skipped.", false, accountingInformation.IsSkippedOnMessaging);

			accountingInformation.EA_InformationID = ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA;
			AssertEquals("Record should be skipped.", true, accountingInformation.IsSkippedOnMessaging);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ExportAWBAccountingInformation>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var header = factory.NewWithValidTestData<ConsolExportAWBHeader>();
			header.EH_ParentID = consol.PK;
			header.ForceSavingByFactory = true;

			var result = factory.NewWithValidTestData<ExportAWBAccountingInformation>();
			result.EA_EH = header.PK;

			return result;
		}
	}
}
