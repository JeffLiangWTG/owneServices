using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageSendingObjectParentNotificationCollector : CustomsNotificationCollector
	{
		readonly CusTWControllingMessageHeader[] sendingObjects;

		public LicensingMessageSendingObjectParentNotificationCollector(LicensingMessageSendingObjectCollection sendingObjects, IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude)
			: base(business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
			this.sendingObjects = sendingObjects.Where(sendingObject => sendingObject.ShouldSend).Select(sendingObject => sendingObject.Header).ToArray();
		}

		protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject) => base.ShouldIncludeNotificationsFromObject(businessObject)
			&& !(businessObject is JobComInvoiceLine)
			&& !(businessObject is CusTWControllingMessageHeader header && !sendingObjects.Contains(header));

		protected override bool ShouldIncludeNotificationsFromInfo(ZPropertyInfo info)
		{
			var result = base.ShouldIncludeNotificationsFromInfo(info);
			if (result)
			{
				var bizObj = info.BizObj;
				var infoName = info.Name;
				if (bizObj is JobComInvoiceLine)
				{
					result = !ExcludedNotificationsFromInvoiceLineProperties.Contains(infoName);
				}
				else if (bizObj is JobComInvoiceHeader)
				{
					result = !ExcludedNotificationsFromInvoiceProperties.Contains(infoName);
				}
				else if (bizObj is JobDeclaration)
				{
					result = !ExcludedNotificationsFromDeclarationProperties.Contains(infoName);
				}
				else if (bizObj is CusEntryHeader)
				{
					result = !ExcludedNotificationsFromCusEntryHeaderProperties.Contains(infoName);
				}
			}
			return result;
		}

		#region CusEntryHeader
		ImmutableHashSet<string> ExcludedNotificationsFromCusEntryHeaderProperties => excludedNotificationsFromCusEntryHeaderProperties ??= GetExcludedNotificationsFromCusEntryHeaderPropertiesCore().ToImmutableHashSet();
		ImmutableHashSet<string> excludedNotificationsFromCusEntryHeaderProperties;

		protected virtual IEnumerable<string> GetExcludedNotificationsFromCusEntryHeaderPropertiesCore() => GetExcludedAdditionalNotificationsFromCusEntryHeaderPropertiesCore();

		protected virtual IEnumerable<string> GetExcludedAdditionalNotificationsFromCusEntryHeaderPropertiesCore() => Enumerable.Empty<string>();
		#endregion

		#region InvoiceLine
		ImmutableHashSet<string> ExcludedNotificationsFromInvoiceLineProperties => excludedNotificationsFromInvoiceLineProperties ??= GetExcludedNotificationsFromInvoiceLinePropertiesCore().ToImmutableHashSet();
		ImmutableHashSet<string> excludedNotificationsFromInvoiceLineProperties;

		protected virtual IEnumerable<string> GetExcludedNotificationsFromInvoiceLinePropertiesCore()
		{
			yield return AutoTWJobComInvoiceLine.Schema.JI_TextileWidth;
			yield return AutoTWJobComInvoiceLine.Schema.JI_TextileWidthUQ;
			foreach (var excludedNotification in GetExcludedAdditionalNotificationsFromInvoiceLinePropertiesCore())
			{
				yield return excludedNotification;
			}
		}

		protected virtual IEnumerable<string> GetExcludedAdditionalNotificationsFromInvoiceLinePropertiesCore() => Enumerable.Empty<string>();
		#endregion

		#region Invoice Header
		ImmutableHashSet<string> ExcludedNotificationsFromInvoiceProperties => excludedNotificationsFromInvoiceProperties ??= GetExcludedNotificationsFromInvoicePropertiesCore().ToImmutableHashSet();
		ImmutableHashSet<string> excludedNotificationsFromInvoiceProperties;

		protected virtual IEnumerable<string> GetExcludedNotificationsFromInvoicePropertiesCore() => GetExcludedAdditionalNotificationsFromInvoicePropertiesCore();

		protected virtual IEnumerable<string> GetExcludedAdditionalNotificationsFromInvoicePropertiesCore() => Enumerable.Empty<string>();
		#endregion

		#region Declaration
		ImmutableHashSet<string> ExcludedNotificationsFromDeclarationProperties => excludedNotificationsFromDeclarationProperties ??= GetExcludedNotificationsFromDeclarationPropertiesCore().ToImmutableHashSet();
		ImmutableHashSet<string> excludedNotificationsFromDeclarationProperties;

		protected virtual IEnumerable<string> GetExcludedNotificationsFromDeclarationPropertiesCore() => GetExcludedAdditionalNotificationsFromDeclarationPropertiesCore();

		protected virtual IEnumerable<string> GetExcludedAdditionalNotificationsFromDeclarationPropertiesCore() => Enumerable.Empty<string>();
		#endregion
	}
}
