using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgHandlingUnitDocumentSupporter))]
	class PkgHandlingUnitDocumentSupporterTest : DocumentSupporterTest
	{
		#region TestShowShowReasonForNotPrinting

		public void TestShowShowReasonForNotPrinting()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var docSupporter = ((IDocumentSupportable)handlingUnit).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			var handlingUnit = Factory.New<PkgHandlingUnit>();
			var docSupporter = ((IDocumentSupportable)handlingUnit).DocumentSupporter;
			AssertEquals(Env.Security.PkgHandlingUnitCustomizeDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGenericNewPackageID

		public void TestGenericNewPackageID()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (branch.SetAsTemporaryContext()) // required to set registration key
			{
				var handlingUnit = Factory.New<PkgHandlingUnit>();
				handlingUnit.KPU_JobContext = "TWH";
				handlingUnit.KPU_GB_Branch = branch.PK;
				var docSupporter = ((IDocumentSupportable)handlingUnit).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packages = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertEquals("Should generate a new package id", "HU-001", packages.FirstOrDefault()["RefNumber"]);
			}
		}

		#endregion

		#region Implementation

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			base.DoSetupForDocument(command, documentSupportableBO);

			var handlingUnit = (PkgHandlingUnit)documentSupportableBO;
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			packageJob.Packages.AddNew();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_JobContext = "TWH";
			handlingUnit.KPU_GB_Branch = branch.PK;

			return handlingUnit;
		}

		#endregion
	}
}
