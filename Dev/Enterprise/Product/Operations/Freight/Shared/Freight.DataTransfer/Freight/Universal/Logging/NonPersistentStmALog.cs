using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer
{
	public sealed class NonPersistentStmALog : BaseStmALog
	{
		public NonPersistentStmALog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Parameters = new Dictionary<string, string>();
		}

		public override IDictionary<string, string> Parameters { get; }

		public override bool IsSavedByFactory => false;

		public static NonPersistentStmALog GetDummyLog(BusinessObjectFactory factory, BusinessObject parent, IQueuedLog queuedLog, string parameterType)
		{
			var parameters = new Dictionary<string, string>();

			if (!string.IsNullOrWhiteSpace(parameterType))
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, parameterType);
			}

			return GetDummyLog(factory, parent, queuedLog, parameters);
		}

		public static NonPersistentStmALog GetDummyLog(BusinessObjectFactory factory, BusinessObject parent, IQueuedLog queuedLog)
		{
			var parameters = StmALog.GetParametersFromReference(queuedLog.SJ_Reference);
			return GetDummyLog(factory, parent, queuedLog, parameters);
		}

		static NonPersistentStmALog GetDummyLog(BusinessObjectFactory factory, BusinessObject parent, IQueuedLog queuedLog, IDictionary<string, string> parameters)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));
			_ = parent ?? throw new ArgumentNullException(nameof(parent));
			_ = queuedLog ?? throw new ArgumentNullException(nameof(queuedLog));

			var result = factory.New<NonPersistentStmALog>();
			result.SL_Table = parent.TableName;
			result.SL_Parent = parent.PK;
			result.SL_EventTimeOffset = StmALog.ToDateTimeOffset(factory, queuedLog.SJ_EventTime, queuedLog.SJ_EventTimeUtc, queuedLog.SJ_GB_NKBranch);
			result.SL_SE_NKEvent = queuedLog.SJ_SE_NKEvent;
			result.SL_IsEstimate = queuedLog.SJ_IsEstimate;

			foreach (var parameter in parameters)
			{
				result.Parameters[parameter.Key] = parameter.Value;
			}

			return result;
		}
	}
}
