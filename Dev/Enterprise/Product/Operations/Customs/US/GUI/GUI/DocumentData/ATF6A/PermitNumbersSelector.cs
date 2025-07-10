using System.Linq;
using System.Windows.Forms;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.GUI
{
	internal class PermitNumbersSelector : IATF6AFormPermitNumbersSelector
	{
		public Either<string, ZString[]> SelectPermitNumbers(object obj)
		{
			if (obj is JobDeclaration declaration)
			{
				var permitNumbers = declaration.Invoices
					.SelectMany(x => x.InvoiceLines).OfType<JobComInvoiceLine>()
					.SelectMany(x => x.ATFLines).OfType<ATF>()
					.Select(x => x.US_PermitNumber)
					.Distinct().OrderBy(x => x).ToArray();

				if (permitNumbers == null
					|| permitNumbers.Length == 0
					|| permitNumbers.Length == 1)
				{
					return permitNumbers;
				}

				var collection = new PermitNumbersToSelectFromForPrintingCollection();
				foreach (var permitNumber in permitNumbers)
				{
					var permitNumberToSelectFromForPrinting = new PermitNumberToSelectFromForPrinting(permitNumber);
					collection.Add(permitNumberToSelectFromForPrinting);
				}

				using (var form = new PermitNumbersSelectorForm(collection))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.Yes)
					{
						return collection.SelectedPermitNumbers.ToArray();
					}

					return string.Empty;
				}
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
