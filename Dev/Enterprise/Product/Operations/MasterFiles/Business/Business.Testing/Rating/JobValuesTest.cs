using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobValuesTest : TestCaseWithFactory
	{
		public void TestContainerNumbers()
		{
			var containerInfo1 = CreateContainerInfo(containerNumber: "DFDF1111116");
			var containerInfo2 = CreateContainerInfo(containerNumber: "DFDF1212127");

			var jobValues = new MeasureValue(new[] { containerInfo1, containerInfo2 });
			List<ZString> containerNumbers = jobValues.ContainerNumbers();

			AssertEquals("Should be 2 elements in the list", 2, containerNumbers.Count);
			AssertEquals("DFDF1111116", containerNumbers[0]);
			AssertEquals("DFDF1212127", containerNumbers[1]);
		}

		public void TestGetContainerTeus()
		{
			var containerInfo1 = CreateContainerInfo(teu: 3);
			var containerInfo2 = CreateContainerInfo(teu: 4);

			var jobValues = new MeasureValue(new[] { containerInfo1, containerInfo2 });
			IList<ZDecimal> containerTeus = jobValues.GetContainerTeus();

			AssertEquals("Should be 2 elements in the list", 2, containerTeus.Count);
			AssertEquals(3m, containerTeus[0]);
			AssertEquals(4m, containerTeus[1]);
		}

		public void TestGetContainerTeusWhenContainersNull()
		{
			var containerInfo1 = CreateContainerInfo(teu: 3);
			var containerInfo2 = CreateContainerInfo(teu: 4);

			var jobValues = new MeasureValue((IEnumerable<MeasureInfo.ContainerInfo>)null);
			IList<ZDecimal> containerTeus = jobValues.GetContainerTeus();

			AssertEquals("Should be 0 elements in the list", 0, containerTeus.Count);
		}

		public void TestContainerCountInTeu()
		{
			var containerInfo1 = CreateContainerInfo(teu: 3);
			var containerInfo2 = CreateContainerInfo(teu: 4);
			var jobValues = new MeasureValue(new[] { containerInfo1, containerInfo2 });

			AssertEquals(7m, jobValues.Containers.Sum(x => x.TEU));
		}

		public void TestContainerCountInTeuWhenContainersNull()
		{
			var jobValues = new MeasureValue((IEnumerable<MeasureInfo.ContainerInfo>)null);

			AssertEquals(0m, jobValues.Containers.Sum(x => x.TEU));
		}

		public void TestGetContainerShipmentShares()
		{
			var containerInfo1 = CreateContainerInfo(shipmentShare: 0.6m);
			var containerInfo2 = CreateContainerInfo(shipmentShare: 0.4m);

			var jobValues = new MeasureValue(new[] { containerInfo1, containerInfo2 });
			IList<ZDecimal> containerShipmentShares = jobValues.GetContainerShipmentShares();

			AssertEquals("Should be 2 elements in the list", 2, containerShipmentShares.Count);
			AssertEquals(0.6m, containerShipmentShares[0]);
			AssertEquals(0.4m, containerShipmentShares[1]);
		}

		MeasureInfo.ContainerInfo CreateContainerInfo(int teu = 1, string containerNumber = "12345", decimal shipmentShare = 1m)
		{
			return new MeasureInfo.ContainerInfo(
				3m,
				Constants.Weight.Kilograms,
				3m,
				Constants.Volume.CubicMetres,
				3,
				teu,
				containerNumber,
				shipmentShare,
				"",
				1,
				ZGuid.Empty,
				null);
		}
	}
}
