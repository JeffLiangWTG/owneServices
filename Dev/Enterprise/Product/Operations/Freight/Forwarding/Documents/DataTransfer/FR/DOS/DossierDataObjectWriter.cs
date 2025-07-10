using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class DossierDataObjectWriter : DataObjectWriter<Dossier, UniversalShipment>
	{
		public DossierDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(Dossier dossier)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = dossier.CreateUXmlDataContext();

			shipment.ContainerMode = dossier.ContainerMode?.ToUXmlContainerMode();
			shipment.ShipmentType = dossier.ShipmentType?.ToUXmlCodeDescriptionPair();
			shipment.BookingConfirmationReference = dossier.CarrierBookingReference;
			shipment.WayBillNumber = dossier.BillOfLading;
			shipment.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = CreateAdditionalReferences(dossier).ToArray();

				return additionalReferences.Length > 0
					? new DataObjectList<AdditionalReference>(additionalReferences)
					: null;
			});

			shipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfosRoot(dossier).ToList();

				return addInfos.Count > 0
					? addInfos
					: null;
			});

			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(dossier).ToList();

				return addresses.Count > 0
					? addresses
					: null;
			});

			shipment.SetNoteCollection(() =>
			{
				var notes = CreateNotes(dossier).ToArray();

				return notes.Length > 0
					? new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>(notes)
					: null;
			});

			return shipment;
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(Dossier dossier)
		{
			var freightForwarderReference = dossier.ShipmentNumber.IsEmpty ? dossier.ConsolNumber : dossier.ShipmentNumber;

			if (!freightForwarderReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = freightForwarderReference,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}

			if (!dossier.CarrierBookingReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = dossier.CarrierBookingReference,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.CarrierBookingReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.CarrierBookingReference
					}
				};
			}

			if (!dossier.AgentReference.IsEmpty)
			{
				var values = dossier.AgentReference.Split(", ");
				foreach (var value in values)
				{
					yield return new AdditionalReference
					{
						ReferenceNumber = value,
						Type = new EntryType
						{
							Code = DocDataConstants.AdditionalReferences.Codes.CarrierBookingReference,
							Description = DocDataConstants.AdditionalReferences.Descriptions.CarrierBookingReference
						}
					};
				}
			}
		}

		IEnumerable<AddInfo> CreateAddInfosRoot(Dossier dossier)
		{
			if (!dossier.OperationalPort.IsEmpty())
			{
				foreach (var addInfo in dossier.OperationalPort.ToUXmlAddInfos(nameof(dossier.OperationalPort)))
				{
					yield return addInfo;
				}
			}

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programatic version number
			};

			if (!dossier.OTC.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(dossier.OTC),
					Value = dossier.OTC
				};
			}

			if (!dossier.ATP.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(dossier.ATP),
					Value = dossier.ATP
				};
			}

			if (!dossier.ThirdPartyReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = "ThirdParty_Reference", // Programatic constant
					Value = dossier.ThirdPartyReference
				};
			}

			yield return new AddInfo
			{
				Key = nameof(dossier.FileComplete),
				Value = dossier.FileComplete.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(dossier.UniqueDeclaration),
				Value = dossier.UniqueDeclaration.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(dossier.MultipleDeclaration),
				Value = dossier.MultipleDeclaration.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(dossier.LastDeclaration),
				Value = dossier.LastDeclaration.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(dossier.ImplicitAcknowledgement),
				Value = dossier.ImplicitAcknowledgement.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(dossier.ExplicitAcknowledgement),
				Value = dossier.ExplicitAcknowledgement.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(dossier.ImplicitBAET),
				Value = dossier.ImplicitBAET.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(dossier.ExplicitBAET),
				Value = dossier.ExplicitBAET.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(dossier.NumberOfDeclarations),
				Value = dossier.NumberOfDeclarations.ToString()
			};

			if (!dossier.ConfirmationReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = "DOSReference", // Programatic constant
					Value = dossier.ConfirmationReference
				};
			}
			if (!dossier.ECVReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = "AMQReference", // Programatic constant
					Value = dossier.ECVReference.ReplaceIgnoringCase(", ", "|")
				};
			}
		}

		IEnumerable<OrganizationAddress> CreateAddresses(Dossier dossier)
		{
			if (!dossier.Carrier.IsEmpty() && !dossier.ConsolNumber.IsEmpty)
			{
				yield return dossier.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dossier.CarrierCI5, dossier.CarrierSON));
			}

			if (!dossier.ReceivingForwarder.IsEmpty())
			{
				yield return dossier.ReceivingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dossier.ReceivingForwarderCI5, dossier.ReceivingForwarderSON));
			}

			if (!dossier.SendingForwarder.IsEmpty())
			{
				yield return dossier.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dossier.SendingForwarderCI5, dossier.SendingForwarderSON));
			}

			if (!dossier.ThirdParty.IsEmpty())
			{
				yield return dossier.ThirdParty.ToUXmlOrganizationAddress("ThirdPartyAddress", writeManager.WriterStrategy, CreateRegistrationNumbers(dossier.ThirdPartyCI5, dossier.ThirdPartySON)); // Programatic constant
			}

			if (!dossier.SendingParty.IsEmpty())
			{
				yield return dossier.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(dossier.SendingPartyCI5, dossier.SendingPartySON));
			}

			if (!dossier.Agent.IsEmpty())
			{
				yield return dossier.Agent.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dossier.AgentCI5, dossier.AgentSOA));
			}
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.Note> CreateNotes(Dossier dossier)
		{
			if (!dossier.ThirdPartyNotes.IsEmpty)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = (NoResString)"Third Party Notes", // Programatic constant
					NoteText = dossier.ThirdPartyNotes
				};
			}

			if (!dossier.Notes.IsEmpty)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = (NoResString)"Carrier Booking Notes", // Programatic constant
					NoteText = dossier.Notes
				};
			}
		}
	}
}
