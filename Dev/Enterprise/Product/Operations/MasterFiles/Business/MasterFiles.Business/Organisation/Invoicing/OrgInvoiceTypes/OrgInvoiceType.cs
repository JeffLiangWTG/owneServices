using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgInvoiceType : AutoOrgInvoiceType, IOrgInvoiceType
	{
		public OrgInvoiceType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoOrgInvoiceType.Schema
		{
			public const string InvoiceLayoutDescription = "InvoiceLayoutDescription";
			public const string CommenceOnDescription = "CommenceOnDescription";
			public const string BillingIntervalDescription = "BillingIntervalDescription";
			public const string ModuleDescription = "ModuleDescription";
			public const string SecondaryInvoiceLayoutDescription = "SecondaryInvoiceLayoutDescription";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;
			PI_Type = InvoiceTypeLayoutList.Codes.INV;
			PI_SecondaryType = InvoiceTypeLayoutList.Codes.INV;
		}

		#region InvoiceLayoutDescription

		[MaxLength(255)]
		public ZString InvoiceLayoutDescription
		{
			get { return Lookups.InvoiceLayout.GetDescriptionFromCode(PI_Type); }
		}

		public ZPropertyInfo InvoiceLayoutDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceLayoutDescription); }
		}

		#endregion

		#region SecondaryInvoiceLayoutDescription

		[MaxLength(255)]
		public ZString SecondaryInvoiceLayoutDescription
		{
			get { return Lookups.InvoiceLayout.GetDescriptionFromCode(PI_SecondaryType); }
		}

		public ZPropertyInfo SecondaryInvoiceLayoutDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.SecondaryInvoiceLayoutDescription); }
		}

		#endregion

		#region Properties

		#region PI_Type
		[List("Lookups.InvoiceLayout")]
		public override ZString PI_Type
		{
			get
			{
				return base.PI_Type;
			}
			set
			{
				base.PI_Type = value;
			}
		}

		#endregion

		#region PI_StartDay
		[List("Lookups.CommenceOn")]
		public override ZString PI_StartDay
		{
			get
			{
				return base.PI_StartDay;
			}
			set
			{
				base.PI_StartDay = value;
			}
		}
		#endregion

		#region PI_Interval

		[List("Lookups.BillingInterval")]
		public override ZString PI_Interval
		{
			get
			{
				return base.PI_Interval;
			}
			set
			{
				base.PI_Interval = value;
			}
		}

		#endregion

		#region PI_Calc_IsInclude

		ZString fPI_Calc_IsInclude;

		[List("Lookups.ChargeInclusionType")]
		public ZString PI_Calc_IsInclude
		{
			get
			{
				if (IsInDatabase && !HasChanges)
				{
					return !PI_IsInclude ? InvoiceTypeChargeInclusionTypeList.Codes.EXC : DeferredCharges.Count > 0 ? InvoiceTypeChargeInclusionTypeList.Codes.INC : InvoiceTypeChargeInclusionTypeList.Codes.ALL;
				}

				return fPI_Calc_IsInclude;
			}
			set
			{
				SetNonPersistentPropertyValue(PI_Calc_IsIncludeInfo, ref fPI_Calc_IsInclude, value);
				PI_IsInclude = value != InvoiceTypeChargeInclusionTypeList.Codes.EXC;

				SetDeferredChargesReadOnly(value == InvoiceTypeChargeInclusionTypeList.Codes.ALL);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePI_Calc_IsInclude();
				}
			}
		}

		void SetDeferredChargesReadOnly(bool readOnly)
		{
			if (readOnly)
			{
				DeferredCharges.RemoveAndDeleteAll();
			}
			DeferredCharges.SetReadOnlyIncludingChildren(readOnly);
			DeferredCharges.RefreshBindingIncludingChildren();
		}

		public ZPropertyInfo PI_Calc_IsIncludeInfo
		{
			get { return GetZPropertyInfo(nameof(PI_Calc_IsInclude)); }
		}

		#endregion

		#region PI_Module
		[List("Lookups.JobTypeList")]
		public override ZString PI_Module
		{
			get
			{
				return base.PI_Module;
			}
			set
			{
				base.PI_Module = value;
				OrgInvoiceTypeLookupHelper.OnSettingJobType(value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePI_Type();
				}
			}
		}

		#endregion

		#region PI_TransportMode
		[List("Lookups.TransportModeList")]
		public override ZString PI_TransportMode
		{
			get
			{
				return base.PI_TransportMode;
			}
			set
			{
				base.PI_TransportMode = value;
			}
		}

		protected bool PI_TransportMode_ReadOnly
		{
			get { return OrgInvoiceTypeLookupHelper.TransportMode_ReadOnly; }
		}

		#endregion

		#region PI_ServiceDirection
		[List("Lookups.ServiceDirectionList")]
		public override ZString PI_ServiceDirection
		{
			get
			{
				return base.PI_ServiceDirection;
			}
			set
			{
				base.PI_ServiceDirection = value;
			}
		}

		protected bool PI_ServiceDirection_ReadOnly
		{
			get { return OrgInvoiceTypeLookupHelper.ServiceDirection_ReadOnly; }
		}

		#endregion

		#region PI_SecondaryType
		[List("Lookups.SecondaryInvoiceLayout")]
		public override ZString PI_SecondaryType
		{
			get
			{
				return base.PI_SecondaryType;
			}
			set
			{
				base.PI_SecondaryType = value;
			}
		}

		#endregion

		#region PI_ServiceLevel

		[List("Lookups.ServiceLevelList")]
		public override ZString PI_RS_NKServiceLevel
		{
			get => base.PI_RS_NKServiceLevel;
			set => base.PI_RS_NKServiceLevel = value;
		}

		protected bool PI_RS_NKServiceLevel_ReadOnly => OrgInvoiceTypeLookupHelper.ServiceLevel_ReadOnly;

		#endregion

		#region DeferredCharges

		[ChildEditable]
		[ActionFieldFollow(true)]
		public OrgInvTypeDeferredChargesCollection DeferredCharges
		{
			get
			{
				if (fDeferredCharges == null)
				{
					fDeferredCharges = new OrgInvTypeDeferredChargesCollection(this);
					fDeferredCharges.Load();
					RegisterEditableChildObject(DeferredCharges);
				}
				fDeferredCharges.SetReadOnlyIncludingChildren(IsInDatabase && !HasChanges ? PI_IsInclude && fDeferredCharges.Count == 0
																						: fPI_Calc_IsInclude == InvoiceTypeChargeInclusionTypeList.Codes.ALL);

				return fDeferredCharges;
			}
		}
		OrgInvTypeDeferredChargesCollection fDeferredCharges;

		#endregion

		#region CommenceOnDescription

		[MaxLength(255)]
		public ZString CommenceOnDescription
		{
			get { return Lookups.CommenceOn.GetDescriptionFromCode(PI_StartDay); }
		}

		public ZPropertyInfo CommenceOnDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CommenceOnDescription); }
		}

		#endregion

		#region BillingIntervalDescription

		[MaxLength(255)]
		public ZString BillingIntervalDescription
		{
			get { return Lookups.BillingInterval.GetDescriptionFromCode(PI_Interval); }
		}

		public ZPropertyInfo BillingIntervalDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BillingIntervalDescription); }
		}

		#endregion

		#region ModuleDescription

		[MaxLength(255)]
		public ZString ModuleDescription
		{
			get { return Lookups.ModuleList.GetDescriptionFromCode(PI_Module); }
		}

		public ZPropertyInfo ModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ModuleDescription); }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (CompanyData != null && CompanyData.Header != null &&
				!CompanyData.Header.SecurityProvider.HasModifyReceivablesInvoiceBatchingSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region IOrgInvoiceType
		ZString IOrgInvoiceType.JobType
		{
			get { return PI_Module; }
			set { PI_Module = value; }
		}

		ZPropertyInfo IOrgInvoiceType.JobTypeInfo
		{
			get { return PI_ModuleInfo; }
		}

		ZString IOrgInvoiceType.ServiceDirection
		{
			get { return PI_ServiceDirection; }
			set { PI_ServiceDirection = value; }
		}

		ZPropertyInfo IOrgInvoiceType.ServiceDirectionInfo
		{
			get { return PI_ServiceDirectionInfo; }
		}

		ZString IOrgInvoiceType.TransportMode
		{
			get { return PI_TransportMode; }
			set { PI_TransportMode = value; }
		}

		ZPropertyInfo IOrgInvoiceType.TransportModeInfo
		{
			get { return PI_TransportModeInfo; }
		}

		ZString IOrgInvoiceType.ServiceLevel
		{
			get => PI_RS_NKServiceLevel;
			set => PI_RS_NKServiceLevel = value;
		}

		ZPropertyInfo IOrgInvoiceType.ServiceLevelInfo => PI_RS_NKServiceLevelInfo;

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

		CodeDescriptionPairList IOrgInvoiceType.ServiceLevelList => Lookups.ServiceLevelList;

		string IOrgInvoiceType.DuplicateRowErrorMessage
		{
			get { return OrgInvoiceTypeLookupHelper.DuplicateRowErrorMessage; }
		}

		public BusinessObjectCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (OrgInvoiceTypeCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}
		#endregion

		public OrgInvoiceTypeHelper OrgInvoiceTypeLookupHelper
		{
			get { return orgInvoiceTypeLookupHelper_innerValue ?? (orgInvoiceTypeLookupHelper_innerValue = new OrgInvoiceTypeHelper(this)); }
		}
		OrgInvoiceTypeHelper orgInvoiceTypeLookupHelper_innerValue;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (PI_Type.IsEmpty)
			{
				PI_Type = "INV";
			}
			PI_SecondaryType = PI_Type;
		}
#endif
	}
}
#endregion
