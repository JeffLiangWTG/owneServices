using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Manifest.Business;

public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
{
	public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent) : base(parent)
	{
	}

	AsycudaManifestHeader ManifestHeader => Parent as AsycudaManifestHeader;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateAMA_DriverName();
		ValidateAMA_DriverCommunicationId();
	}

	protected override void CheckAMA_TransportMeans()
	{
		base.CheckAMA_TransportMeans();
		ListValidation.MessageErrorIfInvalidCode(ManifestHeader.AMA_TransportMeansInfo);
		MandatoryValidation.MessageErrorIfNotEntered(ManifestHeader.AMA_TransportMeansInfo);
	}

	protected override void CheckAMA_RN_NKConveyanceNationality()
	{
		base.CheckAMA_RN_NKConveyanceNationality();
		MandatoryValidation.MessageErrorIfNotEntered(ManifestHeader.AMA_RN_NKConveyanceNationalityInfo);
	}

	internal void ValidateAMA_DriverName()
	{
		ValidateCalculatedProperty(ManifestHeader.AMA_DriverNameInfo);
	}

	protected override void CheckAMA_DateAtCustomsOffice()
	{
		base.CheckAMA_DateAtCustomsOffice();
		MandatoryValidation.MessageErrorIfNotEntered(ManifestHeader.AMA_DateAtCustomsOfficeInfo);
	}

	protected void CheckAMA_DriverName()
	{
		MandatoryValidation.MessageErrorIfNotEntered(ManifestHeader.AMA_DriverNameInfo);
	}

	internal void ValidateAMA_DriverCommunicationId()
	{
		ValidateCalculatedProperty(ManifestHeader.AMA_DriverCommunicationIdInfo);
	}

	protected void CheckAMA_DriverCommunicationId()
	{
		MandatoryValidation.MessageErrorIfNotEntered(ManifestHeader.AMA_DriverCommunicationIdInfo);
	}

	protected override void CheckAMA_OA_Carrier()
	{
		base.CheckAMA_OA_Carrier();
		CheckAMA_OA_Carrier_Ok_CodeType();
		CheckAMA_OA_Carrier_OrgAddress_PhoneOrEmail();
	}

	protected override void CheckAMA_OA_CarrierMandatory()
	{
		MandatoryValidation.MessageErrorIfNotEntered(ManifestHeader.AMA_OA_CarrierInfo);
	}

	void CheckAMA_OA_Carrier_Ok_CodeType()
	{
		if (!ManifestHeader.AMA_OA_Carrier.IsEmpty)
		{
			if (!IsOrgAddressContainsRequiredCodeType(ManifestHeader.Carrier, RequiredCodeTypeListForForwarder))
			{
				ManifestHeader.AMA_OA_CarrierInfo.AddMessageError(
					Res.GetString("33838DDC-29E2-4F46-A789-8EF422BBF809", "The carrier must have an ID of type MVA, ORG or EOR for usage in DMO manifest. See the organization’s tab Config – Registration Numbers/Codes"));
			}
		}
	}

	static bool IsOrgAddressContainsRequiredCodeType(OrgAddress orgAddress, ImmutableArray<string> codeTypes) =>
		orgAddress is { Header.CustomsCodes: { } cusCodes } &&
		cusCodes.Any(cusCode => codeTypes.Contains(cusCode.OK_CodeType));

	void CheckAMA_OA_Carrier_OrgAddress_PhoneOrEmail()
	{
		if (ManifestHeader.AMA_OA_Carrier.IsEmpty || ManifestHeader.Carrier?.Header is null)
		{
			return;
		}

		if (!ManifestHeader.Carrier.Header.Addresses
			.Where(x => x.IsAddressOfType(OrgAddressType.CustomsAddressOfRecord) || x.IsAddressOfType(OrgAddressType.Office))
			.Any(x => !x.OA_Email.IsEmpty || !x.OA_Phone.IsEmpty))
		{
			ManifestHeader.AMA_OA_CarrierInfo.AddMessageError(Res.GetString("18F9F026-FA0B-4B08-9206-0D705BDCE9F4", "The carrier must have phone number or an email address in an address of type 'Customs Address of Record' or 'Office Address' for usage in DMO manifest. See the Organization’s Address tab"));
		}
	}

	static ImmutableArray<string> RequiredCodeTypeListForForwarder => ImmutableArray.Create(
		OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
		OrgCusCode.CodeTypes.OrganizationNumber,
		OrgCusCode.NorwayCodeTypes.MVA
	);
}
