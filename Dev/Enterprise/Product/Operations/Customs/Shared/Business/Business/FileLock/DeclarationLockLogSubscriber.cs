using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[Serializable]
	public class DeclarationLockLogSubscriber : LogSubscriber
	{
		#region Override

		public override string Name => "DeclarationLockLogSubscriber";

		public override string FriendlyName => (NoResString)"Declaration Lock Log Subscriber";

		public override string[] TableNames => new[] { JobDeclarationSchema.Constants.TableName, CusEntryHeaderSchema.Constants.TableName };

		public override string[] EventTypes => GetEventTypesCore();

		public override bool HasDynamicProperties => true;

		string[] GetEventTypesCore()
		{
			if (eventTypes == null)
			{
				var result = new List<string>();

				var companies = GlbCompany.GetActiveCompanies();

				foreach (var company in companies)
				{
					var registryItemValue = CustomsDataRegistry.Instance.DeclarationLockForEdit.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

					if (registryItemValue.Count > 0)
					{
						result.AddRange(registryItemValue
								.Cast<DeclarationLockConfig>()
								.SelectMany(c => c.EventInfos.Cast<DeclarationEventLockInfo>().Select(d => d.EventType.ToString())));
					}
				}

				eventTypes = result.Distinct().ToArray();
			}

			return eventTypes;
		}

		string[] eventTypes;

		#endregion

		#region Process

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs != null && queuedLogs.Length > 0)
			{
				var groups = queuedLogs.GroupBy(c => c.SJ_ParentID);

				foreach (var logGroup in groups)
				{
					var firstLog = logGroup.First();
					var parent = GetLockParent(firstLog.Factory, firstLog.SJ_ParentTableCode, logGroup.Key);

					if (parent != null)
					{
						using (DisposableEnvironment.ForBranch(parent.BranchPk.ToGuid()))
						{
							var lockConfigs = CustomsDataRegistry.Instance.DeclarationLockForEdit.Value;
							var cusEntryHeader = parent as CusEntryHeader;
							var entryType = cusEntryHeader?.CH_MessageType ?? string.Empty;

							DeclarationEventLockInfo matchingEventLockInfo = null;
							foreach (var config in lockConfigs.Cast<DeclarationLockConfig>().Where(c => c.DeclarationType.EqualsIgnoringCase(parent.DeclarationType)))
							{
								matchingEventLockInfo = config.GetEventLockInfoFromConfigWhichMatchesLog(logGroup, entryType);
								if (matchingEventLockInfo != null)
								{
									parent.LockFile(FriendlyName);
									var declaration = cusEntryHeader?.Declaration;
									if (cusEntryHeader != null && declaration != null)
									{
										LockDeclarationIfConditionsFulfilled(declaration, matchingEventLockInfo, FriendlyName);
									}
									break;
								}
							}
						}
					}
				}
			}
		}

		static void LockDeclarationIfConditionsFulfilled(BaseJobDeclaration declaration, DeclarationEventLockInfo eventLockInfo, string lockReference)
		{
			var lockMode = eventLockInfo.LockConfig.LockMode.ToUpperInvariant();
			if (MatchesConfigAny() || MatchesConfigAll())
			{
				((ICustomsFileParent)declaration).LockFile(lockReference);
			}

			IEnumerable<ICustomsFileParent> GetEntryHeadersAsCustomsFileParents()
			{
				var entryType = eventLockInfo.EntryType;
				var isAllCode = entryType.EqualsIgnoringCase(Core.Constants.Customs.EntryHeaderTypes.Codes.All);
				return isAllCode ? declaration.ActiveEntryHeaders.Cast<ICustomsFileParent>() : declaration.ActiveEntryHeaders.Where(x => x.CH_MessageType.EqualsIgnoringCase(entryType)).Cast<ICustomsFileParent>();
			}

			bool MatchesConfigAny() => lockMode == Core.Constants.Customs.DeclarationLockModes.Codes.Any && GetEntryHeadersAsCustomsFileParents().Any(c => c.IsLocked);
			bool MatchesConfigAll() => lockMode == Core.Constants.Customs.DeclarationLockModes.Codes.All && GetEntryHeadersAsCustomsFileParents().All(c => c.IsLocked);
		}

		protected virtual ICustomsFileParent GetLockParent(BusinessObjectFactory factory, ZString parentTable, ZGuid parentPk)
		{
			switch (parentTable)
			{
				case JobDeclarationSchema.Constants.Prefix:
					{
						return factory.Load<BaseJobDeclaration>(parentPk);
					}

				case CusEntryHeaderSchema.Constants.Prefix:
					{
						return factory.Load<CusEntryHeader>(parentPk);
					}
			}

			return null;
		}

		#endregion
	}
}
