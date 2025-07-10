using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.Utils;

public interface IColumnMetaDataProvider
{
	List<ColumnMetaData> GetMetaData(string objectName);
}
