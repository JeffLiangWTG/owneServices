using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Recruiter.ServiceTasks.HRJobApplicationParsingQueueServiceTask.Code,
						"Human Resources Parsing Queue Service Task",
						"HRM",
						typeof(Enterprise.Recruiter.ServiceTasks.HRJobApplicationParsingQueueServiceTask),
						MinimumPeriod = "30seconds",
						CanRunInAnyBranch = true,
						DefaultScheduleRunEvery = "15minutes",
						ActiveByDefault = true,
						AllowsMultipleInstances = false)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Recruiter.ServiceTasks.HRJobApplicationParsingQueueServiceTask.Code,
											HRJobApplicationParsingQueueSchema.Constants.TableName,
											new string[] { },
											"Parsing Queue")]
namespace Enterprise.Recruiter.ServiceTasks
{
	public class HRJobApplicationParsingQueueServiceTask : ServiceProviderImpl
	{
		public const string Code = "HPQ";
		protected int BatchSize = 1;

		[HostedServiceRequirement]
		public static string CheckDaxtraEnabled()
		{
			if (RecruiterDataRegistry.Instance.DaxtraEnable.Value)
			{
				return string.Empty;
			}

			return (NoResString)"This service requires Daxtra to be enabled"; // information for logging only.
		}

		BusinessObjectFactory DeleteFactory
		{
			get
			{
				if (deleteFactory == null)
				{
					deleteFactory = new BusinessObjectFactory(); // Create new Factory to garantee that all items get deleted
					deleteFactory.RefreshEnabled = false;
				}
				return deleteFactory;
			}
		}
		BusinessObjectFactory deleteFactory;

		#region ListsToProcess
		readonly List<ZGuid> forceToDeleteItems = new List<ZGuid>();
		readonly List<HRJobApplication> itemsToForceSave = new List<HRJobApplication>();
		readonly List<KeyValuePair<string, string>> itemsWithError = new List<KeyValuePair<string, string>>();

		void ProcessLists()
		{
			ProcessItemsWithError();
			ProcessItemsToForceSave();
			ProcessForceToDeleteItems();
			ResetListsToProcess();
		}

		void ProcessItemsWithError()
		{
			foreach (var item in itemsWithError)
			{
				ErrorReporter.ReportOnce(item.Key, item.Value);
			}
		}

		void ProcessItemsToForceSave()
		{
			if (itemsToForceSave.Count > 0)
			{
#if DEBUG
				VerifyApplicationsNotSavedForTest?.Invoke(null, new VerifyApplicationsForTestEventArgs(itemsToForceSave));
#endif
				Log(LogType.Information, Res.GetString("0E0D5556-3B87-45C9-B255-66273422ED42", "Force saving applications"));
				foreach (var item in itemsToForceSave)
				{
					try
					{
						var newFactory = new BusinessObjectFactory();
						var newApplication = newFactory.ImportFromAnotherFactory(item, typeof(HRJobApplication)) as HRJobApplication;
						newApplication.HasChanges = item.HasChanges;
						if (item.Applicant != null)
						{
							var newApplicant = newFactory.ImportFromAnotherFactory(item.Applicant, typeof(HRJobApplicant)) as HRJobApplicant;
							newApplicant.HasChanges = item.Applicant.HasChanges;
						}
						foreach (var doc in item.Documents)
						{
							var docInDb = newFactory.ImportFromAnotherFactory(doc, typeof(HRJobApplicationDocument)) as HRJobApplicationDocument;
							docInDb.HasChanges = item.HasChanges;
						}
#if DEBUG
						if (Globals.IsTest)
						{
							SimulateEmailErrorForTest(newApplication);
						}
#endif
						newFactory.Save();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Log(Res.GetString("E12342CF-35C8-4843-BFD0-812357FBFBAF", "Error force saving application"), ex);
						var key = this.GetType().Name + ".ForceSaveOneByOne." + ex.GetType().Name;
						ErrorReporter.ReportOnce(key, "Error force saving application", ex); // Developer error message
					}
				}
			}
		}

		void ProcessForceToDeleteItems()
		{
			try
			{
				var parsingQueueItems = DeleteFactory.Load<HRJobApplicationParsingQueue>(new ZQuery(HRJobApplicationParsingQueueSchema.PK, forceToDeleteItems));
				if (parsingQueueItems.Length > 0)
				{
					Array.ForEach(parsingQueueItems, a => a.Delete());
					DeleteFactory.Save();
					Log(LogType.Information, Res.GetString("0DCC5AF2-10FD-4930-B027-3865B30E8140", "Forcibly deleted {0} queue items", parsingQueueItems.Length));
				}

#if DEBUG
				if (Globals.IsTest)
				{
					ForceDeleteParsingQueueItemsErrorForTest();
				}
#endif
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log(Res.GetString("14C15D2F-1A46-4CA8-A040-84200526534E", "Error deleting queue items"), ex);
				var key = this.GetType().Name + ".ForceDeleteParsingQueueItems." + ex.GetType().Name;
				ErrorReporter.ReportOnce(key, "Error deleting queue items", ex); // Developer error message
			}
		}

		void ResetListsToProcess()
		{
			forceToDeleteItems.Clear();
			itemsToForceSave.Clear();
			itemsWithError.Clear();
		}

		#endregion

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			using (Env.Instance.SuspendBranchAccessError())
			{
				var totalDocsProcessed = 0L;

				var reader = new FilteredBusinessObjectReader(new ZQuery(), typeof(HRJobApplicationParsingQueue));
				reader.BatchSize = BatchSize;

				var batch = reader.LoadNextBatchInANewFactory(ZGuid.Empty); 
				while (batch.Any())
				{
					ProcessBatch(batch, reader);

					totalDocsProcessed += batch.Length;
#if DEBUG
					if (Globals.IsTest)
					{
						LogBatchSizeForTest(batch.Length);
					}
#endif

					if (batch.Length == BatchSize)
					{
						if (youMustReactToThisToken.IsCancellationRequested)
						{
							Log(LogType.Information, Res.GetString("366DD8FF-95D1-4D1D-BC87-863ED8B3FFFB", "Stopped processing because of cancellation"));
							break;
						}

						batch = reader.LoadNextBatchInANewFactory(batch[batch.Length - 1]);
					}
					else
					{
						break;
					}
				}

				Log(LogType.Information, Res.GetString("C1259998-E55F-4260-9BAA-9AFFC8EC4633", "Processed {0} queued document(s)", totalDocsProcessed));
			}
		}

		#region ProcessBatch

		void ProcessBatch(BusinessObject[] batch, FilteredBusinessObjectReader reader)
		{
			foreach (HRJobApplicationParsingQueue item in batch)
			{
				ProcessParsingQueue(item);
			}

			TrySaveCurrentRecordsInReader(reader);
		}

		void ProcessParsingQueue(HRJobApplicationParsingQueue parsingQueue)
		{
			var application = parsingQueue.Application;

			try
			{
				if (application != null)
				{
					ProcessApplication(application, parsingQueue);
				}
				else
				{
					Log(LogType.Warning, Res.GetString("F10221A3-6A3E-4B3F-9045-52933D25E0EB", "Job application not found. PK: {0}", parsingQueue.HPQ_HP));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogCriticalException(ex, application);
			}
			finally
			{
				SortApplicationIntoListToProcess(application);
				forceToDeleteItems.Add(parsingQueue.PK);
				parsingQueue.Delete();
			}
		}

		void ProcessApplication(HRJobApplication application, HRJobApplicationParsingQueue parsingQueue)
		{
			Log(LogType.Information, Res.GetString("8397E35F-5AAA-402B-879B-B97574C6BDAD", "Job application for '{0}' ({1})", application.Applicant?.HA_FullName, application.Applicant?.HA_EmailAddress)); // Log message
#if DEBUG
			if (Globals.IsTest)
			{
				CreateErrorForTest(application);
			}
#endif
			var doc = application.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(f => f.UniqueKey == parsingQueue.HPQ_StorageDocReference);
			if (doc != null)
			{
				ProcessDocument(doc, application);
			}
			else
			{
				Log(LogType.Warning, Res.GetString("5E849AB3-23FD-4BC3-A0F1-78BCDE816CC7", "Document not found. Storage doc reference: {0}", parsingQueue.HPQ_StorageDocReference));
			}

			var resumeConverter = ObjectFactory.Get<IResumeConverter>(nameof(IResumeConverter), ServiceLogger);
			resumeConverter.ConvertAvailableResumes(application);
		}

		void ProcessDocument(IeDoc doc, HRJobApplication application)
		{
			Log(LogType.Information, Res.GetString("929D9470-91E0-4834-BAC3-1BFC899CEA64", "Processing document '{0}'", doc.FileName));

			using var daxtraResumeParser = new DaxtraResumeParser();
			var result = CreateHRJobApplicationEmailParser(application, daxtraResumeParser).ProcessQueueFile(doc.FileName, doc.ImageData, doc.DateAdded.ToDateTime());

			var message = HRJobApplicationEmailParser.ProcessFileResultMessage(result);
			if (!string.IsNullOrEmpty(message))
			{
				Log(LogType.Information, Res.GetString("9450C491-20FE-4CF5-B18A-2621D830B45C", "Parsing notifications: {0}", message));
			}
			else
			{
				Log(LogType.Information, Res.GetString("4C3B9448-52B2-48A1-A4AD-C9BF82E64B20", "Document '{0}' has been successfully processed", doc.FileName));
			}
		}

		protected virtual HRJobApplicationEmailParser CreateHRJobApplicationEmailParser(HRJobApplication application, DaxtraResumeParser daxtraResumeParser)
		{
			return new HRJobApplicationEmailParser(application, daxtraResumeParser);
		}

		void SortApplicationIntoListToProcess(HRJobApplication application)
		{
			if (ApplicationHasErrors(application))
			{
				var key = this.GetType().Name + ".ApplicationErrorMessage";
				itemsWithError.Add(new KeyValuePair<string, string>(key, (NoResString)"Application has errors " + application.PK)); // Developer error message
			}
			else
			{
				itemsToForceSave.Add(application);
			}
		}

		bool ApplicationHasErrors(HRJobApplication application)
		{
			var result = false;

			if (application != null && (application.HasErrors || (application.Applicant != null && application.Applicant.HasErrors)))
			{
				application.CancelChanges();
				application.Applicant?.CancelChanges();

				Log(LogType.Error, Res.GetString("F78E3276-DCD0-4CD1-A6B7-33B00F7BC40E", "Processed with errors: {0}", ConstructErrorMessageForApplication(application)));
				result = true;
			}

			return result;
		}

		string ConstructErrorMessageForApplication(HRJobApplication application)
		{
			var errorMsg = string.Empty;
			var errorMessageApplication = application.NotificationsIncludingChildren.Where(n => n.Type == CargoWise.ComponentModel.NotificationType.Error).Select(n => n.Message);
			if (errorMessageApplication.Any())
			{
				errorMsg = string.Join("\n", errorMessageApplication);
			}

			var errorMessageApplicant = application.Applicant?.NotificationsIncludingChildren.Where(n => n.Type == CargoWise.ComponentModel.NotificationType.Error).Select(n => n.Message);
			if (errorMessageApplicant.Any())
			{
				errorMsg += string.Join("\n", errorMessageApplicant);
			}

			return errorMsg;
		}

		void TrySaveCurrentRecordsInReader(FilteredBusinessObjectReader reader)
		{
			try
			{
				SaveBatchAndLoadNew(reader);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is SqlException || ex is ZCannotSaveException || ex is ZSaveConcurrencyException)
				{
					ErrorReporter.ReportOnce("Error saving some applications", ex); // Developer error message
				}

				Log(Res.GetString("98AA76B8-3834-4C76-A213-BDFDFB7114DB", "Error saving some applications"), ex);

				ProcessLists();
			}
		}

		protected virtual void SaveBatchAndLoadNew(FilteredBusinessObjectReader reader)
		{
			reader.FactoryProvider.SaveCurrentAndCreateNew();
		}

		#endregion

#if DEBUG
		protected virtual void CreateErrorForTest(HRJobApplication application) { }
		protected virtual void LogBatchSizeForTest(int batchLength) { }
		protected virtual void SimulateEmailErrorForTest(HRJobApplication application) { }

		protected virtual void ForceDeleteParsingQueueItemsErrorForTest() { }

		public event EventHandler<VerifyApplicationsForTestEventArgs> VerifyApplicationsNotSavedForTest;

		public class VerifyApplicationsForTestEventArgs : EventArgs
		{
			public VerifyApplicationsForTestEventArgs(List<HRJobApplication> applications)
			{
				Applications = applications;
			}

			public readonly List<HRJobApplication> Applications;
		}
#endif

		#region Log

		protected void Log(string message, Exception ex)
		{
			ServiceLogger.Log(LogType.Error, message, ex);
		}

		protected void Log(LogType logType, string format, params object[] args)
		{
			Log(logType, false, format, args);
		}

		protected void Log(LogType logType, bool verboseModeOnly, string format, params object[] args)
		{
			if (!verboseModeOnly)
			{
				ServiceLogger.Log(logType, string.Format(CultureInfo.InvariantCulture, format, args));
			}
		}

		void LogCriticalException(Exception ex, HRJobApplication application)
		{
			Log(Res.GetString("B607F86A-76CC-47B6-B4AE-B1F3A5BC69DF", "Error processing document"), ex);

			if (application != null)
			{
				var key = this.GetType().Name + ".ProcessDocument." + ex.GetType().Name;
				ErrorReporter.ReportOnce(key, "Error processing document for application " + application.PK, ex); // Developer error message
			}
		}

		#endregion
	}
}
