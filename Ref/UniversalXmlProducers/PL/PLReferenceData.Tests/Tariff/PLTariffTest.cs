using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.PLReferenceData.Business.Tariff.Constants;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff
{
	[TestFixture]
	sealed class PLTariffTest : PLTariffExtractor
	{
		[Test]
		public void TestIsExport()
		{
			Assert.AreEqual(true, IsExport(Constants.MeasureType.P03));

			Assert.AreEqual(false, IsExport(Constants.MeasureType.P01));
			Assert.AreEqual(false, IsExport(Constants.MeasureType.P02));
			Assert.AreEqual(false, IsExport(Constants.MeasureType.P04));
			Assert.AreEqual(false, IsExport(Constants.MeasureType.P05));
			Assert.AreEqual(false, IsExport(Constants.MeasureType.P06));
			Assert.AreEqual(false, IsExport(Constants.MeasureType.P07));
			Assert.AreEqual(false, IsExport(Constants.MeasureType.P09));
			Assert.AreEqual(false, IsExport(Constants.MeasureType.P10));
			Assert.AreEqual(false, IsExport(Constants.MeasureType.P11));

			Assert.AreEqual(false, IsExport("3"));
			Assert.AreEqual(false, IsExport("03"));
			Assert.AreEqual(false, IsExport("p3"));
			Assert.AreEqual(false, IsExport("p02"));
			Assert.AreEqual(false, IsExport("p04"));
		}

		[Test]
		public void TestGetCommentBasedOnMeasureTypeId()
		{
			Assert.AreEqual(Constants.PTypeMeasureComment.P01, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P01));
			Assert.AreEqual(Constants.PTypeMeasureComment.P02, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P02));
			Assert.AreEqual(Constants.PTypeMeasureComment.P03, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P03));
			Assert.AreEqual(Constants.PTypeMeasureComment.P04, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P04));
			Assert.AreEqual(Constants.PTypeMeasureComment.P05, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P05));
			Assert.AreEqual(Constants.PTypeMeasureComment.P06, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P06));
			Assert.AreEqual(Constants.PTypeMeasureComment.P07, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P07));
			Assert.AreEqual(Constants.PTypeMeasureComment.P09, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P09));
			Assert.AreEqual(Constants.PTypeMeasureComment.P10, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P10));
			Assert.AreEqual(Constants.PTypeMeasureComment.P11, GetCommentBasedOnMeasureTypeId(Constants.MeasureType.P11));
		}

		[Test]
		public void TestGetRefCusVATApplicabilityFromPLTariff()
		{
			var data = new ExtractedMeasureData()
			{
				EndDate = new DateTime(2020, 12, 30),
				StartDate = new DateTime(2020, 01, 1),
				NKTaxOrFeeCode = "111",
				AdditionalCode = "1111A"
			};

			var expected = new RefCusVATApplicability()
			{
				ZX5_EndDate = new DateTime(2020, 12, 30),
				ZX5_StartDate = new DateTime(2020, 01, 1),
				ZX5_ZZF_NKTaxOrFeeCode = "111",
				ZX5_AdditionalCode = "1111A"
			};

			var result = GetRefCusVATApplicabilityFromPLTariff(data);

			Assert.AreEqual(expected.ZX5_EndDate, result.ZX5_EndDate);
			Assert.AreEqual(expected.ZX5_StartDate, result.ZX5_StartDate);
			Assert.AreEqual(expected.ZX5_ZZF_NKTaxOrFeeCode, result.ZX5_ZZF_NKTaxOrFeeCode);
			Assert.AreEqual(expected.ZX5_AdditionalCode, result.ZX5_AdditionalCode);
		}

		[Test]
		public void TestGetEndDateTime()
		{
			Assert.AreEqual(Constants.ConstantEndDate, GetEndDateTime(DateTime.MinValue, DateTime.MinValue));
			Assert.AreEqual(new DateTime(2020, 12, 30), GetEndDateTime(new DateTime(2020, 12, 30), DateTime.MinValue));
			Assert.AreEqual(new DateTime(2020, 12, 30), GetEndDateTime(DateTime.MinValue, new DateTime(2020, 12, 30)));
			Assert.AreEqual(new DateTime(2020, 11, 30), GetEndDateTime(new DateTime(2020, 11, 30), new DateTime(2020, 12, 30)));

			var data = GetEndDateTime(new DateTime(2020, 11, 30), new DateTime(2010, 10, 20));
			Assert.AreEqual(PLReferenceData.Business.Constants.LatestVatChangeDate.AddYears(-1), data);
		}

		[Test]
		public void TestIgnoreExpired()
		{
			var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2021, 1, 1));

			var data = new List<ExtractedMeasureData>
			{
				new ExtractedMeasureData() {
					StartDate = new DateTime(2020, 01, 1),
					EndDate = new DateTime(2020, 12, 30),
					NKTaxOrFeeCode = "111",
					AdditionalCode = "1111A",
					DataType = MeasureDataType.REF_CUS_VAT_APPLICABILITY,
					TariffCode = "123abc"
				},

				new ExtractedMeasureData() {
					StartDate = new DateTime(2000, 01, 1),
					EndDate = new DateTime(2007, 12, 30),
					NKTaxOrFeeCode = "222",
					AdditionalCode = "2222B",
					DataType = MeasureDataType.REF_CUS_VAT_APPLICABILITY,
					TariffCode = "321cba"
				},
			};

			var expected = new List<RefCusTariff>
			{
				new RefCusTariff() {
					ZZ1_TariffCode = "123abc",
					RefCusVATApplicabilities = new RefCusVATApplicability[]
					{
						new RefCusVATApplicability() {
							ZX5_StartDate = new DateTime(2020, 01, 1),
							ZX5_EndDate =  new DateTime(2020, 12, 30),
							ZX5_ZZF_NKTaxOrFeeCode = "111",
							ZX5_AdditionalCode = "1111A"
						}
					}
				}
			};

			var result = GetRefCusTariffFromPLTariff(data, dateTimeProviderMock);

			Assert.AreEqual(expected.Count, result.Count);
			Assert.AreEqual(expected[0].ZZ1_TariffCode, result[0].ZZ1_TariffCode);
			Assert.AreEqual(expected[0].RefCusVATApplicabilities.Length, result[0].RefCusVATApplicabilities.Length);
			Assert.AreEqual(expected[0].RefCusVATApplicabilities[0].ZX5_StartDate, result[0].RefCusVATApplicabilities[0].ZX5_StartDate);
			Assert.AreEqual(expected[0].RefCusVATApplicabilities[0].ZX5_EndDate, result[0].RefCusVATApplicabilities[0].ZX5_EndDate);
			Assert.AreEqual(expected[0].RefCusVATApplicabilities[0].ZX5_ZZF_NKTaxOrFeeCode, result[0].RefCusVATApplicabilities[0].ZX5_ZZF_NKTaxOrFeeCode);
			Assert.AreEqual(expected[0].RefCusVATApplicabilities[0].ZX5_AdditionalCode, result[0].RefCusVATApplicabilities[0].ZX5_AdditionalCode);
		}

		[Test]
		public void TestIgnoreActionCodes05And25()
		{
			var data = new measure()
			{
				measureCondition = new measureCondition[] {
					new measureCondition() {
						measureAction = new measureAction() {
							actionCode = "25"
						}
					}
				}
			};

			var result = new List<ExtractedMeasureData>();

			OnNationalMeasureStartingLetter(result, data);
			Assert.AreEqual(0, result.Count);

			data.measureCondition[0].measureAction.actionCode = "05";
			OnNationalMeasureStartingLetter(result, data);
			Assert.AreEqual(0, result.Count);

			data = new measure()
			{
				measureType = new measureType()
				{
					measureTypeId = Constants.MeasureType.P02
				},
				additionalCode = new additionalCode()
				{
					additionalCodeType = new additionalCodeType()
					{
						additionalCodeTypeId = "V"
					},
					additionalCodeCode = "888"
				},
				measureGeneratingRegulationId = "888",
				regulationRoleType = new regulationRoleType()
				{
					regulationRoleTypeId = "3"
				},
				goodsNomenclature = new goodsNomenclature()
				{
					goodsNomenclatureItemId = "53463546456"
				},
				measureComponent = new measureComponent[] {
										new measureComponent() {
											dutyAmount = 23.0m
										}
									},
				validityStartDateSpecified = true,
				validityStartDate = new DateTime(2020, 09, 23),
				validityEndDateSpecified = false,
				geographicalArea = new geographicalArea()
				{
					geographicalAreaId = "1A"
				},
				measureCondition = new measureCondition[] {
					new measureCondition() {
						certificate = new certificate() {
							certificateType = new certificateType() {
								certificateTypeCode = "01"
							},
							certificateCode = "A"
						},
						measureAction = new measureAction() {
							actionCode = "4"
						}
					}
				},
			};

			OnNationalMeasureStartingLetter(result, data);
			Assert.AreEqual(1, result.Count);
		}

		[Test]
		public void TestGetRefCusConditionFromPLTariff()
		{
			var data = new ExtractedMeasureData()
			{
				StartDate = new DateTime(2021, 09, 20),
				EndDate = new DateTime(2021, 09, 21),
				MeasureTypeId = MeasureType.P01,
				AdditionalCode = "abc",
				ConditionNkTradeGroup = "asd",
				RefCusConditions = new List<RefCusCondition> { }
			};

			var expected = new RefCusCondition()
			{
				ZX1_StartDate = new DateTime(2021, 09, 20),
				ZX1_EndDate = new DateTime(2021, 09, 21),
				ZX1_ZX2_NKConditionType = MeasureType.P01,
				ZX1_Comment = PTypeMeasureComment.P01,
				ZX1_IsExport = false,
				ZX1_IsImport = true,
				ZX1_ConditionValueTrueMeansStop = false,
				RefCusApplicabilities = new RefCusApplicability[] {
					new RefCusApplicability()
					{
						ZZT_AdditionalCode = "abc",
						ZZT_EndDate = new DateTime(2021, 09, 21),
						ZZT_StartDate = new DateTime(2021, 09, 20),
						ZZT_ZZA_NKTradeGroup = "asd"
					}
				}
			};

			var result = new RefCusCondition();
			GetRefCusConditionFromPLTariff(data, result);

			Assert.AreEqual(expected.ZX1_StartDate, result.ZX1_StartDate);
			Assert.AreEqual(expected.ZX1_EndDate, result.ZX1_EndDate);
			Assert.AreEqual(expected.ZX1_ZX2_NKConditionType, result.ZX1_ZX2_NKConditionType);
			Assert.AreEqual(expected.ZX1_Comment, result.ZX1_Comment);
			Assert.AreEqual(expected.ZX1_IsExport, result.ZX1_IsExport);
			Assert.AreEqual(expected.ZX1_IsImport, result.ZX1_IsImport);
			Assert.AreEqual(expected.ZX1_ConditionValueTrueMeansStop, result.ZX1_ConditionValueTrueMeansStop);

			Assert.NotNull(result.RefCusApplicabilities);
			Assert.AreEqual(expected.RefCusApplicabilities.Length, result.RefCusApplicabilities.Length);
			Assert.AreEqual(expected.RefCusApplicabilities[0].ZZT_AdditionalCode, result.RefCusApplicabilities[0].ZZT_AdditionalCode);
			Assert.AreEqual(expected.RefCusApplicabilities[0].ZZT_EndDate, result.RefCusApplicabilities[0].ZZT_EndDate);
			Assert.AreEqual(expected.RefCusApplicabilities[0].ZZT_StartDate, result.RefCusApplicabilities[0].ZZT_StartDate);
			Assert.AreEqual(expected.RefCusApplicabilities[0].ZZT_ZZA_NKTradeGroup, result.RefCusApplicabilities[0].ZZT_ZZA_NKTradeGroup);
		}
	}
}
