using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business.Accounting.Helpers
{
	public interface IDataRefreshBusUpdateActionDecider
	{
		bool ShouldApplyDataRefreshBusUpdate(DataRefreshAction action, BusinessObject subscriber, BusinessObject publisher, ZPropertyInfo[] strictPropertyInfo);
		bool HasSkippedDataRefreshBusUpdate(BusinessObject businessObject);
		void ValidateDataRefreshBusChanges(BusinessObject businessObject);
		void RemoveSkipDataRefreshBusUpdateBusinessContexts(BusinessObject businessObject);
		bool IsStrictPropertyChangedOnSubscriberOrPublisher(BusinessObject publisher, params ZPropertyInfo[] strictPropertyInfo);
	}

	public class DataRefreshBusUpdateActionDecider : IDataRefreshBusUpdateActionDecider
	{
		bool IDataRefreshBusUpdateActionDecider.ShouldApplyDataRefreshBusUpdate(DataRefreshAction action, BusinessObject subscriber, BusinessObject publisher, ZPropertyInfo[] strictPropertyInfo) => ShouldApplyDataRefreshBusUpdate(action, subscriber, publisher, strictPropertyInfo);

		bool IDataRefreshBusUpdateActionDecider.HasSkippedDataRefreshBusUpdate(BusinessObject businessObject) => HasSkippedDataRefreshBusUpdate(businessObject);

		void IDataRefreshBusUpdateActionDecider.ValidateDataRefreshBusChanges(BusinessObject businessObject) => ValidateDataRefreshBusChanges(businessObject);

		void IDataRefreshBusUpdateActionDecider.RemoveSkipDataRefreshBusUpdateBusinessContexts(BusinessObject businessObject) => RemoveSkipDataRefreshBusUpdateBusinessContexts(businessObject);

		bool IDataRefreshBusUpdateActionDecider.IsStrictPropertyChangedOnSubscriberOrPublisher(BusinessObject publisher, params ZPropertyInfo[] strictPropertyInfo) => IsStrictPropertyChangedOnSubscriberOrPublisher(publisher, strictPropertyInfo);

		static bool ShouldApplyDataRefreshBusUpdate(DataRefreshAction action, BusinessObject subscriber, BusinessObject publisher, ZPropertyInfo[] strictPropertyInfo)
		{
			var shouldNotApply = false;
			switch (action)
			{
				case DataRefreshAction.UpdateDeletedSubscriberWhenPublisherUpdated:
				case DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherDeleted:
				case DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated:
				case DataRefreshAction.UpdateDeletedSubscriberWhenPublisherDeleted:
					shouldNotApply = IsDataRefreshBusUpdateNotAllowed(subscriber, publisher, strictPropertyInfo);
					break;
				case DataRefreshAction.None:
					shouldNotApply = false;
					break;
				default:
					ErrorReporter.ReportOnce(Invariant($"Unexpected DataRefreshAction value {action}. Please implemented processing logic for it here."));
					break;
			}

			if (shouldNotApply)
			{
				if (!subscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange))
				{
					subscriber.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
				}
				if (!subscriber.IsDeleted)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(subscriber.Factory).AddInfoOnceWhenAllowed(subscriber.PK
					, key: CriticalValidationInfoCollectorServiceKeyType.DataRefreshBusUpdateSkipped
					, generateDebugInfoWhenRequired: () =>
						{
							string subscriberProperties;

								using (!subscriber.Factory.ThreadSentry.IsOwner ? subscriber.Factory.ThreadSentry.SuppressReporting() : null)
								{
									subscriberProperties = subscriber.GetAllPropertyValues();
								}

								var message = new ZStringBuilder();
								message.AppendLine((NoResString)"Subscriber: " + subscriberProperties);
								message.AppendLine((NoResString)"Publisher: " + publisher.GetAllPropertyValues());
								message.AppendLine("StackTrace:");
								message.AppendLine(System.Environment.StackTrace);
								return message.ToString();
							}
						, collectionFrequency: CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
				}
			}
			else if (subscriber is IShouldSkipDataRefreshUpdateForDeletedSubscriber && subscriber.IsDeleted && !publisher.IsDeleted && GetPropertiesUpdatedInPublisher(publisher, strictPropertyInfo).Any())
			{
				subscriber.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToSubscriberIsDeleted);
			}

			return !shouldNotApply;
		}

		static bool HasSkippedDataRefreshBusUpdate(BusinessObject businessObject)
		{
			return businessObject.HasAnyOfContexts(BusinessContextSets.GetSkipDataRefreshBusUpdateSet());
		}

		static void ValidateDataRefreshBusChanges(BusinessObject businessObject)
		{
			var error = Res.GetString("7c84fe10-b0f1-4116-90d3-60ab7786830d", "This record was modified by this user during another operation. Please cancel your changes and reload the form.");
			businessObject.RemoveRowError(error);
			if (HasSkippedDataRefreshBusUpdate(businessObject))
			{
				businessObject.AddRowError(error);
			}
		}

		static void RemoveSkipDataRefreshBusUpdateBusinessContexts(BusinessObject businessObject)
		{
			businessObject.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
			businessObject.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToSubscriberIsDeleted);
		}

		static bool IsStrictPropertyChangedOnSubscriberOrPublisher(BusinessObject publisher, params ZPropertyInfo[] strictPropertyInfo)
		{
			var anyStrictPorpertyChange = strictPropertyInfo.Any(x => x.HasChanges);
			return anyStrictPorpertyChange || GetPropertiesUpdatedInPublisher(publisher, strictPropertyInfo).Any();
		}

		static bool IsDataRefreshBusUpdateNotAllowed(BusinessObject subscriber, BusinessObject publisher, ZPropertyInfo[] strictPropertyInfo)
		{
			var strictPropertiesToBeUpdatedByDataRefreshBus = Array.Empty<ZPropertyInfo>();

			return (publisher.IsDeleted || MadeImportantChanges()) && (subscriber.IsDeleted || HasImportantChanges());

			bool MadeImportantChanges()
			{
				strictPropertiesToBeUpdatedByDataRefreshBus = GetPropertiesUpdatedInPublisher(publisher, strictPropertyInfo).ToArray();

				return strictPropertiesToBeUpdatedByDataRefreshBus.Any();
			}

			bool HasImportantChanges()
			{
				if (subscriber is JobCharge charge)
				{
					return charge.HasChargeChanged();
				}
				else
				{
					return ((INeedRow)subscriber).Row.RowState == System.Data.DataRowState.Modified;
				}
			}
		}

		static IEnumerable<ZPropertyInfo> GetPropertiesUpdatedInPublisher(BusinessObject publisher, ZPropertyInfo[] strictPropertyInfo)
		{
			var isDeleted = strictPropertyInfo.FirstOrDefault()?.BizObj.IsDeleted ?? false;
			return (from info in strictPropertyInfo
					let publisherInfo = publisher.FindPropertyInfo(info.Name)
					let infoValue = isDeleted ? info.OriginalValue : info.Value
					where publisherInfo != null && !(infoValue == null ? publisherInfo.Value == null : infoValue.Equals(publisherInfo.Value))
					select info);
		}
	}
}
