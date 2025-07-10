using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PermitValidator
	{
		public PermitValidator(ZString permitIndicator, JobComInvoiceLine invoiceLine)
		{
			this.permitIndicator = permitIndicator;
			this.InvoiceLine = invoiceLine;
		}
		readonly ZString permitIndicator;

		protected JobComInvoiceLine InvoiceLine { get; }

		public ZString GetErrorTextForRequirement(ZString permit)
		{
			string result = "";
			if (permit.IsEmpty && IsPermitNoRequired(InvoiceLine.ImportEntryType))
			{
				result = $"The {PermitDescription} number is required.";

				var extraMsg = ExtraRequirementErrorMessage;
				if (!extraMsg.IsEmpty)
				{
					result += " " + extraMsg;
				}
			}
			return result;
		}

		public virtual ZString GetErrorTextForPermitNotRequired()
		{
			return ZString.Empty;
		}

		public ZString GetErrorTextIfInvalidFormatOrNotRequired(ZString permit)
		{
			var result = ZString.Empty;

			if (DoFormatValidation && !IsPermitFormatValid(permit))
			{
				result = PermitValidatorHelper.GetPermitFormatErrorMessage(InvoiceLine.Factory, permitIndicator);
			}
			else
			{
				result = GetErrorTextForExtraCondition(permit);
			}

			return result;
		}

		public ZString GetWarningText(ZString permit)
		{
			return GetWarningTextCore(permit);
		}

		protected virtual ZString ExtraRequirementErrorMessage => ZString.Empty;

		protected virtual ZString GetErrorTextForExtraCondition(ZString permit) => ZString.Empty;

		protected virtual ZString GetWarningTextCore(ZString permit) => ZString.Empty;

		protected virtual bool IsPermitNoRequired(ZString entryType) => MandatoryForEntry(entryType);

		protected virtual bool MandatoryForEntry(string entryType) => !EntryTypeList.IsWarehouseOrTIB(entryType);

		public virtual bool IsPermitNoNotRequired() => false;

		protected string PermitDescription => LicencePermitTypeList.GetLicencePermitTypeList(InvoiceLine.Factory).GetDescriptionFromCode(permitIndicator);

		protected ZString RegexPattern => PermitValidatorHelper.GetPermitFormatMask(InvoiceLine.Factory, permitIndicator);

		bool DoFormatValidation => !RegexPattern.IsEmpty;

		bool IsPermitFormatValid(string permit) => Regex.IsMatch(permit, $@"^{RegexPattern}$", RegexOptions.IgnoreCase);

		public virtual void ValidateForExtraCondition(ZPropertyInfo propertyInfo)
		{
		}
	}
}
