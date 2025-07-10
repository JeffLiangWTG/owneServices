using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(ContainerManagerForm))]
	internal class ContainerManagerFormTest : ZFormBasherTest
	{
		public void TestSelectAndShow()
		{
			const string dialogText = "Question The selected movement does not match the existing filters and the filters could not be adjusted " + "because there are movements with changes that need to be saved first.";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ContainerMovement movement1 = stock.Movements.AddNew();
			movement1.E9_MovementType = ContainerMovementTypes.Codes.Load;
			ContainerMovement movement2 = stock.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.Discharge;
			using (ContainerManagerForm form = new ContainerManagerForm(stock))
			{
				form.Show();
				Application.DoEvents();
				ContainerManagerMovementsControl movements = GetControl<ContainerManagerMovementsControl>(form, "movementsControl");
				ZGrid grid = GetControl<ContainerManagerMovementsControl, ZGrid>(movements, "movementsGrid");
				AssertEquals("hidden to begin with", false, grid.Visible);
				form.SelectAndShowMovement(movement1.PK);
				AssertEquals("should be shown now", true, grid.Visible);
				AssertEquals("should have selected container1", movement1, grid.ListManager.GetCurrent());
				form.SelectAndShowMovement(movement2.PK);
				AssertEquals("should have selected container2", movement2, grid.ListManager.GetCurrent());
				form.SelectAndShowMovement(ZGuid.Empty);
				form.SelectAndShowMovement(ZGuid.NewZGuid());
				Factory.Save();
				stock.Filter.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
				stock.Filter.Find();
				AssertEquals("precondition", 0, stock.Filter.Movements.Count);
				form.SelectAndShowMovement(movement1.PK);
				AssertEquals("should have shown an explenation", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("should have adjusted filter to match movement1", ContainerMovementTypes.Codes.Load, stock.Filter.MovementType);
				AssertEquals("should have adjusted filter and selected movement1", movement1, grid.ListManager.GetCurrent());
				movement1.HasChanges = true;
				form.SelectAndShowMovement(movement2.PK);
				AssertEquals("should have shown an explenation", dialogText, UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("should not have adjusted filter as a movement has changes.", ContainerMovementTypes.Codes.Load, stock.Filter.MovementType);
				AssertEquals("should not have selected movement2 as a movement has changes.", movement1, grid.ListManager.GetCurrent());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				form.SelectAndShowMovement(movement2.PK);
				AssertEquals("should have shown an explenation", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("should have adjusted filter to match movement2", ContainerMovementTypes.Codes.Discharge, stock.Filter.MovementType);
				AssertEquals("should have adjusted filter and selected movement2", movement2, grid.ListManager.GetCurrent());
			}
		}

		#region Implementation
		T GetControl<T>(ContainerManagerForm form, string name)
			where T : Control
		{
			return GetControl<ContainerManagerForm, T>(form, name);
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}

		protected override Form GetFormToBashCore()
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			var result = new ContainerManagerForm(stock);
			result.ControllerID = ControllerIDs.AgencyContainerManager;
			return result;
		}
		#endregion
	}
}
