using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface ICodeTypeCodeDescription : ICodeDescription
	{
		string CodeType { get; }
	}
}
