using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class AddressInformationDocumentWrapper : NonPersistentBusinessObject, IAddressDocumentInformation
{
	public ZString Name { get; private set; }

	public ZString Address { get; private set; }

	public ZString City { get; private set; }

	public ZString PostCode { get; private set; }

	public ZString CountryCode { get; private set; }

	public ZString EORINumber { get; private set; }

	public ZString VATNumber { get; private set; }

	public AddressInformationDocumentWrapper(OrgHeader organisation)
	{
		var address = organisation?.CustomsAddress;
		if (address != null)
		{
			SetOrgAddress(address);
		}
		else
		{
			SetEmptyAddress();
		}
	}

	public AddressInformationDocumentWrapper(OrgAddress address)
	{
		SetOrgAddress(address);
	}

	public AddressInformationDocumentWrapper(JobDocAddress docAddress)
	{
		if (docAddress != null)
		{
			if (docAddress.HasRealAddress)
			{
				SetOrgAddress(docAddress.Address);
			}
			else
			{
				SetJobDocAddress(docAddress);
			}
		}
		else
		{
			SetEmptyAddress();
		}
	}

	public AddressInformationDocumentWrapper(CusGoodsLocation goodsLocation)
	{
		if (goodsLocation != null)
		{
			SetJobDocAddress(goodsLocation.Address);
			if (goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.PostcodeAddress)
			{
				Address = goodsLocation.AdditionalIdentifier;
			}
		}
		else
		{
			SetEmptyAddress();
		}
	}

	void SetOrgAddress(OrgAddress address)
	{
		if (address != null)
		{
			Name = address.CompanyName;
			Address = address.Address1AndAddress2;
			City = address.City;
			PostCode = address.Postcode;
			CountryCode = address.OA_RN_NKCountryCode;
			EORINumber = address.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			if (EORINumber.IsEmpty)
			{
				EORINumber = ZString.Empty;
			}
			VATNumber = address.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW);
			if (VATNumber.IsEmpty)
			{
				VATNumber = ZString.Empty;
			}
		}
		else
		{
			SetEmptyAddress();
		}
	}

	void SetJobDocAddress(JobDocAddress jobDocAddress)
	{
		if (jobDocAddress != null)
		{
			Name = jobDocAddress.E2_CompanyName;
			Address = jobDocAddress.E2_Address1AndE2_Address2;
			City = jobDocAddress.E2_City;
			PostCode = jobDocAddress.E2_Postcode;
			CountryCode = jobDocAddress.E2_RN_NKCountryCode;
			EORINumber = jobDocAddress.E2_GovRegNum;
			if (EORINumber.IsEmpty)
			{
				EORINumber = ZString.Empty;
			}
			VATNumber = ZString.Empty;
		}
		else
		{
			SetEmptyAddress();
		}
	}

	void SetEmptyAddress()
	{
		Name = ZString.Empty;
		Address = ZString.Empty;
		City = ZString.Empty;
		PostCode = ZString.Empty;
		CountryCode = ZString.Empty;
		EORINumber = ZString.Empty;
		VATNumber = ZString.Empty;
	}
}
