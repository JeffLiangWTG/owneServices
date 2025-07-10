using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CargoManifestQuerySendingObjectCollection : NonPersistentBusinessObjectCollection<CargoManifestQuerySendingObject>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CargoManifestQuerySendingObjectCollection(CargoManifestStatusQueryHeaderObject master)
			: base(master.Factory)
		{
			this.master = master;
			PopulateObjects(ZString.Empty);
		}
		readonly CargoManifestStatusQueryHeaderObject master;

		public void PopulateObjects(ZString actionCode, IAutoQueryFilter filter = null)
		{
			RemoveAndDeleteAll();

			foreach (ICargoManifestStatusQueryData objectForQuery in master.Header.ObjectsForQuery)
			{
				if (objectForQuery.IsRelevantFor(actionCode) && (filter?.Filter(objectForQuery) ?? true))
				{
					var obj = new CargoManifestQuerySendingObject(master, objectForQuery);
					Add(obj);
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
