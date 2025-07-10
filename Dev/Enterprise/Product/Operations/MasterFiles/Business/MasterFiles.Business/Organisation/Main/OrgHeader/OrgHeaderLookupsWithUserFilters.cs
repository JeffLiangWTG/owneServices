namespace Enterprise.MasterFiles.Business
{
	class OrgHeaderLookupsWithUserFilters : OrgHeaderLookups
	{
		public OrgHeaderLookupsWithUserFilters(OrgHeader parent)
			: base(parent)
		{
		}

		#region ClosestPortsOverride

		public override RefUNLOCOCollection ClosestPorts
		{
			get
			{
				if (closestPorts == null)
				{
					closestPorts = new RefUNLOCOCollection(Factory, (OrgHeader)Parent);
				}
				return closestPorts;
			}
		}

		RefUNLOCOCollection closestPorts;
		#endregion
	}
}
