using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceSubTypeAllocationOverrideConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceSubTypeAllocationOverrideConfigurationCollection()
		{
		}

		public ComplianceSubTypeAllocationOverrideConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory, ZString countryCode)
			: base(fallbackLevel, factory)
		{
			CountryCode = countryCode;
		}

		public new ComplianceSubTypeAllocationOverrideConfiguration this[int x]
		{
			get { return (ComplianceSubTypeAllocationOverrideConfiguration)base[x]; }
		}

		public new ComplianceSubTypeAllocationOverrideConfiguration AddNew()
		{
			var newObject = new ComplianceSubTypeAllocationOverrideConfiguration(CurrentFallbackLevel, CurrentFactory, CountryCode);
			Add(newObject);
			return newObject;
		}

		protected override BusinessObject AddNewCore()
			=> this.AddNew();

		public ZString CountryCode { get; private set; }

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			if (CountryCode == ZString.Empty && fallbackLevel != null)
			{
				CountryCode = ComplianceSubTypeAllocationOverrideConfigurationRegistryItem.GetCountryCodeFrom(fallbackLevel, factory);
			}

			return new ComplianceSubTypeAllocationOverrideConfigurationCollection(fallbackLevel, factory, CountryCode);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new ComplianceSubTypeAllocationOverrideConfiguration(CurrentFallbackLevel, Factory, CountryCode);

		#region DrawsFromOtherSubTypeBooksCollections

		internal IReadOnlyCollection<ZString> DrawsFromOtherSubTypeBooks_ErrorSubTypes
		{
			get
			{
				InitDrawsFromOtherSubTypeBooksCollections();
				return DrawsFromOtherSubTypeBooks_ErrorSubTypesValue;
			}
		}

		internal IReadOnlyCollection<ZString> DrawsFromOtherSubTypeBooks_WarningSubTypes
		{
			get
			{
				InitDrawsFromOtherSubTypeBooksCollections();
				return DrawsFromOtherSubTypeBooks_WarningSubTypesValue;
			}
		}

		void InitDrawsFromOtherSubTypeBooksCollections()
		{
			if (CurrentFallbackLevel != null
				&& (DrawsFromOtherSubTypeBooks_ErrorSubTypesValue == null || DrawsFromOtherSubTypeBooks_WarningSubTypesValue == null))
			{
				var companyPK = CurrentFallbackLevel.CompanyPK(false);
				var dependencies = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)
										.Cast<ComplianceSubTypeDependencyConfiguration>();
				var childSubTypes = dependencies.Select(x => x.ChildSubType).Where(x => !x.IsEmpty).ToHashSet();
				var parentSubTypes = dependencies.Select(x => x.ParentSubType).Where(x => !x.IsEmpty).ToHashSet();

				DrawsFromOtherSubTypeBooks_ErrorSubTypesValue = childSubTypes;
				DrawsFromOtherSubTypeBooks_WarningSubTypesValue = parentSubTypes.Except(childSubTypes).ToHashSet();
			}
		}

		HashSet<ZString> DrawsFromOtherSubTypeBooks_ErrorSubTypesValue;
		HashSet<ZString> DrawsFromOtherSubTypeBooks_WarningSubTypesValue;

		#endregion

		public ComplianceSubTypeAllocationOverrideConfiguration GetMatchingConfiguration(ZGuid branchPK, ZString complianceSubType)
		{
			if (!branchPK.IsValid || complianceSubType.IsEmpty)
			{
				return null;
			}

			var typedCollection = this.Cast<ComplianceSubTypeAllocationOverrideConfiguration>();

			var matchedByBranchAndSubType = typedCollection.FirstOrDefault(x => x.BranchPK == branchPK && x.SubType == complianceSubType);
			if (matchedByBranchAndSubType != null)
			{
				return matchedByBranchAndSubType;
			}

			var matchedBySubType = typedCollection.FirstOrDefault(x => x.BranchPK.IsEmpty && x.SubType == complianceSubType);
			if (matchedBySubType != null)
			{
				return matchedBySubType;
			}

			return null;
		}
	}
}
