using CargoWise.eHub.DataModel.Business;

namespace CargoWise.eHub.DataModel.Tests.Business.TestFiles
{
	[ClientSystemRegistrationStatus("Sample1")]
	public enum ClientSystemRegistrationCodeAndDescriptionStatusSample1
	{
		Invalid = 0,
		Valid = 1,
		Unknown = 2,
		Disabled = 255
	}
}
