
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.eTail.Business;

class ESCustomsStatusStore : DefaultCustomsStatusStore
{
	public ESCustomsStatusStore(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override IEnumerable<CustomsStatusInfo> GetCustomsStatusListForCodeType(string codeType)
	{
		var customsStatusList = base.GetCustomsStatusListForCodeType(codeType);
		var multipleStatus = new CustomsStatusInfo(CommonEntryStatusList.Codes.MultipleEntryStatus, codeType, MultipleEntryStatusDescription, HVLVReleaseStatus.Held);

		return customsStatusList.Append(multipleStatus);
	}

	protected override string GetCodeTypeForImport(bool hasFormalDeclaration)
	{
		return RefCusCodeListTypes.Codes.CustomsStatus;
	}

	protected override string[] ImportCodeTypes => new[] { RefCusCodeListTypes.Codes.CustomsStatus };

	protected override ZString CountryCode => CountryCodes.Spain;

	string MultipleEntryStatusDescription => Res.GetString("4c1079c4-9091-4aad-9438-00468bd504b0", "Multiple");
}
