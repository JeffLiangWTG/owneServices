using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.GlbCompany)]
	public class GlbCompanyCollection : ActiveBusinessObjectCollection<GlbCompany>, Integration.IGlbCompanyCollection
	{
		public GlbCompanyCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public GlbCompanyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GlbCompanyCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected override void SetDefaultsForNewElementCore(GlbCompany newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			var company = newElement;
			if (!company.GC_GeoLocation.IsValid)
			{
				company.GC_GeoLocation = new CargoWise.Types.ZGeography("0 0");
			}
		}
	}
}
