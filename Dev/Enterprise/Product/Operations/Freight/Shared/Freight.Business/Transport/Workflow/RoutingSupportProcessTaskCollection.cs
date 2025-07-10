using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public abstract class RoutingSupportProcessTaskCollection : ProcessTaskCollection
	{
		protected RoutingSupportProcessTaskCollection(IRoutingSupport parent)
			: base((BusinessObject)parent)
		{
		}

		public override bool RequiresReferenceCode
		{
			get { return true; }
		}

		public override string ReferenceCodeCaption
		{
			get { return Res.GetString("Freight|RoutingSupportProcessTaskCollection|ReferenceCodeCaption", "Leg"); }
		}
	}
}
