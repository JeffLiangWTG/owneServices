using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerDetention))]
	internal sealed partial class ContainerDetentionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJob()
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_OH_Client = client.PK;
			detention.NC_OH_Principal = principal.PK;

			Factory.Save();

			AssertNull("JobHeader not created yet", detention.Job);

			JobHeader header = new JobHeader.Loader(detention).TryCreateWithMutex();
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AssertNotNull("JobHeader created now", detention.Job);

			Factory.Save();
		}

		public void TestReadOnlynes()
		{
			Detention.NC_OH_Client = Client.PK;
			Detention.NC_OH_Principal = Principal.PK;

			AssertEquals("pre-save: NC_JobNumber", true, Detention.NC_JobNumberInfo.ReadOnly);
			AssertEquals("pre-save: NC_DetentionType", false, Detention.NC_DetentionTypeInfo.ReadOnly);
			AssertEquals("pre-save: NC_OH_Client", false, Detention.NC_OH_ClientInfo.ReadOnly);
			AssertEquals("pre-save: NC_OH_Principal", false, Detention.NC_OH_PrincipalInfo.ReadOnly);

			Factory.Save();

			AssertEquals("post-save: NC_JobNumber", true, Detention.NC_JobNumberInfo.ReadOnly);
			AssertEquals("post-save: NC_DetentionType", true, Detention.NC_DetentionTypeInfo.ReadOnly);
			AssertEquals("post-save: NC_OH_Client", true, Detention.NC_OH_ClientInfo.ReadOnly);
			AssertEquals("post-save: NC_OH_Principal", true, Detention.NC_OH_PrincipalInfo.ReadOnly);
		}

		[RunInExtraTransaction] // needed because we set the next fountain number.
		public void TestJobNumber()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();

			Env.NumberFountains.DetentionInvoiceNumbers(GlbCompany.CurrentCompany.PK.ToGuid()).SetNext(Factory, 100);

			ContainerDetention detention1 = Factory.New<ContainerDetention>();
			detention1.NC_OH_Client = client.PK;
			detention1.NC_OH_Principal = principal.PK;

			ContainerDetention detention2 = Factory.New<ContainerDetention>();
			detention2.NC_OH_Client = client.PK;
			detention2.NC_OH_Principal = principal.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Expected job numbers should have been assigned",
				new string[] { "DI00000100", "DI00000101" },
				new string[] { detention1.NC_JobNumber, detention2.NC_JobNumber });

			detention2.NC_DetentionType = DetentionInvoiceType.Codes.Import;

			ContainerDetention detention3 = Factory.New<ContainerDetention>();
			detention3.NC_OH_Client = client.PK;
			detention3.NC_OH_Principal = principal.PK;

			ContainerDetention detention4 = Factory.New<ContainerDetention>();
			detention4.NC_OH_Client = client.PK;
			detention4.NC_OH_Principal = principal.PK;

			Factory.Saving += delegate
			{ throw new InvalidOperationException(); };

			try
			{
				Factory.Save();
			}
			catch (InvalidOperationException) { }

			AssertContainsExactElementsInAnyOrder("Should not have changed the already allocated job numbers.",
				new string[] { "DI00000100", "DI00000101" },
				new string[] { detention1.NC_JobNumber, detention2.NC_JobNumber });

			AssertContainsExactElementsInAnyOrder("Should not have allocated numbers to the new jobs.",
				new string[] { "", "" },
				new string[] { detention3.NC_JobNumber, detention4.NC_JobNumber });
		}

		[RunInExtraTransaction] // needed because we set the next fountain number.
		public void TestHumanReadableNumber()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();

			Env.NumberFountains.DetentionInvoiceNumbers(GlbCompany.CurrentCompany.PK.ToGuid()).SetNext(Factory, 100);

			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_OH_Client = client.PK;
			detention.NC_OH_Principal = principal.PK;

			CombineAssertions(delegate
			{
				AssertEquals("Container Detention", detention.HumanReadableName);

				Factory.Save();
				AssertEquals("Container Detention (DI00000100)", detention.HumanReadableName);
			});
		}

		public void TestCheckConstraintsForNC_DetentionType()
		{
			var detentionInvoiceType = new DetentionInvoiceType();
			detentionInvoiceType.AddPairIfNotExist(string.Empty, string.Empty);

			foreach (CodeDescriptionPair item in detentionInvoiceType)
			{
				var detention = Factory.NewWithValidTestData<ContainerDetention>();
				detention.NC_DetentionType = item.Code;
				AssertNoExceptionThrown(item.Code, () => Factory.Save());
			}
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, Detention.NC_GC);
		}

		public void TestMovementsAllowNew()
		{
			AssertEquals(false, ((System.ComponentModel.IBindingList)Detention.Movements).AllowNew);
		}

		public void TestContainerDetentionCanDelete()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var detention = Factory.New<ContainerDetention>();
			detention.NC_OH_Client = client.PK;
			detention.NC_OH_Principal = principal.PK;

			Factory.Save();

			AssertNull("JobHeader has not been created yet", detention.Job);
			Assert("Able to delete because no job", detention.CanDelete);
			AssertEquals("", detention.ReasonForNotAbleToDelete);

			var job = new JobHeader.Loader(detention).TryCreateWithMutex();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AssertNotNull("JobHeader created now", detention.Job);
			Assert(detention.Job.CanDelete);
			Assert(detention.CanDelete);
			AssertEquals("", detention.ReasonForNotAbleToDelete);

			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_JH = job.PK;
			Factory.Save();

			var reasonOfContainerDetention = @"Container Detention (DI00000001) cannot be deleted.
This record cannot be deleted.
An Invoicing Job Header (DI00000001) has been created in the company EDI.";
			Assert(!detention.Job.CanDelete);
			Assert(!detention.CanDelete);
			AssertEquals(reasonOfContainerDetention, detention.ReasonForNotAbleToDelete.ToString());

			invoice.AH_JH = ZGuid.Empty;
			Factory.Save();

			Assert(!detention.Job.CanDelete);
			Assert(!detention.CanDelete);
			AssertEquals(reasonOfContainerDetention, detention.ReasonForNotAbleToDelete.ToString());
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestContainerDetentionCanDeleteWithDiffCompany()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();

			Env.NumberFountains.DetentionInvoiceNumbers(GlbCompany.CurrentCompany.PK.ToGuid()).SetNext(Factory, 100);
			var currCompany = GlbCompany.CurrentCompany;
			var otherCompany = GlbCompany.GetDemoCompany(Factory);

			var detention1 = Factory.New<ContainerDetention>();
			detention1.NC_OH_Client = client.PK;
			detention1.NC_OH_Principal = principal.PK;
			detention1.NC_GC = currCompany.PK;

			var detention2 = Factory.New<ContainerDetention>();
			detention2.NC_OH_Client = client.PK;
			detention2.NC_OH_Principal = principal.PK;
			detention2.NC_GC = otherCompany.PK;

			var detention3 = Factory.New<ContainerDetention>();
			detention3.NC_OH_Client = client.PK;
			detention3.NC_OH_Principal = principal.PK;
			detention3.NC_GC = otherCompany.PK;

			Factory.Save();

			AssertNull("JobHeader has not been created yet", detention1.Job);
			AssertEquals("Able to delete because no job", true, detention1.CanDelete);
			AssertEquals(string.Empty, detention1.ReasonForNotAbleToDelete);
			AssertEquals("EDI", detention1.Company.GC_Code);

			AssertNull("JobHeader has not been created yet", detention2.Job);
			AssertEquals("Able to delete because no job", true, detention2.CanDelete);
			AssertEquals(string.Empty, detention2.ReasonForNotAbleToDelete);
			AssertEquals("DEM", detention2.Company.GC_Code);

			AssertNull("JobHeader has not been created yet", detention3.Job);
			AssertEquals("Able to delete because no job", true, detention3.CanDelete);
			AssertEquals(string.Empty, detention3.ReasonForNotAbleToDelete);
			AssertEquals("DEM", detention3.Company.GC_Code);

			var job1 = new JobHeader.Loader(detention1).TryCreateWithMutex(detention1.Company.FirstActiveBranch);
			var job2 = new JobHeader.Loader(detention2).TryCreateWithMutex(detention2.Company.FirstActiveBranch);

			AssertNotNull("JobHeader created now", detention1.Job);
			AssertEquals("Detention Can be Deleted Bacause Job has been Created but not in Database", true, detention1.CanDelete);
			AssertEquals(string.Empty, detention1.ReasonForNotAbleToDelete);

			AssertNull("JobHeader is null Because not Created by current Company", detention2.Job);
			AssertEquals("Detention Can not Deleted Bacause Job Created by Other Company", false, detention2.CanDelete);
			AssertEquals("Container Detention (DI00000101) cannot be deleted.", detention2.ReasonForNotAbleToDelete.Trim());

			Factory.Save();

			var reasonOfContainerDetention1 = @"Container Detention (DI00000100) cannot be deleted.
This record cannot be deleted.
An Invoicing Job Header (DI00000100) has been created in the company EDI.";

			var reasonOfContainerDetention2 = @"Container Detention (DI00000101) cannot be deleted.
This record cannot be deleted.
An Invoicing Job Header (DI00000101) has been created in the company DEM.";

			AssertNotNull("JobHeader has been created", detention1.Job);
			AssertEquals("Detention Can not Deleted Bacause Job has been Created and in Database", false, detention1.CanDelete);
			AssertEquals(reasonOfContainerDetention1, detention1.ReasonForNotAbleToDelete);

			AssertNull("JobHeader is null Because not Created by current Company", detention2.Job);
			AssertEquals("Detention Can not Deleted Bacause Job Created by Other Company", false, detention2.CanDelete);
			AssertEquals(reasonOfContainerDetention2, detention2.ReasonForNotAbleToDelete);

			AssertNull("JobHeader has not been created yet", detention3.Job);
			AssertEquals("Able to delete because no job", true, detention3.CanDelete);
			AssertEquals(string.Empty, detention3.ReasonForNotAbleToDelete);
		}

		public void TestDeleteDetachesMovements()
		{
			Detention.NC_OH_Client = Client.PK;
			Detention.NC_OH_Principal = Principal.PK;

			BillOfLadingContainer container = Factory.New<BillOfLading>().RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerNum = "CONT1234457";

			var stock = Factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, container.JC_ContainerNum));
			ContainerMovement movement = container.Movements.AddNew();
			movement.E9_R6 = stock.PK;
			movement.E9_NC = detention.PK;

			Factory.Save();
			{
				BusinessObjectFactory deleteFactory = new BusinessObjectFactory();

				AssertNotNull("precondition: invoice", deleteFactory.Load<ContainerDetention>(detention.PK));
				AssertNotNull("precondition: movement", deleteFactory.Load<ContainerMovement>(movement.PK));

				ContainerDetention detentionInDeleteFactory = deleteFactory.Load<ContainerDetention>(detention.PK);
				detentionInDeleteFactory.Delete();
				deleteFactory.Save();
			}

			{
				BusinessObjectFactory loadFactory = new BusinessObjectFactory();
				AssertNull("invoice", loadFactory.Load<ContainerDetention>(detention.PK));

				ContainerMovement movementInLoadFactory = loadFactory.Load<ContainerMovement>(movement.PK);
				AssertNotNull("movements should be detached, not deleted", movementInLoadFactory);
				AssertEquals("movements should be detached, not deleted", ZGuid.Empty, movementInLoadFactory.E9_NC);
			}
		}

		public void TestFind_Import()
		{
			OrgHeader localDepot = Factory.NewWithValidTestData<OrgHeader>();
			localDepot.OH_Code = "Depot";
			localDepot.OH_RL_NKClosestPort = "AUBNE";

			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();

			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.JS_RL_NKOrigin = "AUMEL";
			bill.JS_RL_NKDestination = "AUBNE";
			bill.JS_OH_DeliveryAgent = Principal.PK;

			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;

			ContainerMovement importMovement = stock.Movements.AddNew();
			importMovement.E9_JV = voyage.PK;
			importMovement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			importMovement.E9_OA_Depot = localDepot.MainAddress.PK;
			importMovement.E9_DetentionDays = 5;

			ContainerMovement exportMovement = stock.Movements.AddNew();
			exportMovement.E9_JV = voyage.PK;
			exportMovement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			exportMovement.E9_OA_Depot = localDepot.MainAddress.PK;
			exportMovement.E9_DetentionDays = 5;

			JobHeader header = new JobHeader.Loader(bill).TryLoadOrCreate();
			header.JH_GB = Env.CurrentBranch.PK;
			header.JH_GE = Env.CurrentDepartment.PK;
			header.JH_OA_LocalChargesAddr = Client.MainAddress.PK;

			Factory.Save();

			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			Detention.NC_OH_Client = Client.PK;
			Detention.NC_OH_Principal = Principal.PK;

			AssertContainsExactElementsInAnyOrder(
				(c) => string.Format("{0} ({1})", c.Stock.R6_ContainerNum, c.E9_MovementType),
				new ContainerMovement[] { importMovement },
				Detention.FindRelatedMovements());
		}

		public void TestFind_Export()
		{
			OrgHeader localDepot = Factory.NewWithValidTestData<OrgHeader>();
			localDepot.OH_Code = "Depot";
			localDepot.OH_RL_NKClosestPort = "AUBNE";

			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();

			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.JS_RL_NKOrigin = "AUMEL";
			bill.JS_RL_NKDestination = "AUBNE";
			bill.JS_OH_DeliveryAgent = Principal.PK;

			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;

			ContainerMovement importMovement = stock.Movements.AddNew();
			importMovement.E9_JV = voyage.PK;
			importMovement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			importMovement.E9_OA_Depot = localDepot.MainAddress.PK;
			importMovement.E9_DetentionDays = 5;

			ContainerMovement exportMovement = stock.Movements.AddNew();
			exportMovement.E9_JV = voyage.PK;
			exportMovement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			exportMovement.E9_OA_Depot = localDepot.MainAddress.PK;
			exportMovement.E9_DetentionDays = 5;

			JobHeader header = new JobHeader.Loader(bill).TryLoadOrCreate();
			header.JH_GB = Env.CurrentBranch.PK;
			header.JH_GE = Env.CurrentDepartment.PK;
			header.JH_OA_LocalChargesAddr = Client.MainAddress.PK;

			Factory.Save();

			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Export;
			Detention.NC_OH_Client = Client.PK;
			Detention.NC_OH_Principal = Principal.PK;

			AssertContainsExactElementsInAnyOrder(
				(c) => string.Format("{0} ({1})", c.Stock.R6_ContainerNum, c.E9_MovementType),
				new ContainerMovement[] { exportMovement },
				Detention.FindRelatedMovements());
		}

		public void TestJobIsClosedWhenLastMovementDetached()
		{
			Detention.NC_OH_Client = Client.PK;
			Detention.NC_OH_Principal = Principal.PK;

			BillOfLadingContainer container = Factory.New<BillOfLading>().RealContainers.AddNew();
			ContainerMovement movement1 = container.Movements.AddNew();
			ContainerMovement movement2 = container.Movements.AddNew();

			Detention.Movements.Add(movement1);
			Detention.Movements.Add(movement2);

			JobHeader header = new JobHeader.Loader(Detention).TryLoadOrCreate();
			header.JH_GB = Env.CurrentBranch.PK;
			header.JH_GE = Env.CurrentDepartment.PK;

			AssertEquals("Status is WRK", "WRK", Detention.Job.JH_Status);

			Detention.Movements.RemoveFromRelationship(movement1);
			AssertEquals("Status is still WRK", "WRK", Detention.Job.JH_Status);

			Detention.Movements.RemoveFromRelationship(movement2);
			AssertEquals("Status is CLS", "CLS", Detention.Job.JH_Status);
		}

		#region Implementation

		ContainerDetention Detention
		{
			get { return detention ?? (detention = Factory.New<ContainerDetention>()); }
		}
		ContainerDetention detention;

		OrgHeader Client
		{
			get { return client ?? (client = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader client;

		OrgHeader Principal
		{
			get { return principal ?? (principal = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader principal;

		#endregion
	}
}
