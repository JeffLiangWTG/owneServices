using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbExternalPasswordCollection : ActiveBusinessObjectCollection<GlbExternalPassword>
	{
		public GlbExternalPasswordCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbExternalPasswordCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
