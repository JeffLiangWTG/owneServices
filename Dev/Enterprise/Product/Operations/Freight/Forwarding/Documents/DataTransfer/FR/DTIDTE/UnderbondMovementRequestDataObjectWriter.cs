using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class UnderbondMovementRequestDataObjectWriter : DataObjectWriter<UnderbondMovementRequest, UniversalShipment>
	{
		public UnderbondMovementRequestDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(UnderbondMovementRequest request)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = request.CreateUXmlDataContext();

			shipment.BookingConfirmationReference = request.CarrierBookingReference;
			shipment.ContainerMode = request.ContainerMode?.ToUXmlContainerMode();
			shipment.ShipmentType = request.ShipmentType?.ToUXmlCodeDescriptionPair();

			shipment.PortOfDestination = request.PortOfDestination.ToUXmlUnloco();
			shipment.PortOfOrigin = request.PortOfOrigin.ToUXmlUnloco();
			shipment.PortFirstForeign = request.PortOfTranshipment.ToUXmlUnloco();

			shipment.VesselName = request.VesselName;
			shipment.VoyageFlightNo = request.VoyageFlightNo;
			shipment.WayBillNumber = request.BillOfLading;

			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = CreateAdditionalReferences(request).ToArray();

				return additionalReferences.Any() ? new DataObjectList<AdditionalReference>(additionalReferences) : null;
			});

			shipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfosRoot(request).ToList();

				return addInfos.Any() ? addInfos : null;
			});

			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(request).ToList();

				return addresses.Any() ? addresses : null;
			});

			shipment.SetNoteCollection(() =>
			{
				var notes = CreateNotes(request).ToArray();

				return notes.Any() ? new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>(notes) : null;
			});

			shipment.SetContainerCollection(() =>
			{
				var containers = CreateContainers(request).ToArray();

				return containers.Any() ? new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>(containers) : null;
			});

			return shipment;
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.Container> CreateContainers(UnderbondMovementRequest request)
		{
			if (request.Containers.Any())
			{
				foreach (var container in request.Containers)
				{
					var result = new UniversalDataBuss.DataObjects.Universal.Container(writeManager.WriterStrategy);
					result.SetAddInfoCollection(() =>
					{
						var addInfos = CreateContainerAddInfos(container).ToList();
						return addInfos.Any() ? addInfos : null;
					});
					result.ContainerNumber = container.Number;
					result.ContainerType = container.ContainerType.ToUXmlContainerType();
					result.IsEmptyContainer = container.IsEmptyContainer;
					result.NonOperatingReefer = container.IsNonOperativeReefer;
					yield return result;
				}
			}
		}

		static IEnumerable<AddInfo> CreateContainerAddInfos(UnderbondMovementRequestContainer container)
		{
			if (!container.ECTICTNumber.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.ECTICTNumber),
					Value = container.ECTICTNumber
				};
			}
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.Note> CreateNotes(UnderbondMovementRequest request)
		{
			if (!request.Notes.IsEmpty)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = (NoResString)"Additional Instruction Notes", // Programatic constant
					NoteText = request.Notes
				};
			}
		}

		IEnumerable<OrganizationAddress> CreateAddresses(UnderbondMovementRequest request)
		{
			if (!request.Carrier.IsEmpty())
			{
				yield return request.Carrier.ToUXmlOrganizationAddress("ShippingLineAddress", writeManager.WriterStrategy, CreateRegistrationNumbers(request.CarrierCI5, request.CarrierSON, request.CarrierCCC)); // Programatic constant
			}

			if (!request.SendingForwarder.IsEmpty())
			{
				yield return request.SendingForwarder.ToUXmlOrganizationAddress("SendingForwarderAddress", writeManager.WriterStrategy, CreateRegistrationNumbers(request.SendingForwarderCI5, request.SendingForwarderSON)); // Programatic constant
			}

			if (!request.ReceivingForwarder.IsEmpty())
			{
				yield return request.ReceivingForwarder.ToUXmlOrganizationAddress("ReceivingForwarderAddress", writeManager.WriterStrategy, CreateRegistrationNumbers(request.ReceivingForwarderCI5, request.ReceivingForwarderSON)); // Programatic constant
			}

			if (!request.Transporter.IsEmpty())
			{
				yield return request.Transporter.ToUXmlOrganizationAddress(request.DTI ? "ArrivalCFSLocalTransportAddress" : "DepartureCFSLocalTransportAddress", writeManager.WriterStrategy, CreateRegistrationNumbers(request.TransporterCI5, request.TransporterSON)); // Programatic constant
			}

			if (!request.SendingParty.IsEmpty())
			{
				yield return request.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(request.SendingPartyCI5, request.SendingPartySON));// Programatic constant
			}
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => x != null && !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(UnderbondMovementRequest request)
		{
			if (!request.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = request.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		IEnumerable<AddInfo> CreateAddInfosRoot(UnderbondMovementRequest request)
		{
			if (!request.OperationalPort.IsEmpty())
			{
				foreach (var addInfo in request.OperationalPort.ToUXmlAddInfos(nameof(request.OperationalPort)))
				{
					yield return addInfo;
				}
			}

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programatic version number
			};

			if (!request.PortLocationFrom.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.PortLocationFrom),
					Value = request.PortLocationFrom.Code
				};
			}

			if (!request.PortAreaFrom.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.PortAreaFrom),
					Value = request.PortAreaFrom.Code
				};
			}

			if (!request.PortLocationTo.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.PortLocationTo),
					Value = request.PortLocationTo.Code
				};
			}

			if (!request.PortAreaTo.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.PortAreaTo),
					Value = request.PortAreaTo.Code
				};
			}

			if (!request.ATP.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.ATP),
					Value = request.ATP
				};
			}

			if (!request.BookingConfirmationCBK.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.BookingConfirmationCBK),
					Value = request.BookingConfirmationCBK
				};
			}

			if (!request.TransportMode.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.TransportMode),
					Value = request.TransportMode
				};
			}

			if (!request.VehicleRegistration.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.VehicleRegistration),
					Value = request.VehicleRegistration
				};
			}

			if (!request.Subcontracted.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.Subcontracted),
					Value = request.Subcontracted ? "Y" : "N"
				};
			}

			if (!request.RequestStatus.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.RequestStatus),
					Value = request.RequestStatus.ObjectValue
				};
			}

			if (!request.ReasonID.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.ReasonID),
					Value = request.ReasonID.ObjectValue
				};
			}

			if (!request.ReasonNote.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.ReasonNote),
					Value = request.ReasonNote
				};
			}

			if (!request.AuthorizationRequired.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.AuthorizationRequired),
					Value = request.AuthorizationRequired.ObjectValue
				};
			}

			if (!request.DateOfResponse.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.DateOfResponse),
					Value = request.DateOfResponse.ToISO8601String()
				};
			}

			if (!request.ResponseType.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.ResponseType),
					Value = request.ResponseType.ObjectValue
				};
			}

			if (request.DTI)
			{
				if (!request.DeclarationDate.IsEmpty)
				{
					yield return new AddInfo
					{
						Key = nameof(request.DeclarationDate),
						Value = request.DeclarationDate.ToISO8601String()
					};
				}

				if (!request.DeclarationNumber.IsEmpty)
				{
					yield return new AddInfo
					{
						Key = nameof(request.DeclarationNumber),
						Value = request.DeclarationNumber
					};
				}

				if (!request.DeclarationVersion.IsEmpty)
				{
					yield return new AddInfo
					{
						Key = nameof(request.DeclarationVersion),
						Value = request.DeclarationVersion
					};
				}
			}

			if (!request.BOLAPPlusID.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.BOLAPPlusID),
					Value = request.BOLAPPlusID
				};
			}

			if (!request.MoveReqAPPlusID.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.MoveReqAPPlusID),
					Value = request.MoveReqAPPlusID
				};
			}

			if (!request.PortDuesPort.IsEmpty())
			{
				yield return new AddInfo
				{
					Key = "PortDuesPortCode",
					Value = request.PortDuesPort.Code
				};
			}

			yield return new AddInfo
			{
				Key = nameof(request.PortDuesAmount),
				Value = request.PortDuesAmount.ToString()
			};

			if (!request.PortDuesCurrency.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(request.PortDuesCurrency),
					Value = request.PortDuesCurrency.Code
				};
			}

			if (!request.PortDuesPayingPartyAPPlusID.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = "PortDuesPayingParty",
					Value = request.PortDuesPayingPartyAPPlusID
				};
			}
		}
	}
}
