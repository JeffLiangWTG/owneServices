using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class MultiPersonMerger
	{
		protected virtual TaskScheduler Scheduler => ObjectFactory.Get<TaskScheduler>();
		readonly GlbPerson retainedPerson;
		readonly IMultiPersonMergerCore mergerCore;
		readonly PersonMergeBusinessObjectCollection dissolvedCollection;
		readonly PersonMergeBusinessObjectCollection retainedCollection;
		readonly PersonMergeBusinessObjectFactoryLoader businessObjectFactoryLoaderProvider;
		protected List<Exception> CaughtExceptions { get; private set; }

		public event EventHandler<MergeProgressEventArgs> MergeProgressNotification;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This Exception message is not visible to the user")]
		public const string FailedMergeExceptionMessage = "Exceptions occured during one or more merges";

		protected MultiPersonMerger(PersonMergeBusinessObjectCollection retainedCollection, PersonMergeBusinessObjectCollection dissolvedCollection, IMultiPersonMergerCore mergerCore, PersonMergeBusinessObjectFactoryLoader businessObjectFactoryLoaderProvider)
		{
			Argument.NotNull(businessObjectFactoryLoaderProvider, nameof(businessObjectFactoryLoaderProvider));
			Argument.NotNull(retainedCollection, nameof(retainedCollection));
			Argument.NotNull(dissolvedCollection, nameof(dissolvedCollection));
			Argument.NotNull(mergerCore, nameof(mergerCore));

			this.businessObjectFactoryLoaderProvider = businessObjectFactoryLoaderProvider;
			this.dissolvedCollection = dissolvedCollection;
			this.retainedCollection = retainedCollection;
			retainedPerson = retainedCollection[0].Person;
			this.mergerCore = mergerCore;
		}

		public MultiPersonMerger(PersonMergeBusinessObjectCollection retainedCollection, PersonMergeBusinessObjectCollection dissolvedCollection, IMultiPersonMergerCore mergerCore)
			: this(retainedCollection, dissolvedCollection, mergerCore, new PersonMergeBusinessObjectFactoryLoader())
		{
		}

		public MultiPersonMerger(PersonMergeBusinessObjectCollection retainedCollection, PersonMergeBusinessObjectCollection dissolvedCollection)
			: this(retainedCollection, dissolvedCollection, new MultiPersonMergerCore(), new PersonMergeBusinessObjectFactoryLoader())
		{
		}

		public async Task<bool> MergeSelected()
		{
			CaughtExceptions = new List<Exception>();

			var tokenSource = new CancellationTokenSource();
			var isCompletelyMerge = true;
			var progressCount = 0;
			var notificationEmailSender = new PersonMergeEmailSender(retainedPerson, dissolvedCollection.Cast<PersonMergeBusinessObject>().Select(x => x.Person));

			foreach (PersonMergeBusinessObject dissolveBizO in dissolvedCollection)
			{
				try
				{
					progressCount++;

					OnMergeProgress(progressCount, dissolvedCollection.Count);

					dissolveBizO.MergeStatus = ParticipantStatus.Merging;
					var dissolvePersonPK = dissolveBizO.Person.PK;

					BusinessObjectFactory threadFactory = null;

					var mergeResult = await Task.Factory.StartNew(() =>
					{
						tokenSource.Token.ThrowIfCancellationRequested();

						return ExecuteMerge(dissolveBizO, out threadFactory);
					}, tokenSource.Token, TaskCreationOptions.None, Scheduler);

					threadFactory.TakeThreadOwnership();

					if (mergeResult.IsSuccessful)
					{
						dissolveBizO.Person.RemoveFromRecentItems();

						businessObjectFactoryLoaderProvider.Reload(mergeResult.NewFactory, retainedPerson, mergeResult.DissolvedPerson, mergeResult.OriginalFactory, PersonMergeMode.Multi);

						dissolveBizO.Person.IsDissolving = true;
						dissolveBizO.MergeStatus = ParticipantStatus.Completed;

						retainedCollection[0].ActiveAssociationsInfo.RefreshBinding();
						notificationEmailSender.MarkPersonAsDissolved(dissolvePersonPK);
					}

					isCompletelyMerge &= mergeResult.IsSuccessful;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					SetCaughtExceptions(dissolveBizO, ParticipantStatus.MergedWithErrors, ex);

					isCompletelyMerge = false;
				}
			}

			var mergeNotificationEmail = notificationEmailSender.GetMergedAccountsEmail();
			if (mergeNotificationEmail != null)
			{
				Env.OutgoingMailManager.CreateAndSave(mergeNotificationEmail);
			}

			return isCompletelyMerge;
		}

		void SetCaughtExceptions(PersonMergeBusinessObject personBizO, string mergeStatus, Exception ex)
		{
			personBizO.MergeStatus = mergeStatus;
			personBizO.AddRowMessageError(ex.ToString());

			CaughtExceptions.Add(ex);
		}

		MultiPersonMergerResult ExecuteMerge(PersonMergeBusinessObject dissolvedBizO, out BusinessObjectFactory factory)
		{
			MultiPersonMergerResult mergeResult = new MultiPersonMergerResult();

			using (Db.DisposableActionForDbConnection())
			{
				factory = new BusinessObjectFactory();

				var retained = factory.Load<GlbPerson>(retainedPerson.PK);
				var dissolved = factory.Load<GlbPerson>(dissolvedBizO.Person.PK);

				try
				{
					mergeResult = mergerCore.Merge(retained, dissolved);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					SetCaughtExceptions(dissolvedBizO, ParticipantStatus.FailedWithCriticalError, ex);
				}

				factory.RelinquishThreadOwnership();
			}

			return mergeResult;
		}

		void OnMergeProgress(int progressCount, int progressTotal)
		{
			MergeProgressNotification?.Invoke(this, new MergeProgressEventArgs(progressCount, progressTotal));
		}
	}
}
