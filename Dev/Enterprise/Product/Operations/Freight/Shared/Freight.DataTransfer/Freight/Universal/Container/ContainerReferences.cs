using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	class ContainerReferences : AdditionalReferencesParent
	{
		public ContainerReferences(CommonContainer container, bool shouldAddParentContextValues = true)
			: base()
		{
			ContainerNumber = container.JC_ContainerNum;
			ContainerReleaseNumber = container.JC_ReleaseNum;

			if (container.RefContainer != null)
			{
				ContainerISOCode = container.RefContainer.ISOType.ISOCode;
			}

			var containerParent = container.ContainerParent as BusinessObject;

			if (containerParent != null && shouldAddParentContextValues)
			{
				var parentDataSourceManager = (IEventDataContextManager)containerParent.GetUniversalDataContextManager();
				if (parentDataSourceManager != null)
				{
					containerParentEventContextValues = parentDataSourceManager.EventContextValues;
				}
			}
		}

		public ContainerReferences(UniversalShipment dataObject)
			: base()
		{
		}

		public ZString ContainerNumber { get; set; }
		public ZString ContainerISOCode { get; set; }
		public ZString ContainerReleaseNumber { get; set; }

		readonly IEnumerable<KeyValuePair<TypeWithDescription, IZType>> containerParentEventContextValues;

		#region Implementation

		public List<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (containerParentEventContextValues != null)
			{
				contextValues.AddRange(containerParentEventContextValues);
			}

			contextValues.AddIfNotEmpty(Event.ContextTypes.ContainerNumber, ContainerNumber);
			contextValues.AddIfNotEmpty(Event.ContextTypes.ContainerISOCode, ContainerISOCode);
			contextValues.AddIfNotEmpty(Event.ContextTypes.ContainerReleaseNumber, ContainerReleaseNumber);
			return contextValues;
		}

		#endregion
	}
}
