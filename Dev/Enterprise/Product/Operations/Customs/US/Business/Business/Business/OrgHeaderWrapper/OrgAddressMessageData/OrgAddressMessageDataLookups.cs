using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class OrgAddressMessageDataLookups
	{
		public OrgAddressMessageDataLookups(OrgAddressMessageData messageData)
		{
			this.messageData = messageData;
			this.factory = messageData.Factory;
		}

		readonly OrgAddressMessageData messageData;
		readonly BusinessObjectFactory factory;

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(factory); }
		}

		public OrgAddressDependentCollection Addresses
		{
			get { return messageData.wrapper.organisation.Addresses; }
		}

		public OrgContactDependentCollection Contacts
		{
			get { return messageData.wrapper.organisation.Contacts; }
		}

		public GlbStaffCollection CusAgents
		{
			get { return new GlbStaffCollection(factory); }
		}

		public ImporterADDNameQualifierList NameQualifierList
		{
			get { return factory.GetCachedValue<ImporterADDNameQualifierList>(); }
		}

		public CodeDescriptionPairList ImporterTypesList => factory.GetCachedValue<ImporterTypeList>();

		public CodeDescriptionPairList ImporterNumberTypeList
		{
			get
			{
				return factory.GetCachedValue("ImporterNumberTypeList", delegate
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();

					result.AddPair(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "Employer Identification Number");
					result.AddPair(OrgCusCode.USACodeTypes.SocialSecurityNumber, "Social Security Number");
					result.AddPair(OrgCusCode.USACodeTypes.CBPAssignedNumber, "CBP Assigned Number");

					return result;
				});
			}
		}

		public AESMexicoStates MappedMXStateCode
		{
			get { return factory.GetCachedValue<AESMexicoStates>(); }
		}

		public ImporterADDActionCodeList ActionCodeList
		{
			get { return factory.GetCachedValue<ImporterADDActionCodeList>(); }
		}

		public CodeDescriptionPairList StateAddress1List
		{
			get { return GetStateAddressList(messageData.US_RN_NKCountry1); }
		}

		public CodeDescriptionPairList StateAddress2List
		{
			get { return GetStateAddressList(messageData.US_RN_NKCountry2); }
		}

		public CodeDescriptionPairList BankCountryStateList
		{
			get { return GetStateAddressList(messageData.US_BankCountry); }
		}

		public CodeDescriptionPairList CertificateCountryStateList
		{
			get { return GetStateAddressList(messageData.US_CountryISOCode); }
		}

		public CodeDescriptionPairList GetStateAddressList(ZString country)
		{
			return factory.GetCachedValue("StateList" + country, delegate
			{
				var result = new CodeDescriptionPairList();
				if (messageData.IsUSImporter(country))
				{
					result = messageData.Factory.GetCachedUSStateAndDistrictList();
				}
				else if (messageData.IsCAImporter(country))
				{
					result = messageData.Factory.GetCachedValue<CanadaStatesList>();
				}
				else if (messageData.IsMXImporter(country))
				{
					result = messageData.Factory.GetCachedValue<MexicoStateList>();
				}
				else
				{
					result.AddPair(OrgAddressMessageData.ForeignBasedImporterStateCode, OrgAddressMessageData.ForeignBasedImporterStateDescription);
				}
				return result;
			});
		}

		public CodeDescriptionPairList MailingAddressTypeList
		{
			get { return factory.GetCachedValue<ImporterAddressTypesList>(); }
		}

		public CodeDescriptionPairList PhysicalAddressTypeList
		{
			get
			{
				return factory.GetCachedValue("PhysicalAddressTypeList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ImporterAddressTypesList.Codes._01, ImporterAddressTypesList.Descriptions._01);
					result.AddPair(ImporterAddressTypesList.Codes._02, ImporterAddressTypesList.Descriptions._02);
					result.AddPair(ImporterAddressTypesList.Codes._03, ImporterAddressTypesList.Descriptions._03);
					result.AddPair(ImporterAddressTypesList.Codes._04, ImporterAddressTypesList.Descriptions._04);
					result.AddPair(ImporterAddressTypesList.Codes._05, ImporterAddressTypesList.Descriptions._05);
					result.AddPair(ImporterAddressTypesList.Codes._08, ImporterAddressTypesList.Descriptions._08);
					return result;
				});
			}
		}

		public NumberOfEntriesPlanningList NumberOfEntriesList
		{
			get { return factory.GetCachedValue<NumberOfEntriesPlanningList>(); }
		}

		public ImporterProgramCodeList ProgramCodesList
		{
			get { return factory.GetCachedValue<ImporterProgramCodeList>(); }
		}
	}
}
