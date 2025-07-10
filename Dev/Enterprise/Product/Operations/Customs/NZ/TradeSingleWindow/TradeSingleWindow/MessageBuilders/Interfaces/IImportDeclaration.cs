
namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	using CargoWise.Types;
	using Enterprise.Integration;

	/// <summary>
	/// o	Details of goods seeking import clearance
	///	o	Submitted by Importers or their brokers
	/// </summary>
	public interface IImportDeclaration : IDeclaration
	{
		ZBool IsPeriodicImport { get; }
		ZBool IsMiscImporter { get; }
		IJobDocAddress MiscImporterAddress { get; }
		ZDateTime DateOfImport { get; }
		ZDateTime ImportPeriod { get; }
		IOrganisation Importer { get; }
		IGoodsShipment GoodsShipment { get; }
	}

	public interface IValuationAdjustment
	{
		ZString AdjustmentQualifier { get; }
		ZDecimal AdjustmentAmountInNZD { get; }
	}

	public interface IInvoice
	{
		ZString InvoiceNumber { get; }
		ZDateTime InvoiceDate { get; }
		ZString IncoTerms { get; }
	}

	public interface IOtherInfo
	{
		ZString Code { get; }
		ZString Data { get; }
	}
}
