using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentDefaultFilterProvider))]
	internal class AgencyShipmentDefaultFilterProviderTest_BillOfLading : AgencyShipmentDefaultFilterProviderTest
	{
		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyBillOfLading;
			}
		}
		#endregion
	}
}
