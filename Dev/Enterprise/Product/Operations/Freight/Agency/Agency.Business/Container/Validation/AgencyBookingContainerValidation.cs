namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingContainerValidation : AgencyShipmentContainerValidation
	{
		public AgencyBookingContainerValidation(AgencyShipmentContainer container)
			: base(container)
		{
		}

		#region JC_ContainerCount

		protected override void CheckJC_ContainerCount()
		{
			if (!Parent.JC_ContainerNum.IsEmpty)
			{
				base.CheckJC_ContainerCount();
			}
		}

		#endregion

		#region JC_ContainerNum

		protected override void CheckJC_ContainerNum()
		{
			if (!Parent.JC_ContainerNum.IsEmpty)
			{
				base.CheckJC_ContainerNum();
			}
		}

		#endregion
	}
}


