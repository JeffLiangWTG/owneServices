using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		public void TestReconDeclarationNoException()
		{
			var createdDec = Factory.New<JobDeclaration>();
			createdDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			createdDec.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Recon;
			createdDec.ReconDeclaration = new ReconDeclaration(createdDec);
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
			try
			{
				AssertNoExceptionThrown(() => controller.ShowFormForNewEntity(createdDec));
			}
			finally
			{
				controller.LastShownForm.Dispose();
			}
		}

		public override Type ControllerToBashType => typeof(JobDeclarationController);

		protected override bool CountryHasExWarehouse => false;
	}
}
