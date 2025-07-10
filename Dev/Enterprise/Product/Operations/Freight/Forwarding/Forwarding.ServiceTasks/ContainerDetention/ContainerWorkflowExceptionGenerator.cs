using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	public abstract class ContainerWorkflowExceptionGenerator : IProcessor
	{
		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			GenerateForConsols(token);
			GenerateForDeclarations(token);

			HighWaterMark = ZDateTime.UtcNow;
		}

		protected abstract string ContainerExceptionEvent { get; }
		protected abstract DateTimeRegistryItem HighWaterMarkRegistryItem { get; }
		protected abstract IEnumerable<ZDBOnlyQuery> ConsolQueries { get; }
		protected abstract IEnumerable<ZDBOnlyQuery> DeclarationQueries { get; }

		#region HighWaterMark

		protected ZDateTime HighWaterMark
		{
			get
			{
				if (!highWaterMark.IsValid)
				{
					DateTime value = HighWaterMarkRegistryItem.Value;
					highWaterMark = (value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(value);
				}
				return highWaterMark;
			}
			set
			{
				HighWaterMarkRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime());
				highWaterMark = value;
			}
		}
		ZDateTime highWaterMark;

		#endregion

		#region GenerateExceptions

		void GenerateForConsols(CancellationToken token)
		{
			var bizoType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>();

			foreach (var query in ConsolQueries)
			{
				token.ThrowIfCancellationRequested();
				query.AddSubQuery(GetProcessTaskSubQuery(), JoinCondition.And);
				GenerateExceptionsCore(bizoType, query);
			}
		}

		void GenerateForDeclarations(CancellationToken token)
		{
			var bizoType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			foreach (var query in DeclarationQueries)
			{
				token.ThrowIfCancellationRequested();
				var filterGroup = new ZDBOnlyQuery(bizoType);

				ZString sql = "JE_ComputedParent NOT IN" +
					" (SELECT " + ProcessTasksSchema.Constants.P9_ParentID + " FROM " + ProcessTasksSchema.Constants.SqlSchemaName + "." + ProcessTasksSchema.Constants.TableName +
					" WHERE " + ProcessTasksSchema.Constants.P9_ParentID + " IS NOT NULL AND " + ProcessTasksSchema.Constants.P9_Type +
					" = @ExceptionType AND " + ProcessTasksSchema.Constants.P9_SE_NKExceptionEvent + " = @ExceptionEvent)";

				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@ExceptionType", Core.Constants.Workflow.ExceptionType, ProcessTasksSchema.P9_Type);
				parameters.Add("@ExceptionEvent", ContainerExceptionEvent, ProcessTasksSchema.P9_SE_NKExceptionEvent);

				filterGroup.AddFilterAndZSQLParameterCollection(sql, parameters);

				query.AddToFilter(filterGroup);

				GenerateExceptionsCore(bizoType, query);
			}
		}

		ZDBOnlySubQuery GetProcessTaskSubQuery()
		{
			ZDBOnlySubQuery processTaskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID, true);
			processTaskSubQuery.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.ExceptionType);
			processTaskSubQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKExceptionEvent, ContainerExceptionEvent);

			return processTaskSubQuery;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		void GenerateExceptionsCore(Type bizoType, ZDBOnlyQuery query)
		{
			while (true)
			{
				BusinessObject[] businessObjects = CreateFactoryAndLoadBusinessObjects(bizoType, query);

				if (businessObjects.Length > 0)
				{
					var debugLog = new StringBuilder();

					AddContainerException(businessObjects, debugLog);

					var saveTryCount = 0;

					try
					{
						var saveSucceeded = false;

						do
						{
							saveSucceeded = TrySaveFactory(businessObjects[0].Factory, ++saveTryCount < NumberOfFactorySaveRetries);
						} while (!saveSucceeded);
					}
					catch (Exception ex)
					{
						var unsavedBusinessObjects = businessObjects.Where(x => !x.HasChanges || !x.IsInDatabaseIncludingChildren);
						var unsavedBusinessObjectsInNewFactory = CreateFactoryAndLoadBusinessObjects(bizoType, query);

						if (unsavedBusinessObjectsInNewFactory.Length > 0)
						{
							debugLog.AppendLine("After save:");
							unsavedBusinessObjects.ForEach(x => debugLog.AppendLine(BizoDebugLog(x)));

							debugLog.AppendLine("unsavedBusinessObjectsInNewFactory:");
							unsavedBusinessObjectsInNewFactory.ForEach(x => debugLog.AppendLine(BizoDebugLog(x)));

							throw new ZCannotSaveException(debugLog.ToString(), "Save Exceeded Retry Count", ex);
						}
					}
				}
				else
				{
					break;
				}
			}
		}

		string BizoDebugLog(BusinessObject bizo) => $"bizo PK - {bizo.PK}, HasChanges - {bizo.HasChanges}, IsInDatabaseIncludingChild - {bizo.IsInDatabaseIncludingChildren}, WorkflowItems.Exceptions: {((IWorkflowProvider)bizo).WorkflowItems.Exceptions.Count}";

		int NumberOfFactorySaveRetries
		{
			get { return 3; }
		}

		protected virtual BusinessObject[] CreateFactoryAndLoadBusinessObjects(Type bizoType, ZDBOnlyQuery query)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory() { NameForDebugging = "Container Exceptions Generator" }; // Factory Name
			query.MaximumRows = BatchSize;

			return factory.Load(bizoType, query);
		}

		int BatchSize
		{
			get { return 100; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		void AddContainerException(BusinessObject[] businessObjects, StringBuilder debugLog)
		{
			debugLog.AppendLine("Before Save:");

			foreach (BusinessObject bizo in businessObjects)
			{
				ProcessTask exception = ((IWorkflowProvider)bizo).WorkflowItems.Exceptions.AddNew();
				exception.P9_SE_NKExceptionEvent = ContainerExceptionEvent;
				exception.P9_Description = exception.GetExceptionEventDescription(exception.P9_SE_NKExceptionEvent);

				debugLog.AppendLine(BizoDebugLog(bizo));
			}
		}

		protected virtual bool TrySaveFactory(BusinessObjectFactory factory, bool shouldEatConcurrencyException)
		{
			try
			{
				factory.Save();
				return true;
			}
			catch (ZSaveConcurrencyException)
			{
				if (!shouldEatConcurrencyException)
				{
					throw;
				}
				return false;
			}
		}

		#endregion
	}
}
