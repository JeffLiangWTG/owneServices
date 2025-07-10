using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	abstract class AgencyShipmentDataObjectReader<T> : ShipmentDataObjectReader<T> where T : AgencyShipment
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected AgencyShipmentDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IAgencyShipmentReadStrategy<T> readStrategy = null)
			: base(dataObject, logger, factory)
		{
			this.readStrategy = readStrategy ?? GetAgencyShipmentReadStrategy();
		}

		readonly IAgencyShipmentReadStrategy<T> readStrategy;
		bool hasCarrierBeenRead;

		protected abstract IAgencyShipmentReadStrategy<T> GetAgencyShipmentReadStrategy();

		#region Properties

		protected bool IsShippingInstruction
		{
			get
			{
				return dataObject.ContainsServiceCode(ServiceCodeType.SIN);
			}
		}

		protected bool IsElectronicBooking
		{
			get
			{
				return dataObject.ContainsServiceCode(ServiceCodeType.BRQ);
			}
		}

		protected bool IsFromCarrier
		{
			get
			{
				return dataObject.ContainsRecipientRole(RecipientRoleType.CAR);
			}
		}

		protected bool IsUnknownMessagePurpose
		{
			get
			{
				var purpose = dataObject.DataContext?.DocumentaryOverride?.Purpose?.Code.ToString();
				return purpose.IsNullOrEmpty();
			}
		}

		protected bool IsOriginalMessage
		{
			get
			{
				return dataObject.DataContext?.DocumentaryOverride?.Purpose?.Code.ToString() == Enterprise.DocumentVisualizer.Integration.MessagePurposes.Codes.Original;
			}
		}

		protected bool IsAmendmentMessage
		{
			get
			{
				return dataObject.DataContext?.DocumentaryOverride?.Purpose?.Code.ToString() == Enterprise.DocumentVisualizer.Integration.MessagePurposes.Codes.Amendment;
			}
		}

		protected bool IsWithdrawalMessage
		{
			get
			{
				return dataObject.DataContext?.DocumentaryOverride?.Purpose?.Code.ToString() == Enterprise.DocumentVisualizer.Integration.MessagePurposes.Codes.Withdrawal;
			}
		}

		#endregion

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(T targetBo)
		{
			var reason = ZString.Empty;

			if (targetBo == null)
			{
				if (References.IsVGM)
				{
					if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && References.IsCarrierVGM)
					{
						reason = GetReasonForNotAbleToUpdateFromCarrierVGMMessageWhenTargetBoIsNull();
						if (!reason.IsEmpty)
						{
							return reason;
						}
					}

					return Res.GetString("f47857b5-dbea-415b-992a-750826b619e6",
						"XML file contains {0} service code and cannot find a matched Agency Shipment.",
						ServiceCodesList.Codes.VerifiedGrossContainerWeight);
				}

				if (References.IsTargettedToBothAgentModules)
				{
					return Res.GetString("47eed6cb-e51a-4629-8e6d-75bedd89ec80", "Cannot determine which type of Agency Shipment to create.");
				}
			}

			if (!References.AgentsReference.IsEmpty && References.OceanBillNumber.IsEmpty && References.CarriersBookingReference.IsEmpty && References.BookingPartyPK.IsEmpty)
			{
				return Res.GetString("756963C8-2385-4521-9896-C8050B4E3070", "XML file does not contains any Ocean Bill Number or Carrier Booking Reference Number.The Booking Party is required when only Agent Reference Number provided.");
			}

			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && IsFromCarrier && IsElectronicBooking)
			{
				reason = GetReasonForNotAbleToUpdateFromBookingRequest(targetBo);
			}

			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && IsFromCarrier && IsShippingInstruction)
			{
				reason = GetReasonForNotAbleToUpdateFromShippingInstruction(targetBo);
			}

			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && References.IsCarrierVGM)
			{
				reason = GetReasonForNotAbleToUpdateFromCarrierVGMMessage(targetBo);
			}

			return reason.IsEmpty
				? base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBo)
				: reason;
		}

		public ZString GetReasonForNotAbleToUpdateFromShippingInstruction(T targetBO)
		{
			if (IsAmendmentMessage || IsOriginalMessage || IsWithdrawalMessage)
			{
				var bookingConfirmationReference = dataObject.BookingConfirmationReference.GetValueOrDefault();
				var bookingPartyName = dataObject.GetBookingPartyName();
				var agentsReference = dataObject.AgentsReference.GetValueOrDefault();
				var bookingPartyPK = dataObject.GetBookingParty(logger, factory);

				if (bookingConfirmationReference.IsEmpty)
				{
					return Res.GetString("66DCD7D0-3288-437A-9150-6E0BD407B5DE", "[*Shipping Instruction message received without Booking Number. Booking Number is mandatory to process the Shipping Instruction. Message rejected.*]");
				}

				if (dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress)) == null)
				{
					return Res.GetString("2D211B1B-3D86-4F00-A841-79C20217936E", "[*Shipping Instruction message received without Booking Party. Booking Party is mandatory to process the Shipping Instruction. Message rejected.*]");
				}

				if (agentsReference.IsEmpty)
				{
					return Res.GetString("CD450819-EA0F-41E6-9F1D-7AF672E9436B", "[*Shipping Instruction message received without Shipper's Reference. Shipper's Reference is mandatory to process the Shipping Instruction. Message rejected.*]");
				}

				if (bookingPartyPK.IsEmpty)
				{
					return Res.GetString("C5356EA1-03B1-419B-98B0-96ECE3F739B1", "[*Shipping Instruction message received from an unknown Booking Party {0}. Message rejected.*]", bookingPartyName);
				}

				if (targetBO == null)
				{
					return Res.GetString("5BD28840-EFDE-4FD7-BD47-D9BDF7BA29AA", "[*Shipping Instruction message with Booking Number {0} cannot find a matching Booking. Message rejected.*]", bookingConfirmationReference);
				}
				else
				{
					if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled)
					{
						return Res.GetString("FB189F84-3B8D-4A85-B3CD-FB6C0041C4F4", "[*Shipping Instruction message cannot be accepted since Booking is Canceled. Message rejected.*]");
					}

					if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
					{
						return Res.GetString("F899F083-228F-4B75-8430-74E190A4D257", "[*Shipping Instruction message cannot be accepted once Booking Withdrawal/Cancellation request is in progress. Message rejected.*]");
					}

					if (!BookingQueryHelper.IsBookingPartyNameMatched(targetBO, bookingPartyName))
					{
						return Res.GetString("419AD0EC-5AE0-403A-AE3E-59EB91A2BC71", "[*Shipping Instruction message received with an incorrect Booking Party {0}. Message rejected.*]", bookingPartyName);
					}

					if (targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.Booked && !targetBO.IsBillOfLadingStage)
					{
						return Res.GetString("7FBAAA6A-163D-485E-99FB-F6593B641F53", "[*Shipping Instruction cannot be processed, because Booking {0} is not confirmed by carrier. Message rejected.*]", bookingConfirmationReference);
					}

					if (IsAmendmentMessage)
					{
						if (References.OceanBillNumber.IsEmpty)
						{
							return Res.GetString("6D2B3FBF-A6E5-4D97-B618-5E0510E14D27", "[*Shipping Instruction Amendment message received without Bill Number. Bill Number is mandatory to process the Shipping Instruction Amendment. Message rejected.*]");
						}

						if (!targetBO.IsBillOfLadingStage)
						{
							return Res.GetString("2AFF90FB-E74D-405C-9B88-5AE83282C9F6", "[*Shipping Instruction Amendment message with Booking Number {0} cannot be processed. Shipping Instruction original message is required. Message rejected.*]", bookingConfirmationReference);
						}

						if (targetBO.JS_HouseBill != References.OceanBillNumber)
						{
							return Res.GetString("3A0EF1A8-8504-4B83-94AE-74A07C80C25A", "[*Shipping Instruction Amendment message with Bill Number {0} cannot find a matching Bill of Lading to update. Message rejected.*]", References.OceanBillNumber);
						}

						if (targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.Confirmed && targetBO.JS_ShipmentStatus != ShipmentStatusList.Codes.SIRejected)
						{
							return Res.GetString("BE83519A-2383-4A1C-AB1F-27A75784F1F1", "[*Shipping Instruction Amendment message with Bill Number {0} cannot be processed because Shipping Instruction is not confirmed. Message rejected.*]", References.OceanBillNumber);
						}
					}
				}
			}

			return ZString.Empty;
		}

		AgencyShipment[] GetRelatedBillOfLadingsForCarrierVGMMessage()
		{
			var query = new ZDBOnlyQuery(typeof(AgencyShipment));
			query.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			query.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);
			query.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());

			var subShipmentReferenceQuery = new ZDBOnlyQuery(typeof(AgencyShipment));
			if (!References.OceanBillNumber.IsEmpty)
			{
				subShipmentReferenceQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_HouseBill, References.OceanBillNumber);
			}

			if (!References.CarriersBookingReference.IsEmpty)
			{
				subShipmentReferenceQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_CFSReference, References.CarriersBookingReference);
			}

			query.AddToFilter(subShipmentReferenceQuery);

			return factory.Load<AgencyShipment>(query);
		}

		public ZString GetReasonForNotAbleToUpdateFromCarrierVGMMessageWhenTargetBoIsNull()
		{
			if (dataObject.ContainerCollection == null || !dataObject.ContainerCollection.Any() || dataObject.ContainerCollection.Any(x => string.IsNullOrEmpty(x.ContainerNumber)))
			{
				return Res.GetString("24b7bbee-54a8-4402-94c4-b8480bb40933", "[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]");
			}

			if (References.OceanBillNumber.IsEmpty && References.CarriersBookingReference.IsEmpty)
			{
				return Res.GetString("5a005461-663b-4fa1-b7cd-c75f886030bb", "[*VGM message received without Booking or Bill of Lading number. Booking or Bill of Lading Number is mandatory to process the VGM. Message rejected.*]");
			}

			var billOfLadings = GetRelatedBillOfLadingsForCarrierVGMMessage();

			if (!References.OceanBillNumber.IsEmpty && billOfLadings.Any(x => x.JS_HouseBill == References.OceanBillNumber))
			{
				return Res.GetString("3679b14a-aade-4cff-ade5-634d1a2f7d03", "[*VGM message failed because Container {0} does not exist in Bill of Lading {1}. Message rejected.*]", References.ContainerNumber, References.OceanBillNumber);
			}

			if (!References.CarriersBookingReference.IsEmpty && billOfLadings.Any(x => x.JS_CFSReference == References.CarriersBookingReference))
			{
				return Res.GetString("2e38c2f0-ca77-48f7-94cf-b5980a6110e0", "[*VGM message failed because Container {0} does not exist in Bill of Lading with Booking number {1}. Message rejected.*]", References.ContainerNumber, References.CarriersBookingReference);
			}

			return !References.OceanBillNumber.IsEmpty
				? Res.GetString("648464e9-0830-4b95-b11c-5ace0387e72f", "[*VGM message with Bill Number {0} cannot find a matching Bill of Lading to update. Message rejected.*]", References.OceanBillNumber)
				: Res.GetString("7ad31e97-b26c-4109-bb79-db94d14e1388", "[*VGM message with Booking Number {0} cannot find a matching Booking. Message rejected.*]", References.CarriersBookingReference);
		}

		public ZString GetReasonForNotAbleToUpdateFromCarrierVGMMessage(T targetBO)
		{
			if (dataObject.ContainerCollection == null || !dataObject.ContainerCollection.Any() || dataObject.ContainerCollection.Any(x => string.IsNullOrEmpty(x.ContainerNumber)))
			{
				return Res.GetString("e1943a06-3548-4bff-bf04-f9ea18cc650c", "[*VGM message received without Container Number. Container Number is mandatory to process the VGM. Message rejected.*]");
			}

			if (!targetBO.IsBillOfLadingStage)
			{
				if (References.CarriersBookingReference.IsEmpty)
				{
					if (!References.OceanBillNumber.IsEmpty)
					{
						if (GetRelatedBillOfLadingsForCarrierVGMMessage().Any(x => x.JS_HouseBill == References.OceanBillNumber))
						{
							return Res.GetString("282ebb8e-8066-4891-bf6d-38ef55e1d9c2", "[*VGM message failed because Container {0} does not exist in Bill of Lading {1}. Message rejected.*]", References.ContainerNumber, References.OceanBillNumber);
						}

						return Res.GetString("985a2418-4bf4-4190-bcfa-38cfebfcb99c", "[*VGM message with Bill Number {0} cannot find a matching Bill of Lading to update. Message rejected.*]", References.OceanBillNumber);
					}

					return Res.GetString("dbe43d7b-28d0-48a6-b13d-2e384a83f633", "[*VGM message received without Booking or Bill of Lading number. Booking or Bill of Lading Number is mandatory to process the VGM. Message rejected.*]");
				}

				var matchingContainer = targetBO.RealContainers.Find(x => x.JC_ContainerNum == References.ContainerNumber).FirstOrDefault()
					?? targetBO.BookedContainers.Find(x => x.JC_ContainerNum == References.ContainerNumber).FirstOrDefault();

				if (matchingContainer == null)
				{
					var billOfLadings = GetRelatedBillOfLadingsForCarrierVGMMessage();

					if (!References.OceanBillNumber.IsEmpty && billOfLadings.Any(x => x.JS_HouseBill == References.OceanBillNumber))
					{
						return Res.GetString("96746c8a-03cb-4681-9911-8e1712f5c885", "[*VGM message failed because Container {0} does not exist in Bill of Lading {1}. Message rejected.*]", References.ContainerNumber, References.OceanBillNumber);
					}

					if (billOfLadings.Any(x => x.JS_CFSReference == References.CarriersBookingReference))
					{
						return Res.GetString("87f34913-0db5-4948-b3c2-4e5788ac37ff", "[*VGM message failed because Container {0} does not exist in Bill of Lading with Booking number {1}. Message rejected.*]", References.ContainerNumber, References.CarriersBookingReference);
					}

					return Res.GetString("2dbc5ed2-1a24-41de-ad1c-c5ffde854cf0", "[*VGM message failed because Container {0} does not exist in Booking {1}. Message rejected.*]", References.ContainerNumber, References.CarriersBookingReference);
				}
			}

			if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.ElectronicBooking)
			{
				return Res.GetString("0102c9c6-567e-4134-bbdb-df5f9b6ba123", "[*VGM message with Booking number {0} cannot be processed because the Booking is not confirmed. Message rejected.*]", References.CarriersBookingReference);
			}

			if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled)
			{
				return Res.GetString("060e44e9-1639-4961-b222-feb776246a3d", "[*VGM message cannot be accepted since the Booking is canceled. Message rejected.*]");
			}

			if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
			{
				return Res.GetString("53f29289-b0d6-4798-ad14-355a4c7479b2", "[*VGM message cannot be accepted once Booking Withdrawal/Cancellation request is in progress. Message rejected.*]");
			}

			if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingRejected)
			{
				return Res.GetString("f85bda30-bb2b-4e50-a75e-84dbae9f7cc7", "[*VGM message cannot be accepted since the Booking is rejected. Message rejected.*]");
			}

			return GetReasonForNotAbleToUpdateFromVGMMessageContainer(targetBO);
		}

		public ZString GetReasonForNotAbleToUpdateFromVGMMessageContainer(T targetBO)
		{
			var legs = targetBO.Transports.Cast<Transport>();
			var firstSeaLeg = legs.Where(x => x.JW_TransportMode == Constants.TransportModes.Sea).OrderBy(x => x.JW_LegOrder).FirstOrDefault();
			var firstSeaLegATD = firstSeaLeg?.JW_ATD ?? ZDateTime.Empty;

			if (!firstSeaLegATD.IsEmpty && ZDateTime.Now > firstSeaLegATD)
			{
				return Res.GetString("450c579e-1c56-4e4e-8308-1249fbf920b2", "[*VGM message cannot be accepted after vessel ATD. Message rejected.*]");
			}

			foreach (var importContainer in dataObject.ContainerCollection)
			{
				if (!firstSeaLegATD.IsEmpty && importContainer.GrossWeightVerificationDateTime.HasValue && importContainer.GrossWeightVerificationDateTime.Value > firstSeaLegATD)
				{
					return Res.GetString("d6658fc5-f1fb-424c-9498-7d3129817fed", "[*VGM message cannot be accepted after vessel ATD. Message rejected.*]");
				}

				var matchingContainer = targetBO.RealContainers.Find(x => x.JC_ContainerNum.ToString() == importContainer.ContainerNumber.ToString()).FirstOrDefault()
					?? (!targetBO.IsBillOfLadingStage ? targetBO.BookedContainers.Find(x => x.JC_ContainerNum.ToString() == importContainer.ContainerNumber.ToString()).FirstOrDefault() : null);

				if (matchingContainer == null)
				{
					return !targetBO.IsBillOfLadingStage
						? Res.GetString("9d6fc2f9-bbe9-4910-bd06-8ca96c04a4a5", "[*VGM message failed because Container {0} does not exist in Booking {1}. Message rejected.*]", importContainer.ContainerNumber, targetBO.JS_CFSReference)
						: Res.GetString("7b6b692c-1111-42dc-bcd6-ce4a02a8c9ed", "[*VGM message failed because Container {0} does not exist in Bill of Lading {1}. Message rejected.*]", importContainer.ContainerNumber, targetBO.JS_HouseBill);
				}

				var vgmValidationResult = VGMHelper.ValidateVGMProperties(dataObject, shouldValidateDateTime: true);
				if(!vgmValidationResult.IsEmpty)
				{
					return vgmValidationResult;
				}

				var reason = GetReasonForNotAbleToUpdateFromVGMMessageContainerWeight(importContainer, matchingContainer);

				if (!reason.IsEmpty)
				{
					return reason;
				}
			}

			return ZString.Empty;
		}

		public ZString GetReasonForNotAbleToUpdateFromVGMMessageContainerWeight(Container importContainer, AgencyShipmentContainer matchingContainer)
		{
			if (importContainer.ContainerType != null && importContainer.ContainerType.Code.HasValue && importContainer.GrossWeight.HasValue)
			{
				var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, importContainer.ContainerType.Code.Value);
				if (refContainer != null)
				{
					var refMaxGrossWeight = refContainer.RC_GrossWeight;
					var refTareWeight = refContainer.RC_TareWeight;
					var grossWeight = importContainer.GrossWeight.Value;

					if (importContainer.WeightUnit.Code.HasValue && importContainer.WeightUnit.Code.Value != Core.Constants.Weight.Kilograms)
					{
						grossWeight = Constants.Weight.ConvertSafe(grossWeight, importContainer.WeightUnit.Code.Value, Core.Constants.Weight.Kilograms, false);
					}

					if (grossWeight < refTareWeight)
					{
						return Res.GetString("cc6247fd-2a88-4c56-99be-f07cca97a79d", "[*Container gross weight cannot be less than Container tare weight. Message rejected.*]");
					}

					if (grossWeight > refMaxGrossWeight)
					{
						return Res.GetString("9c92d427-de5b-437e-8795-feab112b20de", "[*Goods weight exceeds container maximum payload. Message rejected.*]");
					}
				}
			}

			return ZString.Empty;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Reject Reasons")]
		public ZString GetReasonForNotAbleToUpdateFromBookingRequest(T targetBO)
		{
			var bookingConfirmationReference = dataObject.BookingConfirmationReference.GetValueOrDefault();
			var bookingPartyName = dataObject.GetBookingPartyName();
			var agentsReference = dataObject.AgentsReference.GetValueOrDefault();

			if (IsOriginalMessage)
			{
				if (targetBO != null && targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked)
				{
					return Res.GetString("5BF8D8EC-53DF-49FC-BDF5-F22583771992", "[*Booking Request original cannot be processed once Booking is confirmed (Booking Status BKD). Booking Request rejected.*]");
				}

				if (targetBO != null && targetBO.IsBillOfLadingStage)
				{
					return Res.GetString("CF16926C-69C1-4615-B2F2-D1EAFBE6B81E", "[*Booking Request original message cannot be processed once Booking is converted to Bill Of Lading. Booking Request rejected.*]");
				}

				if (targetBO == null && !bookingConfirmationReference.IsEmpty)
				{
					return Res.GetString("DBF98070-59A0-48D9-8A30-BEDEAC2C9B15", "[*Booking Request message with Booking Number {0}, Shipper Reference {1} and Booking Party {2} cannot find a matching Booking to update, Booking Request rejected.*]", bookingConfirmationReference, agentsReference, bookingPartyName);
				}
			}

			if (IsOriginalMessage || IsAmendmentMessage)
			{
				var bookingPartyPK = dataObject.GetBookingParty(logger, factory);
				if (bookingPartyPK.IsEmpty)
				{
					return Res.GetString("AEBDF82A-6EBB-4337-B931-F592AB1C391F", "[*Booking Request message is received from an unknown Booking Party {0}. Booking Request rejected.*]", bookingPartyName);
				}
			}

			if (IsAmendmentMessage)
			{
				if (bookingConfirmationReference.IsEmpty)
				{
					return Res.GetString("EFC50B3D-2EAA-4E6C-A5D9-3F0A734E56AE", "[*Mandatory Booking Number is missing in Booking Request Amendment message, Booking Request Amendment rejected.*]");
				}

				if (targetBO == null)
				{
					return Res.GetString("9E8751BF-4832-4862-82F7-F5DBE5C5B20C", "[*Booking Request Amendment message with Booking Number {0}, Shipper Reference {1} and Booking Party {2} cannot find a matching Booking to update, Booking Request Amendment rejected.*]", bookingConfirmationReference, agentsReference, bookingPartyName);
				}

				if (targetBO.JS_BookingReference != agentsReference)
				{
					return Res.GetString("91A9B720-F443-4BF2-ACC0-8687955B14C4", "[*Booking Request message cannot update Booking {0} because the Shipper Reference in the message {1} is different to one in the Booking. Booking Request rejected.*]", bookingConfirmationReference, agentsReference);
				}

				if (!BookingQueryHelper.IsBookingPartyNameMatched(targetBO, bookingPartyName))
				{
					return Res.GetString("312D69E1-11F1-4CE7-89DC-EF7F377E5714", "[*Booking Request Amendment message is received with the wrong Booking Party {0}. Booking Request rejected.*]", bookingPartyName);
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.ElectronicShippingInstruction)
				{
					return Res.GetString("690F5468-8683-451F-9943-09DD977D506B", "[*Booking Request Amendment message cannot be accepted once Shipping Instruction is processed. Booking Request Amendment rejected.*]");
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled)
				{
					return Res.GetString("9D09EB58-12F7-4F73-95F5-E5627923C1CB", "[*Booking Request Amendment message cannot be accepted once Booking is Canceled. Booking Request Amendment rejected.*]");
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
				{
					return Res.GetString("ACEA426E-E030-4993-B694-BF36E597A325", "[*Booking Request Amendment message cannot be accepted once Booking Withdrawal/Cancellation Request is in progress. Booking Request Amendment rejected.*]");
				}

				if (targetBO.IsBillOfLadingStage)
				{
					return Res.GetString("FCC559D7-CFE6-4FF9-B2DC-B690A9B40BD6", "[*Booking Request Amendment message cannot be processed once Booking is converted to Bill Of Lading. Booking Request rejected.*]");
				}
			}

			if (IsWithdrawalMessage)
			{
				if (bookingConfirmationReference.IsEmpty)
				{
					return Res.GetString("B2EE02C9-EFA1-401D-A463-0D56CC307C74", "[*Mandatory Booking Number is missing in Booking Request Withdraw/Cancel message, Booking Request Withdraw rejected.*]");
				}

				if (targetBO == null)
				{
					return Res.GetString("40EA4E91-2FF5-4FF5-80F2-59A53C7AE32B", "[*Booking Request Withdraw/Cancel message with Booking Number {0}, Shipper Reference {1} and Booking Party {2} cannot find a matching Booking to update, Booking Request Withdraw rejected.*]", bookingConfirmationReference, agentsReference, bookingPartyName);
				}

				if (targetBO.JS_BookingReference != agentsReference)
				{
					return Res.GetString("B279E329-D734-45A1-905F-4868741DD74B", "[*Booking Request Withdraw/Cancel message cannot update Booking {0} because the Shipper Reference in the message {1} is different to one in the Booking. Booking Request Withdraw rejected.*]", bookingConfirmationReference, agentsReference);
				}

				if (!BookingQueryHelper.IsBookingPartyNameMatched(targetBO, bookingPartyName))
				{
					return Res.GetString("A24315EE-B6A4-45F8-919D-CAD6C48F0E26", "[*Booking Request Withdraw/Cancel message is received with the wrong Booking Party {0}. Booking Request rejected.*]", bookingPartyName);
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingRejected)
				{
					return Res.GetString("92CBB61F-2B14-446F-B0AB-D3426BA0A683", "[*Booking Request Withdraw/Cancel cannot be processed once Booking is rejected. Booking Request Withdraw rejected.*]");
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.ElectronicShippingInstruction)
				{
					return Res.GetString("60D8663C-3B52-4A42-934D-1AB3BA9B821C", "[*Booking Request Withdraw/Cancel cannot be processed once Shipping Instruction is received. Booking Request Withdraw rejected.*]");
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.Confirmed)
				{
					return Res.GetString("B304F050-FFBC-4771-9277-F9D258D38C77", "[*Booking Request Withdraw/Cancel cannot be processed once Shipping Instruction is accepted. Booking Request Withdraw rejected.*]");
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.SIRejected)
				{
					return Res.GetString("A5AA20E4-AC1A-4DD5-9BD9-8042CB802E54", "[*Booking Request Withdraw/Cancel cannot be processed once Shipping Instruction is rejected. Booking Request Withdraw rejected.*]");
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled)
				{
					return Res.GetString("2C7B4113-5E14-4A0B-850A-194F40CED9F0", "[*Booking Request Withdraw/Cancel cannot be accepted once Booking is already Canceled. Booking Request Withdraw rejected.*]");
				}

				if (targetBO.JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest)
				{
					return Res.GetString("9DDBE478-FA47-437F-BF40-8ECD397B515B", "[*Booking Request Withdraw/Cancel cannot be accepted once Booking Withdrawal/Cancellation request is already in progress. Booking Request Withdraw rejected.*]");
				}

				if (targetBO.IsBillOfLadingStage)
				{
					return Res.GetString("61406672-BA7F-44D0-B145-3174F746B098", "[*Booking Request Withdraw/Cancel cannot be processed once Booking is converted to Bill of Lading. Booking Request Withdraw rejected.*]");
				}
			}

			return ZString.Empty;
		}

		protected override IMatchingBusinessEntityFinder<T> GetCombinedReferenceMatcher()
		{
			return new AgencyShipmentMatcher<T>(factory.BOFactory, References, logger, DataContextType);
		}

		class OrganizationAddressWithAddressType
		{
			public DocAddressType AddressType { get; set; }
			public OrganizationAddress AddressData { get; set; }
		}

		protected override T GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override sealed void PopulateBusinessObject(T shipmentBizObj)
		{
			ISupportDataImporting supportDataImporting = shipmentBizObj;

			try
			{
				supportDataImporting.IsImportingData = true;
				shipmentBizObj.SuppressPackLinesUpdate = true;

				PopulateShipment(shipmentBizObj);

				if (dataObject.OrganizationAddressCollection != null)
				{
					ReadAddresses(shipmentBizObj, dataObject.OrganizationAddressCollection);
				}

				if (dataObject.DateCollection != null)
				{
					ReadDates(shipmentBizObj, dataObject.DateCollection);
				}

				if (dataObject.NoteCollection != null)
				{
					ReadNotes(shipmentBizObj, dataObject.NoteCollection);
				}

				if (dataObject.EntryNumberCollection != null)
				{
					ReadEntryNumbers(shipmentBizObj, dataObject.EntryNumberCollection);
				}

				if (dataObject.AdditionalReferenceCollection != null)
				{
					ReadAdditionalReferenceNumbers(shipmentBizObj, dataObject.AdditionalReferenceCollection);
				}

				if (dataObject.ContainerCollection != null)
				{
					ReadContainers(shipmentBizObj, dataObject.ContainerCollection);
				}

				if (dataObject.PackingLineCollection != null)
				{
					ReadPackingLines(shipmentBizObj, dataObject.PackingLineCollection);
				}

				if (dataObject.TransportLegCollection != null)
				{
					ReadTransportLegs(shipmentBizObj, dataObject.TransportLegCollection);
				}
			}
			finally
			{
				supportDataImporting.IsImportingData = false;
				shipmentBizObj.SuppressPackLinesUpdate = false;
			}
		}

		protected virtual void PopulateShipment(T shipmentBizObj)
		{
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && IsFromCarrier && (IsElectronicBooking || IsShippingInstruction))
			{
				shipmentBizObj.PurposeDescription = dataObject.DataContext?.DocumentaryOverride?.Purpose?.Description ?? ZString.Empty;
			}

			if (IsFromCarrier && IsShippingInstruction)
			{
				if (dataObject.WayBillNumber.HasValue && !dataObject.WayBillNumber.Value.IsEmpty)
				{
					SetValue(shipmentBizObj, JobShipmentSchema.JS_HouseBill, dataObject.WayBillNumber);
				}
			}
			else
			{
				SetValue(shipmentBizObj, JobShipmentSchema.JS_HouseBill, dataObject.WayBillNumber);
			}

			SetValue(shipmentBizObj, JobShipmentSchema.JS_TransportMode, dataObject.TransportMode);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_RL_NKOrigin, dataObject.PortOfOrigin);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_RL_NKDestination, dataObject.PortOfDestination);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_INCO, dataObject.ShipmentIncoTerm);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_PackingMode, dataObject.ContainerMode);

			SetValue(shipmentBizObj, JobShipmentSchema.JS_GoodsDescription, dataObject.GoodsDescription);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_ReleaseType, dataObject.ReleaseType);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_HBLAWBChargesDisplay, dataObject.HBLAWBChargesDisplay);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_ShippedOnBoard, dataObject.ShippedOnBoard);

			SetValue(shipmentBizObj, JobShipmentSchema.JS_NoCopyBills, dataObject.NoCopyBills);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_NoOriginalBills, dataObject.NoOriginalBills);

			SetValue(shipmentBizObj, JobShipmentSchema.JS_ActualVolume, dataObject.TotalVolume);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_UnitOfVolume, dataObject.TotalVolumeUnit);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_ActualWeight, dataObject.TotalWeight);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_UnitOfWeight, dataObject.TotalWeightUnit);

			SetValue(shipmentBizObj, JobShipmentSchema.JS_OuterPacks, dataObject.OuterPacks);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_F3_NKPackType, dataObject.OuterPacksPackageType);

			SetValue(shipmentBizObj, JobShipmentSchema.JS_InterimReceipt, dataObject.InterimReceiptNumber);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_RS_NKServiceLevel, dataObject.ServiceLevel);

			SetValue(shipmentBizObj, JobShipmentSchema.JS_ShipmentStatus, dataObject.ShipmentStatus);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_BookingReference, dataObject.AgentsReference);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_CFSReference, dataObject.BookingConfirmationReference);

			SetValue(shipmentBizObj, JobShipmentSchema.JS_GoodsValue, dataObject.GoodsValue);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_RX_NKGoodsValueCurr, dataObject.GoodsValueCurrency);

			SetValue(shipmentBizObj, JobShipmentSchema.JS_RL_NKPlaceOfReceipt, dataObject.PlaceOfReceipt);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_RL_NKPlaceOfDischarge, dataObject.PlaceOfDelivery);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_RL_NKHouseBillIssuePlace, dataObject.PlaceOfIssue);
			SetValue(shipmentBizObj, JobShipmentSchema.JS_INCO, dataObject.PaymentMethod);

			ReadShipmentStatus(shipmentBizObj);
		}

		void ReadDates(T shipmentBizObj, IEnumerable<Date> dateDataObjects)
		{
			foreach (var dateDataObject in dateDataObjects)
			{
				ReadDate(shipmentBizObj, dateDataObject);
			}
		}

		void ReadDate(T shipmentBizObj, Date dateDataObject)
		{
			switch (dateDataObject.Type)
			{
				case DateType.BillIssued:
					SetValue(shipmentBizObj, JobShipmentSchema.JS_HouseBillIssueDate, dateDataObject.Value);
					break;

				case DateType.ShippedOnBoard:
					SetValue(shipmentBizObj, JobShipmentSchema.JS_ShippedOnBoardDate, dateDataObject.Value);
					break;

				case DateType.BookingConfirmed:
					SetValue(shipmentBizObj, JobShipmentSchema.JS_A_BKD, dateDataObject.Value);
					break;

				case DateType.Received:
					SetValue(shipmentBizObj, JobShipmentSchema.JS_A_RCV, dateDataObject.Value);
					break;

				case DateType.Departure:
					SetValue(shipmentBizObj, JobShipmentSchema.JS_E_DEP, dateDataObject.Value);
					break;

				case DateType.Arrival:
					SetValue(shipmentBizObj, JobShipmentSchema.JS_E_ARV, dateDataObject.Value);
					break;
			}
		}

		void ReadAddresses(T shipmentBizObj, IEnumerable<OrganizationAddress> organizationAddressesData)
		{
			var addresses = organizationAddressesData
				.Select(address =>
				{
					DocAddressType addressType;

					if (!Enum.TryParse(address.AddressType.GetValueOrDefault(), out addressType))
					{
						addressType = DocAddressType.None;
					}

					return new OrganizationAddressWithAddressType
					{
						AddressType = addressType,
						AddressData = address
					};
				});

			foreach (var address in addresses)
			{
				ReadAddress(shipmentBizObj, address);
			}
		}

		void ReadAddress(T shipmentBizObj, OrganizationAddressWithAddressType address)
		{
			switch (address.AddressType)
			{
				case DocAddressType.ShippingLineAddress:
					SetAddress(shipmentBizObj, address.AddressData, JobShipmentSchema.JS_OA_BookedShippingLineAddress);
					hasCarrierBeenRead = true;
					break;

				case DocAddressType.Principal:
					SetOrganisation(shipmentBizObj, address.AddressData, JobShipmentSchema.JS_OH_DeliveryAgent);
					break;

				case DocAddressType.SendingForwarderAddress:
					SetAddress(shipmentBizObj, address.AddressData);
					if (!HasBookingParty)
					{
						address.AddressData.AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress);
						SetAddress(shipmentBizObj, address.AddressData);
					}
					break;

				case DocAddressType.LocalClient:
					var localClientAddress = new OrganisationDataObjectReader(address.AddressData, logger, factory).GetMatched();
					if (localClientAddress != null)
					{
						var errorMessage = shipmentBizObj.CreateShipmentJobHeaderWithMutex();
						var jobHeader = shipmentBizObj.ShipmentJobHeader ?? throw new DataObjectReadFailureException(string.Format(CultureInfo.InvariantCulture, "Could not save Local Client Organization - Failed to create Shipment JobHeader with mutex. Message : {0}", errorMessage));

						if (!jobHeader.IsInDatabase && jobHeader.Department == null)
						{
							jobHeader.DisposeAndDeleteNew();
							throw new DataObjectReadFailureException("Could not save Local Client Organization - Must have an Origin, Destination and Transport Mode to be able to calculate a Department for a Job Costing record, and the Local Client is saved on the Job Costing record.");
						}
						jobHeader.JH_OA_LocalChargesAddr = localClientAddress.PK;
					}
					break;

				default:
					SetAddress(shipmentBizObj, address.AddressData);
					break;
			}
		}

		void SetAddress(T shipmentBizObj, OrganizationAddress orgAddressData, OrganisationTypes? orgType = null)
		{
			var orgDataReader = new OrganisationDataObjectReader(orgAddressData, logger, factory);
			var address = orgType.HasValue ? orgDataReader.GetMatchedOrNew(shipmentBizObj, orgType.Value) : orgDataReader.GetMatchedOrNew(shipmentBizObj);

			if (address != null)
			{
				shipmentBizObj.DocAddresses.Add(address);
			}
		}

		void SetAddress(T shipmentBizObj, OrganizationAddress orgAddressData, SchemaGuidColumn addressPKColumn)
		{
			var orgDataReader = new OrganisationDataObjectReader(orgAddressData, logger, factory);
			var address = orgDataReader.GetMatched();

			if (address != null)
			{
				shipmentBizObj[addressPKColumn] = address.PK;
			}
		}

		void SetOrganisation(T shipmentBizObj, OrganizationAddress orgAddressData, SchemaGuidColumn organizationPKColumn)
		{
			var orgDataReader = new OrganisationDataObjectReader(orgAddressData, logger, factory);
			var address = orgDataReader.GetMatched();

			if (address != null)
			{
				shipmentBizObj[organizationPKColumn] = address.OA_OH;
			}
		}

		void ReadNotes(T shipmentBizObj, DataObjectList<Note> notes)
		{
			var reader = new NotesCollectionReader(notes, logger, factory, shipmentBizObj);
			reader.ReadIntoCollection();
		}

		void ReadEntryNumbers(T shipmentBizObj, IEnumerable<EntryNumber> entryNumbers)
		{
			var reader = new EntryNumberCollectionReader<CusEntryNumber>(entryNumbers.ToArray(), logger, factory, shipmentBizObj, shipmentBizObj.CusEntryNumbersForAllCountries);
			reader.ReadIntoCollection();
		}

		void ReadAdditionalReferenceNumbers(T shipmentBizObj, DataObjectList<AdditionalReference> additionalReferenceNumbers)
		{
			var reader = new ShipmentAdditionalReferenceCollectionReader<T>(additionalReferenceNumbers, logger, factory, shipmentBizObj);
			reader.ReadIntoCollection();
		}

		void ReadContainers(T shipmentBizObj, DataObjectList<Container> containers)
		{
			readStrategy.ReadContainers(shipmentBizObj, containers);
		}

		void ReadPackingLines(T shipmentBizObj, IEnumerable<PackingLine> packLines)
		{
			readStrategy.ReadPackingLines(shipmentBizObj, packLines);
		}

		void ReadTransportLegs(T shipmentBizObj, DataObjectList<TransportLeg> transportLegs)
		{
			if (hasCarrierBeenRead)
			{
				using (shipmentBizObj.SuspendDefaultingCarrierFromSailing())
				{
					ReadTransportLegsCore(shipmentBizObj, transportLegs);
				}
			}
			else
			{
				ReadTransportLegsCore(shipmentBizObj, transportLegs);
			}
		}

		protected virtual void ReadShipmentStatus(T agencyShipment)
		{
			SetValue(agencyShipment, JobShipmentSchema.JS_ShipmentStatus, dataObject.ShipmentStatus);
		}

		void ReadTransportLegsCore(T shipmentBizObj, DataObjectList<TransportLeg> transportLegs)
		{
			var reader = new TransportLegCollectionReader<Transport>(transportLegs, logger, factory, shipmentBizObj, false);
			reader.ReadIntoCollection();

			var mainLeg = shipmentBizObj.Transports
				.Cast<Transport>()
				.FirstOrDefault(leg => leg.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel && leg.JW_IsLinked);

			if (mainLeg != null)
			{
				shipmentBizObj.JS_JX = mainLeg.JW_JX;
			}
		}

		protected AgencyShipmentReferences References
		{
			get
			{
				if (references == null)
				{
					var bookingPartyPK = GetBookingParty();
					var bookingPartyName = BookingPartyDataObject?.CompanyName.GetValueOrDefault().ToUpper() ?? ZString.Empty;

					references = new AgencyShipmentReferences(dataObject, bookingPartyPK, bookingPartyName);
				}
				return references;
			}
		}
		AgencyShipmentReferences references;

		ZGuid GetBookingParty()
		{
			if (BookingPartyDataObject != null)
			{
				var bookingPartyAddress = new OrganisationDataObjectReader(BookingPartyDataObject, logger, factory).GetMatched();
				if (bookingPartyAddress != null)
				{
					return bookingPartyAddress.OA_OH;
				}
			}
			return ZGuid.Empty;
		}

		OrganizationAddress BookingPartyDataObject
		{
			get
			{
				return dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress))
					?? dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.SendingForwarderAddress));
			}
		}

		bool HasBookingParty
		{
			get
			{
				var bookingPartyAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress));
				return (bookingPartyAddress != null);
			}
		}
	}
}
