namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class VATParser : VATApplicabilitiesParentParser
	{
		public VATParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : base(dateTimeProvider, refDbServiceURI) { }

		protected override string applicabilityType => "VAT";

		protected override string dataSource => Constants.DataSources.VAT;

		protected override string measureTypeCode => MeasuresConstants.VAT.VATCode;

		protected override bool IsValidDutyToProcess(string duty) => duty != MeasuresConstants.VAT.Exemption && CheckDutyIsValid(duty);

		protected override bool CheckDutyIsValid(string duty)
		{
			var result = true;
			if (string.IsNullOrEmpty(duty) || !double.TryParse(duty.Replace("%", ""), out double _))
			{
				ErrorBuilder.AppendLine("Unable to import VAT Applicability as missing or invalid 'duty' for the current tariff code.");
				result = false;
			}
			return result;
		}
	}
}
