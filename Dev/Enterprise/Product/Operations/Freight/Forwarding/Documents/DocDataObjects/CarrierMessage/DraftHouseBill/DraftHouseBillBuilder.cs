using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AdditionalReferenceCodes = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants.AdditionalReferences.Codes;
using ContainerType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ContainerType;
using DraftHouseBillContainer = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DraftHouseBillContainer;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UnlocoExtensions = Enterprise.Freight.Forwarding.Documents.DocDataObjects.UnlocoExtensions;

namespace Enterprise.Freight.Forwarding.Documents
{
	sealed class DraftHouseBillBuilder
	{
		public DraftHouseBillBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = consol ?? throw new ArgumentNullException(nameof(consol));
			this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
			this.context = new ContextWithCarrierUnlocoMapping(consol.Factory.GetCachedReadOnlyFactory(), consol.IsCoLoad ? consol.CreditorPK : consol.ShippingLinePK, false);
		}

		readonly ForwardingConsol consol;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;

		public DraftHouseBill Build()
		{
			var customBusinessObject = consol is ICustomFieldProvider customFieldProvider ? customFieldProvider.GetCustomBusinessObject() : null;
			var draftHouseBill = new DraftHouseBill(nameof(ForwardingConsol), consol.JK_UniqueConsignRef, customBusinessObject);
			draftHouseBill.IsDraft = true;

			var universalShipmentDraftBOL = parameters?.Data is UniversalShipment uxml
				? uxml
				: consol.GetDraftBillOfLadingUniversalXml();

			PopulateHeaderFields(draftHouseBill, universalShipmentDraftBOL);
			PopulateAddressess(draftHouseBill, universalShipmentDraftBOL);
			PopulateUnlocos(draftHouseBill, universalShipmentDraftBOL);
			PopulateContainers(draftHouseBill, universalShipmentDraftBOL);
			PopulateFreightCharges(draftHouseBill, universalShipmentDraftBOL);

			return draftHouseBill;
		}

		void PopulateHeaderFields(DraftHouseBill draftHouseBill, UniversalShipment universalShipment)
		{
			draftHouseBill.CarrierAgent = GetCarrierAgent(universalShipment);
			draftHouseBill.CarrierBookingReference = universalShipment?.BookingConfirmationReference.GetValueOrDefault() ?? ZString.Empty;
			draftHouseBill.HouseBillNumber = universalShipment?.WayBillNumber.GetValueOrDefault() ?? ZString.Empty;
			draftHouseBill.ExportReference = FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.EHubInterchangeID);
			draftHouseBill.ShippersReference = FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.ShipperReference);
			draftHouseBill.FreightForwarderReference = FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.FreightForwarderReference);
			draftHouseBill.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List) { Code = universalShipment?.ContainerMode?.Code ?? ZString.Empty };
			draftHouseBill.DeliveryMode = universalShipment?.DeliveryMode?.Code.GetValueOrDefault() ?? ZString.Empty;
			draftHouseBill.CarrierContractNumber = FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.CarrierContractNumber);
			draftHouseBill.PreCarriageBy = universalShipment?.TransportLegCollection?.FirstOrDefault(t => t.LegType != null && t.LegType.Value == LegType.PreCarriage).TransportMode.Value.ToString();
			draftHouseBill.VesselName = universalShipment?.VesselName.GetValueOrDefault() ?? ZString.Empty;
			draftHouseBill.Voyage = universalShipment?.VoyageFlightNo.GetValueOrDefault() ?? ZString.Empty;
			draftHouseBill.LloydsIMO = universalShipment?.LloydsIMO.GetValueOrDefault() ?? ZString.Empty;
		}

		ZString GetCarrierAgent(UniversalShipment universalShipment)
		{
			var carrier = AddressBuilder.Create(context, GetAddress(universalShipment, DocAddressType.ShippingLineAddress));

			return carrier.IsNull
				? ZString.Empty
				: !carrier.CompanyName.IsEmpty
					? carrier.CompanyName
					: carrier.RegistrationNumbers?.FirstOrDefault(r => r.Type != null && string.Equals(r.Type.Code, OrgCusCode.CodeTypes.CarrierCode, StringComparison.OrdinalIgnoreCase))?.Value ?? ZString.Empty;
		}

		ZString FindAdditionalReferenceNumber(UniversalShipment universalShipment, string additionalReferenceTypeCode)
		{
			return universalShipment?.AdditionalReferenceCollection?.FirstOrDefault(a => a.Type != null && string.Equals(a.Type.Code.GetValueOrDefault(), additionalReferenceTypeCode, StringComparison.OrdinalIgnoreCase))?.ReferenceNumber ?? ZString.Empty;
		}

		void PopulateAddressess(DraftHouseBill draftHouseBill, UniversalShipment universalShipment)
		{
			draftHouseBill.Shipper = AddressBuilder.Create(context, GetAddress(universalShipment, DocAddressType.ConsignorDocumentaryAddress));
			draftHouseBill.Consignee = AddressBuilder.Create(context, GetAddress(universalShipment, DocAddressType.ConsigneeDocumentaryAddress));
			draftHouseBill.NotifyParty = AddressBuilder.Create(context, GetAddress(universalShipment, DocAddressType.NotifyParty));
		}

		OrganizationAddress GetAddress(UniversalShipment universalShipment, DocAddressType addressType)
		{
			var address = universalShipment?.OrganizationAddressCollection?.FirstOrDefault(address => addressType.ToString().Equals(address.AddressType, StringComparison.OrdinalIgnoreCase));
			if (address?.Country != null && address.Country.Code.HasValue && !address.Country.Code.Value.IsEmpty)
			{
				address.Country.Name = consol.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, (ZString)address.Country.Code)?.RN_Desc ?? string.Empty;
			}
			return address;
		}

		void PopulateUnlocos(DraftHouseBill draftHouseBill, UniversalShipment universalShipment)
		{
			draftHouseBill.PlaceOfReceipt = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PlaceOfReceipt);
			draftHouseBill.PlaceOfDelivery = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PlaceOfDelivery);
			draftHouseBill.PortOfLoading = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PortOfLoading);
			draftHouseBill.PortOfDischarge = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PortOfDischarge);
		}

		void PopulateContainers(DraftHouseBill draftHouseBill, UniversalShipment universalShipment)
		{
			if (universalShipment?.ContainerCollection != null)
			{
				var containers = new List<DraftHouseBillContainer>();
				var packingLinesLinkedToContainer = GetPackingLinesLinkedToConainer(universalShipment);

				foreach (var universalContainer in universalShipment?.ContainerCollection)
				{
					if (universalContainer != null)
					{
						var containerLink = universalContainer?.Link ?? 0;
						List<UniversalPackingLine> packingLines = null;

						if (containerLink > 0)
						{
							packingLinesLinkedToContainer.TryGetValue(containerLink, out packingLines);
						}
						containers.Add(CreateDraftHouseBillContainer(universalContainer, packingLines));
					}
				}
				draftHouseBill.Containers = containers;
			}
		}

		Dictionary<ZInt, List<UniversalPackingLine>> GetPackingLinesLinkedToConainer(UniversalShipment universalShipment)
		{
			var packingLinesLinkedToContainer = new Dictionary<ZInt, List<UniversalPackingLine>>();
			var packingLines = universalShipment?.PackingLineCollection?.Where(x => x.PackingLineCollection != null).SelectMany(x => x.PackingLineCollection)?.Cast<UniversalPackingLine>() ?? Enumerable.Empty<UniversalPackingLine>();

			foreach (var packingLine in packingLines)
			{
				if (packingLine.ContainerLink is ZInt containerLink)
				{
					if (packingLinesLinkedToContainer.ContainsKey(containerLink))
					{
						packingLinesLinkedToContainer[containerLink].Add(packingLine);
					}
					else
					{
						packingLinesLinkedToContainer[containerLink] = new List<UniversalPackingLine>() { packingLine };
					}
				}
			}

			return packingLinesLinkedToContainer;
		}

		DraftHouseBillContainer CreateDraftHouseBillContainer(UniversalContainer universalContainer, IReadOnlyCollection<UniversalPackingLine> packingLines)
		{
			var container = new DraftHouseBillContainer()
			{
				Number = universalContainer.ContainerNumber.GetValueOrDefault(),
				Seal = universalContainer.Seal.GetValueOrDefault(),
				SecondSeal = universalContainer.SecondSeal.GetValueOrDefault(),
				ThirdSeal = universalContainer.ThirdSeal.GetValueOrDefault(),
				Type = new ContainerType(context.ContainerTypes as IFindBoxListProvider)
				{
					Code = universalContainer.ContainerType?.Code.GetValueOrDefault() ?? ZString.Empty,
					ISOCode = universalContainer.ContainerType?.ISOCode.GetValueOrDefault() ?? ZString.Empty
				},
				GrossWeight = new Measurement()
				{
					Value = universalContainer.GrossWeight.GetValueOrDefault(),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = universalContainer.WeightUnit?.Code.GetValueOrDefault() ?? ZString.Empty
					}
				},
				VolumeCapacity = new Measurement()
				{
					Value = universalContainer.VolumeCapacity.GetValueOrDefault(),
					Unit = new CodeDescription(context.VolumeUnits)
					{
						Code = universalContainer.VolumeUnit?.Code.GetValueOrDefault() ?? ZString.Empty
					}
				}
			};

			if (packingLines != null)
			{
				container.PackType = new CodeDescription(new CodeDescriptionPairList())
				{
					Code = packingLines.Count == 1 ? packingLines.FirstOrDefault().PackType.Code.GetValueOrDefault() : Core.Constants.PkgUnit.Package
				};
				container.PackCount = (ZInt)packingLines.Sum(p => p.PackQty.GetValueOrDefault());
				container.MarksAndNumbers = FormatMarksAndNumbers(packingLines);
				container.GoodsDescription = FormatGoodsDescription(packingLines);
			}

			return container;
		}

		ZString FormatMarksAndNumbers(IReadOnlyCollection<UniversalPackingLine> packingLines)
		{
			var marksAndNumbers = new ZStringBuilder();
			foreach (var packingGroup in packingLines.GroupBy(p => p.MarksAndNos.GetValueOrDefault()))
			{
				var markAndNumber = packingGroup.FirstOrDefault().MarksAndNos ?? ZString.Empty;
				if (!markAndNumber.IsEmpty)
				{
					marksAndNumbers.AppendLine(markAndNumber);
				}
			}

			return marksAndNumbers.ToString();
		}

		ZString FormatGoodsDescription(IReadOnlyCollection<UniversalPackingLine> packingLines)
		{
			var goodsDescriptions = new ZStringBuilder();
			foreach (var packingGroup in packingLines.GroupBy(p => p.DetailedDescription.GetValueOrDefault()))
			{
				var goodsDescription = packingGroup.FirstOrDefault().DetailedDescription ?? ZString.Empty;
				if (!goodsDescription.IsEmpty)
				{
					goodsDescriptions.AppendLine(goodsDescription);
				}
			}

			var undgStrings = new List<string>();
			foreach (var packingLine in packingLines)
			{
				if (packingLine.UNDGCollection != null)
				{
					foreach (var u in packingLine.UNDGCollection)
					{
						undgStrings.Add($"{u.PackQty.GetValueOrDefault()} {u.PackType?.Code.GetValueOrDefault()} of {u.UNDGCode.GetValueOrDefault()} {u.ProperShippingName.GetValueOrDefault()} {u.TechicalName.GetValueOrDefault()} Class {u.IMOClass.GetValueOrDefault()} {u.SubLabel1.GetValueOrDefault()} {u.SubLabel2.GetValueOrDefault()} {u.FlashPoint.GetValueOrDefault()} C c.c. PG {u.PackingGroup.GetValueOrDefault()} {u.MarinePollutant?.Code.GetValueOrDefault()} {u.PackedInLimitedQuantity.GetValueOrDefault()} {u.EmergencyScheduleFire?.Code.GetValueOrDefault()} {u.EmergencyScheduleSpillage?.Code.GetValueOrDefault()} Contact {u.Contact?.FullName.GetValueOrDefault()} {u.Contact?.Phone.GetValueOrDefault()} Net Weight {u.Weight.GetValueOrDefault()} {u.WeightUQ?.Code.GetValueOrDefault()}");
					}
				}
			}

			var undgString = string.Join(System.Environment.NewLine, undgStrings.Distinct());
			if (!undgString.IsNullOrEmpty())
			{
				goodsDescriptions.AppendLine(undgString);
			}

			return goodsDescriptions.ToString();
		}

		void PopulateFreightCharges(DraftHouseBill houseBill, UniversalShipment universalShipment)
		{
			houseBill.FreightCharges = FormatFreightCharges(universalShipment);
			var freightPayableAt = new UNLOCO() { Code = GetAdditionalInfo(universalShipment, $"{DocDataConstants.AddinfoTypes.FreightPayableAt}_{nameof(UNLOCO.Code)}"), Name = GetAdditionalInfo(universalShipment, $"{DocDataConstants.AddinfoTypes.FreightPayableAt}_{nameof(UNLOCO.Name)}") };
			houseBill.FreightPayableAt = UnlocoExtensions.CreateFromUNLOCO(context, freightPayableAt);
			houseBill.DeclaredValueOfGoods = new DocDataObjects.Money()
			{
				Amount = universalShipment?.GoodsValue.GetValueOrDefault() ?? 0.0,
				Currency = new CodeDescription(new CodeDescriptionPairList())
				{
					Code = universalShipment?.GoodsValueCurrency?.Code.GetValueOrDefault() ?? ZString.Empty
				}
			};
			houseBill.PlaceOfIssue = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PlaceOfIssue);
			houseBill.DateOfIssue = GetDate(universalShipment, nameof(DateType.BillIssued));
			houseBill.ShippedOnBoard = new ShippedOnBoard(new CodeDescriptionPairList())
			{
				Code = nameof(DateType.ShippedOnBoard),
				Date = GetDate(universalShipment, nameof(DateType.ShippedOnBoard))
			};
			houseBill.Clause = GetNotes(universalShipment, (NoResString)"Bill Clause Notes");
		}

		ZString FormatFreightCharges(UniversalShipment universalShipment)
		{
			var freightCarges = new ZStringBuilder();

			if (universalShipment?.PaymentHandlingInstructionCollection != null)
			{
				foreach (var paymentHandlingInstruction in universalShipment?.PaymentHandlingInstructionCollection)
				{
					switch (paymentHandlingInstruction.Category.Code)
					{
						case DocDataConstants.Charges.Categories.Codes.Freight:
							freightCarges.AppendLine($"{DocDataConstants.Charges.Categories.Descriptions.FreightCharges}: {GetPaymentMethod(paymentHandlingInstruction)}");
							break;
						case DocDataConstants.Charges.Categories.Codes.OriginPort:
							freightCarges.AppendLine($"{DocDataConstants.Charges.Categories.Descriptions.OriginPortCharge}: {GetPaymentMethod(paymentHandlingInstruction)}");
							break;
						case DocDataConstants.Charges.Categories.Codes.OriginHaulage:
							freightCarges.AppendLine($"{DocDataConstants.Charges.Categories.Descriptions.OriginHaulage}: {GetPaymentMethod(paymentHandlingInstruction)}");
							break;
						case DocDataConstants.Charges.Categories.Codes.DestinationHaulage:
							freightCarges.AppendLine($"{DocDataConstants.Charges.Categories.Descriptions.DestinationHaulage}: {GetPaymentMethod(paymentHandlingInstruction)}");
							break;
						case DocDataConstants.Charges.Categories.Codes.DestinationPort:
							freightCarges.AppendLine($"{DocDataConstants.Charges.Categories.Descriptions.DestinationPortCharge}: {GetPaymentMethod(paymentHandlingInstruction)}");
							break;
					}
				}
			}

			return freightCarges.ToString();
		}

		ZString GetPaymentMethod(PaymentHandlingInstruction paymentHandlingInstruction)
		{
			if (paymentHandlingInstruction != null && paymentHandlingInstruction.PaymentMethod != null)
			{
				return paymentHandlingInstruction.PaymentMethod.Code.GetValueOrDefault() == DocDataConstants.Charges.Codes.Prepaid ? DocDataConstants.Charges.Descriptions.Prepaid : DocDataConstants.Charges.Descriptions.Collect;
			}

			return ZString.Empty;
		}

		ZString GetAdditionalInfo(UniversalShipment universalShipment, string additionalInfoKey)
		{
			if (universalShipment?.AddInfoCollection != null)
			{
				foreach (var addInfo in universalShipment?.AddInfoCollection)
				{
					if (string.Equals(addInfo.Key.GetValueOrDefault(), additionalInfoKey, StringComparison.OrdinalIgnoreCase))
					{
						return addInfo.Value ?? ZString.Empty;
					}
				}
			}

			return ZString.Empty;
		}

		ZDateTime GetDate(UniversalShipment universalShipment, string dateType)
		{
			if (universalShipment?.DateCollection != null)
			{
				foreach (var date in universalShipment?.DateCollection)
				{
					if (string.Equals(date.Type.ToString(), dateType, StringComparison.OrdinalIgnoreCase))
					{
						return date.Value ?? ZDateTime.Empty;
					}
				}
			}

			return ZDateTime.Empty;
		}

		ZString GetNotes(UniversalShipment universalShipment, string noteDescription)
		{
			if (universalShipment?.NoteCollection != null)
			{
				foreach (var note in universalShipment?.NoteCollection)
				{
					if (string.Equals(note.Description.GetValueOrDefault(), noteDescription, StringComparison.OrdinalIgnoreCase))
					{
						return note.NoteText ?? ZString.Empty;
					}
				}
			}

			return ZString.Empty;
		}
	}
}
