using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(LearningCentreScaledTestPlugInController))]
	class LearningCentreScaledTestPlugInControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(LearningCentreScaledTestPlugInController);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LearningCentreScaledTestPlugIn;
		}
	}
}
