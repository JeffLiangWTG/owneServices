namespace Enterprise.Warehouse.Transit.Business
{
	public interface ITWHJobMatcher
	{
		public TWHJobMatcherResult Process();

		public void SetNextJobMatcher(ITWHJobMatcher matcher);
	}
}
