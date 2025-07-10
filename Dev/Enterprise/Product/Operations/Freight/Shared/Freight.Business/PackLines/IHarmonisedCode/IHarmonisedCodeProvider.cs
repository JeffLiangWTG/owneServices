using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public interface IHarmonisedCodesProvider : IBusiness
	{
		IBusinessObjectCollection HarmonisedCodes { get; }
	}
}
