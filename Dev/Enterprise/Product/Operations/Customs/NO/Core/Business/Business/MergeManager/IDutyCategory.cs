namespace Enterprise.Customs.NO.Business
{
	public interface IDutyCategory
	{
		bool IsCustomsDuty { get; }
		bool IsAgriculturalDuty { get; }
		bool IsExciseDuty { get; }
		bool IsVAT { get; }

		string DutyCode { get; }
	}
}
