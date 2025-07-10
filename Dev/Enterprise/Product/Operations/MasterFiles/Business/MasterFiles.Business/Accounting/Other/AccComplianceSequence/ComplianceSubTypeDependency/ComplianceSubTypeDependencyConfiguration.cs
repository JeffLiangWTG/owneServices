using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceSubTypeDependencyConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string Country = "Country";
			public const string ChildSubType = "ChildSubType";
			public const string ParentSubType = "ParentSubType";
		}

		readonly MultilingualString ChildSameAsParentSubType = ResString.GetMultilingualString("14CFBF23-92CF-4AE9-9E15-36A21F46DBC7", "Child subtype can not be the same as the parent subtype.");
		readonly MultilingualString ChildSubTypeAlreadyExists = ResString.GetMultilingualString("CE939B98-6E0D-435F-BF27-78D62C9912ED", "The selected child subtype already exists in the configuration.");
		readonly MultilingualString CyclicDependencyExists = ResString.GetMultilingualString("741684D3-5E57-4955-A463-2CEB0D74DB73", "Cyclic dependency detected along the relationship chain.");

		#endregion

		public ComplianceSubTypeDependencyConfiguration()
		{ }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceSubTypeDependencyConfiguration();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCountry();
			ValidateChildSubType();
			ValidateParentSubType();
		}

		public ComplianceSubTypeDependencyConfigurationCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ComplianceSubTypeDependencyConfigurationCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new ComplianceSubTypeDependencyConfigurationCollection();
				}
			}
		}

		#region Bound Properties

		#region Country

		[List("Lookups.CountryList")]
		[MaxLength(2)]
		public ZString Country
		{
			get { return country; }
			set
			{
				CheckMaximumLength(CountryInfo, value);
				SetNonPersistentPropertyValue(CountryInfo, ref country, value);
				if (!IsValidationSuspended)
				{
					ValidateCountry();
				}
			}
		}

		public ZPropertyInfo CountryInfo
		{
			get { return GetZPropertyInfo(Schema.Country); }
		}

		public void ValidateCountry()
		{
			CountryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryInfo, (IMultilingualString)ResString.GetMultilingualString("4754c845-d853-409d-b47c-b6d0e6cd9cac", "Country/Region"));
			if (!CountryInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(CountryInfo, Lookups.CountryList);
			}
			if (!CountryInfo.HasErrors() && CheckChildSubTypeAlreadyExists())
			{
				CountryInfo.AddError(ChildSubTypeAlreadyExists);
			}
			if (!CountryInfo.HasErrors() && CheckCyclicDependencyExists())
			{
				CountryInfo.AddError(CyclicDependencyExists);
			}
		}

		ZString country;

		#endregion

		#region ChildSubType

		[List("Lookups.SubTypeList")]
		[MaxLength(3)]
		public ZString ChildSubType
		{
			get { return childSubType; }
			set
			{
				SetNonPersistentPropertyValue(ChildSubTypeInfo, ref childSubType, value);
				if (!IsValidationSuspended)
				{
					ValidateChildSubType();
				}
			}
		}

		public ZPropertyInfo ChildSubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ChildSubType); }
		}

		public void ValidateChildSubType()
		{
			ChildSubTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ChildSubTypeInfo, (IMultilingualString)ResString.GetMultilingualString("5f5e184e-597b-45dd-b27a-2631336c8ff8", "Child Subtype"));
			if (!ChildSubTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ChildSubTypeInfo, Lookups.SubTypeList);
			}
			if (!ChildSubTypeInfo.HasErrors() && CheckChildSameAsParentSubType())
			{
				ChildSubTypeInfo.AddError(ChildSameAsParentSubType);
			}
			if (!ChildSubTypeInfo.HasErrors() && CheckChildSubTypeAlreadyExists())
			{
				ChildSubTypeInfo.AddError(ChildSubTypeAlreadyExists);
			}
			if (!ChildSubTypeInfo.HasErrors() && CheckCyclicDependencyExists())
			{
				ChildSubTypeInfo.AddError(CyclicDependencyExists);
			}
		}

		ZString childSubType;

		#endregion

		#region ParentSubType

		[List("Lookups.SubTypeList")]
		[MaxLength(3)]
		public ZString ParentSubType
		{
			get { return parentSubType; }
			set
			{
				SetNonPersistentPropertyValue(ParentSubTypeInfo, ref parentSubType, value);
				if (!IsValidationSuspended)
				{
					ValidateParentSubType();
				}
			}
		}

		public ZPropertyInfo ParentSubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ParentSubType); }
		}

		public void ValidateParentSubType()
		{
			ParentSubTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ParentSubTypeInfo, (IMultilingualString)ResString.GetMultilingualString("f1e667f9-38d2-448a-b196-c893a36c2905", "Parent Subtype"));
			if (!ParentSubTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ParentSubTypeInfo, Lookups.SubTypeList);
			}
			if (!ParentSubTypeInfo.HasErrors() && CheckChildSameAsParentSubType())
			{
				ParentSubTypeInfo.AddError(ChildSameAsParentSubType);
			}
			if (!ParentSubTypeInfo.HasErrors() && CheckChildSubTypeAlreadyExists())
			{
				ParentSubTypeInfo.AddError(ChildSubTypeAlreadyExists);
			}
			if (!ParentSubTypeInfo.HasErrors() && CheckCyclicDependencyExists())
			{
				ParentSubTypeInfo.AddError(CyclicDependencyExists);
			}
		}

		ZString parentSubType;

		#endregion

		#endregion

		#region ComplianceSubTypeAttributionRuleConfigurationLookups

		public ComplianceSubTypeDependencyConfigurationLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = GetNewLookups();
				}
				return fLookups;
			}
		}

		protected virtual ComplianceSubTypeDependencyConfigurationLookups GetNewLookups()
		{
			return new ComplianceSubTypeDependencyConfigurationLookups(this);
		}

		ComplianceSubTypeDependencyConfigurationLookups fLookups;

		#endregion

		ZBool CheckChildSameAsParentSubType()
		{
			return ChildSubType == parentSubType;
		}

		ZBool CheckChildSubTypeAlreadyExists()
		{
			int count = 0;
			if (ParentCollections.Count > 0)
			{
				foreach (ComplianceSubTypeDependencyConfiguration item in ParentCollection)
				{
					if (PK != item.PK
						&& Country == item.Country
						&& ChildSubType == item.ChildSubType)
					{
						count++;
					}
				}
			}
			return count > 0;
		}

		ZBool CheckCyclicDependencyExists()
		{
			return ParentCollections.Count > 0 ? ParentCollection.SearchCyclicSubTypeRelationship(Country, childSubType, ParentSubType) : ZBool.False;
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Country, Country);
			writer.WriteElementString(Schema.ChildSubType, ChildSubType);
			writer.WriteElementString(Schema.ParentSubType, ParentSubType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Country = reader.ReadElementString(Schema.Country);
			ChildSubType = reader.ReadElementString(Schema.ChildSubType);
			ParentSubType = reader.ReadElementString(Schema.ParentSubType);
		}

		#endregion
	}
}
