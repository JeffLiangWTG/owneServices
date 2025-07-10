using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(USInBondMoveHeader))]
	class USInBondMoveHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadViewFromInBondMovements()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "INB12345678";
			header.BH_CarrierSCAC = "SCAC";
			header.BH_ETA = new ZDateTime(2022, 07, 25);
			header.BH_FIRMS = "W235";
			header.BH_FTZMove = false;
			header.BH_ImportConveyanceCountry = Core.Constants.CountryCodes.Australia;
			header.BH_ImportConveyanceName = "HELLO VESSEL";
			header.BH_ImportLoadPortKCode = "20182";
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MWB123";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			moveHeader.BM_DestinationPortCode = "1234";
			moveHeader.BM_ForeignDestPortKCode = "R!456";
			moveHeader.BM_InBondCarrierSCAC = "EFGH";
			moveHeader.BM_InBondClosedDate = new ZDateTime(2022, 07, 28);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill.PK;
			Factory.Save();

			var moveHeaderView = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
			AssertNotNull("View is loaded", moveHeaderView);
			CombineAssertions("Properties populated from view", () =>
			{
				AssertEquals("moveHeaderView.PK", moveHeader.PK, moveHeaderView.PK);
				AssertEquals("moveHeaderView.BMH_BH", header.PK, moveHeaderView.BMH_BH);
				AssertEquals("moveHeaderView.BMH_CarrierSCAC", "SCAC", moveHeaderView.BMH_CarrierSCAC);
				AssertEquals("moveHeaderView.BMH_CustomsStatus", "CDO", moveHeaderView.BMH_CustomsStatus);
				AssertEquals("moveHeaderView.BMH_DestinationPortCode", "1234", moveHeaderView.BMH_DestinationPortCode);
				AssertEquals("moveHeaderView.BMH_ETA", new ZDateTime(2022, 07, 25), moveHeaderView.BMH_ETA);
				AssertEquals("moveHeaderView.BMH_FIRMS", "W235", moveHeaderView.BMH_FIRMS);
				AssertEquals("moveHeaderView.BMH_ForeignDestPortKCode", "R!456", moveHeaderView.BMH_ForeignDestPortKCode);
				AssertEquals("moveHeaderView.BMH_FTZMove", false, moveHeaderView.BMH_FTZMove);
				AssertEquals("moveHeaderView.BMH_GB", GlbBranch.CurrentBranch.PK, moveHeaderView.BMH_GB);
				AssertEquals("moveHeaderView.BMH_ImportConveyanceCountry", "AU", moveHeaderView.BMH_ImportConveyanceCountry);
				AssertEquals("moveHeaderView.BMH_ImportConveyanceName", "HELLO VESSEL", moveHeaderView.BMH_ImportConveyanceName);
				AssertEquals("moveHeaderView.BMH_ImportLoadPortKCode", "20182", moveHeaderView.BMH_ImportLoadPortKCode);
				AssertEquals("moveHeaderView.BMH_ImportTransportMode", "40", moveHeaderView.BMH_ImportTransportMode);
				AssertEquals("moveHeaderView.BMH_InBondCarrierSCAC", "EFGH", moveHeaderView.BMH_InBondCarrierSCAC);
				AssertEquals("moveHeaderView.BMH_InBondClosedDate", new ZDateTime(2022, 07, 28), moveHeaderView.BMH_InBondClosedDate);
				AssertEquals("moveHeaderView.BMH_InBondEntryType", "61", moveHeaderView.BMH_InBondEntryType);
				AssertEquals("moveHeaderView.BMH_JobReference", "INB12345678", moveHeaderView.BMH_JobReference);
			});
		}

		public void TestBMH_ForeignDestinationPortName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "12031", "SINGLETON", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "12031", "Test 1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "12031", "Test 2", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "3041");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, "3041", "12031", "Test 3", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_ForeignDestPortKCode = "12031";
			Factory.Save();

			var moveHeaderView = Factory.Load<USInBondMoveHeader>(moveHeader.PK);

			AssertEquals("moveHeaderView.BMH_ForeignDestinationPortName", "SINGLETON", moveHeaderView.BMH_ForeignDestinationPortName);
			AssertEquals("Cached", Factory.GetCachedValue("USInBondMoveHeader|BMH_ForeignDestinationPortName|12031", () => ZString.Empty), moveHeaderView.BMH_ForeignDestinationPortName);
		}

		public void TestIDocumentSupportableMembers()
		{
			var documentSupporter = (IDocumentSupportable)GetNewBusinessObject();
			AssertNotNull(documentSupporter.DocumentSupporter);
			AssertEquals(typeof(USInBondMoveHeaderDocumentSupporter), documentSupporter.DocumentSupporter.GetType());
		}

		public void TestIStmALogParentMembers()
		{
			var movement = (USInBondMoveHeader)GetNewBusinessObject();
			var logParent = (IStmALogParent)movement;
			AssertEquals(CusInBondMoveHeader.Schema.TableName, logParent.LogsParentTableName);
		}

		public void TestIParentDocManagerSupport()
		{
			var movement = (USInBondMoveHeader)GetNewBusinessObject();
			IParentDocManagerSupport supporter = movement;
			AssertEquals(Core.Constants.DocManagerCodes.InBond, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", movement.MoveHeader.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", movement.MoveHeader.TableName, supporter.ParentTableName);
		}

		public void TestIDocManagerSupport()
		{
			var movement = (USInBondMoveHeader)GetNewBusinessObject();
			AssertEquals(Core.Constants.DocManagerCodes.InBond, ((IDocManagerSupport)movement).DocManagerInfo.DocManagerCode);
		}

		public void TestIDocumentDeliveredLogSupporterMembers()
		{
			var movement = (USInBondMoveHeader)GetNewBusinessObject();
			var logSupporter = (IDocumentDeliveredLogSupporter)movement;
			AssertEquals(typeof(CusInBondMoveHeader), logSupporter.BusinessObjectTypeToLogAgainst);
			AssertEquals(movement.PK, logSupporter.Identifier);
		}

		public void TestIControllerIDProvider()
		{
			var movement = (USInBondMoveHeader)GetNewBusinessObject();
			var provider = (IControllerIDProvider)movement;
			AssertEquals(movement.PK.ToGuid(), provider.BusinessObjectPK);
			AssertEquals(ControllerIDs.Customs.US.InBondMoveHeader, provider.ControllerID);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(USInBondMoveHeader)));

			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			Factory.Save();
			AssertEquals(1, Factory.GetDatabaseCount(typeof(USInBondMoveHeader)));

			moveHeader.Delete();
			Factory.Save();
			AssertEquals(0, Factory.GetDatabaseCount(typeof(USInBondMoveHeader)));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			Factory.Save();
			return Factory.Load<USInBondMoveHeader>(moveHeader.PK);
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert(true); // When view is created from movements, SetDefaultValues will not be actually called
		}
	}
}
