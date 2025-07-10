using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public abstract class BaseCustomsStmProcessQueueBatchProcessor : BatchProcess
	{
		public BaseCustomsStmProcessQueueBatchProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		public bool NeedToNudge { get; private set; }
		protected sealed override void Execute(CancellationToken cancellationToken)
		{
			NeedToNudge = false;
			var processQueuePKs = GetProcessQueuePKs();
			foreach (var processQueuePK in processQueuePKs)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var factory = new BusinessObjectFactory()
				{
					RefreshEnabled = false,
					NameForDebugging = $"Customs Process Queue Processor Factory"
				};

				var queuedItem = factory.Load<StmProcessQueue>(processQueuePK);
				if (queuedItem != null)
				{
					try
					{
						ProcessQueuedItemCore(queuedItem);
					}
					finally
					{
						var saveCount = 0;
						try
						{
							DeleteQueuedItemAndSave(queuedItem, ref saveCount);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Logger.LogError($"The following error was encountered while system is trying to delete record from dbo.StmProcessQueue, SW_PK={queuedItem.PK}\r\n" + ex.Message);
						}
					}
				}
			}

			NeedToNudge = processQueuePKs.Length == 50;
		}

		ZGuid[] GetProcessQueuePKs()
		{
			var factory = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "Customs Process Queue Query Factory"
			};
			var query = new ZQuery(StmProcessQueueSchema.SW_ApplicationCode, ProcessQueueApplicationCode);
			query.AddToFilter(StmProcessQueueSchema.SW_JobTypeCode, CustomsStmProcessQueueLoader.Constants.JobTypeCode);
			query.AddToFilter(StmProcessQueueSchema.SW_ActionCode, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(StmProcessQueueSchema.SW_ReferenceTableCode, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(StmProcessQueueSchema.SW_ReferenceID, SQLComparisonOperator.NotEqual, null);

			var sqlText = System.FormattableString.Invariant($@"
SELECT TOP 50 {StmProcessQueueSchema.Constants.PK}
FROM dbo.StmProcessQueue
{query.GetAsWhereClause(false)}
ORDER BY SW_PostedTimeUtc
OPTION (RECOMPILE)
");

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sqlText, new ZSqlParameterCollection(query.Params));
			return collection.Select(x => (ZGuid)x[StmProcessQueueSchema.Constants.PK]).ToArray();
		}

		protected abstract string ProcessQueueApplicationCode { get; }

		protected abstract void ProcessQueuedItemCore(StmProcessQueue queuedItem);

		void DeleteQueuedItemAndSave(StmProcessQueue queuedItem, ref int count)
		{
			try
			{
				queuedItem.Delete();
				queuedItem.Factory.Save();
			}
			catch (ZSaveConcurrencyException)
			{
				if (count > 3)
				{
					throw;
				}
				else
				{
					Thread.Sleep(3000);
					count++;
					DeleteQueuedItemAndSave(queuedItem, ref count);
				}
			}
		}
	}
}
