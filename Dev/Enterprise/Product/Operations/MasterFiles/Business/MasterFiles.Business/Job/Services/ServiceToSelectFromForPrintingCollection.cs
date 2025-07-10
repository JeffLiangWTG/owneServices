using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ServiceToSelectFromForPrintingCollection : NonPersistentBusinessObjectCollection<ServiceToSelectFromForPrinting>
	{
		public ServiceToSelectFromForPrintingCollection(JobServiceDependentCollection collection) : base(collection.Factory)
		{
			foreach (JobService service in collection)
			{
				Add(new ServiceToSelectFromForPrinting(service));
			}
			fCollection = collection;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		readonly JobServiceDependentCollection fCollection;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ServiceToSelectFromForPrinting(fCollection.AddNew());
		}
	}
}
