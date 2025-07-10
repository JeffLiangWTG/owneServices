using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(WhsWorkOrderAssemblyConfirmationDialog))]
	class WhsWorkOrderAssemblyConfirmationDialogTest : ZFormBasherTest
	{
		public void TestDialogResult()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(workOrder);

			using (var form = new WhsWorkOrderAssemblyConfirmationDialog(workOrder.Receive))
			{
				form.Shown += (sender, e) => GUITestHelper.FindControl<ZButton>(form.Controls, "ConfirmButton").PerformClick();
				var result = form.ShowDialog();
				AssertEquals(DialogResult.OK, result);
			}

			using (var form = new WhsWorkOrderAssemblyConfirmationDialog(workOrder.Receive))
			{
				form.Shown += (sender, e) => GUITestHelper.FindControl<ZButton>(form.Controls, "CancelAssemblyButton").PerformClick();
				var result = form.ShowDialog();
				AssertEquals(DialogResult.Cancel, result);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);

			using (new DisposableList(GetAllSuspenders(workOrder)))
			{
				workOrder.FinaliseDocketAlwaysFinalisingPick();
			}

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(workOrder);
			Factory.Save();

			return new WhsWorkOrderAssemblyConfirmationDialog(workOrder.Receive);

			IEnumerable<IDisposable> GetAllSuspenders(IBusiness child)
			{
				if (child is BusinessObject bizO)
				{
					yield return bizO.SuspendMarkingAsNeedingValidation();
				}

				foreach (var suspender in child.Children.SelectMany(c => GetAllSuspenders(c)))
				{
					yield return suspender;
				}
			}
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;
	}
}
