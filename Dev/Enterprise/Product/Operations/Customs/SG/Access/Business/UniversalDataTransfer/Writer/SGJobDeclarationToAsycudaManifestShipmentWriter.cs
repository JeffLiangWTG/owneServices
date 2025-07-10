using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGJobDeclarationToAsycudaManifestShipmentWriter : ASYCUDA.Business.UniversalDataTransfer.JobDeclarationToAsycudaManifestShipmentWriter
	{
		public SGJobDeclarationToAsycudaManifestShipmentWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override ZDecimal GetExciseAmount(BaseJobComInvoiceLine invoiceLine)
		{
			return ((JobComInvoiceLine)invoiceLine).JI_Calc_ExciseAmount;
		}

		protected override void PopulateHeaderData(BaseJobDeclaration baseDeclaration, Shipment shipment)
		{
			var declaration = (JobDeclaration)baseDeclaration;
			if (declaration.IsExport)
			{
				shipment.WayBillNumber = declaration.SG_OutwardMAWB;
			}
			else
			{
				shipment.WayBillNumber = declaration.JE_MasterBill;
			}

			shipment.SetEntryHeaderCollection(() =>
			{
				var result = new List<EntryHeader>();
				var entryHeader = new EntryHeader(writeManager.WriterStrategy)
				{
					Type = new EntryType
					{
						Code = Core.Constants.CountryCodes.Singapore
					},
					EntryInstructionLink = 1
				};
				result.Add(entryHeader);
				return result;
			});
			shipment.SetEntryInstructionCollection(() =>
			{
				var result = new List<EntryInstruction>();
				var entryInstruction = new EntryInstruction(writeManager.WriterStrategy)
				{
					Link = 1
				};
				result.Add(entryInstruction);
				PopulateHeaderEntryInstruction(entryInstruction, declaration);
				return result;
			});
			PopulateCustomizedFields(shipment, declaration);
		}

		void PopulateHeaderEntryInstruction(EntryInstruction entryInstruction, JobDeclaration declaration)
		{
			if (declaration.IsImport)
			{
				entryInstruction.AddOrgAddress(writeManager, declaration.InwardCarrierAgent, DocAddressType.ControllingAgent);
			}
			else if (declaration.IsExport)
			{
				entryInstruction.AddOrgAddress(writeManager, declaration.OutwardShippingLineForwarderDocAddress.Organisation, DocAddressType.ControllingAgent);
			}
		}

		protected override void PopulateHouseBillMainData(Shipment shipment, BaseJobDeclaration declaration)
		{
			base.PopulateHouseBillMainData(shipment, declaration);

			if (IsOVRDeclaration(declaration))
			{
				var gstnReferenceNo = GetGSTNReferenceNo(declaration);
				if (!gstnReferenceNo.IsEmpty)
				{
					AddAddInfo(shipment, AddInfoConstants.BillCountry.GSTNReferenceNo, gstnReferenceNo, false);
				}
			}
		}

		ZString GetGSTNReferenceNo(BaseJobDeclaration declaration)
		{
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				foreach (CASCCode1 cascCode1 in invoiceLine.CASCCode1s)
				{
					if (!cascCode1.CY_Data.IsEmpty)
					{
						return cascCode1.CY_Data;
					}
				}
			}

			return ZString.Empty;
		}

		protected override ZString GetHouseBillNumber(BaseJobDeclaration baseDeclaration)
		{
			ZString result;
			var declaration = (JobDeclaration)baseDeclaration;
			if (declaration.IsExport)
			{
				result = declaration.SG_OutwardHAWB;
			}
			else
			{
				result = declaration.JE_HouseBill;
			}
			return result;
		}

		protected override void PopulatePackingLineData(PackingLine packingLineData, BaseJobComInvoiceLine invoiceLine, Money linePrice)
		{
			base.PopulatePackingLineData(packingLineData, invoiceLine, linePrice);
			var sgInvoiceLine = (JobComInvoiceLine)invoiceLine;
			packingLineData.MarksAndNos = sgInvoiceLine.MarksAndNumbers;
		}

		protected override void PopulateAdditionalPackingLineCountryData(List<UniversalXml.AddInfo> addInfoCollection, BaseJobDeclaration declaration, BaseJobComInvoiceHeader invoice, BaseJobComInvoiceLine invoiceLine)
		{
			base.PopulateAdditionalPackingLineCountryData(addInfoCollection, declaration, invoice, invoiceLine);
			var sgInvoiceLine = (JobComInvoiceLine)invoiceLine;
			AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.GoodsType, GetGoodsType(declaration, sgInvoiceLine));
			AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.PermitNumber, entryNumber, false);

			if (IsOVRDeclaration(declaration))
			{
				AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.GSTPaymentIndicator, GetGSTPaid(sgInvoiceLine), false);
			}
		}

		ZString GetGoodsType(BaseJobDeclaration declaration, JobComInvoiceLine invoiceLine)
		{
			var result = Constants.GoodsType.NormalGoods;
			if (declaration.IsImport)
			{
				result = invoiceLine.UniversalTariff.GetSGDefaultImportGoodsType(partyStatus, invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate, invoiceLine.EffectiveDateForDutyRate);
			}
			else if (declaration.IsExport)
			{
				result = invoiceLine.UniversalTariff.GetSGDefaultExportGoodsType(invoiceLine.EffectiveDateForDutyRate);
			}
			return result;
		}

		bool IsOVRDeclaration(BaseJobDeclaration declaration)
		{
			var sgDeclaration = (JobDeclaration)declaration;
			return sgDeclaration.JE_MessageType == MessageTypeCodeList.Codes.INP
				&& sgDeclaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.APS
				&& sgDeclaration.SG_US_NKPlaceOfReceipt == Constants.OverseasVendorRegistration;
		}

		ZString GetGSTPaid(JobComInvoiceLine sgInvoiceLine)
		{
			if (sgInvoiceLine.ProductCodes.Cast<ICusProductCode>().Any(x => x.ProductCode == Constants.OverseasVendorRegistration))
			{
				return Customs.Business.YesNoList.Codes.Yes;
			}

			return Customs.Business.YesNoList.Codes.No;
		}

		protected override void PopulateHouseBillEntryHeader(EntryHeader entryHeader, BaseJobDeclaration declaration)
		{
			base.PopulateHouseBillEntryHeader(entryHeader, declaration);
			entryNumber = declaration.ActiveEntryHeaders.Cast<V4.Business.CusEntryHeader>().FirstOrDefault()?.EntryNumber ?? ZString.Empty;
			if (!entryNumber.IsEmpty)
			{
				var entryNumberCollection = entryHeader.EntryNumberCollection ?? new List<UniversalXml.Customs.EntryNumber>();
				entryNumberCollection.Add(new UniversalXml.Customs.EntryNumber()
				{
					Type = new EntryType() { Code = ASYCUDA.Business.Constants.CustomsEntryType.TradeNetPermit },
					Number = entryNumber
				});
				entryHeader.EntryNumberCollection = entryNumberCollection;
			}
		}
		ZString entryNumber;

		protected override void PopulateHouseBillEntryInstruction(EntryInstruction entryInstruction, BaseJobDeclaration declaration)
		{
			partyStatus = ZString.Empty;
			base.PopulateHouseBillEntryInstruction(entryInstruction, declaration);
			if (declaration.IsImport)
			{
				var importerAddress = declaration.Importer?.MainAddress;
				if (importerAddress != null)
				{
					AddAddInfo(entryInstruction, AddInfoConstants.BillCountry.PartyIndicator, importerAddress.GetSGUniqueEntityNumber(), false);
					AddAddInfo(entryInstruction.AddInfoCollection, AddInfoConstants.BillCountry.PartyStatus, (partyStatus = importerAddress.GetSGPartyStatusType()), false);
					AddAddInfo(entryInstruction.AddInfoCollection, AddInfoConstants.BillCountry.PayeeIndicator, importerAddress.GetSGPayeeIndicator(), false);
				}
			}
			else if (declaration.IsExport)
			{
				var supplierAddress = declaration.Supplier?.MainAddress;
				if (supplierAddress != null)
				{
					AddAddInfo(entryInstruction, AddInfoConstants.BillCountry.PartyIndicator, supplierAddress.GetSGUniqueEntityNumber(), false);
				}
			}
		}
		ZString partyStatus;
	}
}
