using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.MasterFiles.Business.Res;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceSubTypeAllocationOverrideConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string Country = "Country";
			public const string BranchPK = "BranchPK";
			public const string SubType = "SubType";
			public const string AllocationMethod = "AllocationMethod";
		}

		public static class CalculatedFieldNames
		{
			public const string SubTypeDescription = "SubTypeDescription";
			public const string SubTypeDocumentTitle = "SubTypeDocumentTitle";
		}

		#endregion

		public const string DefaultAllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print;

		/// <summary>
		/// Public default constructor for benefit of serialisation. Do not use from code.
		/// </summary>
		public ComplianceSubTypeAllocationOverrideConfiguration()
			: base()
		{
		}

		public ComplianceSubTypeAllocationOverrideConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory, ZString countryCode)
			: base(fallbackLevel, factory)
		{
			Country = countryCode;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceSubTypeAllocationOverrideConfiguration(fallbackLevel, factory, Country);
		}

		protected override void SetCustomDefaultValuesCore()
		{
			AllocationMethod = GetDefaultForAllocationMethod();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCountry();
			ValidateSubType();
			ValidateAllocationMethod();
			ValidateBranchPK();
		}

		#region Bound Properties

		#region Country

		[MaxLength(2)]
		[ResourceStringData("Accounting|ComplianceSubTypeAllocationOverrideConfiguration|Country", Caption = "Country/Region")]
		[ReadOnly(true)]
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
			MandatoryValidation.CheckEntered(CountryInfo, (IMultilingualString)ResString.GetMultilingualString("077c0cfd-7a8d-4359-838c-97b014fb1102", "Country/Region"));
			CheckCollectionValidation(CountryInfo);
		}

		ZString country;

		#endregion

		#region SubType

		[List("Lookups.SubTypeList")]
		[MaxLength(3)]
		[ResourceStringData("Accounting|ComplianceSubTypeAllocationOverrideConfiguration|SubType", Caption = "Sub-Type")]
		public ZString SubType
		{
			get { return subType; }
			set
			{
				CheckMaximumLength(SubTypeInfo, value);
				SetNonPersistentPropertyValue(SubTypeInfo, ref subType, value);
				if (!IsValidationSuspended)
				{
					ValidateSubType();
				}
			}
		}

		public ZPropertyInfo SubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.SubType); }
		}

		public void ValidateSubType()
		{
			SubTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(SubTypeInfo, (IMultilingualString)ResString.GetMultilingualString("b540a51c-3c50-409a-ac9e-d389de64b5f0", "Compliance Sub Type"));
			if (!SubTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(SubTypeInfo, Lookups.SubTypeList);
			}
			CheckSubTypeDrawsFromOtherSubTypeBooks();
			CheckCollectionValidation(SubTypeInfo);
		}

		ZString subType;

		void CheckSubTypeDrawsFromOtherSubTypeBooks()
		{
			if (!SubTypeInfo.HasErrors()
				&& !SubTypeInfo.HasWarnings()
				&& CurrentFallbackLevel != null
				&& ParentCollection != null)
			{
				if (ParentCollection.DrawsFromOtherSubTypeBooks_ErrorSubTypes.Contains(SubType))
				{
					SubTypeInfo.AddError(Res.GetString("13793f85-a305-4673-ac29-8a94d71d6fad", "This Sub Type draws numbers from a book that belongs to another Sub Type. Please select the Sub Type that owns that book to setup rules for all Sub Types associated with that book."));
					return;
				}

				if (ParentCollection.DrawsFromOtherSubTypeBooks_WarningSubTypes.Contains(SubType))
				{
					SubTypeInfo.AddWarning(Res.GetString("d13fcd18-50be-4c7c-b060-4428b43cbf90", "You are setting a rule for a Sub Type that shares its book with other sub types, be aware that the rule will apply to all Sub Types that draw numbers from that book."));
					return;
				}
			}
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("Accounting|ComplianceSubTypeAllocationOverrideConfiguration|SubTypeDescription", Caption = "Description")]
		public ZString SubTypeDescription
		{
			get { return Lookups.SubTypeList.GetDescriptionFromCode(SubType); }
		}

		public ZPropertyInfo SubTypeDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(CalculatedFieldNames.SubTypeDescription, x => SubTypeInfo); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("Accounting|ComplianceSubTypeAllocationOverrideConfiguration|SubTypeDocumentTitle", Caption = "Document Title")]
		public ZString SubTypeDocumentTitle
		{
			get { return Lookups.SubTypeInLocalLanguageList.GetDescriptionFromCode(SubType); }
		}

		public ZPropertyInfo SubTypeDocumentTitleInfo
		{
			get { return GetWrappedZPropertyInfo(CalculatedFieldNames.SubTypeDocumentTitle, x => SubTypeInfo); }
		}

		#endregion

		#region AllocationMethod

		[List("Lookups.AllocationMethodList")]
		[MaxLength(3)]
		[ResourceStringData("Accounting|ComplianceSubTypeAllocationOverrideConfiguration|AllocationMethod", Caption = "Allocation Method")]
		public ZString AllocationMethod
		{
			get { return allocationMethod; }
			set
			{
				CheckMaximumLength(AllocationMethodInfo, value);
				SetNonPersistentPropertyValue(AllocationMethodInfo, ref allocationMethod, value);
				if (!IsValidationSuspended)
				{
					ValidateAllocationMethod();
				}
			}
		}

		public ZPropertyInfo AllocationMethodInfo
		{
			get { return GetZPropertyInfo(Schema.AllocationMethod); }
		}

		public void ValidateAllocationMethod()
		{
			AllocationMethodInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AllocationMethodInfo, (IMultilingualString)ResString.GetMultilingualString("b0081b32-59ac-481c-ac65-88676d320f72", "Allocation Method"));

			if (!AllocationMethodInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(AllocationMethodInfo, Lookups.AllocationMethodList);
			}

			CheckAllocationMethodFromCountryCompliance();
			CheckCollectionValidation(AllocationMethodInfo);
			CheckAllocationMethodIsSameAsParentRegistry();
		}

		void CheckAllocationMethodFromCountryCompliance()
		{
			if (!AllocationMethodInfo.HasErrors() && IsEInvoicingEnabledForThisCompany.HasValue)
			{
				var eInvoicingEnabled = IsEInvoicingEnabledForThisCompany.Value;
				var validationResultFromCountryCompliance = DefaultProvider?.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(AllocationMethod, eInvoicingEnabled);
				var validationResult = validationResultFromCountryCompliance ?? ComplianceDocumentNumberAllocation_ReceivablesRegistryValidators.CheckCannotBeGovernmentNumberAllocate(AllocationMethod, Country);
				if (!string.IsNullOrEmpty(validationResult))
				{
					AllocationMethodInfo.AddError(validationResult);
				}
			}
		}

		void CheckAllocationMethodIsSameAsParentRegistry()
		{
			if (!AllocationMethodInfo.HasErrors() && !AllocationMethodInfo.HasWarnings() && FallbackCompanyPK.HasValue)
			{
				var parentRegistryValue = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(FallbackCompanyPK.Value, Guid.Empty, Guid.Empty);
				if (AllocationMethod == parentRegistryValue)
				{
					AllocationMethodInfo.AddWarning(Res.GetString("30382bf8-15a1-4b3c-8d03-bc116580ae61", "You are selecting the same method as the Company level rule. This registry should be used to set different methods. Please select a different value."));
				}
			}
		}

		ZString allocationMethod;

		#endregion

		#region BranchPK

		[List("Lookups.BranchList")]
		[ResourceStringData("Accounting|ComplianceSubTypeAllocationOverrideConfiguration|BranchPK", Caption = "Branch")]
		public ZGuid BranchPK
		{
			get { return branchPK; }
			set
			{
				SetNonPersistentPropertyValue(BranchPKInfo, ref branchPK, value);
				if (!IsValidationSuspended)
				{
					ValidateBranchPK();
				}
			}
		}

		public ZPropertyInfo BranchPKInfo
		{
			get { return GetZPropertyInfo(nameof(BranchPK)); }
		}

		public void ValidateBranchPK()
		{
			BranchPKInfo.ClearAllNotifications();
			if (BranchPK.IsValid && FallbackCompanyPK.HasValue)
			{
				ListValidation.ErrorIfInvalidPK(BranchPKInfo, ResString.GetMultilingualString("e2688768-85b7-4f4e-8db4-6b5c3297999b", "Enter a valid Branch."));
			}
			CheckCollectionValidation(BranchPKInfo);
		}

		ZGuid branchPK;

		#endregion

		#region Collection Level Validation

		public void CheckCollectionValidation(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.HasErrors()
				|| ParentCollection == null)
			{
				return;
			}

			foreach (ComplianceSubTypeAllocationOverrideConfiguration other in ParentCollection)
			{
				var isMatchingCriteriaSame = this.PK != other.PK
											&& this.Country == other.Country
											&& this.SubType == other.SubType
											&& this.BranchPK == other.BranchPK;
				var hasDuplicate = isMatchingCriteriaSame
								&& this.AllocationMethod == other.AllocationMethod;
				var hasConflict = isMatchingCriteriaSame
								&& this.AllocationMethod != other.AllocationMethod;

				if (hasDuplicate)
				{
					propertyInfo.AddError(Res.GetString("8bace9ad-343c-49ee-bcf6-ee47e3bef613", "Identical override configuration detected. Duplicate rules are not permitted."));
					return;
				}
				else if (hasConflict)
				{
					propertyInfo.AddError(Res.GetString("a7bd99c1-8f11-4fed-9afe-4a08a0df63ab", "Conflicting override configuration detected. Identical rules for same Sub-Type and Branch, but different Allocation Method are not permitted."));
					return;
				}
			}
		}

		#endregion

		#endregion

		#region Other Public Properties

		public GlbCompany Company
			=> CurrentFactory.Load<GlbCompany>(FallbackCompanyPK.GetValueOrDefault());

		#endregion

		#region Lookups

		public ComplianceSubTypeAllocationOverrideConfigurationLookups Lookups
			=> lookupsValue ?? (lookupsValue = GetNewLookups());

		protected virtual ComplianceSubTypeAllocationOverrideConfigurationLookups GetNewLookups()
			=> new ComplianceSubTypeAllocationOverrideConfigurationLookups(this);

		ComplianceSubTypeAllocationOverrideConfigurationLookups lookupsValue;

		#endregion

		#region Helpers

		IComplianceRegistryDefaultProvider DefaultProvider
			=> ObjectFactory.Get<ICountryComplianceFactory>()?.GetIComplianceRegistryDefaultProvider(Country);

		ZString GetDefaultForAllocationMethod()
			=> CurrentFallbackLevel == null
				? DefaultAllocationMethod
				: AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.GetValueWithoutFallback(CurrentFallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty);

		bool? IsEInvoicingEnabledForThisCompany
		{
			get
			{
				var companyPK = FallbackCompanyPK;
				return !companyPK.HasValue
						? null
						: AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(companyPK.Value, Guid.Empty, Guid.Empty);
			}
		}

		Guid? FallbackCompanyPK
			=> CurrentFallbackLevel == null     // FallbackLevel is not assigned until after derialisation completes, so may not be valid at all times.
				? null
				: CurrentFallbackLevel.CompanyPK(false);

		ComplianceSubTypeAllocationOverrideConfigurationCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ComplianceSubTypeAllocationOverrideConfigurationCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Country, Country);
			writer.WriteElementString(Schema.SubType, SubType);
			writer.WriteElementString(Schema.AllocationMethod, AllocationMethod);
			writer.WriteElementString(Schema.BranchPK, BranchPK.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Country = reader.ReadElementString(Schema.Country);
			SubType = reader.ReadElementString(Schema.SubType);
			AllocationMethod = reader.ReadElementString(Schema.AllocationMethod);
			BranchPK = new ZGuid(reader.ReadElementString(Schema.BranchPK));
		}

		#endregion
	}
}
