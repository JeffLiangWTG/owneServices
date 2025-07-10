using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CheckAlgorithm = Enterprise.NumberFountain.CheckDigitAlgorithm;
using Res = Enterprise.MasterFiles.Business.Res;

namespace Enterprise.Registry.Business
{
	public class InvoiceRemittanceCustomisationElementValidation
	{
		public InvoiceRemittanceCustomisationElementValidation(InvoiceRemittanceCustomisationElement parent)
		{
			Parent = parent;
		}

		readonly InvoiceRemittanceCustomisationElement Parent;

		public void ValidateOrder()
		{
			Parent.OrderInfo.ClearAllNotifications();
			if (Parent.Include)
			{
				if (Parent.Order == 0)
				{
					Parent.OrderInfo.AddError(Res.GetString("05039A78-E914-47A6-AF98-C393C648188B", "Order must be greater than 0."));
				}
				else if (Parent.ParentCollection.Cast<InvoiceRemittanceCustomisationElement>().Any(x => x.PK != Parent.PK && x.Order == Parent.Order))
				{
					Parent.OrderInfo.AddError(Res.GetString("7517D1FD-0438-468A-B588-E297F1AFA42D", "The Order already exists."));
				}
			}
		}

		public void ValidateInclude()
		{
			Parent.IncludeInfo.ClearAllNotifications();
			if (Parent.Include)
			{
				if (Parent.ElementName == InvoiceRemittanceCustomisationElement.ElementNames.BillerCode && string.IsNullOrWhiteSpace(Parent.ParentConfiguration.BillerCode))
				{
					Parent.IncludeInfo.AddError(Res.GetString("2F94178F-7A0C-43AC-A87D-5BFD7CECCF43", "This data element cannot be selected as a 'Biller Code' has not been entered."));
				}
				if (Parent.ElementName == InvoiceRemittanceCustomisationElement.ElementNames.BillerAccountNumber && string.IsNullOrWhiteSpace(Parent.ParentConfiguration.BillerAccountNumber))
				{
					Parent.IncludeInfo.AddError(Res.GetString("E68D37C6-B81D-445E-A8B3-A6B9AD414899", "This data element cannot be selected as a 'Biller Account Number' has not been entered."));
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void ValidateDigitCode()
		{
			Parent.DigitCodeInfo.ClearAllNotifications();
			if (Parent.Include)
			{
				if (Parent.IsCustomElement || Parent.IsInvoiceAmountElement)
				{
					MandatoryValidation.CheckEntered(Parent.DigitCodeInfo);
				}
				if (!Parent.DigitCodeInfo.HasErrors())
				{
					if (Parent.IsCustomElement && !Parent.CheckDigit.IsEmpty)
					{
						if (!Parent.DigitCode.IsNumbersOnlyOrEmpty && Parent.ParentCollection[Parent.CheckDigit].CheckDigitAlgorithm != CheckAlgorithm.MOD97)
						{
							Parent.DigitCodeInfo.AddError(Res.GetString("99EBD725-F080-447C-BEAA-EF3143AE971E", "Digit/Code should be numeric value with this check digit."));
						}
						else if (Parent.DigitCode.ToString().Any(x => char.IsSymbol(x)))
						{
							Parent.DigitCodeInfo.AddError(Res.GetString("DC409F8C-B9D4-479B-A988-FC7ED587B4BE", "Digit/Code with symbol cannot have check digit."));
						}
					}
					if (Parent.IsInvoiceAmountElement)
					{
						ZInt.TryParse(Parent.DigitCode, out ZInt digitCode);
						if (digitCode == 0)
						{
							Parent.DigitCodeInfo.AddError(Res.GetString("2BFA1868-18EA-48F8-B871-BDB1FB009741", "Digit/Code should be a number greater than 0."));
						}
					}
				}
			}
		}

		public void ValidateCheckDigit()
		{
			Parent.CheckDigitInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.CheckDigitInfo, Parent.CheckDigitList);
			if (!Parent.CheckDigitInfo.HasErrors())
			{
				if (!Parent.CheckDigit.IsEmpty && !Parent.ParentCollection[Parent.CheckDigit].Include)
				{
					Parent.CheckDigitInfo.AddError(Res.GetString("4B412DB4-E967-4659-B0BC-8B3767FD7FE5", "This check digit is not included."));
				}
				if (Parent.CheckDigit == Parent.ElementName)
				{
					Parent.CheckDigitInfo.AddError(Res.GetString("68610E71-16D5-4317-81E0-52782BDADFAF", "This check digit cannot select itself."));
				}
			}
		}

		public void ValidateCheckDigitAlgorithm()
		{
			Parent.CheckDigitAlgorithmInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.CheckDigitAlgorithmInfo, Parent.CheckDigitAlgorithmList);
			if (Parent.Include && Parent.IsCheckDigitElement)
			{
				MandatoryValidation.CheckEntered(Parent.CheckDigitAlgorithmInfo);
			}
		}
	}
}
