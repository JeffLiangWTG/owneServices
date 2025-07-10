using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class DocumentServices : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentServices(IHaveServices parent, ServiceToSelectFromForPrintingCollection serviceToSelectFrom) : base(serviceToSelectFrom.Factory)
		{
			fServiceToSelectFrom = serviceToSelectFrom;
			fParent = parent;
		}

		public static new string TableName
		{
			get { return "JobService"; }
		}

		protected ServiceToSelectFromForPrintingCollection fServiceToSelectFrom;
		protected IHaveServices fParent;

		public ServiceToSelectFromForPrintingCollection ServiceToSelectFrom
		{
			get { return fServiceToSelectFrom; }
		}

		public IHaveServices Parent
		{
			get { return fParent; }
		}

		public JobServiceDependentCollection ServicesToPrint
		{
			get
			{
				JobServiceDependentCollection result = new JobServiceDependentCollection((BusinessObject)fParent, Factory);

				foreach (ServiceToSelectFromForPrinting selectService in ServiceToSelectFrom)
				{
					if (selectService.ES_Calc_PrintDocumentForService)
					{
						JobService serviceToPrint = (JobService)Factory.Load(typeof(JobService), selectService.Service.PK);
						result.Add(serviceToPrint);
					}
				}

				return result;
			}
		}
	}
}
