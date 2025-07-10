using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		protected DocCusEntryLine(ICusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(ICusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryLine == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryLine(cusEntryLine, factoryToWrap);
			}
		}

		#region Overrides

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Enterprise.Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap)
		{
			return DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);
		}

		#endregion

		#region Wrapper Fields

		public DocJobComInvoiceLine InvoiceLine
		{
			get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
		}

		public DocCusEntryHeader CusEntryHeader
		{
			get { return DocCusEntryHeader.New(CusEntryLine.Header, Factory); }
		}

		#endregion

		#region Additional Information

		public ZString AdditionalInfoCodeOne
		{
			get
			{
				if (fAdditionalInfoCodeOne.IsEmpty)
				{
					BuildAddInfoColumns();
				}

				return fAdditionalInfoCodeOne;
			}
		}

		public ZString AdditionalInfoOne
		{
			get
			{
				if (fAdditionalInfoOne.IsEmpty)
				{
					BuildAddInfoColumns();
				}

				return fAdditionalInfoOne;
			}
		}

		public ZString AdditionalInfoCodeTwo
		{
			get
			{
				if (fAdditionalInfoCodeTwo.IsEmpty)
				{
					BuildAddInfoColumns();
				}

				return fAdditionalInfoCodeTwo;
			}
		}

		public ZString AdditionalInfoTwo
		{
			get
			{
				if (fAdditionalInfoTwo.IsEmpty)
				{
					BuildAddInfoColumns();
				}

				return fAdditionalInfoTwo;
			}
		}

		public ZString AdditionalInfoCodeThree
		{
			get
			{
				if (fAdditionalInfoCodeThree.IsEmpty)
				{
					BuildAddInfoColumns();
				}

				return fAdditionalInfoCodeThree;
			}
		}

		public ZString AdditionalInfoThree
		{
			get
			{
				if (fAdditionalInfoThree.IsEmpty)
				{
					BuildAddInfoColumns();
				}

				return fAdditionalInfoThree;
			}
		}
		public ZString AdditionalInfoCodeFour
		{
			get
			{
				if (fAdditionalInfoCodeFour.IsEmpty)
				{
					BuildAddInfoColumns();
				}

				return fAdditionalInfoCodeFour;
			}
		}

		public ZString AdditionalInfoFour
		{
			get
			{
				if (fAdditionalInfoFour.IsEmpty)
				{
					BuildAddInfoColumns();
				}

				return fAdditionalInfoFour;
			}
		}

		#endregion

		#region ZString Fields

		public ZDecimal MarkupPercent
		{
			get
			{
				ZDecimal result = 0m;
				if (CusEntryLine.Header != null && CusEntryLine.Header.Supplier != null && CusEntryLine.Header.Declaration.Importer != null)
				{
					OrgSupplierBuyerLink link = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(CusEntryLine.Header.Supplier, CusEntryLine.Header.Declaration.Importer, CusEntryLine.Header.Declaration.FinalDestinationCountryCode);
					if (link != null)
					{
						result = link.OL_ValuationBasisMarkupPercent / 100.0m;
					}
				}
				return result;
			}
		}

		public ZString HasVTEAddInfo
		{
			get { return AdditionalInfoHasCodeOf("VTE") ? "TRUE" : "FALSE"; }
		}

		public ZString ActualPrice
		{
			get
			{
				ZString result = ZString.Empty;

				if (!LineNumber.IsEmpty)
				{
					ZDecimal actualPrice = 0M;
					result = actualPrice > 0M ? actualPrice.ToString() : "NC";
				}

				return result;
			}
		}

		public ZString ValueDeterminationNumber
		{
			get { return CusEntryLine.ValueDeterminationNumber; }
		}

		public ZString TariffDescription
		{
			get { return CusEntryLine.Description; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal CustomsDuty
		{
			get { return CusEntryLine.CustomsDuty; }
		}

		protected CurrencyConverter CurrencyConverter
		{
			get { return ((JobComInvoiceLine)CusEntryLine.InvoiceLines[0]).InvoiceHeader.CurrencyConverter; }
		}

		public ZDecimal ConvertToLocalAmount(ZDecimal amount, RefCurrency currency)
		{
			ZDecimal result = amount;

			if (currency != null)
			{
				CurrencyConverter converter = CurrencyConverter;
				Money moneyAmount = new Money(amount, currency);
				result = converter.ConvertRounded(moneyAmount, GlbCompany.CurrentCompany.LocalCurrency).Amount;
			}

			return result;
		}

		public ZDecimal ImportDutyPaid
		{
			get { return CusEntryLine.ImportDutyPaid; }
		}

		public ZDecimal ImportDutySch1P2BPaid
		{
			get { return CusEntryLine.ImportDutySch1P2BPaid; }
		}

		public ZDecimal ImportVATPaid
		{
			get { return CusEntryLine.ImportVATPaid; }
		}

		public ZDecimal CustomsValueFromRelatedImportEntry
		{
			get { return CusEntryLine.ImportCustomsValue; }
		}

		#endregion

		#region Implementation

		protected bool AdditionalInfoHasCodeOf(string code) => AdditionalLineInfo.Any(addInfo => addInfo.CY_Code == code);

		protected ZBool IsImportMessage
		{
			get
			{
				if (CusEntryHeader != null)
				{
					return CusEntryHeader.Declaration.IsImportMessage;
				}

				return ZBool.False;
			}
		}

		protected AdditionalInformation[] fAdditionalLineInfo;
		protected AdditionalInformation[] AdditionalLineInfo
		{
			get
			{
				if (fAdditionalLineInfo == null)
				{
					var cusEntryLine = CusEntryLine as CusEntryLine;
					if (cusEntryLine?.AdditionalInformationCodes != null)
					{
						fAdditionalLineInfo = cusEntryLine.AdditionalInformationCodes.Cast<AdditionalInformation>().OrderBy(x => x.CY_Code).ToArray();
					}
				}

				return fAdditionalLineInfo;
			}
		}

		protected ZString fAdditionalInfoCodeOne;
		protected ZString fAdditionalInfoOne;
		protected ZString fAdditionalInfoCodeTwo;
		protected ZString fAdditionalInfoTwo;
		protected ZString fAdditionalInfoCodeThree;
		protected ZString fAdditionalInfoThree;
		protected ZString fAdditionalInfoCodeFour;
		protected ZString fAdditionalInfoFour;
		protected void BuildAddInfoColumns()
		{
			if (AdditionalLineInfo != null)
			{
				int currentAddInfo = 0;
				for (int i = 0; i < AdditionalLineInfo.Length; i++)
				{
					if (LineAdditionalInfoIsValid(AdditionalLineInfo[i].CY_Code))
					{
						switch (currentAddInfo)
						{
							case 0:
								fAdditionalInfoCodeOne = AdditionalLineInfo[i].CY_Code;
								fAdditionalInfoOne = AdditionalLineInfo[i].CY_Data;
								break;
							case 1:
								fAdditionalInfoCodeTwo = AdditionalLineInfo[i].CY_Code;
								fAdditionalInfoTwo = AdditionalLineInfo[i].CY_Data;
								break;
							case 2:
								fAdditionalInfoCodeThree = AdditionalLineInfo[i].CY_Code;
								fAdditionalInfoThree = AdditionalLineInfo[i].CY_Data;
								break;
							case 3:
								fAdditionalInfoCodeFour = AdditionalLineInfo[i].CY_Code;
								fAdditionalInfoFour = AdditionalLineInfo[i].CY_Data;
								break;
						}
					}
					currentAddInfo++;
				}
			}
		}

		protected bool LineAdditionalInfoIsValid(string cY_Code)
		{
			// TODO: ToBeChecked: Logic Changed with the removal of PurposeCode, Need re-implement accordingly #Victor 20160315
			return cY_Code != "ATV";
		}

		ICusEntryLine CusEntryLine
		{
			get { return (ICusEntryLine)WrappedObject; }
		}

		#endregion
	}
}
