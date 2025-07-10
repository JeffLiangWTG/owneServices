using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UnitConversionTest : TestCaseWithFactory
	{
		public void TestCoreConversion()
		{
			ZDecimal expected = Core.Constants.Weight.Convert(1m, "T", "LB");
			AssertEquals("T->LB", decimal.Round(expected, 2), decimal.Round(Converter.ConversionFactor("T", "LB"), 2));
		}

		public void TestDivideByZeroException()
		{
			var partUnits = Part.PartUnits;
			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "XXX";
			partUnit1.OF_QuantityInParent = 0;
			partUnit1.OF_ParentPackType = "YYY";

			var partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "AAA";
			partUnit2.OF_QuantityInParent = 0;
			partUnit2.OF_ParentPackType = "XXX";

			var partUnit3 = partUnits.AddNew();
			partUnit3.OF_PackType = "XXX";
			partUnit3.OF_QuantityInParent = 0;
			partUnit3.OF_ParentPackType = "AAA";

			AssertNoExceptionThrown(delegate
			{ Part.UnitConverter.ConversionFactor("XXX", "YYY"); });
		}

		public void TestProductConversionTakesPriority()
		{
			CreateRefPacks("PR", "UNT", 0.51);
			var untPRRefPack = CreateRefPacks("UNT", "PR", 2);
			Factory.Save();

			Part.OP_StockKeepingUnit = "UNT";

			OrgPartUnitCollection partUnits = Part.PartUnits;
			OrgPartUnit partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "UNT";
			partUnit1.OF_QuantityInParent = 0.083333333;
			partUnit1.OF_ParentPackType = "PR";

			OrgPartUnit partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "PR";
			partUnit2.OF_QuantityInParent = 12;
			partUnit2.OF_ParentPackType = "UNT";

			AssertEquals(36m, Part.UnitConverter.Convert(3m, "UNT", "PR"));

			partUnits.RemoveAndDelete(partUnit2);
			Factory.Save();
			AssertEquals(36.0001440006m, Part.UnitConverter.Convert(3m, "UNT", "PR"));

			partUnits.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals(6m, Part.UnitConverter.Convert(3m, "UNT", "PR"));

			untPRRefPack.Delete();
			Factory.Save();
			AssertEquals(5.8823529412m, Part.UnitConverter.Convert(3m, "UNT", "PR"));
		}

		public void TestConvertibleWithTheSameUQ()
		{
			Assert("PCE -> PCE convertible", Converter.Convertible("PCE", "PCE"));
		}

		public void TestConvertibleWithEmptyUQ()
		{
			AssertEquals("PCE -> empty convertible", false, Converter.Convertible("PCE", ZString.Empty));
			AssertEquals("empty -> PCE convertible", false, Converter.Convertible(ZString.Empty, "PCE"));
			AssertEquals("empty -> empty convertible", false, Converter.Convertible(ZString.Empty, ZString.Empty));
		}

		public void TestConvertibleWithStandardUQ()
		{
			Assert("KG -> T convertible", Converter.Convertible("KG", "T"));
			AssertEquals("KG->T", 0.001m, Converter.ConversionFactor("KG", "T"));
			AssertEquals("T->KG", 1000m, Converter.ConversionFactor("T", "KG"));
		}

		public void TestConvertibleWithGenericUQ()
		{
			CreateRefPacks("PCE", "NO", 1);
			Assert("PCE -> NO convertible", Converter.Convertible("PCE", "NO"));
			Assert("NO -> PCE convertible", Converter.Convertible("NO", "PCE"));
		}

		public void TestConvertibleWithSupplierSpecificUQ()
		{
			CreateRefPacks("PCE", "M2", 1, Supplier2.PK);
			Assert("PCE -> M2 not convertible as it is specific", !Converter.Convertible("PCE", "M2"));
		}

		public void TestConversion()
		{
			CreateRefPacks("XX", "NO", 2);//2 NO = 1 XX
			AssertEquals("XX -> NO", 2m, Converter.Convert(1, "XX", "NO")); // 1 XX = 2 NO
			AssertEquals("NO -> XX", 1m, Converter.Convert(2, "NO", "XX"), 0.00001m);
		}

		public void TestGetConvertibleUnitsWithoutSupplierSpecific()
		{
			CreateRefPacks("PCE", "NO", 1);
			CreateRefPacks("PCE", "PR", 2);

			Assert("NO is a convertible units", Converter.ConvertibleUQs.ContainsCode("NO"));
			Assert("PR is a convertible units", Converter.ConvertibleUQs.ContainsCode("PR"));
		}

		public void TestGetConvertibleUnitsWithSupplierSpecific()
		{
			CreateRefPacks("PCE", "NO", 1);
			CreateRefPacks("PCE", "CC", 2, Supplier2.PK);
			Assert("NO is a convertible units", Converter.ConvertibleUQs.ContainsCode("NO"));
			Assert("CC is not a convertible units", !Converter.ConvertibleUQs.ContainsCode("CC"));
		}

		[ExpectNoExceptions]
		public void TestShouldNotbeAccessingPropertyOnADeletedBusinessObject()
		{
			var pack = CreateRefPacks("PCE", "NO", 1);
			Converter.Convertible("PCE", "NO");
			pack.Delete();
			Converter.Convertible("PCE", "NO");
		}

		public void TestGetConversionFactorFromAllPacksCollection()
		{
			CreateRefPacks("DOZ", "NO", 12m);//12 No = 1 Doz
			CreateRefPacks("PCE", "NO", 1m);
			CreateRefPacks("PCE", "PR", 1m);

			Assert("DOZ -> PR convertible", Converter.Convertible("DOZ", "PR"));
			AssertEquals("DOZ -> PR Conversion Factor", 12m, Converter.Convert(1, "DOZ", "PR"));
		}

		public void TestGetConversionFactorFromAllPacksCollectionWithType()
		{
			var converter = new UnitConverter(new DummyConvertibleBizO());
			CreateRefPacks("A", "B", 2m, "CIP").RP_CustomsCountry = "AU";
			CreateRefPacks("A", "B", 0.1m, "").RP_CustomsCountry = "AU";
			CreateRefPacks("A", "E", 0.2m, "").RP_CustomsCountry = "AU";
			CreateRefPacks("B", "C", 3m, "").RP_CustomsCountry = "AU";
			CreateRefPacks("C", "D", 4m, "CIP").RP_CustomsCountry = "AU";
			CreateRefPacks("E", "F", 0.3m, "").RP_CustomsCountry = "AU";
			CreateRefPacks("F", "D", 0.4m, "").RP_CustomsCountry = "AU";

			Factory.Save();
			Assert("A -> D convertible", converter.Convertible("A", "D"));
			AssertEquals("A -> D Conversion Factor", 24m, converter.Convert(1, "A", "D"));

			Assert("A -> D convertible", Converter.Convertible("A", "D"));
			AssertEquals("A -> D Conversion Factor", 0.024m, Converter.Convert(1, "A", "D"));
		}

		public void TestPartUnitsWithPackConversion()
		{
			OrgPartUnitCollection partUnits = Part.PartUnits;
			OrgPartUnit partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "PCE";
			partUnit1.OF_QuantityInParent = 2;
			partUnit1.OF_ParentPackType = "KG";

			AssertEquals("KG->PCE should be convertible", true, Part.UnitConverter.Convertible("KG", "PCE"));
			AssertEquals("KG->PCE", 2m, decimal.Round(Part.UnitConverter.ConversionFactor("KG", "PCE"), 0));
		}

		public void TestHasMoreThanOneConversionFactor()
		{
			OrgPartUnitCollection partUnits = Part.PartUnits;
			OrgPartUnit partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "PCE";
			partUnit1.OF_QuantityInParent = 2;
			partUnit1.OF_ParentPackType = "KG";

			OrgPartUnit partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "PCE";
			partUnit2.OF_QuantityInParent = 3;
			partUnit2.OF_ParentPackType = "CNT";

			OrgPartUnit partUnit3 = partUnits.AddNew();
			partUnit3.OF_PackType = "CNT";
			partUnit3.OF_QuantityInParent = 3;
			partUnit3.OF_ParentPackType = "KG";

			AssertEquals("has more than one convesion", true, Part.UnitConverter.HasMoreThanOneConversionFactor("PCE", "KG"));
		}

		public void TestCoreUnitConverters()
		{
			var converters = Part.UnitConverter.AllRelevantConverters();
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Weight.Grams)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Weight.Ounces)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Weight.OuncesTroy)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Weight.Pounds)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Weight.PoundsTroy)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Weight.Tonnes)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Weight.ShortTons)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Weight.LongTons)));

			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.CubicCentimeters)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.CubicDecimetres)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.CubicFeet)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.CubicInches)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.CubicYards)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.Litre)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.MegaLitre)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.USGallons)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Volume.ImperialGallons)));

			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Length.Feet)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Length.Inches)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Length.Millimetres)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Length.Yards)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Length.Centimetres)));

			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Area.SquareCentimetre)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Area.SquareFoot)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Area.SquareInch)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Area.SquareMillimetre)));
			Assert(converters.Any(data => data.Value.Any(converter => converter.ParentUnit == Core.Constants.Area.SquareYard)));
		}

		public void TestTakeTheBetterOfMultipleConversionFactors()
		{
			CreateRefPacks("GRM", "LTR", 0.001000000, "CIP", "AU");
			CreateRefPacks("DZN", "SET", 12.000000000, "CIP", "AU");
			CreateRefPacks("PCE", "NPR", 0.500000000, "CIP", "AU");
			CreateRefPacks("PCS", "DZN", 0.083333333, "CIP", "AU");
			CreateRefPacks("PCS", "PST", 0.500000000, "CIP", "AU");
			CreateRefPacks("PST", "DZN", 0.166666666, "CIP", "AU");
			CreateRefPacks("PST", "NPR", 1.000000000, "CIP", "AU");
			CreateRefPacks("NPR", "PST", 1.000000000, "CIP", "AU");
			CreateRefPacks("DZN", "NPR", 12.000000000, "CIP", "AU");
			CreateRefPacks("SET", "NPR", 0.500000000, "CIP", "AU");
			CreateRefPacks("PCE", "DZN", 0.083333333, "CIP", "AU");
			CreateRefPacks("NPR", "SET", 2.000000000, "CIP", "AU");
			CreateRefPacks("PST", "SET", 2.000000000, "CIP", "AU");
			CreateRefPacks("NPR", "PCE", 2.000000000, "CIP", "AU");
			CreateRefPacks("SET", "PST", 0.500000000, "CIP", "AU");
			CreateRefPacks("KGM", "LBR", 2.204600000, "CIP", "AU");
			CreateRefPacks("HPC", "PCE", 100.000000000, "CIP", "AU");
			CreateRefPacks("NMB", "DZN", 0.083333333, "CIP", "AU");
			CreateRefPacks("TNE", "KGM", 1000.000000000, "CIP", "AU");
			CreateRefPacks("PCE", "PST", 0.500000000, "CIP", "AU");
			CreateRefPacks("SET", "PCE", 1.000000000, "CIP", "AU");
			CreateRefPacks("SET", "DZN", 0.083333333, "CIP", "AU");
			CreateRefPacks("KPC", "PCE", 1000.000000000, "CIP", "AU");
			CreateRefPacks("PCS", "PCE", 1.000000000, "CIP", "AU");
			CreateRefPacks("DZN", "PST", 6.000000000, "CIP", "AU");
			CreateRefPacks("PCE", "PCS", 1.000000000, "CIP", "AU");
			CreateRefPacks("LBR", "TNE", 0.000453597, "CIP", "AU");
			CreateRefPacks("PCE", "KPC", 0.001000000, "CIP", "AU");
			CreateRefPacks("KPC", "SET", 1000.000000000, "CIP", "AU");
			CreateRefPacks("PRS", "DZN", 0.083333333, "CIP", "AU");
			CreateRefPacks("PST", "PCE", 2.000000000, "CIP", "AU");
			CreateRefPacks("PCS", "SET", 1.000000000, "CIP", "AU");
			CreateRefPacks("PCE", "SET", 1.000000000, "CIP", "AU");
			CreateRefPacks("LBR", "KGM", 0.453597024, "CIP", "AU");
			CreateRefPacks("TNE", "LBR", 2204.600000000, "CIP", "AU");
			CreateRefPacks("NPR", "DZN", 0.083333333, "CIP", "AU");
			CreateRefPacks("DZN", "PCE", 12.000000000, "CIP", "AU");
			CreateRefPacks("KGM", "TNE", 0.001000000, "CIP", "AU");
			CreateRefPacks("MTR", "MTK", 99.000000000, "CIP", "AU");
			CreateRefPacks("A", "B", 2, "CIP", "AU");
			CreateRefPacks("B", "C", 3, "CIP", "AU");
			CreateRefPacks("C", "D", 4, "CIP", "AU");
			CreateRefPacks("A", "F", 7, "CIP", "AU");
			CreateRefPacks("F", "D", 3, "CIP", "AU");

			Factory.Save();

			var converter = new UnitConverter(new DummyConvertibleBizO());
			var result = converter.ConversionFactor("PCE", "SET");
			AssertEquals(1m, result);

			result = converter.ConversionFactor("PCS", "PST");
			AssertEquals(0.5m, result);

			result = converter.ConversionFactor("NPR", "SET");
			AssertEquals(2m, result);

			result = converter.ConversionFactor("A", "D");
			AssertEquals(21m, result);
		}

		class ConverterDataProvider : IUnitConverterDataProvider
		{
			public ConverterDataProvider(BusinessObjectFactory factory, ZGuid supplier)
			{
				Supplier = supplier;
				Factory = factory;
			}

			ZGuid Supplier { get; }
			BusinessObjectFactory Factory { get; }

			OrgSupplierPart IUnitConverterDataProvider.Product => null;

			ZString IUnitConverterDataProvider.CountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			ZGuid IUnitConverterDataProvider.SupplierFK => Supplier;

			BusinessObjectFactory IUnitConverterDataProvider.Factory => Factory;

			bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions => false;

			ZString IUnitConverterDataProvider.Type => RPTypeList.Codes.AllAreas;

			IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits()
			{
				return null;
			}
		}

		public void TestGenericRefPacks()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.FillWithValidTestData();

			CreateRefPacks("CRT", "001", 1m, supplier.PK);
			CreateRefPacks("CRT", "115", 1m, supplier.PK);
			CreateRefPacks("CRT", "115", 10m, ZGuid.Empty);
			CreateRefPacks("CRT", "111", 2m, ZGuid.Empty);
			Factory.Save();

			var dataProvider = new ConverterDataProvider(Factory, supplier.PK);
			var unitConverter = new UnitConverter(dataProvider);

			AssertEquals("'CRT' - '115' - Empty Supplier RefPacks would be ignored", 3, unitConverter.AllCusRefPacks.Count(cnt => cnt.RP_CommercialPack == "CRT"));
			AssertEquals("Should have selected the correct RefPacks", "111", unitConverter.AllCusRefPacks.FirstOrDefault(p => p.RP_CommercialPack == "CRT" && p.RP_OH_Supplier.IsEmpty).RP_CustomsPack);
		}

		public void TestMultipleConversionPaths()
		{
			var partUnit1 = Part.PartUnits.AddNew();
			partUnit1.OF_PackType = "CAS";
			partUnit1.OF_QuantityInParent = 11;
			partUnit1.OF_ParentPackType = "CTN";

			var partUnit2 = Part.PartUnits.AddNew();
			partUnit2.OF_PackType = "BOT";
			partUnit2.OF_QuantityInParent = 4;
			partUnit2.OF_ParentPackType = "CTN";

			var partUnit3 = Part.PartUnits.AddNew();
			partUnit3.OF_PackType = "XXX";
			partUnit3.OF_QuantityInParent = 3;
			partUnit3.OF_ParentPackType = "BOT";

			var partUnit4 = Part.PartUnits.AddNew();
			partUnit4.OF_PackType = "ZZZ";
			partUnit4.OF_QuantityInParent = 2;
			partUnit4.OF_ParentPackType = "XXX";

			AssertEquals("Should find a proper conversion", 2m * 3m * 4m, Converter.ConversionFactor("CTN", "ZZZ"));
		}

		public void TestUserDefinedConversionHasAPrecedenceOverDefaultOnes()
		{
			AssertEquals("Default conversion should be used", 1000m, Converter.ConversionFactor(Core.Constants.Weight.Kilograms, Core.Constants.Weight.Grams));

			var partUnit1 = Part.PartUnits.AddNew();
			partUnit1.OF_PackType = Core.Constants.Weight.Grams;
			partUnit1.OF_QuantityInParent = 150;
			partUnit1.OF_ParentPackType = Core.Constants.Weight.Kilograms;

			AssertEquals("User-defined conversion should be used", 150m, Converter.ConversionFactor(Core.Constants.Weight.Kilograms, Core.Constants.Weight.Grams));

			var partUnit2 = Part.PartUnits.AddNew();
			partUnit2.OF_PackType = Core.Constants.Weight.Kilograms;
			partUnit2.OF_QuantityInParent = 500;
			partUnit2.OF_ParentPackType = Core.Constants.Weight.Tonnes;

			AssertEquals("User-defined conversion should be used", 150m * 500m, Converter.ConversionFactor(Core.Constants.Weight.Tonnes, Core.Constants.Weight.Grams));
		}

		public void TestHasMoreThanOneConversionFactor2()
		{
			OrgPartUnitCollection partUnits = Part.PartUnits;
			OrgPartUnit partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "PCE";
			partUnit1.OF_QuantityInParent = 2;
			partUnit1.OF_ParentPackType = "KG";

			OrgPartUnit partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "PCE";
			partUnit2.OF_QuantityInParent = 3;
			partUnit2.OF_ParentPackType = "T";

			AssertEquals("has not more than one convesion", true, Part.UnitConverter.HasMoreThanOneConversionFactor("PCE", "KG"));
		}

		public void TestOnlyCountrySpecificConversionsAreUsed()
		{
			NUnit.Framework.TestCaseHelper.ClearTable(RefPacksSchema.Constants.TableName);

			var usPackConversion = CreateRefPacks("BOX", "NO", 1m);
			usPackConversion.RP_CustomsCountry = "US";

			Assert("BOX to NO is a US conversion. It should not be used in AU conversion", !Converter.Convertible("BOX", "NO"));
		}

		public void TestProductSpecificConversionTakesPrecendence()
		{
			NUnit.Framework.TestCaseHelper.ClearTable(RefPacksSchema.Constants.TableName);
			var packConversion = CreateRefPacks("BOX", "NO", 1m);
			Factory.Save();

			Part.PartUnits.RemoveAndDeleteAll();
			var partUnit1 = Part.PartUnits.AddNew();
			partUnit1.OF_PackType = "BOX";
			partUnit1.OF_QuantityInParent = 2;
			partUnit1.OF_ParentPackType = "NO";

			Assert("Product specific conversion should be used in lieu of generic conversions", !Part.UnitConverter.HasMoreThanOneConversionFactor("BOX", "NO"));
			AssertEquals("Conversion", 1m, Part.UnitConverter.Convert(2m, "BOX", "NO"));
		}

		public void TestWeDontThrowAnExceptionIfThereIsAZeroConversionFactor()
		{
			OrgPartUnitCollection partUnits = Part.PartUnits;
			OrgPartUnit partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "PCE";
			partUnit1.OF_QuantityInParent = 0;
			partUnit1.OF_ParentPackType = "KG";

			ZDecimal conversionFactor = Converter.ConversionFactor("PCE", "KG");
			Assert(true);
		}

		#region TestConvert_Rounding

		public void TestConvert_Rounding()
		{
			Part.OP_StockKeepingUnit = "UNT";
			CreatePartUnit(Part, "UNT", "BOX", 12m);
			CreatePartUnit(Part, "BOX", "CAS", 12m);
			CreatePartUnit(Part, "CAS", "PLT", 15m);

			AssertEquals((ZDecimal)(12 * 12 * 15), Part.UnitConverter.Convert(1m, "PLT", "UNT"));
		}

		public OrgPartUnit CreatePartUnit(OrgSupplierPart part, string package, string parentPackage, ZDecimal quantityInParent)
		{
			OrgPartUnit partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = package;
			partUnit.OF_ParentPackType = parentPackage;
			partUnit.OF_QuantityInParent = quantityInParent;
			return partUnit;
		}

		#endregion

		#region TestConvert_NoStackOverflow

		public void TestConvert_NoStackOverflow()
		{
			var org = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "PR";

			// do *NOT* change these values or their order as it is required to prove the StackOverflow is fixed.
			CreatePartUnit(part, "PR", "CTN", 10);
			CreatePartUnit(part, "KG", "CTN", 17.3);
			CreatePartUnit(part, "M3", "CTN", 0.087);
			CreatePartUnit(part, "UNT", "PR", 2);
			CreatePartUnit(part, "UNT", "CTN", 20);
			CreatePartUnit(part, "PR", "PLT", 120);
			CreatePartUnit(part, "KG", "PR", 0.865);
			CreatePartUnit(part, "M3", "PR", 0.004);

			AssertNoExceptionThrown(() => part.UnitConverter.Convert(1m, "PR", "PLT"));
		}

		#endregion

		#region TestUnitConverterAllRelevantConvertersDictionary

		public void TestCacheValueForUnitConverterAllRelevantConverters()
		{
			using (UnitConverter.TemporarySetupCachedConvertion(Factory))
			{
				var supplierFK = new ZGuid();
				var product1 = Factory.New<OrgSupplierPart>();
				var product2 = Factory.New<OrgSupplierPart>();
				var conversion1 = new UnitConversion(2, "KG", "NO");
				var conversion2 = new UnitConversion(3, "KG", "NO");

				var mock1 = new Mock<IUnitConverterDataProvider>();
				mock1.Setup(m => m.CountryCode).Returns("US");
				mock1.Setup(m => m.Factory).Returns(Factory);
				mock1.Setup(m => m.Product).Returns(product1);
				mock1.Setup(m => m.SupplierFK).Returns(supplierFK);
				mock1.Setup(m => m.ProductHasSpecificUnitConversions).Returns(false);
				mock1.Setup(m => m.GetUnitConversionFactorsFromProductUnits()).Returns(new IUnitConverter[] { conversion1 });
				mock1.Setup(m => m.Type).Returns(RPTypeList.Codes.AllAreas);

				var mock2 = new Mock<IUnitConverterDataProvider>();
				mock2.Setup(m => m.CountryCode).Returns("US");
				mock2.Setup(m => m.Factory).Returns(Factory);
				mock2.Setup(m => m.Product).Returns(product2);
				mock2.Setup(m => m.SupplierFK).Returns(supplierFK);
				mock2.Setup(m => m.ProductHasSpecificUnitConversions).Returns(false);
				mock2.Setup(m => m.GetUnitConversionFactorsFromProductUnits()).Returns(new IUnitConverter[] { conversion2 });
				mock2.Setup(m => m.Type).Returns(RPTypeList.Codes.AllAreas);

				AssertEquals(2m, new UnitConverter(mock1.Object).AllRelevantConverters().First().Value.First().ConversionFactor);
				AssertEquals(2m, new UnitConverter(mock2.Object).AllRelevantConverters().First().Value.First().ConversionFactor);

				mock1 = new Mock<IUnitConverterDataProvider>();
				mock1.Setup(m => m.CountryCode).Returns("US");
				mock1.Setup(m => m.Factory).Returns(Factory);
				mock1.Setup(m => m.Product).Returns(product1);
				mock1.Setup(m => m.SupplierFK).Returns(supplierFK);
				mock1.Setup(m => m.ProductHasSpecificUnitConversions).Returns(true);
				mock1.Setup(m => m.GetUnitConversionFactorsFromProductUnits()).Returns(new IUnitConverter[] { conversion1 });
				mock1.Setup(m => m.Type).Returns(RPTypeList.Codes.AllAreas);

				mock2 = new Mock<IUnitConverterDataProvider>();
				mock2.Setup(m => m.CountryCode).Returns("US");
				mock2.Setup(m => m.Factory).Returns(Factory);
				mock2.Setup(m => m.Product).Returns(product2);
				mock2.Setup(m => m.SupplierFK).Returns(supplierFK);
				mock2.Setup(m => m.ProductHasSpecificUnitConversions).Returns(true);
				mock2.Setup(m => m.GetUnitConversionFactorsFromProductUnits()).Returns(new IUnitConverter[] { conversion2 });
				mock2.Setup(m => m.Type).Returns(RPTypeList.Codes.AllAreas);

				AssertEquals(2m, new UnitConverter(mock1.Object).AllRelevantConverters().First().Value.First().ConversionFactor);
				AssertEquals(3m, new UnitConverter(mock2.Object).AllRelevantConverters().First().Value.First().ConversionFactor);
			}
		}

		#endregion

		#region Test Insensitive to Case

		public void TestConversionFactor_SameUnit_CaseInsensitive()
		{
			var partUnits = Part.PartUnits;
			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "unt";
			partUnit1.OF_QuantityInParent = 10;
			partUnit1.OF_ParentPackType = "UNT";

			var conversionFactorResult = Converter.ConversionFactor("unt", "UNT");
			AssertEquals("Should be unity and not lookup defined unit conversion", 1m, conversionFactorResult);
		}

		public void TestAllRelevantConvertersGroupByUnit_CaseInsensitive()
		{
			var partUnits = Part.PartUnits;
			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "ASM";
			partUnit1.OF_QuantityInParent = 10;
			partUnit1.OF_ParentPackType = "AMD";

			var partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "amd";
			partUnit2.OF_QuantityInParent = 10;
			partUnit2.OF_ParentPackType = "alg";

			var partUnit3 = partUnits.AddNew();
			partUnit3.OF_PackType = "axs";
			partUnit3.OF_QuantityInParent = 100;
			partUnit3.OF_ParentPackType = "amd";

			var relevantConverters = Converter.AllRelevantConvertersGroupByUnit();
			var result = relevantConverters.TryGetValue("amd", out var converterList);
			AssertEquals("Case insensitive comparer", StringComparer.OrdinalIgnoreCase, relevantConverters.Comparer);
			AssertEquals("Should include 3 conversions", 3, converterList.Count);
		}

		public void TestAllRelevantConverters_CaseInsensitive()
		{
			var partUnits = Part.PartUnits;
			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "asm";
			partUnit1.OF_QuantityInParent = 10;
			partUnit1.OF_ParentPackType = "amd";

			var relevantConverters = Converter.AllRelevantConverters();
			var result = relevantConverters.TryGetValue("AMD|ASM", out var converterList);
			Assert("Can retrieve converter by case insensitive key", result);
			AssertEquals("Case insensitive comparer", StringComparer.OrdinalIgnoreCase, relevantConverters.Comparer);
		}

		public void TestHasMoreThanOneConversionFactor_CaseInsensitive()
		{
			var partUnits = Part.PartUnits;
			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "ASM";
			partUnit1.OF_QuantityInParent = 20;
			partUnit1.OF_ParentPackType = "ALG";

			var partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "asm";
			partUnit2.OF_QuantityInParent = 10;
			partUnit2.OF_ParentPackType = "amd";

			var partUnit3 = partUnits.AddNew();
			partUnit3.OF_PackType = "AMD";
			partUnit3.OF_QuantityInParent = 100;
			partUnit3.OF_ParentPackType = "alg";
			Assert("Should determine 2 conversion factors", Converter.HasMoreThanOneConversionFactor("amd", "alg"));
		}

		public void TestHasMoreThanOneConversionFactor_CaseInsensitive_False()
		{
			var partUnits = Part.PartUnits;
			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "AMD";
			partUnit1.OF_QuantityInParent = 20;
			partUnit1.OF_ParentPackType = "ALG";

			var partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "amd";
			partUnit2.OF_QuantityInParent = 20;
			partUnit2.OF_ParentPackType = "alg";

			var partUnit3 = partUnits.AddNew();
			partUnit3.OF_PackType = "asm";
			partUnit3.OF_QuantityInParent = 2;
			partUnit3.OF_ParentPackType = "amd";

			var partUnit4 = partUnits.AddNew();
			partUnit4.OF_PackType = "ASM";
			partUnit4.OF_QuantityInParent = 40;
			partUnit4.OF_ParentPackType = "ALG";

			Assert("Should determine 1 conversion factors", !Converter.HasMoreThanOneConversionFactor("AMD", "alg"));
		}

		public void TestConversionFactor_ShortestPath_CaseInsensitive()
		{
			// 20 ASM in ALG, 2 amd in ALG
			// x ASM/amd = (20 ASM/ALG) * (1/2 ALG/amd) ==> 10 ASM in amd
			var partUnits = Part.PartUnits;
			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "asm";
			partUnit1.OF_QuantityInParent = 20;
			partUnit1.OF_ParentPackType = "alg";

			var partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "amd";
			partUnit2.OF_QuantityInParent = 2;
			partUnit2.OF_ParentPackType = "alg";

			// 200 ASM in AXX, 2 AXL in AXX, 50 AMD in AXL
			// x ASM/AMD = (200 ASM/AXX) * (1/2 AXX/AXL) * (1/50 AXL/AMD) ==> 2 ASM in AMD
			var partUnit3 = partUnits.AddNew();
			partUnit3.OF_PackType = "ASM";
			partUnit3.OF_QuantityInParent = 200;
			partUnit3.OF_ParentPackType = "AXX";

			var partUnit4 = partUnits.AddNew();
			partUnit4.OF_PackType = "AXL";
			partUnit4.OF_QuantityInParent = 2;
			partUnit4.OF_ParentPackType = "AXX";

			var partUnit5 = partUnits.AddNew();
			partUnit5.OF_PackType = "AMD";
			partUnit5.OF_QuantityInParent = 50;
			partUnit5.OF_ParentPackType = "AXL";

			Assert("Should determine 2 conversion factors", Converter.HasMoreThanOneConversionFactor("ASM", "AMD"));
			AssertEquals("Should choose shortest conversion path", 10m, Converter.ConversionFactor("AMD", "ASM"));
		}

		public void TestConversionsWithCaseDifferences()
		{
			var partUnits = Part.PartUnits;
			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_PackType = "ASM";
			partUnit1.OF_QuantityInParent = 10;
			partUnit1.OF_ParentPackType = "AMD";

			var partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "amd";
			partUnit2.OF_QuantityInParent = 10;
			partUnit2.OF_ParentPackType = "alg";

			var partUnit3 = partUnits.AddNew();
			partUnit3.OF_PackType = "ctn";
			partUnit3.OF_QuantityInParent = 1000;
			partUnit3.OF_ParentPackType = "plt";

			var partUnit4 = partUnits.AddNew();
			partUnit4.OF_PackType = "unt";
			partUnit4.OF_QuantityInParent = 3;
			partUnit4.OF_ParentPackType = "g";

			var result = Converter.AllRelevantConverters();

			CombineAssertions(() =>
			{
				AssertEquals("Upper db, lower requested", 10m, Converter.Convert(1, "amd", "asm"), 0.00001m);
				AssertEquals("lower db, upper requested", 1000m, Converter.Convert(1, "PLT", "ctn"), 0.00001m);
				AssertEquals("mixed case path inside db", 100m, Converter.Convert(1, "alg", "asm"), 0.00001m);
				AssertEquals("lower g in db", 2m, Converter.Convert(6000, "UNT", "KG"), 0.00001m);
			});
		}

		#endregion

		#region Implementation
		UnitConverter Converter;
		OrgHeader Supplier;
		OrgHeader Supplier2;
		OrgHeader RelatedParty;
		OrgSupplierPart Part;

		public class DummyConvertibleBizO : IUnitConverterDataProvider
		{
			public OrgSupplierPart Product { get; }
			public ZString CountryCode => "AU";
			public ZGuid SupplierFK => ZGuid.Empty;
			public BusinessObjectFactory Factory => new BusinessObjectFactory();
			public bool ProductHasSpecificUnitConversions => false;
			public ZString Type => RPTypeList.Codes.CommercialInvoice;
			public IEnumerable<IUnitConverter> GetUnitConversionFactorsFromProductUnits()
			{
				throw new NotImplementedException();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Supplier2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Supplier.PK));

			ZQuery filter = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Supplier.PK);
			filter.AddToFilter(JoinCondition.And, OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Supplier2.PK);
			RelatedParty = Factory.LoadTop1<OrgHeader>(filter);

			Part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation = Part.RelatedOrganisations.AddNew();
			relation.OU_OH = Supplier.PK;

			OrgPartRelation relation2 = Part.RelatedOrganisations.AddNew();
			relation2.OU_OH = RelatedParty.PK;

			Part.OP_PartNum = "NewPart";
			Part.OP_StockKeepingUnit = "KG";

			Converter = Part.UnitConverter;
		}

		CusRefPacks CreateRefPacks(ZString commercialPack, ZString customsPack, ZDecimal factor, ZGuid supplier)
		{
			var packConversion = CreateRefPacks(commercialPack, customsPack, factor);
			packConversion.RP_OH_Supplier = supplier;
			return packConversion;
		}

		CusRefPacks CreateRefPacks(ZString commercialPack, ZString customsPack, ZDecimal factor)
		{
			var packConversion = Factory.New<CusRefPacks>();
			packConversion.RP_CommercialPack = commercialPack;
			packConversion.RP_ConversionFactor = factor;
			packConversion.RP_CustomsPack = customsPack;
			return packConversion;
		}

		CusRefPacks CreateRefPacks(ZString commercialPack, ZString customsPack, ZDecimal factor, ZString type)
		{
			var packConversion = CreateRefPacks(commercialPack, customsPack, factor);
			packConversion.RP_Type = type;
			return packConversion;
		}

		CusRefPacks CreateRefPacks(ZString commercialPack, ZString customsPack, ZDecimal factor, ZString type, ZString countryCode)
		{
			var packConversion = CreateRefPacks(commercialPack, customsPack, factor, type);
			packConversion.RP_CustomsCountry = countryCode;
			return packConversion;
		}

		#endregion
	}
}
