using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Constants;
using static CargoWise.RefDbRepo.PLReferenceData.Business.CusProcedure.Constants;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.CusProcedure
{
	[TestFixture]
	sealed class CusProcedureTest
	{
		[Test]
		public void TestGenerateRefCusProceduresFromRefCusCodeLists_NoCombinations()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate),
				GetNewRefCusProcedure("03", startDate, endDate)
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, startDate),
				GetNewRefCusCodeList("03", startDate, startDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, startDate),
				GetNewRefCusCodeList("03", startDate, startDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(new RefCusCodeList[] { }, dataProcedure, dataPreviousProcedure, new RefCusCodeList[] { }, isImport: false);

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestGenerateRefCusProceduresFromRefCusCodeLists_SomeCombinations()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var combinationStartDate = DateTime.Today.AddDays(1);
			var combinationEndDate = DateTime.Today.AddDays(1);
			var concessionStartDate = DateTime.Today.AddDays(-1);
			var concessionEndDate = DateTime.Today.AddDays(-1);
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate),
				GetNewRefCusProcedure("01", combinationEndDate, combinationEndDate, previousProcedureCode: "05", description: " - "),
				GetNewRefCusProcedure("01", concessionStartDate, concessionEndDate, previousProcedureCode: "05", concession: "A00"),
				GetNewRefCusProcedure("03", startDate, endDate),
				GetNewRefCusProcedure("03", combinationEndDate, combinationEndDate, previousProcedureCode: "04", description: " - "),
				GetNewRefCusProcedure("03", concessionStartDate, concessionEndDate, previousProcedureCode: "04", concession: "A00"),
				GetNewRefCusProcedure("03", combinationEndDate, combinationEndDate, previousProcedureCode: "05", description: " - "),
				GetNewRefCusProcedure("03", concessionStartDate, concessionEndDate, previousProcedureCode: "05", concession: "A00")
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0105", combinationStartDate, combinationEndDate),
				GetNewRefCusCodeList("0304", combinationStartDate, combinationEndDate),
				GetNewRefCusCodeList("0305", combinationStartDate, combinationEndDate)
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, startDate),
				GetNewRefCusCodeList("05", startDate, startDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, startDate),
				GetNewRefCusCodeList("03", startDate, startDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", concessionStartDate, concessionEndDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestGenerateRefCusProceduresFromRefCusCodeLists_NoConcessions()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - "),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate)
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, startDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, startDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, new RefCusCodeList[] { }, isImport: false);

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestGenerateRefCusProceduresFromRefCusCodeLists_NoPreviousProcedure()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, startDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, startDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, new RefCusCodeList[] { }, dataConcessions, isImport: false);

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestGenerateRefCusProceduresFromRefCusCodeLists_NoProcedure()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate)
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, startDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, startDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, new RefCusCodeList[] { }, dataPreviousProcedure, dataConcessions, isImport: false);

			Assert.AreEqual(0, result.Count, "0 entries should be added");
		}

		[Test]
		public void TestDescription()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, description: "desc proc 1"),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: "desc proc 1 - desc prev proc 1"),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", description: "desc concession 1"),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "B22", description: "desc concession 2"),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "05", description: "desc proc 1 - desc prev proc 2"),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "05", concession: "A00", description: "desc concession 1"),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "05", concession: "B22", description: "desc concession 2"),
				GetNewRefCusProcedure("03", startDate, endDate, description: "desc proc 2"),
				GetNewRefCusProcedure("03", startDate, endDate, previousProcedureCode: "04", description: "desc proc 2 - desc prev proc 1"),
				GetNewRefCusProcedure("03", startDate, endDate, previousProcedureCode: "04", concession: "A00", description: "desc concession 1"),
				GetNewRefCusProcedure("03", startDate, endDate, previousProcedureCode: "04", concession: "B22", description: "desc concession 2"),
				GetNewRefCusProcedure("03", startDate, endDate, previousProcedureCode: "05", description: "desc proc 2 - desc prev proc 2"),
				GetNewRefCusProcedure("03", startDate, endDate, previousProcedureCode: "05", concession: "A00", description: "desc concession 1"),
				GetNewRefCusProcedure("03", startDate, endDate, previousProcedureCode: "05", concession: "B22", description: "desc concession 2"),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate, "desc combination 1"),
				GetNewRefCusCodeList("0105", startDate, endDate, "desc combination 2"),
				GetNewRefCusCodeList("0304", startDate, endDate, "desc combination 3"),
				GetNewRefCusCodeList("0305", startDate, endDate, "desc combination 4")
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate, "desc prev proc 1"),
				GetNewRefCusCodeList("05", startDate, endDate, "desc prev proc 2")
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate, "desc proc 1"),
				GetNewRefCusCodeList("03", startDate, endDate, "desc proc 2")
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate, "desc concession 1"),
				GetNewRefCusCodeList("B22", startDate, endDate, "desc concession 2")
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestShipmentType()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate)
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestCalculatedDuty()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, shipmentType: ShipmentType.Export, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended, startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: false, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure}04", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts}04", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended}04", startDate, endDate)
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended, startDate, endDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			Assert.Multiple(() =>
			{
				Assert.AreEqual(expected.Length, result.Count, "amount");

				for (var i = 0; i < result.Count; i++)
				{
					var resultData = result[i];
					AssertRefCusProcedure(expected[i], resultData, $"{i}");
					Assert.AreEqual(resultData.ZZ6_CalculateDuty, resultData.ZZ6_CalculateVAT, $"{i} - CalculateVat should be the same as CalculateDuty");
				}
			});
		}

		[Test]
		public void TestLandedCost()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure}04", startDate, endDate),
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestIntoWarehouse()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, shipmentType: ShipmentType.Export, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, intoWarehouse: ApplicableXmlValue),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: true, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, landedCost: false, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, intoWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: false, calculateVat: false, intoWarehouse: ApplicableXmlValue),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure}04", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts}04", startDate, endDate),
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestOutOfWarehouse()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, description: " - ", shipmentType: ShipmentType.Export, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, concession: "A00", shipmentType: ShipmentType.Export, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, description: " - ", shipmentType: ShipmentType.Export, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, concession: "A00", shipmentType: ShipmentType.Export, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.EntryOfGoodsForAFreeZoneSubjectToTypeIIControls, description: " - ", shipmentType: ShipmentType.Export, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.EntryOfGoodsForAFreeZoneSubjectToTypeIIControls, concession: "A00", shipmentType: ShipmentType.Export, outOfWarehouse: ApplicableXmlValue),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.EntryOfGoodsForAFreeZoneSubjectToTypeIIControls, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfWarehouse: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.EntryOfGoodsForAFreeZoneSubjectToTypeIIControls, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfWarehouse: ApplicableXmlValue),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList($"0104", startDate, endDate),
				GetNewRefCusCodeList($"01{ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure}", startDate, endDate),
				GetNewRefCusCodeList($"01{ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts}", startDate, endDate),
				GetNewRefCusCodeList($"01{ProcedureCodes.EntryOfGoodsForAFreeZoneSubjectToTypeIIControls}", startDate, endDate),
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingOfGoodsUnderTheCustomsWarehousingProcedure, startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts, startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.EntryOfGoodsForAFreeZoneSubjectToTypeIIControls, startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate),
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate),
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestIsTemporaryImport()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, description: " - ", shipmentType: ShipmentType.Export, outOfTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, concession: "A00", shipmentType: ShipmentType.Export, outOfTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, shipmentType: ShipmentType.Export, intoTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, intoTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, intoTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, description: " - ", shipmentType: ShipmentType.Export, intoTemporaryImport: ApplicableXmlValue, outOfTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, concession: "A00", shipmentType: ShipmentType.Export, intoTemporaryImport: ApplicableXmlValue, outOfTemporaryImport: ApplicableXmlValue),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryImport: ApplicableXmlValue, outOfTemporaryImport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate, previousProcedureCode: ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryImport: ApplicableXmlValue, outOfTemporaryImport: ApplicableXmlValue),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate),
				GetNewRefCusCodeList($"01{ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission}", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission}04", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission}{ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission}", startDate, endDate),
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.PlacingOfGoodsUnderTemporaryAdmission, startDate, endDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestIsTemporaryExport()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, description: " - ", shipmentType: ShipmentType.Export, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, concession: "A00", shipmentType: ShipmentType.Export, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, description: " - ", shipmentType: ShipmentType.Export, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, concession: "A00", shipmentType: ShipmentType.Export, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, description: " - ", shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, concession: "A00", shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, description: " - ", shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, concession: "A00", shipmentType: ShipmentType.Export, intoTemporaryExport: ApplicableXmlValue, outOfTemporaryExport: ApplicableXmlValue),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue, outOfTemporaryExport: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoTemporaryExport: ApplicableXmlValue, outOfTemporaryExport: ApplicableXmlValue),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate),
				GetNewRefCusCodeList($"01{ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21}", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21}04", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21}{ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21}", startDate, endDate),
				GetNewRefCusCodeList($"01{ProcedureCodes.TemporaryExportForReturnInTheUnalteredState}", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.TemporaryExportForReturnInTheUnalteredState}04", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.TemporaryExportForReturnInTheUnalteredState}{ProcedureCodes.TemporaryExportForReturnInTheUnalteredState}", startDate, endDate),
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.TemporaryExportOtherThanThatReferredToUnderCode21, startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.TemporaryExportForReturnInTheUnalteredState, startDate, endDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestIsInwardProcessing()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.InwardProcessingProcedureSuspensionSystem, description: " - ", shipmentType: ShipmentType.Export, outOfInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.InwardProcessingProcedureSuspensionSystem, concession: "A00", shipmentType: ShipmentType.Export, outOfInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, shipmentType: ShipmentType.Export, intoInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, intoInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, intoInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, previousProcedureCode: ProcedureCodes.InwardProcessingProcedureSuspensionSystem, description: " - ", shipmentType: ShipmentType.Export, intoInwardProcessing: ApplicableXmlValue, outOfInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, previousProcedureCode: ProcedureCodes.InwardProcessingProcedureSuspensionSystem, concession: "A00", shipmentType: ShipmentType.Export, intoInwardProcessing: ApplicableXmlValue, outOfInwardProcessing: ApplicableXmlValue),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.InwardProcessingProcedureSuspensionSystem, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.InwardProcessingProcedureSuspensionSystem, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, previousProcedureCode: ProcedureCodes.InwardProcessingProcedureSuspensionSystem, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoInwardProcessing: ApplicableXmlValue, outOfInwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate, previousProcedureCode: ProcedureCodes.InwardProcessingProcedureSuspensionSystem, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoInwardProcessing: ApplicableXmlValue, outOfInwardProcessing: ApplicableXmlValue),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate),
				GetNewRefCusCodeList($"01{ProcedureCodes.InwardProcessingProcedureSuspensionSystem}", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.InwardProcessingProcedureSuspensionSystem}04", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.InwardProcessingProcedureSuspensionSystem}{ProcedureCodes.InwardProcessingProcedureSuspensionSystem}", startDate, endDate),
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.InwardProcessingProcedureSuspensionSystem, startDate, endDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			AssertAllRefCusProcedure(expected, result);
		}

		[Test]
		public void TestIsOutwardProcessing()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today;
			var expected = new RefCusProcedure[]
			{
				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, description: " - ", shipmentType: ShipmentType.Export, outOfOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, concession: "A00", shipmentType: ShipmentType.Export, outOfOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, shipmentType: ShipmentType.Export, intoOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Export, intoOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Export, intoOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, description: " - ", shipmentType: ShipmentType.Export, intoOutwardProcessing: ApplicableXmlValue, outOfOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, concession: "A00", shipmentType: ShipmentType.Export, intoOutwardProcessing: ApplicableXmlValue, outOfOutwardProcessing: ApplicableXmlValue),

				GetNewRefCusProcedure("01", startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure("01", startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, outOfOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, previousProcedureCode: "04", description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, previousProcedureCode: "04", concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, description: " - ", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoOutwardProcessing: ApplicableXmlValue, outOfOutwardProcessing: ApplicableXmlValue),
				GetNewRefCusProcedure(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate, previousProcedureCode: ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, concession: "A00", shipmentType: ShipmentType.Import, calculateDuty: true, calculateVat: true, intoOutwardProcessing: ApplicableXmlValue, outOfOutwardProcessing: ApplicableXmlValue),
			};

			var dataCombinations = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("0104", startDate, endDate),
				GetNewRefCusCodeList($"01{ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure}", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure}04", startDate, endDate),
				GetNewRefCusCodeList($"{ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure}{ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure}", startDate, endDate),
			};

			var dataPreviousProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("04", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate)
			};

			var dataProcedure = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("01", startDate, endDate),
				GetNewRefCusCodeList(ProcedureCodes.TemporaryExportUnderTheOutwardProcessingProcedure, startDate, endDate)
			};

			var dataConcessions = new RefCusCodeList[]
			{
				GetNewRefCusCodeList("A00", startDate, endDate)
			};

			var result = Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: false);
			result.AddRange(Business.CusProcedure.CusProcedure.GenerateRefCusProceduresFromRefCusCodeLists(dataCombinations, dataProcedure, dataPreviousProcedure, dataConcessions, isImport: true));

			AssertAllRefCusProcedure(expected, result);
		}

		void AssertAllRefCusProcedure(RefCusProcedure[] expected, List<RefCusProcedure> result)
		{
			Assert.Multiple(() =>
			{
				Assert.AreEqual(expected.Length, result.Count, "amount");

				for (var i = 0; i < result.Count; i++)
				{
					AssertRefCusProcedure(expected[i], result[i], $"{i}");
				}
			});
		}

		void AssertRefCusProcedure(RefCusProcedure expected, RefCusProcedure result, string message)
		{
			var assertMessage = $"{message} - {expected.ZZ6_ShipmentType} {expected.ZZ6_ProcedureCode}:{expected.ZZ6_PreviousProcedureCode}:{expected.ZZ6_Concession}";
			Assert.AreEqual(expected.ZZ6_ProcedureCode, result.ZZ6_ProcedureCode, $"{assertMessage} - ZZ6_ProcedureCode");
			Assert.AreEqual(expected.ZZ6_PreviousProcedureCode, result.ZZ6_PreviousProcedureCode, $"{assertMessage} - ZZ6_PreviousProcedureCode");
			Assert.AreEqual(expected.ZZ6_Concession, result.ZZ6_Concession, $"{assertMessage} - ZZ6_Concession");
			Assert.AreEqual(expected.ZZ6_StartDate, result.ZZ6_StartDate, $"{assertMessage} - ZZ6_StartDate");
			Assert.AreEqual(expected.ZZ6_EndDate, result.ZZ6_EndDate, $"{assertMessage} - ZZ6_EndDate");
			Assert.AreEqual(expected.ZZ6_Description, result.ZZ6_Description, $"{assertMessage} - ZZ6_Description");
			Assert.AreEqual(expected.ZZ6_ShipmentType, result.ZZ6_ShipmentType, $"{assertMessage} - ZZ6_ShipmentType");
			Assert.AreEqual(expected.ZZ6_CalculateDuty, result.ZZ6_CalculateDuty, $"{assertMessage} - ZZ6_CalculateDuty");
			Assert.AreEqual(expected.ZZ6_CalculateVAT, result.ZZ6_CalculateVAT, $"{assertMessage} - ZZ6_CalculateVAT");
			Assert.AreEqual(expected.ZZ6_LandedCost, result.ZZ6_LandedCost, $"{assertMessage} - ZZ6_LandedCost");
			Assert.AreEqual(expected.ZZ6_IntoWarehouse, result.ZZ6_IntoWarehouse, $"{assertMessage} - ZZ6_IntoWarehouse");
			Assert.AreEqual(expected.ZZ6_OutOfWarehouse, result.ZZ6_OutOfWarehouse, $"{assertMessage} - ZZ6_OutOfWarehouse");
			Assert.AreEqual(expected.ZZ6_IntoTemporaryImport, result.ZZ6_IntoTemporaryImport, $"{assertMessage} - ZZ6_IntoTemporaryImport");
			Assert.AreEqual(expected.ZZ6_OutOfTemporaryImport, result.ZZ6_OutOfTemporaryImport, $"{assertMessage} - ZZ6_OutOfTemporaryImport");
			Assert.AreEqual(expected.ZZ6_IntoTemporaryExport, result.ZZ6_IntoTemporaryExport, $"{assertMessage} - ZZ6_IntoTemporaryExport");
			Assert.AreEqual(expected.ZZ6_OutOfTemporaryExport, result.ZZ6_OutOfTemporaryExport, $"{assertMessage} - ZZ6_OutOfTemporaryExport");
			Assert.AreEqual(expected.ZZ6_IntoInwardProcessing, result.ZZ6_IntoInwardProcessing, $"{assertMessage} - ZZ6_IntoInwardProcessing");
			Assert.AreEqual(expected.ZZ6_OutOfInwardProcessing, result.ZZ6_OutOfInwardProcessing, $"{assertMessage} - ZZ6_OutOfInwardProcessing");
			Assert.AreEqual(expected.ZZ6_IntoOutwardProcessing, result.ZZ6_IntoOutwardProcessing, $"{assertMessage} - ZZ6_IntoOutwardProcessing");
			Assert.AreEqual(expected.ZZ6_OutofOutwardProcessing, result.ZZ6_OutofOutwardProcessing, $"{assertMessage} - ZZ6_OutofOutwardProcessing");
		}

		RefCusProcedure GetNewRefCusProcedure(string procedureCode,
			DateTime startDate,
			DateTime endDate,
			string previousProcedureCode = "",
			string concession = "",
			string description = "",
			string shipmentType = "EXP",
			bool calculateDuty = false,
			bool calculateVat = false,
			bool landedCost = false,
			string intoWarehouse = NotApplicableXmlValue,
			string outOfWarehouse = NotApplicableXmlValue,
			string intoTemporaryImport = NotApplicableXmlValue,
			string outOfTemporaryImport = NotApplicableXmlValue,
			string intoTemporaryExport = NotApplicableXmlValue,
			string outOfTemporaryExport = NotApplicableXmlValue,
			string intoInwardProcessing = NotApplicableXmlValue,
			string outOfInwardProcessing = NotApplicableXmlValue,
			string intoOutwardProcessing = NotApplicableXmlValue,
			string outOfOutwardProcessing = NotApplicableXmlValue
		) => new RefCusProcedure()
		{
			ZZ6_ProcedureCode = procedureCode,
			ZZ6_PreviousProcedureCode = previousProcedureCode,
			ZZ6_Concession = concession,
			ZZ6_StartDate = startDate,
			ZZ6_EndDate = endDate,
			ZZ6_Description = description,
			ZZ6_ShipmentType = shipmentType,
			ZZ6_CalculateDuty = calculateDuty,
			ZZ6_CalculateVAT = calculateVat,
			ZZ6_LandedCost = landedCost,
			ZZ6_IntoWarehouse = intoWarehouse,
			ZZ6_OutOfWarehouse = outOfWarehouse,
			ZZ6_IntoTemporaryImport = intoTemporaryImport,
			ZZ6_OutOfTemporaryImport = outOfTemporaryImport,
			ZZ6_IntoTemporaryExport = intoTemporaryExport,
			ZZ6_OutOfTemporaryExport = outOfTemporaryExport,
			ZZ6_IntoInwardProcessing = intoInwardProcessing,
			ZZ6_OutOfInwardProcessing = outOfInwardProcessing,
			ZZ6_IntoOutwardProcessing = intoOutwardProcessing,
			ZZ6_OutofOutwardProcessing = outOfOutwardProcessing
		};

		RefCusCodeList GetNewRefCusCodeList(string code, DateTime startDate, DateTime endDate, string description = "") => new RefCusCodeList()
		{
			ZZD_Code = code,
			ZZD_StartDate = startDate,
			ZZD_EndDate = endDate,
			ZZD_Description = description,
		};
	}
}
