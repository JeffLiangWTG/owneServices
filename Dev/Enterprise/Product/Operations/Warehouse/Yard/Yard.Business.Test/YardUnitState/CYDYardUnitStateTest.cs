using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDYardUnitState))]
	public class CYDYardUnitStateTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDYardUnitState>();
		}

		public void TestNoteTypes()
		{
			var receiveAdvice = Factory.New<CYDYardUnitState>();

			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.HandlingInstructions,
				PredefinedNoteTypes.Instance.DeliveryInstructionsNote },
				receiveAdvice.NoteTypes);
		}

		#region eDocs Provider

		public void TestGetEDocsProviderSupport()
		{
			var businessObj = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), businessObj.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<CYDYardUnitState>();
			AssertEquals(Core.Constants.DocManagerCodes.CYDYardUnitState, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion

		public void TestClientAddress()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.Address1 = "TEST1";

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();

			var receiveAdviceLine = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;

			var yardUnit = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnit.YUS_YRL_ReceiveLine = receiveAdviceLine.PK;

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_ParentID = receiveAdvice.PK;
			jobDocAddress.E2_AddressType = "BKD";
			jobDocAddress.E2_OA_Address = orgAddress.PK;

			Factory.Save();

			var yus = Factory.Load<CYDYardUnitState>(yardUnit.PK);

			AssertEquals(orgAddress.PK, yus.ClientAddress.PK);
			AssertEquals("TEST1", yus.ClientAddress.Address1);
		}

		public void TestRefContainer()
		{
			var container = Factory.NewWithValidTestData<RefContainer>();

			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			delivery.UnitLineItem.YLI_RC_ContainerType = container.PK;

			var yardUnit = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnit.YUS_YDL_Delivery = delivery.PK;

			Factory.Save();

			var yus = Factory.Load<CYDYardUnitState>(yardUnit.PK);

			AssertEquals(container.PK, yus.Container.PK);
		}

		public void TestFreeStorageDays()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var client = Helper.CreateClient();
			var receiveAdvice = Helper.CreateReceiveAdvice(client, yard);
			var emptyContainerOf20Feet = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN000001");
			var fullContainerOf20Feet = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN000002");
			var emptyContainerOf40Feet = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN000003");
			var fullContainerOf40Feet = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN000004");
			var emptyContainerOf45Feet = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN000005");
			var unloadTime = new ZDateTimeOffset(2025, 2, 1);
			var unloadLocation = row.Locations[0];
			Helper.UnloadYardUnit(emptyContainerOf20Feet, unloadTime, unloadLocation, "20GP", isContainerEmpty: true);
			Helper.UnloadYardUnit(fullContainerOf20Feet, unloadTime, unloadLocation, "20GP", isContainerEmpty: false);
			Helper.UnloadYardUnit(emptyContainerOf40Feet, unloadTime, unloadLocation, "40GP", isContainerEmpty: true);
			Helper.UnloadYardUnit(fullContainerOf40Feet, unloadTime, unloadLocation, "40GP", isContainerEmpty: false);
			Helper.UnloadYardUnit(emptyContainerOf45Feet, unloadTime, unloadLocation, "45HC", isContainerEmpty: false);

			Helper.CreateYardStorageFreeDays(client, null, unitType: string.Empty, unitLength: 0, containerClass: string.Empty, unitLoad: string.Empty, transportMode: string.Empty, freeDays: 1);
			Helper.CreateYardStorageFreeDays(client, null, unitType: "CNT", unitLength: 0, containerClass: string.Empty, unitLoad: string.Empty, transportMode: "ROA", freeDays: 2);
			Helper.CreateYardStorageFreeDays(client, null, unitType: string.Empty, unitLength: 20.0, containerClass: string.Empty, unitLoad: string.Empty, transportMode: string.Empty, freeDays: 3);
			Helper.CreateYardStorageFreeDays(client, null, unitType: string.Empty, unitLength: 0, containerClass: "20F", unitLoad: string.Empty, transportMode: "ROA", freeDays: 4);
			Helper.CreateYardStorageFreeDays(client, yard, unitType: "CNT", unitLength: 20.0, containerClass: string.Empty, unitLoad: "EMP", transportMode: string.Empty, freeDays: 5);
			Helper.CreateYardStorageFreeDays(client, yard, unitType: "CNT", unitLength: 40.0, containerClass: string.Empty, unitLoad: "LAD", transportMode: "ROA", freeDays: 6);
			Helper.CreateYardStorageFreeDays(client, yard, unitType: string.Empty, unitLength: 20.0, containerClass: string.Empty, unitLoad: "LAD", transportMode: string.Empty, freeDays: 7);
			Helper.CreateYardStorageFreeDays(client, yard, unitType: string.Empty, unitLength: 40.0, containerClass: string.Empty, unitLoad: "EMP", transportMode: "ROA", freeDays: 8);

			Factory.Save();

			AssertEquals(5, emptyContainerOf20Feet.FreeStorageDays);
			AssertEquals(7, fullContainerOf20Feet.FreeStorageDays);
			AssertEquals(8, emptyContainerOf40Feet.FreeStorageDays);
			AssertEquals(6, fullContainerOf40Feet.FreeStorageDays);
			AssertEquals(2, emptyContainerOf45Feet.FreeStorageDays);
		}

		#region Implementation

		CYDYardTestHelper Helper
		{
			get { return helper ?? (helper = new CYDYardTestHelper(Factory)); }
		}

		CYDYardTestHelper helper;

		#endregion

		public void DocAddressesIsEmpty()
		{
			var yardUnitState = Factory.New<CYDYardUnitState>();
			var docAddresses = yardUnitState.DocAddresses;
			AssertEquals(0, docAddresses.Count);
		}

		public void TestSupportedAddressTypes()
		{
			var docAdresses = (IDocAddresses)Factory.New<CYDYardUnitState>();
			var supportedAddressTypes = docAdresses.SupportedAddressTypes;
			var expectedSupportedAddressTypes = new[]
				{
					DocAddressType.InsuredByDocumentaryAddress,
					DocAddressType.SurveyReportPartyDocumentaryAddress
				};
			AssertSequencesEqual(expectedSupportedAddressTypes, supportedAddressTypes);
		}

		#region Insurer

		public void TestInsurerIsPopulatedCorrectly()
		{
			var yardUnitState = (CYDYardUnitState)GetNewBusinessObject();
			AssertEquals(DocAddressType.InsuredByDocumentaryAddress, yardUnitState.Insurer.DocAddressType);
			AssertEquals(ContactType.LocalClient, yardUnitState.Insurer.DefaultContactType);
			AssertEquals(yardUnitState, yardUnitState.Insurer.Parent);
		}

		#endregion

		#region ThirdParty

		public void TestThirdPartyIsPopulatedCorrectly()
		{
			var yardUnitState = (CYDYardUnitState)GetNewBusinessObject();
			AssertEquals(DocAddressType.SurveyReportPartyDocumentaryAddress, yardUnitState.ThirdParty.DocAddressType);
			AssertEquals(ContactType.LocalClient, yardUnitState.ThirdParty.DefaultContactType);
			AssertEquals(yardUnitState, yardUnitState.ThirdParty.Parent);
		}

		#endregion
	}
}
