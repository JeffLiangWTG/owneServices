using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetInstructionDataContextManager))]
	class DtbConsignmentRunSheetInstructionContextManagerTest : ShipmentDataContextManagerTestCase<DtbConsignmentRunSheetInstructionDataContextManager, DtbConsignmentRunSheetInstruction>
	{
		#region TestDataContextType
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.TransportConsignmentRunSheetInstruction, new DtbConsignmentRunSheetInstructionDataContextManager().DataContextType);
			AssertEquals("Runsheet data context must be TransportConsignmentRunSheetInstruction.", DataContextType.TransportConsignmentRunSheetInstruction, Factory.New<DtbConsignmentRunSheetInstruction>().GetUniversalDataContextManager().DataContextType);
		}

		#endregion
		#region TestBusinessObjectImplementsIJobNumber
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		#endregion
		#region TestDataContextKey
		public void TestDataContextKey()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.KG_RunSheetNumber = "KG1";
			var address = Factory.New<OrgAddress>();
			address.Address1 = "TEST ADDRESS";
			var helper = new TransportConsignmentTestHelper(Factory.BOFactory);
			var consignment = helper.CreateConsignment();
			var consignmentAddress = helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, address);
			var action = consignmentAddress.Actions.AddNew();
			var instruction = helper.CreateRunSheetInstruction(runSheet, action);
			AssertEquals("KG1-TEST ADDRESS", instruction.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion
		#region TestDefaultOutputDirectory
		public void TestDefaultOutputDirectory()
		{
			AssertNull(new DtbConsignmentRunSheetInstructionDataContextManager().DefaultOutputDirectory);
		}

		#endregion
		#region TestManageShipments
		public void TestManageShipments()
		{
			AssertEquals(true, new DtbConsignmentRunSheetInstructionDataContextManager().ManagesShipments());
		}

		#endregion
		#region TestEventParentFinder
		public void TestGetLogParentsForEvent()
		{
			IEventDataContextManager manager = new DtbConsignmentRunSheetInstructionDataContextManager();
			AssertEquals(0, manager.GetLogParentsForEvent(new Event(), Factory.BOFactory, new TestErrorLogger()).Length);
		}

		#endregion
		#region TestEventContextValues
		public void TestEventContextValues()
		{
			var manager = new DtbConsignmentRunSheetInstructionDataContextManager() as IEventDataContextManager;
			AssertEquals(false, manager.EventContextValues.Any());
		}

		#endregion
		#region TestShipmentDataObjectWriter
		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new DtbConsignmentRunSheetInstructionDataContextManager();
			AssertEquals(typeof(DtbConsignmentRunSheetInstructionDataObjectWriter), manager.GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetType());
		}

		#endregion
		#region SupportedRecipientRoleTypes
		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get
			{
				return System.Array.Empty<RecipientRoleType>();
			}
		}

		#endregion
		#region ValidPopulatedUniversalShipmentXML
		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return "";
			}
		}

		#endregion
		#region ManagerChecksDataTargetToImport
		protected override bool ManagerChecksDataTargetToImport
		{
			get
			{
				return false;
			}
		}
		#endregion
	}
}
