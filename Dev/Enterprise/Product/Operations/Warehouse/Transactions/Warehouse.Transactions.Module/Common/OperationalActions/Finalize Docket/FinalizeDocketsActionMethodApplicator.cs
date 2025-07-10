using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract partial class FinalizeDocketsActionMethodApplicator<T> : WhsOperationalActionMethodApplicator
		where T : WhsDocket
	{
		protected FinalizeDocketsActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		protected const string OutputTextFormat = "{0} {1} - {2}";

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);

			foreach (T target in targets)
			{
				log.BumpSectionProgress();
				TryToFinaliseDocket(target, log);
			}
		}

		void TryToFinaliseDocket(T target, IOperationalActionSectionLog log)
		{
			var targetLink = GetDocketIdLink(target);
			var otherFactory = new BusinessObjectFactory(); // using separate factory for each docket to minimise amount of reloaded / cached objects in a factory. Also means we don't have to rollback individual dockets if they fail to finalise.
			var targetInOtherFactory = otherFactory.Load<T>(target.PK);

			if (CanTryToFinaliseDocket(targetInOtherFactory, log, targetLink))
			{
				FinaliseDocket(targetInOtherFactory, log, targetLink);
			}
		}

		bool CanTryToFinaliseDocket(T targetInOtherFactory, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			var result = true;
			if (targetInOtherFactory.IsFinalised)
			{
				result = false;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					OutputTextFormat,
					targetInOtherFactory.Description,
					targetLink,
					Res.GetString("66e05bb8-81b7-444e-adb2-6f646a0fc562", "is already finalized."));
			}
			else
			{
				result = CanTryToFinaliseDocketCore(targetInOtherFactory, log, targetLink);
			}
			return result;
		}

		protected virtual bool CanTryToFinaliseDocketCore(T targetInOtherFactory, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			return true;
		}

		protected virtual void CanFinaliseDocketCore(T target)
		{
			target.FinaliseDocketWithoutUserConfirmation();
		}

		protected void FinaliseDocket(T target, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			var factory = target.Factory;
			var targetAsDataImporting = (ISupportDataImporting)target;

			using (new DisposableAction(
			() => targetAsDataImporting.IsImportingData = true, // suppressing printing of documents
			() => targetAsDataImporting.IsImportingData = false))
			{
				target.FinaliseDocketWithoutUserConfirmation();
			}

			UserModifyDocketBeforeFinalisation(factory);

			if (target.IsFinalised)
			{
				SaveAndHandleErrors(() =>
				{
					var saveSucceeded = false;

					try
					{
						factory.Save();
						saveSucceeded = true;
					}
					catch (Exception ex) when (ex is ZConcurrencyCheckFailureException || ex is ZSaveConcurrencyException)
					{
						if (!ProcessSaveException(ex, target, log, targetLink))
						{
							throw;
						}
					}

					if (saveSucceeded)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
							OutputTextFormat,
							target.Description,
							targetLink,
							Res.GetString("ad4726c9-03c3-42a9-96b4-3eef6317da55", "was successfully finalized."));
					}
				}, log, OutputTextFormat, target.Description, targetLink);
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					OutputTextFormat,
					target.Description,
					targetLink,
					Res.GetString("927f9467-b695-4635-8729-2841ab850d17", "could not be auto-finalized. It must be manually finalized."));
			}
		}

		protected abstract bool ProcessSaveException(Exception exception, T target, IOperationalActionSectionLog log, LogControllerLink targetLink);

		partial void UserModifyDocketBeforeFinalisation(BusinessObjectFactory factory);
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Module
{
	using System;
	using Enterprise.ZArchitecture.Environment;

	public partial class FinalizeDocketsActionMethodApplicator<T>
	{
		partial void UserModifyDocketBeforeFinalisation(BusinessObjectFactory factory)
		{
			if (Globals.IsTest && UserModifyDataInDBAction != null)
			{
				UserModifyDataInDBAction(factory);
			}
		}
		public Action<BusinessObjectFactory> UserModifyDataInDBAction;
	}
}

#endif
#endregion
