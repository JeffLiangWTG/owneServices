using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Freight.Business
{
	public class TransportValidation : JobConsolTransportValidation
	{
		protected TransportValidation(Transport parent)
			: base(parent)
		{
			if (TransportSupporterWithSchedule != null && TransportSupporterWithSchedule.DontErrorOnMissingDetails)
			{
				dontErrorOnMissingDetails = true;
				notificationType = CargoWise.EntityFramework.NotificationType.Warning;
			}
			else
			{
				notificationType = CargoWise.EntityFramework.NotificationType.Error;
			}
		}

		protected readonly INotificationType notificationType;
		protected readonly bool dontErrorOnMissingDetails;

		#region Static New

		public static TransportValidation New(Transport transport)
		{
			switch (transport.JW_TransportMode)
			{
				case Constants.TransportModes.Air:
					return new TransportAirValidation(transport);
				case Constants.TransportModes.Rail:
					return new TransportRailValidation(transport);
				case Constants.TransportModes.Road:
					return new TransportRoadValidation(transport);
				case Constants.TransportModes.Sea:
					return new TransportSeaValidation(transport);
				case Constants.TransportModes.Storage:
					return new TransportStorageValidation(transport);
				case Constants.TransportModes.InlandWaterwayTransport:
					return new TransportInlandWaterwayValidation(transport);
				default:
					return new TransportValidation(transport);
			}
		}

		#endregion

		#region Check Methods

		protected override void CheckJW_CarrierBookingReference()
		{
			base.CheckJW_CarrierBookingReference();

			if (TransportSupporterWithSchedule != null)
			{
				if (string.IsNullOrWhiteSpace(Parent.JW_CarrierBookingReference) &&
					Parent.JW_Status == Constants.TransportStatus.Confirmed &&
					Parent.Carrier != null &&
					Parent.Carrier.PK != TransportSupporterWithSchedule.ShippingLine)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.JW_CarrierBookingReferenceInfo);
				}
			}
		}

		protected override void CheckJW_IsLinked()
		{
			base.CheckJW_IsLinked();
			ValidateRequiredSecurity(Parent.JW_IsLinkedInfo);

			if (!Parent.JW_IsLinkedInfo.HasErrors() && Parent.JW_IsLinked && Parent.SailingManagerHasSufficientInformation() && Parent.JW_JX.IsEmpty)
			{
				Parent.JW_IsLinkedInfo.AddError(Res.GetString("37a4ee26-db2d-4646-89da-b5598d1d7b6e",
					"Sailings for this Consol could not be linked due to conflicts on the Sailing Schedule for these two ports. Check the Sailing Schedule for this Voyage."));
			}
		}

		protected override void CheckJW_TransportMode()
		{
			base.CheckJW_TransportMode();
			NotifyIfNotEntered(Parent.JW_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JW_TransportModeInfo, Parent.JW_TransportMode_List);
			ValidateRequiredSecurity(Parent.JW_TransportModeInfo);
		}

		protected override void CheckJW_TransportType()
		{
			base.CheckJW_TransportType();
			NotifyIfNotEntered(Parent.JW_TransportTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JW_TransportTypeInfo, Parent.JW_TransportType_List);

			var transportParent = Parent.Parent;
			if (transportParent != null && TransportTypeShouldBeUnique(Parent.JW_TransportType))
			{
				if (OtherParentTransports.Any(t => t.JW_TransportType == Parent.JW_TransportType))
				{
					Parent.JW_TransportTypeInfo.AddError(Res.GetString("ff8fa494-39b3-4bc8-9e2d-ce697625c54e", "Can't have more than one {0} {1}", Parent.JW_TransportType, Parent.JW_TransportTypeInfo.Description));
				}
			}
		}

		protected override void CheckJW_Status()
		{
			base.CheckJW_Status();
			NotifyIfNotEntered(Parent.JW_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JW_StatusInfo, Parent.JW_Status_List);
		}

		protected override void CheckIsDomestic()
		{
			if (Parent.LoadPort != null && Parent.DiscPort != null)
			{
				if (Parent.IsDomestic && Parent.LoadPort.Country != Parent.DiscPort.Country)
				{
					Parent.IsDomesticInfo.AddNotification(notificationType, Res.GetString("d990ff66-f916-4e02-9cd0-82a7f02157d5", "You have marked this transport as domestic but the load and discharge ports are not in the same country/region."));
				}
				else if (!Parent.IsDomestic && Parent.LoadPort.Country == Parent.DiscPort.Country)
				{
					Parent.IsDomesticInfo.AddNotification(notificationType, Res.GetString("49ebdccb-13a5-488a-8efe-50ba35cd0af7", "You have marked this transport as international but the load and discharge ports are for a domestic movement."));
				}
			}

			ValidateJW_RL_NKLoadPort();
			ValidateJW_RL_NKDiscPort();
		}

		protected override void CheckJW_RL_NKLoadPort()
		{
			base.CheckJW_RL_NKLoadPort();
			ListValidation.ErrorIfInvalidCode(Parent.JW_RL_NKLoadPortInfo, Parent.UNLOCOCollection);
			ValidateRequiredSecurity(Parent.JW_RL_NKLoadPortInfo);

			VoyageOrigin origin = (Parent.Sailing == null ? null : Parent.Sailing.Origin);

			if (origin == null || !ProxyValidation(Parent.JW_RL_NKLoadPortInfo, origin.JA_RL_NKPortOfLoadingInfo))
			{
				if (Parent.JW_IsLinked)
				{
					MandatoryValidation.CheckEntered(Parent.JW_RL_NKLoadPortInfo, Res.GetString("485c87b1-d774-4b07-a7de-19649b0d77a2", "Load Port"));
				}
				ListValidation.ErrorIfInvalidCode(Parent.JW_RL_NKLoadPortInfo, Parent.UNLOCOCollection);
			}

			CheckPorts(true);
			ValidateJW_RL_NKDiscPort();

			if (!Parent.JW_RL_NKLoadPortInfo.HasErrors())
			{
				var transportParent = Parent.Parent as ITransportParent;
				if (transportParent != null)
				{
					foreach (Transport siblingTransport in transportParent.Transports)
					{
						var warning = siblingTransport.DestinationSupplyChainSecurityConfiguration.GetWarningForProhibitedRouting(Parent);
						if (!warning.IsEmpty && !Parent.JW_RL_NKLoadPortInfo.HasWarning(warning))
						{
							Parent.JW_RL_NKLoadPortInfo.AddWarning(warning);
						}
					}
				}
			}
		}

		protected override void CheckJW_RL_NKDiscPort()
		{
			base.CheckJW_RL_NKDiscPort();
			ListValidation.ErrorIfInvalidCode(Parent.JW_RL_NKDiscPortInfo, Parent.UNLOCOCollection);
			ValidateRequiredSecurity(Parent.JW_RL_NKDiscPortInfo);

			VoyageDestination destination = Parent.Sailing == null ? null : Parent.Sailing.Destination;

			if (destination == null || !ProxyValidation(Parent.JW_RL_NKDiscPortInfo, destination.JB_RL_NKPortOfDischargeInfo))
			{
				if (Parent.JW_IsLinked)
				{
					MandatoryValidation.CheckEntered(Parent.JW_RL_NKDiscPortInfo, Res.GetString("0b81a26c-e9e2-4ea2-88a9-3ebcbdad2c7a", "Discharge Port"));
				}
				ListValidation.ErrorIfInvalidCode(Parent.JW_RL_NKDiscPortInfo, Parent.UNLOCOCollection);
			}

			CheckPorts(false);
			ValidateJW_RL_NKLoadPort();
		}

		protected override void CheckJW_Vessel()
		{
			base.CheckJW_Vessel();
			ValidateRequiredSecurity(Parent.JW_VesselInfo);
			JobVoyage voyage = Parent.Sailing == null ? null : Parent.Sailing.Voyage;

			if (voyage == null || !ProxyValidation(Parent.JW_VesselInfo, voyage.JV_RV_NKVesselInfo))
			{
				CheckJW_Vessel_WithoutSailing();
			}
		}

		protected virtual void CheckJW_Vessel_WithoutSailing()
		{
		}

		protected override void CheckJW_VoyageFlight()
		{
			base.CheckJW_VoyageFlight();
			ValidateRequiredSecurity(Parent.JW_VoyageFlightInfo);

			if (Parent.Voyage != null)
			{
				ProxyValidation(Parent.JW_VoyageFlightInfo, Parent.Voyage.JV_VoyageFlightInfo);
			}
		}

		protected override void CheckJW_IsCargoOnly()
		{
			base.CheckJW_IsCargoOnly();

			if (Parent.Voyage != null)
			{
				ProxyValidation(Parent.JW_IsCargoOnlyInfo, Parent.Voyage.JV_IsCargoOnlyInfo);
			}
		}

		#region IsOnTemplateRecord

		bool IsOnTemplateRecord =>
			Parent.GetParentSafe() is ITemplateRecordProvider templateRecordProvider
			&& templateRecordProvider.IsTemplateRecord
			&& templateRecordProvider.TemplateRecord != null;

		#endregion

		#region JW_ETD

		protected override void CheckJW_ETD()
		{
			base.CheckJW_ETD();

			if (TransportSupporterWithSchedule?.SupportETD ?? true)
			{
				VoyageOrigin origin = Parent.Sailing == null ? null : Parent.Sailing.Origin;

				if (origin == null || !ProxyValidation(Parent.JW_ETDInfo, origin.JA_E_DEPInfo))
				{
					if (!IsMandatoryETD && notificationType == CargoWise.EntityFramework.NotificationType.Error)
					{
						MandatoryValidation.WarnIfNotEntered(Parent.JW_ETDInfo);
					}

					TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_ETDInfo);
				}

				if (!Parent.JW_ETDInfo.HasErrors() && IsMandatoryETD)
				{
					NotifyIfNotEntered(Parent.JW_ETDInfo);
				}

				CheckETDvsETA();

				if (!Parent.JW_ETDInfo.HasErrors())
				{
					ValidateJW_STD();
				}
			}
		}

		protected bool IsMandatoryETD
		{
			get { return Parent.JW_IsLinked && !ImportExportHelper.IsImport(Parent.JW_RL_NKLoadPort, Parent.JW_RL_NKDiscPort) && !IsOnTemplateRecord; }
		}

		protected virtual void CheckETDvsETA()
		{
			if (!Parent.JW_ETDInfo.HasErrors())
			{
				if (Parent.JW_ETD_UTC.IsValid && Parent.JW_ETA_UTC.IsValid)
				{
					if (Parent.JW_ETD_UTC.Date > Parent.JW_ETA_UTC.Date)
					{
						Parent.JW_ETDInfo.AddNotification(notificationType, Res.GetString("4fb37379-fb42-496c-9f20-b2c1f2b599a5", "ETD UTC cannot be after ETA UTC."));
					}
				}
				else if (Parent.JW_ETD.IsValid && Parent.JW_ETA.IsValid)
				{
					if (Parent.JW_ETD > Parent.JW_ETA)
					{
						Parent.JW_ETDInfo.AddNotification(notificationType, Res.GetString("a153fa48-49f9-404e-a01a-a7bd5097305c", "ETD cannot be after ETA."));
					}
				}
			}
		}

		#endregion

		#region JW_ETA

		protected override void CheckJW_ETA()
		{
			base.CheckJW_ETA();
			VoyageDestination destination = Parent.Sailing == null ? null : Parent.Sailing.Destination;

			if (destination == null || !ProxyValidation(Parent.JW_ETAInfo, destination.JB_E_ARVInfo))
			{
				if (!IsMandatoryETA && notificationType == CargoWise.EntityFramework.NotificationType.Error)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.JW_ETAInfo);
				}

				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_ETAInfo);
			}

			if (!Parent.JW_ETAInfo.HasErrors() && IsMandatoryETA)
			{
				NotifyIfNotEntered(Parent.JW_ETAInfo);
			}

			if (!Parent.JW_ETAInfo.HasErrors() && destination != null)
			{
				ValidateJW_TerminalAvailabilityDate();
			}

			CheckETAvsETD();

			if (!Parent.JW_ETAInfo.HasErrors())
			{
				ValidateJW_STA();
			}
		}

		protected bool IsMandatoryETA
		{
			get { return Parent.JW_IsLinked && ImportExportHelper.IsImport(Parent.JW_RL_NKLoadPort, Parent.JW_RL_NKDiscPort) && !IsOnTemplateRecord; }
		}

		protected virtual void CheckETAvsETD()
		{
			if (!Parent.JW_ETAInfo.HasErrors())
			{
				if (Parent.JW_ETA_UTC.IsValid && Parent.JW_ETD_UTC.IsValid)
				{
					if (Parent.JW_ETA_UTC.Date < Parent.JW_ETD_UTC.Date)
					{
						Parent.JW_ETAInfo.AddNotification(notificationType, Res.GetString("831abb99-c239-4e8a-8498-24c6989ea2e1", "ETA UTC cannot be before ETD UTC."));
					}
				}
				else if (Parent.JW_ETA.IsValid && Parent.JW_ETD.IsValid)
				{
					if (Parent.JW_ETA < Parent.JW_ETD)
					{
						Parent.JW_ETAInfo.AddNotification(notificationType, Res.GetString("2f59ecae-4749-4b55-809c-db7ebf185c60", "ETA cannot be before ETD."));
					}
				}
			}
		}

		protected override void CheckJW_JX_Load_ETA()
		{
			base.CheckJW_JX_Load_ETA();

			ValidateRequiredSecurity(Parent.JW_JX_Load_ETAInfo);
			ValidateProxiedSailingOnlyProperty(Parent.JW_JX_Load_ETAInfo);

			if (Parent.Sailing == null || Parent.Sailing.Origin == null || !ProxyValidation(Parent.JW_JX_Load_ETAInfo, Parent.Sailing.Origin.JA_E_ARVInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_JX_Load_ETAInfo);
			}
		}

		#endregion

		#region JW_ATD

		protected override void CheckJW_ATD()
		{
			base.CheckJW_ATD();
			VoyageOrigin origin = Parent.Sailing == null ? null : Parent.Sailing.Origin;

			if (origin == null || !ProxyValidation(Parent.JW_ATDInfo, origin.JA_A_DEPInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_ATDInfo);
			}

			CheckATDvsATA();

			if (!Parent.JW_RL_NKLoadPort.IsEmpty && !Parent.JW_RL_NKLoadPortInfo.HasErrors() && Parent.LoadPort != null)
			{
				CheckRelatedPortLocalTimeNotSetToFuture(Parent.JW_ATDInfo, Parent.LoadPort);
			}
		}

		protected virtual void CheckATDvsATA()
		{
			if (!Parent.JW_ATDInfo.HasErrors() && Parent.JW_ATA.IsValid)
			{
				if (Parent.JW_ATD > Parent.JW_ATA)
				{
					Parent.JW_ATDInfo.AddNotification(notificationType, Res.GetString("8e7555a4-f7ce-4988-8f5a-f36ba42e42da", "ATD cannot be after ATA."));
				}
			}
		}

		#endregion

		#region JW_ATA

		protected override void CheckJW_ATA()
		{
			base.CheckJW_ATA();
			VoyageDestination destination = Parent.Sailing == null ? null : Parent.Sailing.Destination;

			if (destination == null || !ProxyValidation(Parent.JW_ATAInfo, destination.JB_A_ARVInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_ATAInfo);
			}

			CheckATAvsATD();

			if (!Parent.JW_RL_NKDiscPort.IsEmpty && !Parent.JW_RL_NKDiscPortInfo.HasErrors() && Parent.DiscPort != null)
			{
				CheckRelatedPortLocalTimeNotSetToFuture(Parent.JW_ATAInfo, Parent.DiscPort);
			}

			if (!Parent.JW_ATAInfo.HasErrors() && destination != null)
			{
				ValidateJW_TerminalAvailabilityDate();
			}
		}

		protected virtual void CheckATAvsATD()
		{
			if (!Parent.JW_ATAInfo.HasErrors() && Parent.JW_ATD.IsValid)
			{
				if (Parent.JW_ATA < Parent.JW_ATD)
				{
					Parent.JW_ATAInfo.AddNotification(notificationType, Res.GetString("14fc3493-c860-43a2-b65a-872a1e3efef4", "ATA cannot be before ATD."));
				}
			}
		}

		#endregion

		#region JW_JX_Load_ATA

		protected override void CheckJW_JX_Load_ATA()
		{
			base.CheckJW_JX_Load_ATA();

			ValidateRequiredSecurity(Parent.JW_JX_Load_ATAInfo);
			ValidateProxiedSailingOnlyProperty(Parent.JW_JX_Load_ATAInfo);

			if (Parent.Sailing == null || Parent.Sailing.Origin == null || !ProxyValidation(Parent.JW_JX_Load_ATAInfo, Parent.Sailing.Origin.JA_A_ARVInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_JX_Load_ATAInfo);
			}
		}

		#endregion

		protected override void CheckJW_DistanceUnit()
		{
			base.CheckJW_DistanceUnit();
			ListValidation.ErrorIfInvalidCode(Parent.JW_DistanceUnitInfo, Parent.Lookups.DistanceUnit_List);

			if (Parent.JW_Distance > 0m)
			{
				MandatoryValidation.CheckEntered(Parent.JW_DistanceUnitInfo);
			}
		}

		protected override void CheckJW_JX_JV_RegistrationNo()
		{
			base.CheckJW_JX_JV_RegistrationNo();

			ValidateProxiedSailingOnlyProperty(Parent.JW_JX_JV_RegistrationNoInfo);

			if (Parent.Voyage == null || !ProxyValidation(Parent.JW_JX_JV_RegistrationNoInfo, Parent.Voyage.JV_RegistrationNoInfo))
			{
				CheckJW_JX_JV_RegistrationNo_NoSailing();
			}
		}

		protected virtual void CheckJW_JX_JV_RegistrationNo_NoSailing()
		{
		}

		protected override void CheckJW_TerminalReceivalCommences()
		{
			base.CheckJW_TerminalReceivalCommences();

			if (Parent.Sailing == null || Parent.Sailing.Origin == null || !ProxyValidation(Parent.JW_TerminalReceivalCommencesInfo, Parent.Sailing.Origin.JA_ReceivalCommencesInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_TerminalReceivalCommencesInfo);
			}
		}

		protected override void CheckJW_DepotReceivalCommences()
		{
			base.CheckJW_DepotReceivalCommences();

			if (Parent.Sailing == null || !ProxyValidation(Parent.JW_DepotReceivalCommencesInfo, Parent.Sailing.JX_DepotReceivalCommencesInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_DepotReceivalCommencesInfo);
			}
		}

		protected override void CheckJW_TerminalCutOff()
		{
			base.CheckJW_TerminalCutOff();

			if (Parent.Sailing == null || Parent.Sailing.Origin == null || !ProxyValidation(Parent.JW_TerminalCutOffInfo, Parent.Sailing.Origin.JA_CutOffInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_TerminalCutOffInfo);
			}
		}

		protected override void CheckJW_DepotCutOff()
		{
			base.CheckJW_DepotCutOff();

			if (Parent.Sailing == null || !ProxyValidation(Parent.JW_DepotCutOffInfo, Parent.Sailing.JX_DepotCutOffInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_DepotCutOffInfo);
			}
		}

		protected override void CheckJW_DocumentaryCutOff()
		{
			base.CheckJW_DocumentaryCutOff();

			if (Parent.Sailing == null || Parent.Sailing.Origin == null || !ProxyValidation(Parent.JW_DocumentaryCutOffInfo, Parent.Sailing.Origin.JA_DocumentaryCutoffInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_DocumentaryCutOffInfo);
			}
		}

		protected override void CheckJW_VGMCutOff()
		{
			base.CheckJW_VGMCutOff();

			if (Parent.Sailing == null || Parent.Sailing.Origin == null || !ProxyValidation(Parent.JW_VGMCutOffInfo, Parent.Sailing.Origin.JA_VGMCutOffInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_VGMCutOffInfo);
			}
		}

		protected override void CheckJW_TerminalAvailabilityDate()
		{
			base.CheckJW_TerminalAvailabilityDate();

			if (Parent.Sailing == null || Parent.Sailing.Destination == null || !ProxyValidation(Parent.JW_TerminalAvailabilityDateInfo, Parent.Sailing.Destination.JB_AvailabilityDateInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_TerminalAvailabilityDateInfo);
			}
		}

		protected override void CheckJW_DepotAvailabilityDate()
		{
			base.CheckJW_DepotAvailabilityDate();

			if (Parent.Sailing == null || !ProxyValidation(Parent.JW_DepotAvailabilityDateInfo, Parent.Sailing.JX_DepotAvailabilityDateInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_DepotAvailabilityDateInfo);
			}
		}

		protected override void CheckJW_TerminalStorageDate()
		{
			base.CheckJW_TerminalStorageDate();

			if (Parent.Sailing == null || Parent.Sailing.Destination == null || !ProxyValidation(Parent.JW_TerminalStorageDateInfo, Parent.Sailing.Destination.JB_StorageDateInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_TerminalStorageDateInfo);
			}
		}

		protected override void CheckJW_JX_IsPublished()
		{
			if (Parent.JW_JX_IsPublished)
			{
				ValidateProxiedSailingOnlyProperty(Parent.JW_JX_IsPublishedInfo);
			}

			if (Parent.Sailing != null)
			{
				ProxyValidation(Parent.JW_JX_IsPublishedInfo, Parent.Sailing.JX_IsPublishedInfo);
			}
		}

		protected override void CheckJW_DepotStorageDate()
		{
			base.CheckJW_DepotStorageDate();

			if (Parent.Sailing == null || !ProxyValidation(Parent.JW_DepotStorageDateInfo, Parent.Sailing.JX_DepotStorageDateInfo))
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.JW_DepotStorageDateInfo);
			}
		}

		protected override void CheckCreditorPK()
		{
			if (!Parent.CreditorPK.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.CreditorPKInfo, ResString.GetMultilingualString("ae5ebe59-d923-9b9f-4383-a9d0b0621d45", "The selected organization is not valid. Please choose a new organization or amend the organization using F3."));
			}
		}

		protected override void CheckCarrierPK()
		{
			if (!Parent.CarrierPK.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.CarrierPKInfo);
			}
		}

		protected override void CheckJW_PL_NKCarrierServiceLevel()
		{
			if (!Parent.JW_PL_NKCarrierServiceLevel.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JW_PL_NKCarrierServiceLevelInfo);
			}
		}

		protected override void CheckJW_EmptyReceivalCommences()
		{
			base.CheckJW_EmptyReceivalCommences();

			if (Parent.JW_EmptyReceivalCommences > Parent.JW_EmptyCutOff)
			{
				Parent.JW_EmptyReceivalCommencesInfo.AddError(EmptyReceivalCommencesMustBeforeEmptyCutOff);
			}
		}

		protected override void CheckJW_EmptyCutOff()
		{
			base.CheckJW_EmptyCutOff();

			if (Parent.JW_EmptyReceivalCommences > Parent.JW_EmptyCutOff)
			{
				Parent.JW_EmptyCutOffInfo.AddError(EmptyReceivalCommencesMustBeforeEmptyCutOff);
			}
		}

		string EmptyReceivalCommencesMustBeforeEmptyCutOff => Res.GetString("53509936-d6ed-4080-ade3-bb3df08c1a24", "Empty Receival Start date must be before the Empty Cut Off date.");

		protected override void CheckJW_ReeferReceivalCommences()
		{
			base.CheckJW_ReeferReceivalCommences();

			if (Parent.JW_ReeferReceivalCommences > Parent.JW_ReeferCutOff)
			{
				Parent.JW_ReeferReceivalCommencesInfo.AddError(ReeferReceivalCommencesMustBeforeReeferCutOff);
			}
		}

		protected override void CheckJW_ReeferCutOff()
		{
			base.CheckJW_ReeferCutOff();

			if (Parent.JW_ReeferReceivalCommences > Parent.JW_ReeferCutOff)
			{
				Parent.JW_ReeferCutOffInfo.AddError(ReeferReceivalCommencesMustBeforeReeferCutOff);
			}
		}

		string ReeferReceivalCommencesMustBeforeReeferCutOff => Res.GetString("dd4cdcb4-4614-46cd-811f-dfd209773b80", "Reefer Receival Start date must be before the Reefer Cut Off date.");

		protected override void CheckJW_DGReceivalCommences()
		{
			base.CheckJW_DGReceivalCommences();

			if (Parent.JW_DGReceivalCommences > Parent.JW_DGCutOff)
			{
				Parent.JW_DGReceivalCommencesInfo.AddError(DGReceivalCommencesMustBeforeDGCutOff);
			}
		}

		protected override void CheckJW_DGCutOff()
		{
			base.CheckJW_DGCutOff();

			if (Parent.JW_DGReceivalCommences > Parent.JW_DGCutOff)
			{
				Parent.JW_DGCutOffInfo.AddError(DGReceivalCommencesMustBeforeDGCutOff);
			}
		}

		protected override void CheckTotalCO2eForSorting()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForSortingInfo);

			var value = (Parent as ICO2eProvider).RequireTEU ? Parent.GetCO2ePerTEUInKg() : Parent.GetCO2ePerTonneInKg();
			if (Parent.GetCO2eStatus() == CO2eStatusList.Codes.Current && value == 0 && Parent.LoadPort != null && Parent.DiscPort != null)
			{
				Parent.TotalCO2eForSortingInfo.AddWarning(Res.GetString("2e7aff34-9cbf-4937-bfc8-7c83f89e96c5", "The greenhouse gas emissions for the input data on routing leg cannot be calculated."));
			}
		}

		string DGReceivalCommencesMustBeforeDGCutOff => Res.GetString("6d05c50f-8520-49b2-88e9-363d3489d16c", "HAZ Receival Start date must be before the HAZ Cut Off date.");

		#endregion

		#region Implementation

		protected void NotifyIfNotEntered(ZPropertyInfo info)
		{
			if (dontErrorOnMissingDetails)
			{
				MandatoryValidation.WarnIfNotEntered(info);
			}
			else
			{
				MandatoryValidation.CheckEntered(info);
			}
		}

		bool TransportTypeShouldBeUnique(ZString transportType)
		{
			return transportType == Constants.TransportPlanningType.MainVessel
				|| transportType == Constants.TransportPlanningType.Flight1
				|| transportType == Constants.TransportPlanningType.Flight2
				|| transportType == Constants.TransportPlanningType.Flight3;
		}

		protected void ValidateRequiredSecurity(ZPropertyInfo info)
		{
			if (Parent.CreateDeniedBySecurity)
			{
				SecurityCheckpoint checkpoint = FreightUtilities.GetCreateScheduleFromJobSecurityCheckpoint(Parent.JW_TransportMode);
				info.AddError(Res.GetString("a8c0007c-b552-4d01-9726-66475cadc27b",
					"No matching schedule could be found and you do not have the security rights to create one. If you believe you should have rights then ask your administrator for the {0} security right.",
					checkpoint.DisplayTextPathToSecurityRight));
			}
		}

		void CheckPorts(bool isLoad)
		{
			var portPropertyInfo = isLoad ? Parent.JW_RL_NKLoadPortInfo : Parent.JW_RL_NKDiscPortInfo;

			if (Parent.JW_TransportMode != Constants.TransportModes.Storage && !portPropertyInfo.Value.IsEmpty)
			{
				if (Parent.JW_RL_NKLoadPort == Parent.JW_RL_NKDiscPort
					&& (Parent.TransportMode == Constants.TransportModes.Air
					|| Parent.JW_OA_ArrivalLocation == Parent.JW_OA_DepartureLocation
					|| Parent.JW_OA_ArrivalLocation.IsEmpty != Parent.JW_OA_DepartureLocation.IsEmpty))
				{
					if (Parent.IsRailOrRoadOrInlandWaterway)
					{
						portPropertyInfo.AddWarning(Res.GetString("d10c6ab2-7a58-4645-8832-82f8af2cc6a8", "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations."));
					}
					else
					{
						portPropertyInfo.AddNotification(notificationType, Res.GetString("0911a096-96fa-4b97-b6e1-4ae79a523516", "The Load and Discharge cannot be the same."));
					}
				}

				CheckIfSamePortsOnOtherLegs(portPropertyInfo, isLoad);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Concatenating two res strings")]
		void CheckIfSamePortsOnOtherLegs(ZPropertyInfo portPropertyInfo, bool isLoad)
		{
			var parentIsWithinPort = Parent.IsWithinPort();

			if (Parent.Parent != null && !(parentIsWithinPort && Parent.IsRailOrRoadOrInlandWaterway))
			{
				var duplicateMessage = Res.GetString("54a84102-bf78-4a65-8f70-e564f7fe5046", "You can't have more than one leg {0} the same port.", isLoad ? "leaving" : "arriving at");
				var locationsMessage = Res.GetString("a3e68513-1dfe-41f9-b3b4-7055e8c0514a", "Please specify arrival and departure locations if you need this port setup.");
				var parentLocation = isLoad ? Parent.JW_OA_DepartureLocation : Parent.JW_OA_ArrivalLocation;

				foreach (var otherTransport in OtherParentTransports)
				{
					if (!otherTransport.IsWithinPort()
						&& otherTransport.JW_TransportMode != Constants.TransportModes.Storage
						&& portPropertyInfo.Value.Equals(otherTransport[portPropertyInfo.Name]))
					{
						var otherLocation = isLoad ? otherTransport.JW_OA_DepartureLocation : otherTransport.JW_OA_ArrivalLocation;
						if (parentIsWithinPort && parentLocation == otherLocation)
						{
							portPropertyInfo.AddError($"{duplicateMessage} {locationsMessage}");
							break;
						}
						else if (!parentIsWithinPort)
						{
							portPropertyInfo.AddError(duplicateMessage);
							break;
						}
					}
				}
			}
		}

		bool ProxyValidation(ZPropertyInfo toInfo, ZPropertyInfo fromInfo)
		{
			bool success;

			if (Parent.JW_IsLinked && !fromInfo.BizObj.IsValidationSuspended)
			{
				((IBusinessObjectInternals)fromInfo.BizObj).Validate(fromInfo);
				CheckAllowEditProxiedField(toInfo, fromInfo);
				toInfo.AddAllNotificationsFrom(fromInfo);
				success = true;
			}
			else
			{
				success = false;
			}

			return success;
		}

		void CheckAllowEditProxiedField(ZPropertyInfo toInfo, ZPropertyInfo fromInfo)
		{
			SecurityCheckpoint checkpoint = FreightUtilities.GetEditScheduleSecurityCheckpoint(Parent.JW_TransportMode);

			if (checkpoint != null && !checkpoint.IsAllowed && (fromInfo.HasChanges || !toInfo.Value.Equals(fromInfo.Value)))
			{
				toInfo.AddError(Res.GetString("00ebae55-6fd9-474e-9f69-60c22c212115",
					"You do not have security rights to edit this value.\r\nIf you believe you should have rights then ask your administrator for the {0} security right.",
					checkpoint.DisplayTextPathToSecurityRight));
			}
		}

		#region Check Time has not been set to the future for the Local Time of a Related Port

		public static void CheckRelatedPortLocalTimeNotSetToFuture(ZPropertyInfo info, RefUNLOCO relatedPort)
		{
			ZDateTime localTime = (ZDateTime)info.Value;

			if (localTime.IsValid && relatedPort != null && relatedPort.TimeZoneSet != null)
			{
				ZDateTime utcTimeFromPort = Env.Time.GetUtcFromUnlocoTime(relatedPort.Code.ToString(), localTime.ToDateTime());
				if (utcTimeFromPort.IsValid && utcTimeFromPort > CargoWise.Types.ZDateTime.UtcNow)
				{
					var offsetTime = (localTime - utcTimeFromPort).TotalHours;
					var sign = (offsetTime < 0) ? "" : "+";

					var utcOffset = sign + offsetTime;

					info.AddError(Res.GetString("c6c544e5-80ed-4fa0-98c9-e6d9aef278c1",
						"The {0} cannot be set in the future. The date {1} is in the future for {2} (UTC{3}).", info.HumanReadableName, localTime.ToLongTimeString(), relatedPort.Code, utcOffset));
				}
			}
		}

		#endregion

		void ValidateProxiedSailingOnlyProperty(ZPropertyInfo info)
		{
			bool isEmpty = info is ZPropertyInfoBool ? !(ZBool)info.Value : info.Value.IsEmpty;
			if (!Parent.JW_IsLinked && !isEmpty)
			{
				info.AddError(ErrorMesageValueOnlySavedForUnlinkedTransports);
			}
		}

		static string ErrorMesageValueOnlySavedForUnlinkedTransports
		{
			get { return Res.GetString("3822a106-341e-4a73-8c92-a84cc08a50ad", "This value is only saved for linked transports."); }
		}

		#endregion

		#region TransportSupporterWithSchedule

		protected TransportSupporter TransportSupporterWithSchedule
		{
			get { return Parent.TransportSupporterWithSchedule; }
		}

		#endregion

		#region OtherParentTransports

		IEnumerable<Transport> OtherParentTransports
		{
			get { return Parent.OtherParentTransports; }
		}

		#endregion
	}
}
