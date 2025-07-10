using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class PersonIdentityInformationDataLookups
	{
		public PersonIdentityInformationDataLookups(PersonIdentityInformationData personData)
		{
			this.personData = personData;
			this.factory = personData.Factory;
		}
		readonly PersonIdentityInformationData personData;
		readonly BusinessObjectFactory factory;

		public OrgContactDependentCollection Contacts
		{
			get { return personData.messageData.wrapper.organisation.Contacts; }
		}

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(factory); }
		}

		public PassportTypesList PassportTypeList
		{
			get { return factory.GetCachedValue<PassportTypesList>(); }
		}
	}
}
