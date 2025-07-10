using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingBookingContainerValidation : AgencyShipmentContainerValidation
	{
		public BillOfLadingBookingContainerValidation(BillOfLadingContainer container)
			: base(container) { }

		protected override void CheckJC_ContainerNum()
		{
			base.CheckJC_ContainerNum();

			if (Parent.JC_Purpose == ContainerBookedStatus.Codes.Real)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JC_ContainerNumInfo);
			}
		}

		protected override void CheckJC_IsEmptyContainer()
		{
			base.CheckJC_IsEmptyContainer();

			if (!Parent.JC_IsEmptyContainer && Parent.JC_Purpose == ContainerBookedStatus.Codes.Real && Parent.PackLines.Count == 0)
			{
				Parent.JC_IsEmptyContainerInfo.AddMessageError(notEmptyContainerHasNoPacklines);
			}
		}
	}
}


