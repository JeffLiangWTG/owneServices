using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceSubTypeDependencyConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceSubTypeDependencyConfigurationCollection()
		{
		}

		public ComplianceSubTypeDependencyConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ComplianceSubTypeDependencyConfiguration this[int x]
		{
			get { return (ComplianceSubTypeDependencyConfiguration)base[x]; }
		}

		public new ComplianceSubTypeDependencyConfiguration AddNew()
		{
			return (ComplianceSubTypeDependencyConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceSubTypeDependencyConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceSubTypeDependencyConfiguration();
		}

		public ZBool SearchCyclicSubTypeRelationship(ZString countryCode, ZString searchSubTypeCode, ZString parentSubTypeCode)
		{
			foreach (ComplianceSubTypeDependencyConfiguration item in this)
			{
				if (item.ChildSubType.IsEmpty || item.ParentSubType.IsEmpty)
				{
					return ZBool.False;
				}
				if (item.Country == countryCode && item.ChildSubType == parentSubTypeCode)
				{
					if (item.ParentSubType == searchSubTypeCode)
					{
						return ZBool.True;
					}

					if (item.ParentSubType != parentSubTypeCode)
					{
						return SearchCyclicSubTypeRelationship(countryCode, searchSubTypeCode, item.ParentSubType);
					}
				}
			}
			return ZBool.False;
		}

		public ZString FindParentSubTypeInCollection(ZString country, ZString subType)
		{
			foreach (ComplianceSubTypeDependencyConfiguration config in this)
			{
				if (config.Country == country && config.ChildSubType == subType)
				{
					var dependencyType = FindParentSubTypeInCollection(country, config.ParentSubType);
					return dependencyType.IsEmpty ? config.ParentSubType : dependencyType;
				}
			}
			return ZString.Empty;
		}
	}
}
