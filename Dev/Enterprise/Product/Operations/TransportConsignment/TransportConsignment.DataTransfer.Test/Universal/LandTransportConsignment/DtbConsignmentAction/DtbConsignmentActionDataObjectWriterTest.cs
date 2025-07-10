using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentActionDataObjectWriterTest : DtbConsignmentUniversalTestCase
	{
		#region TestPopulateConsignmentAction
		public void TestPopulateConsignmentAction()
		{
			var action = Factory.New<DtbConsignmentAction>();
			var package = CreatePackage(Constants.PkgUnit.Box);
			var containerPackage = CreatePackage(Constants.PkgUnit.Container);
			var packageDictionary = new Dictionary<ZGuid, ZInt>
			{
				{ package.PK, 1 },
				{ containerPackage.PK, 2 }
			};
			CreateActionPackageDivot(action, package);
			CreateActionPackageDivot(action, containerPackage);

			action.LTA_ActualTime = new ZDateTimeOffset(2025, 1, 26);
			action.LTA_ActionType = ActionTypes.Codes.PickUp;
			action.LTA_EstimatedTime = new ZDateTimeOffset(2025, 1, 27);
			action.LTA_Slot = new ZDateTimeOffset(2025, 1, 28);
			action.LTA_RequiredTo = new ZDateTimeOffset(2025, 1, 30);
			action.LTA_SignedBy = "Test";
			action.LTA_ReferenceNumber = "reference";
			action.LTA_RequiredFrom = new ZDateTimeOffset(2025, 1, 29);

			var actionDataObject = new DtbConsignmentActionDataObjectWriter(new DataWritingManager(new DummyActionInfo()), packageDictionary).GetDataObject(action);
			var nonContainterPackingLink = actionDataObject.PackingLinkCollection.Find((p) => p.PackingLineLink == 1);
			var containerPackingLink = actionDataObject.PackingLinkCollection.Find((p) => p.PackingLineLink == 2);

			AssertEquals(new ZDateTimeOffset(2025, 1, 26).ToLocalZDateTime(), actionDataObject.ActualDate);
			AssertEquals("PIC", actionDataObject.DateDescription);
			AssertEquals(new ZDateTimeOffset(2025, 1, 28).ToLocalZDateTime(), actionDataObject.SlotDate);
			AssertEquals(new ZDateTimeOffset(2025, 1, 30).ToLocalZDateTime(), actionDataObject.RequiredToDate);
			AssertEquals("Test", actionDataObject.ReceivedBy);
			AssertEquals("reference", actionDataObject.Reference);
			AssertEquals(new ZDateTimeOffset(2025, 1, 29).ToLocalZDateTime(), actionDataObject.RequiredFromDate);
			AssertEquals(new ZDateTimeOffset(2025, 1, 27).ToLocalZDateTime(), actionDataObject.EstimatedDate);
			AssertEquals(2, actionDataObject.PackingLinkCollection.Count);
			AssertNotNull(nonContainterPackingLink);
			AssertEquals(ZBool.False, nonContainterPackingLink.IsContainer);
			AssertNotNull(containerPackingLink);
			AssertEquals(ZBool.True, containerPackingLink.IsContainer);
		}

		void CreateActionPackageDivot(DtbConsignmentAction action, PkgPackage package)
		{
			var divot = Factory.New<DtbConsignmentActionPackageDivot>();
			divot.LTP_LTA_ConsignmentAction = action.PK;
			divot.LTP_KP_Package = package.PK;
			divot.LTP_PackageQuantity = 1;
		}

		PkgPackage CreatePackage(ZString packType)
		{
			var package = Factory.New<PkgPackage>();
			package.KP_PackageQty = 1;
			package.KP_F3_NKPackType = packType;
			return package;
		}
		#endregion
	}
}
