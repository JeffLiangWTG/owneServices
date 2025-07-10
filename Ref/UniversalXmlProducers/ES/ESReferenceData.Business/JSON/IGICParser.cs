namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class IGICParser : VATApplicabilitiesParentParser
	{
		public IGICParser(IDateTimeProvider dateTimeProvider, string refDbServiceURI) : base(dateTimeProvider, refDbServiceURI) { }

		protected override string applicabilityType => "IGIC";

		protected override string dataSource => Constants.DataSources.IGIC;

		protected override string measureTypeCode => MeasuresConstants.IGIC.IGICCode;

		protected override bool IsValidDutyToProcess(string duty) => CheckDutyIsValid(duty);

		protected override bool CheckDutyIsValid(string duty)
		{
			var result = true;
			if (string.IsNullOrEmpty(duty) || !double.TryParse(duty.Replace("%", ""), out double _))
			{
				ErrorBuilder.AppendLine("Unable to import IGIC Applicability as missing or invalid 'duty' for the current tariff code.");
				result = false;
			}
			return result;
		}
	}
}
