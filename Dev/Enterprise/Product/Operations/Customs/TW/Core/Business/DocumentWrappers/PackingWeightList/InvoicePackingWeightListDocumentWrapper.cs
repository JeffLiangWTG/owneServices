using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	[DefaultField("InvoiceNumber")]
	public class InvoicePackingWeightListDocumentWrapper : PackingWeightListDocumentWrapper
	{
		public InvoicePackingWeightListDocumentWrapper(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory) : base(invoiceHeader.JobDeclaration, factory)
		{
			Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
			InvoiceHeader = invoiceHeader;
			codeDescriptionPairList = TWRefCusCodeListTypes.GetCustomsPackUnitsList(Factory, Core.SharedConstants.Languages.English);
		}

		public JobComInvoiceHeader InvoiceHeader { get; }

		#region override

		protected override void SetPackingLines()
		{
			var jobDeclaration = InvoiceHeader.JobDeclaration;
			if (jobDeclaration != null)
			{
				var invoiceHeaderPK = InvoiceHeader.PK;
				foreach (var package in jobDeclaration.Packages.Cast<Package>().OrderBy(x => x.CW_MarksAndNos))
				{
					var invoiceLinesPivotCollection = package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().Where(x => x.InvoiceLine.InvoiceHeader.PK == invoiceHeaderPK).OrderBy(x => x.InvoiceLine.JI_LineNo);
					if (invoiceLinesPivotCollection.Any())
					{
						var packingLine = new PackingLine(package, Factory, invoiceLinesPivotCollection);
						PackingLines.Add(packingLine);
						if (packingLine.GroupPackingLines.Any())
						{
							PackingLines.AddRange(packingLine.GroupPackingLines);
						}
					}
				}
			}
		}

		protected override ZString MarksAndNumbersCore => InvoiceHeader.JZ_MarksAndNumbers;

		protected override ZString RemarksCore => InvoiceHeader.JZ_Remarks;
		#endregion

		#region Properties

		public ZString InvoiceNumber => InvoiceHeader.JZ_InvoiceNumber;

		public ZString PackageDescription
		{
			get
			{
				var packTypeSummary = PackingLines.PackTypeSummaryInfo.Where(info => !info.Key.IsEmpty);
				var typeCount = packTypeSummary.Count();
				if (typeCount == 0)
				{
					return ZString.Empty;
				}

				var result = ZString.Empty;
				var count = 0;
				foreach (var keyValuePair in packTypeSummary)
				{
					var code = keyValuePair.Key;
					var quantity = (int)keyValuePair.Value;
					var description = codeDescriptionPairList.GetDescriptionFromCode(code);
					if (description == null)
					{
						continue;
					}

					++count;
					if (count == 1)
					{
						result += (NoResString)"SAY TOTAL "; // Prefix of final result
					}
					if (count > 1 && count == typeCount)
					{
						result += " AND ";
					}
					else if (count > 1)
					{
						result += ", ";
					}

					description = description.ToUpper(CultureInfo.InvariantCulture);
					var quantityInEnglish = NumberToString_EN.ConvertNumberToWords(quantity).ToUpper(CultureInfo.InvariantCulture);
					if (quantity > 1)
					{
						description = Grammar.Instance.Pluralize(description).ToUpper(CultureInfo.InvariantCulture);
					}

					result += string.Format(CultureInfo.InvariantCulture, "{0} ({1}) {2}", quantityInEnglish, quantity, description);
				}

				if (!result.IsEmpty)
				{
					result += " ONLY.";
				}

				return result;
			}
		}

		public override ZString PackDate
		{
			get
			{
				var result = base.PackDate;
				if (result.IsEmpty)
				{
					result = InvoiceHeader.JZ_InvoiceDate.ToISO8601ShortDateString();
				}
				return result;
			}
		}

		#endregion

		readonly CodeDescriptionPairList codeDescriptionPairList;
	}
}
