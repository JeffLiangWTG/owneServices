using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public interface IAddInfoGroupToOtherTableDataObjectReader
	{
		IColumnIndexer ReadIntoRow(ZGuid parentPK, ZString parentTableCode);
	}
}
