using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class AQISPremisesCodesPartialParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "AQISPremisesCodes";

		protected override string TextFileName => "AQSPREMS-P1-EDCHNG-2504240001.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_AQISPremisesCodes_Partial.xml";

		protected override DateTime PublishedDate => new DateTime(2025, 01, 02, 01, 39, 00).Date;

		protected override ICMRDataParser Parser => new AQISPremisesCodesPartialParserForTest();
	}

	class AQISPremisesCodesPartialParserForTest : AQISPremisesCodesPartialParser
	{
		protected override IRefDataLoader GetRefDataLoader()
		{
			var mockDataLoader = new Mock<IRefDataLoader>();
			mockDataLoader.Setup(x => x.LoadData<RefCusCodeList>(It.Is<string>(query => query.StartsWith("RefCusCodeListUpdate?"))))
			              .Returns<string>(x => Task.FromResult(dbEntities.AsEnumerable()));
			return mockDataLoader.Object;
		}

		RefCusCodeList[] dbEntities = new RefCusCodeList[]
		{
			new RefCusCodeList()
				{
					ZZD_ZZK_NKCodeType = CMRConstants.CodeTypes.CMRAP,
					ZZD_Code = "A000501",
					ZZD_Description = "AAE, PIALLIGO, 2609   (A0005)",
					ZZD_StartDate = Constants.RefData_Common.MinimumDateTime,
					ZZD_EndDate = Constants.RefData_Common.MaximumDateTime,
					ZZD_ZZZ_NKDataGrouping = Constants.DataGrouping,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[]
					{
						new RefCusCodeListAttribute()
						{
							ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISPremisesPortCode,
							ZZE_Value = "AUBTB",
							ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
							ZZE_EndDate = Constants.RefData_Common.MaximumDateTime
						}
					}
				},
			new RefCusCodeList()
				{
					ZZD_ZZK_NKCodeType = CMRConstants.CodeTypes.CMRAP,
					ZZD_Code = "A000502",
					ZZD_Description = "AUSTRALIAN AIR EXPRESS, PIALLIGO, 2609   (A0005)",
					ZZD_StartDate = Constants.RefData_Common.MinimumDateTime,
					ZZD_EndDate = new DateTime(2025, 01, 01, 23, 59, 00),
					ZZD_ZZZ_NKDataGrouping = Constants.DataGrouping,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[]
					{
						new RefCusCodeListAttribute()
						{
							ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISPremisesPortCode,
							ZZE_Value = "AUBTB",
							ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
							ZZE_EndDate = Constants.RefData_Common.MaximumDateTime
						}
					}
				},
			new RefCusCodeList()
				{
					ZZD_ZZK_NKCodeType = CMRConstants.CodeTypes.CMRAP,
					ZZD_Code = "A000503",
					ZZD_Description = "AXX, PIALLIGO, 2609   (A0005)",
					ZZD_StartDate = Constants.RefData_Common.MinimumDateTime,
					ZZD_EndDate = Constants.RefData_Common.MaximumDateTime,
					ZZD_ZZZ_NKDataGrouping = Constants.DataGrouping,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[]
					{
						new RefCusCodeListAttribute()
						{
							ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISPremisesPortCode,
							ZZE_Value = "AUMEL",
							ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
							ZZE_EndDate = new DateTime(2024, 06, 01, 00, 00, 00)
						},
						new RefCusCodeListAttribute()
						{
							ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISPremisesPortCode,
							ZZE_Value = "AUBTB",
							ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
							ZZE_EndDate = Constants.RefData_Common.MaximumDateTime
						}
					}
				},
			new RefCusCodeList()
				{
					ZZD_ZZK_NKCodeType = CMRConstants.CodeTypes.CMRAP,
					ZZD_Code = "A0015",
					ZZD_Description = "COMMONWEALTH SCIENTIFIC AND INDUSTRIAL RESEARCH ORGANISATION",
					ZZD_StartDate = Constants.RefData_Common.MinimumDateTime,
					ZZD_EndDate = Constants.RefData_Common.MaximumDateTime,
					ZZD_ZZZ_NKDataGrouping = Constants.DataGrouping,
					RefCusCodeListAttributes = new RefCusCodeListAttribute[]
					{
						new RefCusCodeListAttribute()
						{
							ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.AQISPremisesPortCode,
							ZZE_Value = "AUBTB",
							ZZE_StartDate = Constants.RefData_Common.MinimumDateTime,
							ZZE_EndDate = Constants.RefData_Common.MaximumDateTime
						}
					}
				}
		};
	}
}
