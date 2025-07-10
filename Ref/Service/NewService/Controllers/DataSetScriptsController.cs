using System.Linq;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.RemoteDbManager;
using CargoWise.RefDbRepo.Service.DataContractAdaptor;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewService.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class DataSetScriptsController : ControllerBase
	{
		public DataSetScriptsController(IUpdateScriptProvider updateScriptProvider)
		{
			this.updateScriptProvider = updateScriptProvider;
		}
		readonly IUpdateScriptProvider updateScriptProvider;

		[HttpGet]
		public AllDataSetScripts GetAllDataSetScripts([FromQuery] int version)
		{
			return new AllDataSetScripts
			{
				DataVersion = RemoteDbContractConverter.GetContractVersion(version),
				Scripts = updateScriptProvider.GetScriptInfos(version).Select(x =>
					new DataSetScripts
					{
						Prerequisites = x.Prerequisites,
						DataSetType = x.DataSetName,
						Mapping = x.Mapping,
						PrepareTemporaryTablesScripts = x.PrepareTemporaryTablesScripts,
						MergeScript = x.MergeScript,
						UpdaterVersion = x.UpdaterVersion
					}).ToArray()
			};
		}
	}
}
