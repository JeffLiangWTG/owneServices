using CargoWise.EntityFramework;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection : NonPersistentBusinessObjectCollection<HVLVConsignmentForStandAloneDeclarationConversionWrapper>
	{
		public HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection(HVLVShipmentConsignmentCollection consignments) : base()
		{
			foreach (HVLVConsignment consignment in consignments)
			{
				if (ConsignmentCanBeConverted(consignment))
				{
					Add(new HVLVConsignmentForStandAloneDeclarationConversionWrapper(consignment));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HVLVConsignmentForStandAloneDeclarationConversionWrapper();
		}

		bool ConsignmentCanBeConverted(HVLVConsignment consignment)
		{
			return consignment.CanConvertToStandAloneDeclaration;
		}
	}
}
