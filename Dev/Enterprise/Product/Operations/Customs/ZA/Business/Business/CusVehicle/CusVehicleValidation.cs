using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business;

public class CusVehicleValidation : Customs.Business.CusVehicleValidation
{
	public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
	{
	}

	public new CusVehicle Parent => (CusVehicle)base.Parent;
}
