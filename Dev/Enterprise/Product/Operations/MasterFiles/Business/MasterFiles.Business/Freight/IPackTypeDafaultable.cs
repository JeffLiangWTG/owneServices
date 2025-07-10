
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IPackTypeDafaultable
	{
		ZDecimal Height { set; }
		ZDecimal Length { set; }
		ZDecimal Width { set; }
		ZDecimal Weight { set; }
		ZString UnitOfDimension { set; }
		ZString UnitOfWeight { set; }
	}
}
