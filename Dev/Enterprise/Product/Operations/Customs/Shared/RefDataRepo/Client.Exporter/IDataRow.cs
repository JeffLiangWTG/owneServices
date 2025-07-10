namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public interface IDataRow
	{
		object this[string propertyName] { get; }
	}
}
