using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using ICodeDescription = CargoWise.Integration.ICodeDescription;
using IDocument = Enterprise.DocumentVisualizer.DocDataObjects.IDocument;
using IPage = Enterprise.DocumentVisualizer.DocDataObjects.IPage;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class HouseBillMacroLibraryTest : TestCaseWithFactory
	{
		#region TestAsReferenceNumbersOfType

		public void TestAsReferenceNumbersOfType()
		{
			var arr = new object[]
			{
				111,
				222,
				333
			};

			var expr = "AsReferenceNumbersOfType(\"ZZZ\")".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(arr) is IEnumerable<IReferenceNumber> numbers)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertContainsExactElementsInAnyOrder("created reference numbers numbers",
					new[]
					{
						"ZZZ|111",
						"ZZZ|222",
						"ZZZ|333"
					},
					numbers.Select(n => string.Concat(n.Type.Code, "|", n.Value)));
			}
			else
			{
				Fail($"Macro did not create reference numbers. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		#endregion

		#region TestGenerateReferenceNumber

		public void TestGenerateReferenceNumber()
		{
			var numberTypes = new CodeDescriptionPairList();
			numberTypes.AddPair("AAA", "AAA desc");
			numberTypes.AddPair("BBB", "BBB desc");
			numberTypes.AddPair("CCC", "CCC desc");
			numberTypes.AddPair("DDD", "DDD desc");

			var numbers = new object[]
			{
				CreateReferenceNumber("AAA", "1111", numberTypes),
				CreateReferenceNumber("BBB", "2222", numberTypes),
				CreateReferenceNumber("AAA", "3333", numberTypes),
				CreateReferenceNumber("CCC", "4444", numberTypes).MakeDynamic(),
				CreateReferenceNumber("DDD", "5555", numberTypes),
				CreateReferenceNumber("AAA", "6666", numberTypes).MakeDynamic(),
				CreateReferenceNumber("DDD", "7777", numberTypes),
			};

			var expr = "let referenceNumberLabels = {AAA: \"CLIENT REF: \", BBB: \"SHIPPER REF: \", CCC: \"CONSIGNEE REF: \", DDD: \"ORDER REF: \"}; @data.GenerateReferenceNumber(@referenceNumberLabels, 3, 30)".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(numbers) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertMultilineASCIIEquals("generated reference number",
@"CLIENT REF: 1111, 3333, 6666
SHIPPER REF: 2222
CONSIGNEE REF: 4444", result);
			}
			else
			{
				Fail($"Macro did not create reference number. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestGenerateReferenceNumber_OnEmptySet()
		{
			var numberTypes = new CodeDescriptionPairList();
			numberTypes.AddPair("AAA", "AAA desc");
			numberTypes.AddPair("BBB", "BBB desc");
			numberTypes.AddPair("CCC", "CCC desc");
			numberTypes.AddPair("DDD", "DDD desc");

			var numbers = Array.Empty<IReferenceNumber>();

			var expr = "let referenceNumberLabels = {AAA: \"CLIENT REF: \", BBB: \"SHIPPER REF: \", CCC: \"CONSIGNEE REF: \", DDD: \"ORDER REF: \"}; @data.GenerateReferenceNumber(@referenceNumberLabels, 3, 30)".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(numbers) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertMultilineASCIIEquals("generated reference number",
					"", result);
			}
			else
			{
				Fail($"Macro did not create reference number. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestGenerateReferenceNumber_AllNumbersDontFitInLine()
		{
			var numberTypes = new CodeDescriptionPairList();
			numberTypes.AddPair("AAA", "AAA desc");
			numberTypes.AddPair("BBB", "BBB desc");

			var numbers = new[]
			{
				CreateReferenceNumber("AAA", "1111", numberTypes),
				CreateReferenceNumber("AAA", "2222", numberTypes),
				CreateReferenceNumber("AAA", "3333", numberTypes),
				CreateReferenceNumber("BBB", "4444", numberTypes),
			};

			var expr = "let referenceNumberLabels = {AAA: \"CLIENT REF: \", BBB: \"SHIPPER REF: \"}; @data.GenerateReferenceNumber(@referenceNumberLabels, 3, 27)".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(numbers) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertMultilineASCIIEquals("generated reference number",
@"CLIENT REF: 1111, 2222
SHIPPER REF: 4444", result);
			}
			else
			{
				Fail($"Macro did not create reference number. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		IReferenceNumber CreateReferenceNumber(string numberType, string number, ICodeDescriptionPairList numberTypes)
		{
			return new ReferenceNumber
			{
				Type = new CodeDescription(numberTypes)
				{
					Code = numberType
				},
				Value = number
			};
		}

		#endregion

		#region TestLibraryIsRegesteredInObjectFactory

		public void TestLibraryIsRegesteredInObjectFactory()
		{
			var lib = ObjectFactory.Get<IHouseBillMacroLibrary>();

			Assert("HouseBillMacroLibrary is registered on ObjectFactory", lib is IMacroLibrary);
		}

		#endregion

		#region TestDangerousGoodsDescription

		public void TestDangerousGoodsDescription_DoNotShowVariant()
		{
			var packingLine = CreatePackingLineWithDangerousGoods();
			var expr = "DangerousGoodsDescription".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(packingLine) is string result)
			{
				AssertMultilineASCIIEquals("generated reference number",
@"UN001, Nitroglycerin, class 1.1, PG Z
UN002, Methanol, class 2.2, (11.0C c.c.)
UN003, Sulfur, PG B", result);
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestDangerousGoodsDescription_PacklineWithNoDangerousGoods()
		{
			var packingLine = new PackingLine("zzz", Factory);
			var expr = "DangerousGoodsDescription".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(packingLine) is string result)
			{
				AssertEquals(string.Empty, result);
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestDangerousGoodsDescription_ShowTechnicalNameAndPackedInLimitedQuantity()
		{
			var packingLine = CreatePackingLineWithDangerousGoods();
			var dgs = packingLine.DangerousGoods.ToArray();
			dgs[0].PackedInLimitedQuantity = true;
			dgs[1].PackedInLimitedQuantity = true;
			dgs[1].TechnicalName = "T0002";
			dgs[2].TechnicalName = "T0003";

			var expr = "DangerousGoodsDescription".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(packingLine) is string result)
			{
				AssertMultilineASCIIEquals("generated reference number",
@"UN001, Nitroglycerin, class 1.1, PG Z, LTD QTY
UN002, Methanol, (T0002), class 2.2, (11.0C c.c.), LTD QTY
UN003, Sulfur, (T0003), PG B", result);
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestDangerousGoodsDescription_ContainsRadioactiveMaterialForLimitedQuantity_WhenStandardIsCFRAndOneOtherClassIsSeven()
		{
			var packingLine = CreatePackingLineWithCFRDangerousGoods();
			var dgs = packingLine.DangerousGoods.ToArray();
			dgs[0].PackedInLimitedQuantity = true;

			var expr = "DangerousGoodsDescription".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(packingLine) is string result)
			{
				AssertContains("generated reference number", "UN3507, class 6.1, PG Z, Limited quantity radioactive material", result);
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		PackingLine CreatePackingLineWithCFRDangerousGoods()
		{
			var packingLine = new PackingLine("zzz", Factory);
			var context = new CommonContext(Factory);
			packingLine.DangerousGoods = new[]
			{
				new DangerousGood
				{
					Code = "3507",
					Unno = "3507",
					Variant = "A",
					IMOClass = "6.1",
					Standard = "CFR",
					PackingGroup = "Z",
					SecondaryClass = "7",
				}
			};

			return packingLine;
		}

		PackingLine CreatePackingLineWithDangerousGoods()
		{
			var packingLine = new PackingLine("zzz", Factory);
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
					PackingGroup = "Z"
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
					}
				},
				new DangerousGood
				{
					Code = "003C",
					Unno = "003",
					Variant = "C",
					ProperShippingName = "Sulfur",
					PackingGroup = "B"
				}
			};

			return packingLine;
		}

		#endregion

		#region TestPackType/TestPackTypeDescription

		public void TestPackType_EmptyPackType()
		{
			var expr = "PackTypeCode".With<HouseBillMacroLibrary>().CreateExpression();

			var data = Array.Empty<IPackingLine>();

			if (expr.Evaluate(data) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertEquals("generated pack type code", string.Empty, result);
			}
			else
			{
				Fail($"Macro did not return PackTypeCode. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestPackType_SamePackTypes_PackingLines() => AssertPackType_SamePackTypes_DynamicData(false, GetPackingLinesForTest_SamePackTypes());
		public void TestPackType_SamePackTypes_PackTypes() => AssertPackType_SamePackTypes_DynamicData(false, GetPackingTypesForTest_SamePackTypes());

		public void TestPackType_SamePackTypes_PackingLines_DynamicData() => AssertPackType_SamePackTypes_DynamicData(true, GetPackingLinesForTest_SamePackTypes());
		public void TestPackType_SamePackTypes_PackTypes_DynamicData() => AssertPackType_SamePackTypes_DynamicData(true, GetPackingTypesForTest_SamePackTypes());

		void AssertPackType_SamePackTypes_DynamicData(bool useDynamicData, object dataSource)
		{
			var expr = "PackTypeDescription".With<HouseBillMacroLibrary>().CreateExpression();

			var data = useDynamicData
				? dataSource.MakeDynamic()
				: dataSource;

			if (expr.Evaluate(data) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertEquals("generated pack type code", "Box", result);
			}
			else
			{
				Fail($"Macro did not return PackTypeDescription. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestPackType_VariousPackTypes_PackingLines() => AssertPackType_VariousPackTypes(false, GetPackingLinesForTest_VariousPackTypes());
		public void TestPackType_VariousPackTypes_PackTypes() => AssertPackType_VariousPackTypes(false, GetPackingTypesForTest_VariousPackTypes());

		public void TestPackType_VariousPackTypes_PackingLines_DynamicData() => AssertPackType_VariousPackTypes(true, GetPackingLinesForTest_VariousPackTypes());
		public void TestPackType_VariousPackTypes_PackTypes_DynamicData() => AssertPackType_VariousPackTypes(true, GetPackingTypesForTest_VariousPackTypes());

		public void AssertPackType_VariousPackTypes(bool useDynamicData, object dataSource)
		{
			var expr = "PackTypeCode".With<HouseBillMacroLibrary>().CreateExpression();

			var data = useDynamicData
				? dataSource.MakeDynamic()
				: dataSource;

			if (expr.Evaluate(data) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertEquals("generated pack type code", "PKG", result);
			}
			else
			{
				Fail($"Macro did not return PackTypeCode. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestPackTypeDescription_EmptyPackType()
		{
			var expr = "PackTypeDescription".With<HouseBillMacroLibrary>().CreateExpression();

			var data = Array.Empty<IPackingLine>();

			if (expr.Evaluate(data) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertEquals("generated pack type description", string.Empty, result);
			}
			else
			{
				Fail($"Macro did not return PackTypeDescription. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestPackTypeDescription_SamePackTypes_PackingLines() => AssertPackTypeDescription_SamePackTypes_DynamicData(false, GetPackingLinesForTest_SamePackTypes());
		public void TestPackTypeDescription_SamePackTypes_PackTypes() => AssertPackTypeDescription_SamePackTypes_DynamicData(false, GetPackingTypesForTest_SamePackTypes());

		public void TestPackTypeDescription_SamePackTypes_PackingLines_DynamicData() => AssertPackTypeDescription_SamePackTypes_DynamicData(true, GetPackingLinesForTest_SamePackTypes());
		public void TestPackTypeDescription_SamePackTypes_PackTypes_DynamicData() => AssertPackTypeDescription_SamePackTypes_DynamicData(true, GetPackingTypesForTest_SamePackTypes());

		public void AssertPackTypeDescription_SamePackTypes_DynamicData(bool useDynamicData, object dataSource)
		{
			var expr = "PackTypeDescription".With<HouseBillMacroLibrary>().CreateExpression();

			var data = useDynamicData
				? dataSource.MakeDynamic()
				: dataSource;

			if (expr.Evaluate(data) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertEquals("generated pack type description", "Box", result);
			}
			else
			{
				Fail($"Macro did not return PackTypeDescription. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		public void TestPackTypeDescription_PackingLines_PackTypes_VariousPackTypes() => AssertPackTypeDescription_VariousPackTypes(false, GetPackingLinesForTest_VariousPackTypes());
		public void TestPackTypeDescription_PackTypes_VariousPackTypes() => AssertPackTypeDescription_VariousPackTypes(false, GetPackingLinesForTest_VariousPackTypes());

		public void TestPackTypeDescription_PackingLines_VariousPackTypes_DynamicData() => AssertPackTypeDescription_VariousPackTypes(true, GetPackingLinesForTest_VariousPackTypes());
		public void TestPackTypeDescription_PackTypes_VariousPackTypes_DynamicData() => AssertPackTypeDescription_VariousPackTypes(true, GetPackingLinesForTest_VariousPackTypes());

		public void AssertPackTypeDescription_VariousPackTypes(bool useDynamicData, object dataSource)
		{
			var expr = "PackTypeDescription".With<HouseBillMacroLibrary>().CreateExpression();

			var data = useDynamicData
				? dataSource.MakeDynamic()
				: dataSource;

			if (expr.Evaluate(data) is string result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertEquals("generated pack type description", "Package", result);
			}
			else
			{
				Fail($"Macro did not return PackTypeDescription. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		PackingLine[] GetPackingLinesForTest_SamePackTypes()
		{
			var packTypeList = GetPackTypeCodeDescription();

			return new[]
			{
				new PackingLine("aaa", Factory)
				{
					PackageType = new CodeDescription(packTypeList)
					{
						Code = "BOX"
					}
				},
				new PackingLine("bbb", Factory)
				{
					PackageType = new CodeDescription(packTypeList)
					{
						Code = "BOX"
					}
				},
				new PackingLine("ccc", Factory)
				{
					PackageType = new CodeDescription(packTypeList)
					{
						Code = "BOX"
					}
				}
			};
		}

		PackingLine[] GetPackingLinesForTest_VariousPackTypes()
		{
			var packTypeList = GetPackTypeCodeDescription();

			return new[]
			{
				new PackingLine("aaa", Factory)
				{
					PackageType = new CodeDescription(packTypeList)
					{
						Code = "PLT"
					}
				},
				new PackingLine("bbb", Factory)
				{
					PackageType = new CodeDescription(packTypeList)
					{
						Code = "BOX"
					}
				},
				new PackingLine("ccc", Factory)
				{
					PackageType = new CodeDescription(packTypeList)
					{
						Code = "BLT"
					}
				}
			};
		}

		ICodeDescription[] GetPackingTypesForTest_SamePackTypes()
		{
			var packTypeList = GetPackTypeCodeDescription();

			return new[]
			{
				new CodeDescription(packTypeList)
				{
					Code = "BOX"
				},
				new CodeDescription(packTypeList)
				{
					Code = "BOX"
				},
				new CodeDescription(packTypeList)
				{
					Code = "BOX"
				}
			};
		}

		ICodeDescription[] GetPackingTypesForTest_VariousPackTypes()
		{
			var packTypeList = GetPackTypeCodeDescription();

			return new[]
			{
				new CodeDescription(packTypeList)
				{
					Code = "PLT"
				},
				new CodeDescription(packTypeList)
				{
					Code = "BOX"
				},
				new CodeDescription(packTypeList)
				{
					Code = "BLT"
				}
			};
		}

		#endregion

		#region TestIsLoadingIn

		public void TestIsLoadingIn()
		{
			AssertIsLoadingIn(new[]
				{
					"AU",
					"US"
				},
				"US",
				true);

			AssertIsLoadingIn(new[]
				{
					"AUSYD",
					"USCHI"
				},
				"USCHI",
				true);

			AssertIsLoadingIn(new[]
				{
					"AU",
					"US"
				},
				"NZ",
				false);

			AssertIsLoadingIn(new[]
				{
					"AUSYD",
					"USCHI"
				},
				"NZAKL",
				false);
		}

		void AssertIsLoadingIn(string[] locationCodes, string searchedLocationCode, bool expected)
		{
			var expr = $"IsLoadingIn(\"{searchedLocationCode}\")".With<HouseBillMacroLibrary>().CreateExpression();

			var transports = new List<DummyTransport>();

			foreach (var countryCode in locationCodes)
			{
				var portOfLoading = new DummyUnloco();

				if (countryCode.Length > 2)
				{
					portOfLoading.Code = countryCode;
				}
				else
				{
					portOfLoading.Country = new DummyCountry
					{
						Code = countryCode
					};
				}

				var transport = new DummyTransport
				{
					PortOfLoading = portOfLoading
				};

				transports.Add(transport);
			}

			var data = transports.MakeDynamic();

			if (expr.Evaluate(data) is bool result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertEquals($"Searching for {searchedLocationCode}", expected, result);
			}
			else
			{
				Fail($"Macro should return true/false. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		#endregion

		#region TestIsDischargingIn

		public void TestIsDischargingIn()
		{
			AssertIsDischargingIn(new[]
				{
					"AU",
					"US"
				},
				"US",
				true);

			AssertIsDischargingIn(new[]
				{
					"AUSYD",
					"USCHI"
				},
				"USCHI",
				true);

			AssertIsDischargingIn(new[]
				{
					"AU",
					"US"
				},
				"NZ",
				false);

			AssertIsDischargingIn(new[]
				{
					"AUSYD",
					"USCHI"
				},
				"NZAKL",
				false);
		}

		void AssertIsDischargingIn(string[] locationCodes, string searchedLocationCode, bool expected)
		{
			var expr = $"IsDischargingIn(\"{searchedLocationCode}\")".With<HouseBillMacroLibrary>().CreateExpression();

			var transports = new List<DummyTransport>();

			foreach (var countryCode in locationCodes)
			{
				var portOfDischarge = new DummyUnloco();

				if (countryCode.Length > 2)
				{
					portOfDischarge.Code = countryCode;
				}
				else
				{
					portOfDischarge.Country = new DummyCountry
					{
						Code = countryCode
					};
				}

				var transport = new DummyTransport
				{
					PortOfDischarge = portOfDischarge
				};

				transports.Add(transport);
			}

			var data = transports.MakeDynamic();

			if (expr.Evaluate(data) is bool result)
			{
				AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

				AssertEquals($"Searching for {searchedLocationCode}", expected, result);
			}
			else
			{
				Fail($"Macro should return true/false. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		#endregion

		#region TestHasFollowOnPage

		public void TestHasFollowOnPage_True()
		{
			var expr = "HasFollowOnPage".With<HouseBillMacroLibrary>().CreateExpression();

			var doc = new Mock<IDocument>();
			var firstPage = new Mock<IPage>();
			var followOnPage = new Mock<IPage>();

			firstPage
				.SetupGet(p => p.Name)
				.Returns(HouseBillPageNames.FirstPage);

			followOnPage
				.SetupGet(p => p.Name)
				.Returns($"{HouseBillPageNames.FollowOn} 1");

			doc
				.Setup(d => d.Pages)
				.Returns(new[]
				{
					firstPage.Object,
					followOnPage.Object
				});

			using (var scope = new MacroScope())
			{
				scope.SetVariable(VariableNames.Document, doc.Object);

				if (expr.Evaluate(scope) is bool result)
				{
					AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

					AssertEquals($"Expected to correctly determine whether the document has follow on page", true, result);
				}
				else
				{
					Fail($"Macro should return true/false. Errors:\r\n{expr.ToFormatString()}");
				}
			}
		}

		public void TestHasFollowOnPage_False()
		{
			var expr = "HasFollowOnPage".With<HouseBillMacroLibrary>().CreateExpression();

			var doc = new Mock<IDocument>();
			var firstPage = new Mock<IPage>();
			var termsAndConditionsPage = new Mock<IPage>();

			firstPage
				.SetupGet(p => p.Name)
				.Returns(HouseBillPageNames.FirstPage);

			termsAndConditionsPage
				.SetupGet(p => p.Name)
				.Returns(HouseBillPageNames.TermsAndConditions);

			doc
				.Setup(d => d.Pages)
				.Returns(new[]
				{
					firstPage.Object,
					termsAndConditionsPage.Object
				});

			using (var scope = new MacroScope())
			{
				scope.SetVariable(VariableNames.Document, doc);

				if (expr.Evaluate(scope) is bool result)
				{
					AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

					AssertEquals($"Expected to correctly determine whether the document has follow on page", false, result);
				}
				else
				{
					Fail($"Macro should return true/false. Errors:\r\n{expr.ToFormatString()}");
				}
			}
		}

		#endregion

		#region PrintSignature

		public void TestPrintSignature_True() => AssertPrintSignature(true);
		public void TestPrintSignature_False() => AssertPrintSignature(false);

		void AssertPrintSignature(bool printSignature)
		{
			using (FreightDataRegistry.Instance.PrintSignatureForHBLDocuments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, printSignature))
			{
				var expr = "PrintSignature".With<HouseBillMacroLibrary>().CreateExpression();

				var houseBill = new DocDataObjects.HouseBill("ForwardingShipment", "S00012");

				if (expr.Evaluate(houseBill) is bool result)
				{
					AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

					AssertEquals($"PrintSignature", printSignature, result);
				}
				else
				{
					Fail($"Macro should return true/false. Errors:\r\n{expr.ToFormatString()}");
				}
			}
		}

		#endregion

		#region FollowOnPageNumber

		public void TestFollowOnPageNumber()
		{
			var expr = "FollowOnPageNumber".With<HouseBillMacroLibrary>().CreateExpression();

			var doc = new Mock<DocumentVisualizer.Core.IDocument>();
			var firstPage = new Mock<DocumentVisualizer.Core.IPage>();
			var followOnPage1 = new Mock<DocumentVisualizer.Core.IPage>();
			var followOnPage2 = new Mock<DocumentVisualizer.Core.IPage>();

			firstPage
				.SetupGet(p => p.Name)
				.Returns(HouseBillPageNames.FirstPage);

			followOnPage1
				.SetupGet(p => p.Name)
				.Returns($"{HouseBillPageNames.FollowOn} 1");

			followOnPage2
				.SetupGet(p => p.Name)
				.Returns($"{HouseBillPageNames.FollowOn} 2");

			doc
				.SetupGet(d => d.Pages)
				.Returns(new[]
				{
					firstPage.Object,
					followOnPage1.Object,
					followOnPage2.Object
				});

			using (var scope = new MacroScope())
			{
				scope.SetVariable(VariableNames.DocumentInternal, doc.Object);
				scope.SetVariable(VariableNames.PageInternal, followOnPage2.Object);

				if (expr.Evaluate(scope) is int result)
				{
					AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

					AssertEquals($"Expected to correctly determine the number of the follow on page", 2, result);
				}
				else
				{
					Fail($"Macro should return int number. Errors:\r\n{expr.ToFormatString()}");
				}
			}
		}

		#endregion

		#region TotalFollowOnPages

		public void TestTotalFollowOnPages()
		{
			var expr = "TotalFollowOnPages".With<HouseBillMacroLibrary>().CreateExpression();
			var doc = new Mock<DocumentVisualizer.Core.IDocument>();
			var firstPage = new Mock<DocumentVisualizer.Core.IPage>();
			var followOnPage1 = new Mock<DocumentVisualizer.Core.IPage>();
			var followOnPage2 = new Mock<DocumentVisualizer.Core.IPage>();
			var followOnPage3 = new Mock<DocumentVisualizer.Core.IPage>();

			firstPage
				.SetupGet(p => p.Name)
				.Returns(HouseBillPageNames.FirstPage);

			followOnPage1
				.SetupGet(p => p.Name)
				.Returns($"{HouseBillPageNames.FollowOn} 1");

			followOnPage2
				.SetupGet(p => p.Name)
				.Returns($"{HouseBillPageNames.FollowOn} 2");

			followOnPage3
				.SetupGet(p => p.Name)
				.Returns($"{HouseBillPageNames.FollowOn} 3");

			doc
				.SetupGet(d => d.Pages)
				.Returns(new[]
				{
					firstPage.Object,
					followOnPage1.Object,
					followOnPage2.Object,
					followOnPage3.Object
				});

			using (var scope = new MacroScope())
			{
				scope.SetVariable(VariableNames.DocumentInternal, doc.Object);
				scope.SetVariable(VariableNames.PageInternal, followOnPage2.Object);

				if (expr.Evaluate(scope) is int result)
				{
					AssertMultilineASCIIEquals("expr had no errors", "", expr.ToFormatString());

					AssertEquals($"Expected to correctly determine the total number of follow on pages", 3, result);
				}
				else
				{
					Fail($"Macro should return int number. Errors:\r\n{expr.ToFormatString()}");
				}
			}
		}

		#endregion

		#region TestFIATALogo

		public void TestFIATALogo_Authorised()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var expr = "FIATALogo".With<HouseBillMacroLibrary>().CreateExpression();

				using (var scope = new MacroScope(new DocDataObjects.HouseBill("ForwardingShipment", "S00012")))
				{
					var res = expr.Evaluate(scope);

					AssertMultilineASCIIEquals("expected no evaluation errors", string.Empty, expr.ToFormatString());
					Assert("found image", res is Image);
				}
			}
		}

		public void TestFIATALogo_Unauthorised()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var expr = "FIATALogo".With<HouseBillMacroLibrary>().CreateExpression();

				using (var scope = new MacroScope(new DocDataObjects.HouseBill("ForwardingShipment", "S00012")))
				{
					var res = expr.Evaluate(scope);

					AssertMultilineASCIIEquals("expected no evaluation errors", string.Empty, expr.ToFormatString());
					AssertNull("no image found", res);
				}
			}
		}

		#endregion

		#region FIATALogoText

		public void TestFIATALogoText_Authorised()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var expr = "FIATALogoText".With<HouseBillMacroLibrary>().CreateExpression();

				using (var scope = new MacroScope(new DocDataObjects.HouseBill("ForwardingShipment", "S00012")))
				{
					var res = expr.Evaluate(scope);

					AssertMultilineASCIIEquals("expected no evaluation errors", string.Empty, expr.ToFormatString());
					Assert("found image", res is Image);
				}
			}
		}

		public void TestFIATALogoText_Unauthorised()
		{
			using (FreightDataRegistry.Instance.FIATAAuthorised.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var expr = "FIATALogoText".With<HouseBillMacroLibrary>().CreateExpression();

				using (var scope = new MacroScope(new DocDataObjects.HouseBill("ForwardingShipment", "S00012")))
				{
					var res = expr.Evaluate(scope);

					AssertMultilineASCIIEquals("expected no evaluation errors", string.Empty, expr.ToFormatString());
					Assert("found image", res is Image);
				}
			}
		}

		#endregion

		#region MoneyToWords

		public void TestMoneyToWords()
		{
			var expr = "MoneyToWords".With<HouseBillMacroLibrary>().CreateExpression();

			var money = new DummyMoney
			{
				Amount = 123,
				Currency = new DummyCodeDescription
				{
					Code = "AUD",
					Description = "Australian Dollar"
				}
			};

			var res = expr.Evaluate(money);

			AssertEquals("Money ToWords Conversion", "one hundred and twenty three dollars", res);
		}

		#endregion

		#region TestStatementOfApprovalForCFR

		public void TestStatementOfApprovalForCFR()
		{
			var container = new Container();
			var packingLines = new List<PackingLine>();
			var packingLine1 = CreatePackingLineWithCFRDangerousGoods();
			packingLine1.DangerousGoods.Cast<DangerousGood>().ToList()[0].ProperShippingName = "Excepted Package 1";

			var packingLine2 = CreatePackingLineWithCFRDangerousGoods();
			packingLine2.DangerousGoods.Cast<DangerousGood>().ToList()[0].ProperShippingName = "ExCePtEd PaCkAgE 2";

			packingLines.Add(packingLine1);
			packingLines.Add(packingLine2);
			container.PackingLines = packingLines;

			var houseBill = new DocDataObjects.HouseBill("ForwardingShipment", "S00012");
			houseBill.Containers = new[] { container };

			var expr = "StatementOfApprovalForCFR".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(houseBill) is string result)
			{
				AssertContains("Contains Statement Of Approval", "This is to certify that the above-named/herein-named materials are properly classified, described, packaged, marked and labeled, and are in proper condition for transportation according to the applicable regulations of the Department of Transportation.", result);
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		#endregion

		#region TestAdditionalHandlingInformationForCFR

		public void TestAdditionalHandlingInformationForCFR()
		{
			var container = new Container();
			var packingLines = new List<PackingLine>();
			var packingLine1 = CreatePackingLineWithCFRDangerousGoods();
			packingLine1.DangerousGoods.Cast<DangerousGood>().ToList()[0].ProperShippingName = "Excepted Package 1";

			var packingLine2 = CreatePackingLineWithCFRDangerousGoods();
			packingLine2.DangerousGoods.Cast<DangerousGood>().ToList()[0].ProperShippingName = "ExCePtEd PaCkAgE 2";

			packingLines.Add(packingLine1);
			packingLines.Add(packingLine2);
			container.PackingLines = packingLines;

			var houseBill = new DocDataObjects.HouseBill("ForwardingShipment", "S00012");
			houseBill.Notes = new[]
			{
				new Note {
					Text = "abc",
					Description = PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description
				}
			};
			houseBill.Containers = new[] { container };

			var expr = "AdditionalHandlingInformationForCFR".With<HouseBillMacroLibrary>().CreateExpression();

			if (expr.Evaluate(houseBill) is string result)
			{
				AssertContains("Contains additional information note", "abc", result);
			}
			else
			{
				Fail($"Macro evaluation failed. Errors:\r\n{expr.ToFormatString()}");
			}
		}

		#endregion

		#region Implementation

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
