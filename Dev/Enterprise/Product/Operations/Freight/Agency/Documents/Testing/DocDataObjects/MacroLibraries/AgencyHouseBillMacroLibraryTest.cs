using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class AgencyHouseBillMacroLibraryTest : TestCaseWithFactory
	{
		#region DangerousGoodsDescription

		public void TestDangerousGoodsDescription()
		{
			var packingLine = CreatePackingLineWithDangerousGoods();
			var expr = "DangerousGoodsDescription".With<AgencyHouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(packingLine) is string result)
			{
				AssertMultilineASCIIEquals(@"001A, 1.1, Box
002B, TechnicalName, 2.2, (11.0C c.c.)
003C, Pallet", result);
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		#endregion

		#region DangerousGoodDescription

		public void TestDangerousGoodDescription()
		{
			var packingLine = CreatePackingLineWithDangerousGoods();
			var expr = "DangerousGoodDescription".With<AgencyHouseBillMacroLibrary>().CreateExpression();

			AssertEquals("001A, 1.1, Box", expr.Evaluate(packingLine.DangerousGoods.First(x => x.Code == "001A")).ToString());
			AssertEquals("002B, TechnicalName, 2.2, (11.0C c.c.)", expr.Evaluate(packingLine.DangerousGoods.First(x => x.Code == "002B")).ToString());
			AssertEquals("003C, Pallet", expr.Evaluate(packingLine.DangerousGoods.First(x => x.Code == "003C")).ToString());
		}

		#endregion

		#region PrintSignature

		public void TestPrintSignature()
		{
			var expr = "PrintSignature".With<AgencyHouseBillMacroLibrary>().CreateExpression();

			using (AgencyRegistry.Instance.PrintSignature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				if (expr.Evaluate() is bool result)
				{
					Assert(result);
				}
				else
				{
					Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
				}
			}

			using (AgencyRegistry.Instance.PrintSignature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				if (expr.Evaluate() is bool result)
				{
					Assert(!result);
				}
				else
				{
					Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
				}
			}
		}

		#endregion

		#region ShowPacklineDetailsOnBillsOfLading

		public void TestShowPacklineDetailsOnBillsOfLading()
		{
			var expr = "ShowPacklineDetailsOnBillsOfLading".With<AgencyHouseBillMacroLibrary>().CreateExpression();

			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				if (expr.Evaluate() is bool result)
				{
					Assert(result);
				}
				else
				{
					Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
				}
			}

			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				if (expr.Evaluate() is bool result)
				{
					Assert(!result);
				}
				else
				{
					Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
				}
			}
		}

		#endregion

		#region ConvertMeasurement

		public void TestConvertMeasurement()
		{
			var context = new CommonContext(Factory);

			var weightExpr = "ConvertTo(\"KG\")".With<AgencyHouseBillMacroLibrary>().CreateExpression();
			var weight = new Measurement
			{
				Value = 1000,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Grams
				}
			};

			if (weightExpr.Evaluate(weight) is IMeasurement weightResult)
			{
				AssertMultilineASCIIEquals("1.00 KG", weightResult.ToString());
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{weightExpr.ToFormatString()}");
			}

			var volumeExpr = "ConvertTo(\"M3\")".With<AgencyHouseBillMacroLibrary>().CreateExpression();
			var volume = new Measurement
			{
				Value = 1000,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = Core.Constants.Volume.Litre
				}
			};

			if (volumeExpr.Evaluate(volume) is IMeasurement volumeResult)
			{
				AssertMultilineASCIIEquals("1.00 M3", volumeResult.ToString());
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{volumeExpr.ToFormatString()}");
			}
		}

		#endregion

		#region Implementation

		PackingLine CreatePackingLineWithDangerousGoods()
		{
			var packingLine = new PackingLine("zzz");
			var context = new CommonContext(Factory);
			packingLine.DangerousGoods = new[]
			{
				new DangerousGood
				{
					Code = "001A",
					Unno = "001",
					Variant = "A",
					ProperShippingName = "Nitroglycerin",
					IMOClass = "1.1",
					PackingGroup = "Z",
					Quantity = 22,
					PackageType = new CodeDescription(GetPackTypeCodeDescription())
					{
						Code = "BOX"
					}
				},
				new DangerousGood
				{
					Code = "002B",
					Unno = "002",
					Variant = "B",
					ProperShippingName = "Methanol",
					IMOClass = "2.2",
					FlashPoint = new Measurement
					{
						Value = 11.0234567,
						Unit = new CodeDescription(context.TemperatureUnits)
						{
							Code = Enterprise.Core.Constants.Temperature.Centigrade
						}
					},
					TechnicalName = "TechnicalName"
				},
				new DangerousGood
				{
					Code = "003C",
					Unno = "003",
					Variant = "C",
					ProperShippingName = "Sulfur",
					PackingGroup = "B",
					Quantity = 11,
					PackageType = new CodeDescription(GetPackTypeCodeDescription())
					{
						Code = "PLT"
					}
				}
			};

			return packingLine;
		}

		CodeDescriptionPairList GetPackTypeCodeDescription()
		{
			var res = new CodeDescriptionPairList();
			res.AddPair("PLT", "Pallet");
			res.AddPair("BOX", "Box");
			res.AddPair("BLT", "Bottle");
			return res;
		}

		#endregion
	}
}
