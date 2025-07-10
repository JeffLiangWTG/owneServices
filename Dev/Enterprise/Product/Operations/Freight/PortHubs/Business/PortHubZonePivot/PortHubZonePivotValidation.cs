namespace Enterprise.Freight.PortHubs.Business
{
	using CargoWise.EntityFramework;

	public class PortHubZonePivotValidation : AutoPortHubZonePivotValidation
	{
		public PortHubZonePivotValidation(AutoPortHubZonePivot parent)
			: base(parent)
		{
		}

		protected override void CheckTX_TZ_Zone()
		{
			base.CheckTX_TZ_Zone();

			MandatoryValidation.CheckEntered(Parent.TX_TZ_ZoneInfo);
			ListValidation.ErrorIfInvalidPK(Parent.TX_TZ_ZoneInfo);
			CheckForDuplicates(Parent.TX_TZ_ZoneInfo);
		}

		protected override void CheckTX_PL_NKCarrierServiceLevel()
		{
			base.CheckTX_PL_NKCarrierServiceLevel();

			if (Parent.Carrier != null && Parent.Carrier.OH_IsShippingProvider)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TX_PL_NKCarrierServiceLevelInfo);
			}
			else if (!Parent.TX_PL_NKCarrierServiceLevel.IsEmpty)
			{
				Parent.TX_PL_NKCarrierServiceLevelInfo.AddError(Res.GetString("f4c73f66-d7a3-411a-af3c-83cedec459fb", "Zone Owner is not a carrier and, therefore, no service level is permitted."));
			}

			CheckForDuplicates(Parent.TX_PL_NKCarrierServiceLevelInfo);
		}

		void CheckForDuplicates(ZPropertyInfo propertyInfo)
		{
			if (Parent.PortHub != null)
			{
				foreach (PortHubZonePivot pivot in Parent.PortHub.PortHubZonePivots)
				{
					if (pivot.TX_TZ_Zone == Parent.TX_TZ_Zone && pivot.PK != Parent.PK)
					{
						propertyInfo.AddError(Res.GetString("2680db7e-b567-46fa-bb4b-6b3feca5a929", "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed."));
					}
				}
			}
		}

		protected new PortHubZonePivot Parent
		{
			get { return (PortHubZonePivot)base.Parent; }
		}
	}
}
