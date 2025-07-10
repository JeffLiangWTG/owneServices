using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	public class EnterpriseStaffAndContactAuthorizationPolicy : EnterpriseAuthorizationPolicyBase
	{
		protected override string GetStaffUsername(string heartbeatId)
		{
			ZGuid heartbeatPk;
			if (!ZGuid.TryParse(heartbeatId, out heartbeatPk))
			{
				throw new ArgumentException("not a valid GUID", nameof(heartbeatId));
			}

			var user = EnterpriseUserNamePasswordValidatorBase.LoadUserFromHeartBeat(heartbeatPk, allowStaffOnly: false, loadNameOnly: true)
				?? throw new InvalidOperationException("Credentials passed authentication but no user found during authorization");

			var userName = (ZString)user[GlbStaffSchema.GS_LoginName];
			return userName.ToString();
		}

		protected override Type GetValidatorType()
		{
			return typeof(EnterpriseStaffAndContactValidator);
		}
	}
}
