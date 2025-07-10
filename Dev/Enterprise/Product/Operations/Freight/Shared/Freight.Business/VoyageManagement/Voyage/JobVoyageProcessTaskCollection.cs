using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	class JobVoyageProcessTaskCollection : ProcessTaskCollection
	{
		public JobVoyageProcessTaskCollection(JobVoyage voyage)
			: base(voyage)
		{
		}

		public new JobVoyage Parent
		{
			get { return (JobVoyage)base.Parent; }
		}

		public new JobVoyageProcessTask this[int index]
		{
			get { return (JobVoyageProcessTask)Elements[index]; }
		}

		public new JobVoyageProcessTask AddNew()
		{
			return (JobVoyageProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new JobVoyageProcessTaskCollection(Parent);
		}
	}
}
