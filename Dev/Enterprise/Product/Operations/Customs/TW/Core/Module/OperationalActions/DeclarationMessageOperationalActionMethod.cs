using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class DeclarationMessageOperationalActionMethod : OperationalActionMethod
	{
		public DeclarationMessageOperationalActionMethod()
			: base(new ZGuid("D90ED221-131B-47C4-A85A-13FD7DE654FE"))
		{
		}

		public override string Name => Res.GetString("9F703153-267D-4715-9294-48AC07EB4D7C", "Configuration");

		public override string Description => OperationalActionLogAndUserNotificationWrapper.Constants.SubmitOriginalEntryToTaiwanCustoms;

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new DeclarationMessageOperationalActionMethodApplicator();

		public override bool IsRunAgainDisabled => false;

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new SendOriginalEntryConfigurationControl();

		public override FilterRequirementList GetFilterRequirements()
		{
			var result = base.GetFilterRequirements();
			result.Add(FilterConstants.Country, new string[] { CoreConstants.CountryCodes.Taiwan });
			return result;
		}
	}
}
