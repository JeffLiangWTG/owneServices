
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassContainerCollection : CFSContainerCollection
	{
		public GatePassContainerCollection(GatePassLoadListConsol consol, BusinessObjectFactory factory) : base(consol, factory)
		{
		}

		public new GatePassContainer this[int index]
		{
			get { return (GatePassContainer)Elements[index]; }
		}

		public new GatePassContainer AddNew()
		{
			return (GatePassContainer)base.AddNew();
		}
	}
}
