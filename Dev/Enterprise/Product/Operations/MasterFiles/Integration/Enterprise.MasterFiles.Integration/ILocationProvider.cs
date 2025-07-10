using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface ILocationProvider
	{
		ZQuery GetLocationQuery(LocationTypeEnum locationType, string codeOrDescription);
	}

	public enum LocationTypeEnum
	{
		Port,
		InternationalZone,
		Country
	}
}
