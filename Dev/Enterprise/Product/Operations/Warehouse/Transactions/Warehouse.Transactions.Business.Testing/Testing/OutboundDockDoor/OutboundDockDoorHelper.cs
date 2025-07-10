using System;
using CargoWise.Application;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public static class OutboundDockDoorHelper
	{
		public static IDisposable MockOutboundDockDoorCreator() => ObjectFactory.Substitute<IOutboundDockDoorTransferCreator>(new OutboundDockDoorTransferCreatorMock());

		class OutboundDockDoorTransferCreatorMock : IOutboundDockDoorTransferCreator
		{
			WhsTransferLine IOutboundDockDoorTransferCreator.CreateOutboundDockDoorTransfer(WhsPickLine pickLine)
			{
				PickLinePassedIn = pickLine;
				return null;
			}

			public WhsPickLine PickLinePassedIn { get; private set; }
		}
	}
}
