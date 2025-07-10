using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.BulkMovements.Testing
{
	[TestedType(typeof(BulkMovementsForm))]
	internal class BulkMovementsFormBasherTest : ZFormBasherTest
	{
		public void TestGenerate()
		{
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "depot";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			stock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			Factory.Save();
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			using (BulkMovementsForm form = new BulkMovementsForm(header))
			{
				form.Show();
				Application.DoEvents();
				header.MovementType = ContainerMovementTypes.Codes.YardGateOut;
				header.MovementDate = ZDateTime.Now;
				header.DepotAddressPK = depot.MainAddress.PK;
				BulkMovementsChild child = header.Children.AddNew();
				child.ContainerNum = stock.R6_ContainerNum;
				form.PerformCreateClick();
				AssertEquals("No Errors", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Movement Generated", true, child.IsGenerated);
			}

			AssertEquals("New Movement Created", 1, stock.Movements.Count);
			ContainerMovement movement = stock.Movements[0];
			AssertEquals("New Movement Saved", true, movement.IsInDatabase);
			AssertEquals("Movement Type", ContainerMovementTypes.Codes.YardGateOut, movement.E9_MovementType);
		}

		public void TestGenerate_Error()
		{
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "depot";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			stock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			Factory.Save();
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			using (BulkMovementsForm form = new BulkMovementsForm(header))
			{
				form.Show();
				Application.DoEvents();
				header.MovementType = ContainerMovementTypes.Codes.YardGateOut;
				header.MovementDate = ZDateTime.Now;
				header.DepotAddressPK = ZGuid.Invalid;
				BulkMovementsChild child = header.Children.AddNew();
				child.ContainerNum = stock.R6_ContainerNum;
				form.PerformCreateClick();
				AssertEquals("Error Shown", "Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Movement Not Generated", false, child.IsGenerated);
			}

			AssertEquals("No New Movements", 0, stock.Movements.Count);
		}

		public void TestClear()
		{
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			using (BulkMovementsForm form = new BulkMovementsForm(header))
			{
				form.Show();
				Application.DoEvents();
				BulkMovementsChild child1 = header.Children.AddNew();
				BulkMovementsChild child2 = header.Children.AddNew();
				BulkMovementsChild child3 = header.Children.AddNew();
				BulkMovementsChild child4 = header.Children.AddNew();
				child1.IsGenerated = true;
				child2.IsGenerated = true;
				form.PerformClearGeneratedClick();
				AssertContainsExactElementsInAnyOrder("should have removed the generated children", new BulkMovementsChild[] { child3, child4 }, header.Children.ToArray<BulkMovementsChild>());
			}
		}

		public void TestCancel()
		{
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			using (BulkMovementsForm form = new BulkMovementsForm(header))
			{
				form.Show();
				Application.DoEvents();
				form.PerformCloseClick();
				AssertEquals(false, form.Visible);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			return new BulkMovementsForm(header);
		}
		#endregion
	}
}
