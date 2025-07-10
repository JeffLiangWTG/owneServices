using System.Collections.Generic;
using System.Dynamic;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class LogEventDataModel
	{
		public LogEventDataModel(IWorkflowTriggerSource log)
		{
			this.Log = log;
		}

		#region Properties

		public virtual ZBool IsEstimate => Log.IsEstimate;

		public virtual ZString Reference => BaseStmALog.GetReferenceForBinding(Log.Reference);

		public ZString UserCode => Log.StaffCode;

		public ZString DepartmentCode => Log.DepartmentCode;

		public ZString BranchCode => Log.BranchCode;

		public ZString CompanyCode => Log.CompanyCode;

		public ZString Source => Log.FriendlyTableName;

		public ZDateTime EventTime => Log.EventTime;

		public ZDateTime PostedTimeUtc => Log.PostedTimeUtc;

		public ZDateTime PostedTimeLocal
		{
			get
			{
				var date = Log.PostedTimeUtc;
				if (date.IsValid)
				{
					return EnvProxy.Instance.Time.GetLocalTimeFromUtc(date.ToDateTime());
				}
				else
				{
					return date;
				}
			}
		}

		public DynamicObject Params => parameters ?? (parameters = new LogsParams(Log));

		#endregion

		#region Internal

		protected internal IWorkflowTriggerSource Log { get; }
		DynamicObject parameters;

		#endregion

		#region Types

		class LogsParams : DynamicObject
		{
			public LogsParams(IWorkflowTriggerSource log)
			{
				var reference = log.Reference;
				parameters = StmALog.GetParametersFromReference(reference);
				REF = StmALog.GetFreeTextFromReference(reference);
			}

			#region Properties

			public string REF { get; }

			#endregion

			#region DynamicObject

			public override bool TryGetMember(GetMemberBinder binder, out object result)
			{
				string value;

				if (!parameters.TryGetValue(binder.Name, out value))
				{
					value = string.Empty;
				}

				result = value;
				return true;
			}

			#endregion

			readonly IDictionary<string, string> parameters;
		}

		#endregion
	}
}
