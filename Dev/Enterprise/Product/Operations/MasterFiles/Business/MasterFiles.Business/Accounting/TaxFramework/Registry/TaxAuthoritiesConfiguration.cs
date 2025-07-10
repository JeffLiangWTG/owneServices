using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxAuthoritiesConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string Code = "Code";
			public const string Country = "Country";
			public const string Name = "Name";
			public const string TaxAuthorityType = "TaxAuthorityType";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaxAuthoritiesConfiguration();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCode();
			ValidateCountry();
			ValidateTaxAuthorityType();
		}

		#region Bound Properties

		[MaxLength(10)]
		[ResourceStringData("BFB1D4D2-F52B-4E08-9A6E-EEC4C83628F3", Caption = "Code")]
		public ZString Code
		{
			get
			{
				return code;
			}
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value.ToUpperInvariant());
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);
			if (!CodeInfo.HasErrors())
			{
				if (!code.IsLettersAndNumbersOnlyOrEmpty)
				{
					CodeInfo.AddError(Res.GetString("323ED071-9147-478F-84E9-FE2541D9BF1B", "Please enter alphanumeric characters only."));
				}
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(CodeInfo);
			}
		}

		ZString code;

		[MaxLength(80)]
		[ResourceStringData("64437288-608B-48E2-8A73-17F428CF2C8E", Caption = "Name")]
		public ZString Name
		{
			get
			{
				return name;
			}
			set
			{
				SetNonPersistentPropertyValue(NameInfo, ref name, value);
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(Schema.Name); }
		}

		ZString name;

		[List("CountryList")]
		[MaxLength(2)]
		[ResourceStringData("42193BBF-A341-4A85-AF33-D39DC39E936E", Caption = "Country/Region")]
		public ZString Country
		{
			get
			{
				return country;
			}
			set
			{
				SetNonPersistentPropertyValue(CountryInfo, ref country, value);
			}
		}

		public ZPropertyInfo CountryInfo
		{
			get { return GetZPropertyInfo(Schema.Country); }
		}

		public void ValidateCountry()
		{
			CountryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryInfo);
			if (!CountryInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(CountryInfo);
			}
		}

		ZString country;

		#endregion

		public RefCountryCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = new RefCountryCollection(CurrentFactory);
				}
				return countryList;
			}
		}

		public RefCountryCollection countryList;

		[List("TaxAuthorityTypeList")]
		[MaxLength(3)]
		[ResourceStringData("8E91A6B2-1708-4C10-87C5-0F8CC53620C0", Caption = "Tax Authority Type")]
		public ZString TaxAuthorityType
		{
			get
			{
				return taxAuthorityType;
			}
			set
			{
				SetNonPersistentPropertyValue(TaxAuthorityTypeInfo, ref taxAuthorityType, value);
			}
		}

		public ZPropertyInfo TaxAuthorityTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxAuthorityType); }
		}

		public void ValidateTaxAuthorityType()
		{
			TaxAuthorityTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxAuthorityTypeInfo);
			if (!TaxAuthorityTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(TaxAuthorityTypeInfo);
			}
		}

		ZString taxAuthorityType;

		public CodeDescriptionPairList TaxAuthorityTypeList => new AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList();

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Name, Name);
			writer.WriteElementString(Schema.Country, Country);
			writer.WriteElementString(Schema.TaxAuthorityType, TaxAuthorityType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			Name = reader.ReadElementString(Schema.Name);
			Country = reader.ReadElementString(Schema.Country);
			TaxAuthorityType = reader.ReadElementString(Schema.TaxAuthorityType);
		}
	}
}
