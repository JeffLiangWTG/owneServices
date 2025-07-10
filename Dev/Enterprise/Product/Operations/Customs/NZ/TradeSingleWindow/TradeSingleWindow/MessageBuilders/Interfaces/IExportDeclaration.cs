
namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	using CargoWise.Types;

	/// <summary>
	/// o	Details of goods seeking export clearance
	///	o	Submitted by Exporters or their brokers
	/// </summary>
	public interface IExportDeclaration : IDeclaration
	{
		IOrganisation Exporter { get; }
		ZDateTime DateOfExport { get; }
		IGoodsShipment GoodsShipment { get; }
		IOrganisation Importer { get; }
	}
}
