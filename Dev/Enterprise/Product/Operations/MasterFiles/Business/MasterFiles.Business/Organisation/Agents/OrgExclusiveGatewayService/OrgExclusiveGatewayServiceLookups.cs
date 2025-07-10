namespace Enterprise.MasterFiles.Business
{
	public class OrgExclusiveGatewayServiceLookups : AutoOrgExclusiveGatewayServiceLookups
	{
		public OrgExclusiveGatewayServiceLookups(AutoOrgExclusiveGatewayService parent)
			: base(parent)
		{
		}

		public override RefServiceLevelCollection GatewayServices =>
			new GatewayServiceLevelCollection(Factory);

		public override RefServiceLevelCollection ShipmentServiceLevels =>
			new GatewayServiceLevelCollection(Factory);
	}
}
