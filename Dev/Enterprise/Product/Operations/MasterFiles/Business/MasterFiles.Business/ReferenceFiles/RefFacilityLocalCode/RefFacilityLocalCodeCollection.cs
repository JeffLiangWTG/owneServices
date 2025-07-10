using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefFacilityLocalCodeCollection : ActiveBusinessObjectCollection<RefFacilityLocalCode>
	{
		public RefFacilityLocalCodeCollection(RefFacility master)
				: base(Argument.NotNull(master, nameof(master)).Factory, master, new ZQuery(), RefFacilityLocalCodeSchema.RFL_RFT_NKFacilityCode)
		{
			Master = master;
		}

		public RefFacilityLocalCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public readonly RefFacility Master;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			if (Master != null)
			{
				query.AddToFilter(RefFacilityLocalCodeSchema.RFL_RFT_NKFacilityCode, Master.RFT_Code);
			}

			return query;
		}
	}
}
