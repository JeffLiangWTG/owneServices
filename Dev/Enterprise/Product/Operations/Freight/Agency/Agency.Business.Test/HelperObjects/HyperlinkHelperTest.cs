using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class HyperlinkHelperTest : TestCaseWithFactory
	{
		public void TestBillOfLadingLink()
		{
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000100";
			AssertLink("V00000100", ControllerIDs.AgencyBillOfLading, bill.PK, HyperlinkHelper.Link(bill));
		}

		public void TestBillOfLadingContainerLink()
		{
			BillOfLadingContainer container = Factory.New<BillOfLadingContainer>();
			container.JC_ContainerNum = "TEST4100013";
			AssertLink("TEST4100013", ControllerIDs.AgencyBillContainers, container.PK, HyperlinkHelper.Link(container));
		}

		public void TestStock()
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			AssertLink("TEST4100013", ControllerIDs.AgencyContainerManager, stock.PK, HyperlinkHelper.Link(stock));
		}

		public void TestMovement()
		{
			ContainerMovement movement = Factory.New<RefContainerStock>().Movements.AddNew();
			movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertLink("Wharf Gate In", ControllerIDs.AgencyContainerMove, movement.PK, HyperlinkHelper.Link(movement));
			movement.E9_MovementType = "XXX";
			AssertLink("XXX", ControllerIDs.AgencyContainerMove, movement.PK, HyperlinkHelper.Link(movement));
			movement.E9_MovementType = "";
			AssertLink("<EMPTY>", ControllerIDs.AgencyContainerMove, movement.PK, HyperlinkHelper.Link(movement));
		}

		public void TestDetention()
		{
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "OTH";
			ContainerDetention detention = Factory.New<ContainerDetention>();
			detention.NC_JobNumber = "DI00001000";
			detention.NC_GC = GlbCompany.CurrentCompany.PK;
			AssertLink("DI00001000", ControllerIDs.AgencyContainerDetention, detention.PK, HyperlinkHelper.Link(detention));
			detention.NC_GC = otherCompany.PK;
			AssertLink("DI00001000(OTH)", ControllerIDs.AgencyContainerDetention, detention.PK, HyperlinkHelper.Link(detention));
		}

		#region Implementation
		static void AssertLink(string text, ControllerID controller, ZGuid pk, LogHyperlink link)
		{
			AssertType(typeof(LogControllerLink), link);
			CombineAssertions(delegate
			{
				LogControllerLink clink = (LogControllerLink)link;
				AssertEquals("Text:", text, clink.Text);
				AssertEquals("Controller:", controller, clink.Controller);
				AssertEquals("PK: ", pk, clink.PK);
			});
		}
		#endregion
	}
}
