namespace Enterprise.MasterFiles.Business;

public interface IAddressesValidation
{
	ISupportWebAddressValidation[] AddressesToValidate { get; }
}
