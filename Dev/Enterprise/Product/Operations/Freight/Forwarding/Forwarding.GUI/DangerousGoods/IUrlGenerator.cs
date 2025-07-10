using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.GUI.DangerousGoods
{
	public interface IUrlGenerator
	{
		string GenerateUrl(IBusiness businessEntity);
	}
}
