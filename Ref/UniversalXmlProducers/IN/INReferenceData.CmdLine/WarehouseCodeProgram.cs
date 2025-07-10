using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.INReferenceData.CmdLine;

public static class WarehouseCodeProgram
{
	const string DataSource = "IN CUS Warehouse Code List";

	public static void Run()
	{
		var error = new WarehouseCodeXmlProducer(DataSource).ProduceXml();
		if (!error.IsNullOrEmpty())
		{
			Logger.Log(LogType.ReviewRequired, error);
		}
	}

	static Logger Logger => logger ??= new Logger(new DateTimeProvider());
	static Logger logger;
}
