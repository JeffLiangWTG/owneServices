namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator;

public interface ICustomScriptProvider
{
	string Get(IDataSetUpdaterInfo updaterInfo);
}
