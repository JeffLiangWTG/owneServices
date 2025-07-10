using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class ContainerManagerMovementsControlTest : TestCaseWithFactory
	{
		public void TestDetach()
		{
			ContainerMovement movement1 = Stock.Movements.AddNew();
			movement1.E9_JV = Voyage1.PK;
			ContainerMovement movement2 = Stock.Movements.AddNew();
			movement2.E9_JV = Voyage1.PK;
			ContainerMovement movement3 = Stock.Movements.AddNew();
			movement3.E9_JV = Voyage1.PK;
			ContainerMovement movement4 = Stock.Movements.AddNew();
			movement4.E9_JV = Voyage1.PK;
			movement4.E9_NC = Detention.PK;
			Stock.Filter.Find();
			SelectMovements();
			Control.PerformDetachClick();
			AssertEquals("Question This container has not yet been saved.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.E9_JV);
			AssertEquals(Voyage1.PK, movement2.E9_JV);
			AssertEquals(Voyage1.PK, movement3.E9_JV);
			AssertEquals(Voyage1.PK, movement4.E9_JV);
			Factory.Save();
			Control.PerformDetachClick();
			AssertEquals("Question No movements selected.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.E9_JV);
			AssertEquals(Voyage1.PK, movement2.E9_JV);
			AssertEquals(Voyage1.PK, movement3.E9_JV);
			AssertEquals(Voyage1.PK, movement4.E9_JV);
			SelectMovements(movement1, movement2, movement4);
			Control.PerformDetachClick();
			AssertEquals("Question Can't detach movements with a detention job attached.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.E9_JV);
			AssertEquals(Voyage1.PK, movement2.E9_JV);
			AssertEquals(Voyage1.PK, movement3.E9_JV);
			AssertEquals(Voyage1.PK, movement4.E9_JV);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			SelectMovements(movement1, movement2);
			Control.PerformDetachClick();
			AssertEquals("Question Detach 2 movements?", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.E9_JV);
			AssertEquals(Voyage1.PK, movement2.E9_JV);
			AssertEquals(Voyage1.PK, movement3.E9_JV);
			AssertEquals(Voyage1.PK, movement4.E9_JV);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			SelectMovements(movement1, movement2);
			Control.PerformDetachClick();
			AssertEquals("Question Detach 2 movements?", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(ZGuid.Empty, movement1.E9_JV);
			AssertEquals(ZGuid.Empty, movement2.E9_JV);
			AssertEquals(Voyage1.PK, movement3.E9_JV);
			AssertEquals(Voyage1.PK, movement4.E9_JV);
		}

		public void TestAttach()
		{
			AssertNotNull("lazy load", Voyage1);
			AssertNotNull("lazy load", Voyage2);
			ContainerMovement movement1 = Stock.Movements.AddNew();
			movement1.E9_JV = Voyage1.PK;
			ContainerMovement movement2 = Stock.Movements.AddNew();
			movement2.E9_JV = ZGuid.Empty;
			ContainerMovement movement3 = Stock.Movements.AddNew();
			movement3.E9_JV = ZGuid.Empty;
			ContainerMovement movement4 = Stock.Movements.AddNew();
			movement4.E9_JV = ZGuid.Empty;
			movement4.E9_NC = Detention.PK;
			Stock.Filter.Find();
			SelectMovements();
			Control.PerformAttachClick();
			AssertEquals("Question This container has not yet been saved.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.E9_JV);
			AssertEquals(ZGuid.Empty, movement2.E9_JV);
			AssertEquals(ZGuid.Empty, movement3.E9_JV);
			AssertEquals(ZGuid.Empty, movement4.E9_JV);
			Factory.Save();
			Control.PerformAttachClick();
			AssertEquals("Question No movements selected.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.E9_JV);
			AssertEquals(ZGuid.Empty, movement2.E9_JV);
			AssertEquals(ZGuid.Empty, movement3.E9_JV);
			AssertEquals(ZGuid.Empty, movement4.E9_JV);
			SelectMovements(movement1, movement2, movement4);
			Control.PerformAttachClick();
			AssertEquals("Question At least one selected movement is already attached.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.E9_JV);
			AssertEquals(ZGuid.Empty, movement2.E9_JV);
			AssertEquals(ZGuid.Empty, movement3.E9_JV);
			AssertEquals(ZGuid.Empty, movement4.E9_JV);
			movement1.E9_JV = ZGuid.Empty;
			SelectMovements(movement1, movement2, movement4);
			Control.PerformAttachClick();
			AssertEquals("Question Cannot attach movements that are attached to a detention job.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(ZGuid.Empty, movement1.E9_JV);
			AssertEquals(ZGuid.Empty, movement2.E9_JV);
			AssertEquals(ZGuid.Empty, movement3.E9_JV);
			AssertEquals(ZGuid.Empty, movement4.E9_JV);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			SelectMovements(movement1, movement2);
			Control.PerformAttachClick();
			using (ZFormModaliser.LastFormShownForTest)
			{
				EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;
				popup.Close();
			}

			AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(ZGuid.Empty, movement1.E9_JV);
			AssertEquals(ZGuid.Empty, movement2.E9_JV);
			AssertEquals(ZGuid.Empty, movement3.E9_JV);
			AssertEquals(ZGuid.Empty, movement4.E9_JV);
			SelectMovements(movement1, movement2);
			Control.PerformAttachClick();
			using (ZFormModaliser.LastFormShownForTest)
			{
				EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { Voyage2 });
			}

			AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage2.PK, movement1.E9_JV);
			AssertEquals(Voyage2.PK, movement2.E9_JV);
			AssertEquals(ZGuid.Empty, movement3.E9_JV);
			AssertEquals(ZGuid.Empty, movement4.E9_JV);
		}

		public void TestDefaultFromBOL()
		{
			OrgHeader principal1 = Factory.NewWithValidTestData<OrgHeader>();
			principal1.OH_Code = "Principal1";
			OrgHeader principal2 = Factory.NewWithValidTestData<OrgHeader>();
			principal2.OH_Code = "Principal2";
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_Code = "Client1";
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "Client2";
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_UniqueConsignRef = "V0001";
			bill1.JS_HouseBill = "BOL1";
			bill1.JS_OH_DeliveryAgent = principal1.PK;
			JobHeader job = new JobHeader.Loader(bill1).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = client1.MainAddress.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			ContainerMovement movement1 = Stock.Movements.AddNew();
			movement1.E9_OH_Principal = principal2.PK;
			movement1.E9_OH_ResponsibleParty = client2.PK;
			ContainerMovement movement2 = Stock.Movements.AddNew();
			movement2.E9_OH_Principal = ZGuid.Empty;
			movement2.E9_OH_ResponsibleParty = ZGuid.Empty;
			ContainerMovement movement3 = Stock.Movements.AddNew();
			movement3.E9_OH_Principal = ZGuid.Empty;
			movement3.E9_OH_ResponsibleParty = ZGuid.Empty;
			ContainerMovement movement4 = Stock.Movements.AddNew();
			movement4.E9_NC = Detention.PK;
			Stock.Filter.Find();
			SelectMovements();
			Control.PerformDefaultFromBolClick();
			AssertEquals("Question No reference number entered.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			Stock.Filter.ReferenceNumber = "BOL2";
			Control.PerformDefaultFromBolClick();
			AssertEquals("Question This container has not yet been saved.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			Factory.Save();
			Control.PerformDefaultFromBolClick();
			AssertEquals("Question No movements selected.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			SelectMovements(movement1, movement3, movement4);
			Control.PerformDefaultFromBolClick();
			AssertEquals("Question No matching bills/bookings were found.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			Stock.Filter.ReferenceNumber = "BOL1";
			Control.PerformDefaultFromBolClick();
			AssertEquals("Question Cannot default movements that are attached to a detention job.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			SelectMovements(movement1, movement3);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			Control.PerformDefaultFromBolClick();
			AssertEquals("Question Default the principal and responsible party overrides for 2 movements from V0001.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Control.PerformDefaultFromBolClick();
			AssertEquals("Question Default the principal and responsible party overrides for 2 movements from V0001.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal1.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(principal1.PK, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client1.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(client1.PK, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
		}

		public void TestDefaultFromBookingRef()
		{
			OrgHeader principal1 = Factory.NewWithValidTestData<OrgHeader>();
			principal1.OH_Code = "Principal1";
			OrgHeader principal2 = Factory.NewWithValidTestData<OrgHeader>();
			principal2.OH_Code = "Principal2";
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_Code = "Client1";
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "Client2";
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_UniqueConsignRef = "V0001";
			bill1.JS_CFSReference = "BOL1";
			bill1.JS_OH_DeliveryAgent = principal1.PK;
			JobHeader job = new JobHeader.Loader(bill1).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = client1.MainAddress.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			ContainerMovement movement1 = Stock.Movements.AddNew();
			movement1.E9_OH_Principal = principal2.PK;
			movement1.E9_OH_ResponsibleParty = client2.PK;
			ContainerMovement movement2 = Stock.Movements.AddNew();
			movement2.E9_OH_Principal = ZGuid.Empty;
			movement2.E9_OH_ResponsibleParty = ZGuid.Empty;
			ContainerMovement movement3 = Stock.Movements.AddNew();
			movement3.E9_OH_Principal = ZGuid.Empty;
			movement3.E9_OH_ResponsibleParty = ZGuid.Empty;
			ContainerMovement movement4 = Stock.Movements.AddNew();
			movement4.E9_NC = Detention.PK;
			Stock.Filter.Find();
			SelectMovements();
			Control.PerformDefaultFromBookingRefClick();
			AssertEquals("Question No reference number entered.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			Stock.Filter.ReferenceNumber = "BOL2";
			Control.PerformDefaultFromBookingRefClick();
			AssertEquals("Question This container has not yet been saved.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			Factory.Save();
			Control.PerformDefaultFromBookingRefClick();
			AssertEquals("Question No movements selected.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			SelectMovements(movement1, movement3, movement4);
			Control.PerformDefaultFromBookingRefClick();
			AssertEquals("Question No matching bills/bookings were found.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			Stock.Filter.ReferenceNumber = "BOL1";
			Control.PerformDefaultFromBookingRefClick();
			AssertEquals("Question Cannot default movements that are attached to a detention job.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			SelectMovements(movement1, movement3);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			Control.PerformDefaultFromBookingRefClick();
			AssertEquals("Question Default the principal and responsible party overrides for 2 movements from V0001.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal2.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client2.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Control.PerformDefaultFromBookingRefClick();
			AssertEquals("Question Default the principal and responsible party overrides for 2 movements from V0001.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(principal1.PK, movement1.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_Principal);
			AssertEquals(principal1.PK, movement3.E9_OH_Principal);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_Principal);
			AssertEquals(client1.PK, movement1.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement2.E9_OH_ResponsibleParty);
			AssertEquals(client1.PK, movement3.E9_OH_ResponsibleParty);
			AssertEquals(ZGuid.Empty, movement4.E9_OH_ResponsibleParty);
		}

		public void TestE9_LeaseNumberColumnShouldExist()
		{
			var grid = MovementsGrid;
			AssertNotNull(grid);
			var leaseNumberCol = grid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == "E9_LeaseNumber");
			AssertNotNull(leaseNumberCol);
			Assert(!leaseNumberCol.IsVisible);
		}

		#region Implementation
		protected override void TearDown()
		{
			try
			{
				if (control != null)
				{
					Form form = control.FindForm();
					if (form != null)
					{
						form.Dispose();
					}
					else
					{
						control.Dispose();
					}

					control = null;
				}
			}
			finally
			{
				base.TearDown();
			}
		}

		void SelectMovements(params ContainerMovement[] movements)
		{
			ZGrid grid = MovementsGrid;
			AssertNotNull(grid);
			if (grid.ListManager == null)
			{
				throw new InvalidOperationException("The grid is not yet bound");
			}

			IList toSelect = movements;
			IList managerList = grid.ListManager.List;
			for (int i = 0; i < managerList.Count; i++)
			{
				if (toSelect.Contains(managerList[i]))
				{
					grid.Select(i);
				}
				else
				{
					grid.UnSelect(i);
				}
			}
		}

		ZGrid MovementsGrid => (ZGrid)typeof(ContainerManagerMovementsControl).InvokeMember("movementsGrid", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, Control, Array.Empty<object>());
		ContainerManagerMovementsControl Control
		{
			get
			{
				if (control == null)
				{
					control = new ContainerManagerMovementsControl();
					control.Dock = DockStyle.Fill;
					ZForm form = new ZForm(Stock);
					form.ClientSize = control.Size;
					form.Controls.Add(control);
					control.SetDataBinding(Stock, "");
					form.Show();
					Application.DoEvents();
				}

				return control;
			}
		}

		ContainerManagerMovementsControl control;
		RefContainerStock Stock
		{
			get
			{
				if (stock == null)
				{
					stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = "FAKU4100011";
					stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
					stock.Filter.FromDate = ZDateTime.Empty;
				}

				return stock;
			}
		}

		RefContainerStock stock;
		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = Factory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "Principal";
				}

				return principal;
			}
		}

		OrgHeader principal;
		OrgHeader Client
		{
			get
			{
				if (client == null)
				{
					client = Factory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "Client";
				}

				return client;
			}
		}

		OrgHeader client;
		ContainerDetention Detention
		{
			get
			{
				if (detention == null)
				{
					detention = Factory.New<ContainerDetention>();
					detention.NC_OH_Client = Client.PK;
					detention.NC_OH_Principal = Principal.PK;
				}

				return detention;
			}
		}

		ContainerDetention detention;
		JobVoyage Voyage1
		{
			get
			{
				if (voyage1 == null)
				{
					voyage1 = Factory.New<JobVoyage>();
					voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					voyage1.GenerateSailings();
				}

				return voyage1;
			}
		}

		JobVoyage voyage1;
		JobVoyage Voyage2
		{
			get
			{
				if (voyage2 == null)
				{
					voyage2 = Factory.New<JobVoyage>();
					voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					voyage2.GenerateSailings();
				}

				return voyage2;
			}
		}

		JobVoyage voyage2;
		#endregion
	}
}
