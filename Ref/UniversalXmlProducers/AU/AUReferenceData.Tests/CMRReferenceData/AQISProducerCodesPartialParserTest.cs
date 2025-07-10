using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Business.CMRReferenceData;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISProducerCodesPartialParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISProducerCodes";

		protected override string TextFileName => "AQSPRDCR-P1-EDCHNG-2504240001.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISProducerCodes_Partial.xml";

		protected override DateTime PublishedDate => new DateTime(2025, 01, 02, 01, 39, 00).Date;

		protected override ICMRDataParser Parser => new AQISProducerCodesPartialParserForTest();
	}

	class AQISProducerCodesPartialParserForTest : AQISProducerCodesPartialParser
	{
		protected override IRefDataLoader GetRefDataLoader()
		{
			var mockDataLoader = new Mock<IRefDataLoader>();
			mockDataLoader.Setup(x => x.LoadData<RefCusCodeList>(It.Is<string>(query => query.StartsWith("RefCusCodeListUpdate?"))))
						  .Returns<string>(x => Task.FromResult(dbEntities.AsEnumerable()));
			return mockDataLoader.Object;
		}

		readonly RefCusCodeList[] dbEntities =
		[
			new()
			{
				ZZD_ZZK_NKCodeType = CMRConstants.CodeTypes.CMRPR,
				ZZD_Code = "0000002P",
				ZZD_Description = "A&D CHRISTOPHER RANCH",
				ZZD_StartDate = Constants.RefData_Common.MinimumDateTime,
				ZZD_EndDate = Constants.RefData_Common.MaximumDateTime,
				ZZD_ZZZ_NKDataGrouping = Constants.DataGrouping,
				RefCusCodeListAttributes =
				[
					new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISProducerLocality,
						ZZE_Value = "UNITED STATES OF AMERICA",
						ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
						ZZE_EndDate = Constants.RefData_Common.MaximumDateTime
					},
					new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISProducerCountryCode,
						ZZE_Value = "US",
						ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
						ZZE_EndDate = Constants.RefData_Common.MaximumDateTime
					}
				]
			},
			new()
			{
				ZZD_ZZK_NKCodeType = CMRConstants.CodeTypes.CMRPR,
				ZZD_Code = "0000003Q",
				ZZD_Description = "A & N FOODS CO LTD (ESTABLISHMENT 1001)",
				ZZD_StartDate = Constants.RefData_Common.MinimumDateTime,
				ZZD_EndDate = Constants.RefData_Common.MaximumDateTime,
				ZZD_ZZZ_NKDataGrouping = Constants.DataGrouping,
				RefCusCodeListAttributes =
				[
					new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISProducerLocality,
						ZZE_Value = "SAMUTSAKORN",
						ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
						ZZE_EndDate = Constants.RefData_Common.MaximumDateTime
					},
					new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISProducerCountryCode,
						ZZE_Value = "TH",
						ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
						ZZE_EndDate = Constants.RefData_Common.MaximumDateTime
					}
				]
			},
			new()
			{
				ZZD_ZZK_NKCodeType = CMRConstants.CodeTypes.CMRPR,
				ZZD_Code = "0000004R",
				ZZD_Description = "A A M ENT CO LTD",
				ZZD_StartDate = Constants.RefData_Common.MinimumDateTime,
				ZZD_EndDate = Constants.RefData_Common.MaximumDateTime,
				ZZD_ZZZ_NKDataGrouping = Constants.DataGrouping,
				RefCusCodeListAttributes =
				[
					new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISProducerLocality,
						ZZE_Value = "FRANCE",
						ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
						ZZE_EndDate = new DateTime(2024, 01, 02, 01, 39, 00).Date
					},
					new RefCusCodeListAttribute
					{
						ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISProducerCountryCode,
						ZZE_Value = "FR",
						ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
						ZZE_EndDate = new DateTime(2024, 01, 02, 01, 39, 00).Date
					}
				]
			}
		];
	}
}
