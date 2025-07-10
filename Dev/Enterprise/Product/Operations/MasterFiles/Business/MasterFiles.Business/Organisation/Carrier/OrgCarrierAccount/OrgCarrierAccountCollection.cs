using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAccountCollection : ActiveBusinessObjectCollection<OrgCarrierAccount>
	{
		public OrgCarrierAccountCollection(OrgHeader master)
			: base(Argument.NotNull(master, nameof(master)).Factory, master, new ZQuery(), OrgCarrierAccountSchema.OAN_OH_Carrier)
		{
			Master = master;
		}

		public OrgCarrierAccountCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgCarrierAccountCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override bool AllowNew
		{
			get { return true; }
		}

		public readonly OrgHeader Master;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			if (Master != null)
			{
				query.AddToFilter(OrgCarrierAccountSchema.OAN_OH_Carrier, Master.PK);
			}

			return query;
		}
	}
}
