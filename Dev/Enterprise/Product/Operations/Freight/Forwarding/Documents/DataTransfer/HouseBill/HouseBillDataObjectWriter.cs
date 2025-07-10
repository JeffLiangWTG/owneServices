using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using EZC = Enterprise.ZArchitecture.Core;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class HouseBillDataObjectWriter : DataObjectWriter<HouseBill, UniversalShipment>
	{
		public HouseBillDataObjectWriter(IDataWritingManager writeManager, IDocument document = null)
			: base(writeManager)
		{
			this.document = document;
		}

		readonly IDocument document;

		protected override UniversalShipment PopulateDataObject(HouseBill houseBill)
		{
			var uxmlShipment = new UniversalShipment(writeManager.WriterStrategy);

			uxmlShipment.DataContext = houseBill
				.CreateUXmlDataContext()
				.AddDataProvider()
				.AddUserBranchAndDepartment();

			PopulateHeader(houseBill, uxmlShipment);

			PopulateAddresses(houseBill, uxmlShipment);
			PopulateContainersAndPackingLines(houseBill, uxmlShipment);
			PopulateAddInfos(houseBill, uxmlShipment);
			PopulateBillOfLadingClauses(houseBill, uxmlShipment);
			PopulateDates(houseBill, uxmlShipment);
			PopulateNotes(houseBill, uxmlShipment);
			PopulatePaymentInstructions(houseBill, uxmlShipment);
			PopulateAttachedDocuments(houseBill, uxmlShipment);
			PopulateAdditionalReferences(houseBill, uxmlShipment);
			PopulateEntryNumberCollection(houseBill, uxmlShipment);

			return uxmlShipment;
		}

		#region Header
		void PopulateHeader(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			uxmlShipment.ContainerCount = houseBill.Containers?.Count;
			uxmlShipment.ContainerMode = houseBill.ContainerMode?.ToUXmlContainerMode();
			uxmlShipment.PlaceOfDelivery = houseBill.PlaceOfDelivery?.ToUXmlUnloco();
			uxmlShipment.PlaceOfIssue = houseBill.PlaceOfIssue?.ToUXmlUnloco();
			uxmlShipment.PlaceOfReceipt = houseBill.PlaceOfReceipt?.ToUXmlUnloco();
			uxmlShipment.PortOfOrigin = houseBill.PortOfOrigin?.ToUXmlUnloco();
			uxmlShipment.PortOfLoading = houseBill.PortOfLoading?.ToUXmlUnloco();
			uxmlShipment.PortOfDestination = houseBill.PortOfDestination?.ToUXmlUnloco();
			uxmlShipment.PortOfDischarge = houseBill.PortOfDischarge?.ToUXmlUnloco();
			uxmlShipment.ReleaseType = houseBill.ReleaseType?.ToUXmlCodeDescriptionPair();
			uxmlShipment.TotalNoOfPacks = houseBill.TotalPackCount;
			uxmlShipment.TotalNoOfPacksPackageType = houseBill.TotalPackType != null ? new PackageType() { Code = houseBill.TotalPackType.Code, Description = houseBill.TotalPackType.Description } : null;
			uxmlShipment.TotalVolume = houseBill.TotalVolume?.Value;
			uxmlShipment.TotalVolumeUnit = houseBill.TotalVolume?.Unit.ToUXmlUnitOfVolume();
			uxmlShipment.TotalWeight = houseBill.TotalWeight?.Value;
			uxmlShipment.TotalWeightUnit = houseBill.TotalWeight?.Unit.ToUXmlUnitOfWeight();
			uxmlShipment.VesselName = houseBill.Transports?.Main?.Vessel?.Name;
			uxmlShipment.VoyageFlightNo = houseBill.Transports?.Main?.VoyageFlightNumber;
			uxmlShipment.GoodsDescription = houseBill.GoodsDescription;
			uxmlShipment.HouseBillOfLadingType = houseBill.HouseBillOfLadingType.ToUXmlCodeDescriptionPair();

			uxmlShipment.NoOriginalBills = (byte)houseBill.NumberOfOriginals;
			uxmlShipment.NoCopyBills = (byte)houseBill.NumberOfCopies;
			uxmlShipment.WayBillNumber = houseBill.HouseBillNumber;
			uxmlShipment.WayBillType = new WayBillType
			{
				Code = WayBillTypeList.Codes.House,
				Description = WayBillTypeList.Descriptions.House
			};

			if (houseBill.INCO != null && !houseBill.INCO.Code.IsEmpty)
			{
				uxmlShipment.ShipmentIncoTerm = new IncoTerm
				{
					Code = houseBill.INCO.Code,
					Description = houseBill.INCO.Description
				};
			}

			if (houseBill.IsDraft)
			{
				uxmlShipment.CoLoadBookingConfirmationReference = houseBill.ShipmentNumber;
				uxmlShipment.CoLoadMasterBillNumber = houseBill.HouseBillNumber;
			}
		}
		#endregion

		#region OrganisationAddresses

		void PopulateAddresses(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!houseBill.Shipper.IsEmpty())
				{
					var organizationAddress = houseBill.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy);
					PopulateTaxInfo(organizationAddress, houseBill.ShipperTaxInfo);
					addresses.Add(organizationAddress);
				}

				if (!houseBill.Consignee.IsEmpty())
				{
					var organizationAddress = houseBill.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy);
					PopulateTaxInfo(organizationAddress, houseBill.ConsigneeTaxInfo);
					addresses.Add(organizationAddress);
				}

				if (!houseBill.NotifyParty.IsEmpty())
				{
					var organizationAddress = houseBill.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty), writeManager.WriterStrategy);
					PopulateTaxInfo(organizationAddress, houseBill.NotifyPartyTaxInfo);
					addresses.Add(organizationAddress);
				}

				if (!houseBill.GoodsDelivery.IsEmpty())
				{
					addresses.Add(houseBill.GoodsDelivery.ToUXmlOrganizationAddress(nameof(DocAddressType.DeliveryAgent), writeManager.WriterStrategy));
				}

				if (!houseBill.SendingForwarder.IsEmpty())
				{
					addresses.Add(houseBill.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		void PopulateTaxInfo(OrganizationAddress organizationAddress, TaxInfo taxInfo)
		{
			organizationAddress.GovRegNum = taxInfo?.Number;
			organizationAddress.GovRegNumType = new RegistrationNumberType
			{
				Code = taxInfo?.Code,
				Description = taxInfo?.Description
			};
		}
		#endregion

		#region ContainersAndPackingLines

		void PopulateContainersAndPackingLines(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			var containerLink = 0;
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			var uxmlPackingLines = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();

			if (houseBill.Containers != null)
			{
				foreach (var container in houseBill.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainer.Link = ++containerLink;

					foreach (var packingLine in container.PackingLines)
					{
						var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy);
						uxmlPackingLine.ContainerLink = uxmlContainer.Link;

						uxmlPackingLines.Add(uxmlPackingLine);
					}

					uxmlContainers.Add(uxmlContainer);
				}
			}

			if (houseBill.HouseBillOfLadingType != null && houseBill.HouseBillOfLadingType.Code == HouseBillOfLadingTypes.Code.FIATAHBL && houseBill.LoosePackingLines?.Count > 0)
			{
				foreach (var packingLine in houseBill.LoosePackingLines)
				{
					var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy);
					uxmlPackingLines.Add(uxmlPackingLine);
				}
			}

			uxmlShipment.SetPackingLineCollection(() => uxmlPackingLines);
			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		#endregion

		#region AddInfos

		void PopulateAddInfos(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, (EZC.NoResString)"Freight Amount", houseBill.FreightAmount?.Amount.ToString()); // must always be this value in uxml, no translation
				AddAddInfo(addInfos, "FreightCurrency", houseBill.FreightAmount?.Currency.Code.ToString()); // must always be this value in uxml, no translation
				AddAddInfo(addInfos, (EZC.NoResString)"Declared Value", houseBill.DeclaredValueOfGoods?.Amount.ToString()); // must always be this value in uxml, no translation
				AddAddInfo(addInfos, "DeclaredValueCurrency", houseBill.DeclaredValueOfGoods?.Currency.Code.ToString()); // must always be this value in uxml, no translation
				AddAddInfo(addInfos, (EZC.NoResString)"Freight Payable at", houseBill.FreightPayableAt?.Code.ToString()); // must always be this value in uxml, no translation
				AddAddInfo(addInfos, "ExcessValueDeclarationAmount", houseBill.ExcessValueDeclaration?.Amount.ToString()); // must always be this value in uxml, no translation
				AddAddInfo(addInfos, "ExcessValueDeclarationCurrency", houseBill.ExcessValueDeclaration?.Currency?.Code.ToString()); // must always be this value in uxml, no translation
				AddAddInfo(addInfos, nameof(houseBill.MoveTypeFrom), houseBill.MoveTypeFrom);
				AddAddInfo(addInfos, nameof(houseBill.MoveTypeTo), houseBill.MoveTypeTo);
				AddAddInfo(addInfos, nameof(houseBill.AsAgentOption), houseBill.AsAgentOption?.Code ?? ZString.Empty);
				AddAddInfo(addInfos, nameof(houseBill.AsAgentDetail), houseBill.AsAgentDetail);
				AddAddInfo(addInfos, nameof(houseBill.ConsignorShipperTerminology), houseBill.ConsignorShipperTerminology);
				AddAddInfo(addInfos, nameof(houseBill.ACIDNO), houseBill.ACIDNO);

				if (addInfos.Any())
				{
					addInfos.Add(new AddInfo { Key = DocDataConstants.AddinfoTypes.FormVersion, Value = "1.0.0" }); // Programmatic version number
				}

				return addInfos.Any() ? addInfos : null;
			});
		}

		void AddAddInfo(List<AddInfo> addinfos, ZString key, ZString value)
		{
			if (!value.IsEmpty)
			{
				addinfos.Add(new AddInfo
				{
					Key = key,
					Value = value
				});
			}
		}

		#endregion

		#region Bill Of Lading Clauses

		void PopulateBillOfLadingClauses(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetBillOfLadingClauseCollection(() =>
			{
				var billOfLadingClauses = new List<BillOfLadingClause>();

				if (houseBill.ContainerMode != null)
				{
					if (houseBill.ContainerMode.Code == DocDataConstants.BillOfLadingTypes.Codes.Collect)
					{
						if (houseBill.ShipperLoadAndCount.Code != ZString.Empty)
						{
							AddBillOfLadingClause(billOfLadingClauses, houseBill.ShipperLoadAndCount.Code, houseBill.ShipperLoadAndCount.Description);
						}
						AddBillOfLadingClause(billOfLadingClauses, DocDataConstants.BillOfLadingTypes.Codes.Collect, DocDataConstants.BillOfLadingTypes.Descriptions.Collect);
					}
				}

				return billOfLadingClauses.Any() ? billOfLadingClauses : null;
			});
		}

		void AddBillOfLadingClause(List<BillOfLadingClause> billOfLadingClauses, string clauseCode, string clauseDescription)
		{
			billOfLadingClauses.Add(new BillOfLadingClause
			{
				Type = new CodeDescriptionPair
				{
					Code = clauseCode,
					Description = clauseDescription
				}
			});
		}

		#endregion

		#region Dates

		void PopulateDates(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetDateCollection(() =>
			{
				var dates = new List<Date>();

				if (houseBill.ShippedOnBoard != null)
				{
					AddDate(dates, DateType.ShippedOnBoard, houseBill.ShippedOnBoard.Date);
				}
				AddDate(dates, DateType.BillIssued, houseBill.DateOfIssue.Date);
				AddDate(dates, DateType.Departure, houseBill.DepartureDate);
				AddDate(dates, DateType.Arrival, houseBill.ArrivalDate);

				return dates.Any() ? dates : null;
			});
		}

		void AddDate(List<Date> dates, DateType dateType, ZDateTime dateTime)
		{
			if (!dateTime.IsEmpty)
			{
				dates.Add(new Date
				{
					Type = dateType,
					IsEstimate = false,
					Value = dateTime
				});
			}
		}

		#endregion

		#region Notes

		void PopulateNotes(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetNoteCollection(() =>
			{
				var notes = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>();

				AddNote(notes, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, houseBill.MarksAndNumbers);

				return notes.Any() ? notes : null;
			});
		}

		void AddNote(DataObjectList<UniversalDataBuss.DataObjects.Universal.Note> notes, ZString description, ZString noteText)
		{
			if (!noteText.IsEmpty)
			{
				notes.Add(new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = description,
					IsCustomDescription = false,
					NoteText = noteText
				});
			}
		}

		#endregion

		#region Additional References

		void PopulateAdditionalReferences(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			var additionalReferenceNumbers = new DataObjectList<AdditionalReference>();
			if (houseBill.HIRReference != null)
			{
				AddAdditionalReference(additionalReferenceNumbers, houseBill.HIRReference.Value, houseBill.HIRReference.Type.Code, houseBill.HIRReference.Type.Description);
			}
			AddAdditionalReference(additionalReferenceNumbers, houseBill.CTKNumber, CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote, (EZC.NoResString)"Cargo Tracking Note");

			uxmlShipment.SetAdditionalReferenceCollection(() => additionalReferenceNumbers.Any() ? additionalReferenceNumbers : null);
		}

		void AddAdditionalReference(DataObjectList<AdditionalReference> additionalReferences, ZString additionalReferenceNumber, ZString additionalReferenceTypeCode, ZString additionalReferenceTypeDescription)
		{
			if (!additionalReferenceNumber.IsEmpty)
			{
				var additionalReference = new AdditionalReference
				{
					Type = new EntryType
					{
						Code = additionalReferenceTypeCode,
						Description = additionalReferenceTypeDescription
					},
					ReferenceNumber = additionalReferenceNumber
				};
				additionalReferences.Add(additionalReference);
			}
		}

		#endregion

		#region Entry Numbers

		void PopulateEntryNumberCollection(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			var entryNumbers = new List<EntryNumber>();
			AddEntryNumber(entryNumbers, houseBill.CustomsEntryNumber);

			uxmlShipment.SetEntryNumberCollection(() => entryNumbers.Any() ? entryNumbers : null);
		}

		void AddEntryNumber(List<EntryNumber> entryNumbers, CustomsEntryNumber customsEntryNumber)
		{
			if (customsEntryNumber != null && !customsEntryNumber.Value.IsEmpty)
			{
				entryNumbers.Add(new EntryNumber
				{
					Number = customsEntryNumber.Value,
					Type = new EntryType
					{
						Code = customsEntryNumber.Type?.Code,
						Description = customsEntryNumber.Type?.Description
					}
				});
			}
		}

		#endregion

		#region Payment Instructions

		void PopulatePaymentInstructions(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetPaymentHandlingInstructionCollection(() =>
			{
				var paymentInstructions = new List<PaymentHandlingInstruction>();

				AddPaymentInstruction(paymentInstructions, houseBill.PaymentTerms, DocDataConstants.Charges.Categories.Codes.Freight, DocDataConstants.Charges.Categories.Descriptions.Freight);

				return paymentInstructions.Any() ? paymentInstructions : null;
			});
		}

		void AddPaymentInstruction(List<PaymentHandlingInstruction> paymentInstructions, ICodeDescription paymentTerms, ZString categoryCode, ZString categoryDescription)
		{
			var paymentTermsPair = GetPaymentTermsPair(paymentTerms);
			if (paymentTermsPair != null)
			{
				paymentInstructions.Add(new PaymentHandlingInstruction
				{
					Category = new CodeDescriptionPair
					{
						Code = categoryCode,
						Description = categoryDescription
					},
					PaymentMethod = paymentTermsPair
				});
			}
		}

		CodeDescriptionPair GetPaymentTermsPair(ICodeDescription paymentTerms)
		{
			if (paymentTerms == null)
			{
				return null;
			}

			if (paymentTerms.Code == Core.Constants.PaymentType.Prepaid)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Prepaid,
					Description = DocDataConstants.Charges.Descriptions.Prepaid
				};
			}

			if (paymentTerms.Code == Core.Constants.PaymentType.Collect)
			{
				return new CodeDescriptionPair
				{
					Code = DocDataConstants.Charges.Codes.Collect,
					Description = DocDataConstants.Charges.Descriptions.Collect
				};
			}

			return null;
		}

		#endregion

		#region AttachedDocuments

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Attributes")]
		void PopulateAttachedDocuments(HouseBill houseBill, UniversalShipment uxmlShipment)
		{
			var fileAttributes = new DataObjectWriterHelper.FileAttributes()
			{
				Name = $"Draft Bill ({houseBill.HouseBillNumber})",
				Description = "Draft Bill of Lading",
				Code = "DBL",
				IsPublished = false
			};

			DataObjectWriterHelper.AppendPopulatePDFAttachedDocument(uxmlShipment, document, fileAttributes, () => houseBill.IsDraft && !houseBill.IsElectronicBOL, applyDraftWatermark: true);
		}

		#endregion
	}
}
