using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveUsersCollection : NonPersistentBusinessObjectCollection<ActiveUser>
	{
		public ActiveUsersCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, "Factory");
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ActiveUser(Factory);
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
