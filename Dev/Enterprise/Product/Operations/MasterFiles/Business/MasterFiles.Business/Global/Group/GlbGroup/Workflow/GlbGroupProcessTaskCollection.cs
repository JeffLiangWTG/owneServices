using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupProcessTaskCollection : ProcessTaskCollection
	{
		public GlbGroupProcessTaskCollection(GlbGroup glbGroup) : base(glbGroup)
		{
		}

		public GlbGroupProcessTaskCollection(GlbGroup glbGroup, ZQuery additionalFilter) : base(glbGroup, additionalFilter)
		{
		}

		public new GlbGroupProcessTask this[int index] => (GlbGroupProcessTask)Elements[index];

		public new GlbGroupProcessTask AddNew()
		{
			return (GlbGroupProcessTask)base.AddNew();
		}

		public new GlbGroup Parent => (GlbGroup)base.Parent;
	}
}
