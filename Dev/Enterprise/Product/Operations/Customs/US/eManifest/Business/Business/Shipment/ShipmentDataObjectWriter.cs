using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentDataObjectWriter : TopLevelDataObjectWriter<Shipment, UniversalXml.Shipment>
	{
		public ShipmentDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		internal static ShipmentDataObjectWriter New(IDataWritingManager manager, Shipment shipmentBO, Trip tripBO)
		{
			var helper = new ShipmentDataObjectWriterHelper(shipmentBO, tripBO);
			return new ShipmentDataObjectWriter(manager, helper);
		}

		public ShipmentDataObjectWriter(IDataWritingManager writeManager, ShipmentDataObjectWriterHelper helper)
			: base(writeManager)
		{
			this.helper = helper;
		}
		readonly public ShipmentDataObjectWriterHelper helper;

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}
		protected sealed override IDataContextManager GetDataContextManager(BusinessObject sourceBO)
		{
			return new ShipmentDataContextManager();
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.USeManifestShipment;
		}

		protected override void PopulateDataObject(Shipment shipmentBO, UniversalXml.Shipment dataObject)
		{
			PopulateDataObjectCore(shipmentBO, dataObject);
		}

		void PopulateDataObjectCore(Shipment shipmentBO, UniversalXml.Shipment shipment)
		{
			shipment.MessageType = new CodeDescriptionPair() { Code = Core.Constants.FreightShipmentDirection.Code.Import, Description = Core.Constants.FreightShipmentDirection.Description.Import };
			shipment.Branch = Branch.New(helper.tripBO.Branch);
			shipment.WayBillNumber = helper.tripBO.BH_VoyageNumber;
			shipment.VoyageFlightNo = helper.tripBO.BH_VoyageNumber;

			PopulateOrganisation(shipment);
			PopulateAddInfo(shipment);
			PopulateAdditionalBillCollection(shipment);

			PopulateShipmentData(shipment);
			PopulateTransportModeAndContainerMode(shipmentBO, shipment);
			PopulateDates(shipmentBO, shipment);

			PopulateCommercialData(shipmentBO, shipment);
		}

		void PopulateAdditionalBillCollection(UniversalXml.Shipment shipment)
		{
			shipment.SetAdditionalBillCollection(() =>
			{
				var billCollection = shipment.AdditionalBillCollection ?? new List<AdditionalBill>()
				{
					new AdditionalBill(writeManager.WriterStrategy)
					{
						BillNumber = helper.tripBO.BH_VoyageNumber,
						BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master },
						AddInfoCollection = new List<UniversalXml.AddInfo>()
						{
							new UniversalXml.AddInfo()
							{ Key = Constants.AddInfoKeys.Declaration.NKIssuerSCAC, Value = helper.tripBO.BH_CarrierSCAC }
						}
					}
				};
				return billCollection;
			});
		}

		void PopulateAddInfo(UniversalXml.Shipment shipment)
		{
			shipment.SetAddInfoCollection(() =>
			{
				var addInfoCollection = shipment.AddInfoCollection ?? new List<UniversalXml.AddInfo>();
				var masterBillNumber = helper.tripBO.BH_VoyageNumber;
				if (!masterBillNumber.IsEmpty)
				{
					helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.MasterWayBillNumber, masterBillNumber);
					helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.MasterWayBillIssuerSCAC, helper.tripBO.BH_CarrierSCAC);
					helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC, helper.tripBO.BH_CarrierSCAC);
				}
				helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.NKCarrierSCAC, helper.tripBO.BH_CarrierSCAC);

				if (!helper.tripBO.Shipments.Any(x => x.B0_ShipmentType == ShipmentTypes.Codes.Inbond))
				{
					helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.SchDEntry, helper.tripBO.BH_PortUnladingDCode);
					helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.EntryDate, helper.tripBO.BH_ETA);
				}

				var firmcode = helper.tripBO.Shipments.FirstOrDefault(x => !x.B0_Firms.IsEmpty)?.B0_Firms ?? ZString.Empty;
				if (!firmcode.IsEmpty && helper.tripBO.Shipments.All(x => x.B0_Firms == firmcode))
				{
					helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.FIRM, firmcode);
				}

				helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.SchDArrival, helper.tripBO.BH_PortUnladingDCode);

				return addInfoCollection;
			});
		}

		void PopulateOrganisation(UniversalXml.Shipment shipment)
		{
			shipment.AddOrgAddress(writeManager, helper.tripBO.Importer, DocAddressType.ConsigneeDocumentaryAddress);
			shipment.AddOrgAddress(writeManager, helper.tripBO.Carrier, AddressTypes.ShippingLine);

			var firstShipment = helper.tripBO.Shipments.FirstOrDefault(x => x.Consignee != null);
			var consignee = firstShipment.Consignee.PK;
			if (helper.tripBO.Shipments.All(x => x.Consignee.PK == consignee))
			{
				shipment.AddOrgAddress(writeManager, firstShipment.Consignee.Address, DocAddressType.UltimateConsignee);
			}
		}

		void PopulateShipmentData(UniversalXml.Shipment shipment)
		{
			var listShipments = helper.tripBO.Shipments.ToList();

			var firstShipment = helper.tripBO.Shipments.FirstOrDefault(x => !x.B0_ManifestUQ.IsEmpty);
			var maniUQ = firstShipment?.B0_ManifestUQ ?? ZString.Empty;
			if (!maniUQ.IsEmpty && helper.tripBO.Shipments.All(x => x.B0_ManifestUQ == maniUQ))
			{
				shipment.OuterPacks = listShipments.Sum(x => x.B0_ManifestQty);
				shipment.OuterPacksPackageType = ListHelper.GetWithDescription<PackageType>(firstShipment.B0_ManifestUQ, firstShipment.Lookups.QuantityUnits);
			}

			firstShipment = helper.tripBO.Shipments.FirstOrDefault(x => !x.B0_WeightUQ.IsEmpty);
			var weightUQ = firstShipment?.B0_WeightUQ ?? ZString.Empty;
			if (!weightUQ.IsEmpty && helper.tripBO.Shipments.All(x => x.B0_WeightUQ == weightUQ))
			{
				shipment.TotalWeight = listShipments.Sum(x => x.B0_Weight);
				shipment.TotalWeightUnit = new UnitOfWeight() { Code = weightUQ, Description = Core.Constants.Weight.GetDescription(weightUQ, Core.Constants.PluralState.NonPlural) };
			}

			firstShipment = helper.tripBO.Shipments.FirstOrDefault(x => !(x.Commodities.Count > 0));
			if (listShipments.Count == 1 && firstShipment != null)
			{
				shipment.GoodsDescription = firstShipment.B0_DescriptionOfCargo;
			}
		}

		protected virtual void PopulateDates(Shipment shipmentBO, UniversalXml.Shipment shipment)
		{
			shipment.SetDateCollection(() =>
			{
				var list = new List<Date>();
				list.Add(DateType.Departure, ZBool.True, helper.tripBO.BH_ETA);
				list.Add(DateType.DischargeDate, ZBool.False, helper.tripBO.BH_ETA);

				if (!helper.tripBO.Shipments.Any(x => x.B0_ShipmentType == ShipmentTypes.Codes.Inbond))
				{
					list.Add(DateType.EntryDate, ZBool.False, helper.tripBO.BH_ETA);
				}

				return shipment.DateCollection.MergeCollectionByCandidateKey(list, true);
			});
		}

		protected virtual void PopulateCommercialData(Shipment shipmentBO, UniversalXml.Shipment shipment)
		{
			var invoiceData = new UniversalXml.Customs.CommercialInvoiceHeader(writeManager.WriterStrategy);
			if (invoiceData.OrganizationAddressCollection == null)
			{
				invoiceData.OrganizationAddressCollection = new List<OrganizationAddress>();
			}

			var parties = GetSupportedParties(helper.thisShipmentBO.Parties);
			parties.ForEach(party => invoiceData.AddOrgAddress(writeManager, party.org, party.docType));

			invoiceData.SetCommercialInvoiceLineCollection(() =>
			{
				var lineNo = 0;
				var invoiceLineDataList = new DataObjectList<UniversalXml.Customs.CommercialInvoiceLine>();
				foreach (var oneCommInvLine in shipmentBO.Commodities)
				{
					var invoiceLineData = new UniversalXml.Customs.CommercialInvoiceLine(writeManager.WriterStrategy);
					invoiceLineData.LineNo = lineNo++;
					invoiceLineData.InvoiceQuantity = new ZDecimal(oneCommInvLine.BY_PieceCount);
					invoiceLineData.InvoiceQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(oneCommInvLine.BY_ManifestUnitCode, oneCommInvLine.Lookups.QuantityUnits);
					invoiceLineData.Description = oneCommInvLine.BY_Description;
					invoiceLineData.Weight = oneCommInvLine.BY_GrossWeight;
					invoiceLineData.WeightUnit = new UnitOfWeight() { Code = oneCommInvLine.BY_GrossWeightUnit };
					invoiceLineData.OrganizationAddressCollection = new List<OrganizationAddress>();
					if (oneCommInvLine.HarmonizedNumbers.Any())
					{
						invoiceLineData.AdditionalLineTariffDetailCollection = new List<UniversalXml.Customs.AdditionalLineTariffDetail>()
						{
							new UniversalXml.Customs.AdditionalLineTariffDetail() { Tariff = oneCommInvLine.HarmonizedNumbers[0].CY_TariffFormatted }
						};
					}

					PopulateHazardousMaterial(oneCommInvLine, invoiceLineData);
					parties.ForEach(party => invoiceLineData.AddOrgAddress(writeManager, party.org, party.docType));
					invoiceLineDataList.Add(invoiceLineData);
				}
				return invoiceLineDataList;
			});

			shipment.CommercialInfo = new UniversalXml.Customs.CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalXml.Customs.CommercialInvoiceHeader>(new[] { invoiceData })
			};
		}

		void PopulateTransportModeAndContainerMode(Shipment tripBO, UniversalXml.Shipment shipment)
		{
			shipment.TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Truck, Description = Core.Constants.TransportModes.Truck };
			var containerCode = ContainerModeList.Codes.NonContainerized;
			shipment.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(containerCode, tripBO.Lookups.ContainerModeList);
		}

		(OrgHeader org, ZString docType)[] GetSupportedParties(PartyCollection addresses)
		{
			var result = new List<(OrgHeader org, ZString docType)>();
			foreach ((string addressType, string docType) in new (string addressType, string docType)[]
				{
					(PartyTypes.Codes.ManufacturerOfGoods, nameof(DocAddressType.Manufacturer)),
					(PartyTypes.Codes.Seller, AddressType.Seller),
					(PartyTypes.Codes.Exporter, AddressType.ForeignExporter),
					(PartyTypes.Codes.Buyer, nameof(DocAddressType.BuyingParty)),
					(PartyTypes.Codes.Importer, AddressTypes.Importer),
					(PartyTypes.Codes.ShipTo, AddressType.ShipToParty),
					(PartyTypes.Codes.SoldToAndShipTo, AddressType.SoldToParty)
				})
			{
				var address = addresses.FirstOrDefault(x => x.E2_AddressType == addressType);
				if (address != null && address.Organisation is OrgHeader organisation)
				{
					result.Add((organisation, docType));
				}
			}
			return result.ToArray();
		}

		void PopulateHazardousMaterial(Commodity commodityBO, UniversalXml.Customs.CommercialInvoiceLine invoiceLineData)
		{
			if (commodityBO.HarmonizedNumbers.Any())
			{
				invoiceLineData.HarmonisedCode = commodityBO.HarmonizedNumbers[0].CY_TariffFormatted;
			}

			var hazardousMaterialData = new UniversalXml.Customs.HazardousMaterial()
			{
				Code = commodityBO.Factory.Load<UNDGSubstance>(commodityBO.BY_HazardousGoodsIdentifier)?.DG_Code
			};

			var undgs = helper.Load<UNDGDataItem>(new ZQuery(UNDGDataItemSchema.DI_ParentID, commodityBO.PK));
			if (undgs != null)
			{
				hazardousMaterialData.UNDGCollection = ProcessCollection(undgs, new UNDGDataObjectWriter(writeManager));
			}
			invoiceLineData.HazardousMaterial = hazardousMaterialData;
		}
	}
}
