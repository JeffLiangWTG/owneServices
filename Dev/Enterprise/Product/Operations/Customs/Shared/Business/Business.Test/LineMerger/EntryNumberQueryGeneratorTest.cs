using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EntryNumberQueryGeneratorTest : TestCaseWithFactory
	{
		public void TestGetEntryNumberQuery()
		{
			var decWithEntryNumber = Factory.New<BaseJobDeclaration>();
			var entryNumberUnderDec = Factory.New<CusEntryNumber>();
			entryNumberUnderDec.Parent = decWithEntryNumber;
			entryNumberUnderDec.CE_EntryNum = "1";
			entryNumberUnderDec.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumberUnderDec.CE_Category = "CUS";

			var decWithEntryWithNumber = Factory.New<BaseJobDeclaration>();
			var entry = decWithEntryWithNumber.CustomsEntryHeaders.AddNew();
			var entryNumberUnderCEH = Factory.New<CusEntryNumber>();
			entryNumberUnderCEH.Parent = entry;
			entryNumberUnderCEH.CE_EntryNum = "1";
			entryNumberUnderCEH.CE_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			entryNumberUnderCEH.CE_Category = "CUS";

			var decWithEntryWithNumberButTypeIsExcluded = Factory.New<BaseJobDeclaration>();
			var entryWithNumberButTypeIsExcluded = decWithEntryWithNumberButTypeIsExcluded.CustomsEntryHeaders.AddNew();
			var entryNumberUnderCEHButTypeIsExcluded = Factory.New<CusEntryNumber>();
			entryNumberUnderCEHButTypeIsExcluded.Parent = entryWithNumberButTypeIsExcluded;
			entryNumberUnderCEHButTypeIsExcluded.CE_EntryNum = "1";
			entryNumberUnderCEHButTypeIsExcluded.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumberUnderCEHButTypeIsExcluded.CE_Category = "CUS";
			entryNumberUnderCEHButTypeIsExcluded.CE_EntryType = "MUC";

			var declarationWithReferenceNumber = Factory.New<BaseJobDeclaration>();
			var entryNumberUnderDecButWrongCategory = Factory.New<CusEntryNumber>();
			entryNumberUnderDecButWrongCategory.Parent = declarationWithReferenceNumber;
			entryNumberUnderDecButWrongCategory.CE_EntryNum = "1";
			entryNumberUnderDecButWrongCategory.CE_Category = "OTH";
			entryNumberUnderDecButWrongCategory.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var decShipWithEntryNumberCus = Factory.New<BaseJobDeclaration>();
			var shipmentCus = Factory.New<ForwardingShipment>();
			var entryNumberUnderShipmentManuallyEnteredCus = Factory.New<CusEntryNumber>();
			entryNumberUnderShipmentManuallyEnteredCus.Parent = shipmentCus;
			entryNumberUnderShipmentManuallyEnteredCus.CE_EntryIsSystemGenerated = false;
			entryNumberUnderShipmentManuallyEnteredCus.CE_EntryNum = "1";
			entryNumberUnderShipmentManuallyEnteredCus.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			decShipWithEntryNumberCus.JE_JS = shipmentCus.PK;
			entryNumberUnderShipmentManuallyEnteredCus.CE_Category = "CUS";

			var decShipWithEntryNumberOth = Factory.New<BaseJobDeclaration>();
			var shipmentOth = Factory.New<ForwardingShipment>();
			var entryNumberUnderShipmentManuallyEnteredOth = Factory.New<CusEntryNumber>();
			entryNumberUnderShipmentManuallyEnteredOth.Parent = shipmentOth;
			entryNumberUnderShipmentManuallyEnteredOth.CE_EntryNum = "1";
			entryNumberUnderShipmentManuallyEnteredOth.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			decShipWithEntryNumberOth.JE_JS = shipmentOth.PK;
			entryNumberUnderShipmentManuallyEnteredOth.CE_Category = "OTH";

			var decWithNothing = Factory.New<BaseJobDeclaration>();

			Factory.Save();

			var query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "1", Core.Constants.CountryCodes.Australia);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(!decWithEntryWithNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberCus.MatchesFilter(query));
			Assert(!declarationWithReferenceNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberOth.MatchesFilter(query));
			Assert(!decWithNothing.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "1,2", Core.Constants.CountryCodes.Australia);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(!decWithEntryWithNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberCus.MatchesFilter(query));
			Assert(!declarationWithReferenceNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberOth.MatchesFilter(query));
			Assert(!decWithNothing.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Contains, "1,2", Core.Constants.CountryCodes.Australia);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(!decWithEntryWithNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberCus.MatchesFilter(query));
			Assert(!declarationWithReferenceNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberOth.MatchesFilter(query));
			Assert(!decWithNothing.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.StartsWith, "1,2", Core.Constants.CountryCodes.Australia);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(!decWithEntryWithNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberCus.MatchesFilter(query));
			Assert(!declarationWithReferenceNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberOth.MatchesFilter(query));
			Assert(!decWithNothing.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "2", Core.Constants.CountryCodes.Australia);
			Assert(!decWithEntryNumber.MatchesFilter(query));
			Assert(!decWithEntryWithNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberCus.MatchesFilter(query));
			Assert(!declarationWithReferenceNumber.MatchesFilter(query));
			Assert(!decShipWithEntryNumberOth.MatchesFilter(query));
			Assert(!decWithNothing.MatchesFilter(query));

			entryNumberUnderCEH.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			var blankQuery = EntryNumberQueryGenerator.GetEntryNumberQuery(SpecialComparisonOperator.IsBlank, "", Core.Constants.CountryCodes.Australia);
			var isNotBlankQuery = EntryNumberQueryGenerator.GetEntryNumberQuery(SpecialComparisonOperator.IsNotBlank, "", Core.Constants.CountryCodes.Australia);
			Assert(!decWithEntryNumber.MatchesFilter(blankQuery));
			Assert(decWithNothing.MatchesFilter(blankQuery));
			Assert(decWithEntryNumber.MatchesFilter(isNotBlankQuery));
			Assert(!decWithEntryWithNumberButTypeIsExcluded.MatchesFilter(isNotBlankQuery));
			Assert(!decWithNothing.MatchesFilter(isNotBlankQuery));
			Assert(!decWithNothing.MatchesFilter(isNotBlankQuery));
			Assert(decWithEntryWithNumberButTypeIsExcluded.MatchesFilter(blankQuery));

			AssertNoExceptionThrown(delegate
			{ query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "{", Core.Constants.CountryCodes.Australia); });

			var declarationInUS = Factory.New<BaseJobDeclaration>();
			var entryNumberInUS = Factory.New<CusEntryNumber>();
			entryNumberInUS.Parent = declarationInUS;
			entryNumberInUS.CE_EntryNum = "20453607";
			entryNumberInUS.CE_Category = "CUS";
			entryNumberInUS.CE_EntryType = "ENS";
			entryNumberInUS.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "20453607", Core.Constants.CountryCodes.UnitedStates);
			query.IgnoreActiveFilter = true;
			Assert(!decWithEntryNumber.MatchesFilter(query));
			Assert(declarationInUS.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "1,20453607", Core.Constants.CountryCodes.UnitedStates);
			Assert(!decWithEntryNumber.MatchesFilter(query));
			Assert(declarationInUS.MatchesFilter(query));

			entryNumberUnderDec.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "1,20453607", Core.Constants.CountryCodes.UnitedStates);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(declarationInUS.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Contains, "1,2", Core.Constants.CountryCodes.UnitedStates);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(declarationInUS.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.StartsWith, "1,2", Core.Constants.CountryCodes.UnitedStates);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(declarationInUS.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.StartsWith, "1,3", Core.Constants.CountryCodes.UnitedStates);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(!declarationInUS.MatchesFilter(query));

			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Contains, "1,9", Core.Constants.CountryCodes.UnitedStates);
			Assert(decWithEntryNumber.MatchesFilter(query));
			Assert(!declarationInUS.MatchesFilter(query));

			declarationInUS.JE_IsCancelled = true;
			Factory.Save();
			query = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "20453607", Core.Constants.CountryCodes.UnitedStates);
			Assert(!declarationInUS.MatchesFilter(query));
			query.IgnoreActiveFilter = true;
			Assert(declarationInUS.MatchesFilter(query));
		}

		public void TestShouldLoadAllCountriesInWebTracker()
		{
			Globals.IsWeb = true;
			var dec1 = Factory.New<BaseJobDeclaration>();
			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.Parent = dec1;
			entryNumber1.CE_EntryNum = "123";
			entryNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			entryNumber1.CE_Category = "CUS";

			var dec2 = Factory.New<BaseJobDeclaration>();
			var entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.Parent = dec2;
			entryNumber2.CE_EntryNum = "456";
			entryNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			entryNumber2.CE_Category = "CUS";

			Factory.Save();
			var query1 = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "123", Core.Constants.CountryCodes.Canada);
			Assert(dec1.MatchesFilter(query1));

			var query2 = EntryNumberQueryGenerator.GetEntryNumberQuery(SQLComparisonOperator.Equal, "456", Core.Constants.CountryCodes.UnitedStates);
			Assert(dec2.MatchesFilter(query2));
		}
	}
}
