namespace CargoWise.eHub.DataModel.Common
{
	public class ContextFactory<T> where T : ContextBase, new()
	{
		public virtual T CreateContext()
		{
			return new T();
		}
	}
}
