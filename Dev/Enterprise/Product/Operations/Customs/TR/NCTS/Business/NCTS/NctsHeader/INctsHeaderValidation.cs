namespace Enterprise.Customs.TR.NCTS.Business
{
	public interface INctsHeaderValidation
	{
		public NctsHeader Parent { get; }

		void ValidateStampDutyStatus();

		void ValidateStampDuty();
	}
}
