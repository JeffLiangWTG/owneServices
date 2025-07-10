using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Customs.TR.MessageContracts.Interfaces.ExportUnion;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.TR.Business.CusEntryMessageConstants;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryHeaderMessageProvider : IDeclaration
	{
		public CusEntryHeaderMessageProvider(CusEntryHeader cusEntryHeader, ZString messageType)
		{
			Factory = cusEntryHeader.Factory;
			CusEntryHeader = CargoWise.Common.Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			JobDeclaration = cusEntryHeader.Declaration;
			CusEntryInstruction = (CusEntryInstruction)cusEntryHeader.EntryInstruction;
			DefaultInvoice = JobDeclaration.Invoices.Cast<JobComInvoiceHeader>().Single();
			CusEntryPayInfo = cusEntryHeader.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault();
			InvoiceLine = (JobComInvoiceLine)(CusEntryHeader?.InvoiceLines.FirstOrDefault());
			ProcedureCode = InvoiceLine?.JI_Procedure.ToString();
			MessageType = messageType;
		}

		BusinessObjectFactory Factory { get; }
		JobDeclaration JobDeclaration { get; }
		CusEntryHeader CusEntryHeader { get; }
		CusEntryInstruction CusEntryInstruction { get; }
		JobComInvoiceHeader DefaultInvoice { get; }
		CusEntryPayInfo CusEntryPayInfo { get; }
		JobComInvoiceLine InvoiceLine { get; }
		ZString ProcedureCode { get; }
		ZDateTime EffectiveDate => DefaultInvoice.EffectiveValuationDate;
		ZBool IsExport => JobDeclaration.IsExport;
		ZString MessageType { get; }

		public string DeclarationNo => JobDeclaration.DeclarationNumber;
		public string Nature => ProcedureCode;
		public string CustomsOffice => JobDeclaration.JE_CustomsOffice.SubstringSafe(2);
		public string SimplifiedProcedure => JobDeclaration.JE_EntrySubStyle;
		public int CountOfLoadingDocuments => DeclarationProviderHelper.Check8ThousandCodes(ProcedureCode) ? ZInt.Zero : JobDeclaration.ZG_NumberOfDocs;
		public int PackQuantity => JobDeclaration.JE_TotalNoOfPacks;
		public string CountryOfTrade => !DeclarationProviderHelper.Check8ThousandCodes(ProcedureCode) ? UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(Factory, JobDeclaration.ZG_CountryOfSupply, EffectiveDate) : ZString.Empty;
		public string ReferenceNo => JobDeclaration.JE_OwnerRef;

		#region ExportersUnion

		public string UnionRecordNumber => IsExport && CusEntryPayInfo != null ? CusEntryPayInfo.C9_PaymentReference : ZString.Empty;
		public string UnionCryptoNumber => IsExport && CusEntryPayInfo != null ? CusEntryPayInfo.C9_IncomingPayResponseNo : ZString.Empty;

		#endregion

		public string CountryOfDeparture => !IsExport ? UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(Factory, JobDeclaration.JE_GoodsOrigin, EffectiveDate) : ZString.Empty;
		public string CountryOfDestination => IsExport ? UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(Factory, JobDeclaration.JE_GoodsDestination, EffectiveDate) : ZString.Empty;
		public string CountryOfDispatch => UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(Factory, JobDeclaration.ZG_ShippingCountry, EffectiveDate);

		#region VehicleOnExit
		public string TypeOfVehicleOnExit => TransportModeTranslator.TranslateToWCOCode(JobDeclaration.JE_TransportMode);
		public string PlateOfVehicleOnExit => GetPlateOfVehicleOnExit().Trim();

		string GetPlateOfVehicleOnExit()
		{
			if (!(JobDeclaration.IsExport || JobDeclaration.IsImport))
			{
				return string.Empty;
			}

			return (string)JobDeclaration.JE_TransportMode switch
			{
				Core.Constants.TransportModes.Air => (string)JobDeclaration.JE_VoyageFlightNo,
				Core.Constants.TransportModes.Sea => $"{JobDeclaration.JE_VesselName} {JobDeclaration.JE_VoyageFlightNo}",
				_ => (string)JobDeclaration.JE_VesselName,
			};
		}

		public string CountryOfVehicleOnExit => UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(Factory, JobDeclaration.JE_RN_NKTransportNationality, EffectiveDate);
		#endregion

		public string TypeOfDelivery => JobDeclaration.JE_ShipmentIncoTerm;
		public string PlaceOfDelivery => JobDeclaration.JE_ShipmentIncoTermPlace;
		public string IsContainer => JobDeclaration.IsContainerised && JobDeclaration.ContainerMode == Core.Constants.ContainerModes.Containerised ? TurkishAnswers.Yes : TurkishAnswers.No;

		#region VehicleAtBorder

		public string TypeOfVehicleAtBorder => JobDeclaration.JE_TransportMeans;
		public string PlateOfVehicleAtBorder => JobDeclaration.ZG_Box18TransportID;
		public string CountryOfVehicleAtBorder => UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(Factory, JobDeclaration.ZG_Box18TransportNationality, EffectiveDate);

		#endregion
		#region Total Invoice

		public decimal TotalInvoiceAmount => DefaultInvoice.JZ_InvoiceAmount.RoundAmount();
		public string TotalInvoiceAmountCurrency => DefaultInvoice.Invoice_Currency?.Code ?? ZString.Empty;

		#endregion

		#region Total Freight

		public decimal TotalFreight => DefaultInvoice.OverseasFreight.Amount.RoundAmount();
		public string TotalFreightCurrency => DeclarationProviderHelper.GetCurrency(TotalFreight, DefaultInvoice.OverseasFreight.Currency);

		#endregion

		public string TypeOfTransportAtBorder => JobDeclaration.JE_TransportModeInland;
		public string BuyerSellerRelationship => DefaultInvoice.JZ_RelatedIndicator == RelationCodeList.Codes.N ? OrganizationRelationship.NotRelated : DefaultInvoice.JZ_RelatedIndicator == RelationCodeList.Codes.Y ? OrganizationRelationship.Related : string.Empty;

		#region Total Insurance

		public decimal TotalInsurance => DefaultInvoice.OverseasInsurance.Amount.RoundAmount();
		public string TotalInsuranceCurrency => DeclarationProviderHelper.GetCurrency(TotalInsurance, DefaultInvoice.OverseasInsurance.Currency);

		#endregion

		public string LoadingUnloadingPlace => JobDeclaration.DischargePlace;

		#region Total Abroad Expenditure

		readonly ZString[] totalAbroadExpenditureChargeTypes = new ZString[] { TRIncotermChargeCodeList.Codes.COM, TRIncotermChargeCodeList.Codes.DEM, TRIncotermChargeCodeList.Codes.INT,
																	  TRIncotermChargeCodeList.Codes.ROY, TRIncotermChargeCodeList.Codes.OTH };
		public decimal TotalAbroadExpenditure => DeclarationProviderHelper.GetChargesTotal(DefaultInvoice, totalAbroadExpenditureChargeTypes);
		public string TotalAbroadExpenditureCurrency => DeclarationProviderHelper.GetDefaultCurrencyCode(DefaultInvoice, totalAbroadExpenditureChargeTypes);

		#endregion

		public decimal TotalDomesticExpenditure => DeclarationProviderHelper.GetChargesTotal(DefaultInvoice, TRIncotermChargeCodeList.Codes.LocalTotalCharges, TRIncotermChargeCodeList.Codes.LPC, TRIncotermChargeCodeList.Codes.LBC,
																											 TRIncotermChargeCodeList.Codes.LDC, TRIncotermChargeCodeList.Codes.LOT, TRIncotermChargeCodeList.Codes.LSC);

		public string BankCode => JobDeclaration.ZG_BankCode;
		public string GoodsLocation => JobDeclaration.JE_SubLocationOfGoods;
		public string CustomsOfficeOfDestination => string.Empty;
		public string BondedWarehouseCode => JobDeclaration.BondedWarehouseCode;
		public string PlannedRoute => string.Empty;
		public decimal CounterVailingDuty => ZDecimal.Zero;
		public string CustomsOfficeOfEntry => JobDeclaration.EntryOffice.SubstringSafe(2);
		public string QualificationOfProcess => string.Empty;
		public string Descriptions => string.Join(" ", JobDeclaration.AdditionalInfos.Where(x => !x.CSI_Description.IsEmpty).Select(x => x.CSI_Description).ToHashSet());
		public string UserId => TRGlbStaffWrapper.Get(GlbStaff.CurrentUser)?.TRBPassword?.GP_UserID ?? ZString.Empty;
		public string ReferenceDate => CusEntryInstruction != null ? CusEntryInstruction.CEI_DateForDuty.ToISO8601ShortDateString() : string.Empty;
		public string Payment => CountryConstants.PaymentType;
		public string InstrumentOfPayment => JobDeclaration.JE_PaymentMethod;
		public string ReferenceOfBrokers => JobDeclaration.JE_DeclarationReference;

		#region Company Tax Numbers or Details

		public string ShipperTaxNo
		{
			get
			{
				var result = string.Empty;
				var supplier = JobDeclaration.Supplier;
				if (supplier != null)
				{
					var regNoType = supplier.CountryCode == Core.Constants.CountryCodes.Turkey ? OrgCusCode.CodeTypes.VATCode : OrgCusCode.CodeTypes.SupplierCode;
					var taxNo = supplier.CustomsCodes.GetCustomsRegNo(regNoType, Core.Constants.CountryCodes.Turkey);
					var yfkNo = supplier.CustomsCodes.GetCustomsRegNo(TurkeyOrgCusCodeInfo.OrgCusCodes.YFK, Core.Constants.CountryCodes.Turkey);
					if (IsExport || (!IsExport && (yfkNo.IsEmpty || ProcedureCode == ProcedureCodes._7241)))
					{
						result = taxNo;
					}
				}
				return result;
			}
		}

		public string ConsigneeTaxNo => !IsExport || ProcedureCode == ProcedureCodes._7200 || ProcedureCode == ProcedureCodes._7272 ? JobDeclaration.Importer?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey).ToString() ?? string.Empty : string.Empty;
		public string DeclarantTaxNo => JobDeclaration.DeclarantAddress?.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? string.Empty;
		public string AdvisorTaxNo => JobDeclaration.RepresentativeOrgAddress?.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? string.Empty;
		public string PrincipleResponsibleTaxNo => string.Empty;

		public IReadOnlyCollection<ITrader> Traders
		{
			get
			{
				if (fTraders == null)
				{
					var tradersList = new List<ITrader>();
					var supplierVATNo = ZString.Empty;
					var yfkNo = ZString.Empty;

					if (!IsExport)
					{
						var supplier = JobDeclaration.Supplier;
						if (supplier != null)
						{
							supplierVATNo = supplier.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey);
							yfkNo = JobDeclaration.Supplier.CustomsCodes.GetCustomsRegNo(TurkeyOrgCusCodeInfo.OrgCusCodes.YFK, Core.Constants.CountryCodes.Turkey);

							if ((!yfkNo.IsEmpty || supplierVATNo.IsEmpty) && JobDeclaration.SupplierDocumentaryAddress != null)
							{
								tradersList.Add(new TraderProvider(JobDeclaration.SupplierDocumentaryAddress.Address, DocAddressTypes.Codes.SupplierDocumentaryAddress, IsExport, yfkNo, ProcedureCode, EffectiveDate));
							}
						}
					}
					else if (JobDeclaration.ImporterDocumentaryAddress?.Address != null)
					{
						yfkNo = JobDeclaration.Importer.CustomsCodes.GetCustomsRegNo(TurkeyOrgCusCodeInfo.OrgCusCodes.YFK, Core.Constants.CountryCodes.Turkey);
						tradersList.Add(new TraderProvider(JobDeclaration.ImporterDocumentaryAddress.Address, DocAddressTypes.Codes.ImporterDocumentaryAddress, IsExport, yfkNo, ProcedureCode, EffectiveDate));
					}

					if (JobDeclaration.Traders.Count > 0)
					{
						foreach (var trader in JobDeclaration.Traders.Where(x => x.Address != null))
						{
							yfkNo = trader.Organisation.CustomsCodes.GetCustomsRegNo(TurkeyOrgCusCodeInfo.OrgCusCodes.YFK, Core.Constants.CountryCodes.Turkey);
							tradersList.Add(new TraderProvider(trader.Address, trader.E2_AddressType, IsExport, yfkNo, ProcedureCode, EffectiveDate));
						}
					}

					fTraders = tradersList.ToArray();
				}

				return fTraders;
			}
		}
		IReadOnlyCollection<ITrader> fTraders;

		#endregion

		#region Details

		IReadOnlyCollection<IGuarantees> guaranteesCache;
		public IReadOnlyCollection<IGuarantees> Guarantees => !IsExport ? guaranteesCache ?? (guaranteesCache = JobDeclaration.Guarantees.Select(guarantee => new GuaranteesProvider(guarantee)).ToArray()) : Array.Empty<IGuarantees>();

		public IReadOnlyCollection<IQuestionsAndAnswers> QuestionsAndAnswers
		{
			get
			{
				if (MessageType == TRMessageTypes.Codes.DKO)
				{
					return ImmutableArray<IQuestionsAndAnswers>.Empty;
				}

				if (fQuestionsAndAnswers == null)
				{
					fQuestionsAndAnswers = new List<IQuestionsAndAnswers>();

					fQuestionsAndAnswers.AddRange(JobDeclaration.CusEntryHeader.CPDecCollection.Where(x => x.ON_QuestionType == QuestionTypeList.Codes.Q)
																							   .Select(cpDec => new CusEntryCPDecProvider(cpDec, MessageType, ZInt.Zero)));
					if (JobDeclaration.CusEntryHeader.AllEntryLines != null)
					{
						foreach (CusEntryLine entryLine in JobDeclaration.CusEntryHeader.AllEntryLines)
						{
							fQuestionsAndAnswers.AddRange(entryLine.CPDecCollection.Where(x => x.ON_QuestionType == QuestionTypeList.Codes.Q)
																				   .Select(cpDec => new CusEntryCPDecProvider(cpDec, MessageType, entryLine.CL_LineNumber)));
						}
					}
				}

				return fQuestionsAndAnswers;
			}
		}
		List<IQuestionsAndAnswers> fQuestionsAndAnswers;

		public IReadOnlyCollection<IDocuments> Documents
		{
			get
			{
				if (fDocuments == null)
				{
					var list = new List<IDocuments>();
					foreach (CusEntryLine entryline in JobDeclaration.CusEntryHeader.AllEntryLines)
					{
						foreach (JobComInvoiceLine invoiceline in entryline.InvoiceLines)
						{
							foreach (SupportingDocument document in invoiceline.SupportingDocuments)
							{
								if (!list.Any(x => x.LineNo == entryline.CL_LineNumber && x.Code == document.CSI_Code))
								{
									list.Add(new SupportingDocumentsProvider(document, entryline.CL_LineNumber));
								}
							}
						}
					}

					fDocuments = list.ToArray();
				}
				return fDocuments;
			}
		}
		IReadOnlyCollection<IDocuments> fDocuments;

		public IReadOnlyCollection<ITaxes> Taxes
		{
			get
			{
				if (fTaxes == null)
				{
					fTaxes = new List<ITaxes>();
					foreach (CusEntryLine entryline in JobDeclaration.CusEntryHeader.AllEntryLines)
					{
						fTaxes.AddRange(entryline.Fees.Cast<CusEntryLineFee>().Select(x => new CusEntryLineFeeProvider(x)));
					}
				}

				return fTaxes;
			}
		}
		List<ITaxes> fTaxes;

		public IReadOnlyCollection<IDeclarationOfValue> DeclarationOfValue
		{
			get
			{
				if (IsExport)
				{
					return Array.Empty<IDeclarationOfValue>();
				}
				else
				{
					if (fDeclarationOfValue == null)
					{
						fDeclarationOfValue = new List<IDeclarationOfValue>();
						fDeclarationOfValue.AddRange(JobDeclaration.Invoices.Cast<JobComInvoiceHeader>().Select(x => new DeclarationOfValueProvider(x)));
					}
				}

				return fDeclarationOfValue;
			}
		}
		List<IDeclarationOfValue> fDeclarationOfValue;

		IReadOnlyCollection<ISummaryDeclaration> summaryDeclarationCache;
		public IReadOnlyCollection<ISummaryDeclaration> SummaryDeclaration => !JobDeclaration.IsExport ? summaryDeclarationCache ??
			(summaryDeclarationCache = JobDeclaration.ManifestToOpenHeaders.Select(header => new ManifestToOpenBillsHeaderProvider(header)).ToArray()) : Array.Empty<ISummaryDeclaration>();
		#endregion

		IReadOnlyCollection<IEntryLines> linesCache;
		public IReadOnlyCollection<IEntryLines> EntryLines => linesCache ?? (linesCache = CusEntryHeader.AllEntryLines.Select(lines => new EntryLinesProvider(lines, MessageType, EffectiveDate)).ToArray());

		#region Communication

		public string Mail1 => DeclarationProviderHelper.GetMailAddress(1);
		public string Mail2 => DeclarationProviderHelper.GetMailAddress(2);
		public string Mail3 => DeclarationProviderHelper.GetMailAddress(3);
		public string Mobil1 => GlbStaff.CurrentUser.GS_MobilePhone;
		public string Mobil2 => string.Empty;

		#endregion

		public string OvertimeWorkID => string.Empty;
		public string PortCode => IsExport ? JobDeclaration.JE_CustomsLoadPort : JobDeclaration.JE_CustomsDischargePort;

		public string AgencyDispatchNotificationNo
		{
			get
			{
				if (IsExport && JobDeclaration.JE_TransportMode == Core.Constants.TransportModes.Air && JobDeclaration.JE_TransportModeInland == TRTransportModeInland.Codes._40)
				{
					return JobDeclaration.JE_AgentsReference;
				}

				return ZString.Empty;
			}
		}

		public string PayAccountingInfo => string.Empty;

		TransportModeTranslator TransportModeTranslator => transportModeTranslator ?? (transportModeTranslator = new TransportModeTranslator());
		TransportModeTranslator transportModeTranslator;

		#region Export Union Fields

		public IExportUnionDeclaration ExportUnionDeclaration => MessageType == TRMessageTypes.Codes.EUT ? new ExportUnionDeclarationProvider(JobDeclaration, InvoiceLine, CusEntryInstruction) : null;

		#endregion
	}
}
