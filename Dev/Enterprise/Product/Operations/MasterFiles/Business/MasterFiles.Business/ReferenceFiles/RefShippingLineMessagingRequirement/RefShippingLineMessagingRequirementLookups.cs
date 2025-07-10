namespace Enterprise.MasterFiles.Business
{
	public class RefShippingLineMessagingRequirementLookups : AutoRefShippingLineMessagingRequirementLookups
	{
		public RefShippingLineMessagingRequirementLookups(AutoRefShippingLineMessagingRequirement parent) : base(parent)
		{
		}

		#region RefShippingLineMessagingRequirementsList

		public RefShippingLineMessagingRequirementTypeCollection RefShippingLineMessagingRequirementTypesList
		{
			get
			{
				return new RefShippingLineMessagingRequirementTypeCollection(Factory);
			}
		}

		#endregion
	}
}
