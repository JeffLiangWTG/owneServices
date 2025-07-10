using System.Collections.Immutable;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IManufacturerAdd
	{
		ZString FirmName { get; }
		ZString Street { get; }
		ZString City { get; }
		ZString Zip { get; }
		ZString Country { get; }
		ZString MID { get; }
		ZGuid AddressPK { get; }

		BusinessObjectFactory Factory { get; }
	}

	public class ManufacturerAddMessageData : AutoManufacturerAddMessageData, IManufacturerAdd, IAddressDetails
	{
		public ManufacturerAddMessageData(OrgHeaderWrapper wrapper)
			: base(wrapper.Factory)
		{
			this.wrapper = wrapper;
			using (SuspendSettingHasChanges())
			{
				US_OA_AddressDetails = wrapper.MailingAddress.PK;
			}
		}

		public readonly OrgHeaderWrapper wrapper;

		#region Properties

		[List(nameof(Lookups) + "." + nameof(ManufacturerAddMessageDataLookups.CountryList))]
		public override ZString US_Country
		{
			get { return base.US_Country; }
			set { base.US_Country = value.ToUpper(); }
		}

		[List(nameof(Lookups) + "." + nameof(ManufacturerAddMessageDataLookups.Addresses))]
		public override ZGuid US_OA_AddressDetails
		{
			get { return base.US_OA_AddressDetails; }
			set
			{
				ZGuid oldValue = US_OA_AddressDetails;
				base.US_OA_AddressDetails = value;
				if (!IsCopying && oldValue != US_OA_AddressDetails)
				{
					UpdateDetails();
				}
			}
		}

		public override ZString US_FirmName
		{
			get { return base.US_FirmName; }
			set { base.US_FirmName = value.ToUpper(); }
		}

		public override ZString US_Street
		{
			get { return base.US_Street; }
			set { base.US_Street = value.ToUpper(); }
		}

		public override ZString US_City
		{
			get { return base.US_City; }
			set { base.US_City = value.ToUpper(); }
		}

		public override ZString US_Zip
		{
			get { return base.US_Zip; }
			set { base.US_Zip = value.ToUpper(); }
		}

		public override ZString US_MID
		{
			get { return base.US_MID; }
			set { base.US_MID = value.ToUpper(); }
		}

		public OrgAddress AddressDetails
		{
			get { return Factory.Load<OrgAddress>(US_OA_AddressDetails); }
		}

		#endregion

		void UpdateDetails()
		{
			IAddressDetails addressDetails = AddressDetails;

			using (SuspendSettingHasChanges())
			{
				if (addressDetails != null)
				{
					US_FirmName = AddressDetails.EffectiveCompanyName;
					US_Street = new ZString(addressDetails.AddressLine1 + (!addressDetails.AddressLine1.IsEmpty && !addressDetails.AddressLine2.IsEmpty ? " " : "") + addressDetails.AddressLine2).Left(US_StreetInfo.MaxLength);
					US_City = addressDetails.City.Left(US_CityInfo.MaxLength);

					ZString countryToDefault = addressDetails.Country.Left(US_CountryInfo.MaxLength);

					if (countryToDefault == Core.Constants.CountryCodes.Canada)
					{
						string cAProvinceCode = CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(addressDetails.State);
						if (!string.IsNullOrEmpty(cAProvinceCode))
						{
							countryToDefault = cAProvinceCode;
						}
					}

					US_Country = countryToDefault;

					US_Zip = addressDetails.PostCode.KeepAlphanumericCharacters().Left(US_ZipInfo.MaxLength);

					CorrectCommonErrors();

					US_MID = new MIDGenerator().GenerateMID(US_FirmName, US_Street, US_City, US_Country);
				}
				else
				{
					US_FirmName = ZString.Empty;
					US_Street = ZString.Empty;
					US_City = ZString.Empty;
					US_Country = ZString.Empty;
					US_Zip = ZString.Empty;
					US_MID = ZString.Empty;
				}
			}
		}

		void CorrectCommonErrors() //should be called after set all address details and before MID generation
		{
			#region common errors with city name

			switch (US_Country)
			{
				case Core.Constants.CountryCodes.HongKong:
					US_City = HongKongCity;
					break;
				case Core.Constants.CountryCodes.Macau:
					US_City = MacaoCity;
					break;
				case Core.Constants.CountryCodes.Singapore:
					US_City = SingaporeCity;
					break;
				case Core.Constants.CountryCodes.Vatican:
					US_City = VaticanCity;
					break;
				case Core.Constants.CountryCodes.Monaco:
					US_City = MonacoCity;
					break;
				case Core.Constants.CountryCodes.SanMarino:
					US_City = SanMarinoCity;
					break;
				case Core.Constants.CountryCodes.Andorra:
					US_City = AndorraCity;
					break;
				case Core.Constants.CountryCodes.Austria:
					if (US_City.ToUpper() == WrongViennaName)
					{
						US_City = CorrectViennaName;
					}
					break;
				case Core.Constants.CountryCodes.Germany:
					if ((US_City.ToUpper() == WrongCologneName1) || (US_City.ToUpper() == WrongCologneName2))
					{
						US_City = CorrectCologneName;
					}
					if (US_City.ToUpper() == WrongMunichName)
					{
						US_City = CorrectMunichName;
					}
					break;
				case Core.Constants.CountryCodes.Italy:
					if (US_City.ToUpper() == WrongFlorenceName)
					{
						US_City = CorrectFlorenceName;
					}
					break;
				case Core.Constants.CountryCodes.Myanmar:
				case USCCountry.Burma:
					if (US_City.ToUpper() == WrongRangoonName)
					{
						US_City = CorrectRangoonName;
					}
					break;
				default:
					break;
			}

			#endregion

			#region common errors with country

			if (US_Country == Core.Constants.CountryCodes.Ireland)
			{
				bool cityIsLocatedOnNorthernIreland = false;
				foreach (string city in northernIrelandCities)
				{
					if (US_City.EqualsIgnoringCase(city))
					{
						cityIsLocatedOnNorthernIreland = true;
						break;
					}
				}

				if (cityIsLocatedOnNorthernIreland)
				{
					US_Country = Core.Constants.CountryCodes.UnitedKingdom;
				}
			}

			#endregion
		}
		internal const string HongKongCity = "HONG KONG";
		internal const string MacaoCity = "MACAO";
		internal const string SingaporeCity = "SINGAPORE";
		internal const string VaticanCity = "VATICAN";
		internal const string MonacoCity = "MONACO";
		internal const string SanMarinoCity = "SAN MARINO";
		internal const string AndorraCity = "ANDORRA";

		internal const string WrongViennaName = "WIEN";
		internal const string CorrectViennaName = "VIENNA";
		internal const string WrongCologneName1 = "KOLN";
		internal const string WrongCologneName2 = "KOELN";
		internal const string CorrectCologneName = "COLOGNE";
		internal const string WrongMunichName = "MUENCHEN";
		internal const string CorrectMunichName = "MUNICH";
		internal const string WrongFlorenceName = "FIRENZE";
		internal const string CorrectFlorenceName = "FLORENCE";
		internal const string WrongRangoonName = "YANGON";
		internal const string CorrectRangoonName = "RANGOON";

		#region Northern Ireland Cities list

		readonly static ImmutableArray<string> northernIrelandCities = ImmutableArray.Create(
			"Antrim",
			"Armagh",
			"Banbridge",
			"Bangor",
			"Belfast",
			"Cookstown",
			"Craigavon",
			"Cushendall",
			"Derry",
			"Dundonald",
			"Dungannon",
			"Dundrum",
			"Enniskillen",
			"Irvinestown",
			"Kilkeel",
			"Lisburn",
			"Londonderry",
			"Lurgan",
			"Newry",
			"Newtonabbey",
			"Omagh",
			"Portadown",
			"Strabane"
		);

		#endregion

		public ManufacturerAddMessageDataLookups Lookups
		{
			get { return lookups ?? (lookups = new ManufacturerAddMessageDataLookups(this)); }
		}
		ManufacturerAddMessageDataLookups lookups;

		public bool IsUS
		{
			get { return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(US_Country) == Core.Constants.CountryCodes.UnitedStates; }
		}

		public bool IsCA
		{
			get { return CanadaProvinceTerritoryCodes.IsCanadianProvince(US_Country); }
		}

		public bool IsMX
		{
			get { return US_Country == Core.Constants.CountryCodes.Mexico; }
		}

		#region IManufacturerAdd Members

		ZString IManufacturerAdd.FirmName
		{
			get { return US_FirmName; }
		}

		ZString IManufacturerAdd.Street
		{
			get { return US_Street; }
		}

		ZString IManufacturerAdd.City
		{
			get { return US_City; }
		}

		ZString IManufacturerAdd.Country
		{
			get { return US_Country; }
		}

		ZString IManufacturerAdd.MID
		{
			get { return US_MID; }
		}

		ZString IManufacturerAdd.Zip
		{
			get { return IsCA ? US_Zip.Replace(" ", "") : US_Zip; }
		}

		ZGuid IManufacturerAdd.AddressPK
		{
			get { return US_OA_AddressDetails; }
		}

		BusinessObjectFactory IManufacturerAdd.Factory
		{
			get { return Factory; }
		}

		ZString IAddressDetails.CompanyName => US_FirmName;

		ZString IAddressDetails.ContactName => ZString.Empty;

		ZString IAddressDetails.Phone => ZString.Empty;

		ZString IAddressDetails.Fax => ZString.Empty;

		ZString IAddressDetails.Email => ZString.Empty;

		ZString IAddressDetails.AddressLine1 => US_Street;

		ZString IAddressDetails.AddressLine2 => ZString.Empty;

		ZString IAddressDetails.City => US_City;

		ZString IAddressDetails.State => ZString.Empty;

		ZString IAddressDetails.PostCode => US_Zip;

		ZString IAddressDetails.Country => US_Country;

		#endregion
	}
}
