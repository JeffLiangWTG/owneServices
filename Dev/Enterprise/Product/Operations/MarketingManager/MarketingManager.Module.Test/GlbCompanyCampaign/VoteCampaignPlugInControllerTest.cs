using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(VoteCampaignPlugInController))]
	class VoteCampaignPlugInControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(VoteCampaignPlugInController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.VoteCampaignPlugIn;
		}
	}
}
