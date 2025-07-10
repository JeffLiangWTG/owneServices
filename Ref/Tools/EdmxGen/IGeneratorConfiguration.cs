using System.Xml.Linq;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public interface IGeneratorConfiguration
	{
		string TempFilePath { get; }
		XDocument CurrentEDMX { get; }
		string Namespace { get; }
		string EntityContainer { get; }
		string ConnectionString { get; }
		string[] ExtraParameters { get; }
	}
}
