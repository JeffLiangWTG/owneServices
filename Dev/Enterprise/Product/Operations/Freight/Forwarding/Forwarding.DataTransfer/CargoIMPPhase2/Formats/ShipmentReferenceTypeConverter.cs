namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ShipmentReferenceTypeConverter : ListConverter<ShipmentReferenceTypes, ShipmentIdentifiersReferenceTypeList>
	{
		public override ShipmentIdentifiersReferenceTypeList Convert(ShipmentReferenceTypes data, FormattingResult formattingResult)
		{
			ShipmentIdentifiersReferenceTypeList result = null;
			switch (data)
			{
				case ShipmentReferenceTypes.JobNumber:
					result = ShipmentIdentifiersReferenceTypeList.JobNumber;
					break;
				case ShipmentReferenceTypes.MasterHouseBill:
					result = ShipmentIdentifiersReferenceTypeList.MasterHouseBill;
					break;
				case ShipmentReferenceTypes.UniqueConsignmentReference:
					result = ShipmentIdentifiersReferenceTypeList.UniqueConsignmentReference;
					break;
			}

			return result;
		}
	}
}
