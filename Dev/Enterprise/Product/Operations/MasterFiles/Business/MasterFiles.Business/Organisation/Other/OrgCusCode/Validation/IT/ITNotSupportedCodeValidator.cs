using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	sealed class ITNotSupportedCodeValidator : IITCusCodeValidator
	{
		public ITCusCodeValidationResult Validate(ZString code)
		{
			return ITCusCodeValidationResult.InvalidLength;
		}
	}
}
