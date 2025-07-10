using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

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
	[UniversalCopyIgnoreElement(DEAHeader.Schema.US_DrugCode, DEAHeader.Schema.US_Weight, DEAHeader.Schema.US_UnitOfMeasure)]
	public class DEAHeader : CusAddInfo<DEAHeaderAddInfo>, ICusAddInfoTypeSupporter, IDEAHeader, IPGADataCorrection, ICanDelete, IAESDEA, ICusDispositionParent
	{
		public DEAHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<DEAHeaderAddInfo>.Schema
		{
			public const string US_LineNo = USDEAHeaderAddInfoSchema.Constants.US_LineNo;
			public const string US_CountryOfShipment = USDEAHeaderAddInfoSchema.Constants.US_CountryOfShipment;
			public const string US_FormID = USDEAHeaderAddInfoSchema.Constants.US_FormID;
			public const string US_PermitNumber = USDEAHeaderAddInfoSchema.Constants.US_PermitNumber;
			public const string US_RegistrationNumber = USDEAHeaderAddInfoSchema.Constants.US_RegistrationNumber;
			public const string US_TrackingStatus = USDEAHeaderAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
			public const string US_DrugCode = "US_DrugCode";
			public const string US_Weight = "US_Weight";
			public const string US_UnitOfMeasure = "US_UnitOfMeasure";
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
		#endregion

		#region AddInfo Properties

		#region US_LineNo

		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_LineNo", Caption = "Line No.")]
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

		#endregion

		#region US_CountryOfShipment

		[List(nameof(AddInfoLookups) + "." + nameof(USDEAHeaderAddInfoLookups.Countries))]
		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_CountryOfShipment", Caption = "Country/Region Of Shipment")]
		public ZString US_CountryOfShipment
		{
			get { return AddInfo.US_CountryOfShipment; }
			set { AddInfo.US_CountryOfShipment = value; }
		}

		public ZPropertyInfo US_CountryOfShipmentInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CountryOfShipment, x => AddInfo.US_CountryOfShipmentInfo); }
		}

		#endregion

		#region US_FormID

		[List(nameof(AddInfoLookups) + "." + nameof(USDEAHeaderAddInfoLookups.FormTypes))]
		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_FormID", Caption = "Form ID")]
		public ZString US_FormID
		{
			get { return AddInfo.US_FormID; }
			set { AddInfo.US_FormID = value; }
		}

		public ZPropertyInfo US_FormIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FormID, x => AddInfo.US_FormIDInfo); }
		}

		#endregion

		#region US_PermitNumber

		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_PermitNumber", Caption = "Permit Number")]
		[MaxLength(nameof(PermitNumberMaxLength))]
		public ZString US_PermitNumber
		{
			get { return AddInfo.US_PermitNumber; }
			set { AddInfo.US_PermitNumber = value; }
		}

		int PermitNumberMaxLength
		{
			get { return IsExport ? 7 : DEAHeaderAddInfo.Schema.US_PermitNumberMaxLength; }
		}

		public ZPropertyInfo US_PermitNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PermitNumber, x => AddInfo.US_PermitNumberInfo); }
		}

		#endregion

		#region US_RegistrationNumber

		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_RegistrationNumber", Caption = "Registration Number", FullDescription = "DEA Registration or company ID number of the U.S. party authorized by the LPCO")]
		[MaxLength(nameof(RegistrationNumberMaxLength))]
		public ZString US_RegistrationNumber
		{
			get { return AddInfo.US_RegistrationNumber; }
			set { AddInfo.US_RegistrationNumber = value; }
		}

		int RegistrationNumberMaxLength
		{
			get { return IsExport ? 9 : DEAHeaderAddInfo.Schema.US_RegistrationNumberMaxLength; }
		}

		public ZPropertyInfo US_RegistrationNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_RegistrationNumber, x => AddInfo.US_RegistrationNumberInfo); }
		}

		#endregion

		#region US_TrackingStatus

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

		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		#endregion

		#endregion

		#region New Properties

		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_DrugCode", Caption = "Drug Code")]
		[MaxLength(4)]
		public ZString US_DrugCode
		{
			get { return FirstConstituent.US_ProductCode; }
			set { FirstConstituent.US_ProductCode = value; }
		}

		public ZPropertyInfo US_DrugCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DrugCode, x => FirstConstituent.US_ProductCodeInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_Quantity", Caption = "Weight")]
		[DecimalPlaces(4)]
		public ZDecimal US_Weight
		{
			get { return FirstConstituent.US_Weight; }
			set { FirstConstituent.US_Weight = value; }
		}

		public ZPropertyInfo US_WeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Weight, x => FirstConstituent.US_WeightInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.DEAHeader|US_UnitOfMeasure", Caption = "Weight UQ", ShortCaption = "UQ")]
		[List(nameof(AddInfoLookups) + "." + nameof(USDEAHeaderAddInfoLookups.WeightUQList))]
		[MaxLength(3)]
		public ZString US_UnitOfMeasure
		{
			get { return FirstConstituent.US_WeightUQ; }
			set { FirstConstituent.US_WeightUQ = value; }
		}

		public ZPropertyInfo US_UnitOfMeasureInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UnitOfMeasure, x => FirstConstituent.US_WeightUQInfo); }
		}

		public DEAConstituent FirstConstituent
		{
			get
			{
				if (firstConstituent == null)
				{
					firstConstituent = Constituents.OfType<DEAConstituent>().FirstOrDefault();
				}

				if (firstConstituent == null)
				{
					firstConstituent = Constituents.AddNew();
				}

				return firstConstituent;
			}
		}
		DEAConstituent firstConstituent;

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USDEAHeaderAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USDEAHeaderAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		DEAHeaderAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new DEAHeaderAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		DEAHeaderAddInfo fAddInfo;

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

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

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusAddInfoSchema.Constants.TableName, CusAddInfoSchema.Constants.B7_ParentID, CusAddInfoSchema.Constants.B7_ParentTableCode)]
		public DEAConstituentCollection Constituents
		{
			get
			{
				if (constituents == null)
				{
					constituents = new DEAConstituentCollection(this);
					constituents.Load();
					RegisterEditableChildObject(constituents);
				}
				return constituents;
			}
		}
		DEAConstituentCollection constituents;

		#endregion

		#region ICusAddInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USDEAConstituent, typeof(DEAConstituent));
			return result;
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
			return new[] { JobComInvoiceLine.Schema.US_DEAInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_DEADisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.US_FDAADTA };
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
		}

		#endregion

		#region IDEAHeader

		ZInt IDEAHeader.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString IDEAHeader.CountryOfShipment
		{
			get { return US_CountryOfShipment; }
		}

		ZString IDEAHeader.FormID
		{
			get { return US_FormID; }
		}

		ZString IDEAHeader.PermitNumber
		{
			get { return US_PermitNumber.KeepAlphanumericCharacters(); }
		}

		ZString IDEAHeader.RegistrationNumber
		{
			get { return US_RegistrationNumber; }
		}

		IEnumerable<IDEAConstituent> IDEAHeader.Constituents
		{
			get { return Constituents.Cast<IDEAConstituent>(); }
		}

		ZDate IDEAHeader.ArrivalDate
		{
			get
			{
				var arrivalDate = InvoiceLine?.Declaration?.US_FDAADTA;
				return arrivalDate.HasValue && arrivalDate.Value.IsValid ? arrivalDate.Value.Date : ZDate.Empty;
			}
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "DEA Line"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (DEAHeader)base.CloneInternal(args);
			result.US_PermitNumber = ZString.Empty;
			result.CloneChildren(this, args);
			return result;
		}

		internal void CloneChildren(DEAHeader previousLine, BusinessObjectCloneArgs args = null)
		{
			foreach (DEAConstituent constituent in previousLine.Constituents)
			{
				DEAConstituent clonedChild = (DEAConstituent)constituent.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(DEAConstituent), false));
				clonedChild.US_Weight = ZDecimal.Zero;
				Constituents.Add(clonedChild);
			}
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			Constituents.RemoveAndDeleteAll();
			base.Delete();
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

		#region IAESDEA Members

		ZString IAESDEA.DrugCode
		{
			get { return US_DrugCode; }
		}

		ZDecimal IAESDEA.Quantity
		{
			get { return US_Weight; }
		}

		ZString IAESDEA.UnitOfMeasure
		{
			get { return US_UnitOfMeasure; }
		}

		ZString IAESDEA.TransactionType
		{
			get
			{
				var result = ZString.Empty;
				var invoiceHeader = InvoiceLine?.InvoiceHeader;
				var inbondType = invoiceHeader != null ? invoiceHeader.US_InbondType : ZString.Empty;

				switch (inbondType)
				{
					case InbondTypeList.Codes.IEWarehouseWithdrawal:
					case InbondTypeList.Codes.TAndEWarehouseWithdrawal:
					case InbondTypeList.Codes.IEForeignTradeZoneWithdrawal:
					case InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal:
						result = "T";
						break;
					default:
						result = "E";
						break;
				}

				return result;
			}
		}

		ZString IAESDEA.PermitNumber
		{
			get { return US_PermitNumber; }
		}

		ZString IAESDEA.RegistrationNumber
		{
			get { return US_RegistrationNumber; }
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.DEA; }
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
			public Strategy(DEAHeader businessObject)
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
