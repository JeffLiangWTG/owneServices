namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public abstract class BaseParser
	{
		protected BaseParser(string dataSource)
		{
			this.dataSource = dataSource;
		}
		protected string dataSource { get; private set; }
	}
}
