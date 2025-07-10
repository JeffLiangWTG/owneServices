namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.UniversalDataBuss.DataObjects.Universal;
	using Enterprise.UniversalDataBuss.Integration;
	using Enterprise.UniversalDataBuss.Management;
	using UniversalShipment = UniversalDataBuss.DataObjects.Universal.Shipment;

	class AgencyShipmentContainerReferences : AgencyShipmentReferences
	{
		public AgencyShipmentContainerReferences(IXmlEventValueObjectContextValueList context, ZString containerNumber)
			: base(context)
		{
			ContainerNumber = containerNumber;
			ContainerISOCode = context.ContainerISOCode;
			ContainerReleaseNumber = context.ContainerReleaseNumber;
			GoodsItemID = context.GoodsItemID;
		}

		public AgencyShipmentContainerReferences(AgencyShipmentContainer agencyShipmentContainer)
			: base(agencyShipmentContainer.Booking)
		{
			if (!agencyShipmentContainer.IsTopLevelPack)
			{
				ContainerNumber = agencyShipmentContainer.JC_ContainerNum;
				ContainerReleaseNumber = agencyShipmentContainer.JC_ReleaseNum;
				if (agencyShipmentContainer.RefContainer != null)
				{
					ContainerISOCode = agencyShipmentContainer.RefContainer.ISOType.ISOCode;
				}
			}
			else
			{
				GoodsItemID = agencyShipmentContainer.JC_ContainerNum;
			}
		}

		public AgencyShipmentContainerReferences(UniversalShipment dataObject)
			: base(dataObject, ZGuid.Empty, ZString.Empty)
		{
		}

		public ZString ContainerISOCode { get; set; }
		public ZString ContainerReleaseNumber { get; set; }
		public ZString GoodsItemID { get; set; }

		#region Implementation

		public override List<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = base.GetEventContextValues();
			contextValues.AddIfNotEmpty(Event.ContextTypes.ContainerNumber, ContainerNumber);
			contextValues.AddIfNotEmpty(Event.ContextTypes.ContainerISOCode, ContainerISOCode);
			contextValues.AddIfNotEmpty(Event.ContextTypes.ContainerReleaseNumber, ContainerReleaseNumber);
			contextValues.AddIfNotEmpty(Event.ContextTypes.GoodsItemID, GoodsItemID);
			return contextValues;
		}

		#endregion
	}
}


