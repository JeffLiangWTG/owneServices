namespace Enterprise.Customs.Business
{
	public interface IAutoRateDSBOnSaved
	{
		void MarkNeedsAutoRateDSB();
		void ClearNeedsAutoRateDSB();

		void AutoRateDSBOnSavedIfNecessary();
	}
}
