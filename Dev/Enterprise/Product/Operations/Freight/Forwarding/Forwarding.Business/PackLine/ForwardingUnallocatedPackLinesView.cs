using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingUnallocatedPackLinesView : ConsolUnAllocatedPackLinesView
	{
		public ForwardingUnallocatedPackLinesView(ForwardingConsol consol, PackLineNonDependentCollection packLines)
			: base(consol, packLines)
		{
			this.consol = consol;
		}

		public new ForwardingPackLine this[int index]
		{
			get { return (ForwardingPackLine)base[index]; }
		}

		public new ForwardingPackLine AddNew()
		{
			return (ForwardingPackLine)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ForwardingPackLine);
		}
	}
}
