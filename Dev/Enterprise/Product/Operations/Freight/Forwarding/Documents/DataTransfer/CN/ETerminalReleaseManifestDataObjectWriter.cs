using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Container = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Container;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CN
{
	sealed class ETerminalReleaseManifestDataObjectWriter : DataObjectWriter<ETerminalReleaseManifest, UniversalShipment>
	{
		public ETerminalReleaseManifestDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(ETerminalReleaseManifest manifest)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			PopulateVesselInfo(manifest, shipment);
			PopulateOperationalPort(manifest, shipment);
			PopulateTopLevelAddresses(manifest, shipment);
			shipment.SetSubShipmentCollection(() =>
			{
				var ports = new UniversalShipment(writeManager.WriterStrategy);
				PopulatePorts(manifest, ports);

				ports.SetSubShipmentCollection(() =>
				{
					var consol = new UniversalShipment(writeManager.WriterStrategy);
					PopulateConsolInfo(manifest, consol);
					return new DataObjectList<UniversalShipment>() { consol };
				});

				return new DataObjectList<UniversalShipment>() { ports };
			});

			return shipment;
		}

		#region Vessel

		static void PopulateVesselInfo(ETerminalReleaseManifest manifest, UniversalShipment shipment)
		{
			shipment.DataContext = manifest.CreateUXmlDataContext();
			shipment.VesselName = manifest.Transports?.Main?.Vessel?.Name;
			shipment.LloydsIMO = manifest.Transports?.Main?.Vessel?.LloydsIMO;
			shipment.VoyageFlightNo = manifest.Transports?.Main?.VoyageFlightNumber;
		}

		#endregion

		#region PopulateOperationalPort

		static void PopulateOperationalPort(ETerminalReleaseManifest manifest, UniversalShipment shipment)
		{
			if (!manifest.OperationalPort.IsEmpty())
			{
				shipment.SetAddInfoCollection(() =>
				{
					var infos = new List<AddInfo>();
					foreach (var addInfo in manifest.OperationalPort.ToUXmlAddInfos(nameof(manifest.OperationalPort)))
					{
						infos.Add(addInfo);
					}
					return infos;
				});
			}
		}

		#endregion

		#region PopulateTopLevelAddresses

		void PopulateTopLevelAddresses(ETerminalReleaseManifest manifest, UniversalShipment shipment)
		{
			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!manifest.Carrier.IsEmpty())
				{
					addresses.Add(manifest.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy));
				}

				if (!manifest.CurrentUser.IsEmpty())
				{
					addresses.Add(manifest.CurrentUser.ToUXmlOrganizationAddress(nameof(manifest.CurrentUser), writeManager.WriterStrategy));
				}

				if (addresses.Any())
				{
					return addresses;
				}
				return shipment.OrganizationAddressCollection;
			});
		}

		#endregion

		#region Ports

		static void PopulatePorts(ETerminalReleaseManifest manifest, UniversalShipment ports)
		{
			ports.PortOfLoading = manifest.PortOfLoad.ToUXmlUnloco();
			ports.PortOfDischarge = manifest.PortOfDischarge.ToUXmlUnloco();
		}

		#endregion

		#region Consol

		void PopulateConsolInfo(ETerminalReleaseManifest manifest, UniversalShipment consol)
		{
			consol.WayBillNumber = manifest.BillOfLadingNumber;
			consol.ReleaseType = manifest.ReleaseType == null
				? null
				: new CodeDescriptionPair
				{
					Code = manifest.ReleaseType.Code,
					Description = manifest.ReleaseType.Description
				};

			consol.NoCopyBills = (byte?)manifest.NumberOfCopies;
			consol.NoOriginalBills = (byte?)manifest.NumberOfOriginals;
			consol.DeliveryMode = CreateDeliveryMode(manifest);

			consol.PlaceOfIssue = manifest.PlaceOfIssue.ToUXmlUnloco();
			consol.PlaceOfDelivery = manifest.PlaceOfDelivery.ToUXmlUnloco();

			PopulateGoodsDetails(consol, manifest);
			PopulateConsolCollections(manifest, consol);
		}

		CodeDescriptionPair CreateDeliveryMode(ETerminalReleaseManifest manifest)
		{
			var code = string.Format(CultureInfo.InvariantCulture, "{0}T{1}", manifest.IsDoorPickup ? "D" : "P", manifest.IsDoorDelivery ? "D" : "P"); // non-translatable validation message;
			var description = string.Format(CultureInfo.InvariantCulture, (EZC.NoResString)"{0} To {1}", manifest.IsDoorPickup ? (EZC.NoResString)"Door" : (EZC.NoResString)"Peer", manifest.IsDoorDelivery ? (EZC.NoResString)"Door" : (EZC.NoResString)"Peer"); // non-translatable validation message;

			return new CodeDescriptionPair
			{
				Code = code,
				Description = description
			};
		}

		void PopulateConsolCollections(ETerminalReleaseManifest manifest, UniversalShipment consol)
		{
			var orgAddresses = CreateConsolAddresses(manifest).ToList();
			consol.SetOrganizationAddressCollection(() => orgAddresses.Any() ? orgAddresses : null);

			consol.SetPaymentHandlingInstructionCollection(() =>
			{
				var paymentHandling = CreatePaymentInstructions(manifest).ToList();
				return paymentHandling.Any() ? paymentHandling : null;
			});

			consol.SetAddInfoCollection(() =>
			{
				var addinfos = CreateAddInfos(manifest).ToList();
				return addinfos.Any() ? addinfos : null;
			});

			consol.SetNoteCollection(() =>
			{
				var notes = CreateNotes(manifest);
				return notes.Any() ? new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>(notes) : null;
			});
			consol.SetAdditionalReferenceCollection(() =>
			{
				var references = manifest.ToUXmlAdditionalReferences();
				return references.Any() ? new DataObjectList<AdditionalReference>(references) : null;
			});
			consol.SetDateCollection(() =>
			{
				var dates = CreateDates(manifest).ToList();
				return dates.Any() ? dates : null;
			});

			consol.SetBillOfLadingClauseCollection(() =>
			{
				var billOfLadingClauses = CreateBillOfLadingClauses(manifest).ToList();
				return billOfLadingClauses.Any() ? billOfLadingClauses : null;
			});
		}

		#endregion

		#region Dates

		IEnumerable<Date> CreateDates(ETerminalReleaseManifest manifest)
		{
			if (!manifest.RequestedDateOfIssue.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.BillRequiredBy,
					Value = manifest.RequestedDateOfIssue
				};
			}
		}

		#endregion

		#region AddInfos

		IEnumerable<AddInfo> CreateAddInfos(ETerminalReleaseManifest manifest)
		{
			if (!manifest.FreightPayableAt.IsEmpty())
			{
				foreach (var addInfo in manifest.FreightPayableAt.ToUXmlAddInfos(nameof(manifest.FreightPayableAt)))
				{
					yield return addInfo;
				}
			}

			yield return new AddInfo() { Key = "MasterSONumber", Value = manifest.UseBkgRefAsMasterSO ? "CBR" : "MBL" };
		}

		#endregion

		#region Bill Of Lading Clauses

		IEnumerable<BillOfLadingClause> CreateBillOfLadingClauses(ETerminalReleaseManifest manifest)
		{
			yield return new BillOfLadingClause
			{
				Type = new CodeDescriptionPair
				{
					Code = manifest.IsFreightPrepaid
						? BillOfLadingTypeCodes.Prepaid
						: BillOfLadingTypeCodes.Collect,

					Description = manifest.IsFreightPrepaid
						? BillOfLadingTypeDescriptions.Prepaid
						: BillOfLadingTypeDescriptions.Collect
				}
			};
		}

		#endregion

		#region Goods Details

		void PopulateGoodsDetails(UniversalShipment shipment, ETerminalReleaseManifest manifest)
		{
			var seed = 0;

			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			var uxmlPackingLines = new List<UniversalDataBuss.DataObjects.Universal.PackingLine>();

			var uxmlPackingLinesByReferenceNumber = new Dictionary<ZString, List<UniversalDataBuss.DataObjects.Universal.PackingLine>>();

			foreach (var container in manifest.Containers ?? new ReadOnlyCollection<Container>(System.Array.Empty<Container>()))
			{
				var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
				uxmlContainer.Link = ++seed;
				uxmlContainers.Add(uxmlContainer);

				foreach (var packingLine in container.PackingLines)
				{
					var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy);
					uxmlPackingLine.ContainerLink = uxmlContainer.Link;
					uxmlPackingLines.Add(uxmlPackingLine);

					if (!uxmlPackingLinesByReferenceNumber.TryGetValue(packingLine.ExportReferenceNumber, out List<UniversalDataBuss.DataObjects.Universal.PackingLine> bookingPackingLines))
					{
						bookingPackingLines = new List<UniversalDataBuss.DataObjects.Universal.PackingLine>();
						uxmlPackingLinesByReferenceNumber.Add(packingLine.ExportReferenceNumber, bookingPackingLines);
					}

					bookingPackingLines.Add(uxmlPackingLine);
				}
			}

			shipment.SetContainerCollection(() => uxmlContainers.Any() ? uxmlContainers : null);

			shipment.SetSubShipmentCollection(() =>
			{
				var uxmlBookings = new DataObjectList<UniversalShipment>();

				foreach (var booking in manifest.Bookings ?? Enumerable.Empty<IBooking>())
				{
					var uxmlBooking = CreateBookingSubShipment(booking);
					uxmlBookings.Add(uxmlBooking);

					if (uxmlPackingLinesByReferenceNumber.TryGetValue(booking.BookingNumber, out List<UniversalDataBuss.DataObjects.Universal.PackingLine> bookingPackingLines))
					{
						uxmlBooking.SetPackingLineCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>(bookingPackingLines));
					}
				}

				return uxmlBookings.Any() ? uxmlBookings : null;
			});
		}

		UniversalShipment CreateBookingSubShipment(IBooking booking)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = DataSources.Booking,
					Key = booking.BookingNumber
				},
				DocumentaryOverride = new DocumentaryOverride
				{
					Purpose = new CodeDescriptionPair
					{
						Code = MessagePurposes.Codes.Original,
						Description = MessagePurposes.Descriptions.Original
					}
				}
			};

			shipment.BookingConfirmationReference = booking.BookingNumber;

			return shipment;
		}

		#endregion

		#region Addresses

		IEnumerable<OrganizationAddress> CreateConsolAddresses(ETerminalReleaseManifest manifest)
		{
			if (!manifest.Forwarder.IsEmpty())
			{
				yield return manifest.Forwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!manifest.Shipper.IsEmpty())
			{
				yield return manifest.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!manifest.Consignee.IsEmpty())
			{
				yield return manifest.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!manifest.Carrier.IsEmpty())
			{
				yield return manifest.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy);
			}

			if (!manifest.NotifyParty.IsEmpty())
			{
				yield return manifest.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty), writeManager.WriterStrategy);
			}

			if (!manifest.NotifyParty2.IsEmpty())
			{
				yield return manifest.NotifyParty2.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty2), writeManager.WriterStrategy);
			}
		}

		#endregion

		#region Payment Instructions

		IEnumerable<PaymentHandlingInstruction> CreatePaymentInstructions(ETerminalReleaseManifest manifest)
		{
			var paymentMethod = GetFreightPaymentMethod(manifest);

			if (paymentMethod == null)
			{
				yield break;
			}

			yield return new PaymentHandlingInstruction
			{
				Category = new CodeDescriptionPair
				{
					Code = ChargeCategoryCodes.Freight,
					Description = ChargeCategoryDescriptions.Freight
				},
				PaymentMethod = paymentMethod
			};
		}

		CodeDescriptionPair GetFreightPaymentMethod(ETerminalReleaseManifest manifest)
		{
			if (manifest.OtherCharges == null)
			{
				return null;
			}

			if (manifest.OtherCharges.IsPrepaid)
			{
				return new CodeDescriptionPair
				{
					Code = ChargeCodes.Prepaid,
					Description = ChargeDescriptions.Prepaid
				};
			}

			if (manifest.OtherCharges.IsCollect)
			{
				return new CodeDescriptionPair
				{
					Code = ChargeCodes.Collect,
					Description = ChargeDescriptions.Collect
				};
			}

			if (manifest.OtherCharges.IsFree)
			{
				return new CodeDescriptionPair
				{
					Code = ChargeCodes.Free,
					Description = ChargeDescriptions.Free
				};
			}

			if (manifest.OtherCharges.IsPayableElsewhere)
			{
				return new CodeDescriptionPair
				{
					Code = ChargeCodes.PayableElsewhere,
					Description = ChargeDescriptions.PayableElsewhere
				};
			}

			if (manifest.OtherCharges.IsFirstLinePrepaidLineSecondCollect)
			{
				return new CodeDescriptionPair
				{
					Code = ChargeCodes.FirstLinePrepaidSecondLineCollect,
					Description = ChargeDescriptions.FirstLinePrepaidSecondLineCollect
				};
			}

			return null;
		}

		#endregion

		#region Notes

		IEnumerable<UniversalDataBuss.DataObjects.Universal.Note> CreateNotes(ETerminalReleaseManifest manifest)
		{
			if (!string.IsNullOrWhiteSpace(manifest.SpecialInstructions))
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = PredefinedNoteTypes.Instance.SpecialInstructions.MultilingualDescription.GetUnresolvedString(),
					NoteText = manifest.SpecialInstructions
				};
			}

			if (!manifest.FreightPayableAt.IsEmpty())
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = DocDataConstants.Notes.FreightPayableAt,
					NoteText = manifest.FreightPayableAt.Code
				};
			}

			if (!manifest.OtherCharges?.Remarks.IsEmpty ?? false)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = DocDataConstants.Notes.PaymentInstructionRemark,
					NoteText = manifest.OtherCharges.Remarks
				};
			}
		}

		#endregion
	}
}
