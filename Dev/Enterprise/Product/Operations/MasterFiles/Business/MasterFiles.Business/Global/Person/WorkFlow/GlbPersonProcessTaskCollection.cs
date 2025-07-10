using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonProcessTaskCollection : ProcessTaskCollection
	{
		public GlbPersonProcessTaskCollection(GlbPerson glbPerson)
			: base(glbPerson)
		{
		}

		public GlbPersonProcessTaskCollection(GlbPerson glbPerson, ZQuery additionalFilter)
			: base(glbPerson, additionalFilter)
		{
		}

		public new GlbPersonProcessTask this[int index]
		{
			get { return (GlbPersonProcessTask)Elements[index]; }
		}

		public new GlbPersonProcessTask AddNew()
		{
			return (GlbPersonProcessTask)base.AddNew();
		}

		public new GlbPerson Parent
		{
			get { return (GlbPerson)base.Parent; }
		}
	}
}
