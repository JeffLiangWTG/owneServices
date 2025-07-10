using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class ContainerInformationForTest : IContainerInformation
	{
		public ContainerInformationForTest()
		{
		}

		public ContainerInformationForTest(string containerNum)
		{
			ContainerNumber = containerNum;
		}

		public ZString ContainerMode { get; set; }

		public ZString ContainerNumber { get; set; }

		public ZString FirstSealNumber { get; set; }

		public ZString SecondSealNumber { get; set; }
	}
}
