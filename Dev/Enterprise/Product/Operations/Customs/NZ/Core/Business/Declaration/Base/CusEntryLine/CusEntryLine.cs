using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[DependentBusinessObject(typeof(CusEntryHeader), "MergedLines")]
	public class CusEntryLine : Customs.Business.CusEntryLine, Integration.Customs.NZ.ICusEntryLine
	{
		public static new readonly CusEntryLineTypeDecider TypeDecider = new CusEntryLineTypeDecider();

		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region public new CusEntryLineFeeCollection Fees
		public new CusEntryLineFeeCollection Fees
		{
			get { return (CusEntryLineFeeCollection)base.Fees; }
		}

		protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection()
		{
			return new CusEntryLineFeeCollection(this, Factory);
		}
		#endregion

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public CusEntryHeader EntryHeader
		{
			get { return base.Header as CusEntryHeader; }
		}

		protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		#region Amounts Entered on InvoiceLine and Totalled to EntryLine
		#region ALACLevyCreditAmount
		public ZDecimal ALACLevyCreditAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.ALACLevyCredit); }
			set { ALACLevyCreditLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee ALACLevyCreditLineFee
		{
			get
			{
				if (fALACLevyCreditLineFee == null || fALACLevyCreditLineFee.IsDeleted)
				{
					fALACLevyCreditLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.ALACLevyCredit);
				}
				return fALACLevyCreditLineFee;
			}
		}
		CusEntryLineFee fALACLevyCreditLineFee;
		#endregion

		#region ACCLevyCreditAmount
		public ZDecimal ACCLevyCreditAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.ACCLevyCredit); }
			set { ACCLevyCreditLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee ACCLevyCreditLineFee
		{
			get
			{
				if (fACCLevyCreditLineFee == null || fACCLevyCreditLineFee.IsDeleted)
				{
					fACCLevyCreditLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.ACCLevyCredit);
				}
				return fACCLevyCreditLineFee;
			}
		}
		CusEntryLineFee fACCLevyCreditLineFee;
		#endregion

		#region HERALevyCreditAmount
		public ZDecimal HERALevyCreditAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.HERALevyCredit); }
			set { HEARLevyCreditLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee HEARLevyCreditLineFee
		{
			get
			{
				if (fHERALevyCreditLineFee == null || fHERALevyCreditLineFee.IsDeleted)
				{
					fHERALevyCreditLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.HERALevyCredit);
				}
				return fHERALevyCreditLineFee;
			}
		}
		CusEntryLineFee fHERALevyCreditLineFee;
		#endregion

		#region PFMLFuelLevyCreditAmount
		public ZDecimal PFMLFuelLevyCreditAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.PFMLFuelLevyCredit); }
			set { PFMLFuelLevyCreditLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee PFMLFuelLevyCreditLineFee
		{
			get
			{
				if (fPFMLFuelLevyCreditLineFee == null || fPFMLFuelLevyCreditLineFee.IsDeleted)
				{
					fPFMLFuelLevyCreditLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.PFMLFuelLevyCredit);
				}
				return fPFMLFuelLevyCreditLineFee;
			}
		}
		CusEntryLineFee fPFMLFuelLevyCreditLineFee;
		#endregion

		#region AntiDumpingDutyAmount
		public ZDecimal AntiDumpingDutyAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.AntiDumpingDuty); }
			set { AntiDumpingDutyLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee AntiDumpingDutyLineFee
		{
			get
			{
				if (fAntiDumpingDutyLineFee == null || fAntiDumpingDutyLineFee.IsDeleted)
				{
					fAntiDumpingDutyLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.AntiDumpingDuty);
				}
				return fAntiDumpingDutyLineFee;
			}
		}
		CusEntryLineFee fAntiDumpingDutyLineFee;
		#endregion

		#region CountervailingDutyAmount
		public ZDecimal CountervailingDutyAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.CountervailingDuty); }
			set { CountervailingDutyLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee CountervailingDutyLineFee
		{
			get
			{
				if (fCountervailingDutyLineFee == null || fCountervailingDutyLineFee.IsDeleted)
				{
					fCountervailingDutyLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.CountervailingDuty);
				}
				return fCountervailingDutyLineFee;
			}
		}
		CusEntryLineFee fCountervailingDutyLineFee;
		#endregion

		#region DutyCreditAmount
		public ZDecimal DutyCreditAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.DutyCredit); }
			set { DutyCreditLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee DutyCreditLineFee
		{
			get
			{
				if (fDutyCreditLineFee == null || fDutyCreditLineFee.IsDeleted)
				{
					fDutyCreditLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.DutyCredit);
				}
				return fDutyCreditLineFee;
			}
		}
		CusEntryLineFee fDutyCreditLineFee;
		#endregion

		#region GSTCreditAmount
		public ZDecimal GSTCreditAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.GSTCredit); }
			set { GSTCreditLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee GSTCreditLineFee
		{
			get
			{
				if (fGSTCreditLineFee == null || fGSTCreditLineFee.IsDeleted)
				{
					fGSTCreditLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.GSTCredit);
				}
				return fGSTCreditLineFee;
			}
		}
		CusEntryLineFee fGSTCreditLineFee;
		#endregion

		#region DepositRefundAmount
		public ZDecimal DepositRefundAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.DepositRefund); }
			set { DepositRefundLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee DepositRefundLineFee
		{
			get
			{
				if (fDepositRefundLineFee == null || fDepositRefundLineFee.IsDeleted)
				{
					fDepositRefundLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.DepositRefund);
				}
				return fDepositRefundLineFee;
			}
		}
		CusEntryLineFee fDepositRefundLineFee;
		#endregion

		#region ExciseDutyCreditAmount
		public ZDecimal ExciseDutyCreditAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.ExciseDutyCredit); }
			set { ExciseDutyCreditLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee ExciseDutyCreditLineFee
		{
			get
			{
				if (fExciseDutyCreditLineFee == null || fExciseDutyCreditLineFee.IsDeleted)
				{
					fExciseDutyCreditLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.ExciseDutyCredit);
				}
				return fExciseDutyCreditLineFee;
			}
		}
		CusEntryLineFee fExciseDutyCreditLineFee;
		#endregion
		#endregion

		#region Freight and Insurance
		/// <summary>
		/// In NZD
		/// </summary>
		public ZDecimal Freight
		{
			get { return CurrencyConverter.ConvertExact(base.OverseasFreight, CurrencyConverter.LocalCurrency).Amount; }
		}

		public ZPropertyInfo FreightInfo
		{
			get { return GetZPropertyInfo(nameof(Freight)); }
		}

		/// <summary>
		/// In NZD
		/// </summary>
		public ZDecimal Insurance
		{
			get { return CurrencyConverter.ConvertExact(base.OverseasInsurance, CurrencyConverter.LocalCurrency).Amount; }
		}

		public ZPropertyInfo InsuranceInfo
		{
			get { return GetZPropertyInfo(nameof(Insurance)); }
		}

		public ZDecimal FreightWholeNZD
		{
			get { return ZArchitecture.Core.Utilities.Round(Freight, 0); }
		}

		public ZDecimal InsuranceWholeNZD
		{
			get { return ZArchitecture.Core.Utilities.Round(Insurance, 0); }
		}

		public ZDecimal VFDWholeNZD
		{
			get { return ZArchitecture.Core.Utilities.Round(CL_CustomsValue, 0); }
		}
		#endregion

		public ZDecimal CommissionInNZD
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (BaseInvoiceLineApportionedCharge charge in invoiceLine.ApportionedCharges)
					{
						if (charge.J7_ChargeType == CustomsChargeTypeList.Codes.Commission)
						{
							result += CurrencyConverter.ConvertExact(charge.Money, CurrencyConverter.LocalCurrency).Amount;
						}
					}

					foreach (BaseInvoiceLineCharge charge in invoiceLine.Charges)
					{
						if (charge.J7_ChargeType == CustomsChargeTypeList.Codes.Commission)
						{
							result += CurrencyConverter.ConvertExact(charge.Money, CurrencyConverter.LocalCurrency).Amount;
						}
					}
				}

				return result;
			}
		}

		public ZDecimal RoyaltiesInNZD
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (BaseInvoiceLineApportionedCharge charge in invoiceLine.ApportionedCharges)
					{
						if (charge.J7_ChargeType == CustomsChargeTypeList.TSWCodes.Royalties)
						{
							result += CurrencyConverter.ConvertExact(charge.Money, CurrencyConverter.LocalCurrency).Amount;
						}
					}

					foreach (BaseInvoiceLineCharge charge in invoiceLine.Charges)
					{
						if (charge.J7_ChargeType == CustomsChargeTypeList.TSWCodes.Royalties)
						{
							result += CurrencyConverter.ConvertExact(charge.Money, CurrencyConverter.LocalCurrency).Amount;
						}
					}
				}

				return result;
			}
		}

		protected override ZDecimal GetInvoiceLineCustomsValueToAggregate(BaseJobComInvoiceLine invoiceLine)
		{
			return invoiceLine.InvoiceHeader != null ? ConvertToLocalAmountExact(invoiceLine.JI_FOB, false).Amount : ZDecimal.Zero;
		}

		#region Totals

		public ZDecimal TotalLevies
		{
			get
			{
				return ALACLevyAmount
					+ ACCFuelLevyAmount
					+ PFMLFuelLevyAmount
					+ HERALevyAmount
					+ ALACLevyCreditAmount
					+ ACCLevyCreditAmount
					+ HERALevyCreditAmount
					+ PFMLFuelLevyCreditAmount
					+ SyntheticGreenhouseGasesLevyAmount;
			}
		}

		public ZPropertyInfo TotalLeviesInfo
		{
			get { return GetZPropertyInfo(nameof(TotalLevies)); }
		}

		public ZDecimal TotalMisc
		{
			get { return TotalLevies + AntiDumpingDutyAmount + CountervailingDutyAmount; }
		}

		public ZDecimal TotalDutiesAndLevies
		{
			get { return DutyAmount + DutyCreditAmount + ExciseDutyCreditAmount + TotalMisc; }
		}

		public ZDecimal TotalGST
		{
			get { return GSTAmount + GSTCreditAmount; }
		}

		public ZDecimal TotalAmountPayable
		{
			get { return TotalDutiesAndLevies + TotalGST + DepositRefundAmount; }
		}

		public ZDecimal TotalDutyLevyGST
		{
			get { return DutyAmount + TotalLevies + GSTVATAmount; }
		}
		#endregion

		#region Amounts Calculated from InvoiceLine and Proxied back to InvoiceLine

		#region ALACLevyAmount

		public ZDecimal ALACLevyAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.ALACLevy); }
			set { ALACLevyLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee ALACLevyLineFee
		{
			get
			{
				if (fALACLevyLineFee == null || fALACLevyLineFee.IsDeleted)
				{
					fALACLevyLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.ALACLevy);
				}
				return fALACLevyLineFee;
			}
		}
		CusEntryLineFee fALACLevyLineFee;

		#endregion

		#region ACCFuelLevyAmount
		public ZDecimal ACCFuelLevyAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.ACCFuelLevy); }
			set { ACCFuelLevyLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee ACCFuelLevyLineFee
		{
			get
			{
				if (fACCFuelLevyLineFee == null || fACCFuelLevyLineFee.IsDeleted)
				{
					fACCFuelLevyLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.ACCFuelLevy);
				}
				return fACCFuelLevyLineFee;
			}
		}
		CusEntryLineFee fACCFuelLevyLineFee;
		#endregion

		#region PFMLFuelLevyAmount

		public ZDecimal PFMLFuelLevyAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.PFMLFuelLevy); }
			set { PFMLFuelLevyLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee PFMLFuelLevyLineFee
		{
			get
			{
				if (fPFMLFuelLevyLineFee == null || fPFMLFuelLevyLineFee.IsDeleted)
				{
					fPFMLFuelLevyLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.PFMLFuelLevy);
				}
				return fPFMLFuelLevyLineFee;
			}
		}
		CusEntryLineFee fPFMLFuelLevyLineFee;

		#endregion

		#region SyntheticGreenhouseGasesLevyAmount

		public ZDecimal SyntheticGreenhouseGasesLevyAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.SGGLevy); }
			set { SyntheticGreenhouseGasesLevyLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee SyntheticGreenhouseGasesLevyLineFee
		{
			get
			{
				if (fSyntheticGreenhouseGasesLevyLineFee == null || fSyntheticGreenhouseGasesLevyLineFee.IsDeleted)
				{
					fSyntheticGreenhouseGasesLevyLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.SGGLevy);
				}
				return fSyntheticGreenhouseGasesLevyLineFee;
			}
		}
		CusEntryLineFee fSyntheticGreenhouseGasesLevyLineFee;

		#endregion

		#region HERALevyAmount
		public ZDecimal HERALevyAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.HERALevy); }
			set { HERALevyLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee HERALevyLineFee
		{
			get
			{
				if (fHERALevyLineFee == null || fHERALevyLineFee.IsDeleted)
				{
					fHERALevyLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.HERALevy);
				}
				return fHERALevyLineFee;
			}
		}
		CusEntryLineFee fHERALevyLineFee;
		#endregion
		#region DutyAmount
		[ReadOnly(true)]
		public new ZDecimal DutyAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.Duty); }
			set
			{
				DutyLineFee.CF_ChargeAmount = value;
				DutyAmountInfo.RefreshBinding();
			}
		}

		public ZDecimal DissectionReportDutyAmount
		{
			get
			{
				if (Declaration.IsExport)
				{
					var result = ZDecimal.Zero;
					if (Declaration.IsDrawback)
					{
						foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
						{
							result += invoiceLine.JI_DutyCreditAmount;
						}
					}

					return result;
				}
				else
				{
					return DutyAmount;
				}
			}
		}

		public ZString DissectionReportDutyRate
		{
			get
			{
				return (Declaration.IsExport) ? "" : "Duty Rate: " + CL_DutyPercent.ToString("N2", CultureInfo.CurrentCulture) + "%";
			}
		}

		CusEntryLineFee DutyLineFee
		{
			get
			{
				if (fDutyLineFee == null || fDutyLineFee.IsDeleted)
				{
					fDutyLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.Duty);
				}
				return fDutyLineFee;
			}
		}
		CusEntryLineFee fDutyLineFee;
		#endregion
		#region GSTAmount
		public ZDecimal GSTAmount
		{
			get { return Fees.GetAmount(EntryChargeTypeList.Codes.GST); }
			set { GSTLineFee.CF_ChargeAmount = value; }
		}

		CusEntryLineFee GSTLineFee
		{
			get
			{
				if (fGSTLineFee == null || fGSTLineFee.IsDeleted)
				{
					fGSTLineFee = Fees.GetOrAddFeeByFeeType(EntryChargeTypeList.Codes.GST);
				}
				return fGSTLineFee;
			}
		}
		CusEntryLineFee fGSTLineFee;
		#endregion
		#endregion

		#region Statistical/Supplementary Unit and Quantity

		public ZString StatisticalUnit
		{
			get { return CustomsUnitQty; }
		}

		public ZString SupplementaryUQ
		{
			get { return RandomLine.JI_SupplementaryUQ; }
		}

		public ZDecimal StatisticalQty
		{
			get { return CustomsQuantity.Round(JobComInvoiceLine.CustomsQtyMaxDecimalPlaces); }
		}

		public ZDecimal SupplementaryQty
		{
			get
			{
				ZDecimal result = 0m;
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					result += invoiceLine.JI_SupplementaryQty;
				}
				return result.Round(JobComInvoiceLine.CustomsQtyMaxDecimalPlaces);
			}
		}

		public ZPropertyInfo SupplementaryQtyInfo
		{
			get { return GetZPropertyInfo(nameof(SupplementaryQty)); }
		}

		public ZString SupplierName
		{
			get
			{
				JobComInvoiceHeader invoiceHeader = RandomLine.InvoiceHeader;
				return invoiceHeader == null ? ZString.Empty : invoiceHeader.SupplierName;
			}
		}

		#endregion

		#region ZeroRating
		public ZBool IsZeroRatedDuty
		{
			get { return RandomLine.EffectiveIsZeroRatedDuty; }
		}

		public ZBool IsZeroRatedExcise
		{
			get { return RandomLine.EffectiveIsZeroRatedExcise; }
		}

		public ZBool IsZeroRatedLevies
		{
			get { return RandomLine.EffectiveIsZeroRatedLevies; }
		}

		public ZBool IsZeroRatedGST
		{
			get { return RandomLine.EffectiveIsZeroRatedGST; }
		}

		public ZBool IsDutyOnlyGST
		{
			get { return RandomLine.IsDutyOnlyGST; }
		}
		#endregion

		#region Country Of Origin
		public new ZString CountryOfOrigin
		{
			get { return RandomLine.JI_RN_NKEffectiveCountryOfOrigin; }
		}

		public ZString CountryOfOriginDescription
		{
			get { return RandomLine.CountryOfOriginDescription; }
		}

		public ZPropertyInfo CountryOfOriginDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CountryOfOriginDescription)); }
		}
		#endregion

		#region RandomLine
		public new JobComInvoiceLine RandomLine
		{
			get { return (JobComInvoiceLine)base.RandomLine; }
		}

		CachedProperty<JobComInvoiceLine> randomLine;
		protected override BaseJobComInvoiceLine GetRandomLine()
		{
			Factory.GetValue(ref randomLine, delegate
			{
				JobComInvoiceLine fRandomLine = null;
				if (InvoiceLines != null && InvoiceLines.Count > 1)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						if (invoiceLine.JI_ParentLineNo == ZString.Empty)
						{
							fRandomLine = invoiceLine;
							break;
						}
					}
				}
				if (fRandomLine == null)
				{
					fRandomLine = GetBaseRandomLine();
				}
				return fRandomLine;
			});

			return randomLine.Value;
		}

		JobComInvoiceLine GetBaseRandomLine()
		{
			return (JobComInvoiceLine)base.GetRandomLine();
		}
		#endregion

		public ZPropertyInfo IncoTermInfo
		{
			get { return GetZPropertyInfo("IncoTerm"); }
		}

		new public ZPropertyInfo DutyAmountInfo
		{
			get { return GetZPropertyInfo(nameof(DutyAmount)); }
		}

		public ZPropertyInfo TotalLinePriceInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo("TotalLinePriceInLocalCurrency"); }
		}

		public ZPropertyInfo DutyRateDescriptionInfo
		{
			get { return GetZPropertyInfo("DutyRateDescription"); }
		}

		public ZPropertyInfo TotalDutyLevyGSTInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDutyLevyGST)); }
		}

		[ReadOnly(true)]
		public ZString ConcessionCode
		{
			get { return RandomLine.JI_ConcessionCode; }
		}

		public ZPropertyInfo ConcessionCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ConcessionCode)); }
		}

		public ZString UNDGNo
		{
			get { return RandomLine.UNDGs.Count > 0 && RandomLine.UNDGs[0].UNDGSubstance != null ? RandomLine.UNDGs[0].UNDGSubstance.DG_Code : ZString.Empty; }
		}

		public ZString LotNumber
		{
			get { return RandomLine.JI_LotNumber; }
		}

		public ZDateTime DateMarking
		{
			get { return RandomLine.JI_DateMarking; }
		}

		public ZString IntendedUse
		{
			get { return RandomLine.JI_IntendedUse; }
		}

		public ZString IntendedUseCode
		{
			get { return RandomLine.JI_IntendedUseCode; }
		}

		public ZString OriginRegion
		{
			get { return RandomLine.JI_EffectiveOriginRegion; }
		}

		public OrgHeader TreatmentProvider
		{
			get { return RandomLine.TreatmentProvider; }
		}

		public NZCommodityCollection CommodityLines
		{
			get { return RandomLine.CommodityLines; }
		}

		public NZCommodityConstituentCollection CommodityConstituents
		{
			get { return RandomLine.CommodityConstituents; }
		}

		public NZCommodityItineraryCollection CommodityItineraries
		{
			get { return RandomLine.CommodityItineraries; }
		}

		public NZCommodityProductCollection CommodityProducts
		{
			get { return RandomLine.CommodityProducts; }
		}

		public IEnumerable<ItemPackaging> ItemPackaging
		{
			get
			{
				var allPackagings = new List<ItemPackaging>();
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines.ToArray())
				{
					allPackagings.AddRange(invoiceLine.ItemPackages.OfType<ItemPackaging>());
				}
				return allPackagings;
			}
		}

		public ZString PartsOfClassification
		{
			get { return RandomLine.JI_PartsOfClassification; }
		}

		public ZString BrandName
		{
			get { return RandomLine.JI_BrandName; }
		}

		public ZString CommonName
		{
			get { return RandomLine.JI_CommonName; }
		}

		public ZString RegisteredName
		{
			get { return RandomLine.JI_RegisteredName; }
		}

		public ZString TradeName
		{
			get { return RandomLine.JI_TradeName; }
		}

		public ZBool UsedGoods
		{
			get { return RandomLine.JI_UsedGoods; }
		}

		public ZBool GeneticallyModified
		{
			get { return RandomLine.JI_GeneticallyModified; }
		}

		public ZString CountryOfExport
		{
			get { return RandomLine.JI_RN_NKEffectiveCountryOfExport; }
		}

		public OrgHeader Supplier
		{
			get
			{
				return RandomLine.InvoiceHeader != null
					? RandomLine.InvoiceHeader.Supplier
					: null;
			}
		}

		public OrgHeader Buyer
		{
			get
			{
				return RandomLine.InvoiceHeader != null
					? RandomLine.InvoiceHeader.Buyer
					: null;
			}
		}

		public ZString RelationshipIndicator
		{
			get
			{
				return RandomLine.InvoiceHeader != null
					? RandomLine.InvoiceHeader.JZ_RelationshipIndicator
					: null;
			}
		}

		public ZDecimal ExchangeRate
		{
			get
			{
				return RandomLine.InvoiceHeader != null
					? RandomLine.InvoiceHeader.JZ_InvoiceCurrExRate
					: ZDecimal.Zero;
			}
		}

		public RefCurrency OSCurrency
		{
			get { return RandomLine.InvoiceHeader != null ? RandomLine.InvoiceHeader.Invoice_Currency : null; }
		}

		public ZString OSCurrencyCode
		{
			get
			{
				if (OSCurrency != null)
				{
					return OSCurrency.RX_Code;
				}
				else
				{
					return "NZD";
				}
			}
		}

		public ZDecimal ForeignAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (BaseJobComInvoiceLine invoiceLine in InvoiceLines)
				{
					if (invoiceLine.JI_RX_NKLinePriceCurr != "NZD")
					{
						result += invoiceLine.JI_Calc_FOB;
					}
				}

				return result;
			}
		}

		public ZDecimal OSCustomsValue
		{
			get { return CurrencyConverter.ConvertRounded(CustomsValue, OSCurrency).Amount; }
		}

		public ZPropertyInfo OSCustomsValueInfo
		{
			get { return GetZPropertyInfo(nameof(OSCustomsValue)); }
		}

		protected override ZString DescriptionInternal
		{
			get { return base.DescriptionInternal.ToUpper(); }
		}

		public ZDecimal EntryFeeAmount
		{
			get { return EntryHeader != null ? GetAmountApportionedFromCusEntryHeader(EntryHeader.EntryFeeAmount) : ZDecimal.Zero; }
		}

		public ZString PreferentialDutyIndicator
		{
			get { return RandomLine.JI_EffectiveQualifiesForPreferentialDuty; }
		}

		public ZString PreferentialDutyGroup
		{
			get { return RandomLine.JI_EffectivePreferentialCountryGroup; }
		}

		public ZString BestPreferentialDutyGroup
		{
			get { return RandomLine.JI_BestPreferentialCountryGroup; }
		}

		public ZString ExchangeRateIndicator
		{
			get { return RandomLine.InvoiceHeader != null ? RandomLine.InvoiceHeader.JZ_ExchangeRateIndicator : ZString.Empty; }
		}

		public PermitCodeCollection PermitCodes
		{
			get { return RandomLine.PermitCodes; }
		}

		public bool HasFSAPermit
		{
			get { return RandomLine.HasFSAPermit; }
		}

		public LineOtherInfoCollection OtherInfos
		{
			get { return RandomLine.OtherInfos; }
		}

		public ProhibitedCodeCollection ProhibitedCodes
		{
			get { return RandomLine.ProhibitedCodes; }
		}

		public void CalculateDutyAndGST()
		{
			var requireCalculate = ZBool.False;
			if (Declaration.EntryFeeUnPayable)
			{
				requireCalculate = RandomLine.HasLVXCode;
			}
			else if ((Declaration.IsImport && !Declaration.IsPrimaryIndustriesImportDeclaration) || Declaration.IsExcise)
			{
				requireCalculate = ZBool.True;
			}

			if (requireCalculate)
			{
				DutyCalculator dutyCalculator = new DutyCalculator(this);
				DutyAmount = dutyCalculator.DutyAmount;
				ALACLevyAmount = dutyCalculator.ALACLevyAmount;
				ACCFuelLevyAmount = dutyCalculator.ACCFuelLevyAmount;
				PFMLFuelLevyAmount = dutyCalculator.PFMLFuelLevyAmount;
				SyntheticGreenhouseGasesLevyAmount = dutyCalculator.SyntheticGreenhouseGasesLevyAmount;
				HERALevyAmount = dutyCalculator.HERALevyAmount;
				CL_DutyPercent = dutyCalculator.DutyRatePercent;
				CL_FlatAmount = dutyCalculator.DutyRateFlatRate;
				CL_FlatAmountUQ = dutyCalculator.DutyRateFlatUQ;
				CL_AdValoremTariff = dutyCalculator.ActualClassification; // for Ad Valorems

				GSTCalculator gSTCalculator = new GSTCalculator(this);
				GSTAmount = gSTCalculator.GSTAmount;
			}
			else
			{
				DutyAmount = 0.00m;
				ALACLevyAmount = 0.00m;
				ACCFuelLevyAmount = 0.00m;
				PFMLFuelLevyAmount = 0.00m;
				SyntheticGreenhouseGasesLevyAmount = 0.00m;
				HERALevyAmount = 0.00m;
				CL_DutyPercent = 0.00m;
				CL_FlatAmount = 0.00m;
				CL_FlatAmountUQ = ZString.Empty;
				CL_AdValoremTariff = RandomLine.JI_Tariff.Left(15);
				GSTAmount = 0.00m;
			}
		}

		protected override ZString GetDutyRateDescription()
		{
			return new DutyCalculator(this).DutyRateOnly;
		}

		protected override ZDecimal GetGSTRate()
		{
			return new GSTCalculator(this).GSTRate;
		}

		public void CopyInUserEnteredCustomsChargesAndCredits()
		{
			ZDecimal totalLevyCreditAmount = 0m;
			ZDecimal totalAntiDumpingDutyAmount = 0m;
			ZDecimal totalCounterVailingDutyAmount = 0m;
			ZDecimal totalDutyCreditAmount = 0m;
			ZDecimal totalGSTCreditAmount = 0m;
			ZDecimal totalDepositRefundAmount = 0m;
			ZDecimal totalExciseDutyCreditAmount = 0m;

			foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
			{
				totalLevyCreditAmount += invoiceLine.JI_LevyCreditAmount;
				totalAntiDumpingDutyAmount += invoiceLine.JI_AntiDumpingDutyAmount;
				totalCounterVailingDutyAmount += invoiceLine.JI_CountervailingDutyAmount;
				totalDutyCreditAmount += invoiceLine.JI_DutyCreditAmount;
				totalGSTCreditAmount += invoiceLine.JI_GSTCreditAmount;
				totalDepositRefundAmount += invoiceLine.JI_DepositRefundAmount;
				totalExciseDutyCreditAmount += invoiceLine.JI_ExciseDutyCreditAmount;
			}

			if (Declaration.IsImport)
			{
				AntiDumpingDutyAmount = totalAntiDumpingDutyAmount;
				CountervailingDutyAmount = totalCounterVailingDutyAmount;
			}
			else if (Declaration.IsExport && (Declaration.IsDrawback || Declaration.IsCompletion))
			{
				switch (RandomLine.JI_LevyCreditAmountCode)
				{
					case LevyCodesList.Codes.ALAC:
						ALACLevyCreditAmount = totalLevyCreditAmount;
						break;
					case LevyCodesList.Codes.ACC:
						ACCLevyCreditAmount = totalLevyCreditAmount;
						break;
					case LevyCodesList.Codes.HERA:
						HERALevyCreditAmount = totalLevyCreditAmount;
						break;
					case LevyCodesList.Codes.PFML:
						PFMLFuelLevyCreditAmount = totalLevyCreditAmount;
						break;
				}

				DutyCreditAmount = totalDutyCreditAmount;
				GSTCreditAmount = totalGSTCreditAmount;
			}
			else if (Declaration.IsExcise)
			{
				ExciseDutyCreditAmount = totalExciseDutyCreditAmount;
			}

			if ((Declaration.IsImport || Declaration.IsExport) && Declaration.IsCompletion)
			{
				DepositRefundAmount = totalDepositRefundAmount;
			}
		}

		protected CusEntryLineSyncroniser modifier;
	}
}
