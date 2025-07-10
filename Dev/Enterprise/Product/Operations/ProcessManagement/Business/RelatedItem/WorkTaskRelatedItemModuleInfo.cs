using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkTaskRelatedItemModuleInfo
	{
		public static WorkTaskRelatedItemModuleInfo WorkItem(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("WorkItem", delegate
			{
				return WorkItem(factory, true);
			});
		}

		public static WorkTaskRelatedItemModuleInfo WorkItem(BusinessObjectFactory factory, bool allowNew)
		{
			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = Res.GetString("983c135b-d76a-4c5d-b3fe-281bdf04f612", "Work Item"),
				Type = WorkTaskRelatedItemTypes.WorkItem,
				DataContextType = DataContextType.WorkItem,
				AllowNew = allowNew,
				AllowAttach = true,
				ControllerID = ControllerIDs.WorkItem,
				ModuleID = ModuleIDs.WorkItem,
				FindBoxList = new WorkItemCollection(factory),
				BusinessObjectType = typeof(WorkItem),
				JobNumberColumn = WorkItemSchema.WKI_WorkItemNumber,
			};
		}

		public static WorkTaskRelatedItemModuleInfo WorkItem(BusinessObjectFactory factory, bool allowNew, ZQuery additionalFilter)
		{
			WorkItemCollection findBoxList = new WorkItemCollection(factory);

			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = Res.GetString("983c135b-d76a-4c5d-b3fe-281bdf04f612", "Work Item"),
				Type = WorkTaskRelatedItemTypes.WorkItem,
				DataContextType = DataContextType.WorkItem,
				AllowNew = allowNew,
				AllowAttach = true,
				ControllerID = ControllerIDs.WorkItem,
				ModuleID = ModuleIDs.WorkItem,
				FindBoxList = findBoxList,
				AdditionalFilterForFindBox = additionalFilter,
				BusinessObjectType = typeof(WorkItem),
				JobNumberColumn = WorkItemSchema.WKI_WorkItemNumber,
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Hard coded constant")]
		public static WorkTaskRelatedItemModuleInfo Project(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Project", delegate
			{
				return Project(factory, true);
			});
		}

		public static WorkTaskRelatedItemModuleInfo Project(BusinessObjectFactory factory, bool allowNew)
		{
			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = Res.GetString("DD76F761-D7A7-4DF7-B70B-BCB9B6AD09BA", "Project"),
				Type = WorkTaskRelatedItemTypes.Project,
				DataContextType = DataContextType.Project,
				AllowNew = allowNew,
				AllowAttach = true,
				ControllerID = ControllerIDs.Project,
				ModuleID = ModuleIDs.Project,
				FindBoxList = new ProjectCollection(factory),
				BusinessObjectType = typeof(Project),
				JobNumberColumn = WorkProjectSchema.WKP_ProjectNumber,
			};
		}

		public static WorkTaskRelatedItemModuleInfo CustomerServiceTicket(BusinessObjectFactory factory)
		{
			return new WorkTaskRelatedItemModuleInfo
			{
				Caption = WorkRequest.SingularName,
				Type = WorkTaskRelatedItemTypes.CustomerServiceTicket,
				DataContextType = DataContextType.CustomerServiceTicket,
				AllowNew = false,
				AllowAttach = true,
				ControllerID = ControllerIDs.CustomerServiceTicket,
				ModuleID = ModuleIDs.CustomerServiceTicket,
				FindBoxList = new WorkRequestCollection(factory),
				BusinessObjectType = typeof(WorkRequest),
				JobNumberColumn = WorkRequestSchema.WKR_RequestNumber,
			};
		}

		public string Caption { get; set; }
		public string Type { get; set; }
		public DataContextType DataContextType { get; set; }
		public bool AllowNew { get; set; }
		public bool AllowAttach { get; set; }
		public ControllerID ControllerID { get; set; }
		public ModuleIdentifier ModuleID { get; set; }
		public IBusinessObjectCollection FindBoxList { get; set; }
		public ZQuery AdditionalFilterForFindBox { get; set; }
		public SchemaStringColumn JobNumberColumn { get; set; }
		public Type BusinessObjectType { get; set; }
	}
}
