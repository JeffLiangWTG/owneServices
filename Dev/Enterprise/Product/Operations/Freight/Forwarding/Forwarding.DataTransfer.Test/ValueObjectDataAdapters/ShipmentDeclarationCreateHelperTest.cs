using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentDeclarationCreateHelperTest : TestCaseWithFactory
	{
		public void TestShouldImportJobDeclarationForAutoImport()
		{
			AssertEquals("Precondition: autocreatejobdec registry is set to true", true, SystemDataRegistry.Instance.AutoCreateDeclarationWithinShipment.Value);
			AssertEquals("ShouldImportJobDeclaration should be true for automaticImport", true, JobDecHelperForAutoImport.ShouldImportJobDeclaration(DummyBranch));
			AssertEquals("ShouldImportJobDeclaration should be false for manualImport", false, JobDecHelperForManualImport.ShouldImportJobDeclaration(DummyBranch));

			SystemDataRegistry.Instance.AutoCreateDeclarationWithinShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("ShouldImportJobDeclaration should be false for automaticImport", false, JobDecHelperForAutoImport.ShouldImportJobDeclaration(DummyBranch));
		}

		public void TestShouldCreateJobDecIrrespectiveOfCompanies()
		{
			SetUpRegistry();
			AssertEquals("ShouldCreateJobDecIrrespectiveOfCompanies should be true for automatic import", true, JobDecHelperForAutoImport.ShouldCreateJobDecIrrespectiveOfCompanies);
			AssertEquals("ShouldCreateJobDecIrrespectiveOfCompanies should be false for manual import", false, JobDecHelperForManualImport.ShouldCreateJobDecIrrespectiveOfCompanies);

			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Path.Combine("c", "temp"));
			AssertEquals("ShouldCreateJobDecIrrespectiveOfCompanies should be false for automatic import", false, JobDecHelperForManualImport.ShouldCreateJobDecIrrespectiveOfCompanies);
		}

		public void TestLoadBranchFromCode()
		{
			UserContext userContext = JobDecHelperForAutoImport.GetUserContextForBranch(DummyBranch);
			AssertEquals("branch should be dummybranch", DummyBranch.GB_Code, userContext.Branch.Code);
		}

		public void TestGetBranchWithSamePortCode()
		{
			AssertNull("No Branch is returned", JobDecHelperForAutoImport.GetBranchWithSamePortCode(AUPER, DummyBroker));
			AssertNotNull("Branch is returned", JobDecHelperForAutoImport.GetBranchWithSamePortCode(AUPER, Broker));
			AssertEquals("Branch is returned", DummyBranch.GB_Code, JobDecHelperForAutoImport.GetBranchWithSamePortCode(AUMEL, Broker).GB_Code);
			AssertNull("No Branch is returned", JobDecHelperForAutoImport.GetBranchWithSamePortCode(HKHKG, DummyBroker));
		}

		void SetUpRegistry()
		{
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TempPath);
		}

		ShipmentDeclarationCreateHelper JobDecHelperForAutoImport
		{
			get { return jobDecHelperForAutoImport ?? (jobDecHelperForAutoImport = new ShipmentDeclarationCreateHelper(false, Factory)); }
		}
		ShipmentDeclarationCreateHelper jobDecHelperForAutoImport;

		ShipmentDeclarationCreateHelper JobDecHelperForManualImport
		{
			get { return jobDecHelperForManualImport ?? (jobDecHelperForManualImport = new ShipmentDeclarationCreateHelper(true, Factory)); }
		}
		ShipmentDeclarationCreateHelper jobDecHelperForManualImport;

		void SetUpDummyBranch()
		{
			DummyBranch = Factory.New<GlbBranch>();
			DummyBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			DummyBranch.GB_Code = "TTT";
			DummyBranch.GB_BranchName = "LALALA";
			DummyBranch.GB_Address1 = "lalallalalal";
			DummyBranch.GB_RL_NKHomePort = "AUPER";
			DummyBranch.GB_OH_OrgProxy = Broker.PK;
			GlbBranchExtraPorts dummyPort = DummyBranch.ExtraPorts.AddNew();
			dummyPort.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";
			Factory.Save();
		}

		void SetupBroker()
		{
			Broker = Factory.New<OrgHeader>();
			Broker.OH_Code = "BROKER";
			Broker.OH_FullName = "Broker";
			Broker.OH_RL_NKClosestPort = "AUPER";
			Broker.OH_IsBroker = true;
			Broker.MainWebURL.PU_URL = "For match";

			DummyBroker = Factory.New<OrgHeader>();
			DummyBroker.OH_Code = "Dummy";
			Broker.OH_FullName = "Dummy";
			Broker.OH_RL_NKClosestPort = "AUSYD";
			Broker.OH_IsBroker = true;
			Broker.MainWebURL.PU_URL = "For match";
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TempPath = Env.TempPath;
			SetupBroker();
			SetUpDummyBranch();
			AUPER = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUPER");
			AUMEL = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			HKHKG = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "HKHKG");
		}

		ZString TempPath;
		GlbBranch DummyBranch;
		OrgHeader Broker, DummyBroker;
		RefUNLOCO AUPER, AUMEL, HKHKG;
	}
}
