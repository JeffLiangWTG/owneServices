using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public class InvoiceLineFilterBusinessObject : FormFilterBuisnessObject<BaseJobComInvoiceLine>
	{
		public InvoiceLineFilterBusinessObject(Func<IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable) : base(ModuleIDs.Customs.InvoiceLine)
		{
			this.getInvoicesProvider = getInvoicesProvider;
			IsColumnAvailable = isColumnAvailable;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new InvoiceLineFilterBusinessObject(getInvoicesProvider, IsColumnAvailable);

		public sealed override SchemaGuidColumn PKSchemaColumn
		{
			get { return JobComInvoiceLineSchema.PK; }
		}

		readonly Func<IInvoicesProvider> getInvoicesProvider;

		protected Func<ZString, ZBool> IsColumnAvailable { get; }

		protected IInvoicesProvider InvoicesProvider => getInvoicesProvider?.Invoke();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void AddOrUpdateFunc(ModuleFilterCollection filters)
		{
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.InvoiceNumber, x => x.InvoiceNumber, ResString.GetMultilingualString("D1DAA2E9-CDDE-4B54-AFB9-471236FAEEE4", InvoiceLineFilterConstants.InvoiceNumber));

			var invoiceSeqNoFilter = new ModuleNumberRangeFilter(InvoiceLineFilterConstants.InvoiceSequenceNumber,
			(value1, value2) => new ZQuery(),
			() => 1M,
				() =>
				{
					var maxValue = InvoicesProvider?.Invoices.Count() ?? decimal.MaxValue;
					return maxValue > 0 ? maxValue : decimal.MaxValue;
				})
			{
				ShowUpAndDownArrows = true,
				Decimals = 0
			};
			invoiceSeqNoFilter.UpArrowClicked += (sender, e) => Search();
			invoiceSeqNoFilter.DownArrowClicked += (sender, e) => Search();
			invoiceSeqNoFilter.SetDefaultValue(1M);
			invoiceSeqNoFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			invoiceSeqNoFilter.MultilingualDescription = ResString.GetMultilingualString("0E312423-A9B4-48A7-B240-DBDA01AB0AC6", InvoiceLineFilterConstants.InvoiceSequenceNumber);
			AddCustomFilterFunc(filters, invoiceSeqNoFilter, (filter, invoiceLine) => GetNumberRangeFunc(filter, invoiceLine, x => x.InvoiceHeader == null ? new ZDecimal?() : ZDecimal.Parse(x.InvoiceHeader.JZ_InvoiceDisplaySequence.ToString())));

			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.ProductCode, x => x.JI_PartNo, ResString.GetMultilingualString("945963A9-093A-4F38-A568-60A6692A6659", InvoiceLineFilterConstants.ProductCode));
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.LookupCode, x => x.Classification?.CC_LookupCode ?? ZString.Empty, ResString.GetMultilingualString("4CEFB600-0865-41A0-BB9D-80132581FA28", InvoiceLineFilterConstants.LookupCode));
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.Tariff, x => x.JI_Tariff, ResString.GetMultilingualString("349891D1-E402-4AE8-BDD3-557D3D8B8E17", InvoiceLineFilterConstants.Tariff));
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.GoodsDescription, x => x.JI_Description, ResString.GetMultilingualString("1E680693-D67A-4470-8B27-391F36B088DA", InvoiceLineFilterConstants.GoodsDescription));

			var mergeLineNumberFilter = new ModuleNumberRangeFilter(InvoiceLineFilterConstants.MergeLineNumber, (value1, value2) => new ZQuery())
			{
				Decimals = 0
			};
			mergeLineNumberFilter.MultilingualDescription = ResString.GetMultilingualString("857C7F10-F54B-496C-8EA1-3873E473815B", InvoiceLineFilterConstants.MergeLineNumber);
			AddCustomFilterFunc(filters, mergeLineNumberFilter, (filter, invoiceLine) => GetNumberRangeFunc(filter, invoiceLine, x => x.CusEntryLine == null ? new ZDecimal?() : ZDecimal.Parse(x.CusEntryLine.CL_LineNumber.ToString())));

			AddTranslatableNumberRangeFunc(filters, InvoiceLineFilterConstants.InvoiceQuantity, x => x.JI_InvoiceQuantity, ResString.GetMultilingualString("F4EC1236-4B11-4153-8626-F09C2D2FFFC0", InvoiceLineFilterConstants.InvoiceQuantity));
			AddTranslatableNumberRangeFunc(filters, InvoiceLineFilterConstants.LinePrice, x => x.JI_LinePrice, ResString.GetMultilingualString("044B52C4-E480-4199-8C1A-2202762AAB83", InvoiceLineFilterConstants.LinePrice));
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.LineCurrency, x => x.JI_RX_NKLinePriceCurr, ResString.GetMultilingualString("269FAA15-202E-402E-8C88-4435ABAA149A", InvoiceLineFilterConstants.LineCurrency));
			AddTranslatableNumberRangeFunc(filters, InvoiceLineFilterConstants.Weight, x => x.JI_Weight, ResString.GetMultilingualString("14FCC4C1-F814-422A-B785-1B94795C5DD2", InvoiceLineFilterConstants.Weight));
			AddTranslatableNumberRangeFunc(filters, InvoiceLineFilterConstants.Volume, x => x.JI_Volume, ResString.GetMultilingualString("0C04C3A2-CBEA-4589-A3E4-DA577FFC7E35", InvoiceLineFilterConstants.Volume));
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.ContainerNumber, GetContainerNumberFilterFunc, ResString.GetMultilingualString("9FBDFBF2-4840-4C01-BBF9-40E3A68B15E3", InvoiceLineFilterConstants.ContainerNumber));
			AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.Origin, x => x.JI_CountryOfOrigin, ResString.GetMultilingualString("3245F720-DEBB-4EAE-8F19-984F14F774D1", InvoiceLineFilterConstants.Origin));
			AddTranslatableTextFuncIfFieldAvailable(filters, InvoiceLineFilterConstants.Brand, JobComInvoiceLineSchema.Constants.JI_BrandName, x => x.JI_BrandName, ResString.GetMultilingualString("443570C0-6589-4A11-BA0A-E2D7B3C1E2C4", InvoiceLineFilterConstants.Brand));
			AddTranslatableTextFuncIfFieldAvailable(filters, InvoiceLineFilterConstants.Model, JobComInvoiceLineSchema.Constants.JI_Model, x => x.JI_Model, ResString.GetMultilingualString("85A1A4A8-CFC4-4595-A733-88172426C19B", InvoiceLineFilterConstants.Model));
			AddInvoiceLineNumberFilter(filters);
		}

		void AddInvoiceLineNumberFilter(ModuleFilterCollection filters)
		{
			var invoiceLineNoFilter = new ModuleNumberRangeFilter(InvoiceLineFilterConstants.InvoiceLineNumber,
			(value1, value2) => new ZQuery(),
			() => 1M,
			() =>
			{
				var maxValue = InvoicesProvider?.Invoices.Cast<BaseJobComInvoiceHeader>().Select(x => x.InvoiceLines.Count).DefaultIfEmpty().Max() ?? short.MaxValue;
				return maxValue > 0 ? maxValue : short.MaxValue;
			})
			{
				ShowUpAndDownArrows = true,
				Decimals = 0
			};

			invoiceLineNoFilter.UpArrowClicked += (sender, e) => Search();
			invoiceLineNoFilter.DownArrowClicked += (sender, e) => Search();
			invoiceLineNoFilter.SetDefaultValue(1M);
			invoiceLineNoFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			invoiceLineNoFilter.MultilingualDescription = ResString.GetMultilingualString("095C682E-29BA-4474-BBF2-7E083C458BFB", InvoiceLineFilterConstants.InvoiceLineNumber);
			AddCustomFilterFunc(filters, invoiceLineNoFilter, (filter, invoiceLine) => GetNumberRangeFunc(filter, invoiceLine, x => ZDecimal.Parse(x.JI_LineNo.ToString())));
		}

		protected void AddTranslatableTextFuncIfFieldAvailable(ModuleFilterCollection filters, ZString description, ZString columnName, Func<BaseJobComInvoiceLine, ZString> textSelector, MultilingualString multilingualDescription)
		{
			if (IsColumnAvailable != null && IsColumnAvailable(columnName))
			{
				AddTranslatableTextFunc(filters, description, textSelector, multilingualDescription);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool GetContainerNumberFilterFunc(ModuleFilter moduleFilter, BaseJobComInvoiceLine jobComInvoiceLine)
		{
			var result = true;
			var containerNumber = (ModuleTextFilter)moduleFilter;
			var comparisonoperator = containerNumber.SqlComparisonOperator;
			var containers = jobComInvoiceLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>();
			var filterProperty = containerNumber.Property;
			if (comparisonoperator == SQLComparisonOperator.Equal)
			{
				result = filterProperty.IsEmpty ||
						 containers.Any(y => y.ContainerNumber.EqualsIgnoringCase(filterProperty));
			}
			else if (comparisonoperator == SQLComparisonOperator.StartsWith)
			{
				result = filterProperty.IsEmpty ||
						 containers.Any(
							 y => y.ContainerNumber.StartsWith(filterProperty, StringComparison.CurrentCultureIgnoreCase));
			}
			else if (comparisonoperator == SQLComparisonOperator.Contains)
			{
				result = filterProperty.IsEmpty ||
						 containers.Any(y => y.ContainerNumber.Contains(filterProperty, StringComparison.CurrentCultureIgnoreCase));
			}
			else if (comparisonoperator == SQLComparisonOperator.NotEqual)
			{
				result = filterProperty.IsEmpty ||
						 containers.Any(y => !y.ContainerNumber.EqualsIgnoringCase(filterProperty));
			}
			else if (comparisonoperator == SQLComparisonOperator.DoesNotStartWith)
			{
				result = filterProperty.IsEmpty ||
						 containers.Any(
							 y => !y.ContainerNumber.StartsWith(filterProperty, StringComparison.CurrentCultureIgnoreCase));
			}
			else if (comparisonoperator == SQLComparisonOperator.NotContains)
			{
				result = filterProperty.IsEmpty ||
						 containers.Any(y => !y.ContainerNumber.Contains(filterProperty, StringComparison.CurrentCultureIgnoreCase));
			}
			else if (comparisonoperator == SpecialComparisonOperator.IsBlank)
			{
				result = !containers.Any();
			}
			else if (comparisonoperator == SpecialComparisonOperator.IsNotBlank)
			{
				result = containers.Any(y => !y.ContainerNumber.IsEmpty);
			}
			return result;
		}

		public void Search()
		{
			InvoicesProvider?.FilteredInvoiceLines.LoadFilteredLines(Predicate);
		}
	}
}
