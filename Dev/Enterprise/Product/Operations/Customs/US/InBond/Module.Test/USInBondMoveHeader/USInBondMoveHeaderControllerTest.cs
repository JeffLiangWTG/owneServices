using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(USInBondMoveHeaderController))]
	sealed class USInBondMoveHeaderControllerTest : ZControllerBasherTest
	{
		public void TestOpenInBondFormFromInBondMovementModule()
		{
			using (Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals("InBond form is shown", typeof(USInBondForm), Controller.LastShownForm.GetType());
			}
		}

		public void TestOpenDeclarationFormWhenInBondIsPluggedIn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var moveHeader = (USInBondMoveHeader)GetBusinessObjectThatIsInTheDatabase();
			moveHeader.Header.BH_ParentID = declaration.PK;
			moveHeader.Header.BH_ParentTableCode = declaration.TablePrefix;
			Factory.Save();

			using (Controller.ShowEditForm(moveHeader))
			{
				AssertEquals("Declaration form is shown", typeof(JobDeclarationForm), Controller.LastShownForm.GetType());
			}
		}

		public void TestOpenShipmentFormWhenInBondIsPluggedIn()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var moveHeader = (USInBondMoveHeader)GetBusinessObjectThatIsInTheDatabase();
			moveHeader.Header.BH_ParentID = shipment.PK;
			moveHeader.Header.BH_ParentTableCode = shipment.TablePrefix;
			Factory.Save();

			using (Controller.ShowEditForm(moveHeader))
			{
				AssertEquals("Shipment form is shown", typeof(ShipmentForm), Controller.LastShownForm.GetType());
			}
		}

		public void TestCorrectMovementSelected()
		{
			var inBond = Factory.New<CusInBondHeader>();
			_ = inBond.MovementHeader;

			var movement = inBond.MovementHeaders.AddNew();
			Factory.Save();

			var moveHeader = Factory.Load<USInBondMoveHeader>(movement.PK);

			using (Controller.ShowEditForm(moveHeader))
			{
				var inbondForm = (USInBondForm)Controller.LastShownForm;
				var detailsUserControl = inbondForm.FindSingleOrDefault<USInBondHeaderDetailUserControl>();
				var selectedBizObj = detailsUserControl.MovementHeadersGrid.GetFirstSelectedRow();
				AssertEquals(moveHeader.PK, selectedBizObj.PK);
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var inbond = Factory.New<CusInBondHeader>();
			var movement = inbond.MovementHeader;
			Factory.Save();

			return Factory.Load<USInBondMoveHeader>(movement.PK);
		}

		public override void TestNewForm()
		{
			Assert(true); //New form is not allowed in US InBond Movements module
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.InBondMoveHeader;
	}
}
