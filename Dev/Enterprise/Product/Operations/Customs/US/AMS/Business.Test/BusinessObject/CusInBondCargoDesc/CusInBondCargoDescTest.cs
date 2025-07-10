using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondCargoDescImportedFromSailingTest : LinkedSailingBillsImportedTest
	{
		public void TestRemoveSpecificCharForDescription()
		{
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var cargoDescCol = header.Bills.SelectMany(x => x.MovementDetail.Containers.SelectMany(y => y.Commodities));
			var cargoDesc = cargoDescCol.FirstOrDefault(x => x.BY_HarmonisedTariff == "01011000");
			AssertNotNull(cargoDesc);
			cargoDesc.BY_Description = "A		B";//two 'tab'
			AssertEquals("AB", cargoDesc.BY_Description);
		}

		public void TestImporting()
		{
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			var cargoDescCol = header.Bills.SelectMany(x => x.MovementDetail.Containers.SelectMany(y => y.Commodities));
			var cargoDesc = cargoDescCol.FirstOrDefault(x => x.BY_HarmonisedTariff == "01011000");
			AssertNotNull(cargoDesc);
			AssertEquals(1000m, cargoDesc.BY_GrossWeight);
			AssertEquals("KG", cargoDesc.BY_GrossWeightUnit);
			AssertEquals("PLT", cargoDesc.BY_ManifestUnitCode);
			AssertEquals(11, cargoDesc.BY_PieceCount);
			AssertEquals("DESCRIPTION", cargoDesc.BY_Description);
			AssertEquals("MARKS AND NUMBERS", cargoDesc.BY_MarksAndNumbers);

			cargoDesc = cargoDescCol.FirstOrDefault(x => x.BY_HarmonisedTariff == "01011001");
			AssertNotNull(cargoDesc);
			AssertEquals(1m, cargoDesc.BY_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, cargoDesc.BY_GrossWeightUnit);
			AssertEquals(2, cargoDesc.BY_PieceCount);
			AssertEquals(Core.Constants.PkgUnit.Unit, cargoDesc.BY_ManifestUnitCode);
			AssertEquals("VEHICLE", cargoDesc.BY_Description);
			AssertEquals("VEHICLE MARKS & NUMBERS", cargoDesc.BY_MarksAndNumbers);
			AssertEquals(50m, cargoDesc.BY_MonetaryValue);

			cargoDesc = cargoDescCol.FirstOrDefault(x => x.BY_HarmonisedTariff == "01011002");
			AssertNotNull(cargoDesc);
			AssertEquals(1m, cargoDesc.BY_GrossWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, cargoDesc.BY_GrossWeightUnit);
			AssertEquals(2, cargoDesc.BY_PieceCount);
			AssertEquals(ManifestUnitList.Codes.Box, cargoDesc.BY_ManifestUnitCode);
			AssertEquals("TOP PACK", cargoDesc.BY_Description);
			AssertEquals("TOP PACK MAKRS & NUMBERS", cargoDesc.BY_MarksAndNumbers);
			AssertEquals(50m, cargoDesc.BY_MonetaryValue);
		}
	}

	[TestedType(typeof(CusInBondCargoDesc))]
	public class CusInBondCargoDescTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICanDeleteMembers()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			var header = cargoDesc.Header;
			AssertEquals(true, ((ICanDelete)cargoDesc).CanDelete);

			var consol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals(false, ((ICanDelete)cargoDesc).CanDelete);
			AssertEquals("Commodity values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'.", ((ICanDelete)cargoDesc).ReasonForNotAbleToDelete);

			header.BH_OverrideFreightDefaults = true;
			AssertEquals(true, ((ICanDelete)cargoDesc).CanDelete);
		}

		public void TestICargoDescriptionMembersWithLongNumber()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			ICargoDescription lineDetails = cargoDesc;
			cargoDesc.BY_Description = "GOOD*LOOKING*BOB";
			AssertEquals("Description", "GOOD?LOOKING?BOB", lineDetails.Description);

			cargoDesc.BY_MonetaryValue = 999999999.99999999m;
			AssertEquals("Value", 1000000000, lineDetails.Value);
		}

		public void TestICargoDescriptionMembers()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			ICargoDescription lineDetails = cargoDesc;
			cargoDesc.BY_Description = "GOOD*LOOKING*BOB";
			AssertEquals("Description", "GOOD?LOOKING?BOB", lineDetails.Description);

			cargoDesc.BY_MonetaryValue = 10m;
			AssertEquals("Value", 10, lineDetails.Value);

			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			cargoDesc.BY_GrossWeight = 14m;
			AssertEquals("Weight", 14, lineDetails.Weight);
			cargoDesc.BY_GrossWeight = 14.9m;
			AssertEquals("Weight", 15, lineDetails.Weight);
			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals("Weight", 14900, lineDetails.Weight);
			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("Weight", 15, lineDetails.Weight);
			cargoDesc.BY_GrossWeight = 1000000000m;
			AssertEquals("Weight", 1000000000, lineDetails.Weight);
			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals("Weight", 1000000000, lineDetails.Weight);
			cargoDesc.BY_GrossWeight = 999999999m;
			AssertEquals("Weight", 999999999, lineDetails.Weight);
			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("Weight", 999999999, lineDetails.Weight);

			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals("WeightUnit", Core.Constants.Weight.Pounds, lineDetails.WeightUnit);
			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("WeightUnit", Core.Constants.Weight.Kilograms, lineDetails.WeightUnit);
			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("WeightUnit", Core.Constants.Weight.Kilograms, lineDetails.WeightUnit);
			cargoDesc.BY_GrossWeightUnit = "Z!";
			AssertEquals("WeightUnit", "Z!", lineDetails.WeightUnit);

			cargoDesc.BY_PieceCount = 150;
			AssertEquals("PieceCount", 150m, lineDetails.PieceCount);

			cargoDesc.BY_HarmonisedTariff = "10.2030.45";
			AssertEquals("HarmonizedNumber", "10203045", lineDetails.HarmonizedNumber);

			cargoDesc.BY_MarksAndNumbers = "FUNNY*LOOKING*STAIN";
			AssertEquals("MarksAndNumbers", "FUNNY?LOOKING?STAIN", lineDetails.MarksAndNumbers);

			cargoDesc.BY_CusC4Number = "CED354234";
			AssertEquals("C4Number", "CED354234", lineDetails.C4Number);

			cargoDesc.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			AssertEquals("CountryCode", Core.Constants.CountryCodes.NewZealand, lineDetails.CountryCode);

			cargoDesc.BY_ManifestUnitCode = Core.Constants.PkgUnit.Pallet;
			AssertEquals("ManifestUnitCode", Core.Constants.PkgUnit.Pallet, lineDetails.ManifestUnitCode);
		}

		public void TestLookups()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			AssertEquals(typeof(CusInBondCargoDescLookups), cargoDesc.Lookups.GetType());
		}

		public void TestValidation()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			AssertEquals(typeof(CusInBondCargoDescValidation), cargoDesc.Validation.GetType());
		}

		public void TestBY_FormattedHarmonisedTariff()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			cargoDesc.BY_FormattedHarmonisedTariff = "7904";
			AssertEquals("7904.00", cargoDesc.BY_FormattedHarmonisedTariff);

			cargoDesc.BY_FormattedHarmonisedTariff = "";
			AssertEquals("", cargoDesc.BY_FormattedHarmonisedTariff);
			cargoDesc.BY_FormattedHarmonisedTariff = "333";
			AssertEquals("333", cargoDesc.BY_FormattedHarmonisedTariff);
			cargoDesc.BY_FormattedHarmonisedTariff = "22";
			AssertEquals("2200.00", cargoDesc.BY_FormattedHarmonisedTariff);

			cargoDesc.BY_HarmonisedTariff = "101 020 3010";
			AssertEquals("1010.20.3010", cargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("1010203010", cargoDesc.BY_HarmonisedTariff);
			cargoDesc.BY_FormattedHarmonisedTariff = "10.1 569 3.534";
			AssertEquals("1015.69.3534", cargoDesc.BY_FormattedHarmonisedTariff);
			AssertEquals("1015693534", cargoDesc.BY_HarmonisedTariff);

			cargoDesc.BY_HarmonisedTariff = "10.2030.40";
			AssertEquals("TariffNumber", "10203040", cargoDesc.BY_HarmonisedTariff);
			AssertEquals("TariffNumber", "1020.30.40", cargoDesc.BY_FormattedHarmonisedTariff);
		}

		public void TestBY_HarmonisedTariff_MaxLength()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			AssertEquals(10, cargoDesc.BY_HarmonisedTariffInfo.MaxLength);
		}

		public void TestBY_FormattedHarmonised_MaxLength()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			AssertEquals(12, cargoDesc.BY_FormattedHarmonisedTariffInfo.MaxLength);
		}

		public void TestWeight()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			cargoDesc.BY_GrossWeight = 10.1234567m;
			cargoDesc.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals(new ZWeight(10.123457m, Core.Constants.Weight.Pounds), cargoDesc.Weight);
		}

		public void TestICargoDescription_Description()
		{
			var cargoDesc = GetNewCusInBondCargoDesc(Factory);
			cargoDesc.BY_Description = "GOOD*LOOKING*BOB";
			AssertEquals("GOOD*LOOKING*BOB", cargoDesc.BY_Description);
			AssertEquals("GOOD?LOOKING?BOB", ((ICargoDescription)cargoDesc).Description);

			var note = cargoDesc.Notes.AddNew();
			note.ST_NoteType = nameof(StmNoteVisibility.INT);
			note.ST_IsCustomDescription = true;
			note.ST_NoteText = "THIS*IS*A*2048*MAX*STRING";
			note.ST_Description = "Detailed Goods Description";

			AssertEquals("THIS?IS?A?2048?MAX?STRING", ((ICargoDescription)cargoDesc).Description);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewCusInBondCargoDesc(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewCusInBondCargoDesc(factory);

		CusInBondCargoDesc GetNewCusInBondCargoDesc(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			return container.Commodities.AddNew();
		}
	}
}
