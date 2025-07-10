using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class ModuleToDeclarationModuleSenderTest : TestCaseWithFactory
	{
		public void TestCreateJob()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var sender = new ModuleToDeclarationModuleSender();
			var publishResult1 = sender.CreateJob<DummyWithWorkflow>(null);
			AssertNull(publishResult1.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult1.ResultType);
			AssertEquals(@"Failed to create a Job:
Null Entity", publishResult1.ErrorMessage);

			var data = Factory.New<DummyWithWorkflow>();
			data.UseRealShipmentForInternalUniversalXMLSending = true;
			var publishResult2 = sender.CreateJob(data);
			AssertNotNull(publishResult2.FindJobIfExists());
			AssertEquals(UniversalResult.Internal, publishResult2.ResultType);
			AssertEquals("", publishResult2.ErrorMessage);
		}

		sealed class DummyWithWorkflow : MasterFiles.Business.Testing.DummyWithWorkflow, IModuleToModule
		{
			public DummyWithWorkflow(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			IOrgHeader IModuleToModule.RecipientOrganisation => GlbCompany.CurrentCompany.OrgProxy;

			BusinessObject IModuleToModule.GetRelatedObject() => null;

			bool IModuleToModule.CanExportData(out ZString errorMessage)
			{
				errorMessage = ZString.Empty;
				return true;
			}

			void IModuleToModule.AddToRelatedJobs(BusinessObject loadedJob)
			{
			}
		}
	}
}
