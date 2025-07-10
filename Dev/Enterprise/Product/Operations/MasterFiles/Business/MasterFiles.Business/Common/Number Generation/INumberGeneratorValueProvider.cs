namespace Enterprise.MasterFiles.Business
{
	public interface INumberGeneratorValueProvider
	{
		string Key { get; }
		string GetValue(NumberGenerator generator, string detail);
	}
}
