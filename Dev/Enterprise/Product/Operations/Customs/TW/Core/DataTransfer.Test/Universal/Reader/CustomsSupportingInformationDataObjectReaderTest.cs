using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.DataTransfer.Testing;

[TestedType(typeof(CustomsSupportingInformationDataObjectReader))]
sealed class CustomsSupportingInformationDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
{
	public void TestPermitCusSupporting_ItemNumberSetter()
	{
		var information = new CustomsSupportingInformation
		{
			Category = new CodeDescriptionPair() { Code = CusSupportingInfoTypeList.Codes.PermitNumber, Description = "PermitNumber" },
			Type = new CodeDescriptionPair6Char { Code = "Code1", Description = "Desc" },
		};
		var reader = new CustomsSupportingInformationDataObjectReaderForTest(information, new TestErrorLogger(), Factory, ZGuid.Empty, "");
		AssertEquals("ItemNumber has no value", false, reader.GetCusSupportingInfoPropertiesToSuspendSetting_Exposed().Contains(CusSupportingInfo.Schema.CSI_ItemNumber));

		information.ItemNumber = 1;
		reader = new CustomsSupportingInformationDataObjectReaderForTest(information, new TestErrorLogger(), Factory, ZGuid.Empty, "");
		AssertEquals("ItemNumber has value", true, reader.GetCusSupportingInfoPropertiesToSuspendSetting_Exposed().Contains(CusSupportingInfo.Schema.CSI_ItemNumber));
	}
}

class CustomsSupportingInformationDataObjectReaderForTest : CustomsSupportingInformationDataObjectReader
{
	public CustomsSupportingInformationDataObjectReaderForTest(CustomsSupportingInformation supportingInfoDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid parentPK, ZString parentTableCode, GetMatchingDataPredicate getMatchingData = null) : base(supportingInfoDataObject, logger, factory, parentPK, parentTableCode, getMatchingData)
	{
	}

	public IEnumerable<ZString> GetCusSupportingInfoPropertiesToSuspendSetting_Exposed()
	{
		return base.GetCusSupportingInfoPropertiesToSuspendSetting();
	}
}
