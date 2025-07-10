using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefPremisesGateCodeCollection : ActiveBusinessObjectCollection<RefPremisesGateCode>
	{
		public RefPremisesGateCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefPremisesGateCodeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
