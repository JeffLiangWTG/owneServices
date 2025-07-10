using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ABIUnitOfMeasureListTest : TestCase
	{
		public void TestIsPackageType()
		{
			var list = new ABIUnitOfMeasureList();
			foreach (var code in new[]
			{
				ABIUnitOfMeasureList.Codes.Barrels,
				ABIUnitOfMeasureList.Codes.Case,
				ABIUnitOfMeasureList.Codes.Dozen,
				ABIUnitOfMeasureList.Codes.DozenPairs,
				ABIUnitOfMeasureList.Codes.DozenPieces,
				ABIUnitOfMeasureList.Codes.Number,
				ABIUnitOfMeasureList.Codes.Packs,
				ABIUnitOfMeasureList.Codes.Pairs,
				ABIUnitOfMeasureList.Codes.Pieces
			})
			{
				AssertEquals(code, true, ABIUnitOfMeasureList.IsPackageType(code));
				list.RemoveCode(code);
			}

			foreach (var code in new[]
			{
				ABIUnitOfMeasureList.Codes.AlternatingCurrent,
				ABIUnitOfMeasureList.Codes.BolusesDosage,
				ABIUnitOfMeasureList.Codes.CapsulesDosage,
				ABIUnitOfMeasureList.Codes.Carat,
				ABIUnitOfMeasureList.Codes.Celsius,
				ABIUnitOfMeasureList.Codes.Centigrams,
				ABIUnitOfMeasureList.Codes.Centimeters,
				ABIUnitOfMeasureList.Codes.CleanYield,
				ABIUnitOfMeasureList.Codes.CleanYieldGram,
				ABIUnitOfMeasureList.Codes.CleanYieldKilogram,
				ABIUnitOfMeasureList.Codes.ContentGram,
				ABIUnitOfMeasureList.Codes.ContentKilogram,
				ABIUnitOfMeasureList.Codes.ContentTon,
				ABIUnitOfMeasureList.Codes.Cubic,
				ABIUnitOfMeasureList.Codes.CubicCentimeter,
				ABIUnitOfMeasureList.Codes.CubicCentimeters,
				ABIUnitOfMeasureList.Codes.CubicFeetVolume,
				ABIUnitOfMeasureList.Codes.CubicMeters,
				ABIUnitOfMeasureList.Codes.CubicYardsVolume,
				ABIUnitOfMeasureList.Codes.Curie,
				ABIUnitOfMeasureList.Codes.Degree,
				ABIUnitOfMeasureList.Codes.Denier,
				ABIUnitOfMeasureList.Codes.DirectCurrent,
				ABIUnitOfMeasureList.Codes.FeetLength,
				ABIUnitOfMeasureList.Codes.FiberM,
				ABIUnitOfMeasureList.Codes.Fibers,
				ABIUnitOfMeasureList.Codes.Giqabecquerel,
				ABIUnitOfMeasureList.Codes.Gram,
				ABIUnitOfMeasureList.Codes.Gross,
				ABIUnitOfMeasureList.Codes.GrossLines,
				ABIUnitOfMeasureList.Codes.GrossVehicleWeight,
				ABIUnitOfMeasureList.Codes.Hertz,
				ABIUnitOfMeasureList.Codes.Hundreds,
				ABIUnitOfMeasureList.Codes.InternalRevenueCode,
				ABIUnitOfMeasureList.Codes.Kilograms,
				ABIUnitOfMeasureList.Codes.Kilohertz,
				ABIUnitOfMeasureList.Codes.Kilonewtons,
				ABIUnitOfMeasureList.Codes.Kilopascal,
				ABIUnitOfMeasureList.Codes.KilovoltAmperes,
				ABIUnitOfMeasureList.Codes.KilowattHours,
				ABIUnitOfMeasureList.Codes.Kilowatts,
				ABIUnitOfMeasureList.Codes.Linear,
				ABIUnitOfMeasureList.Codes.LinearMeters,
				ABIUnitOfMeasureList.Codes.Liters,
				ABIUnitOfMeasureList.Codes.LongTon2240LbWgt,
				ABIUnitOfMeasureList.Codes.Megabecquerel,
				ABIUnitOfMeasureList.Codes.Megahertz,
				ABIUnitOfMeasureList.Codes.Megapascal,
				ABIUnitOfMeasureList.Codes.Meters,
				ABIUnitOfMeasureList.Codes.MetricTon,
				ABIUnitOfMeasureList.Codes.Millicurie,
				ABIUnitOfMeasureList.Codes.Milligram,
				ABIUnitOfMeasureList.Codes.Milliliter,
				ABIUnitOfMeasureList.Codes.Millimeters,
				ABIUnitOfMeasureList.Codes.NoUnitRequired,
				ABIUnitOfMeasureList.Codes.NumberOfJewels,
				ABIUnitOfMeasureList.Codes.OuncesFluidVolume,
				ABIUnitOfMeasureList.Codes.OuncesTroyAPOTHWgt,
				ABIUnitOfMeasureList.Codes.OuncesWeightAvdp,
				ABIUnitOfMeasureList.Codes.OzoneDepletionEquivalent,
				ABIUnitOfMeasureList.Codes.PintsLiquidUsVolume,
				ABIUnitOfMeasureList.Codes.Pounds,
				ABIUnitOfMeasureList.Codes.Proof,
				ABIUnitOfMeasureList.Codes.ProofGallon,
				ABIUnitOfMeasureList.Codes.ProofLiter,
				ABIUnitOfMeasureList.Codes.QuartsLiquidUsVolume,
				ABIUnitOfMeasureList.Codes.RevolutionsPerMinute,
				ABIUnitOfMeasureList.Codes.ShortTon2000LbWeight,
				ABIUnitOfMeasureList.Codes.SqFeetArea,
				ABIUnitOfMeasureList.Codes.SqInchesArea,
				ABIUnitOfMeasureList.Codes.Square,
				ABIUnitOfMeasureList.Codes.SquareCentimeters,
				ABIUnitOfMeasureList.Codes.SquareMeters,
				ABIUnitOfMeasureList.Codes.SqYardsArea,
				ABIUnitOfMeasureList.Codes.StandardBrickEquivalent,
				ABIUnitOfMeasureList.Codes.SuppositoriesDosage,
				ABIUnitOfMeasureList.Codes.TabletsDosage,
				ABIUnitOfMeasureList.Codes.Thousand,
				ABIUnitOfMeasureList.Codes.ThousandCubicMeters,
				ABIUnitOfMeasureList.Codes.ThousandMeters,
				ABIUnitOfMeasureList.Codes.ThousandSquareMeters,
				ABIUnitOfMeasureList.Codes.ThousandStandardBrick,
				ABIUnitOfMeasureList.Codes.UsVolume,
				ABIUnitOfMeasureList.Codes.Volts,
				ABIUnitOfMeasureList.Codes.Watts,
				ABIUnitOfMeasureList.Codes.Weight,
				ABIUnitOfMeasureList.Codes.WineGallon,
				ABIUnitOfMeasureList.Codes.WineLiter,
				ABIUnitOfMeasureList.Codes.YardsLength
			})
			{
				AssertEquals(code, false, ABIUnitOfMeasureList.IsPackageType(code));
				list.RemoveCode(code);
			}

			if (list.Count > 0)
			{
				Assert("These codes are have not been defined as Package Type or not: " + list.CodesAsString, false);
			}
		}
	}
}
