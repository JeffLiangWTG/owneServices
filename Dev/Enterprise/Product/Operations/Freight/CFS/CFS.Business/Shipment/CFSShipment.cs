using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	[UniversalDataContext(DataContextType.CFSShipment)]
	public class CFSShipment : CommonShipment,
		Integration.CFS.ICFSShipment,
		IStatusClassProvider,
		IHasStatusProvider,
		IDocumentSupportable,
		IJobInvoicingPlugIn,
		IJobHeaderParent,
		IRatingSupporter,
		IWorkflowProvider,
		ICartageParent,
		ICartageParentExtra,
		IOverrideStorageMainDocManagerCode,
		IControllerIDProvider
	{
		#region Schema

		public new class Schema : CommonShipment.Schema
		{
			public const string JS_IsFullyDelivered = "JS_IsFullyDelivered";
			public const string JS_Calc_JobNumber = "JS_Calc_JobNumber";
			public const string JS_OA_CartageCoAddr = "JS_OA_CartageCoAddr";

			public const string JS_GatePassStatusShort = "JS_GatePassStatusShort";
			public const string JS_GatePassStatus = "JS_GatePassStatus";

			public const string CanadaHouseCCN = "CanadaHouseCCN";
		}

		#endregion

		public CFSShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			factory.SetFreightDomainContext(FreightDomainContext.CFS);
#pragma warning disable
			((IBusiness)this).UpdatedByDataRefreshIncludingChildren +=
				new EventHandler(CFSShipment_UpdatedByDataRefreshIncludingChildren);
#pragma warning restore

			new CanadaCFSShipmentSupport().Register(this);
		}

		public event CancelEventHandler CheckDefaultDetailsFromLoadList;

		#region Validation

		protected override JobShipmentValidation GetNewValidation()
		{
			return new CFSShipmentValidation(this);
		}

		public new CFSShipmentValidation Validation
		{
			get { return (CFSShipmentValidation)base.Validation; }
		}

		#endregion

		#region Business Object Overrides

		public override void OnLoaded()
		{
			base.OnLoaded();
			InitialiseStatusProvider();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			IsSettingDefaultValues = true;
			try
			{
				JS_IsForwardRegistered = false;
				JS_IsCFSRegistered = true;
				JS_IsBooking = false;

				if (JS_TransportMode.IsEmpty)
				{
					JS_TransportMode = Constants.TransportModes.Sea;
				}

				if (IsSea || JS_PackingMode.IsEmpty)
				{
					JS_PackingMode = Constants.ContainerModes.LCL;
				}
				JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort != null ? GlbBranch.CurrentBranch.HomePort.Code : ZString.Empty;
				Validation.ValidateJS_Calc_CurrentVessel();
				JS_F3_NKPackType = Constants.PkgUnit.Package;
			}
			finally
			{
				IsSettingDefaultValues = false;
			}
		}

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (!JS_IsForwardRegistered)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		public override void OnSaved(bool hasSaveSucceeded)
		{
			if (!hasSaveSucceeded && JS_UniqueConsignRefInfo.HasChanges)
			{
				if (IsInDatabase)
				{
					JS_UniqueConsignRef = JS_UniqueConsignRefInfo.OriginalValue.ToString();
				}

				if (jobReferenceNumberChangedLog != null && !jobReferenceNumberChangedLog.IsInDatabase)
				{
					jobReferenceNumberChangedLog.Delete();
					jobReferenceNumberChangedLog = null;
				}
			}

			base.OnSaved(hasSaveSucceeded);
		}

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
				JobHeader.DeleteAllJobs(this);
				ObjectFactory.Get<IChildrenDeletionHelper>().DeleteChildren(this);
			}
			base.Delete();
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				result.AddRange(Factory.Load<CommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, PK)));

				return result.ToArray();
			}
		}

		protected override bool DeferFiringWorkflowCore
		{
			get { return JS_IsForwardRegistered; }
		}

		#endregion

		#region Lookups

		public new CFSShipmentLookups Lookups
		{
			get { return (CFSShipmentLookups)base.Lookups; }
		}

		protected override JobShipmentLookups GetNewLookups()
		{
			return new CFSShipmentLookups(this);
		}

		#endregion

		#region TemplateCopy

		public override IBusiness TemplateCopy()
		{
			CFSShipment newShipment = (CFSShipment)Factory.New(this.GetType());

			newShipment.JS_OH_HandledOnBehalfOfForwarder = JS_OH_HandledOnBehalfOfForwarder;
			newShipment.ConsigneePK = ConsigneePK;
			newShipment.ConsignorPK = ConsignorPK;
			newShipment.JS_TransportMode = TransportMode;
			newShipment.JS_RL_NKOrigin = JS_RL_NKOrigin;
			newShipment.JS_RL_NKDestination = JS_RL_NKDestination;
			newShipment.JS_JX = JS_JX;

			return newShipment;
		}

		void CFSShipment_UpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			if (outerPackLines != null)
			{
				SetPackedPackLinesReadOnly();
			}
		}

		#endregion

		#region Properties Overrides

		#region JS_ShipmentType

		public override ZString JS_ShipmentType
		{
			get { return base.JS_ShipmentType; }
			set
			{
				var originalValue = base.JS_ShipmentType;

				base.JS_ShipmentType = value;

				if (originalValue == Constants.ShipmentTypes.BuyersConsolLead && value != Constants.ShipmentTypes.BuyersConsolLead)
				{
					JS_PackingMode = Constants.ContainerModes.LCL;
				}

				if (originalValue != Constants.ShipmentTypes.BuyersConsolLead && value == Constants.ShipmentTypes.BuyersConsolLead)
				{
					JS_PackingMode = Constants.ContainerModes.BuyersConsol;
				}
			}
		}

		#endregion

		#region JS_RL_NKOrigin

		[List("Lookups.RefUNLOCO_List")]
		public override ZString JS_RL_NKOrigin
		{
			get { return base.JS_RL_NKOrigin; }
			set
			{
				base.JS_RL_NKOrigin = value;
				if (this.IsExport())
				{
					JS_TranshipToOtherCFS = false;
				}
				else
				{
					JS_TranshipToOtherCFS = this.IsImport();
				}
			}
		}

		#endregion

		#region JS_RL_NKDestination

		[List("Lookups.RefUNLOCO_List")]
		public override ZString JS_RL_NKDestination
		{
			get { return base.JS_RL_NKDestination; }
			set
			{
				base.JS_RL_NKDestination = value;
				JS_TranshipToOtherCFS = this.IsImport();
			}
		}

		#endregion

		#region  JS_ActualChargeable

		public override ZDecimal JS_ActualChargeable
		{
			get { return base.JS_ActualChargeable; }
			set
			{
				decimal newValue = value;
				if (IsAir)
				{
					newValue = ChargeableWeightRoundingHelper.GetRoundedValueAir(JobShipmentSchema.JS_ActualChargeable, newValue);
				}
				base.JS_ActualChargeable = newValue;
			}
		}

		#endregion

		#region JS_IsFullyDelivered

		public ZBool JS_IsFullyDelivered
		{
			get
			{
				bool delivered = true;
				if (OuterPackLines.Count == 0)
				{
					delivered = false;
				}
				else
				{
					foreach (CFSPackLine packLine in OuterPackLines)
					{
						if (!packLine.IsFullyDelivered)
						{
							delivered = false;
							break;
						}
					}
				}

				return delivered;
			}
		}

		public ZPropertyInfo JS_IsFullyDeliveredInfo
		{
			get { return GetZPropertyInfo(Schema.JS_IsFullyDelivered); }
		}

		#endregion

		#region JS_OH_HandledOnBehalfOfForwarder

		public override ZGuid JS_OH_HandledOnBehalfOfForwarder
		{
			get
			{
				ZGuid result = base.JS_OH_HandledOnBehalfOfForwarder;

				if (!isForwarderCalculationSuppressed && result.IsEmpty && JS_OH_HandledOnBehalfOfForwarderInfo.OriginalValue.IsEmpty)
				{
					var departureConsol = GetDepartureConsol();

					if (this.IsImport() && ArrivalConsol != null && !ArrivalConsol.IsDeleted && ArrivalConsol.IsInDatabase)
					{
						result = (!JS_JS_ColoadMasterShipment.IsEmpty && CoLoadMasterShipment != null)
							? CoLoadMasterShipment.ConsigneePK
							: ArrivalConsol.ReceivingForwarderPK;
					}
					else if (this.IsExport() && departureConsol != null && !departureConsol.IsDeleted && departureConsol.IsInDatabase)
					{
						result = (!JS_JS_ColoadMasterShipment.IsEmpty && CoLoadMasterShipment != null)
							? CoLoadMasterShipment.ConsignorPK
							: departureConsol.SendingForwarderPK;
					}
				}

				return result;
			}
			set
			{
				base.JS_OH_HandledOnBehalfOfForwarder = value;
				ZBool isForward = false;
				if (HandledOnBehalfOfForwarder != null)
				{
					isForward = HandledOnBehalfOfForwarder.IsProxyOrgOfAnyCompany();
				}
				if (!JS_IsForwardRegistered && isForward || !IsInDatabase)
				{
					JS_IsForwardRegistered = isForward;
				}
			}
		}

		internal IDisposable SuppressForwarderCalculation()
		{
			isForwarderCalculationSuppressed = true;
			return new DisposableAction(() => isForwarderCalculationSuppressed = false);
		}

		bool isForwarderCalculationSuppressed;
		#endregion

		#region other new properties and methods

		protected bool fAllowSurplusPacks;

		public bool AllowSurplusPacks
		{
			get { return fAllowSurplusPacks; }
			set { fAllowSurplusPacks = value; }
		}

		protected CFSPackLine fDefaultPackLine;

		public CFSPackLine DefaultPackLine
		{
			get
			{
				if (OuterPackLines.Count > 0)
				{
					return OuterPackLines[0];
				}
				else if (OuterPackLines.Count == 0)
				{
					return OuterPackLines.AddNew();
				}
				else
				{
					return null;
				}
			}
		}

		public bool IsAtLeastPartiallyDelivered //Dispatched from CFS
		{
			get
			{
				foreach (CommonPickupDeliveryConfirm confirm in DestinationCFSDepartures)
				{
					if (confirm.TotalDeliveredPackages > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public CFSContainer ParentContainerRegistration
		{
			get { return fParentContainerRegistration; }
			set { fParentContainerRegistration = value; }
		}

		protected CFSContainer fParentContainerRegistration;

		protected bool fValidateTotalsAgainstPackLines;

		public bool ValidateTotalsAgainstPackLines
		{
			get { return fValidateTotalsAgainstPackLines; }
			set { fValidateTotalsAgainstPackLines = value; }
		}

		#endregion

		#region JS_InterimReceipt & JS_A_RCV

		[ReadOnlyMember(nameof(HasImportConsol))]
		public override ZString JS_InterimReceipt
		{
			get { return base.JS_InterimReceipt; }
			set { base.JS_InterimReceipt = value; }
		}

		[ReadOnlyMember(nameof(HasImportConsol))]
		public override ZDateTime JS_A_RCV
		{
			get { return base.JS_A_RCV; }
			set { base.JS_A_RCV = value; }
		}

		#endregion

		#region JS_Calc_JobNumber

		public ZString JS_Calc_JobNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (!JS_UniqueConsignRef.IsEmpty)
				{
					if (this.IsExport())
					{
						result = JS_UniqueConsignRef.Replace('S', 'P');
					}
					else
					{
						result = JS_UniqueConsignRef.Replace('S', 'U');
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JS_Calc_JobNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_JobNumber); }
		}

		#endregion

		#region HasImportConsol

		public bool HasImportConsol
		{
			get
			{
				return Consols.Cast<CFSLoadListConsol>()
					.Any(c => c.IsImport());
			}
		}

		#endregion

		#region MessageCaption

		ZString fMessageCaption;

		public ZString MessageCaption
		{
			get { return fMessageCaption; }
			set { fMessageCaption = value; }
		}

		#endregion

		#region WarningMessage

		ZString fWarningMessage;

		[MaxLength(300)]
		public ZString WarningMessage
		{
			get { return fWarningMessage; }
			set
			{
				CheckMaximumLength(WarningMessageInfo, value);
				fWarningMessage = value;
				WarningMessageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WarningMessageInfo
		{
			get { return GetZPropertyInfo(nameof(WarningMessage)); }
		}

		#endregion

		#region ErrorMessage

		ZString fErrorMessage;

		[MaxLength(250)]
		public ZString ErrorMessage
		{
			get { return fErrorMessage; }
			set
			{
				CheckMaximumLength(ErrorMessageInfo, value);
				fErrorMessage = value;
				ErrorMessageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ErrorMessageInfo
		{
			get { return GetZPropertyInfo(nameof(ErrorMessage)); }
		}

		#endregion

		#region JS_IsCancelled

		public override ZBool JS_IsCancelled
		{
			get { return base.JS_IsCancelled; }
			set
			{
				base.JS_IsCancelled = value;

				if (!IsDeleted)
				{
					DocsAndCartage.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		protected override bool IsForPickupCartage
		{
			get { return this.IsExport() || JS_RL_NKOrigin == GlbBranch.CurrentBranch.GB_RL_NKHomePort; }
		}

		#region Status Properties

		public ZString JS_GatePassStatus
		{
			get { return ManualReleaseReason.IsEmpty ? StatusProvider.Status : ManualReleaseReason; }
		}

		public ZPropertyInfo JS_GatePassStatusInfo
		{
			get { return GetZPropertyInfo(Schema.JS_GatePassStatus); }
		}

		public ZString JS_GatePassStatusShort
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					if (!CustomsManualStatus.IsEmpty)
					{
						return Res.GetString("E4F0C109-6B58-4DDD-B708-35E4FD2F715E", "MANUAL");
					}
				}
				return StatusProvider.ShortStatus;
			}
		}

		public ZPropertyInfo JS_GatePassStatusShortInfo
		{
			get { return GetZPropertyInfo(Schema.JS_GatePassStatusShort); }
		}

		public StatusClass JS_GatePassStatusClass
		{
			get { return StatusProvider.StatusClass; }
		}

		public ZString UserFriendlyStatus
		{
			get { return StatusProvider.DetailsFromMessages; }
		}

		#endregion

		#region JS_OA_CartageCoAddr

		[RelatedBusinessObject("CartageCoAddr")]
		public ZGuid JS_OA_CartageCoAddr
		{
			get
			{
				return (IsForPickupCartage) ? DocsAndCartage.JP_OA_PickupCartageCoAddr : DocsAndCartage.JP_OA_DeliveryCartageCoAddr;
			}
			set
			{
				if (JS_OA_CartageCoAddr != value)
				{
					if (IsForPickupCartage)
					{
						DocsAndCartage.JP_OA_PickupCartageCoAddr = value;
					}
					else
					{
						DocsAndCartage.JP_OA_DeliveryCartageCoAddr = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJS_OA_CartageCoAddr();
					}
				}
				JS_OA_CartageCoAddrInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JS_OA_CartageCoAddrInfo
		{
			get { return GetZPropertyInfo(Schema.JS_OA_CartageCoAddr); }
		}

		public ZAddress JS_OA_CartageCoAddr_ZAddress
		{
			get
			{
				ZAddress result = new ZAddress(JS_OA_CartageCoAddrInfo);
				result.DefaultAddressType = AddressType.OFC;
				return result;
			}
		}

		public OrgAddress CartageCoAddr
		{
			get { return Factory.Load<OrgAddress>(JS_OA_CartageCoAddr); }
		}

		public OrgHeader CartageCo
		{
			get { return CartageCoAddr != null ? CartageCoAddr.Header : null; }
		}

		public ZGuid CartageCoPK
		{
			get { return CartageCo != null ? CartageCo.PK : ZGuid.Empty; }
			set
			{
				OrgHeader org = Factory.Load<OrgHeader>(value);
				JS_OA_CartageCoAddr = org != null ? org.MainAddress.PK : ZGuid.Empty;
			}
		}

		#endregion

		#region DefaultRequiredDocuments

		protected override bool DefaultRequiredDocuments
		{
			get { return false; }
		}

		#endregion

		#region Canada Specific

		public ZString CanadaHouseCCN
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					var ccnNumber = Numbers.Find(num => num.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Canada
														&& num.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN)
						.FirstOrDefault();
					return (ccnNumber != null) ? ccnNumber.CE_EntryNum : ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString CanadaLoadListCCN
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					return (Consols.Count > 0)
						? Consols.ToArray<CFSLoadListConsol>().Where(c => !c.CanadaCCNNumber.IsEmpty)
							.Select(c => c.CanadaCCNNumber.Replace(" ", "")).Distinct()
							.OrderBy(s => s).DefaultIfEmpty().Aggregate((r, n) => r + ", " + n)
						: ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString CanadaLoadListPCN
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					return (Consols.Count > 0)
						? Consols.ToArray<CFSLoadListConsol>().Where(c => !c.CanadaPCNNumber.IsEmpty)
							.Select(c => c.CanadaPCNNumber.Replace(" ", "")).Distinct()
							.OrderBy(s => s).DefaultIfEmpty().Aggregate((r, n) => r + ", " + n)
						: ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#region CustomsEntryNumber

		public override ZString CustomsEntryNumber
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada &&
					base.CustomsEntryNumber.IsEmpty)
				{
					return rnsStatusProvider.TransactionNumber;
				}
				else
				{
					return base.CustomsEntryNumber;
				}
			}
			set
			{
				if (!CustomsEntryNumberInfo.ReadOnly)
				{
					base.CustomsEntryNumber = value;
				}
			}
		}

		CFSShipmentRNSStatusProvider rnsStatusProvider
		{
			get
			{
				if (fRNSStatusProvider == null)
				{
					fRNSStatusProvider = CFSShipmentRNSStatusProvider.New(this);
				}

				return fRNSStatusProvider;
			}
		}

		CFSShipmentRNSStatusProvider fRNSStatusProvider;

		public override ZPropertyInfo CustomsEntryNumberInfo
		{
			get
			{
				ZPropertyInfo info = base.CustomsEntryNumberInfo;
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada &&
					base.CustomsEntryNumber.IsEmpty && !rnsStatusProvider.TransactionNumber.IsEmpty)
				{
					((IZPropertyInfoObsolete)info).ReadOnly = true;
				}
				else
				{
					((IZPropertyInfoObsolete)info).ReadOnly = false;
				}

				return info;
			}
		}

		#endregion

		#region RNS Status

		public ZString RNSReleaseStatus
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					return rnsStatusProvider.ReleaseStatus;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZDateTime RNSReleaseDate
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					return rnsStatusProvider.ReleaseDate;
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		#endregion

		#region Arrival Certification Status

		public ZString ArrivalCertificationStatus
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					return rnsStatusProvider.ArrivalCertificationStatus;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString ArrivalCertificationStatusCode
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					return rnsStatusProvider.ArrivalCertificationStatusCode;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZDateTime ArrivalCertificationDate
		{
			get
			{
				if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
				{
					return rnsStatusProvider.ArrivalCertificationDate;
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		#endregion

		#region JS_IsForwardRegistered

		public override ZBool JS_IsForwardRegistered
		{
			get => base.JS_IsForwardRegistered;
			set
			{
				base.JS_IsForwardRegistered = value;
				DefaultJS_ShipmentStatus();
			}
		}

		#endregion

		#endregion

		#endregion

		#region Transhipment

		public ZBool IsTranshipment()
		{
			return MostInterestingTransport != null && !JS_RL_NKDestination.IsEmpty &&
				   !MostInterestingTransport.JW_RL_NKDiscPort.IsEmpty
				   && JS_RL_NKDestination.SubstringSafe(0, 2) != MostInterestingTransport.JW_RL_NKDiscPort.SubstringSafe(0, 2);
		}

		#endregion

		#region OnForwarding

		public ZBool IsOnForwarding()
		{
			return MostInterestingTransport != null && !JS_RL_NKDestination.IsEmpty &&
				   !MostInterestingTransport.JW_RL_NKDiscPort.IsEmpty
				   && JS_RL_NKDestination != MostInterestingTransport.JW_RL_NKDiscPort
				   && JS_RL_NKDestination.SubstringSafe(0, 2) == MostInterestingTransport.JW_RL_NKDiscPort.SubstringSafe(0, 2);
		}

		#endregion

		#region New Methods

		public void SetWarnNotErrorOnLocationTotalsOnPackLines(bool warnInsteadOfError)
		{
			foreach (CFSPackLine pack in OuterPackLines)
			{
				pack.WarnNotErrorOnLocationTotals = warnInsteadOfError;
			}
		}

		public void SetPackedPackLinesReadOnly()
		{
			foreach (CFSPackLine pack in OuterPackLines)
			{
				pack.SetPackageDetailsReadOnly(pack.Containers.Count > 0);
			}
		}

		#endregion

		#region Related Business Objects

		#region StatusProvider

		protected internal CFSShipmentStatusProvider StatusProvider
		{
			get
			{
				if (statusProvider == null)
				{
					InitialiseStatusProvider();
				}
				return statusProvider;
			}
		}

#if DEBUG
		internal
#endif
			CFSShipmentStatusProvider statusProvider;

		CFSShipmentStatusProvider IHasStatusProvider.StatusProvider
		{
			get { return StatusProvider; }
		}

		void InitialiseStatusProvider()
		{
			statusProvider = new FallbackCFSShipmentStatusProvider(this);
		}

		internal bool LogsNeededToDetermineSeaCargoStatusWerePrefetched;

		#endregion

		#region DocsAndCartage

		public new CFSDocsAndCartage DocsAndCartage
		{
			get { return (CFSDocsAndCartage)base.DocsAndCartage; }
		}

		public override Type DocsAndCartageType
		{
			get { return typeof(CFSDocsAndCartage); }
		}

		public override Type DocsAndCartageParentType
		{
			get { return this.GetType(); }
		}

		#endregion

		#region Consols/Shipments

		public new CFSShipment CoLoadMasterShipment
		{
			get { return (CFSShipment)base.CoLoadMasterShipment; }
		}

		public new CFSLoadListConsolManyToManyCollection Consols
		{
			get { return (CFSLoadListConsolManyToManyCollection)base.Consols; }
		}

		protected override ConsolCollection GetNewConsolCollection()
		{
			return new CFSLoadListConsolManyToManyCollection(this);
		}

		protected void CoLoadShipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsBuyersConsolLead && ParentContainerRegistration != null)
			{
				bool aggReadOnly = CoLoadShipments.Count > 0;
				OuterPackLines.SetReadOnlyIncludingChildren(aggReadOnly);
			}
		}

		protected override void AdjustCoLoadMasterListFilterAndFilterBusinessObjectDefaults(
			IRelatedShipmentsCollection masterShipmentCollection)
		{
			masterShipmentCollection.AdditionalFilter.AddToFilter(JobShipmentSchema.JS_IsCFSRegistered, true);
		}

		#endregion

		#region OuterPackLines

		[ChildEditable(true)]
		public new CFSPackLineCollection OuterPackLines
		{
			get { return (CFSPackLineCollection)base.OuterPackLines; }
		}

		protected override OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			CFSPackLineCollection result = new CFSPackLineCollection(this, Factory);
			result.CountChanged += new CollectionCountChangedEventHandler(OuterPackLines_CountChanged);

			return result;
		}

		protected override void OuterPackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var packLine = e.BizObject as CFSPackLine;
			if (packLine != null)
			{
				if (e.ItemAdded)
				{
					bool isPacklineReadOnly = false;
					foreach (var container in Containers)
					{
						if (container.PackLines.Contains(packLine.PK))
						{
							isPacklineReadOnly = true;
						}
					}
					packLine.SetPackageDetailsReadOnly(isPacklineReadOnly);
				}
			}
		}

		#endregion

		#region Inner Pack Lines

		protected override InnerPackLineCollection GetNewInnerPackLinesCollection()
		{
			return new CFSInnerPackLineCollection(this);
		}

		#endregion

		#region Sailing

		public override JobSailing Sailing
		{
			get
			{
				JobSailing result = null;
				if (Consols.Count > 0)
				{
					if (ArrivalConsol != null
						&& ArrivalConsol.Schedule != null && !ArrivalConsol.Schedule.IsDeleted
						&& ArrivalConsol.Schedule.Destination != null && !ArrivalConsol.Schedule.Destination.IsDeleted)
					{
						if (
							ArrivalConsol.Schedule.Destination.JB_RL_NKPortOfDischarge.StartsWith(
								GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString(), StringComparison.Ordinal))
						{
							result = ArrivalConsol.Schedule;
						}
					}
					else if (DepartureConsol != null
							 && DepartureConsol.Schedule != null && !DepartureConsol.Schedule.IsDeleted
							 && DepartureConsol.Schedule.Origin != null && !DepartureConsol.Schedule.Origin.IsDeleted)
					{
						if (
							DepartureConsol.Schedule.Origin.JA_RL_NKPortOfLoading.StartsWith(
								GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString(), StringComparison.Ordinal))
						{
							result = DepartureConsol.Schedule;
						}
					}
					if (result == null)
					{
						foreach (CommonConsol consol in Consols)
						{
							if (consol.Schedule != null)
							{
								result = consol.Schedule;
								break;
							}
						}
					}
				}
				else
				{
					result = base.Sailing;
				}

				return result;
			}
		}

		#endregion

		protected override bool CanHaveDeclarations
		{
			get { return false; }
		}

		#endregion

		protected override ZGuid CartageClientIDDefaultOnShipmentSpecificInternalCartageSetup
		{
			get { return JS_OH_HandledOnBehalfOfForwarder; }
		}

		#region PopulateBillAndShipmentNumberIfNeededCore

		protected override void PopulateBillAndShipmentNumberIfNeededCore()
		{
			if (OnSaveWillChangeFromCFSJobNumberToForwardingJobNumber)
			{
				var oldRef = JS_UniqueConsignRef;

				using (EnableShipmentNumberRegeneration())
				{
					base.PopulateBillAndShipmentNumberIfNeededCore();
				}

				var reference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Changed job number from {0} to {1}", oldRef, JS_UniqueConsignRef);
				jobReferenceNumberChangedLog = Logs.CreateRecreateOrUpdateEventLog(Events.EditedARecord, EstimateActual.Actual,
					ZDateTimeOffset.Now, reference);
			}
			else
			{
				base.PopulateBillAndShipmentNumberIfNeededCore();
			}
		}

		StmALog jobReferenceNumberChangedLog;

		public bool OnSaveWillChangeFromCFSJobNumberToForwardingJobNumber
		{
			get { return HasCFSJobNumber && ShouldHaveForwardingJobNumber; }
		}

		#endregion

		#region Light Validation

		protected override JobDocAddress GetNewConsignorDocumentaryAddress()
		{
			JobDocAddress result = base.GetNewConsignorDocumentaryAddress();
			result.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(JobDocAddress_HasChangesChanged);
			return result;
		}

		protected override JobDocAddress GetNewConsigneeDocumentaryAddress()
		{
			JobDocAddress result = base.GetNewConsigneeDocumentaryAddress();
			result.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(JobDocAddress_HasChangesChanged);
			return result;
		}

		protected override JobDocAddress GetNotifyPartyDocumentaryAddress()
		{
			JobDocAddress result = base.GetNotifyPartyDocumentaryAddress();
			result.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(JobDocAddress_HasChangesChanged);
			return result;
		}

		void JobDocAddress_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			MarkAsNeedingValidation();
		}

		#endregion

		#region Address Override Security

		protected override SecurityCheckpoint ConsigneeAddressOverrideCheckpoint
		{
			get
			{
				SecurityCheckpoint result;

				if (this.IsImport())
				{
					switch (JS_TransportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.AirSea:
							result = Env.Security.CFSShipmentImportCFSImpAirConsigneeD;
							result = Env.Security.CFSShipmentImportCFSImpAirConsigneeD;
							break;

						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
							result = Env.Security.CFSShipmentImportCFSImpSeaConsigneeD;
							break;

						case Constants.TransportModes.Rail:
							result = Env.Security.CFSShipmentImportCFSImpRailConsigneeD;
							break;

						case Constants.TransportModes.Road:
							result = Env.Security.CFSShipmentImportCFSImpRoadConsigneeD;
							break;

						default:
							result = Env.Security.CFSShipmentImportCFSImpOtConsignee;
							break;
					}
				}
				else if (this.IsExport())
				{
					switch (JS_TransportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.AirSea:
							result = Env.Security.CFSShipmentExportCFSExpAirConsigneeD;
							break;

						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
							result = Env.Security.CFSShipmentExportCFSExpSeaConsigneeD;
							break;

						case Constants.TransportModes.Rail:
							result = Env.Security.CFSShipmentExportCFSExpRailConsigneeD;
							break;

						case Constants.TransportModes.Road:
							result = Env.Security.CFSShipmentExportCFSExpRoadConsigneeD;
							break;

						default:
							result = Env.Security.CFSShipmentExportCFSExpOtherConsigneeD;
							break;
					}
				}
				else
				{
					result = Env.Security.None;
				}

				return result;
			}
		}

		protected override SecurityCheckpoint ConsignorAddressOverrideCheckpoint
		{
			get
			{
				SecurityCheckpoint result;

				if (this.IsImport())
				{
					switch (JS_TransportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.AirSea:
							result = Env.Security.CFSShipmentImportCFSImpAirConsignorD;
							break;

						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
							result = Env.Security.CFSShipmentImportCFSImpSeaConsignorD;
							break;

						case Constants.TransportModes.Rail:
							result = Env.Security.CFSShipmentImportCFSImpRailConsignorD;
							break;

						case Constants.TransportModes.Road:
							result = Env.Security.CFSShipmentImportCFSImpRoadConsignorD;
							break;

						default:
							result = Env.Security.CFSShipmentImportCFSImpOtConsignor;
							break;
					}
				}
				else if (this.IsExport())
				{
					switch (JS_TransportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.AirSea:
							result = Env.Security.CFSShipmentExportCFSExpAirConsignorD;
							break;

						case Constants.TransportModes.Sea:
						case Constants.TransportModes.SeaAir:
							result = Env.Security.CFSShipmentExportCFSExpSeaConsignorD;
							break;

						case Constants.TransportModes.Rail:
							result = Env.Security.CFSShipmentExportCFSExpRailConsignorD;
							break;

						case Constants.TransportModes.Road:
							result = Env.Security.CFSShipmentExportCFSExpRoadConsignorD;
							break;

						default:
							result = Env.Security.CFSShipmentExportCFSExpOtherConsignorD;
							break;
					}
				}
				else
				{
					result = Env.Security.None;
				}

				return result;
			}
		}

		#endregion

		#region Generation of Job ID

		protected bool IsNewCFSJob
		{
			get { return JS_UniqueConsignRef.IsEmpty && JS_IsCFSRegistered && !JS_IsForwardRegistered; }
		}

		protected bool ShouldHaveForwardingJobNumber
		{
			get
			{
				ZQuery jobHeaderFilter = new ZQuery(JobHeaderSchema.JH_ParentID, PK);
				bool attachedJobHeader = Factory.LoadTop1(typeof(JobHeader), jobHeaderFilter) != null;
				return JS_IsCFSRegistered && JS_IsForwardRegistered && !JS_UniqueConsignRef.IsEmpty && !attachedJobHeader;
			}
		}

		protected bool HasCFSJobNumber
		{
			get { return JS_UniqueConsignRef.StartsWith(NumberFountains.JobShipmentFountainCFSPrefix, StringComparison.Ordinal); }
		}

		protected override INumberFountainProxy NumberFountainForUniqueConsignRef
		{
			get
			{
				if (IsNewCFSJob)
				{
					return Env.NumberFountains.JobShipmentNumberCFS;
				}
				else
				{
					return base.NumberFountainForUniqueConsignRef;
				}
			}
		}

		#endregion

		#region IJobDocsAndCartage overrides

		protected override bool RequireOrderNumbersOnDocs()
		{
			return false;
		}

		protected override ZString JS_CartageTypeOverride
		{
			get { return IsForPickupCartage ? Constants.CartageJobType.LCLExport : Constants.CartageJobType.LCLImport; }
		}

		protected override ZGuid PickupDepotAddress
		{
			get
			{
				ZGuid result = base.PickupDepotAddress;
				if (result.IsEmpty)
				{
					var orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
					result = orgProxy.GetAddressWithFallback(AddressType.DLV).PK;
				}
				return result;
			}
		}

		protected override ZGuid DeliveryDepotAddress
		{
			get
			{
				ZGuid result = base.DeliveryDepotAddress;
				if (result.IsEmpty)
				{
					OrgHeader org = GlbBranch.CurrentBranch.OrgProxy;
					if (org != null)
					{
						result = org.GetAddressWithFallback(AddressType.PIC).PK;
					}
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		public bool ContinueWithChanging;

		#region IsPacked

		internal bool IsPacked
		{
			get
			{
				foreach (CFSPackLine packLine in OuterPackLines)
				{
					if (packLine.Containers.Count > 0)
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		#region CheckClientAndSailingAreDefaultedFromLoadList

		public void CheckClientAndSailingAreDefaultedFromLoadList()
		{
			if (IsInDatabase && Consols.Count == 1)
			{
				CFSLoadListConsol loadList = Consols[0];
				if (JS_OH_HandledOnBehalfOfForwarder.IsEmpty && !loadList.JK_OH_Forwarder.IsEmpty)
				{
					bool updateClient = true;
					//ContinueWithChanging = true;
					if (CheckDefaultDetailsFromLoadList != null)
					{
						CancelEventArgs e = new CancelEventArgs();
						CheckDefaultDetailsFromLoadList(this, e);
						updateClient = !(e.Cancel);
					}
					if (updateClient)
					{
						if (JS_OH_HandledOnBehalfOfForwarder.IsEmpty && JS_JS_ColoadMasterShipment.IsEmpty)
						{
							JS_OH_HandledOnBehalfOfForwarder = loadList.JK_OH_Forwarder;
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region Chargeable Weights

		protected override void UpdateDocumentedChargeableWeight()
		{
			if (IsAir && WeightVolumeUnitsAreValid)
			{
				decimal newValue = ChargeableWeight(JS_DocumentedVolume, JS_DocumentedWeight);
				JS_DocumentedChargeable = ChargeableWeightRoundingHelper.GetRoundedValueAir(JobShipmentSchema.JS_DocumentedChargeable, newValue);
			}
			else
			{
				base.UpdateDocumentedChargeableWeight();
			}
		}

		protected override void UpdateManifestedChargeableWeight()
		{
			if (IsAir && WeightVolumeUnitsAreValid)
			{
				decimal newValue = ChargeableWeight(JS_ManifestedVolume, JS_ManifestedWeight);
				JS_ManifestedChargeable = ChargeableWeightRoundingHelper.GetRoundedValueAir(JobShipmentSchema.JS_ManifestedChargeable, newValue);
			}
			else
			{
				base.UpdateManifestedChargeableWeight();
			}
		}

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo NewDocManager()
		{
			return new CFSShipmentDocManagerInfo(this);
		}

		#endregion

		#region IDocumentSupportable

		public override DocumentSupporter DocumentSupporter
		{
			get { return new CFSShipmentDocumentSupporter(this); }
		}

		#endregion

		#region IJobInvoicingPlugIn Overrides

		protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new CFSShipmentInvoicingSupporter(this);
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.CFSShipment.Code; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CFSShipmentProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = this.GetJobRelatedTemplateSelectionCriteria();

			if (!JS_OH_HandledOnBehalfOfForwarder.IsEmpty)
			{
				result = AdjustClientPriority(result);
			}

			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JS_TransportMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, JS_PackingMode, ZString.Empty);
			return result;
		}

		ColumnValueRanker AdjustClientPriority(ColumnValueRanker result)
		{
			var columnValues = ((IColumnValueRankerInternals)result).ColumnValues;

			result = new ColumnValueRanker();
			object[] values;

			foreach (var columnValuesPair in columnValues)
			{
				values = columnValuesPair.Values;

				if (columnValuesPair.Column != null
					&& columnValuesPair.Column == ProcessTaskTemplateSchema.P0_OH_Client)
				{
					object[] newValues = { JS_OH_HandledOnBehalfOfForwarder };

					values = values != null
						? newValues.Union(values).ToArray()
						: newValues;
				}

				result.Add(columnValuesPair.Column, values);
			}

			return result;
		}

		CFSShipmentProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region ICartageParent Members

		event EventHandler ICartageParent.CartageTypesChanged
		{
			add
			{
				JS_RL_NKOriginInfo.ValueChanged += value;
				JS_RL_NKDestinationInfo.ValueChanged += value;
			}
			remove
			{
				JS_RL_NKOriginInfo.ValueChanged -= value;
				JS_RL_NKDestinationInfo.ValueChanged -= value;
			}
		}

		public override void OnJobCreating(JobHeader job)
		{
			CartageHelper.AttachCartageJobsToParentJob(job, PK);
		}

		IReadOnlyCollection<CartageType> ICartageParent.CartageTypes
		{
			get { return new[] { ((ICartageParent)this).GetLocalCartageType }; }
		}

		CartageType ICartageParent.GetLocalCartageType
		{
			get
			{
				return IsForPickupCartage
					? new ShipmentPickupCartageType(this)
					: new ShipmentDeliveryCartageType(this);
			}
		}

		ZString ICartageParent.UniqueConsignmentID
		{
			get { return JS_UniqueConsignRef; }
		}

		ZGuid ICartageParent.CartageParentID
		{
			get { return PK; }
		}

		ZString ICartageParent.CartageParentTableCode
		{
			get { return JobShipmentSchema.Constants.Prefix; }
		}

		ZGuid ICartageParent.BranchPK
		{
			get { return ZGuid.Empty; }
		}

		ZString ICartageParent.OrderReferenceNumber
		{
			get { return JS_OrderReferences; }
		}

		ZString ICartageParent.ServiceLevel
		{
			get { return JS_RS_NKServiceLevel; }
		}

		ControllerID ICartageParent.ControllerID
		{
			get { return ControllerIDs.ShipmentReceival; }
		}

		ZString ICartageParent.WayBillNumber
		{
			get { return JS_HouseBill; }
		}

		ZString ICartageParent.GoodsDescription
		{
			get { return JS_GoodsDescription; }
		}

		ZGuid ICartageParent.JobHeaderPK
		{
			get { return Job != null ? Job.PK : ZGuid.Empty; }
		}

		ZGuid ICartageParent.LocalClientAddressPK
		{
			get { return Job != null ? Job.JH_OA_LocalChargesAddr : ZGuid.Empty; }
		}

		ZInt ICartageParent.TotalPackages
		{
			get { return JS_OuterPacks; }
		}

		ZString ICartageParent.TotalPackType
		{
			get { return JS_F3_NKPackType; }
		}

		ZDecimal ICartageParent.TotalWeight
		{
			get { return JS_ActualWeight; }
		}

		ZString ICartageParent.TotalWeightUnit
		{
			get { return JS_UnitOfWeight; }
		}

		ZDecimal ICartageParent.TotalVolume
		{
			get { return JS_ActualVolume; }
		}

		ZString ICartageParent.TotalVolumeUnit
		{
			get { return JS_UnitOfVolume; }
		}

		bool ICartageParent.RebuildLocalCartageMenuOnClick
		{
			get { return false; }
		}

		bool ICartageParent.UseJobTotals
		{
			get { return false; }
		}

		void ICartageParent.CartageCreatedAndSaved()
		{
		}

		IStmALogParent ICartageParent.BusinessObjectForRelatedEvents
		{
			get { return this; }
		}

		IDocManagerSupport ICartageParent.BusinessObjectForRelatedEDocs
		{
			get { return this; }
		}

		#endregion

		#region ICartageParentExtra Members

		ZString ICartageParentExtra.CustomAttrib1
		{
			get { return DocsAndCartage.JP_CustomAttrib1; }
		}

		ZString ICartageParentExtra.CustomAttrib2
		{
			get { return DocsAndCartage.JP_CustomAttrib2; }
		}

		ZDateTime ICartageParentExtra.CustomDate1
		{
			get { return DocsAndCartage.JP_CustomDate1; }
		}

		ZDateTime ICartageParentExtra.CustomDate2
		{
			get { return DocsAndCartage.JP_CustomDate2; }
		}

		ZDecimal ICartageParentExtra.CustomDecimal1
		{
			get { return DocsAndCartage.JP_CustomDecimal1; }
		}

		ZDecimal ICartageParentExtra.CustomDecimal2
		{
			get { return DocsAndCartage.JP_CustomDecimal2; }
		}

		ZBool ICartageParentExtra.CustomFlag1
		{
			get { return DocsAndCartage.JP_CustomFlag1; }
		}

		ZBool ICartageParentExtra.CustomFlag2
		{
			get { return DocsAndCartage.JP_CustomFlag2; }
		}

		#endregion

		#region IStatusClassProvider Members

		StatusClass IStatusClassProvider.StatusClass
		{
			get { return JS_GatePassStatusClass; }
		}

		#endregion

		#region Rating

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new ShipmentRatingAdaptersProvider<CFSShipment>(this); }
		}

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new CFSShipmentRatingAdapter<CFSShipment>(this);
		}

		#endregion

		#region Invoicing Supporter

		public class CFSShipmentInvoicingSupporter : CommonShipmentInvoicingSupporter
		{
			public CFSShipmentInvoicingSupporter(CFSShipment parent)
				: base(parent)
			{
			}

			protected override SecurityCheckpoint GetAuditSecurityCore()
			{
				return Env.Security.CFSShipmentAuditBilling;
			}

			public override OrgHeader SendingAgent
			{
				get
				{
					var result = base.SendingAgent;
					if (result == null)
					{
						if (IsExport)
						{
							result = Shipment.HandledOnBehalfOfForwarder;
						}
					}

					return result;
				}
			}
		}

		#endregion

		#region IOverrideStorageMainDocManagerCode

		ZString IOverrideStorageMainDocManagerCode.GetOverridenCodeIfNecessary(ZString docManagerCode)
		{
			return docManagerCode == Core.Constants.DocManagerCodes.Shipment
				&& Factory.GetFreightDomainContext() == FreightDomainContext.CFS // this should not happen (once a factory is a CFS factory it's always a CFS factory), but was left for backwards compatibility, TODO remove once the CFS/Forwarding loading issue is fixed
				? (ZString)Core.Constants.DocManagerCodes.CFSShipmentReceival
				: docManagerCode;
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.ShipmentReceival; }
		}

		#endregion
	}
}
