using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration.CustomValues
{
	public interface IGetCustomFieldStrategy
	{
		IZType GetCustomField(string fieldName, string type = null);
		string GetCustomFieldCodeDescription(string fieldName, string type = null);
	}
}