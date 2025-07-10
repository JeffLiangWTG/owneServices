using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccChargeCodeForRegistry)]
	public class AccChargeCodeCollectionForRegistry : AccChargeCodeCollection
	{
		public AccChargeCodeCollectionForRegistry(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZGuid CompanyPK
		{
			get;
			set;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = new ZQuery();
			if (CompanyPK.IsValid && !CompanyPK.IsEmpty)
			{
				result.AddToFilter(AccChargeCodeSchema.AC_GC, CompanyPK);
			}
			return result;
		}
	}
}
