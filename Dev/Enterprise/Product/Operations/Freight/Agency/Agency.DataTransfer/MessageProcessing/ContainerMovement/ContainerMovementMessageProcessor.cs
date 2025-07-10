using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public sealed class ContainerMovementMessageProcessor
	{
		public ContainerMovementMessageProcessor(ICMMProcessingAdapter messageAdapter)
		{
			this.messageAdapter = Argument.NotNull(messageAdapter, "messageAdapter");
		}

		readonly ICMMProcessingAdapter messageAdapter;

		public CMMEmailGenerator ProcessMessage(CMMEmailGenerator emailBuilder)
		{
			emailBuilder.SetSubjectDetail(messageAdapter.MessageTypeTitle, messageAdapter.Sender.OH_FullNameTruncated);

			if (!string.IsNullOrEmpty(messageAdapter.LloydsNumber) || !string.IsNullOrEmpty(messageAdapter.VoyageNumber))
			{
				if (messageAdapter.Vessel == null)
				{
					emailBuilder.WriteVoyageHeader(null, messageAdapter.LloydsNumber, messageAdapter.VoyageNumber);
					emailBuilder.WriteWarning(Res.GetString("00cbad00-23a1-4639-9116-7ecbc4c3168a", "Could not find a vessel in the database with the Lloyds number '{0}'", messageAdapter.LloydsNumber));
					emailBuilder.WriteVoyageFooter();
				}
				else
				{
					emailBuilder.WriteVoyageHeader(messageAdapter.Vessel.RV_Name, messageAdapter.LloydsNumber, messageAdapter.VoyageNumber);

					if (messageAdapter.Voyage == null)
					{
						emailBuilder.WriteWarning(Res.GetString("f81da2af-34ae-4060-9a59-edcb713dae4a", "This vessel-voyage combination could not be found or was matched to multiple voyages. The movements will not be auto attached to a Sailing Schedule."));
					}

					emailBuilder.WriteVoyageFooter();
				}
			}

			foreach (var container in messageAdapter.Containers)
			{
				ProcessContainer(messageAdapter, emailBuilder, container);
			}

			emailBuilder.WriteSubscriptionComment(true, GetSubscriptionComment(messageAdapter.Factory, AgencyRegistry.Instance.CMMDiscrepanciesEmailGroup));
			emailBuilder.WriteSubscriptionComment(false, GetSubscriptionComment(messageAdapter.Factory, AgencyRegistry.Instance.CMMAcknowledgementEmailGroup));

			return emailBuilder;
		}

		void ProcessContainer(ICMMProcessingAdapter adapter, CMMEmailGenerator htmlBuilder, CMMMessageContainer container)
		{
			RefContainerStock stock;

			if (string.IsNullOrWhiteSpace(container.ContainerNumber))
			{
				htmlBuilder.WriteContainerHeader(Res.GetString("d8252a8a-d802-40c0-8042-1ad80e588dc0", "Missing container number"), null);
				htmlBuilder.WriteWarning(Res.GetString("96cdaf0e-3ff2-4c29-ae8d-3006e4cd81fb", "Container does not contain container number and therefore cannot be processed."));
				htmlBuilder.WriteContainerFooter();
			}
			else if ((stock = RefContainerStock.Load(adapter.Factory, container.ContainerNumber)) != null)
			{
				htmlBuilder.WriteContainerHeader(container.ContainerNumber, GetStockUrl(stock));

				if (stock.Container != null && stock.Container.RC_ISOType != container.ISOType)
				{
					htmlBuilder.WriteWarning(
						Res.GetString("a40311d7-3fbc-4168-ba8a-66d8c530dfeb", "The ISO type recorded against this container ('{0}') is different to the ISO type in the message ('{1}').",
						stock.Container.RC_ISOType, container.ISOType));
				}

				string ownerType = GetOwnerType(container.CmmEquipmentSupplier);

				if (ownerType != null && stock.R6_OwnerType.IsEmpty)
				{
					stock.R6_OwnerType = ownerType;
				}

				ProcessContainer(adapter, htmlBuilder, container, stock);
				htmlBuilder.WriteContainerFooter();
			}
			else if (adapter.CreateMissingContainers)
			{
				if (container.ContainerNumber.Length > RefContainerStockSchema.R6_ContainerNum.MaxLength)
				{
					var warningMessage = Res.GetString(
						"f9b6102b-9011-4ef0-a165-0e2aa3a3845f",
						"The container number '{0}' was too long ({1} characters when {2} characters are allowed) and therefore container was not created",
						container.ContainerNumber,
						container.ContainerNumber.Length,
						RefContainerStockSchema.R6_ContainerNum.MaxLength);

					htmlBuilder.WriteContainerHeader(container.ContainerNumber, null);
					htmlBuilder.WriteWarning(warningMessage);
				}
				else if (container.ISOType.Length > RefContainerSchema.RC_ISOType.MaxLength)
				{
					var warningMessage = Res.GetString(
						"4c550f9f-3b0f-45b1-b577-eed85c429830",
						"The container ISO type '{0}' was too long ({1} characters when {2} characters are allowed) and therefore container was not created",
						container.ISOType,
						container.ISOType.Length,
						RefContainerSchema.RC_ISOType.MaxLength);

					htmlBuilder.WriteContainerHeader(container.ContainerNumber, null);
					htmlBuilder.WriteWarning(warningMessage);
				}
				else
				{
					stock = adapter.Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = container.ContainerNumber;

					htmlBuilder.WriteContainerHeader(container.ContainerNumber, GetStockUrl(stock));
					htmlBuilder.WriteWarning(Res.GetString("fb21d9fc-de06-4ecd-81aa-ec3786061d40", "This container was not found in the container manager module and so was added."));

					var containerType = GetContainerTypeFromISOType(adapter, container.ISOType);
					if (containerType == null)
					{
						containerType = CreateContainerType(adapter, container.ISOType);

						htmlBuilder.WriteWarning(
							Res.GetString("78e5e208-b9dc-4f54-adeb-aa6bb8d4772f", "No container type found with ISO code '{0}', adding new container type '{1}'.",
							container.ISOType, containerType.RC_Code));
					}

					stock.R6_RC = containerType.PK;
					stock.R6_OwnerType = GetOwnerType(container.CmmEquipmentSupplier);

					ProcessContainer(adapter, htmlBuilder, container, stock);
				}

				htmlBuilder.WriteContainerFooter();
			}
			else
			{
				htmlBuilder.WriteContainerHeader(container.ContainerNumber, null);

				htmlBuilder.WriteWarning(
					Res.GetString("6b8322da-a3ee-4d6d-b6b2-ffd00c2091a3", "This container was not found in the container manager module and therefore ignored."));

				if (string.IsNullOrEmpty(adapter.MovementCode))
				{
					htmlBuilder.WriteWarning(
						Res.GetString("39d27157-985d-4142-94ab-f46281994307", "Unable to determine the container movement type."));

					htmlBuilder.WriteInfo(
						Res.GetString("8c6e2dba-5947-469f-9953-a929a20a03d8", "Container movement ({0:yyyy-MM-dd HH:mm}).",
						container.PositioningDateTime));
				}
				else
				{
					htmlBuilder.WriteInfo(
						Res.GetString("7bb48fde-ab54-4a53-ac8d-007226533f0a", "\"{0}\" container movement ({1:yyyy-MM-dd HH:mm}).",
						adapter.GetMovementDescription(adapter.MovementCode),
						container.PositioningDateTime));
				}

				htmlBuilder.WriteContainerFooter();
			}
		}

		void ProcessContainer(ICMMProcessingAdapter adapter, CMMEmailGenerator htmlBuilder, CMMMessageContainer container, RefContainerStock stock)
		{
			string proposedType = adapter.MovementCode;

			if (container.IsEmpty && proposedType == ContainerMovementTypes.Codes.WharfGateIn && WasReleasedFromWharf(stock, container.PositioningDateTime))
			{
				proposedType = ContainerMovementTypes.Codes.ReturnToWharf;
			}

			if (string.IsNullOrEmpty(proposedType))
			{
				WriteUnknownMovementType(htmlBuilder);
			}

			var movement = ContainerMovementHelper.FindDuplicate(stock, proposedType, container.PositioningDateTime);

			if (movement != null)
			{
				adapter.AttachMessage(movement, EDIMessage.Status.Discarded);
				WriteDuplicateMovement(htmlBuilder, adapter.GetMovementDescription(movement.E9_MovementType), container.PositioningDateTime);
			}
			else
			{
				movement = stock.Movements.AddNew();
				movement.E9_JV = adapter.Voyage != null ? adapter.Voyage.PK : ZGuid.Empty;
				movement.E9_MovementType = proposedType;
				movement.E9_ContainerIsEmpty = container.IsEmpty;
				movement.E9_MovementDate = container.PositioningDateTime;
				movement.E9_OA_Depot = adapter.SenderAddress.PK;
				movement.TransportMode = adapter.TransportMode;

				adapter.AttachMessage(movement, EDIMessage.Status.Recognised);
				WriteMovementAdded(htmlBuilder, adapter.GetMovementDescription(movement.E9_MovementType), container.PositioningDateTime);
				SyncShipmentsIfNeeded(adapter, htmlBuilder, container, movement);
				CompareWithOrUpdateFromPastVoyage(adapter, htmlBuilder, container, stock, movement);
			}
		}

		/// <summary>
		/// Attach the raw message to the provided email object
		/// </summary>
		/// <param name="email">The email to create the attachment for</param>
		/// <param name="filename">The filename to use for the attachment MIME chunk</param>
		public void AttachMessageToEmail(EmailDef email, string filename)
		{
			email.Attachments.Add(MessageAsAttachment(messageAdapter.MessageText, filename));
		}

		static string GetSubscriptionComment(BusinessObjectFactory factory, GuidRegistryItem notificationGroupRegistryItem)
		{
			const string subscriptionCommentFormat =
				"You received this email because you are in the '{0}' group and this group is configured to receive these emails. " +
				"If you do not want to receive these emails then ask your administrator to either remove you from this group or " +
				"edit the {1} registry option." +
				"";

			GlbGroup group = factory.Load<GlbGroup>(notificationGroupRegistryItem.Value);

			if (group == null)
			{
				return string.Empty;
			}
			else
			{
				IRegistryItemInternals internals = notificationGroupRegistryItem;
				return string.Format(CultureInfo.InvariantCulture, subscriptionCommentFormat, group.GG_Desc, internals.Location);
			}
		}

		public static EmailDef GenerateErrorEmail(ICMMProcessingAdapter messageAdapter, string errorMessage)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(errorMessage);
			builder.AppendLine();

			string messageSenderReference = messageAdapter.MessageSenderReference;

			if (!string.IsNullOrWhiteSpace(messageSenderReference))
			{
				builder.Append(messageSenderReference);
			}

			builder.AppendLine();
			builder.AppendFormat(CultureInfo.InvariantCulture, GetSubscriptionComment(messageAdapter.Factory, AgencyRegistry.Instance.CMMErrorEmailGroup));

			EmailDef errorEmail = new EmailDef
			{
				Subject = Res.GetString("721734ce-7d30-496a-9fc6-2ff0cba969c8", "Error Processing CODECO/COARRI Message"),
				ContentType = EmailContentTypes.PlainText,
				Body = builder.ToString()
			};

			errorEmail.Attachments.Add(MessageAsAttachment(messageAdapter.MessageText, "message.edi"));
			return errorEmail;
		}

		static void CompareWithOrUpdateFromPastVoyage(ICMMProcessingAdapter messageAdapter, CMMEmailGenerator htmlBuilder, CMMMessageContainer container, RefContainerStock stock, ContainerMovement movement)
		{
			if (CanDefaultVoyageFromPreviousMovement(messageAdapter.MovementCode) && AgencyRegistry.Instance.CMMDefaultVoyageFromMovement.Value)
			{
				JobVoyage voyageOfDesperation = CMMRelatedMovementHelper.GuessVoyageFromContainerHistory(stock, container.PositioningDateTime);

				if (voyageOfDesperation != null)
				{
					JobVoyage movementVoyage = movement.Voyage;

					if (movementVoyage == null)
					{
						WriteDefaultedVoyageFromEarlierMovement(htmlBuilder, voyageOfDesperation.JV_RV_NKVessel, voyageOfDesperation.JV_VoyageFlight);
						movement.E9_JV = voyageOfDesperation.PK;
					}
					else if (voyageOfDesperation != movementVoyage)
					{
						WriteEarlierMovementShowsDifferentVoyage(htmlBuilder, voyageOfDesperation.JV_RV_NKVessel, voyageOfDesperation.JV_VoyageFlight);
					}
				}
			}
		}

		static bool WasReleasedFromWharf(RefContainerStock stock, ZDateTime timestamp)
		{
			if (timestamp.IsEmpty)
			{
				return false;
			}
			else
			{
				string[] ignoredMovementTypes = new string[]
				{
					ContainerMovementTypes.Codes.DepotGateIn,
					ContainerMovementTypes.Codes.DepotGateOut,
				};

				ZQuery filter = new ZQuery();
				filter.AddToFilter(JobContainerMoveSchema.E9_R6, stock.PK);
				filter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.LessThan, timestamp);
				filter.AddToFilter(JobContainerMoveSchema.E9_MovementType, SQLComparisonOperator.NotEqual, ignoredMovementTypes);
				filter.OrderBy = JobContainerMoveSchema.Constants.E9_MovementDate + OrderByClause.Descending;
				var prevMovement = stock.Factory.LoadTop1<ContainerMovement>(filter);

				if (prevMovement == null)
				{
					return false;
				}

				switch (prevMovement.E9_MovementType)
				{
					case ContainerMovementTypes.Codes.WharfGateOut:
					case ContainerMovementTypes.Codes.Discharge:
						return true;

					default:
						return false;
				}
			}
		}

		void SyncShipmentsIfNeeded(ICMMProcessingAdapter adapter, CMMEmailGenerator htmlBuilder, CMMMessageContainer container, ContainerMovement movement)
		{
			ZQuery refFilter = new ZQuery();
			bool canSearch = false;

			if (!string.IsNullOrEmpty(container.BookingReference))
			{
				refFilter.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_CFSReference, container.BookingReference);
				canSearch = true;
			}

			if (!string.IsNullOrEmpty(container.BillOfLading))
			{
				ZQuery subFilter = new ZQuery();
				subFilter.AddToFilter(JobShipmentSchema.JS_HouseBill, container.BillOfLading);
				subFilter.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());

				refFilter.AddToFilter(subFilter, JoinCondition.Or);
				canSearch = true;
			}

			if (canSearch)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(refFilter);
				filter.AddToFilter(JobShipmentSchema.JS_IsShipping, true);

				AgencyShipment[] shipments = adapter.Factory.Load<AgencyShipment>(filter);

				switch (shipments.Length)
				{
					case 0:
						if (string.IsNullOrEmpty(container.BillOfLading))
						{
							htmlBuilder.WriteWarning(Res.GetString("1e4b7601-2e3e-49e8-9d6d-b5fa7a69e73f", "No shipments were found with the booking reference '{0}'.", container.BookingReference));
						}
						else if (string.IsNullOrEmpty(container.BookingReference))
						{
							htmlBuilder.WriteWarning(Res.GetString("e63ab3b2-e9f1-4b25-b685-1c8581e762d6", "No shipments were found with the bill of lading number '{0}'.", container.BillOfLading));
						}
						else
						{
							htmlBuilder.WriteWarning(Res.GetString("3e4c77b6-c959-4c0e-9f28-8c1a7f85fa97", "No shipments were found with the bill of lading number '{0}' or booking reference '{1}'.", container.BillOfLading, container.BookingReference));
						}
						break;

					case 1:
						SyncShipmentIfNeeded(adapter, htmlBuilder, container, movement, shipments[0]);
						break;

					default:
						if (string.IsNullOrEmpty(container.BillOfLading))
						{
							htmlBuilder.WriteWarning(Res.GetString("abca9676-94a1-4344-9d4b-00eaa1165715", "More than 1 shipment has the booking reference '{0}'.", container.BookingReference));
						}
						else if (string.IsNullOrEmpty(container.BookingReference))
						{
							htmlBuilder.WriteWarning(Res.GetString("3dd19b6d-b35f-4bba-8736-353c0ed81c60", "More than 1 shipment has the bill of lading number '{0}'.", container.BillOfLading));
						}
						else
						{
							htmlBuilder.WriteWarning(Res.GetString("b468cadb-9b3e-4492-ab1f-9a0199cfeecc", "More than 1 shipment has the bill of lading number '{0}' or booking reference '{1}'.", container.BillOfLading, container.BookingReference));
						}
						break;
				}
			}

			if (ContainerMovementTypes.RequiresClientAndPrincipal(movement.E9_MovementType))
			{
				if (movement.Principal != null)
				{
					htmlBuilder.WriteInfo(Res.GetString("51d8d947-9fd1-486e-8da3-fc089d6916f1", "Principal defaulted to '{0}'.", movement.Principal.OH_Code));
				}
				else if (adapter.HasRelatedJobs(container.ContainerNumber))
				{
					htmlBuilder.WriteInfo(Res.GetString("F81BE5FA-B65D-41AE-B403-6428E80E7460", "Principal override could not be defaulted, will use values from related jobs."));
				}
				else
				{
					htmlBuilder.WriteWarning(Res.GetString("AD8E3F9D-45D0-4ECD-9DD3-9553FE5024B9", "Principal override could not be defaulted and no related jobs found."));
				}

				if (movement.ResponsibleParty != null)
				{
					htmlBuilder.WriteInfo(Res.GetString("03278608-64be-4d91-b5bd-5ec40f9a155a", "Responsible Party defaulted to '{0}'.", movement.ResponsibleParty.OH_Code));
				}
				else if (adapter.HasRelatedJobs(container.ContainerNumber))
				{
					htmlBuilder.WriteInfo(Res.GetString("57FC87D4-DC37-4A18-93F2-ABE83415D699", "Responsible Party override could not be defaulted, will use value from related jobs."));
				}
				else
				{
					htmlBuilder.WriteWarning(Res.GetString("BB7CF0BA-29D1-4CF4-932A-7FDC0C5E7020", "Responsible Party override could not be defaulted and no related jobs found."));
				}
			}
		}

		void SyncShipmentIfNeeded(ICMMProcessingAdapter adapter, CMMEmailGenerator htmlBuilder, CMMMessageContainer container, ContainerMovement movement, AgencyShipment shipment)
		{
			if (ContainerMovementTypes.RequiresClientAndPrincipal(movement.E9_MovementType))
			{
				movement.E9_OH_Principal = shipment.JS_OH_DeliveryAgent;

				OrgAddress clientAddr = GetLocalClientAddr(shipment, adapter.SenderAddress.EffectiveRelatedPortCode);

				if (clientAddr != null)
				{
					movement.E9_OH_ResponsibleParty = clientAddr.OA_OH;
				}
			}

			if (shipment.IsBillOfLadingStage)
			{
				if (!string.IsNullOrEmpty(container.BillOfLading))
				{
					htmlBuilder.WriteInfo(Res.GetString("af83463c-6a1a-4c66-bd50-ca1c4d5be8c4", "Found bill '{0}' with the bill of lading number '{1}'.", shipment.JS_UniqueConsignRef, shipment.JS_HouseBill));
				}
				else
				{
					htmlBuilder.WriteInfo(Res.GetString("21ac63b0-5142-4746-950b-e64a6a3a732b", "Found bill '{0}' with the booking reference '{1}'.", shipment.JS_UniqueConsignRef, shipment.JS_CFSReference));
				}

				ValidateBill(adapter, htmlBuilder, container, shipment);
			}
			else if (adapter.UpdateBookings)
			{
				htmlBuilder.WriteInfo(Res.GetString("569c52f2-2b6d-4029-b812-90b1f4d28b9c", "Found booking '{0}' with the booking reference '{1}'.", shipment.JS_UniqueConsignRef, shipment.JS_CFSReference));
				UpdateBooking(adapter, htmlBuilder, container, movement.Stock, shipment);
			}
			else
			{
				htmlBuilder.WriteWarning(Res.GetString("28904cda-9303-4db4-8a72-f9668aa82886", "Found booking '{0}' with the booking reference '{1}', but will not update it.", shipment.JS_UniqueConsignRef, shipment.JS_CFSReference));
			}

			JobVoyage movementVoyage;

			if ((movementVoyage = movement.Voyage) != null)
			{
				if (!IsShipmentOnVoyage(shipment, movementVoyage))
				{
					htmlBuilder.WriteWarning(Res.GetString("99ce207c-b8b4-4b61-9ac3-8140dc21ec6f", "This shipment is not entered against the vessel voyage reported in this message ({0}/{1}).",
						movementVoyage.JV_RV_NKVessel, movementVoyage.JV_VoyageFlight));
				}
			}
			else if ((movementVoyage = CMMTransportLocator.FindBestGuessVoyage(shipment, adapter.PortCode, adapter.MovementCode)) != null)
			{
				movement.E9_JV = movementVoyage.PK;

				string message;

				if (shipment.IsBillOfLadingStage)
				{
					message = Res.GetString("632c60b1-5b3c-4f4f-959b-c487a2fb6f20", "Using the bill of lading's vessel voyage ({0}/{1}).",
						movementVoyage.JV_RV_NKVessel, movementVoyage.JV_VoyageFlight);
				}
				else
				{
					message = Res.GetString("055b10fc-c539-458c-a144-77534e4a135f", "Using the booking's vessel voyage ({0}/{1}).",
						movementVoyage.JV_RV_NKVessel, movementVoyage.JV_VoyageFlight);
				}

				htmlBuilder.WriteWarning(message);
			}
		}

		static void UpdateBooking(ICMMProcessingAdapter messageAdapter, CMMEmailGenerator htmlBuilder, CMMMessageContainer containerData, RefContainerStock stock, AgencyShipment booking)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobContainerSchema.JC_JS_FCLBookingOnlyLink, booking.PK);
			filter.AddToFilter(JobContainerSchema.JC_ContainerNum, containerData.ContainerNumber);
			filter.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);

			ICMMCountryProcessor processor = ContainerMovementCountryProcessorFactory.NewProcessor(htmlBuilder, messageAdapter);
			AgencyShipmentContainer container = messageAdapter.Factory.LoadTop1<AgencyShipmentContainer>(filter);

			if (container == null)
			{
				htmlBuilder.WriteInfo(Res.GetString("25199d3a-1d3b-4825-9512-0462bcae64b8", "Adding container to '{1}'.", containerData.ContainerNumber, booking.JS_UniqueConsignRef));

				container = booking.RealContainers.AddNew();
				container.JC_ContainerNum = containerData.ContainerNumber;
				container.JC_RC = stock.R6_RC;

				if (containerData.GrossWeightKG.HasValue)
				{
					container.JC_GrossWeight = containerData.GrossWeightKG.Value;
					container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
				}

				UpdateSeals(htmlBuilder, containerData, container);

				processor.UpdateContainer(containerData, container);
			}
			else
			{
				if (container.JC_RC != stock.R6_RC)
				{
					RefContainer type1 = container.Container;
					RefContainer type2 = stock.Container;

					if (type1 != null && type2 != null && type1.RC_ISOType != type2.RC_ISOType)
					{
						htmlBuilder.WriteInfo(Res.GetString("485520b0-aeb6-46f4-b1ba-05a45f67f70a", "Updating container type from '{0}' to '{1}'.", type1.RC_Code, type2.RC_Code));
						container.JC_RC = type2.PK;
					}
				}

				if (containerData.GrossWeightKG.HasValue)
				{
					decimal weight;

					if (Constants.Weight.ContainsCode(container.JC_GrossWeightUQ))
					{
						weight = Constants.Weight.Convert(containerData.GrossWeightKG.Value, Constants.Weight.Kilograms, container.JC_GrossWeightUQ);
					}
					else
					{
						weight = containerData.GrossWeightKG.Value;
						container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
					}

					if (container.JC_GrossWeight != weight)
					{
						htmlBuilder.WriteInfo(Res.GetString("06851c84-e802-444d-b4fc-a2969b08a043", "Updating container weight from {0:0.000} to {1:0.000}.", container.JC_GrossWeight, weight));
						container.JC_GrossWeight = weight;
					}
				}

				UpdateSeals(htmlBuilder, containerData, container);

				processor.UpdateContainer(containerData, container);
			}
		}

		static void ValidateBill(ICMMProcessingAdapter messageAdapter, CMMEmailGenerator htmlBuilder, CMMMessageContainer containerData, AgencyShipment bill)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobContainerSchema.JC_JS_FCLBookingOnlyLink, bill.PK);
			filter.AddToFilter(JobContainerSchema.JC_ContainerNum, containerData.ContainerNumber);
			filter.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);

			ICMMCountryProcessor processor = ContainerMovementCountryProcessorFactory.NewProcessor(htmlBuilder, messageAdapter);
			AgencyShipmentContainer container = messageAdapter.Factory.LoadTop1<AgencyShipmentContainer>(filter);

			if (container == null)
			{
				htmlBuilder.WriteWarning(Res.GetString("8188d868-d097-4a97-968d-8b4c4f00cea4", "This container does not exist on this bill."));
			}
			else
			{
				RefContainer type = container.Container;

				if (type != null && type.RC_ISOType != containerData.ISOType)
				{
					htmlBuilder.WriteWarning(Res.GetString("2eb8d694-b52e-4786-b450-e177bba044b0", "The ISO code for the container on this bill ({0}) does not match the ISO code in this message ({1}).", type.RC_ISOType, containerData.ISOType));
				}

				if (containerData.GrossWeightKG.HasValue)
				{
					decimal weight;

					if (Constants.Weight.ContainsCode(container.JC_GrossWeightUQ))
					{
						weight = Constants.Weight.Convert(containerData.GrossWeightKG.Value, Constants.Weight.Kilograms, container.JC_GrossWeightUQ);
					}
					else
					{
						weight = containerData.GrossWeightKG.Value;
					}

					if (Math.Abs(container.JC_GrossWeight - weight) < 0.001m)
					{
						htmlBuilder.WriteWarning(Res.GetString("cc3830d3-6caa-485f-8c13-e4c11f61c2a5", "The container on this bill has a weight of {0:0.000} {2} but the message claims a weight of {1:0.000} {2}.", container.JC_GrossWeight, weight, container.JC_GrossWeightUQ));
					}
				}

				ValidateSeals(htmlBuilder, containerData, container);

				processor.ValidateContainer(containerData, container);
			}
		}

		static void UpdateSeals(CMMEmailGenerator htmlBuilder, CMMMessageContainer containerData, AgencyShipmentContainer container)
		{
			const int ExistsInMessage = 0x01;
			const int ExistsInDb = 0x02;

			IDictionary<string, int> messageSeals = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			foreach (string sealNum in containerData.SealNumbers)
			{
				messageSeals[sealNum] = ExistsInMessage;
			}

			// if the message dose not contain any seals, then we should assume seal information
			// is not provided rather then that the container has no seals.
			if (messageSeals.Count > 0)
			{
				foreach (string sealNum in ExtractContainerSeals(container))
				{
					int existing;
					messageSeals.TryGetValue(sealNum, out existing);
					messageSeals[sealNum] = existing | ExistsInDb;
				}

				using (IEnumerator<ZPropertyInfo> infoEnumerator = ExtractAvailableSealFieldsInOrder(container))
				{
					foreach (KeyValuePair<string, int> pair in messageSeals)
					{
						switch (pair.Value)
						{
							case ExistsInMessage:
								if (infoEnumerator.MoveNext())
								{
									if (CheckMaximumLength(htmlBuilder, infoEnumerator.Current, pair))
									{
										infoEnumerator.Current.Value = new ZString(pair.Key);
										WriteSealNumberAdded(htmlBuilder, pair.Key);
									}
								}
								else
								{
									WriteSealNumberNotAdded(htmlBuilder, pair.Key);
								}
								break;

							case ExistsInDb:
								WriteSealNumberMissing(htmlBuilder, pair.Key);
								break;
						}
					}
				}
			}
		}

		static bool CheckMaximumLength(CMMEmailGenerator htmlBuilder, ZPropertyInfo propertyInfo, KeyValuePair<string, int> pair)
		{
			if (propertyInfo.MaxLength < pair.Key.Length)
			{
				htmlBuilder.WriteWarning(Res.GetString("2a830657-5e3a-4269-8d35-e4b570b4c2a0", $"The maximum length of '{propertyInfo.HumanReadableName}' has been exceeded, so it will not be updated. The maximum length of this property is {propertyInfo.MaxLength} characters, but {pair.Key.Length} were entered. New value: {pair.Key}"));
				return false;
			}

			return true;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison")]
		static void ValidateSeals(CMMEmailGenerator htmlBuilder, CMMMessageContainer containerData, AgencyShipmentContainer container)
		{
			List<string> messageSeals = new List<string>(10);
			messageSeals.AddRange(containerData.SealNumbers);

			// if the message dose not contain any seals, then we should assume seal information
			// is not provided rather then that the container has no seals.
			if (messageSeals.Count > 0)
			{
				messageSeals = messageSeals.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(s => s).ToList();
				List<string> dbSeals = ExtractContainerSeals(container).Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(s => s).ToList();

				if (!ListsEqual(messageSeals, dbSeals))
				{
					htmlBuilder.WriteWarning(Res.GetString(
						"4f795b97-692e-47c0-800b-b6bfd203db42",
						"The seal numbers in the message ({0}) and the seal numbers in the database ({1}) are not an exact match.",
						string.Join(", ", messageSeals.ToArray()),
						string.Join(", ", dbSeals.ToArray())
						));
				}
			}
		}

		static string GetStockUrl(RefContainerStock stock)
		{
			return ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
		}

		OrgAddress GetLocalClientAddr(AgencyShipment shipment, RefUNLOCO port)
		{
			JobHeader header;

			if (port != null && (header = FindJobHeader(shipment, port.RL_Code)) != null)
			{
				return header.LocalChargesAddr;
			}
			else
			{
				return null;
			}
		}

		JobHeader FindJobHeader(AgencyShipment shipment, string port)
		{
			if (port == null || port.Length != 5)
			{
				return null;
			}
			else
			{
				ZDBOnlySubQuery relatedFilter = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_GB);
				relatedFilter.AddToFilter(GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, port);

				ZDBOnlySubQuery branchFilter = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
				branchFilter.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, port);
				branchFilter.AddSubQuery(relatedFilter, JoinCondition.Or);

				ZDBOnlySubQuery companyFilter = new ZDBOnlySubQuery(typeof(GlbCompany), JobHeaderSchema.JH_GC);
				companyFilter.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, port.Substring(0, 2));

				ZDBOnlyQuery headerFilter = new ZDBOnlyQuery(typeof(JobHeader));
				headerFilter.AddSubQuery(companyFilter, JoinCondition.Or);
				headerFilter.AddSubQuery(branchFilter, JoinCondition.Or);
				headerFilter.AddToFilter(JobHeaderSchema.JH_OA_LocalChargesAddr, SQLComparisonOperator.NotEqual, null);
				headerFilter.AddToFilter(JobHeaderSchema.JH_ParentID, shipment.PK);

				JobHeader[] header = shipment.Factory.Load<JobHeader>(headerFilter);

				return header.Length == 0 ? null : header[0];
			}
		}

		static AttachmentDef MessageAsAttachment(string messageText, string filename)
		{
			byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(messageText);
			return new AttachmentDef(filename, contentBytes);
		}

		RefContainer GetContainerTypeFromISOType(ICMMProcessingAdapter adapter, string containerIsoType)
		{
			return adapter.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, (ZString)containerIsoType));
		}

		RefContainer CreateContainerType(ICMMProcessingAdapter adapter, string isoType)
		{
			adapter.Factory.AddFetchHint(RefContainerSchema.Instance, new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.StartsWith, isoType));

			int attempt = 0;
			string code = isoType;

			while (adapter.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, code) != null)
			{
				code = isoType + "_" + (++attempt);
			}

			RefContainer type;
			type = adapter.Factory.New<RefContainer>();
			type.RC_Code = code;
			type.RC_ISOType = isoType;
			return type;
		}

		static List<string> ExtractContainerSeals(AgencyShipmentContainer container)
		{
			List<string> dbSeals = new List<string>(3);

			if (!container.JC_SealNum.IsEmpty)
			{
				dbSeals.Add(container.JC_SealNum);
			}

			if (!container.JC_AdditionalSealNum.IsEmpty)
			{
				dbSeals.Add(container.JC_AdditionalSealNum);
			}

			if (!container.JC_Additional2SealNum.IsEmpty)
			{
				dbSeals.Add(container.JC_Additional2SealNum);
			}

			return dbSeals;
		}

		static IEnumerator<ZPropertyInfo> ExtractAvailableSealFieldsInOrder(AgencyShipmentContainer container)
		{
			if (container.JC_SealNum.IsEmpty)
			{
				yield return container.JC_SealNumInfo;
			}

			if (container.JC_AdditionalSealNum.IsEmpty)
			{
				yield return container.JC_AdditionalSealNumInfo;
			}

			if (container.JC_Additional2SealNum.IsEmpty)
			{
				yield return container.JC_Additional2SealNumInfo;
			}
		}

		static bool IsShipmentOnVoyage(AgencyShipment shipment, JobVoyage voyage)
		{
			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}

			if (voyage == null)
			{
				throw new ArgumentNullException(nameof(voyage));
			}

			foreach (Transport transport in shipment.Transports)
			{
				JobVoyage transportVoyage = transport.Voyage;

				if (transportVoyage != null && transportVoyage.PK == voyage.PK)
				{
					return true;
				}
			}

			return false;
		}

		static string GetOwnerType(CMMEquipmentSupplier supplier)
		{
			switch (supplier)
			{
				case CMMEquipmentSupplier.Carrier:
					return Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
				case CMMEquipmentSupplier.Shipper:
					return Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
				default:
					return null;
			}
		}

		static bool CanDefaultVoyageFromPreviousMovement(string movementType)
		{
			switch (movementType)
			{
				case ContainerMovementTypes.Codes.WharfGateIn:
				case ContainerMovementTypes.Codes.WharfGateOut:
				case ContainerMovementTypes.Codes.DepotGateIn:
				case ContainerMovementTypes.Codes.DepotGateOut:
				case ContainerMovementTypes.Codes.YardGateIn:
					return true;

				default:
					return false;
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison")]
		static bool ListsEqual(List<string> dbSeals, List<string> messageSeals)
		{
			if (dbSeals.Count != messageSeals.Count)
			{
				return false;
			}

			for (int i = 0; i < dbSeals.Count; i++)
			{
				if (!dbSeals[i].Equals(messageSeals[i], StringComparison.InvariantCulture))
				{
					return false;
				}
			}

			return true;
		}

		static void WriteUnknownMovementType(CMMEmailGenerator htmlBuilder)
		{
			htmlBuilder.WriteWarning(Res.GetString("39d27157-985d-4142-94ab-f46281994307", "Unable to determine the container movement type."));
		}

		static void WriteDuplicateMovement(CMMEmailGenerator htmlBuilder, string movementType, ZDateTime movementDate)
		{
			htmlBuilder.WriteWarning(
				Res.GetString("f4bac069-ac31-46c8-a3b7-c792378255f7", "This \"{0}\" movement ({1:yyyy-MM-dd HH:mm}) appears to be a duplicate of an already logged movement, discarding.", movementType, movementDate)
				);
		}

		static void WriteMovementAdded(CMMEmailGenerator htmlBuilder, string movementType, ZDateTime movementDate)
		{
			if (string.IsNullOrEmpty(movementType))
			{
				htmlBuilder.WriteInfo(
					Res.GetString("014be1b0-e71c-4330-ba7e-b9c8933ebbe5", "Container movement added ({0:yyyy-MM-dd HH:mm}).",
					movementDate));
			}
			else
			{
				htmlBuilder.WriteInfo(
					Res.GetString("cdf85ee2-f797-4a43-9a70-a67c5eadd96f", "\"{0}\" container movement added ({1:yyyy-MM-dd HH:mm}).",
					movementType,
					movementDate));
			}
		}

		static void WriteDefaultedVoyageFromEarlierMovement(CMMEmailGenerator htmlBuilder, string vessel, string voyage)
		{
			htmlBuilder.WriteWarning(Res.GetString("689394c4-8d58-4359-b200-45df8e1aefea",
				"Was unable to determine the voyage from the message and therefore defaulted it to {0}/{1} from an earlier movement.",
				vessel,
				voyage));
		}

		static void WriteEarlierMovementShowsDifferentVoyage(CMMEmailGenerator htmlBuilder, string vessel, string voyage)
		{
			htmlBuilder.WriteWarning(Res.GetString(
				"ec7abfed-603f-4d7a-b7ff-044e56ea6740",
				"An earlier movement seems to indicate that this movement relates to a voyage other than what was reported by the message ({0}/{1}).",
				vessel,
				voyage));
		}

		static void WriteSealNumberAdded(CMMEmailGenerator htmlBuilder, string sealNum)
		{
			htmlBuilder.WriteInfo(Res.GetString("2aaee67f-ad12-4756-b6e6-b6526a484106", "Added the seal number '{0}'", sealNum));
		}

		static void WriteSealNumberMissing(CMMEmailGenerator htmlBuilder, string sealNum)
		{
			htmlBuilder.WriteWarning(Res.GetString("2b87a315-a3f2-457d-b90d-2de84bdc7423", "This container is recorded as having the seal '{0}', but that seal was not mentioned in the message.", sealNum));
		}

		static void WriteSealNumberNotAdded(CMMEmailGenerator htmlBuilder, string sealNum)
		{
			htmlBuilder.WriteWarning(Res.GetString("1eeaaeb7-be7a-4da2-9cde-b0efc73c6799", "Unable to add the seal number '{0}' as this container already has 3 seal numbers entered.", sealNum));
		}
	}
}


