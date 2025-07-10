using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.PL.NCTS.Business;

internal static class JobDocAddressValidationFactory
{
	public static ZValidation GetValidation(JobDocAddress addressToValidate, NctsHeader header)
	{
		switch (Argument.NotNull(addressToValidate, nameof(addressToValidate)).DocAddressType)
		{
			case DocAddressType.Principal:
				return new TraderJobDocAddressValidation(addressToValidate, Constants.ValidationCaptions.PrincipalCaption, header);
			case DocAddressType.Representative:
				return new TraderJobDocAddressValidation(addressToValidate, Constants.ValidationCaptions.RepresentativeCaption, header);
			default:
				return new NctsJobDocAddressValidation(addressToValidate, header);
		}
	}
}
