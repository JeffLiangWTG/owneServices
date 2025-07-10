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
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Agency.GUI.BulkMovements.Testing
{
	internal class BulkMovementsControlTestCase : TestCaseWithFactory
	{
		public void TestDetach()
		{
			BulkMovementsChild movement1 = Header.Children.AddNew();
			movement1.VoyagePK = Voyage1.PK;
			BulkMovementsChild movement2 = Header.Children.AddNew();
			movement2.VoyagePK = Voyage1.PK;
			BulkMovementsChild movement3 = Header.Children.AddNew();
			movement3.VoyagePK = Voyage1.PK;
			SelectMovements();
			Factory.Save();
			Control.PerformDetachClick();
			AssertEquals("Question No movements selected.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.VoyagePK);
			AssertEquals(Voyage1.PK, movement2.VoyagePK);
			AssertEquals(Voyage1.PK, movement3.VoyagePK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			SelectMovements(movement1, movement2);
			Control.PerformDetachClick();
			AssertEquals("Question Detach 2 movements?", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, movement1.VoyagePK);
			AssertEquals(Voyage1.PK, movement2.VoyagePK);
			AssertEquals(Voyage1.PK, movement3.VoyagePK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			SelectMovements(movement1, movement2);
			Control.PerformDetachClick();
			AssertEquals("Question Detach 2 movements?", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(ZGuid.Empty, movement1.VoyagePK);
			AssertEquals(ZGuid.Empty, movement2.VoyagePK);
			AssertEquals(Voyage1.PK, movement3.VoyagePK);
		}

		public void TestAttach()
		{
			AssertNotNull("lazy load", Voyage1);
			AssertNotNull("lazy load", Voyage2);
			BulkMovementsChild child1 = Header.Children.AddNew();
			child1.VoyagePK = Voyage1.PK;
			BulkMovementsChild child2 = Header.Children.AddNew();
			child2.VoyagePK = ZGuid.Empty;
			BulkMovementsChild child3 = Header.Children.AddNew();
			child3.VoyagePK = ZGuid.Empty;
			SelectMovements();
			Factory.Save();
			Control.PerformAttachClick();
			AssertEquals("Question No movements selected.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, child1.VoyagePK);
			AssertEquals(ZGuid.Empty, child2.VoyagePK);
			AssertEquals(ZGuid.Empty, child3.VoyagePK);
			SelectMovements(child1, child2);
			Control.PerformAttachClick();
			AssertEquals("Question At least one selected movement is already attached.", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage1.PK, child1.VoyagePK);
			AssertEquals(ZGuid.Empty, child2.VoyagePK);
			AssertEquals(ZGuid.Empty, child3.VoyagePK);
			child1.VoyagePK = ZGuid.Empty;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			SelectMovements(child1, child2);
			Control.PerformAttachClick();
			using (ZFormModaliser.LastFormShownForTest)
			{
				EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;
				popup.Close();
			}

			AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(ZGuid.Empty, child1.VoyagePK);
			AssertEquals(ZGuid.Empty, child2.VoyagePK);
			AssertEquals(ZGuid.Empty, child3.VoyagePK);
			SelectMovements(child1, child2);
			Control.PerformAttachClick();
			using (ZFormModaliser.LastFormShownForTest)
			{
				EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;
				popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new BusinessObject[] { Voyage2 });
			}

			AssertEquals("None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(Voyage2.PK, child1.VoyagePK);
			AssertEquals(Voyage2.PK, child2.VoyagePK);
			AssertEquals(ZGuid.Empty, child3.VoyagePK);
		}

		public void TestLeaseContractNoColumnShouldExist()
		{
			var childrenGridField = Control.GetType().GetField("childrenGrid", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull(childrenGridField);
			var childrenGrid = childrenGridField.GetValue(Control) as ZGrid;
			AssertNotNull(childrenGrid);
			var leaseNumberCol = childrenGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == "LeaseContractNo");
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

		void SelectMovements(params BulkMovementsChild[] movements)
		{
			ZGrid grid = (ZGrid)typeof(BulkMovementsControl).InvokeMember("childrenGrid", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, Control, Array.Empty<object>());
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

		BulkMovementsControl Control
		{
			get
			{
				if (control == null)
				{
					control = new BulkMovementsControl();
					control.Dock = DockStyle.Fill;
					ZForm form = new ZForm(Header);
					form.ClientSize = control.Size;
					form.Controls.Add(control);
					control.SetDataBinding(Header, "");
					form.Show();
					Application.DoEvents();
				}

				return control;
			}
		}

		BulkMovementsControl control;
		BulkMovementsHeader Header
		{
			get
			{
				return header ?? (header = new BulkMovementsHeader(Factory));
			}
		}

		BulkMovementsHeader header;
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
