using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class EntryOrganisationsInfo : IOrganisation
	{
		public EntryOrganisationsInfo(OrgHeader organisation)
		{
			if (organisation == null)
			{
				throw new ArgumentNullException(nameof(organisation));
			}

			this.organisation = organisation;
			this.orgMainAddress = new EntryOrgAddressInfo(organisation.Addresses.MainAddress);
		}
		readonly OrgHeader organisation;

		public EntryOrganisationsInfo(OrgHeader organisation, ZString nameOverride)
			: this(organisation)
		{
			this.nameOverride = nameOverride;
		}
		readonly ZString nameOverride = "";

		#region IOrganisation Members

		public ZString Name
		{
			get { return nameOverride.IsEmpty ? organisation.OH_FullName : nameOverride; }
		}

		public ZString UEN
		{
			get { return organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber); }
		}

		public IAddress Address
		{
			get { return orgMainAddress; }
		}
		readonly IAddress orgMainAddress;

		#endregion

		class EntryOrgAddressInfo : IAddress
		{
			public EntryOrgAddressInfo(OrgAddress address)
			{
				this.address = address;
			}

			readonly OrgAddress address;

			ZString IAddress.FullAddress
			{
				get { return address.AddressAsASingleLineWithoutCompanyName; }
			}

			ZString IAddress.FullAddressWithoutAdditionalInfo
			{
				get { return address.AddressAsASingleLineWithoutCompanyNameAndAdditionalAddressInfo; }
			}

			ZString IAddress.City
			{
				get { return address.CityFallback; }
			}

			ZString IAddress.PostCode
			{
				get { return address.OA_PostCode; }
			}

			ZString IAddress.CountryCode
			{
				get { return address.RelatedCountry != null ? address.RelatedCountry.Code : ZString.Empty; }
			}

			ZString IAddress.SubdivisionCode
			{
				get { return address.OA_State; }
			}

			ZString IAddress.SubdivisionName
			{
				get
				{
					ZString result = "";

					if (address.RelatedState != null)
					{
						result = address.RelatedState.RW_DescriptionMultilingual;
					}

					return result;
				}
			}
		}
	}
}
