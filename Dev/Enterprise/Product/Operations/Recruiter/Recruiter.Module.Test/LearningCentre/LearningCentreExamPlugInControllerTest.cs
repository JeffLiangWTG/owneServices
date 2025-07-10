using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(LearningCentreExamPlugInController))]
	class LearningCentreExamPlugInControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(LearningCentreExamPlugInController);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LearningCentreExamPlugIn;
		}
	}
}
