using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BulkDetentionHeader))]
	internal class BulkDetentionHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHasSelectedChildren()
		{
			AssertEquals("No Children Yet", false, Header.HasSelectedChildren());
			Child1.IsSelected = false;
			Child2.IsSelected = true;
			AssertEquals("One child selected", true, Header.HasSelectedChildren());
			Child2.IsSelected = false;
			AssertEquals("No children selected", false, Header.HasSelectedChildren());
		}

		public void TestGetChildren()
		{
			AssertType(typeof(BulkDetentionChildCollection), Header.Children);
			AssertEquals(Header.Factory, Header.Children.Factory);
		}

		public void TestLookups()
		{
			AssertType("lookups class should be of the correct type", typeof(BulkDetentionHeaderLookups), Header.Lookups);
			AssertSame("Should cache the lookups class", Header.Lookups, Header.Lookups);
		}

		public void TestSelectUnselectAll()
		{
			Header.Children.Add(Child1);
			Header.Children.Add(Child2);
			Child1.IsSelected = true;
			Child2.IsSelected = false;
			Header.SelectUnselectAll(true);
			AssertEquals(true, header.Children[0].IsSelected);
			AssertEquals(true, header.Children[1].IsSelected);
			Header.Children[0].IsSelected = true;
			Header.Children[1].IsSelected = false;
			Header.SelectUnselectAll(false);
			AssertEquals(false, header.Children[0].IsSelected);
			AssertEquals(false, header.Children[1].IsSelected);
		}

		public void TestCreateDetentionInvoice()
		{
			OrgHeader localDepot = Factory.NewWithValidTestData<OrgHeader>();
			localDepot.OH_Code = "Depot";
			localDepot.OH_RL_NKClosestPort = "AUBNE";
			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			RefContainerStock stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100029";
			stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			JobVoyage importVoyage = Factory.New<JobVoyage>();
			importVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			importVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			importVoyage.GenerateSailings();
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_OH_DeliveryAgent = Principal.PK;
			bill1.JS_JX = importVoyage.Sailings[0].PK;
			JobHeader job1 = new JobHeader.Loader(bill1).TryCreate();
			job1.JH_OA_LocalChargesAddr = Client1.MainAddress.PK;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			BillOfLadingContainer container1 = bill1.RealContainers.AddNew();
			container1.JC_ContainerNum = stock1.R6_ContainerNum;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ContainerMovement movement1 = stock1.Movements.AddNew();
			movement1.E9_JV = importVoyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1.E9_OA_Depot = localDepot.MainAddress.PK;
			movement1.E9_DetentionDays = 3;
			JobVoyage exportVoyage = Factory.New<JobVoyage>();
			exportVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			exportVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			exportVoyage.GenerateSailings();
			BillOfLading bill2 = Factory.New<BillOfLading>();
			bill2.JS_OH_DeliveryAgent = Principal.PK;
			bill2.JS_JX = exportVoyage.Sailings[0].PK;
			JobHeader job2 = new JobHeader.Loader(bill2).TryCreate();
			job2.JH_OA_LocalChargesAddr = Client2.MainAddress.PK;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			BillOfLadingContainer container2 = bill2.RealContainers.AddNew();
			container2.JC_ContainerNum = stock2.R6_ContainerNum;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			ContainerMovement movement2 = stock2.Movements.AddNew();
			movement2.E9_JV = exportVoyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement2.E9_OA_Depot = localDepot.MainAddress.PK;
			movement2.E9_DetentionDays = 7;
			Factory.Save();
			Factory.ResetDatabaseLoadCount();
			Child1.ClientPK = Client1.PK;
			Child1.PrincipalPK = Principal.PK;
			Child1.MovementPKs.Add(movement1.PK);
			Child1.IsSelected = true;
			Child1.DetentionType = DetentionInvoiceType.Codes.Import;
			Child2.ClientPK = Client2.PK;
			Child2.PrincipalPK = Principal.PK;
			Child2.MovementPKs.Add(movement2.PK);
			Child2.IsSelected = false;
			Child2.DetentionType = DetentionInvoiceType.Codes.Export;
			BusinessObjectFactory createFactory = new BusinessObjectFactory();
			Header.CreateDetentionInvoice(createFactory);
			createFactory.Save();
			AssertMaxDbHits("Should not have loaded anything in the form factory.", 0, Factory);
			ContainerDetention detention = Factory.Load<ContainerDetention>(movement1.E9_NC);
			AssertNotNull("should be attached to a detention", detention);
			AssertEquals("should not be attached to a detention", ZGuid.Empty, movement2.E9_NC);
			AssertEquals("Child1.IsSelected", false, Child1.IsSelected);
			AssertEquals("Child1.IsLocked", true, Child1.IsLocked);
			AssertEquals("Child1.JobNumber", detention.NC_JobNumber, Child1.JobNumber);
			AssertEquals("Child1.DetentionPK", detention.PK, Child1.DetentionPK);
			AssertEquals("Child2.IsSelected", false, Child2.IsSelected);
			AssertEquals("Child2.IsLocked", false, Child2.IsLocked);
			AssertEquals("Child2.JobNumber", "", Child2.JobNumber);
			AssertEquals("Child2.DetentionPK", ZGuid.Empty, Child2.DetentionPK);
			AssertEquals("Header.GeneratedJobs", 1, Header.GeneratedJobs);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkDetentionHeader(Factory);
		}

		BulkDetentionHeader Header
		{
			get
			{
				return header ?? (header = new BulkDetentionHeader(Factory));
			}
		}

		BulkDetentionHeader header;
		BulkDetentionChild Child1
		{
			get
			{
				return child1 ?? (child1 = Header.Children.AddNew());
			}
		}

		BulkDetentionChild child1;
		BulkDetentionChild Child2
		{
			get
			{
				return child2 ?? (child2 = Header.Children.AddNew());
			}
		}

		BulkDetentionChild child2;
		OrgHeader Client1
		{
			get
			{
				if (client1 == null)
				{
					client1 = Factory.NewWithValidTestData<OrgHeader>();
					client1.OH_Code = "Client1";
				}

				return client1;
			}
		}

		OrgHeader client1;
		OrgHeader Client2
		{
			get
			{
				if (client2 == null)
				{
					client2 = Factory.NewWithValidTestData<OrgHeader>();
					client2.OH_Code = "Client2";
				}

				return client2;
			}
		}

		OrgHeader client2;
		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = Factory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "principal";
				}

				return principal;
			}
		}

		OrgHeader principal;
		#endregion
	}
}
