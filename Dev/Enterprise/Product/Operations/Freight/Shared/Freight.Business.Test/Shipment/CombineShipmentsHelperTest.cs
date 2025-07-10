using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CombineShipmentsHelperTest : BaseFreightTest
	{
		#region Shipments can be Combined

		public void TestShipmentCanBeCombined()
		{
			CombineShipmentsHelper helper = new CombineShipmentsHelper(SA1, Sailing);
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined());
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined(SA2));
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined(SA3));
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined(SA4));
			Assert("Not expecting CommonShipment to be able to be combined, consignee is empty.", !helper.ShipmentCanBeCombined(SA5));
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined(SA6));
			Assert("Not expecting CommonShipment to be able to be combined, sailing is empty.", !helper.ShipmentCanBeCombined(SA7));
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined(SA8));
			Assert("Not expecting CommonShipment to be able to be combined, has jobcharge.", !helper.ShipmentCanBeCombined(SA9));
			Assert("Not expecting CommonShipment to be able to be combined, is referenced by another shipment.", !helper.ShipmentCanBeCombined(SA10));
			Assert("Not expecting CommonShipment to be able to be combined, references another shipment.", !helper.ShipmentCanBeCombined(SA11));

			helper = new CombineShipmentsHelper(SB1, Consol);
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined());
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined(SB2));
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined(SB3));
			Assert("Expecting CommonShipment to be able to be combined.", helper.ShipmentCanBeCombined(SB4));
			Assert("Not expecting CommonShipment to be able to be combined, consol is empty.", !helper.ShipmentCanBeCombined(SB5));

			SB2.JS_IsCancelled = true;
			AssertEquals("Couldn't be combined: shipment is cancelled", false, helper.ShipmentCanBeCombined(SB2));

			SB2.JS_IsCancelled = false;
			AssertEquals("Could be combined", true, helper.ShipmentCanBeCombined(SB2));
		}

		#endregion

		#region Test ShipmentHasSiblings_SailingPerspective()

		public void TestShipmentHasSiblings_SailingPerspective()
		{
			AssertNotNull("Precondition: Sailing should not be null.", Sailing);

			CombineShipmentsHelper helper1 = new CombineShipmentsHelper(SA6, Sailing);
			Assert("Not Expecting CommonShipment SA6 to have siblings.", !helper1.ShipmentHasSiblings);

			CombineShipmentsHelper helper2 = new CombineShipmentsHelper(SA1, Sailing);
			Assert("Expecting CommonShipment SA1 to have siblings.", helper2.ShipmentHasSiblings);
		}

		#endregion

		#region TestRelatedShipments_SailingPerspective()

		public void TestRelatedShipments_SailingPerspective()
		{
			AssertNotNull("Precondition: SA6.Sailing should not be null.", SA6.Sailing);

			CombineShipmentsHelper helper1 = new CombineShipmentsHelper(SA6, SA6.Sailing);
			AssertEquals("Not Expecting CommonShipment SA6 to have siblings.", 0, helper1.RelatedShipments.Count);
			AssertNotNull("Precondition: SA1.Sailing should not be null.", SA1.Sailing);

			CombineShipmentsHelper helper2 = new CombineShipmentsHelper(SA1, SA1.Sailing);
			AssertEquals("Expecting CommonShipment SA1 to have 3 siblings.", 3, helper2.RelatedShipments.Count);
		}

		#endregion

		#region Test ShipmentHasSiblings_ConsolPerspective()

		public void TestShipmentHasSiblings_ConsolPerspective()
		{
			AssertNotNull("Precondition: Consol should not be null.", Consol);

			CombineShipmentsHelper helper1 = new CombineShipmentsHelper(SB4, Consol);
			Assert("Not Expecting CommonShipment SB4 to have siblings.", !helper1.ShipmentHasSiblings);

			CombineShipmentsHelper helper2 = new CombineShipmentsHelper(SB1, Consol);
			Assert("Expecting CommonShipment SB1 to have siblings.", helper2.ShipmentHasSiblings);
		}

		#endregion

		#region TestRelatedShipments_ConsolPerspective()

		public void TestRelatedShipments_ConsolPerspective()
		{
			AssertNotNull("Precondition: Consol should not be null.", Consol);

			CombineShipmentsHelper helper2 = new CombineShipmentsHelper(SB1, Consol);
			AssertEquals("Expecting CommonShipment SB1 to have 2 siblings.", 2, helper2.RelatedShipments.Count);
		}

		#endregion

		#region PreCombineCheckers

		public void TestCombineSelectedShipments_CheckForUnrelated()
		{
			CommonShipment randomShipment1 = Factory.New<CommonShipment>();
			randomShipment1.JS_UniqueConsignRef = "DODGY";

			CommonShipment randomShipment2 = Factory.New<CommonShipment>();
			randomShipment2.JS_UniqueConsignRef = "SNEAKY";

			CombineShipmentsHelper helper = new CombineShipmentsHelper(SB1, Consol);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { SB2.PK, SB3.PK }, helper.RelatedShipments.Select(shipment => shipment.PK));

			CommonShipment[] selectedShipments = new CommonShipment[] { SB2, SB3, randomShipment1, randomShipment2 };
			string message = helper.CombineSelectedShipments(selectedShipments);

			AssertEquals("Check message", "Following shipment(s) are un-related to the original shipment: DODGY, SNEAKY", message);
			AssertEquals("Combine didn't run", false, SB1.HasChanges);
			AssertEquals("Combine didn't run", false, selectedShipments.Any(shipment => shipment.JS_IsCancelled));
		}

		public void TestCombineSelectedShipments_CheckPivotCouldBeDeleted()
		{
			var deleteChecker = new Mock<DeleteChecker>(MockBehavior.Strict);

			Hashtable mockBizoStrategies = new Hashtable();
			mockBizoStrategies.Add("JobConShipLink", new TestObjectHandle(new ArrayList() { deleteChecker.Object }));
			using (ObjectFactory.Substitute("BusinessObjectStrategies", mockBizoStrategies))
			{
				var factory = new BusinessObjectFactory();
				var consol = factory.Load<CommonConsol>(Consol.PK);
				var shipment1 = factory.Load<CommonShipment>(SB1.PK);
				var shipment2 = factory.Load<CommonShipment>(SB2.PK);
				shipment2.JS_UniqueConsignRef = "HELLO";

				factory.Save();

				deleteChecker.Setup(m => m.DeleteDetails(It.IsAny<BusinessObject>())).Returns(new DeleteDetails.Disallow("Problem, officer?.."));

				var helper = new CombineShipmentsHelper(shipment1, consol);
				var selectedShipments = new CommonShipment[] { shipment2 };
				var message = helper.CombineSelectedShipments(selectedShipments);

				AssertEquals("Check message", "Shipment HELLO couldn't be detached from the consol: Problem, officer?..\r\n", message);
				AssertEquals("Combine didn't run", false, SB1.HasChanges);
				AssertEquals("Combine didn't run", false, selectedShipments.Any(shipment => shipment.JS_IsCancelled));

				deleteChecker.VerifyAll();
			}
		}

		public void TestCombineSelectedShipments_JobHeaderHasHotCheck_SameCompany()
		{
			var jobLoaderSB2 = new JobHeader.Loader(SB2);
			var jobSB2 = jobLoaderSB2.TryCreateWithoutMutexForTestOnly();

			var hotChequeSB2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			hotChequeSB2[AccHotChequeSchema.Constants.AQ_JH] = jobSB2.PK;

			Factory.Save();

			var helper = new CombineShipmentsHelper(SB1, Consol);
			var message = helper.CombineSelectedShipments(SB2, SB3);
			AssertEquals(@"S00001011 cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header (S00001011) in the company EDI.", message);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCombineSelectedShipments_JobHeaderHasHotCheck_DifferentCompany()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = newCompany.Branches.AddNew();
			newBranch.FillWithValidTestData();

			var jobLoaderSB2 = new JobHeader.Loader(SB2);
			var jobSB2 = jobLoaderSB2.TryCreateWithoutMutexForTestOnly(newBranch);

			var hotCheckSB2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			hotCheckSB2[AccHotChequeSchema.Constants.AQ_JH] = jobSB2.PK;

			Factory.Save();

			var helper = new CombineShipmentsHelper(SB1, Consol);
			var message = helper.CombineSelectedShipments(SB2, SB3);
			AssertEquals(@"S00001011 cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header (S00001011) in the company DAN.", message);
		}

		public void TestCombineSelectedShipments_MultipleJobHeadersHaveHotCheck_SameCompany()
		{
			var jobLoaderSB2 = new JobHeader.Loader(SB2);
			var jobSB2 = jobLoaderSB2.TryCreateWithoutMutexForTestOnly();

			var hotCheckSB2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			hotCheckSB2[AccHotChequeSchema.Constants.AQ_JH] = jobSB2.PK;

			var jobLoaderSB3 = new JobHeader.Loader(SB3);
			var jobSB3 = jobLoaderSB3.TryCreateWithoutMutexForTestOnly();

			var hotCheckSB3 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			hotCheckSB3[AccHotChequeSchema.Constants.AQ_JH] = jobSB3.PK;

			Factory.Save();

			var helper = new CombineShipmentsHelper(SB1, Consol);
			var message = helper.CombineSelectedShipments(SB2, SB3);
			AssertEquals(@"S00001011 cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header (S00001011) in the company EDI.
S00001012 cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header (S00001012) in the company EDI.", message);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCombineSelectedShipments_MultipleJobHeadersHaveHotCheck_DifferentCompany()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = newCompany.Branches.AddNew();
			newBranch.FillWithValidTestData();

			var jobLoaderSB2 = new JobHeader.Loader(SB2);
			var jobSB2 = jobLoaderSB2.TryCreateWithoutMutexForTestOnly(newBranch);

			var hotCheckSB2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			hotCheckSB2[AccHotChequeSchema.Constants.AQ_JH] = jobSB2.PK;

			var newCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch2 = newCompany.Branches.AddNew();
			newBranch2.FillWithValidTestData();

			var jobLoaderSB3 = new JobHeader.Loader(SB3);
			var jobSB3 = jobLoaderSB3.TryCreateWithoutMutexForTestOnly(newBranch2);

			var hotCheckSB3 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			hotCheckSB3[AccHotChequeSchema.Constants.AQ_JH] = jobSB3.PK;

			Factory.Save();

			var helper = new CombineShipmentsHelper(SB1, Consol);
			var message = helper.CombineSelectedShipments(SB2, SB3);
			AssertEquals(@"S00001011 cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header (S00001011) in the company DAN.
S00001012 cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header (S00001012) in the company DAN.", message);
		}

		public void TestCombineSelectedShipments_EmptyJobHeaderRemoved()
		{
			var jobLoader2 = new JobHeader.Loader(SB2);
			var job2 = jobLoader2.TryCreateWithoutMutexForTestOnly();

			Factory.Save();

			var helper = new CombineShipmentsHelper(SB1, Consol);
			var message = helper.CombineSelectedShipments(SB2, SB3);
			AssertEquals("", message);
			AssertEquals(true, job2.IsCancelled);
		}

		#endregion

		#region TestCombineSelectedShipments_SailingPerspective()

		public void TestCombineSelectedShipments_SailingPerspective()
		{
			CommonShipment[] selectedShipments = new CommonShipment[2];
			selectedShipments[0] = SA2;
			selectedShipments[1] = SA3;

			CombineShipmentsHelper helper = new CombineShipmentsHelper(SA1, SA1.Sailing);

			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentCanBeCombined());
			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentHasSiblings);

			AssertEquals("Expecting 3 Related Shipments.", 3, helper.RelatedShipments.Count);

			helper.CombineSelectedShipments(selectedShipments);

			Assert("Expecting SA2 to be cancelled.", SA2.JS_IsCancelled);
			Assert("Expecting SA3 to be cancelled.", SA3.JS_IsCancelled);
			Assert("Not expecting SA8 to be cancelled - it was a combine candidate, but was not selected for the combine.", !SA8.JS_IsCancelled);

			AssertEquals("Expecting SA1 pack type to be packages.", FreightPacksDataRegistry.Instance.OuterPackUnit.Value, SA1.JS_F3_NKPackType);
			AssertEquals("Expecting SA1 packages to be 15.", 15, SA1.JS_OuterPacks);
			AssertEquals("Expecting SA1 to have 3 outerpacklines.", 3, SA1.OuterPackLines.Count);

			AssertEquals("Related shipment should be re-loaded", 1, helper.RelatedShipments.Count);
		}

		#endregion

		#region TestCombineSelectedShipments_ActualWeightAndActualVolume()

		public void TestCombineSelectedShipments_ActualWeightAndActualVolume()
		{
			var totalWeight = SA1.JS_ActualWeight + SA2.JS_ActualWeight + SA3.JS_ActualWeight;
			var totalVolume = SA1.JS_ActualVolume + SA2.JS_ActualVolume + SA3.JS_ActualVolume;

			var sa2PackLine2 = SA2.OuterPackLines.AddNew();
			sa2PackLine2.JL_PackageCount = 1;
			sa2PackLine2.JL_ActualWeight = 100;
			sa2PackLine2.JL_ActualVolume = 1;

			var sa3PackLine2 = SA3.OuterPackLines.AddNew();
			sa3PackLine2.JL_PackageCount = 1;
			sa3PackLine2.JL_ActualWeight = 100;
			sa3PackLine2.JL_ActualVolume = 1;
			var sa3PackLine3 = SA3.OuterPackLines.AddNew();
			sa3PackLine3.JL_PackageCount = 1;
			sa3PackLine3.JL_ActualWeight = 100;
			sa3PackLine3.JL_ActualVolume = 1;

			Factory.Save();

			var selectedShipments = new CommonShipment[2];
			selectedShipments[0] = SA2;
			selectedShipments[1] = SA3;

			var helper = new CombineShipmentsHelper(SA1, SA1.Sailing);
			helper.CombineSelectedShipments(selectedShipments);

			AssertEquals("Expecting weight to be added.", totalWeight + 300, SA1.TotalOuterPacksWeight);
			AssertEquals("Expecting volume to be added.", totalVolume + 3, SA1.TotalOuterPacksVolume);
		}

		#endregion

		#region TestCombineSelectedShipments_ConsolPerspective()

		public void TestCombineSelectedShipments_ConsolPerspective()
		{
			CommonShipment[] selectedShipments = new CommonShipment[2];
			selectedShipments[0] = SB2;
			selectedShipments[1] = SB3;

			CombineShipmentsHelper helper = new CombineShipmentsHelper(SB1, Consol);

			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentCanBeCombined());
			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentHasSiblings);

			AssertEquals("Expecting 2 Related Shipments.", 2, helper.RelatedShipments.Count);

			helper.CombineSelectedShipments(selectedShipments);

			Assert("Expecting SB2 to be cancelled.", SB2.JS_IsCancelled);
			Assert("Expecting SB3 to be cancelled.", SB3.JS_IsCancelled);

			AssertEquals("Expecting SB1 pack type to be packages.", FreightPacksDataRegistry.Instance.OuterPackUnit.Value, SB1.JS_F3_NKPackType);
			AssertEquals("Expecting SB1 packages to be 15.", 15, SB1.JS_OuterPacks);
			AssertEquals("Expecting SB1 to have 3 outerpacklines.", 3, SB1.OuterPackLines.Count);

			AssertEquals("Expecting SB2 to not be on any consols.", 0, SB2.Consols.Count);

			AssertEquals("Related shipment should be re-loaded", 0, helper.RelatedShipments.Count);
		}

		#endregion

		#region TestCombineSelectedShipmentsPackLineContainerNum_Sailing

		public void TestCombineSelectedShipmentsPackLineContainerNum_Sailing()
		{
			CommonContainer container1 = Sailing.Containers.AddNew();
			CommonContainer container2 = Sailing.Containers.AddNew();
			CommonContainer container3 = Sailing.Containers.AddNew();
			container1.JC_ContainerNum = "A";
			container2.JC_ContainerNum = "B";
			container3.JC_ContainerNum = "C";

			PackLine sA1PackLine1 = SA1.OuterPackLines.AddNew();
			PackLine sA1PackLine2 = SA1.OuterPackLines.AddNew();
			sA1PackLine1.JL_PackageCount = 1;
			sA1PackLine2.JL_PackageCount = 3;
			sA1PackLine1.Containers.Add(container1);
			sA1PackLine2.Containers.Add(container3);

			PackLine sA2PackLine1 = SA2.OuterPackLines.AddNew();
			PackLine sA2PackLine2 = SA2.OuterPackLines.AddNew();
			PackLine sA2PackLine3 = SA2.OuterPackLines.AddNew();
			sA2PackLine1.JL_PackageCount = 2;
			sA2PackLine2.JL_PackageCount = 3;
			sA2PackLine3.JL_PackageCount = 1;
			sA2PackLine1.Containers.Add(container1);
			sA2PackLine2.Containers.Add(container2);
			sA2PackLine3.Containers.Add(container3);

			PackLine sA3PackLine1 = SA3.OuterPackLines.AddNew();
			PackLine sA3PackLine2 = SA3.OuterPackLines.AddNew();
			sA3PackLine1.JL_PackageCount = 3;
			sA3PackLine2.JL_PackageCount = 2;
			sA3PackLine1.Containers.Add(container1);
			sA3PackLine2.Containers.Add(container2);

			CommonShipment[] selectedShipments = new CommonShipment[2];
			selectedShipments[0] = SA2;
			selectedShipments[1] = SA3;

			CombineShipmentsHelper helper = new CombineShipmentsHelper(SA1, SA1.Sailing);

			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentCanBeCombined());
			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentHasSiblings);

			AssertEquals("Expecting 3 Related Shipments.", 3, helper.RelatedShipments.Count);

			helper.CombineSelectedShipments(selectedShipments);

			Assert("Expecting SA2 to be cancelled.", SA2.JS_IsCancelled);
			Assert("Expecting SA3 to be cancelled.", SA3.JS_IsCancelled);
			Assert("Not expecting SA8 to be cancelled - it was a combine candidate, but was not selected for the combine.", !SA8.JS_IsCancelled);

			AssertEquals("Expecting SA1 pack type to be packages.", FreightPacksDataRegistry.Instance.OuterPackUnit.Value, SA1.JS_F3_NKPackType);
			AssertEquals("Expecting SA1 packages to be 15.", 15, SA1.JS_OuterPacks);
			AssertEquals("Expecting SA1 to have 10 outerpacklines.", 10, SA1.OuterPackLines.Count);

			AssertEquals("Expecting SA1 has 3 packages with A container", 3, container1.PackLines.Count);
			AssertEquals("Expecting SA1 has 2 packages with B container", 2, container2.PackLines.Count);
			AssertEquals("Expecting SA1 has 2 packages with C container", 2, container3.PackLines.Count);
		}

		#endregion

		#region TestCombinedShipmentsPackLineContainersNum_Consol

		public void TestCombinedShipmentsPackLineContainersNum_Consol()
		{
			CommonContainer container1 = Consol.Containers.AddNew();
			CommonContainer container2 = Consol.Containers.AddNew();
			CommonContainer container3 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "A";
			container2.JC_ContainerNum = "B";
			container3.JC_ContainerNum = "C";

			AssertEquals("Expected container 1 have 3 packs", 3, container1.PackLines.Count);
			AssertEquals("Expected empty container 2", 0, container2.PackLines.Count);
			AssertEquals("Expected empty container 3", 0, container3.PackLines.Count);

			PackLine sB1PackLine1 = SB1.OuterPackLines.AddNew();
			PackLine sB1PackLine2 = SB1.OuterPackLines.AddNew();
			sB1PackLine1.JL_PackageCount = 1;
			sB1PackLine2.JL_PackageCount = 3;
			sB1PackLine1.JL_JC = container1.PK;
			sB1PackLine2.JL_JC = container3.PK;

			PackLine sB2PackLine1 = SB2.OuterPackLines.AddNew();
			PackLine sB2PackLine2 = SB2.OuterPackLines.AddNew();
			PackLine sB2PackLine3 = SB2.OuterPackLines.AddNew();
			sB2PackLine1.JL_PackageCount = 2;
			sB2PackLine2.JL_PackageCount = 3;
			sB2PackLine3.JL_PackageCount = 1;
			sB2PackLine1.JL_JC = container1.PK;
			sB2PackLine2.JL_JC = container2.PK;
			sB2PackLine3.JL_JC = container3.PK;

			PackLine sB3PackLine1 = SB3.OuterPackLines.AddNew();
			PackLine sB3PackLine2 = SB3.OuterPackLines.AddNew();
			sB3PackLine1.JL_PackageCount = 3;
			sB3PackLine2.JL_PackageCount = 2;
			sB3PackLine1.JL_JC = container1.PK;
			sB3PackLine2.JL_JC = container2.PK;

			CommonShipment[] selectedShipments = new CommonShipment[2];
			selectedShipments[0] = SB2;
			selectedShipments[1] = SB3;

			CombineShipmentsHelper helper = new CombineShipmentsHelper(SB1, Consol);

			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentCanBeCombined());
			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentHasSiblings);

			AssertEquals("Expecting 2 Related Shipments.", 2, helper.RelatedShipments.Count);

			Factory.Save();
			helper.CombineSelectedShipments(selectedShipments);

			Assert("Expecting SB2 to be cancelled.", SB2.JS_IsCancelled);
			Assert("Expecting SB3 to be cancelled.", SB3.JS_IsCancelled);

			AssertEquals("Expecting SB1 pack type to be packages.", FreightPacksDataRegistry.Instance.OuterPackUnit.Value, SB1.JS_F3_NKPackType);
			AssertEquals("Expecting SB1 packages to be 15.", 15, SB1.JS_OuterPacks);
			AssertEquals("Expecting SB1 to have 10 outerpacklines.", 10, SB1.OuterPackLines.Count);

			AssertEquals("Expecting SB1 has 1 package with A container", 6, container1.PackLines.Count);
			AssertEquals("Expecting SB1 has no packages with B container", 2, container2.PackLines.Count);
			AssertEquals("Expecting SB1 has 1 package with C container", 2, container3.PackLines.Count);

			AssertEquals("Expecting SB2 to not be on any consols.", 0, SB2.Consols.Count);
			AssertEquals("Expecting SB3 to not be on any consols.", 0, SB3.Consols.Count);
		}

		#endregion

		#region TestCombineSelectedShipments_AddWarningWhenThereAreUnmatchOrgs

		public void TestCombineSelectedShipments_AddWarningWhenThereAreUnmatchOrgs()
		{
			SB2.ConsigneePK = OrgHeader.UnmatchedOrganisationPK;
			SB3.ConsignorPK = OrgHeader.UnmatchedOrganisationPK;

			var selectedShipments = new CommonShipment[]
			{
				SB2,
				SB3
			};

			var helper = new CombineShipmentsHelper(SB1, Consol);

			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentCanBeCombined());
			Assert("Precondition: Expecting to be able to combine shipments.", helper.ShipmentHasSiblings);

			AssertEquals("Expecting 2 Related Shipments.", 2, helper.RelatedShipments.Count);
			AssertHasRowWarning(SB2, "There are still UNMATCHED organization details on this Shipment. Any Organization details found in the Unmatched Organization Details note will be lost if you continue to merge this Shipment.");
			AssertHasRowWarning(SB3, "There are still UNMATCHED organization details on this Shipment. Any Organization details found in the Unmatched Organization Details note will be lost if you continue to merge this Shipment.");
		}

		#endregion

		#region Notes

		public void TestMergeNotesFromShipment()
		{
			AssertEquals("SA1 should have no notes", 0, SA1.Notes.GetAllNotes().Count);
			AssertEquals("SA2 should have no notes", 0, SA2.Notes.GetAllNotes().Count);
			CombineShipmentsHelper helper = new CombineShipmentsHelper(SA1, SA1.Sailing);

			// test merge when no notes
			helper.CombineSelectedShipments(SA2);
			AssertEquals("SA1 should have no notes", 0, SA1.Notes.GetAllNotes().Count);
			AssertEquals("SA2 should have a transfer note", 1, SA2.Notes.GetAllNotes().Count);

			SA1.Notes.RemoveAndDeleteAll();
			SA2.Notes.RemoveAndDeleteAll();

			StmNote note1 = SA2.Notes.AddNew();
			note1.ST_Description = "Fred Flintstone";
			note1.ST_NoteDataAsText = "Bedrock";
			AssertEquals("SA2 should have a note", 1, SA2.Notes.GetAllNotes().Count);

			// test adding a note
			SA2.JS_IsCancelled = false;
			helper.CombineSelectedShipments(SA2);
			AssertEquals("SA1 should have one note", 1, SA1.Notes.GetAllNotes().Count);
			AssertEquals("SA1 note should match SA2 note", ((StmNoteCollection)SA1.Notes.GetAllNotes())[0].ST_Description, ((StmNoteCollection)SA2.Notes.GetAllNotes())[0].ST_Description);
			AssertEquals("SA1 note should match SA2 note", ((StmNoteCollection)SA1.Notes.GetAllNotes())[0].ST_NoteDataAsText, ((StmNoteCollection)SA2.Notes.GetAllNotes())[0].ST_NoteDataAsText);
			AssertEquals("SA1 note should match SA2 note", ((StmNoteCollection)SA1.Notes.GetAllNotes())[0].ST_NoteData, ((StmNoteCollection)SA2.Notes.GetAllNotes())[0].ST_NoteData);

			SA1.Notes.RemoveAndDeleteAll();
			SA2.Notes.RemoveAndDeleteAll();

			// test appending a note
			StmNote gargamelNote1 = SA1.Notes.AddNew();
			gargamelNote1.ST_Description = "Test Note";
			gargamelNote1.ST_NoteDataAsText = "Note 1";
			StmNote gargamelNote2 = SA2.Notes.AddNew();
			gargamelNote2.ST_Description = "Test Note";
			gargamelNote2.ST_NoteDataAsText = "Note 2";

			SA2.JS_IsCancelled = false;
			helper.CombineSelectedShipments(SA2);
			AssertEquals("", "Note 1" + System.Environment.NewLine + System.Environment.NewLine + "Note 2", ((StmNoteCollection)SA1.Notes.GetAllNotes())[0].ST_NoteDataAsText);
		}

		public void TestMergeNotesFromShipment_DoNotMergeSerializableNotes()
		{
			var helper = new CombineShipmentsHelper(SA1, SA1.Sailing);

			// test merge when no notes
			var note = SA2.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			note.ST_NoteText = "<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Forwarder</OrganisationType><OrganisationSubType>Forwarder</OrganisationSubType><OwnerCode /><EDICode /><OrganisationName>TEST FORWARDER</OrganisationName><AddressLine1 /><AddressLine2 /><City /><PostCode /><StateOrProvince /><Country /></UnmatchOrgRecord></UnmatchOrgRecords>";

			helper.CombineSelectedShipments(SA2);

			ZString expectedNoteText = "Organisation Type: Forwarder\r\nOwner Code: \r\nEDI Code: \r\nOrganisation Name: TEST FORWARDER\r\nAddress Line 1: \r\nAddress Line 2: \r\nCity: \r\nPost Code: \r\nState or Province: \r\nCountry: \r\n";
			AssertEquals("SA1 should have one note", 1, SA1.Notes.GetAllNotes().Count);
			AssertEquals(expectedNoteText, ((StmNoteCollection)SA1.Notes.GetAllNotes())[0].ST_NoteText);

			SA1.Notes.RemoveAndDeleteAll();
			SA2.Notes.RemoveAndDeleteAll();

			// not appending for a serializable note
			var note2 = SA1.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			note2.ST_NoteText = "<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Forwarder</OrganisationType><OrganisationSubType>Forwarder</OrganisationSubType><OwnerCode /><EDICode /><OrganisationName>TEST FORWARDER</OrganisationName><AddressLine1 /><AddressLine2 /><City /><PostCode /><StateOrProvince /><Country /></UnmatchOrgRecord></UnmatchOrgRecords>";

			var note3 = SA2.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description;
			note3.ST_NoteText = "<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Consignee</OrganisationType><OrganisationSubType>Consignee</OrganisationSubType><OwnerCode /><EDICode /><OrganisationName>TEST CONSIGNEE</OrganisationName><AddressLine1 /><AddressLine2 /><City /><PostCode /><StateOrProvince /><Country /></UnmatchOrgRecord></UnmatchOrgRecords>";

			SA2.JS_IsCancelled = false;
			helper.CombineSelectedShipments(SA2);
			AssertEquals("SA1 should have one note", 1, SA1.Notes.GetAllNotes().Count);

			expectedNoteText = "Organisation Type: Forwarder\r\nOwner Code: \r\nEDI Code: \r\nOrganisation Name: TEST FORWARDER\r\nAddress Line 1: \r\nAddress Line 2: \r\nCity: \r\nPost Code: \r\nState or Province: \r\nCountry: \r\n ";
			AssertEquals(expectedNoteText, ((StmNoteCollection)SA1.Notes.GetAllNotes())[0].ST_NoteText);
		}

		#endregion

		#region Orders

		public void TestOrdersMergeFromShipment()
		{
			int orderCount = Factory.Load<Enterprise.Integration.Forwarding.IOrder>(new ZQuery(JobOrderHeaderSchema.JD_JS, SA1.PK)).Length;
			AssertEquals("SA1 should have no orders", 0, orderCount);

			BusinessObject newOrder = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			newOrder[JobOrderHeaderSchema.JD_JS] = SA2.PK;

			CombineShipmentsHelper helper = new CombineShipmentsHelper(SA1, SA1.Sailing);
			helper.CombineSelectedShipments(SA2);

			orderCount = Factory.Load<Enterprise.Integration.Forwarding.IOrder>(new ZQuery(JobOrderHeaderSchema.JD_JS, SA1.PK)).Length;
			int oldShipmentOrderCount = Factory.Load<Enterprise.Integration.Forwarding.IOrder>(new ZQuery(JobOrderHeaderSchema.JD_JS, SA2.PK)).Length;
			AssertEquals("SA1 should have 1 order", 1, orderCount);
			AssertEquals("SA2 should have no orders", 0, oldShipmentOrderCount);
		}

		#endregion

		#region WorkflowItems

		public void TestWorkflowItemsCancelledOnDeactivatedShipments()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport = consol.Transports[0];
			transport.JW_JX = Sailing.PK;

			var shipment1 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment1.JS_JX = Sailing.PK;
			shipment1.ConsignorPK = Org1.PK;
			shipment1.ConsigneePK = Org2.PK;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OuterPacks = 4;
			shipment1.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			shipment1.JS_ActualVolume = 4;
			shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment1.JS_ActualWeight = 1200;
			shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;

			var shipment2 = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment2.JS_JX = Sailing.PK;
			shipment2.ConsignorPK = Org1.PK;
			shipment2.ConsigneePK = Org2.PK;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OuterPacks = 1;
			shipment2.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			shipment2.JS_ActualVolume = 1;
			shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment2.JS_ActualWeight = 100;
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			var workflowProvider = (IWorkflowProvider)shipment1;

			var assignedTask = workflowProvider.WorkflowItems.Tasks.AddNew();
			assignedTask.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			assignedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var closedTask = workflowProvider.WorkflowItems.Tasks.AddNew();
			closedTask.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			closedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var incompleteMilestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			incompleteMilestone.P9_Type = Core.Constants.Workflow.MilestoneType;
			incompleteMilestone.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var completeMilestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			completeMilestone.P9_Type = Core.Constants.Workflow.MilestoneType;
			completeMilestone.SetMilestoneActualDateForTest(ZDateTime.Today);

			Factory.Save();

			var selectedShipments = new CommonShipment[1];
			selectedShipments[0] = shipment1;

			var helper = new CombineShipmentsHelper(shipment2, consol);

			Assert(helper.ShipmentCanBeCombined());
			Assert(helper.ShipmentHasSiblings);

			AssertEquals(1, helper.RelatedShipments.Count);

			helper.CombineSelectedShipments(selectedShipments);

			Factory.Save();

			Assert(shipment1.JS_IsCancelled);

			AssertEquals(FreightPacksDataRegistry.Instance.OuterPackUnit.Value, shipment2.JS_F3_NKPackType);
			AssertEquals(5, shipment2.JS_OuterPacks);
			AssertEquals(2, shipment2.OuterPackLines.Count);

			AssertEquals(0, shipment1.Consols.Count);

			AssertEquals(0, helper.RelatedShipments.Count);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, assignedTask.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, closedTask.P9_Status);

			Assert(incompleteMilestone.IsDeleted);
			AssertEquals(ProcessTask.LastCompletedStatusCode, completeMilestone.P9_Status);
		}

		#endregion

		#region Implementation

		CommonShipment SA1;
		CommonShipment SA2;
		CommonShipment SA3;
		CommonShipment SA4;
		CommonShipment SA5;
		CommonShipment SA6;
		CommonShipment SA7;
		CommonShipment SA8;
		CommonShipment SA9;
		CommonShipment SA10;
		CommonShipment SA11;
		JobSailing Sailing;
		CommonShipment SB1;
		CommonShipment SB2;
		CommonShipment SB3;
		CommonShipment SB4;
		CommonShipment SB5;
		CommonConsol Consol;
		OrgHeader Org1;
		OrgHeader Org2;
		OrgHeader Org3;
		OrgHeader Org4;

		protected override void SetUp()
		{
			base.SetUp();

			Org1 = Factory.New<OrgHeader>();
			Org1.OH_FullName = "Test Org 1";
			Org1.MainAddress.OA_Address1 = "Test Address 1";
			Org1.OH_IsConsignor = ZBool.True;
			Org1.OH_RL_NKClosestPort = "AUSYD";

			Org2 = Factory.New<OrgHeader>();
			Org2.OH_FullName = "Test Org 2";
			Org2.MainAddress.OA_Address1 = "Test Address 2";
			Org2.OH_IsConsignee = ZBool.True;
			Org2.OH_RL_NKClosestPort = "AUSYD";

			Org3 = Factory.New<OrgHeader>();
			Org3.OH_FullName = "Test Org 3";
			Org3.MainAddress.OA_Address1 = "Test Address 3";
			Org3.OH_IsConsignor = ZBool.True;
			Org3.OH_RL_NKClosestPort = "USLAX";

			Org4 = Factory.New<OrgHeader>();
			Org4.OH_FullName = "Test Org 4";
			Org4.MainAddress.OA_Address1 = "Test Address 4";
			Org4.OH_IsConsignee = ZBool.True;
			Org4.OH_RL_NKClosestPort = "USLAX";

			SailingsForTestClasses helper = new SailingsForTestClasses(Factory);
			Sailing = helper.SydLaxSailing;

			Consol = Factory.New<CommonConsol>();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_JX = Sailing.PK;

			Factory.Save();

			SA1 = Factory.New<CommonShipment>();
			SA1.JS_JX = Sailing.PK;
			SA1.ConsignorPK = Org1.PK;
			SA1.ConsigneePK = Org2.PK;
			SA1.JS_RL_NKOrigin = "AUSYD";
			SA1.JS_RL_NKDestination = "USLAX";
			SA1.JS_OuterPacks = 4;
			SA1.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			SA1.JS_ActualVolume = 4;
			SA1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			SA1.JS_ActualWeight = 1200;
			SA1.JS_UnitOfWeight = Constants.Weight.Kilograms;

			SA2 = Factory.New<CommonShipment>();
			SA2.JS_JX = Sailing.PK;
			SA2.ConsignorPK = Org1.PK;
			SA2.ConsigneePK = Org2.PK;
			SA2.JS_RL_NKOrigin = "AUSYD";
			SA2.JS_RL_NKDestination = "USLAX";
			SA2.JS_OuterPacks = 6;
			SA2.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			SA2.JS_ActualVolume = 3;
			SA2.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			SA2.JS_ActualWeight = 900;
			SA2.JS_UnitOfWeight = Constants.Weight.Kilograms;

			SA3 = Factory.New<CommonShipment>();
			SA3.JS_JX = Sailing.PK;
			SA3.ConsignorPK = Org1.PK;
			SA3.ConsigneePK = Org2.PK;
			SA3.JS_RL_NKOrigin = "AUSYD";
			SA3.JS_RL_NKDestination = "USLAX";
			SA3.JS_OuterPacks = 5;
			SA3.JS_F3_NKPackType = Constants.PkgUnit.Drum;
			SA3.JS_ActualVolume = new ZDecimal(2.5);
			SA3.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			SA3.JS_ActualWeight = 500;
			SA3.JS_UnitOfWeight = Constants.Weight.Kilograms;

			SA4 = Factory.New<CommonShipment>();
			SA4.JS_JX = Sailing.PK;
			SA4.ConsignorPK = Org1.PK;
			SA4.ConsigneePK = Org2.PK;
			SA4.JS_RL_NKOrigin = "AUSYD";
			SA4.JS_RL_NKDestination = "USSFO";

			SA5 = Factory.New<CommonShipment>();
			SA5.JS_JX = Sailing.PK;
			SA5.ConsignorPK = Org1.PK;
			SA5.ConsigneePK = ZGuid.Empty;
			SA5.JS_RL_NKOrigin = "AUSYD";
			SA5.JS_RL_NKDestination = "USLAX";

			SA6 = Factory.New<CommonShipment>();
			SA6.JS_JX = Sailing.PK;
			SA6.ConsignorPK = Org3.PK;
			SA6.ConsigneePK = Org4.PK;
			SA6.JS_RL_NKOrigin = "AUSYD";
			SA6.JS_RL_NKDestination = "USLAX";

			SA7 = Factory.New<CommonShipment>();
			SA7.ConsignorPK = Org1.PK;
			SA7.ConsigneePK = Org2.PK;
			SA7.JS_RL_NKOrigin = "AUSYD";
			SA7.JS_RL_NKDestination = "USLAX";

			SA8 = Factory.New<CommonShipment>();
			SA8.JS_JX = Sailing.PK;
			SA8.ConsignorPK = Org1.PK;
			SA8.ConsigneePK = Org2.PK;
			SA8.JS_RL_NKOrigin = "AUSYD";
			SA8.JS_RL_NKDestination = "USLAX";

			SA9 = Factory.New<CommonShipment>();
			SA9.JS_JX = Sailing.PK;
			SA9.ConsignorPK = Org1.PK;
			SA9.ConsigneePK = Org2.PK;
			SA9.JS_RL_NKOrigin = "AUSYD";
			SA9.JS_RL_NKDestination = "USLAX";

			SA10 = Factory.New<CommonShipment>();
			SA10.JS_JX = Sailing.PK;
			SA10.ConsignorPK = Org1.PK;
			SA10.ConsigneePK = Org2.PK;
			SA10.JS_RL_NKOrigin = "AUSYD";
			SA10.JS_RL_NKDestination = "USLAX";

			SA11 = Factory.New<CommonShipment>();
			SA11.JS_JX = Sailing.PK;
			SA11.ConsignorPK = Org1.PK;
			SA11.ConsigneePK = Org2.PK;
			SA11.JS_RL_NKOrigin = "AUSYD";
			SA11.JS_RL_NKDestination = "USLAX";
			SA11.JS_JS_ColoadMasterShipment = SA10.PK;

			SB1 = Factory.New<CommonShipment>();
			SB1.ConsignorPK = Org1.PK;
			SB1.ConsigneePK = Org2.PK;
			SB1.JS_RL_NKOrigin = "AUSYD";
			SB1.JS_RL_NKDestination = "USLAX";
			SB1.JS_OuterPacks = 4;
			SB1.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			SB1.JS_ActualVolume = 4;
			SB1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			SB1.JS_ActualWeight = 1200;
			SB1.JS_UnitOfWeight = Constants.Weight.Kilograms;

			Consol.Shipments.Add(SB1);

			SB2 = Factory.New<CommonShipment>();
			SB2.ConsignorPK = Org1.PK;
			SB2.ConsigneePK = Org2.PK;
			SB2.JS_RL_NKOrigin = "AUSYD";
			SB2.JS_RL_NKDestination = "USLAX";
			SB2.JS_OuterPacks = 6;
			SB2.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			SB2.JS_ActualVolume = 3;
			SB2.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			SB2.JS_ActualWeight = 900;
			SB2.JS_UnitOfWeight = Constants.Weight.Kilograms;

			Consol.Shipments.Add(SB2);

			SB3 = Factory.New<CommonShipment>();
			SB3.ConsignorPK = Org1.PK;
			SB3.ConsigneePK = Org2.PK;
			SB3.JS_RL_NKOrigin = "AUSYD";
			SB3.JS_RL_NKDestination = "USLAX";
			SB3.JS_OuterPacks = 5;
			SB3.JS_F3_NKPackType = Constants.PkgUnit.Drum;
			SB3.JS_ActualVolume = new ZDecimal(2.5);
			SB3.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			SB3.JS_ActualWeight = 500;
			SB3.JS_UnitOfWeight = Constants.Weight.Kilograms;

			Consol.Shipments.Add(SB3);

			SB4 = Factory.New<CommonShipment>();
			SB4.ConsignorPK = Org3.PK;
			SB4.ConsigneePK = Org4.PK;
			SB4.JS_RL_NKOrigin = "AUSYD";
			SB4.JS_RL_NKDestination = "USLAX";

			Consol.Shipments.Add(SB4);

			SB5 = Factory.New<CommonShipment>();
			SB5.ConsignorPK = Org3.PK;
			SB5.ConsigneePK = Org4.PK;
			SB5.JS_RL_NKOrigin = "AUSYD";
			SB5.JS_RL_NKDestination = "USLAX";

			Factory.Save();

			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_ParentID = SA8.PK;
			jobHeader.JH_JobNum = SA8.JS_UniqueConsignRef;

			JobHeader jobHeader2 = Factory.NewJobForTesting<JobHeader>();
			jobHeader2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader2.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader2.JH_ParentID = SA9.PK;
			jobHeader2.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader2.JH_JobNum = SA9.JS_UniqueConsignRef;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = jobHeader2.PK;

			Assert("Unique combination of JH_JobNum and JH_GC expected", jobHeader.JH_JobNum != jobHeader2.JH_JobNum || jobHeader.JH_GC != jobHeader2.JH_GC);
		}

		#endregion
	}
}
