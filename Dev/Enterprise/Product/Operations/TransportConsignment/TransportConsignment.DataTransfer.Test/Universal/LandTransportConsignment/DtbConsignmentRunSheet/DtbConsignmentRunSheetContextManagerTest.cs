using System.Linq;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetDataContextManager))]
	class DtbConsignmentRunSheetContextManagerTest : ShipmentDataContextManagerTestCase<DtbConsignmentRunSheetDataContextManager, DtbConsignmentRunSheet>
	{
		#region TestDataContextType
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.TransportConsignmentRunSheet, new DtbConsignmentRunSheetDataContextManager().DataContextType);
			AssertEquals("Runsheet data context must be TransportConsignmentRunSheet.", DataContextType.TransportConsignmentRunSheet, Factory.New<DtbConsignmentRunSheet>().GetUniversalDataContextManager().DataContextType);
		}

		#endregion
		#region TestDataContextKey
		public void TestDataContextKey()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			runSheet.KG_RunSheetNumber = "R1";
			AssertEquals("R1", runSheet.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion
		#region TestDefaultOutputDirectory
		public void TestDefaultOutputDirectory()
		{
			AssertNull(new DtbConsignmentRunSheetDataContextManager().DefaultOutputDirectory);
		}

		#endregion
		#region TestManageShipments
		public void TestManageShipments()
		{
			AssertEquals(true, new DtbConsignmentRunSheetDataContextManager().ManagesShipments());
		}

		#endregion
		#region TestEventParentFinder
		public void TestGetLogParentsForEvent()
		{
			IEventDataContextManager manager = new DtbConsignmentRunSheetDataContextManager();
			AssertEquals(0, manager.GetLogParentsForEvent(new Event(), Factory.BOFactory, new TestErrorLogger()).Length);
		}

		#endregion
		#region TestEventContextValues
		public void TestEventContextValues()
		{
			var manager = new DtbConsignmentRunSheetDataContextManager() as IEventDataContextManager;
			AssertEquals(false, manager.EventContextValues.Any());
		}

		#endregion
		#region TestShipmentDataObjectWriter
		public void TestShipmentDataObjectWriter()
		{
			IShipmentDataContextManager manager = new DtbConsignmentRunSheetDataContextManager();
			AssertEquals(typeof(DtbConsignmentRunSheetDataObjectWriter), manager.GetShipmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetType());
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
