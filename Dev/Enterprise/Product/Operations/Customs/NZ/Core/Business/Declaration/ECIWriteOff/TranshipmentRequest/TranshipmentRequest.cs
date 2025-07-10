using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusHAWB = Enterprise.Customs.NZ.Business.Express.CusHAWB;
using CusMAWB = Enterprise.Customs.NZ.Business.Express.CusMAWB;

namespace Enterprise.Customs.NZ.Business
{
	public class TranshipmentRequest : CusUnderbond, Integration.Customs.NZ.ICusUnderbond, ITranshipmentDetails, ICusStorageDocPivotParent
	{
		public TranshipmentRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (IsCancelled)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		public static TranshipmentRequest Load(ITranshipmentRequestParent parent)
		{
			TranshipmentRequest result = null;
			if (parent != null)
			{
				var query = new ZQuery(CusUnderbondSchema.C4_ParentID, parent.PK);
				query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
				result = parent.Factory.Load<TranshipmentRequest>(query).OrderBy(x => x.PK).FirstOrDefault();
			}
			return result;
		}

		public static TranshipmentRequest ReLoad(ITranshipmentRequestParent parent)
		{
			TranshipmentRequest result = null;
			if (parent != null)
			{
				var query = new ZQuery(CusUnderbondSchema.C4_ParentID, parent.PK);
				query.ReLoadExistingRows = true;
				result = parent.Factory.Load<TranshipmentRequest>(query).OrderBy(x => x.PK).FirstOrDefault();
			}
			return result;
		}

		public static TranshipmentRequest Create(ITranshipmentRequestParent parent, bool savesOnChanges = false)
		{
			TranshipmentRequest result = null;
			if (parent != null)
			{
				result = parent.Factory.New<TranshipmentRequest>();
				using (result.SuspendSettingHasChanges())
				{
					result.C4_ParentID = parent.PK;
					result.C4_ParentTableCode = parent.TablePrefix;
					result.C4_MovementReason = DefaultMovementReason(parent);
				}
				result.fSavesOnChanges = savesOnChanges;
			}
			return result;
		}

		public static string DefaultMovementReason(ITranshipmentRequestParent parent)
		{
			var movementReason = "";
			if (parent.TablePrefix == JobDeclarationSchema.Constants.Prefix || parent.IsExport)
			{
				movementReason = MovementReason.Codes.InternationalTranshipmentRequest;
			}
			else
			{
				var hawb = parent as CusHAWB;
				if (hawb != null)
				{
					movementReason = hawb.CS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase) ? MovementReason.Codes.DomesticTranshipmentRequest : MovementReason.Codes.InternationalTranshipmentRequest;
				}
				else
				{
					var houseBill = parent as CusSCAHouse;
					if (houseBill != null)
					{
						movementReason = houseBill.CA_RL_NK_PortOfDestination.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase) ? MovementReason.Codes.DomesticTranshipmentRequest : MovementReason.Codes.InternationalTranshipmentRequest;
					}
				}
			}

			return movementReason;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			C4_MovementReason = MovementReason.Codes.InternationalTranshipmentRequest;
			C4_ApplicationCode = Enterprise.Customs.Business.CusUnderbondApplicationCodeList.Codes.NZTranshipmentRequest;
		}

		public override bool CanDoUBM => false;

		[List(nameof(Lookups) + "." + nameof(TranshipmentRequestLookups.MovementReasonList))]
		[ReadOnlyMember(nameof(MovementReason_ReadOnly))]
		public override ZString C4_MovementReason
		{
			get => base.C4_MovementReason;
			set => base.C4_MovementReason = value;
		}

		public virtual bool MovementReason_ReadOnly
		{
			get
			{
				var result = false;
				if (Parent != null)
				{
					result = Parent.IsExport || ParentIsDeclaration;
				}

				return result;
			}
		}

		[ResourceStringData("F8A326A1-0F24-4F30-AB9C-8F323EE942D5", Caption = "Transfer Transport Mode")]
		[List(nameof(Lookups) + "." + nameof(TranshipmentRequestLookups.ModeOfMovement))]
		public override ZString C4_ModeOfMovement
		{
			get => base.C4_ModeOfMovement;
			set
			{
				var originalValue = C4_ModeOfMovement;
				base.C4_ModeOfMovement = value;
				if (!IsCopying && C4_ModeOfMovement != originalValue)
				{
					DefaultFromRoutingIfNeeded();
				}
			}
		}

		[ResourceStringData("B22641FB-2CEA-45A6-960B-B3C1527FCBAB", Caption = "Incoming Transport Mode")]
		[List(nameof(Lookups) + "." + nameof(TranshipmentRequestLookups.TranshipmentModeOfMovement))]
		public override ZString C4_TranshipModeOfMovement
		{
			get => base.C4_TranshipModeOfMovement;
			set
			{
				var originalValue = C4_TranshipModeOfMovement;
				base.C4_TranshipModeOfMovement = value;
				if (!IsCopying && C4_TranshipModeOfMovement != originalValue)
				{
					ClearIrrelevantData();
				}
			}
		}

		#region TranshipBySea

		public void SetTranshipBySeaVessel(RefVessel vessel)
		{
			C4_TranshipBySeaVessel = vessel.RV_Code;
			C4_TranshipBySeaLloydsIMONum = vessel.RV_LloydsNumber;
		}

		[ResourceStringData("A84E235E-2C51-4203-A876-16CCBD7EC2A9", Caption = "Vessel")]
		[List(nameof(Lookups) + "." + nameof(TranshipmentRequestLookups.VesselList))]
		public override ZString C4_TranshipBySeaVessel
		{
			get { return base.C4_TranshipBySeaVessel; }
			set
			{
				base.C4_TranshipBySeaVessel = value;
				UpdateTranshipBySeaLloydsIMONumFromVessel();
			}
		}

		[ResourceStringData("575A7DDB-A0E1-4C80-AB64-66055548346A", Caption = "Lloyds IMO")]
		public override ZString C4_TranshipBySeaLloydsIMONum
		{
			get { return base.C4_TranshipBySeaLloydsIMONum; }
			set
			{
				base.C4_TranshipBySeaLloydsIMONum = value;
				UpdateTranshipBySeaVesselFromLloyds();
			}
		}

		void UpdateTranshipBySeaLloydsIMONumFromVessel()
		{
			if (C4_TranshipBySeaLloydsIMONum.IsEmpty && !C4_TranshipBySeaVessel.IsEmpty)
			{
				var vessels = RefVessel.LookupVesselByName(C4_TranshipBySeaVessel, Factory);
				if (vessels.Length == 1)
				{
					base.C4_TranshipBySeaLloydsIMONum = vessels.First().RV_LloydsNumber;
				}
			}
		}

		void UpdateTranshipBySeaVesselFromLloyds()
		{
			if (C4_TranshipBySeaVessel.IsEmpty && !C4_TranshipBySeaLloydsIMONum.IsEmpty)
			{
				var vesselFromLloyds = RefVessel.LookupVesselByLloyds(C4_TranshipBySeaLloydsIMONum, Factory);
				if (vesselFromLloyds != null)
				{
					base.C4_TranshipBySeaVessel = vesselFromLloyds.RV_Code;
				}
			}
		}

		#endregion

		[ResourceStringData("931C8D1F-EDFF-4F25-A8A3-F94E364ABECD", Caption = "Voyage")]
		public override ZString C4_TranshipBySeaVoyage { get => base.C4_TranshipBySeaVoyage; set => base.C4_TranshipBySeaVoyage = value; }

		[ResourceStringData("F367E107-39C0-4872-87F9-03071DA37C47", Caption = "Flight No")]
		public override ZString C4_FlightNo { get => base.C4_FlightNo; set => base.C4_FlightNo = value; }

		[ResourceStringData("82B2A9A9-E1FD-4019-BB3D-D0308E18F472", Caption = "Departure Date")]
		public override ZDateTime C4_TranshipDepartureDate { get => base.C4_TranshipDepartureDate; set => base.C4_TranshipDepartureDate = value; }

		[ResourceStringData("BF3A266A-789A-4A65-9935-C98D32AC19A1", Caption = "Premise")]
		[List(nameof(Lookups) + "." + nameof(TranshipmentRequestLookups.TransitDestinationList))]
		public override ZGuid C4_OA_DestinationAddress { get => base.C4_OA_DestinationAddress; set => base.C4_OA_DestinationAddress = value; }

		protected override ZAddress GetNewC4_OA_DestinationAddress_ZAddress()
		{
			var address = base.GetNewC4_OA_DestinationAddress_ZAddress();
			address.GetDefaultAddress = header => header?.MainAddress?.PK ?? ZGuid.Empty;
			return address;
		}

		[ReadOnlyMember(nameof(C4_IsMoveFromDischarge))]
		[List(nameof(Lookups) + "." + nameof(TranshipmentRequestLookups.OriginLocationList))]
		public override ZGuid C4_OA_OriginAddress { get => base.C4_OA_OriginAddress; set => base.C4_OA_OriginAddress = value; }

		protected override ZAddress GetNewC4_OA_OriginAddress_ZAddress()
		{
			var address = base.GetNewC4_OA_OriginAddress_ZAddress();
			address.GetDefaultAddress = header => header?.MainAddress?.PK ?? ZGuid.Empty;
			return address;
		}

		[ReadOnly(true)]
		public override ZString C4_Status
		{
			get => base.C4_Status;
			set
			{
				var oldValue = base.C4_Status;
				if (value != oldValue)
				{
					base.C4_Status = value;
					if (!IsCopying && IsCancelled)
					{
						SetReadOnlyIncludingChildren(true);
					}
				}
			}
		}

		[ResourceStringData("29A3701A-4ECA-47A9-B0D9-BC7A2583C7D3", Caption = "Transit Dest. Port")]
		public override ZString C4_RL_NKTranshipDestPort { get => base.C4_RL_NKTranshipDestPort; set => base.C4_RL_NKTranshipDestPort = value; }

		[ResourceStringData("66FB6EF9-A66C-4B4A-97AC-9A8004E7A385", Caption = "Status")]
		public ZString MovementStatusDesc
		{
			get { return C4_Status.IsEmpty ? string.Empty : Lookups.CombinedMovementStatusList.GetDescriptionFromCode(C4_Status); }
		}

		[ReadOnlyMember(nameof(MoveFromPort_ReadOnly))]
		public override ZBool C4_IsMoveFromDischarge { get => base.C4_IsMoveFromDischarge; set => base.C4_IsMoveFromDischarge = value; }

		public ZBool MoveFromPort_ReadOnly => !C4_OA_OriginAddress.IsEmpty;

		public bool IsITR => C4_MovementReason == MovementReason.Codes.InternationalTranshipmentRequest;
		public bool IsDTR => C4_MovementReason == MovementReason.Codes.DomesticTranshipmentRequest;

		public bool IsCancelled => C4_Status == CombinedMovementStatus.Codes.CAN;

		public bool IsATranshipment => IsITR || IsDTR;

		public bool ParentIsDeclaration => Parent?.TablePrefix == JobDeclarationSchema.Constants.Prefix;

		public virtual bool IsSeaJob => Factory.GetValue(ref fIsSeaJob, delegate
		{
			return (Parent is CusSCAHouse) || (Parent is CusSCAContainer) || ((Parent is JobDeclaration) && (Parent as JobDeclaration).IsSea);
		});

		CachedProperty<bool> fIsSeaJob;

		public virtual bool IsAirJob => Factory.GetValue(ref fIsAirJob, delegate
		{
			return (Parent is CusMAWB) || (Parent is CusHAWB) || ((Parent is JobDeclaration) && (Parent as JobDeclaration).IsAir);
		});

		CachedProperty<bool> fIsAirJob;

		public ZString TransitPremiseCode
		{
			get
			{
				var transitPremiseCode = ZString.Empty;
				if (C4_OA_DestinationAddress.IsValid)
				{
					transitPremiseCode = DestinationAddress?.GetResponsiblePartyPremiseID() ?? ZString.Empty;
				}

				if (transitPremiseCode.IsEmpty)
				{
					transitPremiseCode = C4_RL_NKTranshipDestPort;
				}

				return transitPremiseCode;
			}
		}

		[ReadOnly(true)]
		public CusEntryNumber TSWEntryNumber
		{
			get
			{
				if (tSWEntryNumber == null || tSWEntryNumber.IsDeleted || tSWEntryNumber.CE_EntryType != Common.NZ.CusEntryNumberTypeList.Codes.DomesticTranshipmentRequest)
				{
					tSWEntryNumber = CusEntryNumber.Load(this, Common.NZ.CusEntryNumberTypeList.Codes.DomesticTranshipmentRequest, Core.Constants.CountryCodes.NewZealand);
					if (tSWEntryNumber != null)
					{
						RegisterEditableChildObject(tSWEntryNumber);
					}
				}
				return tSWEntryNumber;
			}
		}
		public CusEntryNumber tSWEntryNumber;

		public ZString TSWNumber => TSWEntryNumber?.CE_EntryNum ?? null;

		public ZPropertyInfo TSWNumberInfo => GetZPropertyInfo(nameof(TSWNumber));

		#region Lookups

		protected override CusUnderbondLookups GetNewLookups() => new TranshipmentRequestLookups(this);

		public new TranshipmentRequestLookups Lookups => (TranshipmentRequestLookups)base.Lookups;

		#endregion

		void DefaultFromRoutingIfNeeded()
		{
			if (ShouldDefaultFromRouting)
			{
				var matchingTransport = MatchingTransport;
				if (matchingTransport != null)
				{
					var parent = Parent;
					var transportMode = matchingTransport.JW_TransportMode;
					if (transportMode == Core.Constants.TransportModes.Air)
					{
						C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
						C4_FlightNo = matchingTransport.JW_VoyageFlight;
					}
					else if (transportMode == Core.Constants.TransportModes.Sea)
					{
						C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
						C4_TranshipBySeaVessel = matchingTransport.JW_Vessel;
						C4_TranshipBySeaVoyage = matchingTransport.JW_VoyageFlight;
					}

					if (parent.IsTSWCREWriteOff)
					{
						var ata = matchingTransport.JW_ATA;
						C4_ArrivalDate = ata.IsEmpty ? matchingTransport.JW_ETA : ata;
					}
					else
					{
						var atd = matchingTransport.JW_ATD;
						C4_TranshipDepartureDate = atd.IsEmpty ? matchingTransport.JW_ETD : atd;
					}
				}
			}
		}

		Transport MatchingTransport
		{
			get
			{
				Transport matchingTransport = null;
				var parent = Parent;
				if (parent != null && parent.TransportParent != null)
				{
					var transportLegs = parent.TransportParent.Transports;
					if (parent.IsTSWCREWriteOff)
					{
						matchingTransport = transportLegs.Cast<Transport>()
							.Where(x => x.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase) && !x.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase))
							.OrderBy(x =>
							{
								var ata = x.JW_ATA;
								var eta = x.JW_ETA;
								return (ata < eta) ? ata : eta;
							}).FirstOrDefault();
					}
					else if (parent.IsTSWICRWriteOff)
					{
						matchingTransport = transportLegs.Cast<Transport>()
							.Where(x => x.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase) && !x.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.NewZealand, StringComparison.OrdinalIgnoreCase))
							.OrderBy(x =>
							{
								var atd = x.JW_ATD;
								var etd = x.JW_ETD;
								return (atd < etd) ? atd : etd;
							}).FirstOrDefault();
					}
				}

				return matchingTransport;
			}
		}

		public virtual ITranshipmentRequestParent Parent
		{
			get => (ITranshipmentRequestParent)ParentLoaders.LoadBusinessObject(Factory, C4_ParentTableCode, C4_ParentID);
		}

		protected override TypeLoaderCollection GetParentLoaders()
		{
			var result = new TypeLoaderCollection();
			result.Add(ObjectFactory.GetType<Integration.Customs.NZ.IJobDeclaration>());
			result.Add(ObjectFactory.GetType<Integration.Customs.NZ.ICusHAWB>());
			result.Add(ObjectFactory.GetType<Integration.Customs.NZ.ICusMAWB>());
			result.Add(ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAHouse>());
			result.Add(ObjectFactory.GetType<Integration.Customs.NZ.ICusSCAContainer>());
			return result;
		}

		bool ShouldDefaultFromRouting => Parent != null
										&& C4_TranshipModeOfMovement.IsEmpty
										&& C4_FlightNo.IsEmpty
										&& C4_TranshipBySeaVessel.IsEmpty
										&& C4_TranshipBySeaVoyage.IsEmpty
										&& C4_ArrivalDate.IsEmpty;

		void ClearIrrelevantData()
		{
			var transhipModeOfMovement = C4_TranshipModeOfMovement;
			if (transhipModeOfMovement == TranshipmentRequestModeOfMovement.Codes.Air)
			{
				C4_TranshipBySeaVessel = ZString.Empty;
				C4_TranshipBySeaLloydsIMONum = ZString.Empty;
				C4_TranshipBySeaVoyage = ZString.Empty;
			}
			else if (transhipModeOfMovement == TranshipmentRequestModeOfMovement.Codes.Sea)
			{
				C4_FlightNo = ZString.Empty;
			}
			else
			{
				C4_TranshipBySeaVessel = ZString.Empty;
				C4_TranshipBySeaLloydsIMONum = ZString.Empty;
				C4_TranshipBySeaVoyage = ZString.Empty;
				C4_FlightNo = ZString.Empty;
				C4_ArrivalDate = ZDateTime.Empty;
			}
		}

		protected override CusUnderbondValidation GetNewValidation() => new TranshipmentRequestValidation(this);

		public new TranshipmentRequestValidation Validation => (TranshipmentRequestValidation)base.Validation;

		public override bool IsSavedByFactory => (!fSavesOnChanges || HasChanges) && base.IsSavedByFactory;
		bool fSavesOnChanges;

		ZBool ITranshipmentDetails.InternationalTranshipmentRequest => ((!C4_ModeOfMovement.IsEmpty || !C4_TranshipModeOfMovement.IsEmpty) && C4_MovementReason == MovementReason.Codes.InternationalTranshipmentRequest);

		ZBool ITranshipmentDetails.DomesticTranshipmentRequest => ((!C4_ModeOfMovement.IsEmpty || !C4_TranshipModeOfMovement.IsEmpty) && C4_MovementReason == MovementReason.Codes.DomesticTranshipmentRequest);

		ZString ITranshipmentDetails.ModeOfTransportForTransfer => C4_ModeOfMovement;

		ZString ITranshipmentDetails.ITRImportCraft => C4_TranshipBySeaVessel;

		ZString ITranshipmentDetails.ITRImportMode => C4_TranshipModeOfMovement;

		ZDateTime ITranshipmentDetails.ITRArrivalDate => C4_ArrivalDate;

		ZDateTime ITranshipmentDetails.ITRDepartureDate => C4_TranshipDepartureDate;

		ZString ITranshipmentDetails.ITRVoyageFlight
		{
			get
			{
				var result = ZString.Empty;
				var transhipmentMode = C4_TranshipModeOfMovement;
				if (TranshipmentRequestModeOfMovement.IsAir(transhipmentMode))
				{
					result = C4_FlightNo;
				}
				else if (TranshipmentRequestModeOfMovement.IsSea(transhipmentMode))
				{
					result = C4_TranshipBySeaVoyage;
				}

				return result;
			}
		}

		ZString ITranshipmentDetails.PremiseCode => TransitPremiseCode;

		#region ICusStorageDocPivotParent

		void ICusStorageDocPivotTypeSupporter.ReloadCollection()
		{
			if (IsEDocPivotCollectionLoaded)
			{
				EDocPivotCollection.Reload(true);
			}
		}

		bool IsEDocPivotCollectionLoaded => eDocPivotCollection != null && eDocPivotCollection.IsLoaded;

		[ChildEditable(true)]
		public CusStorageDocPivotCollection EDocPivotCollection
		{
			get
			{
				if (eDocPivotCollection == null)
				{
					eDocPivotCollection = new CusStorageDocPivotCollection(this);
					eDocPivotCollection.Load();
					RegisterEditableChildObject(eDocPivotCollection);
				}

				return eDocPivotCollection;
			}
		}
		CusStorageDocPivotCollection eDocPivotCollection;

		CusStorageDocPivotCollection ICusStorageDocPivotParent.EDocPivotCollection => EDocPivotCollection;

		Type ICusStorageDocPivotTypeSupporter.CusStorageDocPivotType => typeof(CusStorageDocPivot);

		IEnumerable<IStorageDocsBaseCollection> ICusStorageDocPivotTypeSupporter.EDocCollections => Container != null ? EDocsHelper.GetEDocCollections(Container.OceanBill) : Array.Empty<IStorageDocsBaseCollection>();

		CusSCAContainer Container => Parent as CusSCAContainer;

		#endregion
	}
}
