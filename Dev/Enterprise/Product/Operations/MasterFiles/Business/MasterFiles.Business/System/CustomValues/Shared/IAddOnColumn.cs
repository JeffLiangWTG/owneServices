using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public interface IAddOnColumn
	{
		BusinessObject Parent { get; set; }
	}
}
