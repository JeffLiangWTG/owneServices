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
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
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
	[GlowDataDefinition("ITTBLine")]
	public class TTBLine : AutoTTBLine, ICusAddInfoTypeSupporter, ITTBLine, IPGADataCorrection, ICanDelete, IAESTTB, ICusDispositionParent
	{
		public TTBLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoTTBCOLAAndCertificate.Schema
		{
			public const string ConsigneeOrgPK = "ConsigneeOrgPK";
			public const string US_PermitExemptionCodeDesc = "US_PermitExemptionCodeDesc";
			public const string US_ProgramCodeDesc = "US_ProgramCodeDesc";
			public const string US_ProcessingCodeDesc = "US_ProcessingCodeDesc";
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		#region Flags

		public bool IsBeverageProgramType
		{
			get { return US_ProgramCode == TTBProgramCodeList.Codes.Beverage; }
		}

		public bool IsDistilledSpiritsProgramType
		{
			get { return US_ProgramCode == TTBProgramCodeList.Codes.DistilledSpirits; }
		}

		public bool IsTobaccoProgramType
		{
			get { return US_ProgramCode == TTBProgramCodeList.Codes.Tobacco; }
		}

		public bool IsTobaccoProgramTypeAndNotPaperOrTube
		{
			get { return IsTobaccoProgramType && !IsQuantityRequired; }
		}

		public bool IsQuantityRequired
		{
			get { return TTBTOBProcessingCodeList.IsQuantityRequired(US_ProcessingCode); }
		}

		public bool IsWineProgramType
		{
			get { return US_ProgramCode == TTBProgramCodeList.Codes.Wine; }
		}

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

		public JobComInvoiceLine InvoiceLine
		{
			get { return B7_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? Parent as JobComInvoiceLine : null; }
		}

		public CusClassPartPivot Pivot
		{
			get { return B7_ParentTableCode == CusClassPartPivotSchema.Constants.Prefix ? Parent as CusClassPartPivot : null; }
		}

		#region ConsigneeOrgPK
		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|ConsigneeOrgPK", Caption = "Consignee")]
		[List(nameof(AddInfoLookups) + "." + nameof(USTTBLineAddInfoLookups.Organisations))]
		[ReadOnlyMember(nameof(IsNotReleaseUnderBond))]
		public ZGuid ConsigneeOrgPK
		{
			get { return US_OA_ConsigneeAddress_ZAddress.OrgPK; }
			set { US_OA_ConsigneeAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ConsigneeOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeOrgPK, x => US_OA_ConsigneeAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region Override Properties

		protected override ZString HumanReadableNameCore
		{
			get { return "TTB Line"; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_ProgramCode", Caption = "Program Code", ShortCaption = "Code")]
		[ReadOnlyMember(nameof(US_ProgramCode_ReadOnly))]
		public override ZString US_ProgramCode
		{
			get { return base.US_ProgramCode; }
			set
			{
				var oldValue = US_ProgramCode;
				base.US_ProgramCode = value;
				if (!IsCopying && oldValue != value)
				{
					ClearCigarsOrCOLAAndCertificaesOrQuantityIfNotNeeded();
				}
			}
		}

		protected bool US_ProgramCode_ReadOnly { get; private set; }

#if DEBUG //Set by form basher to not modify the property value
		public void SetProgramCodeReadOnlyForTest(bool value)
		{
			US_ProgramCode_ReadOnly = value;
		}
#endif

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_ProgramCodeDesc", Caption = "Program Description")]
		public ZString US_ProgramCodeDesc
		{
			get { return AddInfoLookups.ProgramCodes.GetDescriptionFromCode(US_ProgramCode); }
		}

		public ZPropertyInfo US_ProgramCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProgramCodeDesc); }
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_LineNo", Caption = "Line No")]
		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set
			{
				try
				{
					suspendTrackingStatusChange = true;
					base.US_LineNo = value;
				}
				finally
				{
					suspendTrackingStatusChange = false;
				}
			}
		}
		bool suspendTrackingStatusChange;

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_PermitExemptionCode", Caption = "Exemption Code", ShortCaption = "Exemption")]
		public override ZString US_PermitExemptionCode
		{
			get { return base.US_PermitExemptionCode; }
			set { base.US_PermitExemptionCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_PermitExemptionCodeDesc", Caption = "Permit Exemption Code Description")]
		public ZString US_PermitExemptionCodeDesc
		{
			get { return AddInfoLookups.PermitExemptionCodes.GetDescriptionFromCode(US_PermitExemptionCode); }
		}

		public ZPropertyInfo US_PermitExemptionCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_PermitExemptionCodeDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_IsReleaseUnderBond", Caption = "Is Release Under Bond?", ShortCaption = "Is Bonded?")]
		public override ZBool US_IsReleaseUnderBond
		{
			get { return base.US_IsReleaseUnderBond; }
			set
			{
				var oldValue = US_IsReleaseUnderBond;
				base.US_IsReleaseUnderBond = value;
				if (!IsCopying && oldValue != US_IsReleaseUnderBond)
				{
					ClearBondDetailIfNotNeeded();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_NumberForIRC", Caption = "Number for Internal Revenue Code", ShortCaption = "Number for IRC", FullDescription = "IRC Registry number for the distilled spirits plant, bonded wine cellar, or brewery, or the TTB-issued permit number indicating the IRC-bonded manufacturer or export warehouse proprietor")]
		[ReadOnlyMember(nameof(US_NumberForIRC_ReadOnly))]
		public override ZString US_NumberForIRC
		{
			get { return base.US_NumberForIRC; }
			set { base.US_NumberForIRC = value; }
		}

		bool US_NumberForIRC_ReadOnly
		{
			get { return IsNotReleaseUnderBond && !IsExport; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_OA_ConsigneeAddress", Caption = "Consignee Address", ShortCaption = "Consignee", FullDescription = "For Tobacco this is the receiving manufacturer or export warehouse proprietor; for non-Tobacco this is the receiving brewery, bonded wine cellar, or DSP.")]
		public override ZGuid US_OA_ConsigneeAddress
		{
			get { return base.US_OA_ConsigneeAddress; }
			set
			{
				base.US_OA_ConsigneeAddress = value;
				if (ConsigneeAddress != null)
				{
					var ttiCustomsRegNo = ConsigneeAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.TTIRegistrationNumber, Core.Constants.CountryCodes.UnitedStates);
					if (!ttiCustomsRegNo.IsEmpty)
					{
						US_NumberForIRC = ttiCustomsRegNo.Left(US_NumberForIRCInfo.MaxLength);
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_PermitNumber", Caption = "Permit Number", ShortCaption = "Permit")]
		public override ZString US_PermitNumber
		{
			get { return base.US_PermitNumber; }
			set { base.US_PermitNumber = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_ProcessingCode", Caption = "Processing Code", ShortCaption = "Processing")]
		public override ZString US_ProcessingCode
		{
			get { return base.US_ProcessingCode; }
			set
			{
				var oldValue = US_ProcessingCode;
				base.US_ProcessingCode = value;
				if (!IsCopying && oldValue != US_ProcessingCode)
				{
					ClearCigarsOrCOLAAndCertificaesOrQuantityIfNotNeeded();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_ProcessingCodeDesc", Caption = "Processing Description")]
		public ZString US_ProcessingCodeDesc
		{
			get { return AddInfoLookups.ProcessingCodes.GetDescriptionFromCode(US_ProcessingCode); }
		}

		public ZPropertyInfo US_ProcessingCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProcessingCodeDesc); }
		}

		[ReadOnlyMember(nameof(US_QuantityInPCS_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_QuantityInPCS", Caption = "Quantity in PCS")]
		public override ZDecimal US_QuantityInPCS
		{
			get { return base.US_QuantityInPCS; }
			set { base.US_QuantityInPCS = value; }
		}

		bool US_QuantityInPCS_ReadOnly
		{
			get { return !IsTobaccoProgramType || !IsQuantityRequired; }
		}

		[ReadOnly(true)]
		public override ZString US_TrackingStatus
		{
			get { return base.US_TrackingStatus; }
			set { base.US_TrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_Date", Caption = "Departure Date")]
		public override ZDateTime US_Date
		{
			get { return base.US_Date; }
			set { base.US_Date = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.TTBLine|US_SerialNumber", Caption = "Serial Number")]
		public override ZString US_SerialNumber
		{
			get { return base.US_SerialNumber; }
			set { base.US_SerialNumber = value; }
		}

		#endregion

		[ChildEditable(true)]
		public TTBCigarCollection Cigars
		{
			get
			{
				if (ttbCigars == null)
				{
					ttbCigars = new TTBCigarCollection(this);
					ttbCigars.Load();
					RegisterEditableChildObject(ttbCigars);
				}
				return ttbCigars;
			}
		}
		TTBCigarCollection ttbCigars;

		[ChildEditable(true)]
		public TTBCOLAAndCertificateCollection COLAAndCertificates
		{
			get
			{
				if (colaAndCertificates == null)
				{
					colaAndCertificates = new TTBCOLAAndCertificateCollection(this);
					colaAndCertificates.Load();
					RegisterEditableChildObject(colaAndCertificates);
				}
				return colaAndCertificates;
			}
		}
		TTBCOLAAndCertificateCollection colaAndCertificates;

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			Cigars.RemoveAndDeleteAll();
			COLAAndCertificates.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#region Implementation

		void ClearBondDetailIfNotNeeded()
		{
			if (!US_IsReleaseUnderBond)
			{
				US_NumberForIRC = ZString.Empty;
				ConsigneeOrgPK = ZGuid.Empty;
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (TTBLine)base.CloneInternal(args);
			result.US_LineNo = ZShort.Zero;
			result.US_QuantityInPCS = CargoWise.Types.ZDecimal.Zero;

			foreach (TTBCigar cigar in Cigars)
			{
				result.Cigars.Add((TTBCigar)cigar.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(TTBCigar), false)));
			}
			foreach (TTBCOLAAndCertificate permit in COLAAndCertificates)
			{
				result.COLAAndCertificates.Add((TTBCOLAAndCertificate)permit.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(TTBCOLAAndCertificate), false)));
			}
			return result;
		}

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}

		void ClearCigarsOrCOLAAndCertificaesOrQuantityIfNotNeeded()
		{
			if (IsTobaccoProgramType)
			{
				COLAAndCertificates.RemoveAndDeleteAll();
				if (IsQuantityRequired)
				{
					Cigars.RemoveAndDeleteAll();
				}
				Cigars.MarkAsNeedingValidation();
			}
			else
			{
				Cigars.RemoveAndDeleteAll();
				COLAAndCertificates.MarkAsNeedingValidation();
			}
			if (!US_QuantityInPCS.IsEmpty && US_QuantityInPCS_ReadOnly)
			{
				US_QuantityInPCS = ZDecimal.Zero;
			}
		}

		internal void DefaultPermitNumberFromIOR(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null && invoiceLine.IORWrapper != null && US_PermitNumber.IsEmpty)
			{
				US_PermitNumber = invoiceLine.IORWrapper.GetCustomsCode(OrgCusCode.USACodeTypes.TTBPermitNumber).Left(AutoUSTTBLineAddInfo.Schema.US_PermitNumberMaxLength);
			}
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USTTBCigar, typeof(TTBCigar));
			result.Add(CusAddInfoTypeAttribute.Codes.USTTBCOLAAndCertificate, typeof(TTBCOLAAndCertificate));
			return result;
		}

		#endregion

		#region ITTBLine Members

		ZInt ITTBLine.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString ITTBLine.ProgramCode
		{
			get { return US_ProgramCode; }
		}

		ZString ITTBLine.ProcessingCode
		{
			get { return US_ProcessingCode; }
		}

		ZString ITTBLine.PermitNumber
		{
			get { return US_PermitNumber; }
		}

		ZString ITTBLine.ExemptionCode
		{
			get { return US_PermitExemptionCode; }
		}

		ZString ITTBLine.NumberForIRC
		{
			get { return US_NumberForIRC; }
		}

		IAddressDetails ITTBLine.Consignee
		{
			get { return ConsigneeAddress; }
		}

		ZString ITTBLine.ConsigneeEIN
		{
			get
			{
				var consigneeAddress = ConsigneeAddress;
				var consignee = consigneeAddress == null ? null : consigneeAddress.Header;
				return consignee == null ? ZString.Empty : consignee.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates);
			}
		}

		IEnumerable<ITTBCigar> ITTBLine.Cigars
		{
			get { return IsTobaccoProgramType ? Cigars.OfType<ITTBCigar>() : Enumerable.Empty<ITTBCigar>(); }
		}

		IEnumerable<ITTBCOLAAndCertificate> ITTBLine.COLAAndCertificates
		{
			get { return COLAAndCertificates.OfType<ITTBCOLAAndCertificate>(); }
		}

		ZDecimal ITTBLine.QuantityInPCS
		{
			get { return US_QuantityInPCS_ReadOnly ? ZDecimal.Zero : US_QuantityInPCS; }
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
			get { return suspendTrackingStatusChange || Data.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_TTBInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_TTBDisclaimReason };
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
			return Array.Empty<string>();
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

		#region IAESTTB Members

		ZString IAESTTB.IRCNumber
		{
			get { return US_NumberForIRC; }
		}

		ZDate IAESTTB.Date
		{
			get { return US_Date.Date; }
		}

		ZString IAESTTB.SerialNumber
		{
			get { return US_SerialNumber; }
		}

		ZString IAESTTB.Disclaimer
		{
			get { return InvoiceLine != null && OGAIndicatorList.IsToBeDisclaimed(InvoiceLine.US_TTBInd) ? "1" : string.Empty; }
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.TTB; }
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
	}
}
