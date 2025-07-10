using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff
{
	[TestFixture]
	sealed class PLRefCusConditionTest : PLRefCusConditionReader
	{
		[Test]
		public void TestGetRefCusConditionFromIsztarHistoryResponse()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2021, 1, 1));

			var dataToTest = new IsztarHistoryResponse()
			{
				IsztarHistoryItem = new IsztarHistoryResponseIsztarHistoryItem[] {
					new IsztarHistoryResponseIsztarHistoryItem() {
						Item = new findMeasureByDatesResponseHistory() {
							Measure = new measure[] {
								new measure() {
									measureType = new measureType() {
										measureTypeId = Constants.MeasureType.P01
									},
									additionalCode = new additionalCode() {
										additionalCodeType = new additionalCodeType() {
											additionalCodeTypeId = "V"
										},
										additionalCodeCode = "999"
									},
									measureGeneratingRegulationId = "999",
									regulationRoleType = new regulationRoleType() {
										regulationRoleTypeId = "1"
									},
									goodsNomenclature = new goodsNomenclature() {
										goodsNomenclatureItemId = "653462334"
									},
									measureComponent = new measureComponent[] {
										new measureComponent() {
											dutyAmount = 3.0m
										}
									},
									validityStartDateSpecified = true,
									validityStartDate = new DateTime(2020, 09, 23),
									validityEndDateSpecified = true,
									validityEndDate = Constants.ConstantEndDate,
									geographicalArea = new geographicalArea() {
										geographicalAreaId = "2B"
									},
									measureCondition = new measureCondition[] {
										new measureCondition() {
											certificate = new certificate() {
												certificateType = new certificateType() {
													certificateTypeCode = "02"
												},
												certificateCode = "B"
											},
											measureAction = new measureAction() {
												actionCode = "4"
											}
										}
									}
								},
								new measure() {
									measureType = new measureType() {
										measureTypeId = Constants.MeasureType.P02
									},
									additionalCode = new additionalCode() {
										additionalCodeType = new additionalCodeType() {
											additionalCodeTypeId = "V"
										},
										additionalCodeCode = "888"
									},
									measureGeneratingRegulationId = "888",
									regulationRoleType = new regulationRoleType() {
										regulationRoleTypeId = "3"
									},
									goodsNomenclature = new goodsNomenclature() {
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
									geographicalArea = new geographicalArea() {
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
									}
								}
							}
						}
					},
					new IsztarHistoryResponseIsztarHistoryItem() {
						Item = new findBaseRegulationByDatesResponseHistory() {
							BaseRegulation = new baseRegulation[] {
								new baseRegulation() {
									baseRegulationId = "999",
									regulationRoleType = new regulationRoleType() {
										regulationRoleTypeId = "1"
									},
									effectiveEndDateSpecified = false,
									effectiveEndDate = new DateTime(2050, 11, 30)
								},
								new baseRegulation() {
									baseRegulationId = "888",
									regulationRoleType = new regulationRoleType() {
										regulationRoleTypeId = "1"
									},
									effectiveEndDateSpecified = true,
									effectiveEndDate = new DateTime(2050, 12, 30)
								}
							}
						}
					},
					new IsztarHistoryResponseIsztarHistoryItem() {
						Item = new findModificationRegulationByDatesResponseHistory() {
							ModificationRegulation = new modificationRegulation[] {
								new modificationRegulation() {
									effectiveEndDateSpecified = true,
									effectiveEndDate = new DateTime(2040, 11, 30),
									modificationRegulationId = "mod888",
									baseRegulation = new baseRegulation() {
										baseRegulationId = "888"
									}
								}
							}
						}
					}
				}
			};

			var resultData = PLTariffExtractor.GeneratePLTariffData(dataToTest, dateTimeProviderMock);
			var tempOutput = Path.GetTempFileName();
			TariffUniversalReferenceDataXmlGenerator.GenerateReferenceDataXml(new DateTime(2010, 11, 11), resultData, tempOutput);

			var expected = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff.TestFiles.Output.TestGetRefCusConditionFromIsztarHistoryResponse.xml"));
			var result = XDocument.Load(tempOutput);

			Assert.AreEqual(expected.ToString(), result.ToString());
		}

		[Test]
		public void TestMissingCertificateCodeDoesNotGenerateRefCusConditionValue()
		{
			var data = new measure()
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
							}
						},
						measureAction = new measureAction() {
							actionCode = "1"
						}
					}
				}
			};

			var result = GetRefCusConditionFromIsztarHistoryResponse(data);
			Assert.AreEqual(0, result.RefCusConditions.Count);

			data.measureCondition[0].certificate.certificateCode = string.Empty;
			result = GetRefCusConditionFromIsztarHistoryResponse(data);
			Assert.AreEqual(0, result.RefCusConditions.Count);
		}

		[Test]
		public void TestMultipleRefCusConditionValues()
		{
			var data = new measure()
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
							certificateCode = "asd"
						},
						measureAction = new measureAction() {
							actionCode = "1"
						}
					},
					new measureCondition() {
						certificate = new certificate() {
							certificateType = new certificateType() {
								certificateTypeCode = "02"
							},
							certificateCode = "zxc"
						},
						measureAction = new measureAction() {
							actionCode = "2"
						}
					}
				}
			};

			var result = GetRefCusConditionFromIsztarHistoryResponse(data);
			Assert.AreEqual(2, result.RefCusConditions.Count);
		}

		[Test]
		public void TestTrueMeansStop()
		{
			var data = new measure()
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
							actionCode = "1"
						}
					}
				}
			};

			var result = GetRefCusConditionFromIsztarHistoryResponse(data);
			Assert.AreEqual(false, result.RefCusConditions[0].ZX1_ConditionValueTrueMeansStop);

			data.measureCondition[0].measureAction.actionCode = "4";
			result = GetRefCusConditionFromIsztarHistoryResponse(data);
			Assert.AreEqual(true, result.RefCusConditions[0].ZX1_ConditionValueTrueMeansStop);

			data.measureCondition[0].measureAction.actionCode = "5";
			result = GetRefCusConditionFromIsztarHistoryResponse(data);
			Assert.AreEqual(true, result.RefCusConditions[0].ZX1_ConditionValueTrueMeansStop);

			data.measureCondition[0].measureAction.actionCode = "6";
			result = GetRefCusConditionFromIsztarHistoryResponse(data);
			Assert.AreEqual(true, result.RefCusConditions[0].ZX1_ConditionValueTrueMeansStop);

			data.measureCondition[0].measureAction.actionCode = "9";
			result = GetRefCusConditionFromIsztarHistoryResponse(data);
			Assert.AreEqual(true, result.RefCusConditions[0].ZX1_ConditionValueTrueMeansStop);
		}

		[Test]
		public void TestSampleMeasure()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var rawInputData = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff.TestFiles.Input.singleMeasureSample.xml"));
			var parsedInputData = XmlParser.Deserialize<IsztarHistoryResponse>(rawInputData);

			var findMeasureByDatesResponseHistory = (findMeasureByDatesResponseHistory)parsedInputData.IsztarHistoryItem[0].Item;
			var result = GetRefCusConditionFromIsztarHistoryResponse(findMeasureByDatesResponseHistory.Measure[0]);

			Assert.AreEqual(1, result.RefCusConditions.Count);
			Assert.AreEqual(2, result.RefCusConditions[0].RefCusConditionValues.Length);
		}
	}
}
