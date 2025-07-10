using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbDepartmentLookups : AutoGlbDepartmentLookups
	{
		const StmNoteContextModule DefaultContextActivity = StmNoteContextModule.A;
		const StmNoteContextDirection DefaultContextDirection = StmNoteContextDirection.A;
		const StmNoteContextFreightMode DefaultContextMode = StmNoteContextFreightMode.A;

		public GlbDepartmentLookups(AutoGlbDepartment parent)
			: base(parent) { }

		protected new GlbDepartment Parent => (GlbDepartment)base.Parent;

		#region Activity

		public CodeDescriptionPairList ActivityList
		{
			get
			{
				if (fActivityList == null)
				{
					fActivityList = new CodeDescriptionPairList();
					fActivityList.AddPair(StmNoteContextModule.D, "C", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|Customs", "Customs"));
					fActivityList.AddPair(StmNoteContextModule.C, "D", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|DepotCFS", "Depot CFS"));
					fActivityList.AddPair(StmNoteContextModule.F, "F", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|Forwarding", "Forwarding"));
					fActivityList.AddPair(DefaultContextActivity, "L", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|Linehaul", "Linehaul"));
					fActivityList.AddPair(StmNoteContextModule.S, "S", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|Shipping", "Ships Agency"));
					fActivityList.AddPair(StmNoteContextModule.T, "T", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|Cartage", "Port Transport"));
					fActivityList.AddPair(StmNoteContextModule.W, "W", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|Warehouse", "Warehouse"));
					fActivityList.AddPair(DefaultContextActivity, "G", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|Gateway", "Gateway"));
					fActivityList.AddPair(DefaultContextActivity, "Y", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|ContainerYard", "Container Yard"));
					fActivityList.AddPair(DefaultContextActivity, "M", Res.GetString("MasterFiles|GlbDepartmentLookups|ActivityList|Miscellaneous", "Miscellaneous"));
				}

				return fActivityList;
			}
		}

		CodeDescriptionPairList fActivityList;

		#endregion

		#region Direction

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				if (fDirectionList == null)
				{
					fDirectionList = new CodeDescriptionPairList();
					fDirectionList.AddPair(StmNoteContextDirection.E, "E", Res.GetString("MasterFiles|GlbDepartmentLookups|DirectionList|Export", "Export"));
					fDirectionList.AddPair(StmNoteContextDirection.I, "I", Res.GetString("MasterFiles|GlbDepartmentLookups|DirectionList|Import", "Import"));
					fDirectionList.AddPair(StmNoteContextDirection.D, "D", Res.GetString("MasterFiles|GlbDepartmentLookups|DirectionList|Domestic", "Domestic"));
					fDirectionList.AddPair(DefaultContextDirection, "O", Res.GetString("MasterFiles|GlbDepartmentLookups|DirectionList|Other", "Other"));
				}
				return fDirectionList;
			}
		}

		CodeDescriptionPairList fDirectionList;

		#endregion

		public CodeDescriptionPairList ModesList => ModeListByActivity(Parent.GE_Activity);

		#region TransportMode

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (transportModeList == null)
				{
					transportModeList = new CodeDescriptionPairList();
					transportModeList.AddPair(StmNoteContextFreightMode.I, "A", Res.GetString("MasterFiles|GlbDepartmentLookups|TransportModeList|Air", "Air"));
					transportModeList.AddPair(StmNoteContextFreightMode.S, "S", Res.GetString("MasterFiles|GlbDepartmentLookups|TransportModeList|Sea", "Sea"));
					transportModeList.AddPair(StmNoteContextFreightMode.W, "L", Res.GetString("MasterFiles|GlbDepartmentLookups|TransportModeList|Rail", "Rail"));
					transportModeList.AddPair(StmNoteContextFreightMode.R, "R", Res.GetString("MasterFiles|GlbDepartmentLookups|TransportModeList|Road", "Road"));
					transportModeList.AddPair(DefaultContextMode, "P", Res.GetString("MasterFiles|GlbDepartmentLookups|TransportModeList|Post", "Post"));
					transportModeList.AddPair(DefaultContextMode, "O", Res.GetString("MasterFiles|GlbDepartmentLookups|TransportModeList|Other", "Other"));
				}
				return transportModeList;
			}
		}

		CodeDescriptionPairList transportModeList;

		#endregion

		#region ShippingMode

		public CodeDescriptionPairList ShippingModeList
		{
			get
			{
				if (shippingModeList == null)
				{
					shippingModeList = new CodeDescriptionPairList();
					shippingModeList.AddPair(DefaultContextMode, "C", Res.GetString("MasterFiles|GlbDepartmentLookups|ShippingModeList|Containerised", "Containerized"));
					shippingModeList.AddPair(DefaultContextMode, "B", Res.GetString("MasterFiles|GlbDepartmentLookups|ShippingModeList|BBulk", "B-Bulk"));
					shippingModeList.AddPair(DefaultContextMode, "V", Res.GetString("MasterFiles|GlbDepartmentLookups|ShippingModeList|ROROVechile", "RORO/Vehicle"));
					shippingModeList.AddPair(DefaultContextMode, "U", Res.GetString("MasterFiles|GlbDepartmentLookups|ShippingModeList|Bulk", "Bulk"));
					shippingModeList.AddPair(DefaultContextMode, "L", Res.GetString("MasterFiles|GlbDepartmentLookups|ShippingModeList|LiquidBulk", "Liquid Bulk"));
					shippingModeList.AddPair(DefaultContextMode, "D", Res.GetString("MasterFiles|GlbDepartmentLookups|ShippingModeList|Detention", "Detention"));
					shippingModeList.AddPair(DefaultContextMode, "A", Res.GetString("MasterFiles|GlbDepartmentLookups|ShippingModeList|VoyageAccounting", "Voyage Accounting"));
				}

				return shippingModeList;
			}
		}

		CodeDescriptionPairList shippingModeList;

		#endregion

		#region ModeListByActivity

		public CodeDescriptionPairList ModeListByActivity(ZString activity)
		{
			switch (activity)
			{
				case "":
					return new CodeDescriptionPairList();
				case "S":
					return ShippingModeList;
				default:
					return TransportModeList;
			}
		}

		#endregion
	}
}
