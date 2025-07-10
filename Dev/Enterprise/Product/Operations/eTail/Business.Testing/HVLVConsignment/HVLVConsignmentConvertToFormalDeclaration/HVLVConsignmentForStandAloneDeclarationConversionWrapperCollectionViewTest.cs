using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView))]
	public class HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionViewTest : NonPersistentBusinessObjectCollectionViewTestCase<HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView>
	{
		public void TestHVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var consignments = new HVLVShipmentConsignmentCollection(shipment);
			BuildImportConsignment(consignments, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignments, HVLVReleaseStatus.Held);
			BuildImportConsignment(consignments, HVLVReleaseStatus.Held);

			var consignmentsConvertToStandAloneDeclaration = new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection(consignments);
			var consignmentsConvertToStandAloneDeclarationView = new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView(consignmentsConvertToStandAloneDeclaration);

			AssertEquals("Expected three consignments in the collection", 3, consignmentsConvertToStandAloneDeclarationView.Count);
		}

		protected override HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView GetCollectionToTest()
		{
			var consignments = new HVLVShipmentConsignmentCollection(Factory.New<ForwardingShipment>());
			var consignmentsConvertToStandAloneDeclaration = new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollection(consignments);
			return new HVLVConsignmentForStandAloneDeclarationConversionWrapperCollectionView(consignmentsConvertToStandAloneDeclaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HVLVConsignmentForStandAloneDeclarationConversionWrapper();
		}

		HVLVConsignment BuildImportConsignment(HVLVShipmentConsignmentCollection consignments, string importReleaseStatus)
		{
			var consignment = consignments.AddNew();
			consignment.HVC_ImportReleaseStatus = importReleaseStatus;
			consignment.HVC_RN_NKShipperCountryCode = "NZ";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			return consignment;
		}
	}
}
