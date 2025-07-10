using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TW.DataTransfer.Universal;

public class CustomsSupportingInformationDataObjectReader : Customs.DataTransfer.Universal.CustomsSupportingInformationDataObjectReader
{
	public CustomsSupportingInformationDataObjectReader(CustomsSupportingInformation supportingInfoDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid parentPK, ZString parentTableCode, GetMatchingDataPredicate getMatchingData = null) : base(supportingInfoDataObject, logger, factory, parentPK, parentTableCode, getMatchingData)
	{
	}

	protected override IEnumerable<ZString> GetCusSupportingInfoPropertiesToSuspendSetting()
	{
		var result = base.GetCusSupportingInfoPropertiesToSuspendSetting();
		return dataObject.ItemNumber.HasValue ? result.Append(CusSupportingInfo.Schema.CSI_ItemNumber) : result;
	}
}
