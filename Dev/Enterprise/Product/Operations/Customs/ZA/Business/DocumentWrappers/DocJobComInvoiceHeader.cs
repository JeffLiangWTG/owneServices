using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocJobComInvoiceHeader : DocBaseJobComInvoiceHeader
	{
		DocJobComInvoiceHeader(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceHeader, factoryToWrap)
		{
		}

		public static DocJobComInvoiceHeader New(JobComInvoiceHeader jobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
		{
			if (jobComInvoiceHeader == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceHeader(jobComInvoiceHeader, factoryToWrap);
			}
		}

		#region Template Constants

		public ZInt MarksAndNumbersWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 1); }
		}

		public ZInt GoodsDescWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 1); }
		}

		public ZInt MarksAndNumbsAndDescHeight
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 1); }
		}

		public ZInt NoOfContainerRows
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1); }
		}

		#endregion

		#region Overrides

		public override ZString TariffHeading
		{
			get { return (NoResString)"Tariff / Rebate / Trade Agreement / Sch 5/6 / Sch 2 / Sch 1 P 2A/2B"; }
		}

		protected override DocBaseJobDeclaration CreateJobDeclaration(Enterprise.Customs.Business.BaseJobDeclaration declarationToWrap)
		{
			return DocDeclaration.New((JobDeclaration)declarationToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Enterprise.Customs.Business.BaseJobComInvoiceLineViewCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection((JobComInvoiceLineViewCollection)collectionToWrap, Factory);
		}

		public ZString ConversionFactorAsString
		{
			get
			{
				if (base.ConversionFactor == ZDecimal.Zero)
				{
					return "-";
				}
				else
				{
					return base.ConversionFactor.ToString();
				}
			}
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal InvoiceAmountInZAR
		{
			get
			{
				return ConvertAmount(InvoiceAmount, InvoiceCurr);
			}
		}

		public ZDecimal FOBInZAR
		{
			get { return ConvertAmount(FOBAmount, FOBCurrency); }
		}

		protected ZDecimal ConvertAmount(ZDecimal amount, DocCurrency currency)
		{
			ZDecimal result = amount;

			if (currency != null && currency.Code != Core.Constants.CurrencyCodes.SouthAfrica)
			{
				var zARCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, "ZAR"));
				var currentCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, currency.Code));

				CurrencyConverter converter = JobComInvoiceHeader.CurrencyConverter;
				Money moneyAmount = new Money(amount, currentCurrency);
				result = converter.ConvertRounded(moneyAmount, zARCurrency).Amount;
			}

			return result;
		}

		public ZDecimal TotalCIFAndCInLocalCurrency
		{
			get { return JobComInvoiceHeader.JZ_Calc_CIFAmount_InLocalCurrency; }
		}

		#endregion

		#region ZString Fields

		public ZString InvoiceAmountWithExchangeRate
		{
			get
			{
				ZString result = ZString.Empty;

				if (InvoiceCurr != null && InvoiceCurr.Code != Core.Constants.CurrencyCodes.SouthAfrica)
				{
					result = InvoiceCurr.Code + " " + InvoiceAmount.ToString() + " @ = " + InvoiceCurrExRate;
				}

				return result;
			}
		}

		#region Spilt Date

		public ZString DayChar1
		{
			get { return new ZString(CurrentDate[0].ToString()); }
		}

		public ZString DayChar2
		{
			get { return new ZString(CurrentDate[1].ToString()); }
		}

		public ZString MonthChar1
		{
			get { return new ZString(CurrentDate[2].ToString()); }
		}

		public ZString MonthChar2
		{
			get { return new ZString(CurrentDate[3].ToString()); }
		}

		public ZString YearChar1
		{
			get { return new ZString(CurrentDate[4].ToString()); }
		}

		public ZString YearChar2
		{
			get { return new ZString(CurrentDate[5].ToString()); }
		}

		public ZString YearChar3
		{
			get { return new ZString(CurrentDate[6].ToString()); }
		}

		public ZString YearChar4
		{
			get { return new ZString(CurrentDate[7].ToString()); }
		}

		protected internal ZString fCurrentDate;
		protected ZString CurrentDate
		{
			get
			{
				if (fCurrentDate.IsEmpty)
				{
					ZString currentDate = ZDateTime.Today.Day.ToString().PadLeft(2, '0');
					ZString currentMonth = ZDateTime.Today.Month.ToString().PadLeft(2, '0');
					ZString currentYear = ZDateTime.Today.Year.ToString();
					fCurrentDate = currentDate + currentMonth + currentYear;
				}

				return fCurrentDate;
			}
		}

		#endregion

		#endregion

		#region Collections

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}
		public DocJobComInvoiceLineCollection UnclassifiedInvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)UnclassifiedInvoiceLinesInternal; }
		}

		#endregion

		#region Wrapper Fields

		public DocDeclaration Declaration
		{
			get { return (DocDeclaration)DeclarationInternal; }
		}

		public DocExchangeControlDec ExchangeControlDec
		{
			get { return new DocExchangeControlDec(this); }
		}

		#endregion

		#region Implementation

		JobComInvoiceHeader JobComInvoiceHeader
		{
			get { return (JobComInvoiceHeader)WrappedObject; }
		}

		#endregion
	}
}
