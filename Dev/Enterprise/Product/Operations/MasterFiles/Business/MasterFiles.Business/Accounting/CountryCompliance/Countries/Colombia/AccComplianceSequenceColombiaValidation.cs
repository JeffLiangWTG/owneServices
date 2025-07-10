using System.Text.RegularExpressions;

namespace Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Countries.Colombia
{
	public class AccComplianceSequenceColombiaValidation : AccComplianceSequenceValidation
	{
		public AccComplianceSequenceColombiaValidation(AutoAccComplianceSequence parent) : base(parent)
		{
		}

		protected override void CheckXD_PrintingAuthorizationNumber()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value &&
				!Regex.IsMatch(Parent.XD_PrintingAuthorizationNumber, "^[^-]+-[^-]+$"))
			{
				Parent.XD_PrintingAuthorizationNumberInfo.AddWarning(Res.GetString("8D93C154-A3FA-4476-9D9C-8235C13E2432",
					@"Format is invalid. The 'Resolution Number' and the 'Technical Key' assigned by DIAN must be separated by a hyphen."));
			}
		}
	}
}
