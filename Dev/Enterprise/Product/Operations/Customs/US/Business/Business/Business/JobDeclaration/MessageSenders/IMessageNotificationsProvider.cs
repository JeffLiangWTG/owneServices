using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IMessageNotificationsProvider : IBusiness
	{
		IEnumerable<BusinessObject> ExcludedChildrenAndTheirDescendentsFromMessageNotifications { get; }
		void ExcludeChildAndItsDescendentsFromMessageNotifications(BusinessObject bizObj);
		ZBool HasMessageErrors { get; }
	}
}
