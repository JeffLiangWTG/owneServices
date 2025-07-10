using System.Collections.Generic;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.NO.Manifest.Business;

class ZZDatabaseValidationHelper : ASYCUDA.Business.ZZDatabaseValidationHelper
{
	public ZZDatabaseValidationHelper(AsycudaManifestHeader header) : base(header)
	{
	}

	protected override bool GetMandatoryFieldsInZZ => false;

	protected override Dictionary<string, MandatoryValidationRule> GetMandatoryFieldsCore()
	{
		var mandatoryFields = base.GetMandatoryFieldsCore();
		mandatoryFields[ManifestValidationRuleCodes.Consignor] = new MandatoryValidationRule(Res.GetString("NO.Manifest.ZZDatabaseValidationHelper|Consignor", "A Shipper is required"), () => true);
		return mandatoryFields;
	}
}
