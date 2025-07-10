using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DocRollupOrSort;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public partial class OrgInvoiceRollupOrGroup : AutoOrgInvoiceRollupOrGroup, IInvoiceRollupOrGroup
	{
		public OrgInvoiceRollupOrGroup(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		#region Loader

		public new partial class Loader : BaseDocRollupOrSortLoader<OrgInvoiceRollupOrGroup>
		{
			public Loader(AccTransactionHeader transaction)
					: this(transaction.Header, transaction.Branch, transaction.Department)
			{
			}

			public Loader(OrgHeader org, GlbBranch branch, GlbDepartment department)
					: base(org.Factory, org, branch, department)
			{
				this.Org = org;
				this.Branch = branch;
				this.Department = department;
			}

			readonly OrgHeader Org;
			readonly GlbBranch Branch;
			readonly GlbDepartment Department;

			internal OrgHeader OrgInternal => Org;
			internal GlbBranch BranchInternal => Branch;
			internal GlbDepartment DepartmentInternal => Department;

			internal BusinessObjectFactory FactoryInternal => Factory;

			public ZString GetInvoicePostingStyle(ZString serviceDirection, ZString transportMode, ZString mode, params ZString[] jobTypes)
				=> Get
				(
					orgField: OrgInvoiceRollupOrGroupSchema.PG_InvoicePostingStyle,
					registryField: InvoiceRollupOrGroup.Schema.InvoicePostingStyle,
					defaultSetting: OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode,
					serviceDirection,
					transportMode,
					mode,
					jobTypes
				);

			public ZString GetInvoicePostingCurrency(ZString serviceDirection, ZString transportMode, ZString mode, params ZString[] jobTypes)
				=> Get
				(
					orgField: OrgInvoiceRollupOrGroupSchema.PG_RX_NKInvoicePostingCurrency,
					registryField: InvoiceRollupOrGroup.Schema.InvoicePostingCurrency,
					defaultSetting: ZString.Empty,
					serviceDirection,
					transportMode,
					mode,
					jobTypes
				);

			public ZString GetInvoiceLineDisplayOption(ZString serviceDirection, ZString transportMode, ZString mode, params ZString[] jobTypes)
				=> Get
				(
					orgField: OrgInvoiceRollupOrGroupSchema.PG_InvoiceLineDisplayOption,
					registryField: InvoiceRollupOrGroup.Schema.InvoiceLineDisplayOption,
					defaultSetting: OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode,
					serviceDirection,
					transportMode,
					mode,
					jobTypes
				);

			public ZString GetGroupOrSubTotal(ZString serviceDirection, ZString transportMode, ZString mode, params ZString[] jobTypes)
				=> Get
				(
					orgField: OrgInvoiceRollupOrGroupSchema.PG_GroupOrSubTotal,
					registryField: InvoiceRollupOrGroup.Schema.GroupOrSubTotal,
					defaultSetting: OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode,
					serviceDirection,
					transportMode,
					mode,
					jobTypes
				);

			public ZString GetGroupOrSubtotalStyle(ZString serviceDirection, ZString transportMode, ZString mode, params ZString[] jobTypes)
			{
				CodeDescriptionPairList validValueList = null;
				if (jobTypes.Length > 0)
				{
					validValueList = OrgInvoiceRollupOrGroupLookups.GetGroupOrSubTotalStyleList(jobTypes[0]);
				}

				return GetFromOrganisation(OrgInvoiceRollupOrGroupSchema.PG_GroupOrSubtotalStyle, OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode, serviceDirection, transportMode, mode, jobTypes)
					?? GetFromRegistry(InvoiceRollupOrGroup.Schema.GroupOrSubtotalStyle, serviceDirection, transportMode, mode, jobTypes, validValueList, OrgInvoiceRollupOrGroupLookups.GroupOrSubTotalStyleCodeWhenRegistryValueInvalid);
			}

			#region Implementation

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(OrgInvoiceRollupOrGroup);

			protected override SchemaColumn JobTypeSchemaColumn => OrgInvoiceRollupOrGroupSchema.PG_JobType;

			protected override SchemaColumn ServiceDirectionSchemaColumn => OrgInvoiceRollupOrGroupSchema.PG_ServiceDirection;

			protected override SchemaColumn TransportModeSchemaColumn => OrgInvoiceRollupOrGroupSchema.PG_TransportMode;

			protected override SchemaColumn CompanyDataSchemaColumn => OrgInvoiceRollupOrGroupSchema.PG_OB;

			protected override RegistryBusinessObjectCollectionTemplate GetRegistrySettings(Guid companyPK, Guid branchPK, Guid departmentPK)
				=> OrganisationRegistry.Instance.InvoiceRollupOrGroup.GetFallBackValueAtAllLevels
				(
					companyPK,
					branchPK,
					departmentPK
				);

			#endregion
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			PG_InvoicePostingStyle = InvoicePostingOptionDefaultCode;
			PG_InvoiceLineDisplayOption = InvoiceLineDisplayOptionDefaultCode;
			PG_GroupOrSubTotal = GroupOrSubtotalOptionDefaultCode;
			PG_GroupOrSubtotalStyle = GroupOrSubtotalStyleOptionDefaultCode;
		}

		#endregion

		#region Properties

		#region Posting Style

		public const string InvoicePostingOptionDefaultCode = "DEF";

		#endregion

		#region Invoice Line Display Option Includes Exchange Rate

		public const string InvoiceLineDisplayOptionDefaultCode = "DEF";

		public static ZBool InvoiceLineDisplayOptionIncludesExchangeRate(ZString invoiceLineDisplayOption)
		{
			return
					invoiceLineDisplayOption == InvoiceDescriptionOptionsList.Codes.FreightExRate ||
					invoiceLineDisplayOption == InvoiceDescriptionOptionsList.Codes.FreightFOBExRate ||
					invoiceLineDisplayOption == InvoiceDescriptionOptionsList.Codes.AllExRate ||
					invoiceLineDisplayOption == InvoiceDescriptionOptionsList.Codes.NoneExRate;
		}

		#endregion

		#region PG_InvoicePostingStyle

		[List("Lookups.InvoicePostingOptionsList")]
		public override ZString PG_InvoicePostingStyle
		{
			get { return base.PG_InvoicePostingStyle; }
			set
			{
				base.PG_InvoicePostingStyle = value;

				if (CompanyData != null && CompanyData.Header != null)
				{
					CompanyData.Header.Addresses.MarkAsNeedingValidation();
				}
			}
		}

		protected bool PG_InvoicePostingStyle_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.InvoicePostingStyle_ReadOnly; }
		}

		#endregion

		#region PG_InvoiceLineDisplayOption

		[List("Lookups.InvoiceLineDisplayOptionsList")]
		public override ZString PG_InvoiceLineDisplayOption
		{
			get { return base.PG_InvoiceLineDisplayOption; }
			set
			{
				base.PG_InvoiceLineDisplayOption = value;

				if (CompanyData != null && CompanyData.Header != null)
				{
					CompanyData.Header.Addresses.MarkAsNeedingValidation();
				}
			}
		}

		protected bool PG_InvoiceLineDisplayOption_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.InvoiceLineDisplayOption_ReadOnly; }
		}

		#endregion

		#region PG_JobType

		[List("Lookups.JobTypeList")]
		public override ZString PG_JobType
		{
			get { return base.PG_JobType; }
			set
			{
				base.PG_JobType = value;
				InvoiceRollupOrGroupHelper.OnSettingJobType(value);

				if (CompanyData != null && CompanyData.Header != null)
				{
					CompanyData.Header.Addresses.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region PG_ServiceDirection

		[List("Lookups.ServiceDirectionList")]
		public override ZString PG_ServiceDirection
		{
			get { return base.PG_ServiceDirection; }
			set
			{
				base.PG_ServiceDirection = value;
				if (CompanyData != null && CompanyData.Header != null)
				{
					CompanyData.Header.Addresses.MarkAsNeedingValidation();
				}
			}
		}

		protected bool PG_ServiceDirection_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.ServiceDirection_ReadOnly; }
		}

		#endregion

		#region PG_TransportMode

		[List("Lookups.TransportModeList")]
		public override ZString PG_TransportMode
		{
			get { return base.PG_TransportMode; }
			set
			{
				base.PG_TransportMode = value;
				if (CompanyData != null && CompanyData.Header != null)
				{
					CompanyData.Header.Addresses.MarkAsNeedingValidation();
				}
			}
		}

		protected bool PG_TransportMode_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.TransportMode_ReadOnly; }
		}

		#endregion

		#region PG_GroupOrSubtotalStyle

		public const string GroupOrSubtotalStyleOptionDefaultCode = "DEF";

		[List("Lookups.GroupOrSubTotalStyleList")]
		public override ZString PG_GroupOrSubtotalStyle
		{
			get { return base.PG_GroupOrSubtotalStyle; }
			set
			{
				base.PG_GroupOrSubtotalStyle = value;
				if (CompanyData != null && CompanyData.Header != null)
				{
					CompanyData.Header.Addresses.MarkAsNeedingValidation();
				}
			}
		}

		protected bool PG_GroupOrSubtotalStyle_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.GroupOrSubtotalStyle_ReadOnly; }
		}

		#endregion

		#region PG_OB

		public override ZGuid PG_OB
		{
			get { return base.PG_OB; }
			set
			{
				base.PG_OB = value;
				if (CompanyData != null && CompanyData.Header != null)
				{
					CompanyData.Header.Addresses.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region PG_GroupOrSubTotal

		public const string GroupOrSubtotalOptionDefaultCode = "DEF";

		[List("Lookups.GroupOrSubTotalList")]
		public override ZString PG_GroupOrSubTotal
		{
			get { return base.PG_GroupOrSubTotal; }
			set
			{
				base.PG_GroupOrSubTotal = value;

				if (PG_GroupOrSubTotal == GroupOrSubtotalOptionDefaultCode)
				{
					PG_GroupOrSubtotalStyle = GroupOrSubtotalStyleOptionDefaultCode;
				}

				if (CompanyData != null && CompanyData.Header != null)
				{
					CompanyData.Header.Addresses.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#endregion

		#region ReadOnly

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = CompanyData == null || CompanyData.Header == null || !CompanyData.Header.SecurityProvider.HasModifyReceivablesChargeGroupingSecurity;
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region InvoiceRollupOrGroupHelper

		internal InvoiceRollupOrGroupHelper InvoiceRollupOrGroupHelper
		{
			get { return invoiceRollupOrGroupHelper_innerValue ?? (invoiceRollupOrGroupHelper_innerValue = new OrgInvoiceRollupOrGroupHelper(this)); }
		}
		InvoiceRollupOrGroupHelper invoiceRollupOrGroupHelper_innerValue;

		class OrgInvoiceRollupOrGroupHelper : InvoiceRollupOrGroupHelper
		{
			public OrgInvoiceRollupOrGroupHelper(IInvoiceRollupOrGroup parent)
					: base(parent)
			{
			}

			protected override bool IsGroupOrSubTotalDoNotEqualAnyValueFromList
			{
				get
				{
					return base.IsGroupOrSubTotalDoNotEqualAnyValueFromList &&
							Parent.GroupOrSubTotal != OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode;
				}
			}
		}

		#endregion

		#region IInvoiceRollupOrGroup Members

		ZString IInvoiceRollupOrGroup.GroupOrSubTotal
		{
			get { return PG_GroupOrSubTotal; }
			set { PG_GroupOrSubTotal = value; }
		}

		ZPropertyInfo IInvoiceRollupOrGroup.GroupOrSubTotalInfo
		{
			get { return PG_GroupOrSubTotalInfo; }
		}

		ZString IInvoiceRollupOrGroup.GroupOrSubtotalStyle
		{
			get { return PG_GroupOrSubtotalStyle; }
			set { PG_GroupOrSubtotalStyle = value; }
		}

		ZPropertyInfo IInvoiceRollupOrGroup.GroupOrSubtotalStyleInfo
		{
			get { return PG_GroupOrSubtotalStyleInfo; }
		}

		ZString IInvoiceRollupOrGroup.InvoiceLineDisplayOption
		{
			get { return PG_InvoiceLineDisplayOption; }
			set { PG_InvoiceLineDisplayOption = value; }
		}

		ZPropertyInfo IInvoiceRollupOrGroup.InvoiceLineDisplayOptionInfo
		{
			get { return PG_InvoiceLineDisplayOptionInfo; }
		}

		ZString IInvoiceRollupOrGroup.InvoicePostingStyle
		{
			get { return PG_InvoicePostingStyle; }
			set { PG_InvoicePostingStyle = value; }
		}

		ZPropertyInfo IInvoiceRollupOrGroup.InvoicePostingStyleInfo
		{
			get { return PG_InvoicePostingStyleInfo; }
		}

		ZString IOrgInvoiceType.JobType
		{
			get { return PG_JobType; }
			set { PG_JobType = value; }
		}

		ZPropertyInfo IOrgInvoiceType.JobTypeInfo
		{
			get { return PG_JobTypeInfo; }
		}

		ZString IOrgInvoiceType.ServiceDirection
		{
			get { return PG_ServiceDirection; }
			set { PG_ServiceDirection = value; }
		}

		ZPropertyInfo IOrgInvoiceType.ServiceDirectionInfo
		{
			get { return PG_ServiceDirectionInfo; }
		}

		ZString IOrgInvoiceType.TransportMode
		{
			get { return PG_TransportMode; }
			set { PG_TransportMode = value; }
		}

		ZPropertyInfo IOrgInvoiceType.TransportModeInfo
		{
			get { return PG_TransportModeInfo; }
		}

		ZString IOrgInvoiceType.ServiceLevel { get; set; }

		ZPropertyInfo IOrgInvoiceType.ServiceLevelInfo { get; }

		CodeDescriptionPairList IOrgInvoiceType.ServiceLevelList => Lookups.ServiceLevelList;

		CodeDescriptionPairList IInvoiceRollupOrGroup.InvoicePostingOptionsList
		{
			get { return Lookups.InvoicePostingOptionsList; }
		}

		ZString IInvoiceRollupOrGroup.InvoicePostingCurrency
		{
			get { return PG_RX_NKInvoicePostingCurrency; }
			set { PG_RX_NKInvoicePostingCurrency = value; }
		}

		ZPropertyInfo IInvoiceRollupOrGroup.InvoicePostingCurrencyInfo
		{
			get { return PG_RX_NKInvoicePostingCurrencyInfo; }
		}

		CodeDescriptionPairList IInvoiceRollupOrGroup.InvoiceLineDisplayOptionsList
		{
			get { return Lookups.InvoiceLineDisplayOptionsList; }
		}

		CodeDescriptionPairList IOrgInvoiceType.JobTypeList
		{
			get { return Lookups.JobTypeList; }
		}

		CodeDescriptionPairList IOrgInvoiceType.TransportModeList
		{
			get { return Lookups.TransportModeList; }
		}

		CodeDescriptionPairList IOrgInvoiceType.ServiceDirectionList
		{
			get { return Lookups.ServiceDirectionList; }
		}

		CodeDescriptionPairList IInvoiceRollupOrGroup.GroupOrSubTotalList
		{
			get { return Lookups.GroupOrSubTotalList; }
		}

		CodeDescriptionPairList IInvoiceRollupOrGroup.GroupOrSubTotalStyleList
		{
			get { return Lookups.GroupOrSubTotalStyleList; }
		}

		RefCurrencyCollection IInvoiceRollupOrGroup.InvoicePostingCurrencies
		{
			get { return Lookups.InvoicePostingCurrencies; }
		}

		string IOrgInvoiceType.DuplicateRowErrorMessage
		{
			get { return InvoiceRollupOrGroupHelper.DuplicateRowErrorMessage; }
		}

		BusinessObjectCollection IOrgInvoiceType.ParentCollection
		{
			get { return CompanyData != null ? CompanyData.InvoiceRollupOrGroups : null; }
		}

		#endregion
	}
}
