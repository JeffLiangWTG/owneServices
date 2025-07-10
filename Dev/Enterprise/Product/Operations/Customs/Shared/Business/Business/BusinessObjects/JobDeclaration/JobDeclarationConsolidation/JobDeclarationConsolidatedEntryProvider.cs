using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationConsolidatedEntryProvider
	{
		public JobDeclarationConsolidatedEntryProvider(BaseJobDeclaration declaration, IConsolidatedEntryDeclarationRemover consolidatedEntryDeclarationRemover = null)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			factory = declaration.Factory;
			ConsolidatedEntryDeclarationRemover = consolidatedEntryDeclarationRemover;
		}

		protected BaseJobDeclaration declaration;
		readonly BusinessObjectFactory factory;
		IConsolidatedEntryDeclarationRemover ConsolidatedEntryDeclarationRemover { get; }

		public string CannotDetermineConsolidatedDeclarationStatus => Res.GetString("F0CC5387-7EF0-4E31-B0AB-5F586CC248E8", "This entry is in the process of Consolidation and cannot be submitted at this time.");

		public bool CanRemove => ConsolidatedEntryDeclarationRemover?.CanRemove(this, declaration) ?? false;

		public virtual ZString ConsolidationStatus
		{
			get
			{
				var result = declaration.JE_EntryStatus;
				if (!result.IsEmpty && !ConsolidationStatusList.ContainsCode(result))
				{
					result = ZString.Empty;
				}
				return result;
			}
			protected set
			{
				if (value.IsEmpty || ConsolidationStatusList.ContainsCode(value))
				{
					declaration.JE_EntryStatus = value;
				}
			}
		}

		protected ConsolidatedEntryStatusList ConsolidationStatusList => consolidationStatusList ?? (consolidationStatusList = new ConsolidatedEntryStatusList());
		ConsolidatedEntryStatusList consolidationStatusList;

		public bool CanConsolidateEntry
		{
			get { return ConsolidationStatus.IsEmpty && !HasMessages; }
		}

		public bool IsConsolidated
		{
			get { return ConsolidatedDeclaration.IsConsolidated(declaration); }
		}

		public bool IsQueuedForConsolidation
		{
			get { return ConsolidationStatus == ConsolidatedEntryStatusList.Codes.ReadyForConsolidation; }
		}

		public void ApplyForConsolidation()
		{
			ConsolidationStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
		}

		public bool QueueForConsolidation(out string message)
		{
			if (!ValidateDeclaration(out var errorMessage) || !QueueForConsolidationValidation(out errorMessage))
			{
				message = errorMessage;
				return false;
			}

			using (var sqlLock = LockDeclarationForConsolidation())
			{
				if (sqlLock?.IsHoldingLock() ?? false)
				{
					if (CanConsolidateEntry)
					{
						ConsolidationStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
						return SaveAndSetMessage(Res.GetString("47D11855-0604-4E23-9389-A19243DF3A25", "Job queued for Consolidation."), out message);
					}

					message = Res.GetString("B29EF6AD-3DEA-4871-8204-676DAB98C0B4", "Unable to Queue for Consolidation at this time.");
					return false;
				}

				message = CannotDetermineConsolidatedDeclarationStatus;
				return false;
			}
		}

		public bool DequeueOrRemoveFromConsolidation(out string message)
		{
			var result = false;
			if (ValidateDeclaration(out message))
			{
				using (var sqlLock = LockDeclarationForConsolidation())
				{
					if (sqlLock?.IsHoldingLock() ?? false)
					{
						if (IsQueuedForConsolidation)
						{
							result = DequeueFromConsolidation(out message);
						}
						else if (CanRemove)
						{
							result = RemoveFromConsolidation(out message);
						}
						else
						{
							message = Res.GetString("33BA252A-2305-477E-8533-2A7505BD13D0", "Unable to Dequeue from Consolidation at this time.");
						}
					}
					else
					{
						message = CannotDetermineConsolidatedDeclarationStatus;
					}
				}
			}

			return result;
		}

		public SqlApplicationLock LockDeclarationForConsolidation()
		{
			SqlApplicationLock sqlLock = null;

			try
			{
				var key = ZString.Format("JobDeclarationConsolidation_{0}", declaration.PK.ToString());
				var dbConnection = ((IDbConnected)factory).Connection;
				if (dbConnection.TryGetLock(key, out sqlLock))
				{
					ReloadEntryStatus();
				}
			}
			catch (Exception)
			{
				sqlLock?.Dispose();
				throw;
			}

			return sqlLock;
		}

		bool HasMessages => declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(h => h.Messages.Any());

		void ReloadEntryStatus()
		{
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load("SELECT JE_EntryStatus FROM dbo.JobDeclaration WHERE JE_PK = @JobDeclarationPK",
				new ZSqlParameterCollection() { ZSqlParameter.New("@JobDeclarationPK", declaration.PK, JobDeclarationSchema.PK) });

			var bizo = collection.Single();
			using (declaration.SuspendSettingHasChanges())
			{
				declaration.JE_EntryStatus = (ZString)bizo[JobDeclarationSchema.JE_EntryStatus];
			}
		}

		bool ValidateDeclaration(out string message)
		{
			message = string.Empty;

			if (declaration.HasChanges || (declaration.Shipment?.HasChanges ?? false))
			{
				message = Res.GetString("5C35C411-1535-469F-8A57-96B83FCB0A01", "You must save the current Declaration details before submitting.");
				return false;
			}

			return true;
		}

		bool QueueForConsolidationValidation(out string message)
		{
			message = string.Empty;
			if (declaration.Shipment?.Job?.JH_GB.IsEmpty ?? false)
			{
				message = Res.GetString("8BAEA5CA-4651-45EC-BF5A-CD14C8C1D149", "The Invoice must have a proper Branch.");
				return false;
			}

			var result = false;
			using (BeforeQueueForConsolidationValidation())
			{
				result = MergeDeclaration() && MessageSendingDeclarationValidation();
			}
			return result;
		}

		protected virtual IDisposable BeforeQueueForConsolidationValidation()
		{
			return null;
		}

		bool MergeDeclaration()
		{
			return (declaration.IsMergeDone && !declaration.MergeManager.RequiresMerge) || declaration.DoMerge();
		}

		bool MessageSendingDeclarationValidation()
		{
			var validator = MessageSendingValidation.New(declaration, null);
			return validator.CheckBusinessObjectLevelValidation(declaration.MessageInitiator);
		}

		bool DequeueFromConsolidation(out string message)
		{
			ConsolidationStatus = string.Empty;
			return SaveAndSetMessage(Res.GetString("86B196C1-E9DF-45D1-97D4-BBE23650B416", "Job is no longer queued for Consolidation."), out message);
		}

		bool RemoveFromConsolidation(out string message)
		{
			var result = ConsolidatedEntryDeclarationRemover.RemoveFromConsolidation(declaration, out message);
			if (result)
			{
				ConsolidationStatus = string.Empty;
				result = SaveAndSetMessage(message, out message);
			}

			return result;
		}

		bool SaveAndSetMessage(string successMessage, out string resultMessage)
		{
			try
			{
				factory.Save();
				resultMessage = successMessage;
				return true;
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				resultMessage = null;
				return false;
			}
		}
	}
}
