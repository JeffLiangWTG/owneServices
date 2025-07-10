using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffManagerCollection : ActiveBusinessObjectCollection<GlbStaffManager>
	{
		public GlbStaffManagerCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffManagerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffManagerCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		readonly GlbStaff master;
		public GlbStaffManagerCollection(GlbStaff master, ZQuery filter, bool isManager = false)
			: base(master.Factory, master, filter, isManager ? GlbStaffManagerSchema.GSM_GS_Manager : GlbStaffManagerSchema.GSM_GS_Staff)
		{
			this.master = master;
		}

		public GlbStaffManagerCollection(GlbStaff master, ZString type, bool isManager = false)
			: base(master.Factory, master, new ZQuery(GlbStaffManagerSchema.GSM_ManagerType, type), isManager ? GlbStaffManagerSchema.GSM_GS_Manager : GlbStaffManagerSchema.GSM_GS_Staff)
		{
			this.master = master;
			Type = type;
		}

		string Type { get; }

		protected override void SetDefaultsForNewElementCore(GlbStaffManager newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Type != null)
			{
				newElement.GSM_ManagerType = Type;
			}

			if (newElement.GSM_ManagerType != DefaultStaffReportingRoles.Codes.DirectManager)
			{
				newElement.GSM_EffectiveDate = ZDateTime.Today;
			}
		}

		protected override void SetHasChanges(bool hasChanges)
		{
			if (master != null)
			{
				master.HasChanges = hasChanges;
			}
		}
	}
}
