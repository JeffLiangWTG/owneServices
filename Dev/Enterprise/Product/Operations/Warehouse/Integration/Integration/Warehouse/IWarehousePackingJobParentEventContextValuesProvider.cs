using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IWarehousePackingJobParentEventContextValueProvider
	{
		[SuppressMessage("Microsoft.Design", "CA1006")]
		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetAdditionalEventContextValues(IWhsOrder order);
	}
}
