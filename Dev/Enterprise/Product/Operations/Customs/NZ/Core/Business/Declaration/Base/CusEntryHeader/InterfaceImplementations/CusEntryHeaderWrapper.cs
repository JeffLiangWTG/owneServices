using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusEntryHeaderWrapper : IImportDeclaration, IExportDeclaration, IGoodsShipment, ITSWSubmitter
	{
		public CusEntryHeaderWrapper(FormalEntry.CusEntryHeader entryHeader, IAdditionalInformation additionalInformation)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "EntryHeader cannot be null");
			declaration = entryHeader.Declaration;
			this.additionalInformation = additionalInformation;
		}

		readonly FormalEntry.CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly IAdditionalInformation additionalInformation;

		#region IImportDeclaration Implementation

		ZBool IDeclaration.IsSea
		{
			get { return declaration.IsSea; }
		}

		ZBool IDeclaration.IsAir
		{
			get { return declaration.IsAir; }
		}

		ZBool IDeclaration.IsMail
		{
			get { return declaration.IsPost; }
		}

		ZBool IDeclaration.IsContainerised
		{
			get { return declaration.CusContainers.Count > 0; }
		}

		ZBool IDeclaration.IsCompletionEntry
		{
			get { return declaration.IsCompletion; }
		}

		ZBool IDeclaration.HasContainersOrPallets
		{
			get { return declaration.CusContainers.Count > 0; }
		}

		ZBool IImportDeclaration.IsPeriodicImport
		{
			get { return declaration.IsPeriodic; }
		}

		ZString IDeclaration.MessageType
		{
			get { return declaration.MappedTSWMessageSubType; }
		}

		ZString IDeclaration.SenderReferenceNumber
		{
			get
			{
				if (entryHeader.CH_BGMReference.IsEmpty || entryHeader.CH_BGMReference.Length > 13 || declaration.IsWriteOffChangedToFormal)
				{
					return new ZString(TSWConstants.SendersReferencePlaceHolder);
				}
				else
				{
					return entryHeader.CH_BGMReference.Replace("/", "");
				}
			}
		}

		ZString IDeclaration.TSWReferenceNumber
		{
			get
			{
				var result = ZString.Empty;
				if (declaration.IsPrimaryIndustriesImportDeclaration)
				{
					result = declaration.EntryHeaderForIPIEntryNumber.EntryNumber;
				}
				else
				{
					if (declaration.IsCompletion)
					{
						result = declaration.CompletionEntryNumber;
					}

					if (result.IsEmpty)
					{
						var entryHeader = declaration.EntryHeaderForOriginalEntryNumber;
						result = entryHeader != null ? entryHeader.EntryNumber : declaration.JE_OriginalEntryNumber;
					}
				}

				return result;
			}
		}

		IAdditionalInformation IDeclaration.AdditionalInformation
		{
			get { return additionalInformation; }
		}

		IEnumerable<IOtherInfo> IDeclaration.OtherInfoCodes
		{
			get
			{
				foreach (Business.OtherInfo otherInfoDetail in declaration.OtherInfos)
				{
					if (HeaderOtherInfoList.IsRequiredOtherInfoCode(otherInfoDetail.ZO_Code))
					{
						yield return new OtherInfo(otherInfoDetail.ZO_Code, otherInfoDetail.ZO_Data);
					}
				}
			}
		}

		IEnumerable<ZString> IDeclaration.Permits
		{
			get
			{
				foreach (PermitCode permit in declaration.PermitCodes)
				{
					yield return permit.ZO_Code + "," + permit.ZO_Data;
				}
			}
		}

		IEnumerable<IOtherInfo> IDeclaration.OtherReferencedDocuments
		{
			get
			{
				foreach (Business.OtherInfo otherInfoDetail in declaration.OtherInfos)
				{
					if (HeaderOtherInfoList.IsReferenceDocument(otherInfoDetail.ZO_Code))
					{
						yield return new OtherInfo(otherInfoDetail.ZO_Code, otherInfoDetail.ZO_Data);
					}
				}
			}
		}

		ZString IDeclaration.HandlingInformation
		{
			get { return GoodsHandling; }
		}

		ZString IDeclaration.MPIAccountDetails
		{
			get
			{
				var result = ZString.Empty;
				if (declaration != null && !declaration.AccountNumberToBeSent.IsEmpty)
				{
					result = declaration.AccountNumberToBeSent + "," + declaration.AccountHolderNameToBeSent;
				}

				return result;
			}
		}

		ZInt IDeclaration.TransactionType
		{
			get
			{
				ZInt result = 9; //TransactionTypeList.Codes.Original
				if (declaration.IsCompletion)
				{
					result = 22; //TransactionTypeList.Codes.Completion
				}

				return result;
			}
		}

		ZDecimal IDeclaration.TotalGrossWeightInKGM
		{
			get { return declaration.JE_DeclaredWeight; }   // This value is always in KGM
		}

		ZString IDeclaration.TotalGrossWeightUnit
		{
			get { return "KGM"; }
		}

		ZDateTime IImportDeclaration.DateOfImport
		{
			get { return declaration.JE_DateOfArrival; }
		}

		ZDateTime IImportDeclaration.ImportPeriod
		{
			get { return declaration.JE_EntryAuthorisationDate; }
		}

		ZString IDeclaration.BrokerCode
		{
			get
			{
				ZString brokerCode = NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant();
				if (brokerCode.Length < 9)
				{
					brokerCode = brokerCode.PadLeft(9, '0');
				}
				return brokerCode;
			}
		}

		ZString IDeclaration.PremiseID
		{
			get
			{
				var result = ZString.Empty;
				foreach (Business.OtherInfo otherInfoDetail in declaration.OtherInfos)
				{
					if (otherInfoDetail.ZO_Code == HeaderOtherInfoList.Codes.ApprovedTransitionalFacility)
					{
						result = otherInfoDetail.ZO_Data;
						break;
					}
				}

				return result;
			}
		}

		ZString IDeclaration.CraftName
		{
			get { return declaration.JE_VesselName.ToUpper(); }
		}

		ZString IDeclaration.LloydsNo
		{
			get
			{
				var result = ZString.Empty;
				if (declaration.IsSea && !declaration.JE_VesselName.IsEmpty && !declaration.VesselIndicatesPeriodic)
				{
					var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, declaration.JE_VesselName);
					var vessel = declaration.Factory.LoadTop1<RefVessel>(vesselQuery);
					if (vessel != null)
					{
						result = vessel.RV_LloydsNumber;
					}

					if (result.IsEmpty)
					{
						result = declaration.JE_LloydsIMO;
					}
				}

				return result;
			}
		}

		ZString IDeclaration.VoyageNo
		{
			get { return declaration.JE_VoyageFlightNo.Left(8); }
		}

		ZString IDeclaration.FlightNo
		{
			get { return declaration.JE_VoyageFlightNo; }
		}

		ZDateTime IDeclaration.DepartureDate
		{
			get { return declaration.JE_ExportDate; }
		}

		IOrganisationSimple IDeclaration.Carrier
		{
			get { return OrgHeaderWrapper.New(declaration.ShippingLine); }
		}

		IEnumerable<ICurrency> IDeclaration.ExchangeRates
		{
			get
			{
				var currencyList = new List<ZString>();

				foreach (JobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
				{
					string indicatorCode = GetRateIndicator(invoice.JZ_ExchangeRateIndicator);
					if (!invoice.JZ_RX_NKInvoice_Currency.IsEmpty && !currencyList.Contains(invoice.JZ_RX_NKInvoice_Currency))
					{
						currencyList.Add(invoice.JZ_RX_NKInvoice_Currency);
						yield return new Currency(invoice.JZ_RX_NKInvoice_Currency, invoice.JZ_InvoiceCurrExRate, indicatorCode);
					}
				}
			}
		}

		string GetRateIndicator(string exchangeRateIndicator)
		{
			if (exchangeRateIndicator == ExchangeRateIndicatorList.Codes.Floating)
			{
				return "F";
			}
			else if (exchangeRateIndicator == ExchangeRateIndicatorList.Codes.ForwardCover)
			{
				return "C";
			}
			else
			{
				return "N";
			}
		}

		IDeclarant IDeclaration.Declarant
		{
			get { return new TSWGlbStaffWrapper(GlbStaff.CurrentUser); }
		}

		ZString IDeclaration.PaymentType
		{
			get
			{
				var result = ZString.Empty;

				switch (declaration.JE_PaymentMethod)
				{
					case PaymentMethodList.Codes.CashPaidByBroker:
					case PaymentMethodList.Codes.CashPaidByClient:
						result = CustomsPaymentTypeList.Codes.CashPayment;
						break;
					case PaymentMethodList.Codes.BrokerDeferred:
						result = CustomsPaymentTypeList.Codes.BrokerDeferred;
						break;
					case PaymentMethodList.Codes.ClientDeferred:
						result = CustomsPaymentTypeList.Codes.ClientDeferred;
						break;
				}

				return result;
			}
		}

		IEnumerable<IDutyTaxFee> IDeclaration.DutyTaxFees
		{
			get
			{
				var aLACLevyDutyTaxFee = GetDutyTaxFee(entryHeader.ALACLevyAmount, DutyTaxFeeTypeList.Codes.AL);
				if (aLACLevyDutyTaxFee != null)
				{
					yield return aLACLevyDutyTaxFee;
				}

				var hERALevyDutyTaxFee = GetDutyTaxFee(entryHeader.HERALevyAmount, DutyTaxFeeTypeList.Codes.SL);
				if (hERALevyDutyTaxFee != null)
				{
					yield return hERALevyDutyTaxFee;
				}

				var pFMLLevyDutyTaxFee = GetDutyTaxFee(entryHeader.PFMLFuelLevyAmount, DutyTaxFeeTypeList.Codes.PF);
				if (pFMLLevyDutyTaxFee != null)
				{
					yield return pFMLLevyDutyTaxFee;
				}

				var aCCFuelLevyDutyTaxFee = GetDutyTaxFee(entryHeader.ACCFuelLevyAmount, DutyTaxFeeTypeList.Codes.AC);
				if (aCCFuelLevyDutyTaxFee != null)
				{
					yield return aCCFuelLevyDutyTaxFee;
				}

				var syntheticGreenhouseGasesDutyTaxFee = GetDutyTaxFee(entryHeader.SyntheticGreenhouseGasesLevyAmount, DutyTaxFeeTypeList.Codes.GG);
				if (syntheticGreenhouseGasesDutyTaxFee != null)
				{
					yield return syntheticGreenhouseGasesDutyTaxFee;
				}

				var antiDumpingDutyDutyTaxFee = GetDutyTaxFee(entryHeader.AntiDumpingDutyAmount, DutyTaxFeeTypeList.Codes.ADD);
				if (antiDumpingDutyDutyTaxFee != null)
				{
					yield return antiDumpingDutyDutyTaxFee;
				}

				var countervailingDutyDutyTaxFee = GetDutyTaxFee(entryHeader.CountervailingDutyAmount, DutyTaxFeeTypeList.Codes.CVD);
				if (countervailingDutyDutyTaxFee != null)
				{
					yield return countervailingDutyDutyTaxFee;
				}

				var entryHeaderDuty = GetDutyTaxFee(entryHeader.DutyAmount, DutyTaxFeeTypeList.Codes.CUD);
				if (entryHeaderDuty != null)
				{
					yield return entryHeaderDuty;
				}

				var entryHeaderGST = GetDutyTaxFee(entryHeader.GSTAmount, DutyTaxFeeTypeList.Codes.GST);
				if (entryHeaderGST != null)
				{
					yield return entryHeaderGST;
				}

				var entryTotalAmountPayable = GetDutyTaxFee(entryHeader.TotalAmountPayable, DutyTaxFeeTypeList.Codes.TOT);
				if (entryTotalAmountPayable != null)
				{
					yield return entryTotalAmountPayable;
				}

				if (entryHeader.Declaration.IsDrawback)
				{
					var dutyCredit = GetDutyTaxFee(entryHeader.DutyCreditAmount, DutyTaxFeeTypeList.Codes.CUD);
					if (dutyCredit != null)
					{
						yield return dutyCredit;
					}

					var aLACLevyCredit = GetDutyTaxFee(entryHeader.ALACLevyCreditAmount, DutyTaxFeeTypeList.Codes.AL);
					if (aLACLevyCredit != null)
					{
						yield return aLACLevyCredit;
					}

					var aCCLevyCredit = GetDutyTaxFee(entryHeader.ACCLevyCreditAmount, DutyTaxFeeTypeList.Codes.AC);
					if (aCCLevyCredit != null)
					{
						yield return aCCLevyCredit;
					}

					var hERALevyCredit = GetDutyTaxFee(entryHeader.HERALevyCreditAmount, DutyTaxFeeTypeList.Codes.SL);
					if (hERALevyCredit != null)
					{
						yield return hERALevyCredit;
					}

					var pFMLFuelLevyCredit = GetDutyTaxFee(entryHeader.PFMLFuelLevyCreditAmount, DutyTaxFeeTypeList.Codes.PF);
					if (pFMLFuelLevyCredit != null)
					{
						yield return pFMLFuelLevyCredit;
					}

					var gSTCredit = GetDutyTaxFee(entryHeader.GSTCreditAmount, DutyTaxFeeTypeList.Codes.GST);
					if (gSTCredit != null)
					{
						yield return gSTCredit;
					}
				}
			}
		}

		DutyTaxFee GetDutyTaxFee(ZDecimal amount, ZString typeCode)
		{
			DutyTaxFee result = null;
			if (amount > 0)
			{
				result = new DutyTaxFee(amount, typeCode, Core.Constants.CurrencyCodes.NewZealand);
			}

			return result;
		}

		ZBool IImportDeclaration.IsMiscImporter
		{
			get { return IsMiscellaneousImporter; }
		}

		Integration.IJobDocAddress IImportDeclaration.MiscImporterAddress
		{
			get { return IsMiscellaneousImporter && declaration.Shipment != null && declaration.Shipment.ConsigneeDocumentaryAddress != null ? declaration.Shipment.ConsigneeDocumentaryAddress : null; }
		}

		protected ZBool IsMiscellaneousImporter
		{
			get { return declaration.Importer != null && declaration.Importer.PK == declaration.CachedMiscOrgPK; }
		}

		IOrganisation IImportDeclaration.Importer
		{
			get { return OrgHeaderWrapper.New(declaration.Importer); }
		}

		IEnumerable<IMasterBillTransportDocument> IDeclaration.MasterBills
		{
			get
			{
				foreach (Bill billDetails in declaration.Bills)
				{
					if (billDetails.CU_BillType == BillTypeList.Codes.MasterBill)
					{
						yield return new MasterBillWrapper(billDetails);
					}
				}
			}
		}

		IEnumerable<IAssociatedTransportDocument> IDeclaration.AllBills
		{
			get
			{
				foreach (Bill billDetails in declaration.LowestBills)
				{
					yield return new BillNumberWrapper(billDetails);
				}
			}
		}

		IEnumerable<ITransportEquipment> IDeclaration.Equipment
		{
			get
			{
				foreach (PackingGroup packGroup in declaration.PackingGroups)
				{
					if (packGroup.Container != null && packGroup.Packages.Count > 0)
					{
						yield return new ContainerWrapper(packGroup);
					}
				}
			}
		}

		IEnumerable<IPackaging> IDeclaration.Packaging
		{
			get
			{
				foreach (Package package in declaration.Packages)
				{
					ZString houseBill = package.Bill != null ? package.Bill.CU_HouseBill : ZString.Empty;
					var packaging = new Packaging(package, houseBill);
					yield return packaging;
				}
			}
		}

		ZString IDeclaration.PreviousDocumentNo
		{
			get { return declaration.JE_OriginalEntryNumber; }
		}

		ZString IDeclaration.PreviousDocumentType
		{
			get
			{
				var result = ZString.Empty;
				if (!declaration.JE_OriginalEntryNumber.IsEmpty)
				{
					if (!declaration.JE_OriginalEntryType.IsEmpty)
					{
						result = declaration.JE_OriginalEntryType == JobMessageSubTypeList.Codes.Sight ? PreviousDocumentTypeList.Codes.I52 : PreviousDocumentTypeList.Codes.I51;
					}
					else
					{
						var originalEntry = declaration.EntryHeaderForOriginalEntryNumber;
						if (declaration.IsExport)
						{
							var entryNumQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, declaration.JE_OriginalEntryNumber);
							entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypeList.Codes.FormalEntry);
							var entryNum = entryHeader.Factory.LoadTop1<CusEntryNumber>(entryNumQuery);
							if (entryNum != null)
							{
								originalEntry = entryHeader.Factory.Load<CusEntryHeader>(entryNum.CE_ParentID);
							}
						}

						if (originalEntry != null)
						{
							result = originalEntry.CH_LastEntryStyle == JobMessageSubTypeList.Codes.Sight ? PreviousDocumentTypeList.Codes.I52 : PreviousDocumentTypeList.Codes.I51;
						}
					}
				}

				return result;
			}
		}

		IGoodsShipment IImportDeclaration.GoodsShipment
		{
			get { return this; }
		}

		#endregion

		#region IExportDeclaration Implementation

		IOrganisation IExportDeclaration.Exporter
		{
			get { return OrgHeaderWrapper.New(declaration.Supplier); }
		}

		ZDateTime IExportDeclaration.DateOfExport
		{
			get { return declaration.JE_ExportDate; }
		}

		IGoodsShipment IExportDeclaration.GoodsShipment
		{
			get { return this; }
		}

		IOrganisation IExportDeclaration.Importer
		{
			get { return OrgHeaderWrapper.New(declaration.Importer); }
		}

		#endregion

		#region IGoodsShipment Implementation

		ZString IGoodsShipment.ShipmentOrigin
		{
			get { return declaration.JE_RL_NKOrigin.SubstringSafe(0, 2); }
		}

		ZString IGoodsShipment.NatureOfTransaction
		{
			get { return declaration.JE_TransactionNature; }
		}

		ZBool IGoodsShipment.MAFContainerDeclaration => declaration.JE_SendMCDContainerQuarantineDeclaration;

		//format container number, MCD Response eg ABCU1234560,YYNNN
		IEnumerable<ZString> IGoodsShipment.MAFContainerStatements
		{
			get
			{
				if (((IGoodsShipment)this).MAFContainerDeclaration)
				{
					var formattedMAFContainerDeclaration = declaration.MCDOtherInfoValue;
					foreach (CusContainer container in declaration.CusContainers)
					{
						yield return container.CO_ContainerNumber + "," + formattedMAFContainerDeclaration;
					}
				}
			}
		}

		//format Container number,MAS Approved System number (e.g. ABCU1234567,10635)
		IEnumerable<ZString> IGoodsShipment.MPIApprovedSystemNumbers
		{
			get
			{
				foreach (CusContainer container in declaration.CusContainers)
				{
					if (!container.CO_MPIApprovedSystemNumber.IsEmpty)
					{
						yield return container.CO_ContainerNumber + "," + container.CO_MPIApprovedSystemNumber;
					}
				}
			}
		}

		ZString IGoodsShipment.LocationOfGoods
		{
			//	Air and Mail - Must be transmitted to state the Cargo Terminal Operator / consolidator / freight forwarder premises where the goods are located
			//	Sea - Must be transmitted to state the place at which the goods are located if different to the place of discharge element L013. Not required when goods are cleared directly from the Port 
			//			TSW Premises code - A list of codes will be published in due course
			get { return declaration.JE_Cal_GoodsLocation; }
		}

		ZString IGoodsShipment.PortOfLoading
		{
			get { return declaration.JE_RL_NKPortOfLoading; }
		}

		ZString IGoodsShipment.PortOfDischarge
		{
			get { return declaration.JE_RL_NKPortOfArrival; }
		}

		ZDecimal IGoodsShipment.FreightCostsInNZD
		{
			//Are you sure that this is freight only. Should it be CIF amount - FOB amount? Key to understanding this is understanding how duty is calculated. is duty based on FOB or CIF. Ben can probably answer easily
			get
			{
				var result = ZDecimal.Zero;
				if (entryHeader.OverseasFreight.Amount > 0)
				{
					if (entryHeader.OverseasFreight.Currency.Code == Core.Constants.CurrencyCodes.NewZealand)
					{
						result = entryHeader.OverseasFreight.Amount;
					}
					else
					{
						result = entryHeader.CurrencyConverter.ConvertExact(entryHeader.OverseasFreight, entryHeader.CurrencyConverter.LocalCurrency).Amount;
					}
				}

				return result;
			}
		}

		ZString IGoodsShipment.FreightApportionmentMethod
		{
			get
			{
				var result = FreightProportioningMethodList.Codes.F161; // by value
				var oFT = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true);
				if (entryHeader.RandomHeader.Charges.HasChargesDistributedByOtherThanValue(oFT))
				{
					result = FreightProportioningMethodList.Codes.F160; // by weight
				}

				return result;
			}
		}

		IOrganisation IGoodsShipment.DeliverToParty
		{
			get { return OrgHeaderWrapper.New(declaration.DeliveryDestinationPartyDocAddress.Organisation, declaration.DeliveryDestinationPartyDocAddress); }
		}

		IEnumerable<IGoodsItems> IGoodsShipment.Items
		{
			get
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					yield return new CusEntryLineWrapper(entryLine);
				}
			}
		}

		IEnumerable<IInvoice> IGoodsShipment.Invoices
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
				{
					yield return new InvoiceHeader(invoice.JZ_InvoiceNumber, invoice.IncoTerm, invoice.JZ_InvoiceDate);
				}
			}
		}

		IEnumerable<IOrganisationSimple> IGoodsShipment.NotifyParties
		{
			get
			{
				var notifyParty = declaration.Factory.Load<OrgHeader>(declaration.JE_OH_NotifyParty);
				yield return OrgHeaderWrapper.New(notifyParty);
			}
		}

		IEnumerable<ZString> IGoodsShipment.NotifyPartyCodes
		{
			get
			{
				yield return declaration.NotifyParty2DocumentaryAddress.GetCCPOrATFCode();
				yield return declaration.JE_RL_NKPortOfDeliveryNotify;
			}
		}

		IEnumerable<IOrganisation> IGoodsShipment.Sellers
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
				{
					if (invoice.SellerAddress != null)
					{
						yield return OrgAddressWrapper.New(invoice.SellerAddress);
					}
				}
			}
		}

		IEnumerable<IOrganisation> IGoodsShipment.StuffingEstablishments
		{
			get
			{
				foreach (CusContainer container in entryHeader.Declaration.CusContainers)
				{
					if (container.StuffingEstablishmentAddress != null)
					{
						yield return OrgAddressWrapper.New(container.StuffingEstablishmentAddress);
					}
				}
			}
		}

		ZString IGoodsShipment.CustomsControlledArea
		{
			//	Conditional – must be stated where excisable goods imported are to be stored in premises licenced under Section 10 of the Customs & Excise Act 1966
			get
			{
				var result = ZString.Empty;
				var ccaOrg = declaration.WarehouseDocAddress.Organisation;
				if (ccaOrg != null)
				{
					result = declaration.GetCustomsCodeForAddress(ccaOrg, declaration.WarehouseDocAddress.E2_OA_Address, OrgCusCode.CodeTypes.ControlledPremisesID);
				}

				return result;
			}
		}

		IEnumerable<IOrganisation> IGoodsShipment.Suppliers
		{
			get
			{
				foreach (JobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
				{
					yield return OrgHeaderWrapper.New(invoice.Supplier);
				}
			}
		}

		#endregion

		#region ITSWSubmitter Implementation

		ZString ITSWSubmitter.SubmitterCode
		{
			get
			{
				var submitterCode = NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant();

				if (submitterCode.Length < 9)
				{
					submitterCode = submitterCode.PadLeft(9, '0');
				}

				return submitterCode;
			}
		}

		#endregion

		#region DutyTaxFee

		class DutyTaxFee : IDutyTaxFee
		{
			public DutyTaxFee(ZDecimal value, ZString typeCode, ZString currencyCode)
			{
				this.value = value;
				this.typeCode = typeCode;
				this.currencyCode = currencyCode;
			}

			readonly ZDecimal value;
			readonly ZString typeCode;
			readonly ZString currencyCode;

			public ZDecimal Amount
			{
				get { return value.Round(2); }
			}

			public ZString DutyTaxFeeType
			{
				get { return typeCode; }
			}

			public ZString CurrencyCode
			{
				get { return currencyCode; }
			}
		}

		#endregion

		#region Currency

		class Currency : ICurrency
		{
			public Currency(ZString code, ZDecimal rate, ZString indicator)
			{
				this.code = code;
				this.rate = rate;
				this.indicator = indicator;
			}
			readonly ZString code;
			readonly ZDecimal rate;
			readonly ZString indicator;

			public ZString CurrencyCode
			{
				get { return code; }
			}

			public ZDecimal ExchangeRate
			{
				get { return rate; }
			}

			public ZString ExchangeRateIndicator
			{
				get { return indicator; }
			}
		}

		#endregion

		#region InvoiceHeader

		class InvoiceHeader : IInvoice
		{
			public InvoiceHeader(ZString invNumber, ZString incoTerm, ZDateTime invoiceDate)
			{
				this.invNumber = invNumber;
				this.incoTerm = incoTerm;
				this.invoiceDate = invoiceDate;
			}
			readonly ZString invNumber;
			readonly ZString incoTerm;
			readonly ZDateTime invoiceDate;

			public ZString InvoiceNumber
			{
				get { return invNumber; }
			}

			public ZString IncoTerms
			{
				get { return incoTerm; }
			}

			public ZDateTime InvoiceDate
			{
				get { return invoiceDate; }
			}
		}

		#endregion

		#region OtherInfo

		class OtherInfo : IOtherInfo
		{
			public OtherInfo(ZString code, ZString data)
			{
				this.code = code;
				this.data = data;
			}
			readonly ZString code;
			readonly ZString data;

			public ZString Code
			{
				get { return code; }
			}

			public ZString Data
			{
				get { return data; }
			}
		}

		#endregion

		#region Goods Handling

		public ZString GoodsHandling
		{
			get
			{
				StmNote goodsHandlingNote = GetGoodsHandlingNote();
				return goodsHandlingNote != null ? new ZString(new string(goodsHandlingNote.ST_NoteText.Replace("\r\n", " ").Replace("\r", "").Replace("\n", "").ToString().Take(512).ToArray())) : ZString.Empty;
			}
		}

		protected StmNote GetGoodsHandlingNote()
		{
			StmNote[] handlingInstructionsNotes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			return handlingInstructionsNotes.Length > 0 ? handlingInstructionsNotes[0] : null;
		}

		#endregion
	}
}
