using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class LinehaulAndRunSheetCostSupporterTest : DtbBookingConsignmentTestCaseWithFactory
	{
		public void TestShipmentList()
		{
			var parent = Factory.New<DummyBusinessObject>();
			IGenericJobCostSupporter costSupporter = new DummyLinehaulAndRunSheetCostSupporter(parent);

			var consignment1 = Helper.CreateBookingConsignmentWithTemplate();
			var consignment2 = Helper.CreateBookingConsignmentWithTemplate();

			var consignments = new DtbBookingConsignment[] { consignment1, consignment2 };
			((DummyLinehaulAndRunSheetCostSupporter)costSupporter).ConsignmentsToReturn = consignments;

			AssertCollectionContains(consignment1, costSupporter.ShipmentsList);
			AssertCollectionContains(consignment2, costSupporter.ShipmentsList);

			AssertCollectionContains(consignment1.PK, costSupporter.ShipmentsListPKs);
			AssertCollectionContains(consignment2.PK, costSupporter.ShipmentsListPKs);
		}

		public void TestIGenericJobCostSupporter()
		{
			var parent = Factory.New<DummyBusinessObject>();
			IGenericJobCostSupporter costSupporter = new DummyLinehaulAndRunSheetCostSupporter(parent);
			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);

			AssertEquals("ConsolMode", "", costSupporter.ConsolMode);
			AssertEquals("ETA", ZDateTime.Empty, costSupporter.ETA);
			AssertEquals("ETD", ZDateTime.Empty, costSupporter.ETD);
			AssertEquals("HasChanges", parent.HasChanges, costSupporter.HasChanges);
			AssertEquals("IsBuyersConsol", false, costSupporter.IsBuyersConsol);
			AssertEquals("Direction", Directions.Unknown, costSupporter.Direction);
			AssertEquals("IsInDatabase", parent.IsInDatabase, costSupporter.IsInDatabase);
			AssertEquals("MasterBillNum", "", costSupporter.MasterBillNum);
			AssertEquals("PK", parent.PK, costSupporter.PK);
			AssertEquals("PortOfDischarge", "", costSupporter.PortOfDischarge);
			AssertEquals("PortOfLoading", "", costSupporter.PortOfLoading);
			AssertEquals("TotalChargeableUnit", Constants.Weight.Kilograms, costSupporter.TotalChargeableUnit);
			AssertEquals("TransportMode", "", costSupporter.TransportMode);
			AssertEquals("Type", parent.TablePrefix, costSupporter.Type);

			AssertNull("ReceivingForwarder", costSupporter.ReceivingForwarder);
			AssertNull("SendingForwarder", costSupporter.SendingForwarder);
			AssertNull("Shipments", costSupporter.Shipments);

			AssertEquals("ExcludedApportionmentMethods", 1, costSupporter.ExcludedApportionmentMethods.Count());
			AssertEquals("IsApportionmentFilterEnabled", true, costSupporter.IsApportionmentFilterEnabled);
			AssertEquals("JobConsolCostingCheckPoint", Env.Security.None, costSupporter.JobConsolCostingCheckPoint);

			var org = Helper.CreateOrganisation("RS");
			((DummyLinehaulAndRunSheetCostSupporter)costSupporter).CreditorToReturnPk = org.PK;

			var chargeCodeGroupCodes = ((ChargeCodeGroupList)System.Activator.CreateInstance(typeof(ChargeCodeGroupList), null));
			var validCodeGroup = new[] { ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Codes.TransportBooking };
			foreach (var chargeCodeGroup in chargeCodeGroupCodes)
			{
				var code = chargeCodeGroup.ToString();
				var expectCreditor = validCodeGroup.Contains(code);
				var expectedCreditor = expectCreditor ? org.PK : ZGuid.Empty;
				AssertEquals(string.Format("Get Creditor {0} have a creditor for ChargeGroupCode {1}", expectCreditor ? "should" : "should not", code), expectedCreditor, costSupporter.GetCreditorPK(code, ZGuid.Empty));
			}
		}

		public void TestIGenericJobCostSupporter_DocumentSupportable()
		{
			var parent = Factory.New<DummyBusinessObject>();
			IGenericJobCostSupporter costSupporter = new DummyLinehaulAndRunSheetCostSupporter(parent);
			AssertNull("DocumentSupporter", costSupporter.DocumentSupporter);

			var parent2 = Factory.New<DummyBusinessObjectWithDocuments>();
			IGenericJobCostSupporter costSupporterWithDocuments = new DummyLinehaulAndRunSheetCostSupporter(parent2);
			AssertNotNull("DocumentSupporter", costSupporterWithDocuments.DocumentSupporter);
		}
	}
}
