using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IChargesCollection : IReadOnlyCollection<IChargeLine>
	{
		ZBool Hide { get; }
		ZBool ShowAsLumpSum { get; }
		ZBool ShowAsAgreed { get; }
		ZBool ShowPrepaid { get; }
		ZBool ShowCollect { get; }
		IMoney LumpSum { get; }
		IReadOnlyCollection<IChargeLine> All { get; }
	}
}
