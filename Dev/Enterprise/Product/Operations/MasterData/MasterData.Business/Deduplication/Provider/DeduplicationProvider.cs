using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationProvider
	{
		public static class Constants
		{
			#region Type Constants

			public const string Organisations = nameof(Organisations);
			public const string Person = nameof(Person);
			public const string Staff = nameof(Staff);
			public const string Applicant = nameof(Applicant);
			public const string OrganisationNames = nameof(OrganisationNames);
			public const string PersonNames = nameof(PersonNames);
			public const string PhoneNumbers = nameof(PhoneNumbers);
			public const string Emails = nameof(Emails);
			public const string Birthdays = nameof(Birthdays);
			public const string Websites = nameof(Websites);
			public const string Domains = nameof(Domains);
			public const string Contacts = nameof(Contacts);
			public const string Addresses = nameof(Addresses);
			public const string RegistrationCodes = nameof(RegistrationCodes);
			public const string ActiveAssociations = nameof(ActiveAssociations);

			#endregion Type Constants

			#region Detail Constants

			public const string Similarity = nameof(Similarity);
			public const string ConfidenceScore = nameof(ConfidenceScore);
			public const string Source = nameof(Source);
			public const string Active = nameof(Active);
			public const string PrimaryWorkplace = nameof(PrimaryWorkplace);
			public const string Name = nameof(Name);
			public const string Email = nameof(Email);
			public const string Birthday = nameof(Birthday);
			public const string Website = nameof(Website);
			public const string Number = nameof(Number);
			public const string Domain = nameof(Domain);
			public const string Type = nameof(Type);
			public const string Code = nameof(Code);
			public const string Country = nameof(Country);
			public const string Address = nameof(Address);
			public const string UNLOCO = nameof(UNLOCO);
			public const string City = nameof(City);
			public const string State = nameof(State);
			public const string Coordinates = nameof(Coordinates);
			public const string Brand = nameof(Brand);
			public const string ContactPhone = nameof(ContactPhone);
			public const string AddressPhone = nameof(AddressPhone);
			public const string ContactMobile = nameof(ContactMobile);
			public const string AddressMobile = nameof(AddressMobile);
			public const string OtherPhone = nameof(OtherPhone);
			public const string HomePhone = nameof(HomePhone);
			public const string ContactFax = nameof(ContactFax);
			public const string AddressFax = nameof(AddressFax);
			public const string CusCodeCustomsRegNo = nameof(CusCodeCustomsRegNo);
			public const string PersonPhone = nameof(PersonPhone);
			public const string PersonWorkPhone = nameof(PersonWorkPhone);
			public const string PersonMobile = nameof(PersonMobile);
			public const string PersonFax = nameof(PersonFax);

			#endregion Detail Constants
		}

		public ConfidenceRating GetConfidenceForScore(double score)
		{
			if (score <= 0.25)
			{
				return ConfidenceRating.None;
			}
			else if (score <= 0.5)
			{
				return ConfidenceRating.Low;
			}
			else if (score <= 0.8)
			{
				return ConfidenceRating.Medium;
			}
			else if (score <= 0.99)
			{
				return ConfidenceRating.High;
			}
			else
			{
				return ConfidenceRating.Exact;
			}
		}

		public DeduplicationDisplayMode GetDisplayModeForType(Type type)
		{
			var provider = GetGlowProvider(type);

			return provider?.DisplayModeForType ?? DeduplicationDisplayMode.Undefined;
		}

		public string GetGroupNameForType(Type type)
		{
			var provider = GetGlowProvider(type);

			return provider != null ? provider.GroupNameForType : string.Empty;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			var heading = string.Empty;

			if (source != null)
			{
				var provider = GetBizOProvider(source.GetType());

				if (provider != null)
				{
					heading = provider.GetHeading(source);
				}
			}

			return heading;
		}

		public string GetTargetHeading(object targets, Guid pk, Type objectType, HeaderType headerType = HeaderType.Short)
		{
			var heading = string.Empty;
			var provider = GetBizOProvider(objectType, true);

			if (provider != null)
			{
				heading = provider.GetHeading(targets, pk, headerType);
			}

			return heading;
		}

		public string GetMasterHeading(IDeduplicationMaster parent, Guid childObjectPK, Type childObjectType)
		{
			var source = GetChildObject(parent, childObjectPK, childObjectType);
			return GetHeading(source as IDeduplicationGlowObject);
		}

		object GetChildObject(IDeduplicationMaster parent, Guid childObjectPK, Type childObjectType)
		{
			var provider = GetBizOProvider(childObjectType, true);
			return provider?.GetChildObject(parent, childObjectPK);
		}

		public string GetTargetComparisonValue(IEnumerable<IDeduplicationGlowObject> master, Guid pk, Type type, IEnumerable<string> columnNames, bool standardizeDomains = false)  // TODO - Refactor to standardization instructions
		{
			if (pk != Guid.Empty)
			{
				var provider = GetGlowProvider(type);

				if (provider != null)
				{
					var source = provider.GetComparisonSource(master, pk, columnNames, standardizeDomains);

					return source != null ? string.Join(" ", columnNames.Select(columnName => GetPropertyValue(source, columnName, standardizeDomains))) : string.Empty;
				}
			}

			return string.Empty;
		}

		public string GetMasterComparisonValue(object parent, Guid childObjectPK, Type childObjectType, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			var source = GetChildObject(parent as IDeduplicationMaster, childObjectPK, childObjectType);

			return source != null ?
				string.Join(" ", columnNames.Select(columnName => GetPropertyValue(source, columnName, standardizeDomains))) : string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Round is ok for our purposes")]
		string GetPropertyValue(object source, string propertyName, bool standardizeDomains = false)
		{
			var properties = GetSourceProperties(source);

			PropertyInfo property;
			if (!properties.TryGetValue(propertyName, out property))
			{
				var devMessage = Res.GetString("8ea12948-76f9-4ab2-8218-94c2a8686d18", "Accessing key: {0} of {1}", propertyName, source.GetType().ToString());
				ExceptionReporter.Instance.ReportDeveloperException("9e596c6f-de7d-4f76-9ed4-4d573ec68322", devMessage, new KeyNotFoundException(devMessage));
				return string.Empty;
			}

			var rawValue = property.GetValue(source);
			var valueStr = rawValue.ToString();

			if (BirthdayFields.Contains(propertyName) && rawValue is DateTime rawDateTime)
			{
				valueStr = rawDateTime.ToShortDateString();
			}

			if (propertyName == Constants.Address)
			{
				valueStr = string.Join(" ", (rawValue as IList<(string ColName, string Value)>).Select(pair => pair.Value));
			}

			if (propertyName == "OA_Latitude" ||
				propertyName == "OA_Longitude")
			{
				if (rawValue is decimal coordinate)
				{
					valueStr = Math.Round(coordinate, 3).ToString(CultureInfo.InvariantCulture); // Round is ok for our purposes
				}
			}
			else if (standardizeDomains &&
				(propertyName == OrgAddressSchema.Constants.OA_Email ||
				propertyName == OrgContactSchema.Constants.OC_Email))
			{
				valueStr = TextStandardizerHelper.ExtractEmailDomain(valueStr);
			}
			else if (standardizeDomains &&
				propertyName == OrgWebURLSchema.Constants.PU_URL)
			{
				valueStr = TextStandardizerHelper.ExtractUrlDomain(valueStr);
			}

			if (PhoneFields.Contains(propertyName))
			{
				var valueStrFormatted = (phoneNumberFormatter = phoneNumberFormatter ?? new PhoneNumberFormatterAndValidator()).FormatInternational(valueStr, Environment.Env.CurrentCompany.Country.Code).ToString();
				valueStr = string.IsNullOrEmpty(valueStrFormatted) ? valueStr : valueStrFormatted;
			}

			return valueStr;
		}

		PhoneNumberFormatterAndValidator phoneNumberFormatter;

		Dictionary<string, PropertyInfo> GetSourceProperties(object source)
		{
			return source
				.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.GroupBy(property => property.Name)
				.Select(grouping => grouping.First())
				.ToDictionary(property => property.Name);
		}

		public string GetDisplayNameForColumns(IEnumerable<string> columnNames)
		{
			columnNames = columnNames.ToArray();

			if (columnNames.Count() == 1 && columnNames.FirstOrDefault() == OrgAddressSchema.Constants.OA_GeoLocation)
			{
				return Constants.Coordinates;
			}

			if (columnNames.Count() == 1 && columnNames.FirstOrDefault() == Constants.Address)
			{
				return Constants.Address;
			}

			if (columnNames.Count() == 6 &&
				columnNames.Contains(OrgAddressSchema.Constants.OA_Address1) &&
				columnNames.Contains(OrgAddressSchema.Constants.OA_Address2) &&
				columnNames.Contains(OrgAddressSchema.Constants.OA_City) &&
				columnNames.Contains(OrgAddressSchema.Constants.OA_State) &&
				columnNames.Contains(OrgAddressSchema.Constants.OA_PostCode) &&
				columnNames.Contains(OrgAddressSchema.Constants.OA_RN_NKCountryCode))
			{
				return Constants.Address;
			}

			return string.Join(" ", columnNames.Select(column => GetDisplayNameForColumn(column)));
		}

		string GetDisplayNameForColumn(string columnName)
		{
			if (columnMappings.TryGetValue(columnName, out string displayName))
			{
				return displayName;
			}

			return columnName;
		}

		readonly Dictionary<string, string> columnMappings = new Dictionary<string, string>
		{
			[OrgHeaderSchema.Constants.OH_FullName] = Constants.Name,
			[OrgContactSchema.Constants.OC_ContactName] = Constants.Name,
			[GlbStaffSchema.Constants.GS_FullName] = Constants.Name,
			[GlbPersonSchema.Constants.PER_FullName] = Constants.Name,
			[OrgBrandOrRelatedNameSchema.Constants.P1_RelatedName] = Constants.Brand,
			[OrgContactSchema.Constants.OC_Phone] = Constants.ContactPhone,
			[OrgAddressSchema.Constants.OA_Phone] = Constants.AddressPhone,
			[OrgContactSchema.Constants.OC_Mobile] = Constants.ContactMobile,
			[OrgAddressSchema.Constants.OA_Mobile] = Constants.AddressMobile,
			[OrgAddressSchema.Constants.OA_Email] = Constants.Email,
			[OrgContactSchema.Constants.OC_Email] = Constants.Email,
			[OrgContactSchema.Constants.OC_OtherPhone] = Constants.OtherPhone,
			[OrgContactSchema.Constants.OC_HomePhone] = Constants.HomePhone,
			[OrgAddressSchema.Constants.OA_Fax] = Constants.AddressFax,
			[OrgContactSchema.Constants.OC_Fax] = Constants.ContactFax,
			[OrgContactSchema.Constants.OC_Birthday] = Constants.Birthday,
			[OrgWebURLSchema.Constants.PU_URL] = Constants.Website,
			[OrgCusCodeSchema.Constants.OK_CodeType] = Constants.Type,
			[OrgCusCodeSchema.Constants.OK_RN_NKCodeCountry] = Constants.Country,
			[OrgCusCodeSchema.Constants.OK_CustomsRegNo] = Constants.CusCodeCustomsRegNo,
			[GlbPersonSchema.Constants.PER_EmailAddress] = Constants.Email,
			[GlbPersonSchema.Constants.PER_EmailAddress2] = Constants.Email,
			[GlbPersonSchema.Constants.PER_HomeAddress1] = Constants.Addresses,
			[GlbPersonSchema.Constants.PER_HomeAddress2] = Constants.Addresses,
			[GlbPersonSchema.Constants.PER_HomePhone] = Constants.PersonPhone,
			[GlbPersonSchema.Constants.PER_MobilePhone] = Constants.PersonMobile,
			[GlbPersonSchema.Constants.PER_MobilePhone2] = Constants.PersonMobile,
			[GlbPersonSchema.Constants.PER_FaxNumber] = Constants.PersonFax,
			[GlbPersonSchema.Constants.PER_BirthDate] = Constants.Birthday,
			[GlbStaffSchema.Constants.GS_EmailAddress] = Constants.Email,
			[GlbStaffSchema.Constants.GS_HomePhone] = Constants.PersonPhone,
			[GlbStaffSchema.Constants.GS_MobilePhone] = Constants.PersonMobile,
			[GlbStaffSchema.Constants.GS_WorkPhone] = Constants.PersonWorkPhone,
			[GlbStaffSchema.Constants.GS_FaxNum] = Constants.PersonFax,
			[GlbStaffSchema.Constants.GS_Birthdate] = Constants.Birthday,
			[HRJobApplicantSchema.Constants.HA_EmailAddress] = Constants.Email,
			[HRJobApplicantSchema.Constants.HA_WorkPhone] = Constants.PersonWorkPhone,
		};

		readonly string[] phoneConstants = { Constants.ContactPhone, Constants.AddressPhone, Constants.ContactMobile, Constants.AddressMobile, Constants.OtherPhone, Constants.HomePhone, Constants.AddressFax, Constants.ContactFax, Constants.PersonPhone, Constants.PersonMobile, Constants.PersonFax, Constants.PersonWorkPhone };

		HashSet<string> phoneFields;
		HashSet<string> PhoneFields => phoneFields = phoneFields ?? new HashSet<string>(columnMappings.Where(u => phoneConstants.Contains(u.Value)).Select(v => v.Key));

		List<string> birthdayFields;
		List<string> BirthdayFields => birthdayFields = birthdayFields ?? new List<string>(columnMappings.Where(u => u.Value == Constants.Birthday).Select(v => v.Key).ToList());

		List<IDeduplicationMultiSourceProvider> glowProvider;
		List<IDeduplicationBizoProvider> bizoProvider;

		IEnumerable<IDeduplicationMultiSourceProvider> GetAllGlowProviders()
		{
			if (glowProvider == null)
			{
				glowProvider = new List<IDeduplicationMultiSourceProvider>
				{
					new MultiSourceDeduplicationProvider(this, typeof(MultiSourceCompanyName), Constants.OrganisationNames),
					new MultiSourceDeduplicationProvider(this, typeof(MultiSourcePersonName), Constants.PersonNames),
					new MultiSourceDeduplicationProvider(this, typeof(MultiSourceDomain), Constants.Domains),
					new MultiSourceDeduplicationProvider(this, typeof(MultiSourcePhoneNumber), Constants.PhoneNumbers),
					new MultiSourceDeduplicationProvider(this, typeof(MultiSourceEmail), Constants.Emails),
					new MultiSourceDeduplicationProvider(this, typeof(MultiSourceAddress), Constants.Addresses),
					new MultiSourceDeduplicationProvider(this, typeof(MultiSourceBirthday), Constants.Birthdays)
				};

				glowProvider.AddRange(GetAllBusinessObjectProviders());
			}

			return glowProvider;
		}

		IEnumerable<IDeduplicationBizoProvider> GetAllBusinessObjectProviders()
		{
			return bizoProvider ?? (bizoProvider = new List<IDeduplicationBizoProvider>
			{
				new OrgWebUrlDeduplicationProvider(),
				new OrgHeaderDeduplicationProvider(),
				new OrgCusCodeDeduplicationProvider(),
				new OrgContactDeduplicationProvider(),
				new OrgBrandOrRelatedNameDeduplicationProvider(),
				new OrgAddressDeduplicationProvider(),
				new GlbPersonDeduplicationProvider(),
				new GlbStaffDeduplicationProvider(),
				new HRJobApplicantDeduplicationProvider(),
			});
		}

		IDeduplicationMultiSourceProvider GetGlowProvider(Type providerType)
		{
			return GetAllGlowProviders().FirstOrDefault(provider => provider.GlowType.IsAssignableFrom(providerType));
		}

		internal IDeduplicationBizoProvider GetBizOProvider(Type providerType, bool isGlowType = false)
		{
			if (isGlowType)
			{
				return (IDeduplicationBizoProvider)GetGlowProvider(providerType);
			}

			return GetAllBusinessObjectProviders().FirstOrDefault(provider => provider.BusinessObjectType.IsAssignableFrom(providerType));
		}

		internal IDeduplicationBizoProvider GetBizOProvider(string tablePrefix)
		{
			return GetAllBusinessObjectProviders().FirstOrDefault(provider => provider.TablePrefix == tablePrefix);
		}
	}
}
