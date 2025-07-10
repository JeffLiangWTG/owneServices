using System;
using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassContainerManyToManyCollection : CFSContainerManyToManyCollection
	{
		public GatePassContainerManyToManyCollection(GatePassPackLine parent) : base(parent)
		{
		}

		public new GatePassContainer this[int index]
		{
			get { return (GatePassContainer)(Elements[index]); }
		}

		public new GatePassContainer AddNew()
		{
			return (GatePassContainer)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(GatePassContainer);
		}
	}
}
