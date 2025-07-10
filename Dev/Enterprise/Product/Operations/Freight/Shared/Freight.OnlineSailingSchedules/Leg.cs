using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class Leg : NonPersistentBusinessObject
	{
		public Leg(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, "factory");
		}

		#region Schema

		public static class Schema
		{
			public const string OriginPortUnloco = "OriginPortUnloco";
			public const string OriginPortName = "OriginPortName";
			public const string DestinationPortUnloco = "DestinationPortUnloco";
			public const string DestinationPortName = "DestinationPortName";
			public const string Departure = "Departure";
			public const string Arrival = "Arrival";
			public const string TradeLaneName = "TradeLaneName";
			public const string VoyageCode = "VoyageCode";
			public const string VesselName = "VesselName";
			public const string LloydsNumber = "LloydsNumber";
			public const string CarrierSCAC = "CarrierSCAC";
			public const string CarrierCode = "CarrierCode";
			public const string LegType = "LegType";
			public const string DepartureReference = "DepartureReference";
			public const string DepartureReferenceProvider = "DepartureReferenceProvider";
			public const string Co2eKgPerTeu = "Co2eKgPerTeu";
			public const string Co2eKgPerTonne = "Co2eKgPerTonne";
		}

		#endregion

		#region Properties

		#region OriginPort

		[ReadOnly(true)]
		[List("PortList")]
		public ZString OriginPortUnloco
		{
			get { return originPortUnloco; }
			set
			{
				SetNonPersistentPropertyValue(OriginPortUnlocoInfo, ref originPortUnloco, value);
				Validation.ValidateOriginPortUnloco();
			}
		}
		ZString originPortUnloco;

		public ZPropertyInfo OriginPortUnlocoInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.OriginPortUnloco); }
		}

		public ZString OriginPortName
		{
			get { return OriginPort == null ? ZString.Empty : OriginPort.RL_PortName; }
		}

		#endregion

		#region DestinationPort

		[ReadOnly(true)]
		[List("PortList")]
		public ZString DestinationPortUnloco
		{
			get { return destinationPortUnloco; }
			set
			{
				SetNonPersistentPropertyValue(DestinationPortUnlocoInfo, ref destinationPortUnloco, value);
				Validation.ValidateDestinationPortUnloco();
			}
		}
		ZString destinationPortUnloco;

		public ZPropertyInfo DestinationPortUnlocoInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.DestinationPortUnloco); }
		}

		public ZString DestinationPortName
		{
			get { return DestinationPort == null ? ZString.Empty : DestinationPort.RL_PortName; }
		}

		#endregion

		#region Departure

		[ReadOnly(true)]
		public ZDateTime Departure
		{
			get { return departure; }
			set
			{
				SetNonPersistentPropertyValue(DepartureInfo, ref departure, value);
				Validation.ValidateDeparture();
			}
		}
		ZDateTime departure;

		public ZPropertyInfo DepartureInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.Departure); }
		}

		#endregion

		#region Arrival

		[ReadOnly(true)]
		public ZDateTime Arrival
		{
			get { return arrival; }
			set
			{
				SetNonPersistentPropertyValue(ArrivalInfo, ref arrival, value);
				Validation.ValidateArrival();
			}
		}
		ZDateTime arrival;

		public ZPropertyInfo ArrivalInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.Arrival); }
		}

		#endregion

		#region TradeLaneName

		[ReadOnly(true)]
		public ZString TradeLaneName
		{
			get { return tradeLaneName; }
			set { SetNonPersistentPropertyValue(TradeLaneNameInfo, ref tradeLaneName, value); }
		}
		ZString tradeLaneName;

		public ZPropertyInfo TradeLaneNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.TradeLaneName); }
		}

		#endregion

		#region VoyageCode

		[ReadOnly(true)]
		public ZString VoyageCode
		{
			get { return voyageCode; }
			set
			{
				SetNonPersistentPropertyValue(VoyageCodeInfo, ref voyageCode, value);
				Validation.ValidateVoyageCode();
			}
		}
		ZString voyageCode;

		public ZPropertyInfo VoyageCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.VoyageCode); }
		}

		#endregion

		#region VesselName

		[ReadOnly(true)]
		public ZString VesselName
		{
			get { return vesselName; }
			set
			{
				SetNonPersistentPropertyValue(VesselNameInfo, ref vesselName, value);
				Validation.ValidateVesselName();
				Validation.ValidateLloydsNumber();
			}
		}
		ZString vesselName;

		public ZPropertyInfo VesselNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.VesselName); }
		}

		#endregion

		#region LloydsNumber

		[ReadOnly(true)]
		public ZString LloydsNumber
		{
			get { return lloydsNumber; }
			set
			{
				SetNonPersistentPropertyValue(LloydsNumberInfo, ref lloydsNumber, value);
				Validation.ValidateLloydsNumber();
				Validation.ValidateVesselName();
			}
		}
		ZString lloydsNumber;

		public ZPropertyInfo LloydsNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.LloydsNumber); }
		}

		#endregion

		#region CarrierSCAC

		[ReadOnly(true)]
		public ZString CarrierSCAC
		{
			get { return carrierSCAC; }
			set
			{
				SetNonPersistentPropertyValue(CarrierSCACInfo, ref carrierSCAC, value);
				Validation.ValidateCarrierSCAC();
			}
		}
		ZString carrierSCAC;

		public ZPropertyInfo CarrierSCACInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.CarrierSCAC); }
		}

		#endregion

		#region OperatorName

		public ZString OperatorName
		{
			get; private set;
		}

		#endregion

		#region CarrierCode

		[ReadOnly(true)]
		public ZString CarrierCode
		{
			get { return Carrier == null ? ZString.Empty : Carrier.OH_Code; }
		}

		#endregion

		#region Carrier

		public OrgHeader Carrier
		{
			get
			{
				var orgCusCodes = CarrierSCACHelper.GetExistingOrgCustCodes(carrierSCAC, Factory);
				return orgCusCodes.Length == 1 ? orgCusCodes[0].Header : null;
			}
		}

		public bool ScacCodeCanBeAssginedToCarrier => !CarrierSCACHelper.GetExistingOrgCustCodes(carrierSCAC, Factory).Any();

		#endregion

		#region LegType

		[ReadOnly(true)]
		public ZString LegType
		{
			get { return legType; }
			set { SetNonPersistentPropertyValue(LegTypeInfo, ref legType, value); }
		}
		ZString legType;

		public ZPropertyInfo LegTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.LegType); }
		}

		#endregion

		#region IsSea

		public bool IsSea => LegType == GssConstants.SeaLegType;

		#endregion

		#region DepartureReference

		[ReadOnly(true)]
		public ZString DepartureReference
		{
			get { return departureReference; }
			set { SetNonPersistentPropertyValue(DepartureReferenceInfo, ref departureReference, value); }
		}
		ZString departureReference;

		public ZPropertyInfo DepartureReferenceInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.DepartureReference); }
		}

		#endregion

		#region DepartureReferenceProvider

		[ReadOnly(true)]
		public ZString DepartureReferenceProvider
		{
			get { return departureReferenceProvider; }
			set { SetNonPersistentPropertyValue(DepartureReferenceProviderInfo, ref departureReferenceProvider, value); }
		}
		ZString departureReferenceProvider;

		public ZPropertyInfo DepartureReferenceProviderInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.DepartureReferenceProvider); }
		}

		#endregion

		#region Co2eKgPerTeu

		[ReadOnly(true)]
		[DecimalPlaces(3)]
		public ZDecimal Co2eKgPerTeu
		{
			get { return co2eKgPerTeu; }
			set { SetNonPersistentPropertyValue(Co2eKgPerTeuInfo, ref co2eKgPerTeu, value); }
		}
		ZDecimal co2eKgPerTeu;

		public ZPropertyInfo Co2eKgPerTeuInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.Co2eKgPerTeu); }
		}

		#endregion

		#region Co2eKgPerTonne

		[ReadOnly(true)]
		[DecimalPlaces(3)]
		public ZDecimal Co2eKgPerTonne
		{
				get { return co2eKgPerTonne; }
				set { SetNonPersistentPropertyValue(Co2eKgPerTonneInfo, ref co2eKgPerTonne, value); }
		}
		ZDecimal co2eKgPerTonne;

		public ZPropertyInfo Co2eKgPerTonneInfo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return GetZPropertyInfo(Schema.Co2eKgPerTonne); }
		}

		#endregion

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		public RefVessel Vessel
		{
			get
			{
				if (!LloydsNumber.IsEmpty)
				{
					var vessels = Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNumber));

					if (vessels.Length == 1)
					{
						return vessels[0];
					}

					if (vessels.Length > 1)
					{
						return null;
					}
				}

				return RefVessel.LookupVesselByName(VesselName, Factory).FirstOrDefault();
			}
		}

		public RefUNLOCO OriginPort
		{
			get
			{
				return string.IsNullOrEmpty(OriginPortUnloco)
					? null
					: Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, OriginPortUnloco)).FirstOrDefault();
			}
		}

		public RefUNLOCO DestinationPort
		{
			get
			{
				return string.IsNullOrEmpty(DestinationPortUnloco)
					? null
					: Factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, DestinationPortUnloco)).FirstOrDefault();
			}
		}

		protected RefUNLOCOCollection PortList
		{
			get { return portList ?? (portList = new RefUNLOCOCollection(Factory)); }
		}
		RefUNLOCOCollection portList;

		public LegValidation Validation => new LegValidation(this);

		public void SetValues(ServiceModel.Leg leg)
		{
			Argument.NotNull(leg, "leg");
			Argument.NotNull(leg.LoadPort, "Leg.LoadPort");
			Argument.NotNull(leg.DischargePort, "Leg.DischargePort");

			if (leg.LegType == GssConstants.SeaLegType)
			{
				Argument.NotNull(leg.Voyage, "Leg.Voyage");
				Argument.NotNull(leg.Voyage.Vessel, "Leg.Voyage.Vessel");
			}

			LegType = leg.LegType;
			OriginPortUnloco = leg.LoadPort.Unloco;
			Departure = leg.Etd ?? ZDateTime.Empty;
			DestinationPortUnloco = leg.DischargePort.Unloco;
			Arrival = leg.Eta ?? ZDateTime.Empty;
			TradeLaneName = leg.Voyage?.TradeLane?.Name;
			VoyageCode = leg.Voyage?.Code;
			VesselName = leg.Voyage?.Vessel?.VesselName;
			LloydsNumber = leg.Voyage?.Vessel?.ImoNumber;
			CarrierSCAC = leg.Voyage?.Operator?.Code;
			OperatorName = leg.Voyage?.Operator?.Name;
			DepartureReference = leg.DepartureReference;
			DepartureReferenceProvider = leg.DepartureReferenceProvider;
			Co2eKgPerTeu = leg.Co2eKgPerTeu ?? ZDecimal.Zero;
			Co2eKgPerTonne = leg.Co2eKgPerTonne ?? ZDecimal.Zero;
		}

		public RefVessel ExistingVesselWithSameVesselNameButDifferentImo
		{
			get
			{
				if (IsVesselVoyageInfoValid)
				{
					var matchedVessel = RefVessel.LookupVesselByName(VesselName, Factory).FirstOrDefault();

					if (matchedVessel != null)
					{
						var imoNumber = matchedVessel.RV_LloydsNumber;

						if (!imoNumber.IsEmpty && imoNumber != "0" && !imoNumber.EqualsIgnoringCase(LloydsNumber))
						{
							return matchedVessel;
						}
					}
				}

				return null;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design")]
		public bool UpdateVesselImo(RefVessel existingVesselImoWithSameNameButDifferentImo, out string resultMessage)
		{
			var savedImoNumber = existingVesselImoWithSameNameButDifferentImo.RV_LloydsNumber;
			var matchedVessel = RefVessel.LookupVesselByName(VesselName, Factory).FirstOrDefault();

			if (matchedVessel != null && !matchedVessel.RV_LloydsNumber.EqualsIgnoringCase(LloydsNumber) && matchedVessel.RV_LloydsNumber.EqualsIgnoringCase(savedImoNumber))
			{
				var query = new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNumber);
				var vessels = Factory.Load<RefVessel>(query);

				if (vessels.Length >= 1)
				{
					resultMessage = Res.GetString("A600206E-436A-4F38-83AE-C2065160D813", "This IMO number can not be updated because it belongs to existing vessel.");
					return false;
				}

				resultMessage = Res.GetString("3BDA6D38-0BB6-4504-BFBB-17507A438BD5", "The IMO {0} of existing vessel with name {1} has been substituted with IMO {2}.", matchedVessel.RV_LloydsNumber, VesselName, LloydsNumber);
				matchedVessel.RV_LloydsNumber = LloydsNumber;
				ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);
				return true;
			}
			else
			{
				resultMessage = DatabaseVesselDataHasChangedError;
				return false;
			}
		}

		public RefVessel ExistingVesselWithSameImoButDifferentVesselName
		{
			get
			{
				var vessels = GetExistingVesselsByImo();

				if (vessels != null && vessels.Length == 1 && !vessels[0].RV_Name.EqualsIgnoringCase(VesselName))
				{
					return vessels[0];
				}

				return null;
			}
		}

		public RefVessel[] GetExistingVesselsByImo()
		{
			if (IsVesselVoyageInfoValid)
			{
				var query = new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNumber);
				return Factory.Load<RefVessel>(query);
			}

			return null;
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design")]
		public bool CreateNewVessel(RefVessel existingVesselWithSameImoButDifferentVesselName, out string resultMessage)
		{
			var savedVesselName = existingVesselWithSameImoButDifferentVesselName.RV_Name;
			var query = new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNumber);
			var vessels = Factory.Load<RefVessel>(query);

			if (vessels.Length > 1)
			{
				resultMessage = Res.GetString("01E1E2A2-AEA2-46C4-B0EF-630B81672711", "Vessel with name {0} can’t be created/activated because more than one vessel found by IMO {1}", VesselName, LloydsNumber);
				return false;
			}

			if (vessels.Length == 0 || vessels[0].RV_Name.EqualsIgnoringCase(VesselName) || !vessels[0].RV_Name.EqualsIgnoringCase(savedVesselName))
			{
				resultMessage = DatabaseVesselDataHasChangedError;
				return false;
			}

			var matchedVessel = RefVessel.LookupVesselByName(VesselName, Factory, true).FirstOrDefault();

			if (matchedVessel != null && matchedVessel.RV_IsActive)
			{
				resultMessage = Res.GetString("7ABCA253-6580-4979-8497-607370E7F2CC", "Vessel with name {0} already exists and it is active.", matchedVessel.RV_Name);
				return false;
			}

			var vessel = vessels[0];
			vessel.RV_IsActive = false;

			if (matchedVessel != null)
			{
				matchedVessel.RV_IsActive = true;
				matchedVessel.RV_LloydsNumber = LloydsNumber;
				resultMessage = Res.GetString("0F567C4E-2FF5-413D-BB79-30C617F0D446", "The existing vessel with name {0} has been deactivated, and the vessel with name {1} has been activated.", vessel.RV_Name, matchedVessel.RV_Name);
			}
			else
			{
				var newVessel = Factory.New<RefVessel>();
				newVessel.RV_Name = VesselName;
				newVessel.RV_LloydsNumber = LloydsNumber;
				newVessel.RV_IsActive = true;
				resultMessage = Res.GetString("ADBFFF89-BBE7-4B37-98A7-B3FABCD1E71F", "The existing vessel with name {0} has been deactivated, and the vessel with name {1} has been created.", vessel.RV_Name, newVessel.RV_Name);
			}

			ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true);
			return true;
		}

		bool IsVesselVoyageInfoValid
		{
			get
			{
				return !VesselName.IsEmpty && !LloydsNumber.IsEmpty && LloydsNumber.Length == RefVesselSchema.RV_LloydsNumber.MaxLength;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "By design")]
		public bool AssignScacCodeToCarrier(OrgHeader carrier, out string resultMessage)
		{
			if (!ScacCodeCanBeAssginedToCarrier)
			{
				resultMessage = DatabaseCarrierDataHasChangedError;
				return false;
			}

			var orgCusCode = carrier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, CarrierSCAC, Constants.CountryCodes.UnitedStates);
			ZExceptionReporting.ProcessWithSaveExceptionHandling(orgCusCode.Factory.Save, null, true);

			resultMessage = Res.GetString("07D7B8C6-7A3F-491F-8841-87A3D4F47E9B", "The SCAC code {0} has been assigned to the carrier {1}.", CarrierSCAC, carrier.OH_Code);
			return true;
		}

		string DatabaseVesselDataHasChangedError => Res.GetString("3C25CDED-C6B3-4829-8760-7E07AB7BC789", "Another user has already changed vessel you’re trying to update while you were working with schedules.");
		string DatabaseCarrierDataHasChangedError => Res.GetString("6416EFDF-AE8E-445B-9844-7C4AAA09AF3A", "Another user has already changed carrier you’re trying to update while you were working with schedules.");

		public Leg Clone(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			var leg = new Leg(factory);

			using (leg.GetValidationSuspender())
			using (leg.SuspendSettingHasChanges())
			{
				leg.LegType = LegType;
				leg.OriginPortUnloco = OriginPortUnloco;
				leg.DestinationPortUnloco = DestinationPortUnloco;
				leg.Departure = Departure;
				leg.Arrival = Arrival;
				leg.TradeLaneName = TradeLaneName;
				leg.VoyageCode = VoyageCode;
				leg.VesselName = VesselName;
				leg.LloydsNumber = LloydsNumber;
				leg.CarrierSCAC = CarrierSCAC;
				leg.OperatorName = OperatorName;
				leg.DepartureReference = DepartureReference;
				leg.DepartureReferenceProvider = DepartureReferenceProvider;
				leg.Co2eKgPerTeu = Co2eKgPerTeu;
				leg.Co2eKgPerTonne = Co2eKgPerTonne;
			}

			return leg;
		}

		public void CreateEnterpriseVoyage()
		{
			var orgCusCodes = CarrierSCACHelper.GetExistingOrgCustCodes(carrierSCAC, Factory);
			if (orgCusCodes.Length != 1)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Can't match Carrier SCAC {0} to an existing carrier: there are {1} carriers with this SCAC.", CarrierSCAC, orgCusCodes.Length));
			}

			var carrier = orgCusCodes[0].Header;

			var vessel = Vessel;

			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_Name = VesselName;
				vessel.RV_LloydsNumber = LloydsNumber;
				vessel.RV_IsActive = true;

				CreateVoyage(carrier, vessel);
			}
			else
			{
				var lloydsNumberValidation = new LloydsNumberValidation();
				lloydsNumberValidation.Validate(LloydsNumber);
				if ((vessel.RV_LloydsNumber.IsEmpty || vessel.RV_LloydsNumber == "0")
					&& lloydsNumberValidation.IsValid)
				{
					vessel.RV_LloydsNumber = LloydsNumber;
				}

				var voyage = new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Sea, Vessel.RV_FK, VoyageCode, carrier.PK);
				if (voyage == null)
				{
					CreateVoyage(carrier, vessel);
				}
				else
				{
					CreateOrUpdateVoyageDetails(voyage);
				}
			}
		}

		void CreateVoyage(OrgHeader carrier, RefVessel vessel)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = VoyageCode;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = carrier.PK;
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_IsActive = true;

			CreateOrUpdateVoyageDetails(voyage);
		}

		void CreateOrUpdateVoyageDetails(JobVoyage voyage)
		{
			CreateOrUpdateVoyageOriginDestination(voyage);

			if (FindMatchingJobSailing() is JobSailing sailing)
			{
				sailing.JX_ServiceString = TradeLaneName;
			}
		}

		void CreateOrUpdateVoyageOriginDestination(JobVoyage voyage)
		{
			var origin = voyage.Origins.GetOriginFromLoading(OriginPortUnloco);

			if (origin == null)
			{
				origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = OriginPortUnloco;
			}

			origin.JA_DepartReference = DepartureReference;

			var destination = voyage.Destinations.GetDestinationFromDischarge(DestinationPortUnloco);

			if (destination == null)
			{
				destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = DestinationPortUnloco;
			}

			origin.JA_E_DEP = Departure;
			destination.JB_E_ARV = Arrival;
			destination.TryMatchFirstArrivalAndLastForeignPorts();
		}

		public JobSailing FindMatchingJobSailing(bool allowMatchOnVesselWithSameIMOAndDifferentVesselName = false)
		{
			var carrier = Carrier;

			if (carrier != null)
			{
				var voyage = new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Sea, VesselName, VoyageCode, carrier.PK);
				if (voyage == null && Vessel != null && allowMatchOnVesselWithSameIMOAndDifferentVesselName)
				{
					voyage = new JobVoyage.Loader(Factory).Load(Constants.TransportModes.Sea, Vessel.RV_FK, VoyageCode, carrier.PK);
				}
				if (voyage != null)
				{
					return voyage.Sailings.GetSailingFromLoadAndDischarge(OriginPortUnloco, DestinationPortUnloco);
				}
			}

			return null;
		}
	}
}
