using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface ITaxIdAndTaxMessageMappingHelper
	{
		ResourceString ValidateMapping(string lineType, AccTaxRate taxId, AccInvMsg taxMessage);
		ResourceString ValidateMappingForLine(AccTransactionLines line);
		ResourceString ValidateMappingForTaxOverride(string[] lineTypes, AccChargeTaxOverride chargeTaxOverride);
	}
}
