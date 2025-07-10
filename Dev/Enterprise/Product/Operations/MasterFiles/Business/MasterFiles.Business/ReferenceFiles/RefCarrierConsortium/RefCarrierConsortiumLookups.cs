namespace Enterprise.MasterFiles.Business
{
	public class RefCarrierConsortiumLookups : AutoRefCarrierConsortiumLookups
	{
		public RefCarrierConsortiumLookups(AutoRefCarrierConsortium parent) : base(parent)
		{
		}

		#region Shipping Lines

		public SeaShippingProviderCollection ShippingLine_List
		{
			get
			{
				if (fShippingLine_List == null)
				{
					fShippingLine_List = new SeaShippingProviderCollection(Factory);
				}
				return fShippingLine_List;
			}
		}

		SeaShippingProviderCollection fShippingLine_List;

		#endregion

		#region Vessels

		public RefVesselCollection AvailableVessel_List
		{
			get
			{
				if (fAvailableVessel_List == null)
				{
					fAvailableVessel_List = new NonConsortiumVesselCollection(Factory);
				}
				return fAvailableVessel_List;
			}
		}

		RefVesselCollection fAvailableVessel_List;

		#endregion

		#region Organisation Proxys

		public override OrgHeaderCollection Headers
		{
			get { return new ConsortiumShippingProviderCollection(Factory); }
		}

		#endregion
	}
}
