using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff
{
	[TestFixture]
	sealed class PLRefCusVATApplicabilityTest : PLRefCusVATApplicabilityReader
	{
		[Test]
		public void TestGetRefCusVATApplicabilityFromIsztarHistoryResponse()
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
										measureTypeId = Constants.MeasureType.MeasureVatVATApplicabilityTypeId
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
									validityEndDate = Constants.ConstantEndDate
								},
								new measure() {
									measureType = new measureType() {
										measureTypeId = Constants.MeasureType.MeasureVatVATApplicabilityTypeId
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
									validityEndDateSpecified = false
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

			var expected = XDocument.Load(TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff.TestFiles.Output.TestGetRefCusVATApplicabilityFromIsztarHistoryResponse.xml"));
			var result = XDocument.Load(tempOutput);

			Assert.AreEqual(expected.ToString(), result.ToString());
		}

		[Test]
		public void TestGetNKTaxOrFeeCodeBasedOnDutyAmound()
		{
			var measureData = new measure()
			{
				measureComponent = new measureComponent[] {
					new measureComponent() {
						dutyAmountSpecified = true,
						dutyAmount = 0.0m
					}
				}
			};

			measureData.measureComponent[0].dutyAmount = 0.0m;
			Assert.AreEqual(Constants.DutyCode.Ese, GetNKTaxOrFeeCodeBasedOnDutyAmound(measureData));

			measureData.measureComponent[0].dutyAmount = 3.0m;
			Assert.AreEqual(Constants.DutyCode.Spe, GetNKTaxOrFeeCodeBasedOnDutyAmound(measureData));

			measureData.measureComponent[0].dutyAmount = 5.0m;
			Assert.AreEqual(Constants.DutyCode.Min, GetNKTaxOrFeeCodeBasedOnDutyAmound(measureData));

			measureData.measureComponent[0].dutyAmount = 8.0m;
			Assert.AreEqual(Constants.DutyCode.Rid, GetNKTaxOrFeeCodeBasedOnDutyAmound(measureData));

			measureData.measureComponent[0].dutyAmount = 23.0m;
			Assert.AreEqual(Constants.DutyCode.Ord, GetNKTaxOrFeeCodeBasedOnDutyAmound(measureData));

			var borderValue = new decimal[] {
				0.1m, 2.9m, 3.1m, 4.9m, 5.1m, 7.9m, 8.1m, 22.9m, 23.1m
			};

			foreach (var value in borderValue)
			{
				measureData.measureComponent[0].dutyAmount = value;
				Assert.AreEqual(string.Empty, GetNKTaxOrFeeCodeBasedOnDutyAmound(measureData));
			}
		}
	}
}
