using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using CusEntryHeader = Enterprise.Customs.ZA.Business.CusEntryHeader;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader
		, Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader
		, ICaseNumberCollectionProvider
		, IManifestSupportingDocSendingObject
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string MasterCarrierCode = "MasterCarrierCode";
			public const int MasterCarrierCodeMaxLength = 10;
		}
		[MaxLength(10)]
		public override ZString AMA_Trailer1RegNo { get => base.AMA_Trailer1RegNo; set => base.AMA_Trailer1RegNo = value; }
		[MaxLength(10)]
		public override ZString AMA_Trailer2RegNo { get => base.AMA_Trailer2RegNo; set => base.AMA_Trailer2RegNo = value; }

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);
		public new AsycudaContainerCollection Containers => (AsycudaContainerCollection)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection(this);

		public new CusPersonCollection Persons => (CusPersonCollection)base.Persons;
		protected override ASYCUDA.Business.CusPersonCollection CreateNewCusPersonCollection() => new CusPersonCollection(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override Type GetPersonTypeCore() => typeof(CusPerson);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.SouthAfrica;
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		protected override bool ShowVINNumbersCore => IsRoad;
		public override AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.None;
		protected override ZBool IsDeconsolidatorEnabledCore => AreDeconsolidatorAndDischargeTerminalEnabled;
		protected override ZBool IsDischargeTerminalEnabledCore => AreDeconsolidatorAndDischargeTerminalEnabled;

		ZBool AreDeconsolidatorAndDischargeTerminalEnabled
		{
			get
			{
				return AMA_ManifestType == nameof(ManifestDocumentType.COH)
							|| AMA_ManifestType == nameof(ManifestDocumentType.BBB)
							|| AMA_ManifestType == nameof(ManifestDocumentType.FWB)
							|| AMA_ManifestType == nameof(ManifestDocumentType.HAB)
							|| AMA_ManifestType == nameof(ManifestDocumentType.ALH);
			}
		}

		public ZString DepotCode => DeconsolidateAddress?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DepotControlledPremisesID, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;
		public ZString TerminalCode => DischargeTerminalAddress?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.TerminalControlledPremisesID, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set
			{
				var oldValue = AMA_OA_Carrier;
				base.AMA_OA_Carrier = value;
				if (!IsCopying && oldValue != AMA_OA_Carrier)
				{
					UpdateManifestHeaderCarrierCode(CarrierCCCCode);
				}
			}
		}

		void UpdateManifestHeaderCarrierCode(ZString carrierCode)
		{
			AMA_CarrierCode = carrierCode;
		}

		[ResourceStringData("AsycudaManifestHeader.MasterCarrierCode", Caption = "Master Carrier Code")]
		[MaxLength(Schema.MasterCarrierCodeMaxLength)]
		public ZString MasterCarrierCode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.MasterCarrierCode);
			set
			{
				var oldValue = MasterCarrierCode;
				CheckMaximumLength(MasterCarrierCodeInfo, value);
				this.SetSystemDefinedValue(Schema.MasterCarrierCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMasterCarrierCode();
				}
				MasterCarrierCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo MasterCarrierCodeInfo => GetZPropertyInfo(Schema.MasterCarrierCode);

		public ZZRefCarrierCombined GetMasterZZCarrier(ZString carrierCode, bool onlyMaster)
		{
			return (AMA_ManifestType.Equals(nameof(ManifestDocumentType.COH)) && AMA_AgentType.Equals(Core.Constants.AgentType.CoLoad)) && !onlyMaster ?
				GetZZCarrier(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, carrierCode) :
				GetZZCarrier(Core.Constants.Customs.Universal.RefCarrierAttributeNames.MASTER, carrierCode);
		}

		public new AsycudaManifestHeaderDocWrapper GetDocWrapper() => (AsycudaManifestHeaderDocWrapper)base.GetDocWrapper();
		protected override ASYCUDA.Business.AsycudaManifestHeaderDocWrapper GetDocWrapperCore() => new AsycudaManifestHeaderDocWrapper(this);

		public override ZString AMA_CarrierCode
		{
			get => base.AMA_CarrierCode;
			set
			{
				var oldValue = AMA_CarrierCode;
				base.AMA_CarrierCode = value;
				var newValue = AMA_CarrierCode;
				if (!IsCopying && oldValue != newValue)
				{
					DefaultMasterCarrierCodeFromIsStandAlone(newValue);
				}
				AMA_CarrierCodeInfo.RefreshBinding(oldValue);
			}
		}

		void UpdateMasterCarrierCode(ZString carrierCode)
		{
			MasterCarrierCode = carrierCode.Left(AsycudaManifestHeader.Schema.MasterCarrierCodeMaxLength);
		}

		public override ZGuid AMA_OA_ShippingAgent
		{
			get => base.AMA_OA_ShippingAgent;
			set
			{
				var oldValue = AMA_OA_ShippingAgent;
				base.AMA_OA_ShippingAgent = value;
				var newValue = AMA_OA_ShippingAgent;
				if (!IsCopying && oldValue != newValue)
				{
					DefaultMasterCarrierCodeFromIsStandAlone();
				}
			}
		}

		public override ZString AMA_AgentType
		{
			get { return base.AMA_AgentType; }
			set
			{
				var oldValue = AMA_AgentType;
				base.AMA_AgentType = value;
				var newValue = AMA_AgentType;
				if (!IsCopying && oldValue != newValue)
				{
					DefaultMasterCarrierCodeFromIsStandAlone();
				}
			}
		}

		public ZString ShippingAgentCCCCode
		{
			get
			{
				ZString result = ShippingAgent?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, AMA_RN_NKCountry) ?? ZString.Empty;
				if (result.IsEmpty)
				{
					result = ShippingAgent?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, AMA_RN_NKCountry) ?? ZString.Empty;
				}
				return result;
			}
		}

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				base.AMA_TransportMode = value;
				foreach (var bill in Bills)
				{
					bill.CustomsEntryNumbers.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString AMA_Nature
		{
			get => base.AMA_Nature;
			set
			{
				var oldValue = AMA_Nature;
				base.AMA_Nature = value;
				var newValue = AMA_Nature;
				if (!IsStandAlone && oldValue != AMA_Nature)
				{
					if (AMA_ManifestType == nameof(ManifestDocumentType.COH) && AMA_AgentType == Core.Constants.AgentType.CoLoad)
					{
						AMA_OA_ShippingAgent = Consol.CreditorAddress?.PK ?? ZGuid.Empty;
					}
					else
					{
						if (newValue == Universal.Helper.ShipmentTypeList.Codes.Import23)
						{
							AMA_OA_ShippingAgent = Consol.ReceivingForwarderAddress?.PK ?? ZGuid.Empty;
						}
						if (newValue == Universal.Helper.ShipmentTypeList.Codes.Export22)
						{
							AMA_OA_ShippingAgent = Consol.SendingForwarderAddress?.PK ?? ZGuid.Empty;
						}
					}
				}
			}
		}

		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set
			{
				var oldValue = AMA_ManifestType;
				base.AMA_ManifestType = value;
				if (oldValue != value)
				{
					DefaultMasterCarrierCodeFromIsStandAlone();
					foreach (var bill in Bills)
					{
						if (!IsCopying)
						{
							bill.MarkAsNeedingValidation();
							bill.CustomsEntryNumbers.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		void DefaultMasterCarrierCodeFromIsStandAlone(string carrierCode = "")
		{
			ZString cCode = carrierCode;
			if (AMA_ManifestType == nameof(ManifestDocumentType.COH) && AMA_AgentType == Core.Constants.AgentType.CoLoad)
			{
				UpdateMasterCarrierCode(ShippingAgentCCCCode);
			}
			else
			{
				if (cCode.IsEmpty)
				{
					UpdateMasterCarrierCode(CarrierCCCCode);
				}
				else
				{
					UpdateMasterCarrierCode(cCode);
				}
			}
		}

		public override ZString AMA_VesselName
		{
			get => base.AMA_VesselName;
			set
			{
				var oldValue = AMA_VesselName;
				base.AMA_VesselName = value;
				if (!IsCopying && oldValue != AMA_VesselName)
				{
					UpdateLloydsNumberRadioCallSign();
					UpdateSourceConsolVesselIfRequired();
				}
				AMA_VesselNameInfo.RefreshBinding(oldValue);
			}
		}

		void UpdateLloydsNumberRadioCallSign()
		{
			var vessel = Vessel ?? new RefVessel.Loader(Factory).LoadUnique(AMA_VesselName, ZString.Empty, ZString.Empty, ZString.Empty);
			AMA_LloydsNumber = vessel != null ? vessel.RV_LloydsNumber : ZString.Empty;
			AMA_RadioCallSign = vessel != null ? vessel.RV_RadioCallSign : ZString.Empty;
		}

		void UpdateSourceConsolVesselIfRequired()
		{
			if (Consol != null && Consol.IsSea && AMA_OverrideFreightDefaults)
			{
				Consol.Transports[0].JW_Vessel = AMA_VesselName;
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.VesselRadioCallSigns))]
		[MaxLength(RefVessel.Schema.RV_RadioCallSignMaxLength)]
		[ResourceStringData("ZAAsycudaManifestHeader.AMA_RadioCallSign", Caption = "Radio Call Sign", ShortCaption = "Call Sign")]
		public override ZString AMA_RadioCallSign
		{
			get => base.AMA_RadioCallSign;
			set => base.AMA_RadioCallSign = value;
		}

		#region CaseNumbers

		[ChildEditable(true)]
		public CaseNumberCollection CaseNumbers
		{
			get
			{
				if (casenumbers == null)
				{
					casenumbers = new CaseNumberCollection(this);
					casenumbers.Load();
					RegisterEditableChildObject(casenumbers);
				}
				return casenumbers;
			}
		}
		CaseNumberCollection casenumbers;

		BusinessObject ICaseNumberCollectionProvider.Master => this;

		public ZBool CaseNumbersVisible => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.ZAManifestCaseNumbers
				, Core.Constants.CountryCodes.SouthAfrica
				, ZDateTime.Today) && !IsBillLevelManifestType;

		#endregion

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = base.SupportedCusCodeDataTypes;
				result.Add(CusCodeDataTypeList.Codes.CaseNumber, typeof(CaseNumber));
				return result;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		public virtual Customs.Business.SupportingDocSendingObject GetSupportingDocSendingObject()
		{
			return new SupportingDocSendingObject(this);
		}

		public ZString Reference => this.AMA_JobReference;

		public ZString CountryCode => this.Country.Code;

		public ZString AgentDualProfileCode => null;

		public ZString TradingPartyID => null;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaManifestHeaderFetchStrategy(this);
		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source)
			=> new AsycudaManifestHeaderSynchroniser(this, source);

		#region Update Bill UCR and LRNs

		public ZString RefreshMainfestBillUcrAndLrns()
		{
			var message = ZString.Empty;

			foreach (AsycudaBill bill in Bills)
			{
				foreach (ForwardingShipment shipment in Consol?.Shipments)
				{
					var declaration = shipment?.Declarations.OfType<JobDeclaration>().FirstOrDefault();
					if (declaration != null)
					{
						var masterOrHouseNumber = declaration.JE_HouseBill.IsEmpty ? declaration.JE_MasterBill : declaration.JE_HouseBill;
						if (!masterOrHouseNumber.IsEmpty && masterOrHouseNumber == bill.ABL_BillNumber)
						{
							message = message + RefreshUCRFromDeclarationEntryInstructions(bill, declaration);
							message = message + RefreshLRNsFromDeclarationEntries(bill, declaration);
						}
					}
				}
			}

			if (message == ZString.Empty)
			{
				message = NoBillsUpdated;
			}
			return message;
		}

		static string NoBillsUpdated => string.Format(CultureInfo.CurrentCulture, Res.GetString("6BD5C43F-F2D2-4560-A43C-14AE49CDD687", "No Bill data has been updated as there were no matches."));

		ZString RefreshUCRFromDeclarationEntryInstructions(AsycudaBill bill, JobDeclaration declaration)
		{
			var message = ZString.Empty;
			var instruction = declaration.CustomsEntryInstructions?.OfType<ZA.Business.CusEntryInstruction>()?.OrderBy(x => x.AssessmentDate).FirstOrDefault(x => x.CEI_UCROverride != ZString.Empty);
			if (instruction != null)
			{
				message = message + string.Format(CultureInfo.CurrentCulture, Res.GetString("EA6B316D-34FF-4D91-B435-5EC0B9EC93D31", "Bill '{0}': UCR has been updated from '{1}' to '{2}'.\n", bill.ABL_BillNumber, bill.ABL_UCRNumber, instruction.CEI_UCROverride));
				bill.ABL_UCRNumber = instruction.CEI_UCROverride;
			}
			return message;
		}

		ZString RefreshLRNsFromDeclarationEntries(AsycudaBill bill, JobDeclaration declaration)
		{
			var message = ZString.Empty;
			if (SupportMultipleCustomsNumbers)
			{
				var entries = declaration.ActiveEntryHeaders?.OfType<CusEntryHeader>()?.Where(x => x.CH_BGMReference != ZString.Empty);
				if (entries != null && entries.Any())
				{
					foreach (var entry in entries)
					{
						var existingEntryNo = bill.CustomsEntryNumbers.OfType<ABLEntryNum>().FirstOrDefault(x => x.CE_EntryNum == entry.CH_BGMReference && (x.CE_EntryType == ZaLRNTypes.Codes.ABT || x.CE_EntryType == ZaLRNTypes.Codes.AFM) && x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica);
						if (existingEntryNo == null)
						{
							var newEntryNo = bill.CustomsEntryNumbers.AddNew();
							newEntryNo.CE_EntryNum = entry.CH_BGMReference;
							message += string.Format(CultureInfo.CurrentCulture, Res.GetString("D0599092-2588-46C6-B1AD-9FAC34EA6863", "Bill '{0}': LRN '{1}' has been added.\n", bill.ABL_BillNumber, entry.CH_BGMReference));
						}
					}
				}
			}
			return message;
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = ZaManifestTypes.Codes.HAB;
		}
#endif
	}
}
