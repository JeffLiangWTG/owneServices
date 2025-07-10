using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IITCusCodeValidator
	{
		ITCusCodeValidationResult Validate(ZString code);
	}

	public enum ITCusCodeValidationResult
	{
		InvalidLength,
		InvalidPattern,
		InvalidCheckDigit,
		Valid,
	}
}
