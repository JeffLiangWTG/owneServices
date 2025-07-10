using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.Module
{
	public abstract class PRABaseActionMethod : OperationalActionMethod
	{
		protected PRABaseActionMethod(ZGuid methodID)
			: base(methodID) { }

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[] { Env.Security.ForwardingContainerPRAMessagingAU };
		}

		public override bool HasSettings
		{
			get { return true; }
		}
		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new PRASettings(factory);
		}

		public override IComponent NewSettingsControl()
		{
			return new PRASettingsControl();
		}

		public override FilterRequirementList GetFilterRequirements()
		{
			FilterRequirementList result = base.GetFilterRequirements();
			result.Add(FilterConstants.Country, new string[] { Constants.CountryCodes.Australia });
			return result;
		}
	}
}
