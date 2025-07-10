using System;
using CargoWise.EntityFramework;
using Enterprise.Recruitment.Module;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module
{
	[TestedType(typeof(CandidateManagementPopupFormController))]
	sealed class CandidateManagementPopupFormControllerTest : ZSingletonControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(CandidateManagementPopupFormController);

		protected override ControllerID GetControllerID() => ControllerIDs.CandidateManagement;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var collection = new CandidateBusinessObjectCollection(Factory);
			return new CandidateModuleBusinessObject(collection);
		}
	}
}
