using System.Collections.Generic;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Workflow.ValidationAction
{
	class CustomsNotificationCollector : ZNotificationCollector
	{
		internal CustomsNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage) : base(business, includeChildren, includeNotificationTypeInMessage) { }

		internal CustomsNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude) { }

		protected override INotification GetNotification(BusinessObject bizObj, INotification notification, string replacedMessage)
		{
			var bizoName = GetParentChildChainHumanReadableNames();
			if (!string.IsNullOrEmpty(bizoName))
			{
				replacedMessage = bizoName + "\t" + replacedMessage;
			}

			return base.GetNotification(bizObj, notification, replacedMessage);
		}

		string GetParentChildChainHumanReadableNames()
		{
			var resultBuilder = new StringBuilder();

			for (int i = 1; i < ParentChildChain.Count; ++i)
			{
				var business = ParentChildChain[i];
				if (business is IBusinessObjectCollection)
				{
					continue;
				}

				resultBuilder.Append(business.HumanReadableName);
				if (i != ParentChildChain.Count - 1)
				{
					resultBuilder.Append("\t");
				}
			}

			return resultBuilder.ToString();
		}

		protected override IList<IBusiness> ParentChildChain { get { return parentChildChain; } }

		readonly List<IBusiness> parentChildChain = new List<IBusiness>();
	}
}
