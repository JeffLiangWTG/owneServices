namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	using CargoWise.Types;

	public class ContainersTestClass : ICusContainer
	{
		public ContainersTestClass()
		{
		}

		#region ICusContainer Members

		public ZString ContainerNumber
		{
			get { return fContainerNumber; }
			set { fContainerNumber = value; }
		}
		ZString fContainerNumber;

		public ZString ContainerType
		{
			get { return fContainerType; }
			set { fContainerType = value; }
		}
		ZString fContainerType;

		public ZInt ContainerSize
		{
			get { return fContainerSize; }
			set { fContainerSize = value; }
		}
		ZInt fContainerSize;

		public ZDecimal ContainerWeight
		{
			get { return fContainerWeight; }
			set { fContainerWeight = value; }
		}
		ZDecimal fContainerWeight;

		public ZString ContainerWeightUnit
		{
			get { return fContainerWeightUnit; }
			set { fContainerWeightUnit = value; }
		}
		ZString fContainerWeightUnit;

		public ZString SealNumber
		{
			get { return fSealNumber; }
			set { fSealNumber = value; }
		}
		ZString fSealNumber;

		#endregion
	}

	public class LicencesAndDocumentsTestClass : ICusDocument
	{
		public LicencesAndDocumentsTestClass()
		{
		}

		#region ICusDocument Members

		public ZString LicenceNumber
		{
			get { return fLicenceNumber; }
			set { fLicenceNumber = value; }
		}
		ZString fLicenceNumber;

		#endregion
	}

	public class AttachmentsTestClass : ICusAttachment
	{
		public AttachmentsTestClass()
		{
		}

		#region ICusAttachment Members

		public ZGuid UniqueIdentifier
		{
			get { return fUniqueIdentifier; }
			set { fUniqueIdentifier = value; }
		}
		ZGuid fUniqueIdentifier;

		public ZString FileName
		{
			get { return fFileName; }
			set { fFileName = value; }
		}
		ZString fFileName;

		public ZString DocType
		{
			get { return fDocType; }
			set { fDocType = value; }
		}
		ZString fDocType;

		#endregion
	}

	public class ChargeTestClass : ICusCharge
	{
		public ChargeTestClass(string currencyCode, ZDecimal exchangeRate, ZDecimal amount, ZDecimal percentage)
		{
			this.currencyCode = currencyCode;
			this.exchangeRate = exchangeRate;
			this.amount = amount;
			this.percentage = percentage;
		}

		#region ICusCharge Members

		public ZString CurrencyCode
		{
			get { return currencyCode; }
		}
		readonly ZString currencyCode;

		public ZDecimal ExchangeRate
		{
			get { return exchangeRate; }
		}
		readonly ZDecimal exchangeRate;

		public ZDecimal Amount
		{
			get { return amount; }
		}
		readonly ZDecimal amount;

		public ZDecimal Percentage
		{
			get { return percentage; }
		}
		readonly ZDecimal percentage;

		#endregion
	}

	public class OrganisationTestClass : IOrganisation
	{
		#region IOrganisation Members

		public ZString UEN
		{
			get;
			set;
		}

		public ZString Name
		{
			get;
			set;
		}

		public IAddress Address
		{
			get { return address; }
			set { address = value; }
		}
		IAddress address = new OrganisationAddressTestClass("");

		#endregion
	}

	class OrganisationAddressTestClass : IAddress
	{
		public OrganisationAddressTestClass(ZString fullAddress)
			: this(fullAddress, "", "", "", "")
		{
		}

		public OrganisationAddressTestClass(ZString fullAddress, ZString city, ZString postCode, ZString countryCode, ZString state)
		{
			this.FullAddress = fullAddress;
			this.FullAddressWithoutAdditionalInfo = fullAddress;
			this.City = city;
			this.PostCode = postCode;
			this.CountryCode = countryCode;
			this.SubdivisionCode = state;
			this.SubdivisionName = state;
		}

		public ZString FullAddress { get; set; }
		public ZString FullAddressWithoutAdditionalInfo { get; set; }
		public ZString City { get; set; }
		public ZString PostCode { get; set; }
		public ZString CountryCode { get; set; }
		public ZString SubdivisionCode { get; set; }
		public ZString SubdivisionName { get; set; }
	}

	public class AgentInfoTestClass : ICusAgentInfo
	{
		#region ICusAgentInfo Members

		public ZString EntityIdentifier
		{
			get { return fEntityIdentifier; }
			set { fEntityIdentifier = value; }
		}
		ZString fEntityIdentifier;

		public ZString Code
		{
			get { return fCode; }
			set { fCode = value; }
		}
		ZString fCode;

		public ZString Passport
		{
			get { return fPassport; }
			set { fPassport = value; }
		}
		ZString fPassport;

		public ZString Name
		{
			get { return fName; }
			set { fName = value; }
		}
		ZString fName;

		public ZString Phone
		{
			get { return fPhone; }
			set { fPhone = value; }
		}
		ZString fPhone;

		#endregion
	}

	public class ProductCodesTestClass : ICusProductCode
	{
		#region ICusProductCode Members

		public ZString ProductCode
		{
			get { return fProductCode; }
			set { fProductCode = value; }
		}
		ZString fProductCode;

		public ZDecimal ProductCodeQty
		{
			get { return fProductCodeQty; }
			set { fProductCodeQty = value; }
		}
		ZDecimal fProductCodeQty;

		public ZString ProductCodeUnitType
		{
			get { return fProductCodeUnitType; }
			set { fProductCodeUnitType = value; }
		}
		ZString fProductCodeUnitType;

		#endregion
	}

	public class SGCPlaceTestClass : ISGCPlace
	{
		#region ISGCPlace Members

		public ZString Code
		{
			get { return fCode; }
			set { fCode = value; }
		}
		ZString fCode;

		public ZString Type
		{
			get { return fType; }
			set { fType = value; }
		}
		ZString fType;

		public ZString NameAndAddress
		{
			get { return fNameAndAddress; }
			set { fNameAndAddress = value; }
		}
		ZString fNameAndAddress;

		public ZBool AddressRequired
		{
			get { return addressRequired; }
			set { addressRequired = value; }
		}
		ZBool addressRequired;

		public ZBool IsNonSystemNonLicenced
		{
			get { return isNonSystemNonLicenced; }
			set { isNonSystemNonLicenced = value; }
		}
		ZBool isNonSystemNonLicenced;

		#endregion
	}
}
