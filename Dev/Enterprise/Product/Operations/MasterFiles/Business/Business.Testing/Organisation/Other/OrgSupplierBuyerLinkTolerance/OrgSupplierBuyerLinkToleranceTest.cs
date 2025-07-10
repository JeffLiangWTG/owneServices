using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierBuyerLinkTolerance))]
	public class OrgSupplierBuyerLinkToleranceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectForFetchForLoad();

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var supplierBuyerLinkTolerance = (OrgSupplierBuyerLinkTolerance)base.GetBusinessObjectForFetchForLoad();
			supplierBuyerLinkTolerance.OLT_TransportMode = Constants.TransportModes.All;
			return supplierBuyerLinkTolerance;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var supplierBuyerLinkTolerance = (OrgSupplierBuyerLinkTolerance)base.GetNewBusinessObjectForDeleteTest(factory);
			supplierBuyerLinkTolerance.OLT_TransportMode = Constants.TransportModes.All;
			return supplierBuyerLinkTolerance;
		}

		#region TestDefaultValues

		public void TestDefaultValues()
		{
			var tolerance = Factory.New<OrgSupplierBuyerLinkTolerance>();

			AssertEquals("OLT_TransportMode", Constants.TransportModes.All, tolerance.OLT_TransportMode);
			AssertEquals("OLT_PartNumber", ZString.Empty, tolerance.OLT_PartNumber);
		}

		#endregion

		#region TestGetExistingOrgSupplierBuyerLinkTolerance

		public void TestGetExistingOrgSupplierBuyerLinkTolerance()
		{
			const string partNum = "PartNum";
			const string differentPartNum = "DifferentPartNum";
			const string transportMode = Constants.TransportModes.Air;
			const string differentTransportMode = Constants.TransportModes.Sea;

			var currentLink = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			var otherLink = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();

			var currentPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			currentPart.OP_PartNum = partNum;
			currentPart.RelatedOrganisations.AddOrganisationIfNotExist(currentLink.OL_OH_Supplier, OrgPartRelation.RelationshipTypes.Supplier);
			currentPart.RelatedOrganisations.AddOrganisationIfNotExist(currentLink.OL_OH_Buyer, OrgPartRelation.RelationshipTypes.Owner);
			currentPart.OP_IsActive = true;

			var otherPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			otherPart.OP_PartNum = partNum;
			otherPart.RelatedOrganisations.AddOrganisationIfNotExist(otherLink.OL_OH_Supplier, OrgPartRelation.RelationshipTypes.Supplier);
			otherPart.RelatedOrganisations.AddOrganisationIfNotExist(otherLink.OL_OH_Buyer, OrgPartRelation.RelationshipTypes.Owner);
			otherPart.OP_IsActive = true;

			var currentToleranceWithTransportModeAndPartNumber = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
			currentToleranceWithTransportModeAndPartNumber.OLT_OL_SupplierBuyerLink = currentLink.PK;
			currentToleranceWithTransportModeAndPartNumber.OLT_TransportMode = transportMode;
			currentToleranceWithTransportModeAndPartNumber.OLT_PartNumber = partNum;

			var currentToleranceWithTransportMode = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
			currentToleranceWithTransportMode.OLT_OL_SupplierBuyerLink = currentLink.PK;
			currentToleranceWithTransportMode.OLT_TransportMode = transportMode;
			currentToleranceWithTransportMode.OLT_PartNumber = ZString.Empty;

			var currentToleranceWithPartNumber = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
			currentToleranceWithPartNumber.OLT_OL_SupplierBuyerLink = currentLink.PK;
			currentToleranceWithPartNumber.OLT_TransportMode = Constants.TransportModes.All;
			currentToleranceWithPartNumber.OLT_PartNumber = partNum;

			var currentTolerance = Factory.NewWithValidTestData<OrgSupplierBuyerLinkTolerance>();
			currentTolerance.OLT_OL_SupplierBuyerLink = currentLink.PK;
			currentTolerance.OLT_TransportMode = Constants.TransportModes.All;
			currentTolerance.OLT_PartNumber = ZString.Empty;

			Factory.Save();

			var tolerance = OrgSupplierBuyerLinkTolerance.GetExistingOrgSupplierBuyerLinkTolerance(null, transportMode, partNum);
			AssertNull("Should return null when no link is provided.", tolerance);

			tolerance = OrgSupplierBuyerLinkTolerance.GetExistingOrgSupplierBuyerLinkTolerance(otherLink, transportMode, partNum);
			AssertNull("Should return null when no tolerance is found.", tolerance);

			tolerance = OrgSupplierBuyerLinkTolerance.GetExistingOrgSupplierBuyerLinkTolerance(currentLink, transportMode, partNum);
			AssertEquals("A tolerance with transport mode and part number is found.", currentToleranceWithTransportModeAndPartNumber, tolerance);

			tolerance = OrgSupplierBuyerLinkTolerance.GetExistingOrgSupplierBuyerLinkTolerance(currentLink, differentTransportMode, partNum);
			AssertEquals("A tolerance with transport mode 'All' and part number is found.", currentToleranceWithPartNumber, tolerance);

			tolerance = OrgSupplierBuyerLinkTolerance.GetExistingOrgSupplierBuyerLinkTolerance(currentLink, transportMode, differentPartNum);
			AssertEquals("A tolerance with transport mode and empty part number is found.", currentToleranceWithTransportMode, tolerance);

			tolerance = OrgSupplierBuyerLinkTolerance.GetExistingOrgSupplierBuyerLinkTolerance(currentLink, differentTransportMode, differentPartNum);
			AssertEquals("A tolerance with transport mode 'All' and empty part number is found.", currentTolerance, tolerance);
		}

		#endregion
	}
}
