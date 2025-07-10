namespace Enterprise.Freight.Agency.Business
{
	public class AgencyRORContainerValidation : AgencyTopLevelPackValidation
	{
		public AgencyRORContainerValidation(AgencyShipmentContainer container)
			: base(container)
		{
		}

		protected override void CheckJC_ContainerNum()
		{
			if (!Parent.JC_ContainerNum.IsEmpty)
			{
				if (Parent.JC_ContainerNum.Length > 17)
				{
					Parent.JC_ContainerNumInfo.AddError(Res.GetString("08588d44-36a0-4b12-bb73-c14a0f452501", "VIN/serial number can be no more than 17 characters long"));
				}

				if (Parent.JC_ContainerCount > 1)
				{
					Parent.JC_ContainerNumInfo.AddError(Res.GetString("c1d55fa1-c5e1-419e-86b9-bf34ab16b9a5", "You can't enter a VIN if the vehicle count is greater than one."));
				}

				CheckContainerNumberAgainstRelatedContainers();
			}
		}

		protected override string MessageForDuplicatedContainers
		{
			get { return Res.GetString("1512e09a-03f5-42cd-9a3f-c3d8921db889", "Duplicate Vehicle Number is entered."); }
		}
	}
}


