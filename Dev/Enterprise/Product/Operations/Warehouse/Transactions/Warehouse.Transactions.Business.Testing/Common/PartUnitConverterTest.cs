using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class PartUnitConverterTest : WhsTestCaseWithFactory
	{
		public void TestHasPalletDefinition()
		{
			AssertEquals(false, Converter.HasPalletDefinition(null));
			AssertEquals(false, Converter.HasPalletDefinition(Part));

			Helper.CreateProductUnit(Part, "PLT", 10);
			AssertEquals(true, Converter.HasPalletDefinition(Part));
		}

		public void TestUnitConverterNoMatchOrNoPart()
		{
			AssertEquals(0m, Converter.Convert(null, 5m, "CTN", "UNT"));
			AssertEquals(0m, Converter.Convert(Part, 5m, "CTN", "UNT"));

			Helper.CreateProductUnit(Part, "CTN", 6m);

			AssertEquals(0m, Converter.Convert(null, 5m, "CTN", "UNT"));
			AssertEquals(5m, Converter.Convert(Part, 5m, "UNT", "UNT"));
			AssertEquals(5m, Converter.Convert(Part, 5m, "CTN", "CTN"));
			AssertEquals(0m, Converter.Convert(Part, 5m, "BOX", "UNT"));
			AssertEquals(0m, Converter.Convert(Part, 5m, "UNT", "BOX"));
			AssertEquals(0m, Converter.Convert(Part, 5m, "BOX", "CTN"));
			AssertEquals(0m, Converter.Convert(Part, 5m, "CTN", "BOX"));
		}

		public void TestUnitConverterWhenFromAndToAreSame()
		{
			AssertEquals(5m, Converter.Convert(Part, 5m, "UNT", "UNT"));
			AssertEquals(10m, Converter.Convert(Part, 10m, "CTN", "CTN"));
			AssertEquals(15.25m, Converter.Convert(Part, 15.25m, "BOX", "BOX"));
		}

		public void TestUnitConverterWithSimpleExactMatch()
		{
			Helper.CreateProductUnit(Part, "CTN", 6m);
			AssertBasic(5m, "UNT", "CTN");
		}

		public void TestUnitConverterWithSimpleTree()
		{
			Helper.CreateProductUnit(Part, "CTN", 6m);
			Helper.CreateProductUnit(Part, "CTN", "BOX", 8m);
			AssertBasic2(5m, "UNT", "CTN", "BOX");
		}

		public void TestUnitConverterWithComplexTree()
		{
			Helper.CreateProductUnit(Part, "CTN", 6m);
			Helper.CreateProductUnit(Part, "CTN", "BOX", 8m);
			Helper.CreateProductUnit(Part, "BOX", "OUT", 10m);
			Helper.CreateProductUnit(Part, "OUT", "PLT", 2m);
			AssertComplex(5m, "UNT", "CTN", "BOX", "OUT", "PLT");
		}

		public void TestUnitConverterWithComplexTreeIsNotUpsetByVariousOtherSetups()
		{
			Helper.CreateProductUnit(Part, "CTN", 6m);
			Helper.CreateProductUnit(Part, "CTN", "BOX", 8m);
			Helper.CreateProductUnit(Part, "BOX", "OUT", 10m);
			Helper.CreateProductUnit(Part, "OUT", "PLT", 2m);

			// create some spurious
			Helper.CreateProductUnit(Part, "IN", "OUT", 3m);
			Helper.CreateProductUnit(Part, "OUT", "IN", 4m);
			Helper.CreateProductUnit(Part, "CNT", "CTN", 5m);
			Helper.CreateProductUnit(Part, "KG", "UNT", 8m);
			Helper.CreateProductUnit(Part, "M3", "BOX", 3m);

			AssertComplex(5m, "UNT", "CTN", "BOX", "OUT", "PLT");
		}

		#region Assert Conversions

		void AssertBasic(ZDecimal qty, string unit1, string unit2)
		{
			AssertEquals(6m * qty, Converter.Convert(Part, qty, unit2, unit1));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 6m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit1, unit2));
		}

		void AssertBasic2(ZDecimal qty, string unit1, string unit2, string unit3)
		{
			AssertBasic(qty, unit1, unit2);

			AssertEquals(8m * 6m * qty, Converter.Convert(Part, qty, unit3, unit1));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 6m / 8m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit1, unit3));

			AssertEquals(8m * qty, Converter.Convert(Part, qty, unit3, unit2));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 8m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit2, unit3));
		}

		void AssertComplex(ZDecimal qty, string unit1, string unit2, string unit3, string unit4, string unit5)
		{
			AssertBasic2(qty, unit1, unit2, unit3);

			AssertEquals(10m * 8m * 6m * qty, Converter.Convert(Part, qty, unit4, unit1));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 6m / 8m / 10m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit1, unit4));

			AssertEquals(10m * 8m * qty, Converter.Convert(Part, qty, unit4, unit2));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 8m / 10m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit2, unit4));

			AssertEquals(10m * qty, Converter.Convert(Part, qty, unit4, unit3));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 10m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit3, unit4));

			AssertEquals(2m * 10m * 8m * 6m * qty, Converter.Convert(Part, qty, unit5, unit1));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 6m / 8m / 10m / 2m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit1, unit5));

			AssertEquals(2m * 10m * 8m * qty, Converter.Convert(Part, qty, unit5, unit2));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 8m / 10m / 2m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit2, unit5));

			AssertEquals(2m * 10m * qty, Converter.Convert(Part, qty, unit5, unit3));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 10m / 2m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit3, unit5));

			AssertEquals(2m * qty, Converter.Convert(Part, qty, unit5, unit4));
			AssertEquals(ZArchitecture.Core.Utilities.Round(qty / 2m, PartUnitConverter_DonotUseThisClassItCausesAStackOverflow.DefaultDecimals), Converter.ConvertRounded(Part, qty, unit4, unit5));
		}

		#endregion

		#region Implementation

		OrgSupplierPart part;
		OrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Helper.CreateProduct(Helper.CreateClient(), "P1");
					part.PartUnits.RemoveAndDeleteAll();
				}
				return part;
			}
		}

		PartUnitConverter_DonotUseThisClassItCausesAStackOverflow converter;
		PartUnitConverter_DonotUseThisClassItCausesAStackOverflow Converter
		{
			get { return converter ?? (converter = new PartUnitConverter_DonotUseThisClassItCausesAStackOverflow()); }
		}

		#endregion
	}
}
