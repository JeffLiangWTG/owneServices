namespace Enterprise.Freight.Business
{
	public interface IScheduleChangeEmailSupporter
	{
		void AddOrUpdateJobScheduleChange(string dateType, string dataProvider);
		string GetDataProvider(string dateType);
	}
}
