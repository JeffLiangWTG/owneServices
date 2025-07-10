using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskDataContextManager : EventDataContextManager<ProcessTask>, IParentEventDataContextManager
	{
		public ProcessTaskDataContextManager()
		{
		}

		#region Declaring supported functionality

		public override bool ManagesEvents => true;

		#endregion

		#region Data Mapping

		public override DataContextType DataContextType => DataContextType.WorkflowTask;
		public override ZString DataContextKey => ParentBO.P9_TaskID;
		public override string DefaultOutputDirectory => string.Empty;

		public IEnumerable<IEventDataContextManager> ChildContextManagers
		{
			get
			{
				var manager = ParentBO.ParentBusinessObject?.GetUniversalDataContextManager();
				if (manager is IEventDataContextManager parentEventManager)
				{
					return new[] { parentEventManager };
				}
				else
				{
					return Enumerable.Empty<IEventDataContextManager>();
				}
			}
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			KeyValuePair<TypeWithDescription, IZType> MakePair(string type, IZType value)
			{
				return new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription(type), value);
			}

			yield return MakePair("TaskType", ParentBO.P9_Type);
			yield return MakePair("TaskSequence", ParentBO.P9_Sequence);
			yield return MakePair("TaskDescription", ParentBO.P9_Description);
			yield return MakePair("TaskStatus", ParentBO.P9_Status);
			yield return MakePair("AssignedGroup", ParentBO.AssignedGroup?.GG_Code ?? ZString.Empty);
			yield return MakePair("AssignedStaff", ParentBO.P9_GS_NKAssignedStaffMember);
			yield return MakePair("AssignedCapability", ParentBO.RequiredCapability?.G4_Code ?? ZString.Empty);
			yield return MakePair("TaskNotes", (ZString)ORtfTextUtil.RtfToText(ParentBO.P9_NotesAsString));
			yield return MakePair("EffectiveNudge", ParentBO.GetEffectiveTaskNudgeValue());
		}

		#endregion

		#region Unsupported Functionality

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(ProcessTasksSchema.P9_TaskID, matchingValues.Key);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ProcessTasksEventParentFinder(factory, this, logger);
		}

		class ProcessTasksEventParentFinder : EventParentFinder
		{
			internal ProcessTasksEventParentFinder(BusinessObjectFactory factory, ProcessTaskDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(Event xmlEvent)
			{
				return null;
			}
		}

		#endregion
	}
}
