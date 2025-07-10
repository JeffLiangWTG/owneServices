using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class WhsItemTransportationUnitCostSupporterTest<T> : TestCaseWithFactory
		where T : BusinessObject, ITransportationUnitForApportioning, IDocumentSupportable
	{
		public void TestIGenericJobCostSupporter()
		{
			var transportationUnit = Factory.NewWithValidTestData<T>();
			var costSupporter = GetCostSupporter(transportationUnit);

			AssertEquals("ConsolMode", ZString.Empty, costSupporter.ConsolMode);
			AssertEquals("ETA", transportationUnit.ETA, costSupporter.ETA);
			AssertEquals("ETD", transportationUnit.ETD, costSupporter.ETD);
			AssertEquals("HasChanges", transportationUnit.HasChanges, costSupporter.HasChanges);
			AssertEquals("IsBuyersConsol", false, costSupporter.IsBuyersConsol);
			AssertEquals("Direction", Directions.Unknown, costSupporter.Direction);
			AssertEquals("IsInDatabase", transportationUnit.IsInDatabase, costSupporter.IsInDatabase);
			AssertEquals("MasterBillNum", transportationUnit.MasterBillNum, costSupporter.MasterBillNum);
			AssertEquals("PK", transportationUnit.PK, costSupporter.PK);
			AssertEquals("PortOfDischarge", ZString.Empty, costSupporter.PortOfDischarge);
			AssertEquals("PortOfLoading", ZString.Empty, costSupporter.PortOfLoading);
			AssertEquals("TotalChargeableUnit", ZString.Empty, costSupporter.TotalChargeableUnit);
			AssertEquals("TransportMode", ZString.Empty, costSupporter.TransportMode);
			AssertEquals("Type", transportationUnit.TablePrefix, costSupporter.Type);
			AssertEquals("FreeSpace", ZDecimal.Zero, costSupporter.FreeSpace);

			AssertNull("ReceivingForwarder", costSupporter.ReceivingForwarder);
			AssertNull("SendingForwarder", costSupporter.SendingForwarder);
			AssertNull("Shipments", costSupporter.Shipments);
			AssertNotNull("DocumentSupporter", costSupporter.DocumentSupporter);

			Assert("IsApportionmentFilterEnabled", costSupporter.IsApportionmentFilterEnabled);
			AssertEquals("ExcludedApportionmentMethods", 6, costSupporter.ExcludedApportionmentMethods.Count());
			AssertContainsExactElementsInAnyOrder(
				new[] {
					AllocationMethod.Revenue,
					AllocationMethod.ChargeableUnits,
					AllocationMethod.ContainerCount,
					AllocationMethod.TwentyFootEquivalentUnit,
					AllocationMethod.CapacityPerContainer,
					AllocationMethod.FreeSpaceContribution,
				},
				costSupporter.ExcludedApportionmentMethods);

			var chargeCodeGroupCodes = (ChargeCodeGroupList)System.Activator.CreateInstance(typeof(ChargeCodeGroupList), null);
			foreach (var chargeCodeGroup in chargeCodeGroupCodes)
			{
				var code = chargeCodeGroup.ToString();
				var expectCreditor = transportationUnit.DefaultChargeGroups.Contains(code);
				AssertEquals(
					string.Format("Get Creditor {0} have a creditor for ChargeGroupCode {1}", expectCreditor ? "should" : "should not", code),
					expectCreditor ? transportationUnit.CreditorPK : ZGuid.Empty,
					costSupporter.GetCreditorPK(code, ZGuid.Empty));
			}
		}

		#region Implementation

		protected abstract IGenericJobCostSupporter GetCostSupporter(T transportationUnit);

		protected WhsWarehouse Warehouse => warehouse ?? (warehouse = Helper.CreateTRWWarehouse());
		WhsWarehouse warehouse;

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
