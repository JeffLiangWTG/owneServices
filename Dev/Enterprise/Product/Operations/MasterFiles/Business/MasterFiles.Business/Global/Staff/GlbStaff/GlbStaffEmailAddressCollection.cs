using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffEmailAddressCollection : ActiveBusinessObjectCollection<GlbStaffEmailAddress>
	{
		public GlbStaffEmailAddressCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffEmailAddressCollection(GlbStaff master)
			: this(master, new ZQuery())
		{
		}

		public GlbStaffEmailAddressCollection(GlbStaff master, ZQuery query)
			: base(master.Factory, master, query, GlbStaffEmailAddressSchema.GSE_GS)
		{
			this.master = master;
		}

		readonly GlbStaff master;

		protected override void OnAdded(GlbStaffEmailAddress newStaffEmailAddress)
		{
			base.OnAdded(newStaffEmailAddress);

			if (master != null)
			{
				newStaffEmailAddress.GSE_GS = master.PK;
			}

			newStaffEmailAddress.GSE_GC_Company = GlbCompany.CurrentCompany.PK;
		}

		protected override void OnAddIntoRelationshipCore(BusinessObject businessObject)
		{
			base.OnAddIntoRelationshipCore(businessObject);

			var glbStaffEmailAddress = businessObject as GlbStaffEmailAddress;
			glbStaffEmailAddress.GSE_GC_Company = GlbCompany.CurrentCompany.PK;

			if (master != null)
			{
				glbStaffEmailAddress.GSE_GS = master.PK;
			}
		}

		public GlbStaffEmailAddress FindByEmailAddressString(ZString emailAddress)
		{
			return this.FirstOrDefault(x => x.GSE_EmailAddress.EqualsIgnoringCase(emailAddress));
		}

		public GlbStaffEmailAddress FindByEmailAddressType(ZString type)
		{
			return this.FirstOrDefault(x => x.GSE_Type == type);
		}
	}
}
