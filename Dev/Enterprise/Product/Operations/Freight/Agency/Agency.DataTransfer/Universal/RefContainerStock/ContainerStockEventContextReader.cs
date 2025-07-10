using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class ContainerStockEventContextReader
	{
		public ContainerStockEventContextReader(RefContainerStock container)
		{
			this.container = Argument.NotNull(container, "RefContainerStock container");
		}

		readonly RefContainerStock container;

		public void AddContainerContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ContainerNumber, container.R6_ContainerNum);
		}
	}
}
