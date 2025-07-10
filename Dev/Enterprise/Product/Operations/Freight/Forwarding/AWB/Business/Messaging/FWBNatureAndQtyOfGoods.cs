using System.Diagnostics;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	[DebuggerDisplay("{Text}")]
	abstract class FWBNatureAndQtyOfGoods
	{
		public abstract ZString Type { get; }
		public abstract ZString Text { get; }
	}
}
