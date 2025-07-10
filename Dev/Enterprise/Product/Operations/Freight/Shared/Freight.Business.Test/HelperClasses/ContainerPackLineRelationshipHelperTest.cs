using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerPackLineRelationshipHelperTest : BaseFreightTest
	{
		public void TestThis()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			var pivot = Factory.New<JobContainerPackPivot>();
			var container = Factory.New<CommonContainer>();

			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			pivot.J6_JC = container.PK;
			pivot.J6_JL = packLine.PK;

			var pivotsWithAbsentConShipLink = ContainerPackLineRelationshipHelper.GetContainerPackPivotsWithAbsentConShipLink(Factory, null, packLine);
			AssertNoExceptionThrown(() => pivotsWithAbsentConShipLink.Count());
		}

		#region ErrorReportForMissingConshipLink

		public void TestErrorReportIsSentWhenPivotCreatedWithoutTheConshipLink()
		{
			ErrorReporter.Clear();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00000001";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00000002";
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00000003";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00000001";
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C00000002";
			var consol3 = shipment.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C00000003";

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 3;
			container1.JC_RC = refContainer.PK;
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "PCIU2120962";
			var packLine = shipment.OuterPackLines.AddNew();

			var pivot = Factory.New<JobContainerPackPivot>();

			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			pivot.J6_JC = container1.PK;
			pivot.J6_JL = packLine.PK;
			AssertEquals("JobContainerPackPivot.J6_JL_setter", ErrorReporter.LastKeyReported);

			var errorMessage = string.Format(CultureInfo.InvariantCulture, @"Shipment S00000001 {{{0}}}'s PackLine {{{1}}} is being allocated to the Container 20GP (3) belonging to the Consol C00000001 {{{2}}} however no corresponding JobConShipLink exists. Please inform IL team. WI00035124
Packline Factory Instance:{9}
Container Factory Instance:{10}
Base Factory Instance:{11}
Validating factory instance:{11}
Shipment S00000001 {{{0}}} does exist in factory {11};
S00000001 {{{0}}}'s Consols: C00000002 {{{3}}}; C00000003 {{{4}}};
Consol C00000001 {{{2}}} does exist in factory {11};
C00000001 {{{2}}}'s Shipments: S00000002 {{{5}}}; S00000003 {{{6}}};
C00000001 {{{2}}}'s Containers: 20GP (3) {{{7}}}; PCIU2120962 {{{8}}};
Shipment S00000001 {{{0}}} Type: STD, JS_JS_ColoadMasterShipment: 00000000-0000-0000-0000-000000000000;
Record information from DB:
Shipment {{{0}}}:
Consol {{{2}}}:
PackLine {{{1}}}:
Container {{{7}}}:
",
				shipment.PK,
				packLine.PK,
				consol.PK,
				consol2.PK,
				consol3.PK,
				shipment2.PK,
				shipment3.PK,
				container1.PK,
				container2.PK,
				packLine.Factory._Instance,
				container1.Factory._Instance,
				Factory._Instance);
			AssertStartsWith("Summary", errorMessage, ErrorReporter.LastMessageReported);

			var stackTraceHeader = string.Format(CultureInfo.InvariantCulture, @"PackLine.CurrentConsol is set to Consol C00000002 {{{0}}}. StackTrace:", consol2.PK);
			AssertNotContains("PackLine.CurrentConsol stacktrace", stackTraceHeader, ErrorReporter.LastMessageReported);

			pivot.J6_JC = container2.PK;
			AssertEquals("JobContainerPackPivot.J6_JC_setter", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestErrorReportIsSentWhenPivotCreatedWithoutTheConshipLink_UsePKWhenUniqueConsignRefIsNotReady()
		{
			ErrorReporter.Clear();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = ZString.Empty;
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00000001";
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C00000002";
			var consol3 = shipment.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C00000003";

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 3;
			container1.JC_RC = refContainer.PK;
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "PCIU2120962";
			var packLine = shipment.OuterPackLines.AddNew();

			var pivot = Factory.New<JobContainerPackPivot>();

			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			pivot.J6_JC = container1.PK;
			pivot.J6_JL = packLine.PK;
			AssertEquals("JobContainerPackPivot.J6_JL_setter", ErrorReporter.LastKeyReported);

			var errorMessage = string.Format(CultureInfo.InvariantCulture, @"Shipment S00000001 {{{4}}}'s PackLine {{{5}}} is being allocated to the Container 20GP (3) belonging to the Consol  {{{0}}} however no corresponding JobConShipLink exists. Please inform IL team. WI00035124
Packline Factory Instance:{9}
Container Factory Instance:{10}
Base Factory Instance:{11}
Validating factory instance:{11}
Shipment S00000001 {{{4}}} does exist in factory {11};
S00000001 {{{4}}}'s Consols: C00000002 {{{3}}}; C00000003 {{{6}}};
Consol  {{{0}}} does exist in factory {11};
 {{{0}}}'s Shipments:  {{{1}}};  {{{2}}};
 {{{0}}}'s Containers: 20GP (3) {{{7}}}; PCIU2120962 {{{8}}};
Shipment S00000001 {{{4}}} Type: STD, JS_JS_ColoadMasterShipment: 00000000-0000-0000-0000-000000000000;
Record information from DB:
Shipment {{{4}}}:
Consol {{{0}}}:
PackLine {{{5}}}:
Container {{{7}}}:
",
				consol.PK,
				shipment2.PK,
				shipment3.PK,
				consol2.PK,
				shipment.PK,
				packLine.PK,
				consol3.PK,
				container1.PK,
				container2.PK,
				packLine.Factory._Instance,
				container1.Factory._Instance,
				Factory._Instance);
			AssertStartsWith("Summary", errorMessage, ErrorReporter.LastMessageReported);

			var stackTraceHeader = string.Format(CultureInfo.InvariantCulture, @"PackLine.CurrentConsol is set to Consol C00000002 {{{0}}}. StackTrace:", consol2.PK);
			AssertNotContains("PackLine.CurrentConsol stacktrace", stackTraceHeader, ErrorReporter.LastMessageReported);

			pivot.J6_JC = container2.PK;
			AssertEquals("JobContainerPackPivot.J6_JC_setter", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestErrorReportForMissingConshipLinkIsNotCreatedForMultiAWBMaster()
		{
			ErrorReporter.Clear();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			consol.JK_AgentType = Core.Constants.AgentType.AWBMaster;
			AssertEquals("Prerequisite", true, consol.IsMultiAWBMaster);

			ContainerPackLineRelationshipHelper.ReportAbsentConShipLink("foo", packLine, container, Factory);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastKeyReported));

			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			ContainerPackLineRelationshipHelper.ReportAbsentConShipLink("foo", packLine, container, Factory);
			AssertEquals("foo", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		#endregion

		#region TestAddingAndRemovingRefreshContainerWeight

		public void TestAddingAndRemovingRefreshContainerWeight()
		{
			PackLine otherPackLine = Container.Consol.Shipments.AddNew().OuterPackLines.AddNew();
			otherPackLine.JL_ActualWeight = 5000;
			Container.PackLines.Add(otherPackLine);

			Container.JC_Calc_NetWeight = 4000;
			AssertEquals(4000m, Container.JC_Calc_NetWeight);

			ContainerPackLineRelationshipHelper.PackLineAddedToContainer(Container, PackLine);
			AssertEquals(5000m, Container.JC_Calc_NetWeight);

			Container.JC_Calc_NetWeight = 4000;
			AssertEquals(4000m, Container.JC_Calc_NetWeight);

			ContainerPackLineRelationshipHelper.PackLineRemovedFromContainer(Container, PackLine);
			AssertEquals(5000m, Container.JC_Calc_NetWeight);
		}

		#endregion

		#region TestSetServices

		public void TestSetServices()
		{
			Container.Services.AddNew().ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Shipment.DocsAndCartage.Services.RemoveAndDeleteAll();

			AssertEquals("precondition:", false, Shipment.DocsAndCartage.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));

			ContainerPackLineRelationshipHelper.PackLineAddedToContainer(Container, PackLine);
			AssertEquals(true, Shipment.DocsAndCartage.Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation));
		}

		#endregion

		#region TestEnsurePackLineIsInOneContainerPerConsol

		public void TestEnsurePackLineIsInOneContainerPerConsol_TwoContainers()
		{
			var consol = Factory.New<CommonConsol>();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();

			var packLine = shipment.OuterPackLines.AddNew();

			AssertContainsExactElementsInAnyOrder("container 1 packLines collection", System.Array.Empty<PackLine>(), container1.PackLines);
			AssertContainsExactElementsInAnyOrder("container 2 packLines collection", new[] { packLine }, container2.PackLines);
			AssertContainsExactElementsInAnyOrder("container 2 packLines collection", new[] { container2 }, packLine.Containers);

			packLine.JL_JC = container1.PK;

			AssertContainsExactElementsInAnyOrder("container 1 packLines collection", new[] { packLine }, container1.PackLines);
			AssertContainsExactElementsInAnyOrder("container 2 packLines collection", System.Array.Empty<PackLine>(), container2.PackLines);
			AssertContainsExactElementsInAnyOrder("container 2 packLines collection", new[] { container1 }, packLine.Containers);
		}

		public void TestEnsurePackLineIsInOneContainerPerConsol_ThreeContainers()
		{
			var consol = Factory.New<CommonConsol>();

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();

			var packLine = shipment.OuterPackLines.AddNew();

			AssertContainsExactElementsInAnyOrder("container 1 packLines collection", System.Array.Empty<PackLine>(), container1.PackLines);
			AssertContainsExactElementsInAnyOrder("container 2 packLines collection", System.Array.Empty<PackLine>(), container2.PackLines);
			AssertContainsExactElementsInAnyOrder("container 3 packLines collection", new[] { packLine }, container3.PackLines);
			AssertContainsExactElementsInAnyOrder("packLine containers collection", new[] { container3 }, packLine.Containers);

			var pivot = Factory.New<JobContainerPackPivot>();
			pivot.J6_JC = container2.PK;
			pivot.J6_JL = packLine.PK;

			Factory.SaveInTransactionActions.Clear();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var container1_otherFactory = otherFactory.Load<CommonContainer>(container1.PK);
			var container2_otherFactory = otherFactory.Load<CommonContainer>(container2.PK);
			var container3_otherFactory = otherFactory.Load<CommonContainer>(container3.PK);
			var packLine_otherFactory = otherFactory.Load<PackLine>(packLine.PK);

			packLine_otherFactory.JL_JC = container1_otherFactory.PK;

			AssertContainsExactElementsInAnyOrder("container 1 packLines collection", new[] { packLine_otherFactory }, container1_otherFactory.PackLines);
			AssertContainsExactElementsInAnyOrder("container 2 packLines collection", System.Array.Empty<PackLine>(), container2_otherFactory.PackLines);
			AssertContainsExactElementsInAnyOrder("container 3 packLines collection", System.Array.Empty<PackLine>(), container3_otherFactory.PackLines);
			AssertContainsExactElementsInAnyOrder("packLine containers collection", new[] { container1_otherFactory }, packLine_otherFactory.Containers);
		}

		#endregion

		#region TestSettingContainerPackingOrder

		public void TestSettingContainerPackingOrder()
		{
			CommonConsol consol1 = Factory.New<CommonConsol>();
			CommonContainer container1 = consol1.Containers.AddNew();

			CommonConsol consol2 = Factory.New<CommonConsol>();
			CommonContainer container2 = consol2.Containers.AddNew();

			CommonShipment shipment = consol1.Shipments.AddNew();
			consol2.Shipments.Add(shipment);

			PackLine packLineA = shipment.OuterPackLines.AddNew();
			AssertEquals("the first number issued should be 1", 1, packLineA.JL_ContainerPackingOrder);

			PackLine packLineB = shipment.OuterPackLines.AddNew();
			AssertEquals("the second number issued should be 2", 2, packLineB.JL_ContainerPackingOrder);

			PackLine packLineC = shipment.OuterPackLines.AddNew();
			AssertEquals("the third number issued should be 3", 3, packLineC.JL_ContainerPackingOrder);

			packLineB.Delete();
			packLineB = shipment.OuterPackLines.AddNew();
			AssertEquals("dont fill gap's the new order should always have the heighest value", 4, packLineB.JL_ContainerPackingOrder);

			packLineC.Delete();
			packLineB.Delete();
			packLineB = shipment.OuterPackLines.AddNew();
			AssertEquals("use the smallest value greater than all the rest, dont leave unnessisary gaps", 2, packLineB.JL_ContainerPackingOrder);

			packLineC = shipment.OuterPackLines.AddNew();
			packLineC.Containers.RemoveAll();
			AssertEquals("Packing order was reset", 0, packLineC.JL_ContainerPackingOrder);

			packLineA.Containers.RemoveAll();
			packLineB.Containers.RemoveAll();

			packLineC.JL_JC = container1.PK;
			AssertEquals("the first number issued should be 1", 1, packLineC.JL_ContainerPackingOrder);

			packLineB.Containers.RemoveAll();
			AssertEquals("Packing order was reset", 0, packLineB.JL_ContainerPackingOrder);
			packLineB.JL_JC = container1.PK;
			AssertEquals("the second number issued should be 2", 2, packLineB.JL_ContainerPackingOrder);

			container1.PackLines.Remove(packLineB);
			AssertEquals("Packing order was reset", 0, packLineB.JL_ContainerPackingOrder);

			packLineC.Containers.Add(container2);
			AssertEquals("Packing Order isn't changed", 1, packLineC.JL_ContainerPackingOrder);
			packLineC.Containers.Remove(container2);
			AssertEquals("Packing Order isn't changed", 1, packLineC.JL_ContainerPackingOrder);

			container1.CreatedFromCusContainer = true;
			packLineB.JL_JC = container1.PK;
			AssertEquals("Packing Order isn't changed", 0, packLineB.JL_ContainerPackingOrder);
		}

		#endregion

		#region Implementation

		#region Consol

		CommonConsol Consol
		{
			get { return consol ?? (consol = Factory.New<CommonConsol>()); }
		}
		CommonConsol consol;

		#endregion

		#region Container

		CommonContainer Container
		{
			get { return container ?? (container = Consol.Containers.AddNew()); }
		}
		CommonContainer container;

		#endregion

		#region Shipment

		CommonShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<CommonShipment>()); }
		}
		CommonShipment shipment;

		#endregion

		#region PackLine

		PackLine PackLine
		{
			get { return packLine ?? (packLine = Shipment.OuterPackLines.AddNew()); }
		}
		PackLine packLine;

		#endregion

		#endregion
	}
}
