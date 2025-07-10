using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(CreateBulkContainerDetentionForm))]
	internal class CreateBulkContainerDetentionFormBasherTest : ZFormBasherTest
	{
		public void TestSelectAllButtonClick()
		{
			using (CreateBulkContainerDetentionForm form = new CreateBulkContainerDetentionForm(Header))
			{
				form.Show();
				Application.DoEvents();
				Header.Children.Add(Child1);
				Header.Children.Add(Child2);
				Child1.IsSelected = true;
				Child2.IsSelected = false;
				form.PerformSelectAllClick();
				AssertEquals(true, Header.Children[0].IsSelected);
				AssertEquals(true, Header.Children[1].IsSelected);
			}
		}

		public void TestUnSelectAllButtonClick()
		{
			using (CreateBulkContainerDetentionForm form = new CreateBulkContainerDetentionForm(Header))
			{
				form.Show();
				Application.DoEvents();
				Header.Children.Add(Child1);
				Header.Children.Add(Child2);
				Child1.IsSelected = true;
				Child2.IsSelected = false;
				form.PerformUnselectAllClick();
				AssertEquals(false, Header.Children[0].IsSelected);
				AssertEquals(false, Header.Children[1].IsSelected);
			}
		}

		public void TestCreateNewClick()
		{
			using (CreateBulkContainerDetentionForm form = new CreateBulkContainerDetentionForm(Header))
			{
				form.Show();
				Application.DoEvents();
				OrgHeader localDepot = Factory.NewWithValidTestData<OrgHeader>();
				localDepot.OH_Code = "Depot";
				localDepot.OH_RL_NKClosestPort = "AUBNE";
				RefContainerStock stock = Factory.New<RefContainerStock>();
				stock.R6_ContainerNum = "TEST4100013";
				stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
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
				container1.JC_ContainerNum = stock.R6_ContainerNum;
				container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				ContainerMovement movement1 = stock.Movements.AddNew();
				movement1.E9_JV = importVoyage.PK;
				movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement1.E9_OA_Depot = localDepot.MainAddress.PK;
				movement1.E9_DetentionDays = 3;
				Factory.Save();
				Header.DetentionType = DetentionInvoiceType.Codes.Import;
				Child1.ClientPK = Client1.PK;
				Child1.PrincipalPK = Principal.PK;
				Child1.MovementPKs.Add(movement1.PK);
				Child1.IsSelected = true;
				Child1.DetentionType = DetentionInvoiceType.Codes.Import;
				Header.RunPreSaveValidation();
				AssertNoNotifications("precondition:", Header);
				Header.Factory.ResetDatabaseLoadCount();
				form.PerformNewClick();
				AssertMaxDbHits("dont load in the form factory", 0, Header.Factory);
				AssertEquals("Question 1 detention job(s) created.", UnitTestUserNotification.Instance.LastMessage.ToString());
				ContainerDetention detention = Factory.Load<ContainerDetention>(movement1.E9_NC);
				AssertNotNull("Should be attached to a detention", detention);
				AssertEquals("attached detention should be in the database", true, detention.IsInDatabase);
			}
		}

		public void TestCreateNewNoneSelectedClick()
		{
			using (CreateBulkContainerDetentionForm form = new CreateBulkContainerDetentionForm(Header))
			{
				form.Show();
				Application.DoEvents();
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
				container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container1.JC_ContainerNum = "CONT1234457";
				ContainerMovement movement1 = container1.Movements.AddNew();
				movement1.E9_R6 = Factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, container1.JC_ContainerNum)).PK;
				movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement1.E9_DetentionDays = 3;
				Factory.Save();
				Header.DetentionType = DetentionInvoiceType.Codes.Import;
				Child1.ClientPK = Client1.PK;
				Child1.PrincipalPK = Principal.PK;
				Child1.MovementPKs.Add(movement1.PK);
				Child1.IsSelected = false;
				Child1.DetentionType = DetentionInvoiceType.Codes.Import;
				form.PerformNewClick();
				AssertEquals("Information No detentions selected", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Should not have created a detention job", ZGuid.Empty, movement1.E9_NC);
			}
		}

		public void TestClickClose()
		{
			using (CreateBulkContainerDetentionForm form = new CreateBulkContainerDetentionForm(Header))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("form should be shown.", true, form.Visible);
				form.PerformCloseClick();
				AssertEquals("form should be closed", false, form.Visible);
			}
		}

		public void TestFindClicked()
		{
			OrgHeader localDepot = Factory.NewWithValidTestData<OrgHeader>();
			localDepot.OH_Code = "Depot";
			localDepot.OH_RL_NKClosestPort = "AUBNE";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			JobVoyage importVoyage = Factory.New<JobVoyage>();
			importVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			importVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			importVoyage.GenerateSailings();
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_OH_DeliveryAgent = Principal.PK;
			bill1.JS_JX = importVoyage.Sailings[0].PK;
			bill1.JS_RL_NKOrigin = "NLAMS";
			bill1.JS_RL_NKDestination = "AUBNE";
			JobHeader job1 = new JobHeader.Loader(bill1).TryCreate();
			job1.JH_OA_LocalChargesAddr = Client1.MainAddress.PK;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			BillOfLadingContainer container1 = bill1.RealContainers.AddNew();
			container1.JC_ContainerNum = stock.R6_ContainerNum;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			BillOfLadingContainer container2 = bill1.RealContainers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			ContainerMovement movement1 = stock.Movements.AddNew();
			movement1.E9_JV = importVoyage.PK;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement1.E9_OA_Depot = localDepot.MainAddress.PK;
			movement1.E9_DetentionDays = 9;
			ContainerMovement movement2 = stock.Movements.AddNew();
			movement2.E9_JV = importVoyage.PK;
			movement2.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement2.E9_OA_Depot = localDepot.MainAddress.PK;
			movement2.E9_DetentionDays = 8;
			Factory.Save();
			using (CreateBulkContainerDetentionForm form = new CreateBulkContainerDetentionForm(Header))
			{
				form.Show();
				Application.DoEvents();
				CreateBulkContainerDetentionControl control = (CreateBulkContainerDetentionControl)form.Controls.Find("createBulkDetentionInvoicesControl", true)[0];
				Header.DetentionType = DetentionInvoiceType.Codes.Import;
				control.PerformFindClicked();
				AssertEquals(1, Header.Children.Count);
				AssertEquals(2, Header.Children[0].ContainerCount);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			AgencyRegistry.Instance.AllowInterCountryDetentionJobCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override Form GetFormToBashCore()
		{
			return new CreateBulkContainerDetentionForm(new BulkDetentionHeader(Factory));
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
