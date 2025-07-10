using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IHarmonisedCode : IBusiness
	{
		ZString Country { get; set; }
		ZString Code { get; set; }
	}
}
