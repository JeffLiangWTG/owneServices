using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class ACEAMSMessageBlockBuilder : ACECommonMessageBlockBuilder
	{
		public ACEAMSMessageBlockBuilder(IACEBillManifestMessageAttachee attachee, ActionCode actionCode)
			: base(attachee)
		{
			this.actionCode = actionCode;
		}

		protected override IEnumerable<MessageBlock> BuildCore()
		{
			var messageBlocks = new List<MessageBlock>();
			messageBlocks.AddRange(base.BuildCore());
			AddBillOfLadingDetailsRecords(messageBlocks, attachee.BillOfLadingDetails);
			return messageBlocks;
		}

		protected override bool IsUniqueVoyageIdentifierAllowed
		{
			get { return actionCode == ActionCode.Creating; }
		}

		void AddBillOfLadingDetailsRecords(List<MessageBlock> messageBlocks, IACEBillOfLading billOfLading)
		{
			messageBlocks.Add(new INPJ01() { IssuerCode = billOfLading.IssuerCode });
			AddBillOfLading(messageBlocks, billOfLading);
		}

		void AddBillOfLading(List<MessageBlock> messageBlocks, IACEBillOfLading billOfLading)
		{
			if (ActionCodeTool.IsAmendingType(actionCode))
			{
				AddAmendingDetail(messageBlocks, billOfLading);
			}

			var additionalBillOfLadingDetails = (actionCode == ActionCode.Creating || ((actionCode == ActionCode.AmendingAdd || actionCode == ActionCode.AmendingUpdate) && billOfLading.BillActionCode == AMSBillSendingActionCodeList.Codes.AddBill));
			var isInBondAction = actionCode == ActionCode.SubsequentInBondOriginal || actionCode == ActionCode.SubsequentInBondAmendment || actionCode == ActionCode.SubsequentInBondDelete;
			if (additionalBillOfLadingDetails || isInBondAction)
			{
				if (additionalBillOfLadingDetails)
				{
					messageBlocks.Add(new INPB01()
					{
						BillOfLadingSequenceNumber = billOfLading.BillOfLadingSequenceNumber,
						ForeignPort = billOfLading.ForeignPort,
						ManifestQuantity = billOfLading.ManifestQuantity,
						ManifestUnitCode = billOfLading.ManifestUnits,
						Weight = billOfLading.Weight,
						WeightUnit = billOfLading.WeightUnit,
						BillOfLadingStatusIndicator = billOfLading.BillOfLadingStatusIndicator,
						MasterInbondIndicator = billOfLading.IsMasterInbond ? "1" : ""
					});

					messageBlocks.Add(new INPB02()
					{
						Volume = billOfLading.Volume,
						VolumeUnit = billOfLading.Volume != ZDecimal.Zero ? billOfLading.VolumeUnit : ZString.Empty,
						PlaceOfReceiptByCarrier = billOfLading.PlaceOfReceiptByCarrier,
						SecondNotifyParty1 = billOfLading.SecondNotifyParty1,
						SecondNotifyParty2 = billOfLading.SecondNotifyParty2,
						LastForeignPortBeforeDepartingForTheUS = billOfLading.LastForeignPortBeforeDepartingForTheUS,
						ModeOfTransportationFromThePlacePriorToLoading = billOfLading.ModeOfTransportationFromThePlacePriorToLoading,
						MethodOfPaymentForTransportation = billOfLading.MethodOfPaymentForTransportation,
						FirstForeignPort = billOfLading.ContractualPossessionForeignPort
					});
				}
				if (isInBondAction)
				{
					AddSubsequentInBondDetail(messageBlocks, billOfLading);
				}
				if (!isInBondAction || billOfLading.BillActionCode != AMSBillSendingActionCodeList.Codes.CancelSubInBond)
				{
					AddShipmentReferenceDetails(messageBlocks, billOfLading.ShipmentReferenceDetails(actionCode));
					AddEntities(messageBlocks, billOfLading.Entities(((actionCode == ActionCode.AmendingAdd || actionCode == ActionCode.AmendingUpdate) && billOfLading.BillActionCode == AMSBillSendingActionCodeList.Codes.AddBill) ? ActionCode.AmendingAdd : actionCode));
				}
				if (isInBondAction || ((billOfLading.IsMasterInbond || billOfLading.BillOfLadingStatusIndicator == BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF) && (actionCode == ActionCode.Creating || actionCode == ActionCode.AmendingAdd || actionCode == ActionCode.AmendingUpdate)))
				{
					AddInBondDetail(messageBlocks, billOfLading);
				}
				if (additionalBillOfLadingDetails)
				{
					AddContainers(messageBlocks, billOfLading.Containers);
				}
			}
		}

		void AddAmendingDetail(List<MessageBlock> messageBlocks, IACEBillOfLading billOfLading)
		{
			var a01 = new INPA01()
			{
				CarrierCode = attachee.CarrierCode,
				CBPPort = attachee.PortDetails != null ? attachee.PortDetails.DistrictPortOfUnladingCode : ZString.Empty,
				ActionCode = billOfLading.BillActionCode,
				BillOfLadingSequenceNumber = billOfLading.BillOfLadingSequenceNumber,
				AmendmentCode = billOfLading.AmendmentCode
			};

			if (billOfLading.BillActionCode == AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity)
			{
				a01.Quantity = billOfLading.ManifestQuantity;
			}
			if (billOfLading.BillActionCode == AMSBillSendingActionCodeList.Codes.AddNewCBPBroker ||
				billOfLading.BillActionCode == AMSBillSendingActionCodeList.Codes.Add2ndNotifyParty)
			{
				a01.CarrierCode = billOfLading.SecondNotifyParty2;
			}
			messageBlocks.Add(a01);
		}

		void AddContainers(List<MessageBlock> messageBlocks, IEnumerable<IACEContainer> containers)
		{
			if (containers != null)
			{
				foreach (var container in containers)
				{
					messageBlocks.Add(new INPC01()
					{
						ContainerEquipmentNumber = container.ContainerEquipmentNo,
						SealNumber1 = container.SealNumber1,
						SealNumber2 = container.SealNumber2,
						ContainerEquipmentDescriptionCode = container.ContainerEquipmentDescriptionCode,
						ContainerEquipmentLength = container.ContainerEquipmentLength.ToString(),
						Height = container.Height,
						Width = container.Width,
						ContainerEquipmentType = container.ContainerEquipmentType,
						LoadEmptyStatusCode = container.LoadEmptyStatusCode,
						TypeOfServiceCode = container.TypeOfServiceCode
					});

					AddVehicleDetails(messageBlocks, container.VehicleDetails);

					AddCommodities(messageBlocks, container.Commondities);
					AddHazardousMaterials(messageBlocks, container.HazardousMaterials);
				}
			}
		}

		void AddCommodities(List<MessageBlock> messageBlocks, IEnumerable<ICargoDescription> commodities)
		{
			if (commodities != null)
			{
				foreach (var commodity in commodities)
				{
					if (HasD00Data(commodity))
					{
						messageBlocks.Add(new INPD00()
						{
							HarmonizedNumber = commodity.HarmonizedNumber,
							Value = commodity.Value,
							Weight = commodity.Weight,
							WeightUnit = commodity.WeightUnit
						});
					}
					AddCargoDescriptions(messageBlocks, commodity);
					AddMarksAndNumbers(messageBlocks, commodity.MarksAndNumbers);
				}
			}
		}

		bool HasD00Data(ICargoDescription commodity)
			=> !commodity.HarmonizedNumber.IsEmpty
				|| !commodity.Value.IsEmpty
				|| !commodity.Weight.IsEmpty
				|| !commodity.WeightUnit.IsEmpty;

		void AddHazardousMaterials(List<MessageBlock> messageBlocks, IEnumerable<IHazardousMaterial> hazardousMaterials)
		{
			if (hazardousMaterials != null)
			{
				foreach (var hazardousMaterial in hazardousMaterials)
				{
					messageBlocks.Add(new INPV01()
					{
						HazardousMaterialCode = hazardousMaterial.HazMatCode,
						HazardousMaterialClass = hazardousMaterial.HazMatClass,
						HazardousMaterialCodeQualifier = hazardousMaterial.HazMatQualifier,
						HazardousMaterialDescription = hazardousMaterial.HazMatDesc.Left(30),
						HazardousMaterialContact = hazardousMaterial.ContactName.Left(24)
					});
					AddHazardousMaterialTemperature(messageBlocks, hazardousMaterial);
					AddHazardousMaterialDescriptionAndClassification(messageBlocks, hazardousMaterial);
				}
			}
		}

		void AddHazardousMaterialDescriptionAndClassification(List<MessageBlock> messageBlocks, IHazardousMaterial hazardousMaterial)
		{
			var descriptionSplitValues = hazardousMaterial.HazMatDesc.SubstringSafe(30).Split(30);
			var classificationSplitValues = hazardousMaterial.HazMatClassificationDesc.Split(30);
			var noOfDescriptions = descriptionSplitValues.Length;
			var noOfClassification = classificationSplitValues.Length;
			if (noOfDescriptions > 0 || noOfClassification > 0)
			{
				for (var i = 0; i < Math.Min(2, Math.Max(noOfDescriptions, noOfClassification)); i++)
				{
					var description = i < noOfDescriptions ? descriptionSplitValues[i] : ZString.Empty;
					var classification = i < noOfClassification ? classificationSplitValues[i] : ZString.Empty;
					if (!description.IsEmpty || !classification.IsEmpty)
					{
						AddV03Record(messageBlocks, description, classification);
					}
				}
			}
			else
			{
				AddV03Record(messageBlocks, ZString.Empty, ZString.Empty);
			}
		}

		void AddV03Record(List<MessageBlock> messageBlocks, ZString description, ZString classification)
		{
			messageBlocks.Add(new INPV03()
			{
				HazardousMaterialDescription = description,
				HazardousMaterialClassification = classification
			});
		}

		void AddHazardousMaterialTemperature(List<MessageBlock> messageBlocks, IHazardousMaterial hazardousMaterial)
		{
			ZInt temperature = Math.Abs(hazardousMaterial.FlashPointTemp.ToZInt());
			var uQ = !temperature.IsEmpty ? "CE" : "";

			messageBlocks.Add(new INPV02()
			{
				FlashpointTemperature = temperature,
				UnitOfMeasureCode = uQ,
				NegativeIndicator = hazardousMaterial.FlashPointTemp < ZDecimal.Zero ? "N" : ""
			});
		}

		void AddMarksAndNumbers(List<MessageBlock> messageBlocks, ZString marksAndNumbers)
		{
			var splitValues = marksAndNumbers.Split(45);
			foreach (var markAndNumber in splitValues)
			{
				messageBlocks.Add(new INPD02()
				{
					MarksAndNumbers = markAndNumber
				});
			}
		}

		void AddCargoDescriptions(List<MessageBlock> messageBlocks, ICargoDescription cargoDescription)
		{
			if (cargoDescription != null)
			{
				var descriptions = cargoDescription.Description.Split(45);
				var description = descriptions.Length > 0 ? descriptions[0] : ZString.Empty;
				messageBlocks.Add(new INPD01()
				{
					PieceCount = cargoDescription.PieceCount,
					Description = description,
					ManifestUnitCode = cargoDescription.ManifestUnitCode
				});
				for (var i = 1; i < descriptions.Length; i++)
				{
					messageBlocks.Add(new INPD01()
					{
						Description = descriptions[i]
					});
				}
			}
		}

		void AddVehicleDetails(List<MessageBlock> messageBlocks, IEnumerable<IVehicleDetails> vehicleDetails)
		{
			foreach (var vehicle in vehicleDetails)
			{
				if (!vehicle.VIN.IsEmpty)
				{
					messageBlocks.Add(new INPC02()
					{
						VIN = vehicle.VIN
					});
				}
			}
		}

		void AddSubsequentInBondDetail(List<MessageBlock> messageBlocks, IACEBillOfLading billOfLading)
		{
			var movemenDetails = billOfLading.MovemenDetails;
			if (movemenDetails != null)
			{
				messageBlocks.Add(new SIAB03()
				{
					BillOfLadingSequenceNumber = billOfLading.BillOfLadingSequenceNumber,
					PreviousInbondNumber = movemenDetails.PreviousInBondNumber,
					InbondQuantity = (ZDecimal)movemenDetails.InBondQuantity,
					SecondaryNotifyParty1 = billOfLading.SecondNotifyParty1,
					SecondaryNotifyParty2 = billOfLading.SecondNotifyParty2
				});
			}
		}

		void AddInBondDetail(List<MessageBlock> messageBlocks, IACEBillOfLading billOfLading)
		{
			var movemenDetails = billOfLading.MovemenDetails;
			if (movemenDetails != null)
			{
				var entryType = movemenDetails.InbondEntryType;
				var inpi01 = new INPI01()
				{
					InbondEntryType = entryType,
					BTAFDAIndicator = movemenDetails.IsBTAFDA ? "Y" : "N",
					ConventionalInbondNumber = movemenDetails.ConventionalInbondNumber,
					InbondCarrierCode = movemenDetails.InbondCarrierCode,
					USPortOfDestination = movemenDetails.USPortOfDestination,
					ForeignDestination = movemenDetails.ForeignDestination,
					Value = movemenDetails.Value,
					BondedCarrierID = movemenDetails.BondedCarrierID,
					PaperlessInbondNumber = movemenDetails.PaperlessInbondNumber
				};
				if (inpi01.ConventionalInbondNumber.IsEmpty && inpi01.PaperlessInbondNumber.IsEmpty)
				{
					inpi01.ConventionalInbondNumber = AMSEDIMessage.InBondNumberPlaceHolder;
				}
				messageBlocks.Add(inpi01);
				if (billOfLading.BillActionCode != AMSBillSendingActionCodeList.Codes.CancelSubInBond && (entryType == InbondCommonTypeList.Codes._2TransportandExport || entryType == InbondCommonTypeList.Codes._3ImmediateExport))
				{
					var vesselName = movemenDetails.ExportVesselName;
					if (!vesselName.IsEmpty)
					{
						messageBlocks.Add(new INPI02()
						{
							ModeOfTransportationCode = TransportModeCodes.Codes.VesselNonContainer,
							VesselName = vesselName
						});
					}
				}
			}
		}

		void AddEntities(List<MessageBlock> messageBlocks, IEnumerable<IEntity> entities)
		{
			if (entities != null)
			{
				foreach (var entity in entities)
				{
					if (actionCode == ActionCode.SubsequentInBondOriginal && entity.EntityCode != EntityIDCodeList.Codes.SecondaryNotifyParty)
					{
						continue;
					}
					AddEntity(messageBlocks, entity);
				}
			}
		}

		void AddEntity(List<MessageBlock> messageBlocks, IEntity entity)
		{
			messageBlocks.Add(new INPN00()
			{
				EntityCode = entity.EntityCode,
				EntityName = entity.EntityName,
				CodeQualifier = entity.CodeQualifier,
				IDCode = entity.IDCode
			});
			AddN02(messageBlocks, entity.AddressLine1, entity.AddressLine1Part2);
			AddN02(messageBlocks, entity.AddressLine2, entity.AddressLine2Part2);
			var cityName = entity.CityName;
			var stateProvince = entity.StateProvince;
			var postalCode = entity.PostalCode;
			var countryCode = entity.CountryCode;
			if (!cityName.IsEmpty || !stateProvince.IsEmpty || !postalCode.IsEmpty || !countryCode.IsEmpty)
			{
				messageBlocks.Add(new INPN03()
				{
					CityName = cityName,
					StateProvinceCode = stateProvince,
					PostalCode = postalCode,
					CountryCode = countryCode
				});
			}
			AddAdminContact(messageBlocks, entity.AdminContact);
		}

		void AddAdminContact(List<MessageBlock> messageBlocks, INotifyPartyContact notifyPartyContact)
		{
			if (notifyPartyContact != null)
			{
				var contactName = notifyPartyContact.ContactName;
				var number1 = notifyPartyContact.CommunicationsNumber;
				var number2 = notifyPartyContact.CommunicationsNumber2;
				if (!contactName.IsEmpty || !number1.IsEmpty || !number2.IsEmpty)
				{
					messageBlocks.Add(new INPN04()
					{
						ContactName = notifyPartyContact.ContactName,
						CommNumberQualifier = notifyPartyContact.CommNumberQualifier,
						CommunicationsNumber = notifyPartyContact.CommunicationsNumber,
						CommNumberQualifier1 = notifyPartyContact.CommNumberQualifier2,
						CommunicationsNumber1 = notifyPartyContact.CommunicationsNumber2
					});
				}
			}
		}

		void AddN02(List<MessageBlock> messageBlocks, ZString addressLine1, ZString addressLine2)
		{
			if (!addressLine1.IsEmpty || !addressLine2.IsEmpty)
			{
				messageBlocks.Add(new INPN02()
				{
					EntitysAddressLine = addressLine1,
					EntitysAddressLine1 = addressLine2
				});
			}
		}

		void AddShipmentReferenceDetails(List<MessageBlock> messageBlocks, IEnumerable<IShipmentReferenceDetail> shipmentReferenceDetails)
		{
			if (shipmentReferenceDetails != null)
			{
				foreach (var shipmentReferenceDetail in shipmentReferenceDetails)
				{
					messageBlocks.Add(new INPB04()
					{
						ReferenceIdentifierQualifier = shipmentReferenceDetail.Qualifier,
						ReferenceIdentifier = shipmentReferenceDetail.ReferenceIdentifier
					});
				}
			}
		}

		readonly ActionCode actionCode;
	}
}
