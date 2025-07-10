using System.Collections.Generic;

namespace Enterprise.TransportBookings.Document.Testing
{
	sealed class TestContainerData
	{
		public string ContainerID { get; set; }
		public string ContainerType { get; set; }
		public List<TestPackageData> SubPackages { get; set; }
	}
}
