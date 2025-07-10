using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IBox29Supportable
	{
		ZString US_Box29Text { get; set; }
		ZPropertyInfo US_Box29TextInfo { get; }
		ZBool US_Box29IncludeContainers { get; set; }
		ZPropertyInfo US_Box29IncludeContainersInfo { get; }
	}
}
