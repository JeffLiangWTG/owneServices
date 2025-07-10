using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	public interface IBooking
	{
		ZBool Send { get; }
		ZString BookingNumber { get; }
		IReadOnlyCollection<IContainer> Containers { get; }
	}
}
