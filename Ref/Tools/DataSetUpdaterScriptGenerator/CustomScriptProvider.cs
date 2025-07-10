namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator;

class CustomScriptProvider : ICustomScriptProvider
{
	public string Get(IDataSetUpdaterInfo updaterInfo)
	{
		var storageType = updaterInfo.GetStorageType();
		var checkRoundingScript = CheckRoundingScriptFactory.Create(storageType);
		return checkRoundingScript.GetScript();
	}
}
