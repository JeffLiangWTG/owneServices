using System.Linq;
using CargoWise.RefDbRepo.Client.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class DataSetUpdaterMapperTest : TestCase
	{
		public void TestGetDataSets()
		{
			var proxy = new Mock<IServerProxy>().Object;
			var logger = new Mock<ILogger>().Object;
			var config = new Mock<IClientConfiguration>().Object;
			var updaterMapper = new DataSetUpdaterMapper(proxy, logger, config);
			var dataSets = updaterMapper.GetDataSets();
			Assert(dataSets.Any() && dataSets.All(x => x.Name.StartsWith("REF-")));
		}
	}
}
