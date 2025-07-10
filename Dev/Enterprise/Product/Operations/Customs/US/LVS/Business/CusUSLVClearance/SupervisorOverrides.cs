using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.LVS.Business
{
	public class SupervisorOverrides : Customs.Business.SupervisorOverrides
	{
		public SupervisorOverrides(IBusiness businessEntity, string context)
			: base(businessEntity, context)
		{
		}

		protected override void CheckMessageErrors(IBusiness entity)
		{
			if (CheckSecurityRightForCheckpointRequired(Env.Security.AllowMessageErrors))
			{
				if (entity is CusUSLVClearance cusUSLVClearance)
				{
					var consignmentsToSend = cusUSLVClearance.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Where(x => x.SendToCustoms);
					if (consignmentsToSend.Any(x => new CustomsNotificationCollector(x.Consignment, true, false).HasMessageErrors()))
					{
						AddMessageLog(Env.Security.AllowMessageErrors, Constants.DeclarationHasAnyMessageErrors);
					}
				}
				else if (entity is CusUSLVConsignment consignment)
				{
					if (consignment != null && new CustomsNotificationCollector(consignment, true, false).HasMessageErrors())
					{
						AddMessageLog(Env.Security.AllowMessageErrors, Constants.DeclarationHasAnyMessageErrors);
					}
				}
				else if (entity is CusUSLVClearanceMessageWrapper clearanceWrapper)
				{
					var consignmentsToSend = clearanceWrapper.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Where(x => x.SendToCustoms);
					if (consignmentsToSend.Any(x => new CustomsNotificationCollector(x.Consignment, true, false).HasMessageErrors()))
					{
						AddMessageLog(Env.Security.AllowMessageErrors, Constants.DeclarationHasAnyMessageErrors);
					}
				}
			}
		}
	}
}
