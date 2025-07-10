using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public partial interface ITelematicsBusinessObjectLookupProvider
	{
		ITelematicsBusinessObject GetBusinessObject(BusinessObjectFactory factory, ZGuid primaryKey);
	}

	#region ITelematicsBusinessObjectLookupProvider contract binding

	#endregion
}
