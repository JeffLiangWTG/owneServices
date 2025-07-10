using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(SurveyCampaignPlugInController))]
	class SurveyCampaignPlugInControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(SurveyCampaignPlugInController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SurveyCampaignPlugIn;
		}
	}
}
