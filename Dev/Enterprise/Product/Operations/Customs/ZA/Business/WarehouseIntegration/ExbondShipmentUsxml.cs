using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using static Enterprise.Core.Constants;
using DataContext = Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.Business.WarehouseIntegration
{
	public static class ExbondShipmentUsxml
	{
		public enum Mode
		{
			ExWarehouse,
			BelnShipment,
			BelnExbond,
			NonBeln,
			UnderReceipts,
		}

		public static string[] CreateEDIMessage(BusinessObjectFactory factory, ZGuid warehousePK, IEnumerable<ExbondEntryLine> lines, Mode mode)
		{
			var shipments = CreateUniversalShipment(factory, warehousePK, lines, mode).ToArray();
			return shipments.Select(shipment =>
			{
				if (shipment != null)
				{
					using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
					{
						new XmlWriter().WriteXML(shipment, stream, writeXMLDeclaration: false, UniversalXmlInfo.Namespace_2011_11);

						var ediMessage = factory.New<ExbondShipmentUniversalShipmentXMLMessage>();
						ediMessage.SetEM_MessageTextOrDataSource(stream);
						factory.Save();

						return (string)ediMessage.EM_MessageNum;
					}
				}
				return null;
			}).ToArray();
		}

		static IEnumerable<UniversalShipment> CreateUniversalShipment(IFactory factory, ZGuid warehousePK, IEnumerable<ExbondEntryLine> lines, Mode mode)
		{
			var limitLineCount = mode == Mode.ExWarehouse ? ZACustomsRegistry.Instance.MaxNumberLinesPerHomeConsumptionFile.Value : 0;
			var warehouseAddress = factory.Load<OrgAddress>(warehousePK);

			var entryInstructions = new List<(ZGuid? warehouse, string entryStyle, EntryInstructionType instructionType)>();
			var singleOwnerPK = ZGuid.Empty;

			var filteredLines = lines.Where(line => line.IsCustomsControlled || mode == Mode.NonBeln || mode == Mode.BelnShipment);
			var fileLineCount = 0;

			UniversalShipment currentShipment = null;

			foreach (var ownerReference in filteredLines.Select(line => line.OwnerReference).DistinctBy(r => r))
			{
				CommercialInvoiceHeader header = null;

				var totalAmount = 0.0m;
				string currency = null;
				ZDate? date = null;
				var lineNo = 0;
				foreach (var line in filteredLines.Where(line => line.OwnerReference == ownerReference))
				{
					if (currentShipment == null)
					{
						currentShipment = InitialiseUniversalShipment(warehouseAddress, mode);
					}
					if (header == null)
					{
						header = CreateCommercialInvoiceHeader(ownerReference);
					}
					var commercialInvoiceLine = CreateCommercialInvoiceLineFromExbondEntryLine(line, mode, ++lineNo, entryInstructions);
					header.CommercialInvoiceLineCollection.Add(commercialInvoiceLine);

					totalAmount += commercialInvoiceLine.LinePrice ?? 0m;
					var thisCurrency = line.Currency.IsEmpty ? "ZAR" : (string)line.Currency;
					if (string.IsNullOrEmpty(currency))
					{
						currency = thisCurrency;
					}
					else if (currency != thisCurrency)
					{
						throw new ArgumentException("Invoice lines cannot have different currencies for the same invoice");
					}
					if (date == null || (line.InvoiceDate.IsValid && line.InvoiceDate < date))
					{
						date = line.InvoiceDate;
					}

					if (singleOwnerPK.IsEmpty)
					{
						singleOwnerPK = line.Owner;
					}
					else if (singleOwnerPK != line.Owner)
					{
						throw new ArgumentException("Owner must be the same for all lines");
					}

					if (++fileLineCount == limitLineCount)
					{
						FinaliseHeader(ref header, totalAmount, currency, date, currentShipment);
						yield return FinaliseShipment(ref currentShipment, ref singleOwnerPK, factory, entryInstructions, mode);
						fileLineCount = 0;
						lineNo = 0;
						totalAmount = 0.0m;
					}
				}

				if (header != null)
				{
					FinaliseHeader(ref header, totalAmount, currency, date, currentShipment);
				}
			}

			if (currentShipment != null)
			{
				yield return FinaliseShipment(ref currentShipment, ref singleOwnerPK, factory, entryInstructions, mode);
			}
		}

		static UniversalShipment InitialiseUniversalShipment(OrgAddress warehouseAddress, Mode mode)
		{
			var dataTargetCollection = new List<DataTarget>
			{
				new DataTarget
				{
					Type = "CustomsDeclaration",
					Key = string.Empty
				}
			};
			if (mode == Mode.NonBeln || mode == Mode.BelnShipment)
			{
				dataTargetCollection.Add(new DataTarget { Type = "ForwardingShipment", Key = string.Empty });
			}

			var messageTypeCode = mode == Mode.ExWarehouse || mode == Mode.BelnExbond || mode == Mode.UnderReceipts ? JobMessageTypeList.Codes.ExWarehouse : JobMessageTypeList.Codes.Export;

			return new UniversalShipment(DefaultDataObjectWriterStrategy.Instance)
			{
				DataContext = new DataContext
				{
					DataTargetCollection = dataTargetCollection,
					Company = new Company { Code = GlbCompany.CurrentCompany.GC_Code, Country = new Country { Code = GlbCompany.CurrentCompany.Country.RN_Code } },
					DataProvider = ZArchitecture.Environment.CompanyExtensions.GetLicenceCode(GlbCompany.CurrentCompany),
					EnterpriseID = GlbCompany.CurrentCompany.LicenceEnterpriseCode,
					EventBranch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code },
					EventDepartment = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code }
				},
				AdditionalTerms = ZString.Empty,
				AgentsReference = ZString.Empty,
				Branch = Branch.New(GlbBranch.CurrentBranch),
				CommercialInfo = new CommercialInfo
				{
					Name = "All Invoices",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						Content = CollectionContent.Complete
					}
				},
				CustomsOffice = new CodeDescriptionPair10Char { Code = warehouseAddress?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, CountryCodes.SouthAfrica).SubstringSafe(0, 3) ?? ZString.Empty },
				MessageType = new CodeDescriptionPair { Code = messageTypeCode },
				MessagingApplicationCode = new CodeDescriptionPair { Code = ApplicationCodeList.Codes.ZATransactionOrders },
				OwnerRef = ZString.Empty,
				PaymentMethod = new CodeDescriptionPair { Code = PaidByCodeList.Codes.BRK },
				ServiceLevel = new ServiceLevel { Code = "STD" },
				ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = "FOB" },
				TransportMode = new CodeDescriptionPair { Code = ZString.Empty },
			};
		}

		static CommercialInvoiceHeader CreateCommercialInvoiceHeader(ZString ownerReference)
		{
			return new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.Instance)
			{
				InvoiceNumber = ownerReference,
				IncoTerm = new CodeDescriptionPair { Code = "FOB" },
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()));
		}

		static CommercialInvoiceLine CreateCommercialInvoiceLineFromExbondEntryLine(ExbondEntryLine line, Mode mode, int lineNo,
			List<(ZGuid? warehouse, string entryStyle, EntryInstructionType instructionType)> entryInstructions)
		{
			string entryStyle;
			ZDecimal? price;
			switch (mode)
			{
				case Mode.ExWarehouse:
				case Mode.BelnExbond:
				case Mode.UnderReceipts:
					entryStyle = "11";
					price = line.PriceExbond;
					break;
				case Mode.NonBeln:
					entryStyle = line.IsCustomsControlled ? "67" : "60";
					price = line.Price;
					break;
				case Mode.BelnShipment:
					entryStyle = "61";
					price = line.Price;
					break;
				default:
					throw new ArgumentException($"Value of {nameof(mode)} {mode} is not valid");
			}
			price = RoundToRequiredDecimals(price, mode, line.IsCustomsControlled, isPrice: true);

			var commercialInvoiceLine = new CommercialInvoiceLine
			{
				LineNo = lineNo,
				BondedWarehouseQuantity = mode == Mode.UnderReceipts ? line.Quantity : line.CountableQty,
				CountryOfOrigin = new Country { Code = line.CountryOfOrigin },
				CustomsQuantity = RoundToRequiredDecimals(line.CustomsQuantity, mode, line.IsCustomsControlled, isPrice: false),
				CustomsSecondQuantity = line.AdditionalQty1,
				CustomsThirdQuantity = line.AdditionalQty2,
				DataImportMatchingKey = line.DataImportMatchingKey,
				EntryInstructionLink = GetEntryInstructionLink(entryInstructions, line.Warehouse, entryStyle, mode, line.IsCustomsControlled,
					line.CountryOfOrigin != CountryCodes.SouthAfrica),
				HarmonisedCode = line.TariffCode,
				InvoiceQuantity = line.Quantity,
				LinePrice = price,
				PartNo = line.ProductCode,
				TaxType = new CodeDescriptionPair4Char { Code = "VAT" }
			};

			if (line.IsCustomsControlled && !string.IsNullOrEmpty(line.CountableUom))
			{
				commercialInvoiceLine.BondedWarehouseQuantityUnit = new CodeDescriptionPair { Code = line.CountableUom };
			}

			if (line.IsCustomsControlled && mode != Mode.BelnShipment)
			{
				commercialInvoiceLine.PreviousEntryLineNumber = line.MRNLine;
				commercialInvoiceLine.PreviousEntryNumber = line.MRN;
				commercialInvoiceLine.Procedure = entryStyle + (line.PreviousProcedure ?? "00");
			}
			else
			{
				commercialInvoiceLine.Procedure = entryStyle + "00";
			}
			return commercialInvoiceLine;
		}

		static void FinaliseHeader(ref CommercialInvoiceHeader header, decimal totalAmount, string currency, ZDate? date, UniversalShipment currentShipment)
		{
			header.InvoiceAmount = totalAmount;
			header.InvoiceCurrency = new Currency { Code = currency };
			header.InvoiceDate = date;
			currentShipment.CommercialInfo.CommercialInvoiceCollection.Add(header);
			header = null;
		}

		static UniversalShipment FinaliseShipment(ref UniversalShipment shipment, ref ZGuid singleOwnerPK, IFactory factory,
			List<(ZGuid? warehouse, string entryStyle, EntryInstructionType instructionType)> entryInstructions, Mode mode)
		{
			shipment.SetEntryInstructionCollection(() => entryInstructions.Select((w, index) =>
				new EntryInstruction
				{
					Description = GetDescriptionFromEntryInstructionType(w.instructionType),
					Link = index + 1,
					MergeBy = mode == Mode.UnderReceipts ? null : new CodeDescriptionPair { Code = "NON" },
					Style = w.entryStyle,
					OrganizationAddressCollection = CreateOrganizationAddressList(factory, "Warehouse1", w.warehouse)
				}
			).ToList());

			if (singleOwnerPK.IsValid)
			{
				var addressType = mode == Mode.ExWarehouse || mode == Mode.BelnExbond || mode == Mode.UnderReceipts ? "ConsigneeDocumentaryAddress" : "ConsignorDocumentaryAddress";
				var owner = factory.Load<OrgHeader>(singleOwnerPK);
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					CreateOrganizationAddress(addressType, owner?.MainAddress)
				});
			}

			entryInstructions.Clear();
			singleOwnerPK = ZGuid.Empty;
			var result = shipment;
			shipment = null;
			return result;
		}

		static ZDecimal? RoundToRequiredDecimals(ZDecimal? value, Mode mode, bool isCustomsControlled, bool isPrice)
		{
			var result = value;

			if (value is ZDecimal theValue &&
				(mode == Mode.ExWarehouse || mode == Mode.BelnExbond || mode == Mode.UnderReceipts || (mode == Mode.NonBeln && isCustomsControlled)))
			{
				if (!isPrice && theValue > 0 && theValue < 0.01)
				{
					result = 0.01m;
				}
				else
				{
					result = theValue.Round(2);
				}
			}

			return result;
		}

		enum EntryInstructionType
		{
			GoodsFromBondedWarehouse,
			ImportedGoods,
			LocalGoods,
			ExportOfGoods
		}

		static string GetDescriptionFromEntryInstructionType(EntryInstructionType instructionType)
		{
			switch (instructionType)
			{
				case EntryInstructionType.GoodsFromBondedWarehouse:
					return "Goods from Bonded warehouse";
				case EntryInstructionType.ImportedGoods:
					return "Imported Goods";
				case EntryInstructionType.LocalGoods:
					return "Local Goods";
				case EntryInstructionType.ExportOfGoods:
					return "Export of Goods";
				default:
					return "Unexpected goods";
			}
		}

		static int GetEntryInstructionLink(List<(ZGuid? warehouse, string entryStyle, EntryInstructionType instructionType)> entryInstructions, ZGuid? warehouse,
			string entryStyle, Mode mode, bool isCustomsControlled, bool isImported)
		{
			var instructionType = EntryInstructionType.GoodsFromBondedWarehouse;
			switch (mode)
			{
				case Mode.BelnExbond:
				case Mode.ExWarehouse:
				case Mode.UnderReceipts:
					break;
				case Mode.NonBeln:
					if (!isCustomsControlled)
					{
						warehouse = ZGuid.Invalid;
						instructionType = isImported ? EntryInstructionType.ImportedGoods : EntryInstructionType.LocalGoods;
					}
					break;
				case Mode.BelnShipment:
					warehouse = ZGuid.Invalid;
					instructionType = EntryInstructionType.ExportOfGoods;
					break;
				default:
					throw new ArgumentException($"Value of {nameof(mode)} is not valid");
			}

			int index = 0;
			foreach (var item in entryInstructions)
			{
				if (item.warehouse == warehouse && item.entryStyle == entryStyle && item.instructionType == instructionType)
				{
					break;
				}
				index++;
			}
			if (index == entryInstructions.Count)
			{
				entryInstructions.Add((warehouse, entryStyle, instructionType));
			}
			return index + 1;
		}

		static List<OrganizationAddress> CreateOrganizationAddressList(IFactory factory, string addressType, ZGuid? addressPK)
		{
			return (addressPK?.IsValid ?? false)
				? new List<OrganizationAddress> { CreateOrganizationAddress(addressType, factory.Load<OrgAddress>(addressPK.Value)) }
				: null;
		}

		static OrganizationAddress CreateOrganizationAddress(string addressType, OrgAddress orgAddress)
		{
			return orgAddress == null
				? null
				: new OrganizationAddress
				{
					AddressType = addressType,
					AddressShortCode = orgAddress.OA_Code,
					OrganizationCode = orgAddress.Header.OH_Code
				};
		}
	}
}
