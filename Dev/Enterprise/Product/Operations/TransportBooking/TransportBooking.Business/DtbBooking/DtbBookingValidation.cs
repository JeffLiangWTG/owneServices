using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingValidation : Common.DtbBookingValidation
	{
		public DtbBookingValidation(DtbBooking parent)
			: base(parent)
		{
		}

		protected new DtbBooking Parent
		{
			get { return (DtbBooking)base.Parent; }
		}

		protected override void CheckKM_PL_NKCarrierServiceLevel()
		{
			base.CheckKM_PL_NKCarrierServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.KM_PL_NKCarrierServiceLevelInfo);
		}

		protected override void CheckKM_ChargeableIsValidZDecimal()
		{
			if (Parent.KM_OverrideChargeable)
			{
				base.CheckKM_ChargeableIsValidZDecimal();
			}
		}

		protected override void CheckKM_KT_NKBookingTemplate()
		{
			base.CheckKM_KT_NKBookingTemplate();
			ListValidation.ErrorIfInvalidCode(Parent.KM_KT_NKBookingTemplateInfo);
			if (Parent.IsSub & Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				var masterBookingTemplate = Parent.MasterBooking.KM_KT_NKBookingTemplate;
				var subBookingTemplate = Parent.KM_KT_NKBookingTemplate;
				if (masterBookingTemplate != subBookingTemplate)
				{
					Parent.KM_KT_NKBookingTemplateInfo.AddError(Res.GetString("54f5d468-261b-48b2-8285-52ffd9dc7c38", "This Booking has a different template to the Master Booking."));
				}
			}
		}

		protected override void CheckKM_Direction()
		{
			base.CheckKM_Direction();
			if (Parent.IsSub & Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				var masterBookingDirection = Parent.MasterBooking.KM_Direction;
				var subBookingDirection = Parent.KM_Direction;
				if (masterBookingDirection != subBookingDirection)
				{
					Parent.KM_DirectionInfo.AddError(Res.GetString("3e34508b-f217-4874-9201-7cad5b42f024", "This Booking has a different booking direction to the Master Booking."));
				}
			}
		}

		protected override void CheckKM_IsHazardous()
		{
			base.CheckKM_IsHazardous();

			if (!Parent.IsSub && !Parent.KM_IsHazardousInfo.HasErrors())
			{
				var isBookingContainsHazardousPackages = Parent.IsAnyPackageHazardous;
				if (Parent.KM_IsHazardous && !isBookingContainsHazardousPackages)
				{
					Parent.KM_IsHazardousInfo.AddError(Res.GetString("DtbBookingValidation|NoPackagesWithDGs", "No packages with Dangerous Goods have been assigned to Instructions."));
				}
				else if (!Parent.KM_IsHazardous && isBookingContainsHazardousPackages)
				{
					Parent.KM_IsHazardousInfo.AddError(Res.GetString("DtbBookingValidation|HavePackagesWithDGs", "There are packages with Dangerous Goods assigned to Instructions. Is Hazardous needs to be checked."));
				}
			}
		}

		protected override void CheckKM_RequiresRefrigeration()
		{
			base.CheckKM_RequiresRefrigeration();

			if (!Parent.IsSub && !Parent.KM_RequiresRefrigerationInfo.HasErrors())
			{
				var requiresRefrigerationFromPackages = Parent.IsAnyPackageRequiresRefridgeration;
				if (Parent.KM_RequiresRefrigeration && !requiresRefrigerationFromPackages)
				{
					Parent.KM_RequiresRefrigerationInfo.AddError(Res.GetString("DtbBookingValidation|NoPackagesWithRefrigeration", "No packages that Require Refrigeration have been assigned to Instructions."));
				}
				else if (!Parent.KM_RequiresRefrigeration && requiresRefrigerationFromPackages)
				{
					Parent.KM_RequiresRefrigerationInfo.AddError(Res.GetString("DtbBookingValidation|HavePackagesWithRefrigeration", "There are packages that Require Refrigeration assigned to Instructions. Require Refrigeration need to be checked."));
				}
			}
		}

		protected override void CheckKM_RS_NKServiceLevel()
		{
			base.CheckKM_RS_NKServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.KM_RS_NKServiceLevelInfo);
		}

		protected override void CheckKM_IsMaster()
		{
			base.CheckKM_IsMaster();
			if (Parent.KM_IsMaster && Parent.KM_KM_MasterBooking != ZGuid.Empty)
			{
				Parent.KM_IsMasterInfo.AddError(Res.GetString("DtbBookingValidation|MasterBookingKM_IsMaster", "Master Bookings cannot be attached to another Master Booking."));
			}
		}

		protected override void CheckKM_KM_MasterBooking()
		{
			base.CheckKM_KM_MasterBooking();
			if (Parent.KM_KM_MasterBooking != ZGuid.Empty && (ZGuid)Parent.KM_KM_MasterBookingInfo.OriginalValue != ZGuid.Empty && Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				Parent.KM_KM_MasterBookingInfo.AddError(Res.GetString("DtbBookingValidation|MasterBookingKM_KM_MasterBooking", "Booking cannot change directly from one Master Booking to another Master Booking."));
			}
		}

		protected override void CheckKM_MasterBookingVersion()
		{
			base.CheckKM_MasterBookingVersion();
			if ((ZShort)Parent.KM_MasterBookingVersionInfo.OriginalValue > 0 && Parent.KM_KM_MasterBooking != ZGuid.Empty && Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				Parent.KM_MasterBookingVersionInfo.AddError(Res.GetString("DtbBookingValidation|MasterBookingKM_MasterBookingVersion", "Cannot attach a Booking with a non-zero Master Booking Version."));
			}
		}

		protected override void CheckKM_KB_BookingConsolidationMultiJob()
		{
			base.CheckKM_KB_BookingConsolidationMultiJob();
			if (Parent.KM_KB_BookingConsolidationMultiJob != ZGuid.Empty && Parent.KM_KM_MasterBooking != ZGuid.Empty)
			{
				Parent.KM_KB_BookingConsolidationMultiJobInfo.AddError(Res.GetString("DtbBookingValidation|MasterBookingKM_KB_BookingConsolidationMultiJob", "Cannot attach Booking which is part of a Consolidated Booking to a Master Booking."));
			}
		}

		protected override void CheckKM_Status()
		{
			base.CheckKM_Status();
			if (Parent.KM_Status != TransportStatuses.Codes.Available && Parent.KM_KM_MasterBooking != ZGuid.Empty && Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				Parent.KM_StatusInfo.AddError(Res.GetString("DtbBookingValidation|MasterBookingKM_Status", "Cannot attach Booking which does not have a status of Available"));
			}
		}

		protected override void CheckKM_IsAgentBooking()
		{
			base.CheckKM_IsAgentBooking();
			if (Parent.KM_IsAgentBooking && Parent.KM_KM_MasterBooking != ZGuid.Empty)
			{
				Parent.KM_IsAgentBookingInfo.AddError(Res.GetString("DtbBookingValidation|MasterBookingKM_IsAgentBooking", "Cannot attach an Agent Booking"));
			}
		}

		protected override void CheckKM_TransportMode()
		{
			base.CheckKM_TransportMode();
			ListValidation.ErrorIfInvalidCode(Parent.KM_TransportModeInfo);
			if (Parent.IsSub && Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				var masterBookingTransportMode = Parent.MasterBooking.KM_TransportMode;
				var subBookingTransportMode = Parent.KM_TransportMode;
				if (masterBookingTransportMode != subBookingTransportMode)
				{
					Parent.KM_TransportModeInfo.AddError(Res.GetString("a9283974-a2d4-401d-a607-959d9258e534", "This Booking has a different transport mode to the Master Booking."));
				}
			}
			else if (Parent.KM_TransportMode.IsEmpty)
			{
				Parent.KM_TransportModeInfo.AddError(Res.GetString("9b4e3e93-15c9-46d9-b30b-d9ad223a25ff", "Booking Transport Mode cannot be empty."));
			}
		}

		public void ValidateTransportCoAgainstMultiJobConsolidation()
		{
			var errorMessage = (Res.GetString("c4fa3e93-2764-4a67-8f85-1a2ab84db398", "This Booking does not have the same Transport Company as the Consolidation."));
			Parent.RemoveRowError(errorMessage);

			if (Parent.IsOnMultiJobConsolidation)
			{
				var consolidationTransportCo = Parent.ConsolidationMultiJob.Address.Organisation;
				if (consolidationTransportCo != null && Parent.Address.OrganisationPK != consolidationTransportCo.PK)
				{
					Parent.AddRowError(errorMessage);
				}
			}
		}

		void ValidateForSendingXUSToCTOIfRequired()
		{
			Parent.NotificationBufferForSendingXUSToCTO = new NotificationBuffer();

			if (Parent.IsSendingXUSToCTO)
			{
				ValidateCarrierBookingReferenceForSendingXUSToCTO();
				ValidateParentTypeForSendingXUSToCTO();
				ValidateParentTransportModeForSendingXUSToCTO();
				ValidateInstructionTypesForSendingXUSToCTO();
				ValidateInstructionsAndConfirmationsAndAttachedPackagesForSendingXUSToCTO();

				Parent.Address.OrganisationPKInfo.AdditionalValidation += ValidateTransportCompanyForSendingXUSToCTO;
				Parent.Address.Validation.ValidateOrganisationPK();

				CollectAndStoreErrorAndMessageErrorNotificationsForSendingXUSToCTO();
			}
		}

		void CollectAndStoreErrorAndMessageErrorNotificationsForSendingXUSToCTO()
		{
			var bookingNotificationCollector = new HumanReadableNotificationCollector(Parent, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
			var notifications = bookingNotificationCollector.GetMessageErrors().Concat(bookingNotificationCollector.GetErrors());

			foreach (var package in Parent.ConsolidationSingleJob.PackageJob.Packages)
			{
				var packageNotificationCollector = new HumanReadableNotificationCollector(package, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
				var packageNotifications = packageNotificationCollector.GetMessageErrors().Concat(packageNotificationCollector.GetErrors());
				notifications = notifications.Concat(packageNotifications);

				if (package.IsContainer && package.Container != null)
				{
					var packageTypeNotificationCollector = new HumanReadableNotificationCollector(package.Container.ContainerType, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
					var packageTypeNotifications = packageTypeNotificationCollector.GetMessageErrors().Concat(packageTypeNotificationCollector.GetErrors());
					notifications = notifications.Concat(packageTypeNotifications);
				}
			}

			Parent.NotificationBufferForSendingXUSToCTO.AddRange(notifications);
		}

		void ValidateInstructionsAndConfirmationsAndAttachedPackagesForSendingXUSToCTO()
		{
			foreach (var instruction in Parent.Instructions)
			{
				instruction.Validation.ValidateAll();
				instruction.Address.Validation.ValidateAll();
				foreach (var confirmation in instruction.Confirmations)
				{
					confirmation.Validation.ValidateAll();
				}
				foreach (var package in instruction.GetPackages)
				{
					package.Validation.ValidateKP_PackageID();
					package.Validation.ValidateKP_F3_NKPackType();

					package?.Container?.ContainerType?.Validation.ValidateRC_Code();
				}
			}
		}

		void ValidateCarrierBookingReferenceForSendingXUSToCTO()
		{
			if (!Parent.AdditionalReferencesForBinding.Cast<TransportBookingAdditionalReference>().Any(r => r.EntryType == CargoWise.Definitions.TransportAdditionalReferenceTypes.Codes.CarrierBookingReference && !string.IsNullOrEmpty(r.EntryNum)))
			{
				Parent.NotificationBufferForSendingXUSToCTO.AddMessageError(Res.GetString("DtbBookingValidation|SendingXUSToCTO_CarrierBookingReference", "Carrier Booking Reference has not been entered."));
			}
		}

		void ValidateParentTransportModeForSendingXUSToCTO()
		{
			if (Parent.ConsolidationSingleJob.Parent != null && Parent.ConsolidationSingleJob.KB_ParentTableCode == JobShipmentSchema.Constants.Prefix && !Parent.ConsolidationSingleJob.Parent.TransportMode.Equals(Constants.TransportModes.Sea))
			{
				Parent.NotificationBufferForSendingXUSToCTO.AddMessageError(Res.GetString("DtbBookingValidation|SendingXUSToCTO_ParentTransportMode", "The Booking Parent must have a Transport Mode of SEA."));
			}
		}

		void ValidateParentTypeForSendingXUSToCTO()
		{
			if (Parent.ConsolidationSingleJob.KB_ParentTableCode != JobShipmentSchema.Constants.Prefix)
			{
				Parent.NotificationBufferForSendingXUSToCTO.AddMessageError(Res.GetString("DtbBookingValidation|SendingXUSToCTO_ParentTableCode", "The Booking must have a Shipment as a Parent."));
			}
		}

		void ValidateTransportCompanyForSendingXUSToCTO()
		{
			if (Parent.Address.Organisation != null)
			{
				Parent.Address.OrganisationPKInfo.AddMessageError(Res.GetString("DtbBookingValidation|SendingXUSToCTO_TransportCompany", "The Booking must have no assigned Transport Company."));
			}
		}

		void ValidateInstructionTypesForSendingXUSToCTO()
		{
			var expectedInstructionTypes = new List<(string InstructionType, string OrgType)>();

			if (Parent.ConsolidationSingleJob.Direction == DtbBookingDirection.PIC)
			{
				expectedInstructionTypes.Add((InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD));
				expectedInstructionTypes.Add((InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNR));
				expectedInstructionTypes.Add((InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CTO));
			}
			else if (Parent.ConsolidationSingleJob.Direction == DtbBookingDirection.DLV)
			{
				expectedInstructionTypes.Add((InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO));
				expectedInstructionTypes.Add((InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE));
				expectedInstructionTypes.Add((InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD));
			}

			foreach (var instruction in Parent.Instructions)
			{
				var instructionTypeAndOrgType = (instruction.KN_InstructionType, instruction.OrganisationType);
				if (expectedInstructionTypes.Contains(instructionTypeAndOrgType))
				{
					expectedInstructionTypes.Remove(instructionTypeAndOrgType);
				}
			}

			if (expectedInstructionTypes.Count > 0)
			{
				var messageError = new StringBuilder();
				messageError.AppendLine(Res.GetString("986ac61c-efd8-4578-8f40-059be5bcaa28", "Booking requires the following instruction types for sending to CTO:"));
				foreach (var pair in expectedInstructionTypes)
				{
					messageError.AppendLine(Res.GetString("9b37f4dd-f83e-4418-b868-6f521612800a", "Instruction Type: {0}, Organization Type: {1}", pair.InstructionType, pair.OrgType));
				}
				Parent.NotificationBufferForSendingXUSToCTO.AddMessageError(messageError.ToString());
			}
		}

		void ValidateForAttachment()
		{
			Func<DtbBooking, DtbBookingInstruction> returnPickup = delegate(DtbBooking booking) { return booking.FirstPickup; };
			Func<DtbBooking, DtbBookingInstruction> returnMasterPickup = delegate(DtbBooking booking) { return booking.MasterBooking.FirstPickup; };
			ValidateBookingAddressAgainstMasterBookingCore(returnMasterPickup, returnPickup, "FirstPickup");

			Func<DtbBooking, DtbBookingInstruction> returnConsignee = delegate(DtbBooking booking) { return booking.FirstConsignee; };
			Func<DtbBooking, DtbBookingInstruction> returnMasterConsignee = delegate(DtbBooking booking) { return booking.MasterBooking.FirstConsignee; };
			ValidateBookingAddressAgainstMasterBookingCore(returnMasterConsignee, returnConsignee, "FirstConsignee");

			Func<DtbBooking, DtbBookingInstruction> returnConsignor = delegate(DtbBooking booking) { return booking.FirstConsignor; };
			Func<DtbBooking, DtbBookingInstruction> returnMasterConsignor = delegate(DtbBooking booking) { return booking.MasterBooking.FirstConsignor; };
			ValidateBookingAddressAgainstMasterBookingCore(returnMasterConsignor, returnConsignor, "FirstConsignor");

			Func<DtbBooking, DtbBookingInstruction> returnDelivery = delegate(DtbBooking booking) { return booking.LastDelivery; };
			Func<DtbBooking, DtbBookingInstruction> returnMasterDelivery = delegate(DtbBooking booking) { return booking.MasterBooking.LastDelivery; };
			ValidateBookingAddressAgainstMasterBookingCore(returnMasterDelivery, returnDelivery, "LastDelivery");

			ValidateCarrierBookingAgentAgainstMasterBooking();
			ValidateTransportCoAgainstMasterBooking();
		}

		void ValidateTransportCoAgainstMasterBooking()
		{
			if (Parent.IsSub && Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				var nonMasterAddress = GetNonMasterAddress(DocAddressType.TransportCompanyDocumentaryAddress, Parent.PK);
				if (Parent.MasterBooking.Address.Address == null || nonMasterAddress?.E2_OA_Address == ZGuid.Empty || nonMasterAddress == null)
				{
					if (Parent.MasterBooking.Address.Address == null && nonMasterAddress?.E2_OA_Address != ZGuid.Empty && nonMasterAddress != null)
					{
						ErrorHandlerForAttachment("TransportCo", "");
					}
					else if (Parent.MasterBooking.Address.Address != null && nonMasterAddress?.E2_OA_Address == ZGuid.Empty || Parent.MasterBooking.Address.Address != null && nonMasterAddress == null)
					{
						ErrorHandlerForAttachment("TransportCo", Parent.MasterBooking.Address.Address.OA_Code);
					}
				}
				else
				{
					var masterTransportCo = Parent.MasterBooking.Address.Address.PK;
					var subTransportCo = nonMasterAddress.E2_OA_Address;
					if (masterTransportCo != subTransportCo)
					{
						ErrorHandlerForAttachment("TransportCo", Parent.MasterBooking.Address.Address.OA_Code);
					}
				}
			}
		}

		void ValidateCarrierBookingAgentAgainstMasterBooking()
		{
			if (Parent.IsSub && Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				var nonMasterCBA = GetNonMasterAddress(DocAddressType.CarrierBookingAgent, Parent.PK);
				if (Parent.MasterBooking.CarrierBookingAgentDocAddress.Address == null || nonMasterCBA?.E2_OA_Address == ZGuid.Empty || nonMasterCBA == null)
				{
					if (Parent.MasterBooking.CarrierBookingAgentDocAddress.Address == null && nonMasterCBA?.E2_OA_Address != ZGuid.Empty && nonMasterCBA != null)
					{
						ErrorHandlerForAttachment("CarrierBookingAgent", "");
					}
					else if (Parent.MasterBooking.CarrierBookingAgentDocAddress.Address != null && nonMasterCBA?.E2_OA_Address == ZGuid.Empty || Parent.MasterBooking.CarrierBookingAgentDocAddress.Address != null && nonMasterCBA == null)
					{
						ErrorHandlerForAttachment("CarrierBookingAgent", Parent.MasterBooking.CarrierBookingAgentDocAddress.Address.OA_Code);
					}
				}
				else
				{
					var masterCarrierBookingAgent = Parent.MasterBooking.CarrierBookingAgentDocAddress.Address.PK;
					var subCarrierBookingAgent = nonMasterCBA.E2_OA_Address;
					if (masterCarrierBookingAgent != subCarrierBookingAgent)
					{
						ErrorHandlerForAttachment("CarrierBookingAgent", Parent.MasterBooking.CarrierBookingAgentDocAddress.Address.OA_Code);
					}
				}
			}
		}

		void ValidateBookingAddressAgainstMasterBookingCore(Func<DtbBooking, DtbBookingInstruction> getMasterInstruction, Func<DtbBooking, DtbBookingInstruction> getSubInstruction, string errorToCreate)
		{
			if (Parent.IsSub & Parent.KM_KM_MasterBookingInfo.HasChanges)
			{
				if (getMasterInstruction(Parent) == null || getSubInstruction(Parent) == null)
				{
					if (getSubInstruction(Parent) != null && getMasterInstruction(Parent) == null || getSubInstruction(Parent) == null && getMasterInstruction(Parent) != null)
					{
						ErrorHandlerForAttachment(errorToCreate);
					}
				}
				else
				{
					var masterAddress1 = getMasterInstruction(Parent).Address.Address1;
					var subAddress1 = getSubInstruction(Parent).Address.Address1;
					var masterAddress2 = getMasterInstruction(Parent).Address.Address2;
					var subAddress2 = getSubInstruction(Parent).Address.Address2;
					var masterPostcode = getMasterInstruction(Parent).Address.Postcode;
					var subPostcode = getSubInstruction(Parent).Address.Postcode;
					var masterState = getMasterInstruction(Parent).Address.State;
					var subState = getSubInstruction(Parent).Address.State;
					var masterCountryCode = getMasterInstruction(Parent).Address.E2_RN_NKCountryCode;
					var subCountryCode = getSubInstruction(Parent).Address.E2_RN_NKCountryCode;

					if (masterAddress1 != subAddress1 || masterAddress2 != subAddress2 || masterPostcode != subPostcode || masterState != subState || masterCountryCode != subCountryCode)
					{
						ErrorHandlerForAttachment(errorToCreate);
					}
				}
			}
		}

		void ErrorHandlerForAttachment(string errorToCreate, string orgAddressCode = null)
		{
			switch (errorToCreate)
			{
				case "FirstPickup":
					Parent.AddRowError(Res.GetString("458274cb-9c31-432d-94bd-dc8d3695fae6", "This Booking has a different First Pickup Address to the Master Booking."));
					break;
				case "FirstConsignee":
					Parent.AddRowError(Res.GetString("1a9365e1-049b-4473-b04e-5b5455ca14c6", "This Booking has a different First Consignee Address to the Master Booking."));
					break;
				case "FirstConsignor":
					Parent.AddRowError(Res.GetString("6f3f23d1-3043-41e5-98d2-ca1e87cfcd28", "This Booking has a different First Consignor Address to the Master Booking."));
					break;
				case "LastDelivery":
					Parent.AddRowError(Res.GetString("cc4dc401-b9d1-407f-b0f7-292602eccc7b", "This Booking has a different Last Delivery Address to the Master Booking."));
					break;
				case "CarrierBookingAgent":
					Parent.AddRowError(Res.GetString("fc78d449-8828-4722-8cd3-bb4b34c485e8", "This Booking does not have the same Carrier Booking Agent as the Master Booking ({0}).", orgAddressCode));
					break;
				case "TransportCo":
					Parent.AddRowError(Res.GetString("19516d9f-6a1c-4161-a1f2-fe0e7b55a183", "This Booking does not have the same Transport Company as the Master Booking ({0}).", orgAddressCode));
					break;
				default:
					throw new Exception("Invalid 'errorToCreate' given to error handler.");
			}
		}

		public void ValidateTotalCO2eForBinding()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForBindingInfo);
		}

		protected void CheckTotalCO2eForBinding()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForBindingInfo);
		}

		public void ValidateTotalCO2eForSorting()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForSortingInfo);
		}

		protected void CheckTotalCO2eForSorting()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForSortingInfo);
		}

		void ValidateSubsInSyncWithMaster()
		{
			if (Parent.IsSub)
			{
				var checker = new DtbMasterBookingVersionChecker(Parent.MasterBooking);
				var warningMessage = Res.GetString("36cc114c-1a4c-4b67-be43-6d58a95b6cd1", "Not in sync with Master - Update service task in progress.");

				if (!checker.IsSubInSyncWithMaster(Parent))
				{
					Parent.AddRowWarning(warningMessage);
				}
				else
				{
					Parent.RemoveRowWarning(warningMessage);
				}
			}
		}

		public JobDocAddress GetNonMasterAddress(DocAddressType docAddressType, ZGuid bookingPk)
		{
			var requirement = (((IDocAddresses)Parent).GetDocAddressRequirement(docAddressType));
			var docAddresses = new JobDocAddressDependentCollection(Parent);
			docAddresses.Load(FilterDocAddressesByPK(bookingPk));
			var address = docAddresses.FindByDocAddressType(requirement.DefaultDocAddressType);
			return address;
		}

		ZQuery FilterDocAddressesByPK(ZGuid pK)
		{
			var result = new ZDBOnlyQuery(typeof(JobDocAddress));
			result.AddToFilter(JobDocAddressSchema.E2_ParentID, SQLComparisonOperator.Equal, pK);
			return result;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTransportCoAgainstMultiJobConsolidation();
			ValidateForAttachment();
			ValidateSubsInSyncWithMaster();
			ValidateTotalCO2eForBinding();
			ValidateTotalCO2eForSorting();
			ValidateForSendingXUSToCTOIfRequired();
		}

		public void ValidateForSync()
		{
			base.ValidateAll();
			ValidateTransportCoAgainstMultiJobConsolidation();
			ValidateSubsInSyncWithMaster();
		}
	}
}
