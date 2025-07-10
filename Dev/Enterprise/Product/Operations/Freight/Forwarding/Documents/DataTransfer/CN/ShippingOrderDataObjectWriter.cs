using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CN
{
	sealed class ShippingOrderDataObjectWriter : DataObjectWriter<ShippingOrder, UniversalShipment>
	{
		public ShippingOrderDataObjectWriter(IDataWritingManager writeManager, IDocument document = null)
			: base(writeManager)
		{
			this.document = document;
		}

		readonly IDocument document;

		protected override UniversalShipment PopulateDataObject(ShippingOrder shippingOrder)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = shippingOrder.CreateUXmlDataContext();

			shipment.BookingConfirmationReference = shippingOrder.CarrierBookingReference;
			shipment.NoOriginalBills = (byte?)shippingOrder.NumberOfOriginals;
			shipment.NoCopyBills = (byte?)shippingOrder.NumberOfCopies;
			shipment.WayBillNumber = shippingOrder.BillOfLadingNumber;

			shipment.DeliveryMode = Extensions.CreateUXmlDeliveryMode(shippingOrder.IsDoorPickup, shippingOrder.IsDoorDelivery);

			shipment.ContainerMode = new ContainerMode
			{
				Code = shippingOrder.ContainerMode.Code,
				Description = shippingOrder.ContainerMode.Description
			};

			shipment.ShipmentType = new CodeDescriptionPair
			{
				Code = shippingOrder.ShipmentType.Code,
				Description = shippingOrder.ShipmentType.Description
			};

			shipment.ReleaseType = new CodeDescriptionPair
			{
				Code = shippingOrder.ReleaseType.Code,
				Description = shippingOrder.ReleaseType.Description
			};

			shipment.VesselName = shippingOrder.Vessel?.Name;
			shipment.LloydsIMO = shippingOrder.Vessel?.LloydsIMO;
			shipment.VoyageFlightNo = shippingOrder.VoyageFlightNumber;

			shipment.PortOfLoading = new UNLOCO
			{
				Code = shippingOrder.PortOfLoading.Code,
				Name = shippingOrder.PortOfLoading.Name
			};

			shipment.PortOfDischarge = new UNLOCO
			{
				Code = shippingOrder.PortOfDischarge.Code,
				Name = shippingOrder.PortOfDischarge.Name
			};

			shipment.PlaceOfIssue = new UNLOCO
			{
				Code = shippingOrder.PlaceOfIssue.Code,
				Name = shippingOrder.PlaceOfIssue.Name
			};

			shipment.PlaceOfReceipt = new UNLOCO
			{
				Code = shippingOrder.PlaceOfReceipt.Code,
				Name = shippingOrder.PlaceOfReceipt.Name
			};

			shipment.PlaceOfDelivery = new UNLOCO
			{
				Code = shippingOrder.PlaceOfDelivery.Code,
				Name = shippingOrder.PlaceOfDelivery.Name
			};

			if (!shippingOrder.CarrierBookingOffice.IsEmpty())
			{
				shipment.CarrierBookingOffice = shippingOrder.CarrierBookingOffice.ToUXmlUnloco();

				shipment.SetAddInfoCollection(() => new List<AddInfo>
				{
					new AddInfo
					{
						Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(shippingOrder.CarrierBookingOffice), nameof(shippingOrder.CarrierBookingOffice.Code)),
						Value = shippingOrder.CarrierBookingOffice.Code
					},
					new AddInfo
					{
						Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(shippingOrder.CarrierBookingOffice), nameof(shippingOrder.CarrierBookingOffice.Name)),
						Value = shippingOrder.CarrierBookingOffice.Name
					}
			});
			}

			shipment.SetTransportLegCollection(() => CreateTransportLegs(shippingOrder));

			var addresses = CreateAddresses(shippingOrder).ToList();
			shipment.SetOrganizationAddressCollection(() => addresses.Any()
				? addresses
				: null);

			var dates = CreateDates(shippingOrder).ToList();
			shipment.SetDateCollection(() => dates.Any()
				? dates
				: null);

			var notes = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>(CreateNotes(shippingOrder));
			shipment.SetNoteCollection(() => notes.Any()
				? notes
				: null);

			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferenceNumbers = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(shippingOrder));
				return additionalReferenceNumbers.Any() ? additionalReferenceNumbers : null;
			});

			var addInfos = CreateAddInfos(shippingOrder).ToList();
			shipment.SetAddInfoCollection(() => addInfos.Any()
				? addInfos
				: null);

			var paymentInstructions = CreatePaymentInstructions(shippingOrder).ToList();
			shipment.SetPaymentHandlingInstructionCollection(() => paymentInstructions.Any()
				? paymentInstructions
				: null);

			var billOfLadingClauses = CreateBillOfLadingClauses(shippingOrder).ToList();
			shipment.SetBillOfLadingClauseCollection(() => billOfLadingClauses.Any()
				? billOfLadingClauses
				: null);

			PopulateGoodsDetails(shipment, shippingOrder);

			if (shippingOrder.IsRequiredSendAttachment)
			{
				PopulateAttachedDocuments(shipment);
			}

			return shipment;
		}

		#region AttachedDocuments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Attributes")]
		void PopulateAttachedDocuments(UniversalShipment uxmlShipment)
		{
			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = "Shipping Order",
				Description = "Shipping Order",
				Code = "SHO",
				IsPublished = false
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes);
		}

		#endregion

		#region Transpot Legs

		DataObjectList<TransportLeg> CreateTransportLegs(IShippingOrder shippingOrder)
		{
			var list = new DataObjectList<TransportLeg>() { Content = CollectionContent.Complete };
			list.AddRange(shippingOrder
				.Transports
				.Select(t => t.ToUXmlTransportLeg(writeManager.WriterStrategy)));

			return list;
		}

		#endregion

		#region Addresses

		IEnumerable<OrganizationAddress> CreateAddresses(IShippingOrder shippingOrder)
		{
			if (!shippingOrder.Shipper.IsEmpty())
			{
				yield return shippingOrder.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (!shippingOrder.Carrier.IsEmpty())
			{
				yield return shippingOrder.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (!shippingOrder.Consignee.IsEmpty())
			{
				yield return shippingOrder.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (!shippingOrder.CarrierHandlingAgent.IsEmpty())
			{
				yield return shippingOrder.CarrierHandlingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.CarrierHandlingAgent), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (!shippingOrder.CarrierBookingAgent.IsEmpty())
			{
				yield return shippingOrder.CarrierBookingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.CarrierBookingAgent), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (!shippingOrder.NotifyParty.IsEmpty())
			{
				yield return shippingOrder.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (!shippingOrder.NotifyParty2.IsEmpty())
			{
				yield return shippingOrder.NotifyParty2.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty2), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (!shippingOrder.Forwarder.IsEmpty())
			{
				yield return shippingOrder.Forwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (shippingOrder.IsDoorPickup && !shippingOrder.PickupFrom.IsEmpty())
			{
				yield return shippingOrder.PickupFrom.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorPickupDeliveryAddress), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (shippingOrder.IsDoorDelivery && !shippingOrder.DeliverTo.IsEmpty())
			{
				yield return shippingOrder.DeliverTo.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneePickupDeliveryAddress), writeManager.WriterStrategy, isUpperCase: true);
			}

			if (!shippingOrder.CurrentUser.IsEmpty())
			{
				yield return shippingOrder.CurrentUser.ToUXmlOrganizationAddress(nameof(shippingOrder.CurrentUser), writeManager.WriterStrategy, CreateRegistrationNumbers(shippingOrder.NVOCCReference), isUpperCase: true);
			}
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => x != null && !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		#endregion

		#region Dates

		IEnumerable<Date> CreateDates(IShippingOrder shippingOrder)
		{
			if (!shippingOrder.RequestedDateOfIssue.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.BillRequiredBy,
					Value = shippingOrder.RequestedDateOfIssue
				};
			}
		}

		#endregion

		#region Notes

		IEnumerable<UniversalDataBuss.DataObjects.Universal.Note> CreateNotes(IShippingOrder shippingOrder)
		{
			if (!string.IsNullOrWhiteSpace(shippingOrder.ForwardingInstructions))
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = PredefinedNoteTypes.Instance.ForwardingInstructionNotes.MultilingualDescription.GetUnresolvedString(),
					NoteText = shippingOrder.ForwardingInstructions
				};
			}

			if (!string.IsNullOrWhiteSpace(shippingOrder.GoodsHandlingInstructions))
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = PredefinedNoteTypes.Instance.HandlingInstructions.MultilingualDescription.GetUnresolvedString(),
					NoteText = shippingOrder.GoodsHandlingInstructions
				};
			}

			if (!string.IsNullOrWhiteSpace(shippingOrder.SpecialInstructions))
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = PredefinedNoteTypes.Instance.SpecialInstructions.MultilingualDescription.GetUnresolvedString(),
					NoteText = shippingOrder.SpecialInstructions
				};
			}

			if (!string.IsNullOrWhiteSpace(shippingOrder.USCanadaManifestSelfFilerID))
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = "USCanadaManifestSelfFilerID",  // non-translatable xml constant
					NoteText = shippingOrder.USCanadaManifestSelfFilerID,
				};
			}

			if (!shippingOrder.OtherCharges?.Remarks.IsEmpty ?? false)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = DocDataConstants.Notes.PaymentInstructionRemark,
					NoteText = shippingOrder.OtherCharges.Remarks
				};
			}

			yield return new UniversalDataBuss.DataObjects.Universal.Note
			{
				Description = DocDataConstants.Notes.ChargesFreighted,
				NoteText = shippingOrder.IssueFreightedBillOfLading.ToString()
			};
		}

		#endregion

		#region Additional References

		IEnumerable<AdditionalReference> CreateAdditionalReferences(IShippingOrder shippingOrders)
		{
			if (!shippingOrders.BillOfLadingNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = shippingOrders.BillOfLadingNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.BillOfLading,
						Description = DocDataConstants.AdditionalReferences.Descriptions.BillOfLading
					}
				};
			}

			if (!shippingOrders.ShipperReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = shippingOrders.ShipperReference,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.ShipperReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.ShipperReference
					}
				};
			}

			if (!shippingOrders.FreightForwarderReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = shippingOrders.FreightForwarderReference,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}

			if (!shippingOrders.CarrierContractNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = shippingOrders.CarrierContractNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.CarrierContractNumber,
						Description = DocDataConstants.AdditionalReferences.Descriptions.CarrierContractNumber
					}
				};
			}

			if (!shippingOrders.CarrierContractNumber.IsEmpty && shippingOrders.CarrierContractNumberIsQuotationNumber)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = shippingOrders.CarrierContractNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.CarrierQuoteNumber,
						Description = DocDataConstants.AdditionalReferences.Descriptions.CarrierQuoteNumber
					}
				};
			}

			if (!shippingOrders.ContractNamedAccount.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = shippingOrders.ContractNamedAccount,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.ContractNamedAccount,
						Description = DocDataConstants.AdditionalReferences.Descriptions.ContractNamedAccount
					}
				};
			}

			if (!shippingOrders.CarrierBookingPrefix.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = shippingOrders.CarrierBookingPrefix,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.CarrierBookingPrefix,
						Description = DocDataConstants.AdditionalReferences.Descriptions.CarrierBookingPrefix
					}
				};
			}
		}

		#endregion

		#region AddInfos

		IEnumerable<AddInfo> CreateAddInfos(ShippingOrder shippingOrder)
		{
			if (!shippingOrder.CarrierBookingOffice.IsEmpty())
			{
				foreach (var addInfo in shippingOrder.CarrierBookingOffice.ToUXmlAddInfos(nameof(shippingOrder.CarrierBookingOffice)))
				{
					yield return addInfo;
				}
			}

			if (!shippingOrder.OperationalPort.IsEmpty())
			{
				foreach (var addInfo in shippingOrder.OperationalPort.ToUXmlAddInfos(nameof(shippingOrder.OperationalPort)))
				{
					yield return addInfo;
				}
			}

			if (!shippingOrder.FreightPayableAt.IsEmpty())
			{
				foreach (var addInfo in shippingOrder.FreightPayableAt.ToUXmlAddInfos(nameof(shippingOrder.FreightPayableAt)))
				{
					yield return addInfo;
				}
			}

			if (!shippingOrder.NVOCCReference.Value.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = string.Format(CultureInfo.InvariantCulture, "{0}", "NVOCC_Registration_Reference"),
					Value = shippingOrder.NVOCCReference.Value
				};
			}

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = FormVersionHelper.GetOCMFormVersion()
			};

			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value && shippingOrder.PackageGrouping != null)
			{
				yield return new AddInfo
				{
					Key = DocDataConstants.AddinfoTypes.GroupingMethod,
					Value = shippingOrder.PackageGrouping.Code
				};
			}
		}

		#endregion

		#region Payment Instructions

		IEnumerable<PaymentHandlingInstruction> CreatePaymentInstructions(IShippingOrder shippingOrder)
		{
			var paymentMethod = GetFreightPaymentMethod(shippingOrder.OtherCharges);
			if (paymentMethod != null)
			{
				yield return new PaymentHandlingInstruction
				{
					Category = new CodeDescriptionPair
					{
						Code = DocDataConstants.Charges.Categories.Codes.Freight,
						Description = DocDataConstants.Charges.Categories.Descriptions.Freight
					},
					PaymentMethod = paymentMethod
				};
			}

			var optionalChargePaymentMethod = GetOptionalChargePaymentMethod(shippingOrder.OptionalChargeDestinationHaulage);
			if (optionalChargePaymentMethod != null)
			{
				yield return new PaymentHandlingInstruction
				{
					Category = new CodeDescriptionPair
					{
						Code = DocDataConstants.Charges.Categories.Codes.DestinationHaulage,
						Description = DocDataConstants.Charges.Categories.Descriptions.DestinationHaulage
					},
					PaymentMethod = optionalChargePaymentMethod
				};
			}

			optionalChargePaymentMethod = GetOptionalChargePaymentMethod(shippingOrder.OptionalChargeDestinationPort);
			if (optionalChargePaymentMethod != null)
			{
				yield return new PaymentHandlingInstruction
				{
					Category = new CodeDescriptionPair
					{
						Code = DocDataConstants.Charges.Categories.Codes.DestinationPort,
						Description = DocDataConstants.Charges.Categories.Descriptions.DestinationPort
					},
					PaymentMethod = optionalChargePaymentMethod
				};
			}

			optionalChargePaymentMethod = GetOptionalChargePaymentMethod(shippingOrder.OptionalChargeOriginHaulage);
			if (optionalChargePaymentMethod != null)
			{
				yield return new PaymentHandlingInstruction
				{
					Category = new CodeDescriptionPair
					{
						Code = DocDataConstants.Charges.Categories.Codes.OriginHaulage,
						Description = DocDataConstants.Charges.Categories.Descriptions.OriginHaulage
					},
					PaymentMethod = optionalChargePaymentMethod
				};
			}

			optionalChargePaymentMethod = GetOptionalChargePaymentMethod(shippingOrder.OptionalChargeOriginPort);
			if (optionalChargePaymentMethod != null)
			{
				yield return new PaymentHandlingInstruction
				{
					Category = new CodeDescriptionPair
					{
						Code = DocDataConstants.Charges.Categories.Codes.OriginPort,
						Description = DocDataConstants.Charges.Categories.Descriptions.OriginPort
					},
					PaymentMethod = optionalChargePaymentMethod
				};
			}
		}

		CodeDescriptionPair GetOptionalChargePaymentMethod(IOptionalCharge optionalCharge)
		{
			if (optionalCharge == null)
			{
				return null;
			}

			if (optionalCharge.IsPrepaid)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Prepaid,
					Description = DocDataConstants.Charges.Descriptions.Prepaid
				};
			}

			if (optionalCharge.IsCollect)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Collect,
					Description = DocDataConstants.Charges.Descriptions.Collect
				};
			}

			return null;
		}

		CodeDescriptionPair GetFreightPaymentMethod(IOtherCharges otherCharges)
		{
			if (otherCharges == null)
			{
				return null;
			}

			if (otherCharges.IsPrepaid)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Prepaid,
					Description = DocDataConstants.Charges.Descriptions.Prepaid
				};
			}

			if (otherCharges.IsCollect)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Collect,
					Description = DocDataConstants.Charges.Descriptions.Collect
				};
			}

			if (otherCharges.IsFree)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Free,
					Description = DocDataConstants.Charges.Descriptions.Free
				};
			}

			if (otherCharges.IsPayableElsewhere)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.PayableElsewhere,
					Description = DocDataConstants.Charges.Descriptions.PayableElsewhere
				};
			}

			if (otherCharges.IsFirstLinePrepaidLineSecondCollect)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.FirstLinePrepaidSecondLineCollect,
					Description = DocDataConstants.Charges.Descriptions.FirstLinePrepaidSecondLineCollect
				};
			}

			return null;
		}

		#endregion

		#region Bill Of Lading Clauses

		IEnumerable<BillOfLadingClause> CreateBillOfLadingClauses(IShippingOrder shippingOrder)
		{
			yield return new BillOfLadingClause
			{
				Type = new CodeDescriptionPair
				{
					Code = shippingOrder.IsFreightPrepaid
						? DocDataConstants.BillOfLadingTypes.Codes.Prepaid
						: DocDataConstants.BillOfLadingTypes.Codes.Collect,

					Description = shippingOrder.IsFreightPrepaid
						? DocDataConstants.BillOfLadingTypes.Descriptions.Prepaid
						: DocDataConstants.BillOfLadingTypes.Descriptions.Collect
				}
			};
		}

		#endregion

		#region Goods

		void PopulateGoodsDetails(UniversalShipment shipment, ShippingOrder shippingOrder)
		{
			var seed = 0;

			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			var uxmlPackingLines = new List<UniversalPackingLine>();

			var uxmlPackingLinesByReferenceNumber = new Dictionary<ZString, List<UniversalPackingLine>>();
			var containerLinkMap = new Dictionary<ZString, ZInt>();

			foreach (var container in shippingOrder.Containers)
			{
				var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
				uxmlContainer.Link = ++seed;
				uxmlContainers.Add(uxmlContainer);

				if (FreightDataRegistry.Instance.EnablePackageGrouping.Value
					&& shippingOrder.PackageGrouping != null
					&& (shippingOrder.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByShipment || shippingOrder.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByPackLine))
				{
					if (!containerLinkMap.ContainsKey(container.Identifier.ToString()))
					{
						containerLinkMap.Add(container.Identifier.ToString(), uxmlContainer.Link.Value);
					}
				}
				else
				{
					foreach (var packingLine in container.PackingLines)
					{
						var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy, splitHarmonisedCode: FreightDataRegistry.Instance.EnablePackageGrouping.Value);
						uxmlPackingLine.ContainerLink = uxmlContainer.Link;

						if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
						{
							uxmlPackingLine.SetPackingLineCollection(() => new List<UniversalPackingLine>() { (UniversalPackingLine)uxmlPackingLine.Clone() });
						}

						uxmlPackingLines.Add(uxmlPackingLine);

						if (!uxmlPackingLinesByReferenceNumber.TryGetValue(packingLine.ExportReferenceNumber, out var bookingPackingLines))
						{
							bookingPackingLines = new List<UniversalPackingLine>();
							uxmlPackingLinesByReferenceNumber.Add(packingLine.ExportReferenceNumber, bookingPackingLines);
						}

						bookingPackingLines.Add(uxmlPackingLine);
					}
				}
			}

			shipment.SetContainerCollection(() => uxmlContainers);

			shipment.SetSubShipmentCollection(() => GetSubShipmentCollection(shippingOrder, uxmlPackingLinesByReferenceNumber, containerLinkMap));
		}

		DataObjectList<UniversalShipment> GetSubShipmentCollection(ShippingOrder shippingOrder, Dictionary<ZString, List<UniversalPackingLine>> uxmlPackingLinesByReferenceNumber, Dictionary<ZString, ZInt> containerLinkMap)
		{
			var uxmlBookings = new DataObjectList<UniversalShipment>();

			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value
				&& shippingOrder.PackageGrouping != null
				&& (shippingOrder.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByShipment || shippingOrder.PackageGrouping.Code == Core.Constants.PackageGrouping.Codes.GroupByPackLine))
			{
				foreach (var shipment in shippingOrder.Shipments)
				{
					var uxmlBooking = CreateBookingSubShipment(shipment.ShipmentID);
					var parentPackingLineUXmls = new DataObjectList<UniversalPackingLine>();

					foreach (var parentPackingLineDO in shipment.PackingLines)
					{
						var parentPackingLineUXml = parentPackingLineDO.ToUXmlPackingLine(writeManager.WriterStrategy, splitHarmonisedCode: true);

						parentPackingLineUXml.SetPackingLineCollection(() =>
						{
							return new List<UniversalPackingLine>(parentPackingLineDO.PackingLines
							.Select(x =>
							{
								var childPackingLineUXml = x.ToUXmlPackingLine(writeManager.WriterStrategy, splitHarmonisedCode: true);

								var containerPK = x.Identifier.ToString().Substring(0, 36);
								if (containerLinkMap.TryGetValue(containerPK, out var containerLink))
								{
									childPackingLineUXml.ContainerLink = containerLink;
								}

								childPackingLineUXml.MarksAndNos = parentPackingLineUXml.MarksAndNos;
								childPackingLineUXml.GoodsDescription = parentPackingLineUXml.GoodsDescription;
								childPackingLineUXml.DetailedDescription = parentPackingLineUXml.DetailedDescription;

								return childPackingLineUXml;
							}));
						});
						parentPackingLineUXmls.Add(parentPackingLineUXml);

						uxmlBooking.SetPackingLineCollection(() => parentPackingLineUXmls);
					}

					uxmlBookings.Add(uxmlBooking);
				}
			}
			else
			{
				foreach (var bookingNumber in uxmlPackingLinesByReferenceNumber.Keys)
				{
					var uxmlBooking = CreateBookingSubShipment(bookingNumber);
					uxmlBookings.Add(uxmlBooking);

					if (uxmlPackingLinesByReferenceNumber.TryGetValue(bookingNumber, out var bookingPackingLines))
					{
						uxmlBooking.SetPackingLineCollection(() => new DataObjectList<UniversalPackingLine>(bookingPackingLines));
					}
				}
			}

			return uxmlBookings;
		}

		UniversalShipment CreateBookingSubShipment(string bookingNumber)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource
				{
					Type = DocDataConstants.DataSources.Booking,
					Key = bookingNumber
				},
				Workflow = new UniversalDataBuss.DataObjects.Universal._2012_11.Workflow
				{
					ActionPurpose = new CodeDescriptionPair
					{
						Code = DocDataConstants.ActionPurpuses.Codes.AsPerPayload,
						Description = DocDataConstants.ActionPurpuses.Descriptions.AsPerPayload
					}
				}
			};

			shipment.BookingConfirmationReference = bookingNumber;

			return shipment;
		}

		#endregion
	}
}
