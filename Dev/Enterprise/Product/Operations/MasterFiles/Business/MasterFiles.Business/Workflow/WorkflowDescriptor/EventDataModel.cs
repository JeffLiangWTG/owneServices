
namespace Enterprise.MasterFiles.Business
{
	using CargoWise.Common;

	public abstract class EventDataModel<T>
	{
		public EventDataModel(T parent)
		{
			Parent = Argument.NotNull(parent, "parent");
		}

#if DEBUG
		public T Parent_DebugOnly => Parent;
#endif

		protected T Parent
		{
			get;
			private set;
		}
	}
}
