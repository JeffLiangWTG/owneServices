using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ISF.Business
{
	public interface IISFFromShipmentCreator
	{
		CusISFHeader Create(BusinessObjectFactory factory);
	}
}
