using System;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer
{
	[Serializable]
	public class GlbCompanyDataImportLogSubscriber : LogSubscriber
	{
		public override string[] EventTypes
		{
			get { return new string[] { AutoEvents.DataImport.Code }; }
		}

		public override string Name
		{
			get { return nameof(GlbCompanyDataImportLogSubscriber); }
		}

		public override string FriendlyName
		{
			get { return (NoResString)"Company Accounting Related Data Subscriber"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { GlbCompanySchema.Constants.TableName }; }
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (IQueuedLog queuedLog in queuedLogs)
			{
				if (queuedLog.SJ_ParentTableCode == GlbCompanySchema.Constants.Prefix)
				{
					var newCompany = queuedLog.Factory.Load<GlbCompany>(queuedLog.SJ_ParentID);
					newCompany?.CopyOrCreateAccountingRelatedData();
				}
			}
		}
	}
}