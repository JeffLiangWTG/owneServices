using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class SRDbDataSetUpdaterMapperTest : TestCase
	{
		public void TestGetDataSets()
		{
			var dataScript = new DataSetScripts
			{
				UpdaterVersion = 1,
				DataSetType = nameof(RefUNLOCO)
			};
			var allDataScripts = new AllDataSetScripts
			{
				DataVersion = "0_1_9",
				Scripts = new[] { dataScript }
			};
			var proxyMock = new Mock<IServerProxy>();
			proxyMock.Setup(x => x.GetAllDataSetScripts(It.IsAny<IHttpClient>(), 0))
				.Returns(Task.FromResult(allDataScripts));
			var logger = new Mock<ILogger>().Object;
			var config = new Mock<IClientConfiguration>().Object;
			var connection = Db.NewAdminConnection();
			var srdbUpdaterMapper = new SRDbDataSetUpdaterMapper(proxyMock.Object, logger, config, ((IDbConnectionInternals)connection).ADOConnection);
			var dataSets = srdbUpdaterMapper.GetDataSets();
			Assert(dataSets.Any() && dataSets.All(x => x.Name.StartsWith("RDU-")));
		}
	}
}
