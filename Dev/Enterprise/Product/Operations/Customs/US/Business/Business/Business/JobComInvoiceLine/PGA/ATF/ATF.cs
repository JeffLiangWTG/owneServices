using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class ATF : CusAddInfo<ATFAddInfo>, IATFData, IPGADataCorrection, ICanDelete, IAESATF, ICusDispositionParent
	{
		public ATF(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<ATFAddInfo>.Schema
		{
			public const string US_LineNo = USATFAddInfoSchema.Constants.US_LineNo;
			public const string US_AECAExemptionCode = USATFAddInfoSchema.Constants.US_AECAExemptionCode;
			public const string US_AECANumber = USATFAddInfoSchema.Constants.US_AECANumber;
			public const string US_BarrelLength = USATFAddInfoSchema.Constants.US_BarrelLength;
			public const string US_CaliberGaugeSize = USATFAddInfoSchema.Constants.US_CaliberGaugeSize;
			public const string US_CategoryCode = USATFAddInfoSchema.Constants.US_CategoryCode;
			public const string US_ExtendedDescription = USATFAddInfoSchema.Constants.US_ExtendedDescription;
			public const string US_FELExemptionCode = USATFAddInfoSchema.Constants.US_FELExemptionCode;
			public const string US_FELNumber = USATFAddInfoSchema.Constants.US_FELNumber;
			public const string US_FFLExemptionCode = USATFAddInfoSchema.Constants.US_FFLExemptionCode;
			public const string US_FFLNumber = USATFAddInfoSchema.Constants.US_FFLNumber;
			public const string US_Model = USATFAddInfoSchema.Constants.US_Model;
			public const string US_OverallLength = USATFAddInfoSchema.Constants.US_OverallLength;
			public const string US_PermitExemptionCode = USATFAddInfoSchema.Constants.US_PermitExemptionCode;
			public const string US_PermitNumber = USATFAddInfoSchema.Constants.US_PermitNumber;
			public const string US_Quantity = USATFAddInfoSchema.Constants.US_Quantity;
			public const string US_MunitionsListCategory = USATFAddInfoSchema.Constants.US_MunitionsListCategory;
			public const string US_FFLExpirationDate = USATFAddInfoSchema.Constants.US_FFLExpirationDate;
			public const string US_AECAExpirationDate = USATFAddInfoSchema.Constants.US_AECAExpirationDate;
			public const string US_TrackingStatus = USATFAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}
		#endregion

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return B7_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? Parent as JobComInvoiceLine : null; }
		}

		public CusClassPartPivot Pivot
		{
			get { return B7_ParentTableCode == CusClassPartPivotSchema.Constants.Prefix ? Parent as CusClassPartPivot : null; }
		}

		#endregion

		#region AddInfo Properties

		public ZInt US_LineNo
		{
			get { return AddInfo.US_LineNo; }
			set
			{
				var oldValue = US_LineNo;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						AddInfo.US_LineNo = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}
		bool suspendTrackingStatusChange;

		public bool US_LineNo_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo US_LineNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LineNo, x => AddInfo.US_LineNoInfo); }
		}

		[DecimalPlaces(nameof(QuantityDecimalPlaces))]
		public ZDecimal US_Quantity
		{
			get { return AddInfo.US_Quantity; }
			set { AddInfo.US_Quantity = value; }
		}

		int QuantityDecimalPlaces
		{
			get { return IsExport ? 0 : 2; }
		}

		public ZPropertyInfo US_QuantityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Quantity, x => AddInfo.US_QuantityInfo); }
		}

		[MaxLength(nameof(PermitNumberMaxLength))]
		public ZString US_PermitNumber
		{
			get { return AddInfo.US_PermitNumber; }
			set { AddInfo.US_PermitNumber = value; }
		}

		int PermitNumberMaxLength
		{
			get { return IsExport ? 11 : ATFAddInfo.Schema.US_PermitNumberMaxLength; }
		}

		public ZPropertyInfo US_PermitNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PermitNumber, x => AddInfo.US_PermitNumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USATFAddInfoLookups.ExemptionCodesList))]
		[MaxLength(nameof(PermitExemptionCodeMaxLength))]
		public ZString US_PermitExemptionCode
		{
			get { return AddInfo.US_PermitExemptionCode; }
			set { AddInfo.US_PermitExemptionCode = value; }
		}

		int PermitExemptionCodeMaxLength
		{
			get { return IsExport ? 1 : ATFAddInfo.Schema.US_PermitExemptionCodeMaxLength; }
		}

		public ZPropertyInfo US_PermitExemptionCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PermitExemptionCode, x => AddInfo.US_PermitExemptionCodeInfo); }
		}

		public ZDecimal US_OverallLength
		{
			get { return AddInfo.US_OverallLength; }
			set { AddInfo.US_OverallLength = value; }
		}

		public ZPropertyInfo US_OverallLengthInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OverallLength, x => AddInfo.US_OverallLengthInfo); }
		}

		public ZString US_Model
		{
			get { return AddInfo.US_Model; }
			set { AddInfo.US_Model = value; }
		}

		public ZPropertyInfo US_ModelInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Model, x => AddInfo.US_ModelInfo); }
		}

		[MaxLength(nameof(FFLNumberMaxLength))]
		public ZString US_FFLNumber
		{
			get { return AddInfo.US_FFLNumber; }
			set { AddInfo.US_FFLNumber = value; }
		}

		int FFLNumberMaxLength
		{
			get { return IsExport ? 20 : ATFAddInfo.Schema.US_FFLNumberMaxLength; }
		}

		public ZPropertyInfo US_FFLNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FFLNumber, x => AddInfo.US_FFLNumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USATFAddInfoLookups.ExemptionCodesList))]
		[MaxLength(nameof(FFLExemptionCodeMaxLength))]
		public ZString US_FFLExemptionCode
		{
			get { return AddInfo.US_FFLExemptionCode; }
			set { AddInfo.US_FFLExemptionCode = value; }
		}

		int FFLExemptionCodeMaxLength
		{
			get { return IsExport ? 1 : ATFAddInfo.Schema.US_FFLExemptionCodeMaxLength; }
		}

		public ZPropertyInfo US_FFLExemptionCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FFLExemptionCode, x => AddInfo.US_FFLExemptionCodeInfo); }
		}

		public ZString US_FELNumber
		{
			get { return AddInfo.US_FELNumber; }
			set { AddInfo.US_FELNumber = value; }
		}

		public ZPropertyInfo US_FELNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FELNumber, x => AddInfo.US_FELNumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USATFAddInfoLookups.ExemptionCodesList))]
		public ZString US_AECAExemptionCode
		{
			get { return AddInfo.US_AECAExemptionCode; }
			set { AddInfo.US_AECAExemptionCode = value; }
		}

		public ZPropertyInfo US_AECAExemptionCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_AECAExemptionCode, x => AddInfo.US_AECAExemptionCodeInfo); }
		}

		public ZString US_AECANumber
		{
			get { return AddInfo.US_AECANumber; }
			set { AddInfo.US_AECANumber = value; }
		}

		public ZPropertyInfo US_AECANumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_AECANumber, x => AddInfo.US_AECANumberInfo); }
		}

		public ZDecimal US_BarrelLength
		{
			get { return AddInfo.US_BarrelLength; }
			set { AddInfo.US_BarrelLength = value; }
		}

		public ZPropertyInfo US_BarrelLengthInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BarrelLength, x => AddInfo.US_BarrelLengthInfo); }
		}

		public ZString US_CaliberGaugeSize
		{
			get { return AddInfo.US_CaliberGaugeSize; }
			set { AddInfo.US_CaliberGaugeSize = value; }
		}

		public ZPropertyInfo US_CaliberGaugeSizeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CaliberGaugeSize, x => AddInfo.US_CaliberGaugeSizeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USATFAddInfoLookups.CategoryCodeList))]
		[MaxLength(nameof(CategoryCodeMaxLength))]
		public ZString US_CategoryCode
		{
			get { return AddInfo.US_CategoryCode; }
			set { AddInfo.US_CategoryCode = value; }
		}

		int CategoryCodeMaxLength
		{
			get { return IsExport ? 4 : ATFAddInfo.Schema.US_CategoryCodeMaxLength; }
		}

		public ZPropertyInfo US_CategoryCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CategoryCode, x => AddInfo.US_CategoryCodeInfo); }
		}

		public ZString US_ExtendedDescription
		{
			get { return AddInfo.US_ExtendedDescription; }
			set { AddInfo.US_ExtendedDescription = value; }
		}

		public ZPropertyInfo US_ExtendedDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ExtendedDescription, x => AddInfo.US_ExtendedDescriptionInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USATFAddInfoLookups.ExemptionCodesList))]
		public ZString US_FELExemptionCode
		{
			get { return AddInfo.US_FELExemptionCode; }
			set { AddInfo.US_FELExemptionCode = value; }
		}

		public ZPropertyInfo US_FELExemptionCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FELExemptionCode, x => AddInfo.US_FELExemptionCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USATFAddInfoLookups.MunitionsCategoryList))]
		public ZString US_MunitionsListCategory
		{
			get { return AddInfo.US_MunitionsListCategory; }
			set { AddInfo.US_MunitionsListCategory = value; }
		}

		public ZPropertyInfo US_MunitionsListCategoryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_MunitionsListCategory, x => AddInfo.US_MunitionsListCategoryInfo); }
		}

		public ZDateTime US_FFLExpirationDate
		{
			get { return AddInfo.US_FFLExpirationDate; }
			set { AddInfo.US_FFLExpirationDate = value; }
		}

		public ZPropertyInfo US_FFLExpirationDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FFLExpirationDate, x => AddInfo.US_FFLExpirationDateInfo); }
		}

		public ZDateTime US_AECAExpirationDate
		{
			get { return AddInfo.US_AECAExpirationDate; }
			set { AddInfo.US_AECAExpirationDate = value; }
		}

		public ZPropertyInfo US_AECAExpirationDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_AECAExpirationDate, x => AddInfo.US_AECAExpirationDateInfo); }
		}

		[ReadOnly(true)]
		public ZString US_TrackingStatus
		{
			get { return AddInfo.US_TrackingStatus; }
			set { AddInfo.US_TrackingStatus = value; }
		}

		public ZPropertyInfo US_TrackingStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TrackingStatus, x => AddInfo.US_TrackingStatusInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.ATF|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		#endregion

		#region New Properties

		public ZBool HasExportData
		{
			get { return !IsDeleted && (!US_Quantity.IsEmpty || !US_CategoryCode.IsEmpty || !US_FFLNumber.IsEmpty || !US_FFLExemptionCode.IsEmpty || !US_PermitNumber.IsEmpty || !US_PermitExemptionCode.IsEmpty); }
		}

		public void CopyQuantityFrom(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.JI_CustomsQuantity > 0 && invoiceLine.JI_CustomsUnitQty == AESUnitOfMeasureList.Codes.Number)
			{
				US_Quantity = invoiceLine.JI_CustomsQuantity;
			}
		}

		#endregion

		#region Flags

		public ZBool IsExport
		{
			get
			{
				if (isExportCached == null)
				{
					isExportCached = new CachedProperty<ZBool>(Factory, () =>
					{
						var parent = Parent;
						return parent != null && ((parent.TablePrefix == JobComInvoiceLineSchema.Constants.Prefix && ((JobComInvoiceLine)parent).IsExport) || (parent.TablePrefix == CusClassPartPivotSchema.Constants.Prefix && ((CusClassPartPivot)parent).IsExportTariff));
					});
				}
				return isExportCached.Value;
			}
		}
		CachedProperty<ZBool> isExportCached;

		public ZBool IsFirearms => US_MunitionsListCategory == MunitionsCategoryList.Codes.I;

		public ZBool IsImplementsOfWar =>
			US_MunitionsListCategory == MunitionsCategoryList.Codes.II ||
			US_MunitionsListCategory == MunitionsCategoryList.Codes.IV ||
			US_MunitionsListCategory == MunitionsCategoryList.Codes.VI ||
			US_MunitionsListCategory == MunitionsCategoryList.Codes.VII ||
			US_MunitionsListCategory == MunitionsCategoryList.Codes.VIII ||
			US_MunitionsListCategory == MunitionsCategoryList.Codes.XIV ||
			US_MunitionsListCategory == MunitionsCategoryList.Codes.XVI ||
			US_MunitionsListCategory == MunitionsCategoryList.Codes.XX ||
			US_MunitionsListCategory == MunitionsCategoryList.Codes.XXI;

		public ZBool IsAmmunition => US_MunitionsListCategory == MunitionsCategoryList.Codes.III;

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "ATF"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (ATF)base.CloneInternal(args);
			return result;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			base.Delete();
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USATFAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USATFAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		ATFAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new ATFAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		ATFAddInfo fAddInfo;

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		#endregion

		#region IATFLines Members

		ZInt IATFData.LineNumber
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString IATFData.AECAExemptionCode
		{
			get { return US_AECAExemptionCode; }
		}

		ZString IATFData.AECANumber
		{
			get { return US_AECANumber; }
		}

		ZDateTime IATFData.ArrivalDate
		{
			get
			{
				var result = ZDateTime.Empty;
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null)
				{
					var declaration = invoiceLine.Declaration;
					result = declaration != null ? declaration.US_FDAADTA : ZDateTime.Empty;
				}
				return result;
			}
		}

		ZString IATFData.ArrivalLocation
		{
			get
			{
				var result = ZString.Empty;
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null)
				{
					var declaration = invoiceLine.Declaration;
					result = declaration != null ? declaration.US_SchDEntry : ZString.Empty;
				}
				return result;
			}
		}

		ZDecimal IATFData.BarrelLength
		{
			get { return US_BarrelLength; }
		}

		ZString IATFData.CaliberGaugeSize
		{
			get { return US_CaliberGaugeSize; }
		}

		ZString IATFData.CategoryCode
		{
			get { return US_CategoryCode; }
		}

		ZString IATFData.ExportCountry
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.US_UC_NKCountryOfExport : ZString.Empty;
			}
		}

		ZString IATFData.ExtendedDescription
		{
			get { return US_ExtendedDescription; }
		}

		ZString IATFData.FELExemptionCode
		{
			get { return US_FELExemptionCode; }
		}

		ZString IATFData.FELNumber
		{
			get { return US_FELNumber; }
		}

		ZString IATFData.FFLExemptionCode
		{
			get { return US_FFLExemptionCode; }
		}

		ZString IATFData.FFLNumber
		{
			get { return US_FFLNumber; }
		}

		ZString IATFData.Model
		{
			get { return US_Model; }
		}

		ZDecimal IATFData.OverallLength
		{
			get { return US_OverallLength; }
		}

		ZString IATFData.PermitExemptionCode
		{
			get { return US_PermitExemptionCode; }
		}

		ZString IATFData.PermitNumber
		{
			get { return US_PermitNumber; }
		}

		ZDecimal IATFData.Quantity
		{
			get { return US_Quantity; }
		}

		IAddressDetails IATFData.ManufacturerAddress
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return InvoiceLine != null ? invoiceLine.ManufacturerAddress : null;
			}
		}

		#endregion

		#region IPGADataCorrection

		IPGADataCorrection PGADataCorrection
		{
			get { return this; }
		}

		bool IPGADataCorrection.SettingStatusInProgress
		{
			get { return settingPGATrackingStatusInProgress; }
			set { settingPGATrackingStatusInProgress = value; }
		}
		bool settingPGATrackingStatusInProgress;

		bool IPGADataCorrection.SuspendTrackingStatusChange
		{
			get { return suspendTrackingStatusChange || AddInfo.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}
		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_ATFInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return System.Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_UC_NKCountryOfExport, JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress, JobComInvoiceLine.Schema.US_SetInd };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return new[] { JobComInvoiceHeader.Schema.US_UC_NKCountryOfExport, JobComInvoiceHeader.Schema.JZ_OA_ManufacturerAddress, JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill };
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return System.Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.US_FDAADTA, JobDeclaration.Schema.US_SchDEntry, JobDeclaration.Schema.US_UC_NKCountryOfExport, JobDeclaration.Schema.JE_OA_ManufacturerAddress };
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
		}

		#endregion

		#region ICanDelete

		bool ICanDelete.CanDelete
		{
			get { return PGADataCorrection.PGALinesCanBeDeleted(); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return PGADataChangeTracker.ReasonForNotAbleToDelete; }
		}

		#endregion

		#region IAESATF Members

		ZString IAESATF.FFLNumber
		{
			get { return US_FFLNumber; }
		}

		ZString IAESATF.FFLExemptionCode
		{
			get { return US_FFLExemptionCode; }
		}

		ZString IAESATF.PermitNumber
		{
			get { return US_PermitNumber; }
		}

		ZString IAESATF.PermitExemptionCode
		{
			get { return US_PermitExemptionCode; }
		}

		ZDecimal IAESATF.Quantity
		{
			get { return US_Quantity; }
		}

		ZString IAESATF.CategoryCode
		{
			get { return US_CategoryCode; }
		}

		ZString IAESATF.Description
		{
			get { return InvoiceLine != null ? InvoiceLine.JI_Description.Left(28) : ZString.Empty; }
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.ATF; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return US_LineNo; }
		}

		CusDispositionCollection IPGALineStatus.PGALineCusDispositions
		{
			get
			{
				if (fCusDisposition == null)
				{
					fCusDisposition = new CusDispositionCollection(this);
					fCusDisposition.Load();
				}
				return fCusDisposition;
			}
		}
		CusDispositionCollection fCusDisposition;

		public ZString Status
		{
			get { return this.GetStatus(); }
		}

		public ZString StatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(Status); }
		}

		public ZDateTime StatusDate
		{
			get { return this.GetStatusDate(); }
		}

		ZString ICusDispositionParent.Type
		{
			get { return CusDispositionTypeCodeList.Codes.USPGALineStatus; }
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return CusAddInfoSchema.Constants.Prefix; }
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, status, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(ATF businessObject)
				: base(businessObject)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		#endregion
	}
}
