namespace Enterprise.Warehouse.Transactions.Business
{
	interface IOutboundDockDoorTransferCreator
	{
		WhsTransferLine CreateOutboundDockDoorTransfer(WhsPickLine pickLine);
	}
}
