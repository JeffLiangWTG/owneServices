using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID("GlbCapability")]
	public class GlbCapabilityCollection : ActiveBusinessObjectCollection<GlbCapability>, IGlbCapabilityCollection
	{
		/// <summary>
		/// For use with the ObjectFactory
		/// </summary>
		public GlbCapabilityCollection()
			: base(new BusinessObjectFactory())
		{
		}

		public GlbCapabilityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
