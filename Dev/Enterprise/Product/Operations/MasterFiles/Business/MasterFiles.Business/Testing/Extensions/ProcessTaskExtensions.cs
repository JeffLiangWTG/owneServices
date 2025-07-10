
#if DEBUG

namespace Enterprise.MasterFiles.Business.Testing
{
	using CargoWise.Types;

	public static class ProcessTaskExtensions
	{
		/// <summary>
		///		Just for singleline or inline initialization.
		/// </summary>
		/// <remarks>
		///		Please add new optional parameters on demand.
		/// </remarks>
		/// <example>
		///		processTask.With(P9_LineTriggerType: "SBL", P9_LineTrigger: new ZGuid("a8da7392-2b50-11e5-9eb7-902b34dc814a"));
		///	
		///		processTask.With(
		///			P9_LineTriggerType: "SBL", 
		///				P9_LineTrigger: new ZGuid("a8da7392-2b50-11e5-9eb7-902b34dc814a"));
		///				
		///		processTask
		///			.With(P9_LineTriggerType: "SBL")
		///			.With(P9_LineTrigger: new ZGuid("a8da7392-2b50-11e5-9eb7-902b34dc814a"));
		///	
		/// </example>
		public static ProcessTask With(this ProcessTask processTask, string p9_LineTriggerType = null, ZGuid? p9_ParentID = null, ZGuid? p9_GC = null, string p9_SE_NKMilestoneEvent = null)
		{
			processTask.TriggerConditions.TriggerEventCode = p9_SE_NKMilestoneEvent;
			processTask.P9_LineTriggerType = p9_LineTriggerType;

			if (p9_ParentID.HasValue)
			{
				processTask.P9_ParentID = p9_ParentID.Value;
			}

			if (p9_GC.HasValue)
			{
				processTask.P9_GC = p9_GC.Value;
			}

			return processTask;
		}
	}
}

#endif
