using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IEquivalentComplianceSubTypeProvider
	{
		string GetEquivalentComplianceSubType(ZString complianceSubType);
	}
}
