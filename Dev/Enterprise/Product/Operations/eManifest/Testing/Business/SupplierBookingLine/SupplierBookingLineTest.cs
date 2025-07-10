using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.eManifest.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.eManifest.Business.Testing
{
	[TestedType(typeof(SupplierBookingLine))]
	[SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification hides desired base class")]
	internal class SupplierBookingLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEManifestLineMembers()
		{
			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			line.DL_ConsigneeReference = "CONSREF";
			line.DL_PiecesManifested = 1234;
			line.DL_RN_NKConsigneeCountryCode = Constants.CountryCodes.Singapore;
			line.DL_ConsignorName = "TESTNAME";
			line.DL_GoodsDescription = "SOME TEST GOODS";

			var eManifestLine = line as IEManifestLine;
			CombineAssertions(() =>
			{
				AssertEquals("PK", eManifestLine.PK, line.PK);
				AssertEquals("Reference", eManifestLine.Reference, line.DL_ConsigneeReference);
				AssertEquals("PackCount", eManifestLine.PackCount, line.DL_PiecesManifested);
				AssertEquals("CountryOfDestination", eManifestLine.CountryOfDestination, line.DL_RN_NKConsigneeCountryCode);
				AssertEquals("GoodsOwner", eManifestLine.GoodsOwner, line.DL_ConsignorName);
				AssertEquals("GoodsDescription", eManifestLine.GoodsDescription, line.DL_GoodsDescription);
			});
		}

		public void TestHumanReadableName()
		{
			var line = Factory.New<SupplierBookingLine>();
			line.DL_ConsigneeReference = "SH4633637134723412";
			line.DL_ConsigneeName = "FIGHTERS OF FOO";

			AssertEquals("HumanReadableName", "Supplier Booking Line SH4633637134723412(Consignee='FIGHTERS OF FOO')", line.HumanReadableName);

			line = Factory.New<SupplierBookingLine>();
			AssertEquals("HumanReadableName", "Supplier Booking Line (Consignee='')", line.HumanReadableName);
		}

		public void TestShipmentGetter_ApprovedShipmentIsSpecified_ReturnValidShipment()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000001";

			var line = Factory.New<SupplierBookingLine>();
			line.DL_JS_ApprovedShipment = ((BusinessObject)shipment).PK;

			AssertEquals("S000001", line.Shipment.JS_UniqueConsignRef);
		}

		public void TestShipmentGetter_ApprovedShipmentIsNotSpecified_ReturnNull()
		{
			var line = Factory.New<SupplierBookingLine>();
			AssertNull(line.Shipment);
		}

		public void TestBookingHeader()
		{
			var header = Factory.New<SupplierBookingHeader>();
			header.DH_SupplierReference = "Test";
			var line = Factory.New<SupplierBookingLine>();
			line.DL_DH_BookingHeader = header.PK;
			AssertEquals(header.DH_SupplierReference, line.BookingHeader.DH_SupplierReference);
		}

		public void TestLocalTransportCompanyLabel()
		{
			var company1 = Factory.NewWithValidTestData<OrgHeader>();
			company1.OH_Code = "COMP1";
			company1.OH_IsLocalTransport = true;
			company1.OH_IsShippingProvider = true;

			var company2 = Factory.NewWithValidTestData<OrgHeader>();
			company2.OH_Code = "COMP2";
			company2.OH_IsLocalTransport = true;
			company2.OH_IsShippingProvider = true;

			Factory.Save();

			var localTransportCompanyCollection = new LocalTransportCompanyBrandingCollection();

			var localTransportCompany1 = localTransportCompanyCollection.AddNew();
			localTransportCompany1.LocalTransportCompanyPK = company1.PK;
			localTransportCompany1.LabelName = LabelNames.DeliveryLabel;
			localTransportCompany1.Image = new Bitmap(10, 10);

			var localTransportCompany2 = localTransportCompanyCollection.AddNew();
			localTransportCompany2.LocalTransportCompanyPK = company2.PK;
			localTransportCompany2.LabelName = LabelNames.EParcelLabel;
			localTransportCompany2.Image = new Bitmap(10, 10);

			DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, localTransportCompanyCollection);

			var supplierBookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();
			supplierBookingLine.DL_OH_LastMileCarrier = company1.PK;

			AssertEquals(LabelNames.DeliveryLabel, supplierBookingLine.LocalTransportCompanyLabel);

			supplierBookingLine.DL_OH_LastMileCarrier = company2.PK;
			AssertEquals(LabelNames.EParcelLabel, supplierBookingLine.LocalTransportCompanyLabel);
		}

		public void TestLoadingHVLVLinesAsBOsBlowsUpDuringCreationOfTransportBooking()
		{
			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			using (CreatingSupplierBookingLineBizOsPreventer.SuspendCreatingSupplierBookingLineBizOs(factory2))
			{
				AssertExceptionThrown<ApplicationException>(
					"Can't create a SupplierBookingLine as its constructor threw an exception.", () => factory2.Load<SupplierBookingLine>(line.PK));
			}

			AssertNoExceptionThrown(() => factory2.Load<SupplierBookingLine>(line.PK));
		}

		public void TestLastMileTransportBooking()
		{
			var transportBooking = Factory.New<IDtbBooking>();
			transportBooking.KM_JobID = "TB000011";

			var line = Factory.New<SupplierBookingLine>();
			line.DL_KM_LastMileTransportBooking = transportBooking.PK;

			AssertEquals(transportBooking, line.LastMileTransportBooking);
			AssertEquals("TB000011", line.LastMileTransportBooking.KM_JobID);
		}

		public void TestTransportBookingNumberForBinding()
		{
			var transportBooking = Factory.New<IDtbBooking>();
			transportBooking.KM_JobID = "TB000011";

			var line = Factory.New<SupplierBookingLine>();
			AssertEquals(ZString.Empty, line.TransportBookingNumberForBinding);

			line.DL_KM_LastMileTransportBooking = transportBooking.PK;
			AssertEquals("TB000011", line.TransportBookingNumberForBinding);
		}

		public void TestJobNumber()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000001";
			var header = Factory.New<SupplierBookingHeader>();
			header.DH_SupplierReference = "Test";
			var line = Factory.New<SupplierBookingLine>();
			line.DL_DH_BookingHeader = header.PK;
			line.DL_ConsigneeReference = "HB1";
			line.DL_JS_ApprovedShipment = ((BusinessObject)shipment).PK;
			AssertEquals("S000001 / HB1", line.JobNumber);
		}

		public void TestJobNumberNull()
		{
			var header = Factory.New<SupplierBookingHeader>();
			header.DH_SupplierReference = "Test";
			var line = Factory.New<SupplierBookingLine>();
			line.DL_DH_BookingHeader = header.PK;
			line.DL_ConsigneeReference = "HB1";
			AssertEquals("", line.JobNumber);
		}

		public void TestLoadAUCusHAWBInRelatedEventsOneHAWBOneMAWB()
		{
			Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_HouseBill = "MB1";
			consol.JK_DatePortOfFirstArrival = ZDateTime.Now;
			consol.JK_RL_NKDischargePort = "AUSYD";

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_SupplierReference = "Test";
			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			line.DL_JS_ApprovedShipment = shipment.PK;
			line.DL_DH_BookingHeader = header.PK;
			var hawb = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			var mawb = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			mawb.CM_MasterHouseBill = "MB1";
			mawb.CM_ArrivalDate = ZDateTime.Now;

			line.DL_ConsigneeReference = "HB1";
			hawb.CS_HAWB = "HB1";
			hawb.CS_JS = shipment.PK;
			hawb.CS_CM = mawb.PK;

			Factory.Save();

			AssertCollectionContains(hawb, line.BusinessObjectsWithRelatedEvents);
		}

		public void TestBusinessObjectsWithRelatedEventsGetter_ShouldContainParentShipmentAndConsolsAndRelatedTransports()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			shipment.TransportsIncludingRelated.RemoveAll();

			var transport1 = shipment.TransportsIncludingRelated.AddNew();
			var transport2 = shipment.TransportsIncludingRelated.AddNew();

			var line = Factory.New<SupplierBookingLine>();
			line.DL_JS_ApprovedShipment = shipment.PK;

			AssertContainsExactElementsInAnyOrder(new[] { consol1.PK, consol2.PK, shipment.PK, transport1.PK, transport2.PK }, line.BusinessObjectsWithRelatedEvents.Select(b => b.PK));
		}

		public void TestLoadAUCusHAWBInRelatedEventsOneHAWBTwoMAWBs()
		{
			Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_HouseBill = "MB1";
			consol.JK_DatePortOfFirstArrival = ZDateTime.Now;
			consol.JK_RL_NKDischargePort = "AUSYD";

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_SupplierReference = "Test";
			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			line.DL_JS_ApprovedShipment = shipment.PK;
			line.DL_DH_BookingHeader = header.PK;
			var hawb = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			var mawb = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			mawb.CM_MasterHouseBill = "MB1";
			mawb.CM_ArrivalDate = ZDateTime.Now;

			line.DL_ConsigneeReference = "HB1";
			hawb.CS_HAWB = "HB1";
			hawb.CS_JS = shipment.PK;
			hawb.CS_CM = mawb.PK;

			var mawb2 = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			mawb2.CM_MasterHouseBill = "MB1";
			mawb2.CM_ArrivalDate = ZDateTime.Now.AddMonths(-24);

			Factory.Save();

			AssertCollectionContains(hawb, line.BusinessObjectsWithRelatedEvents);
		}

		public void TestLoadAUCusHAWBInRelatedEventsTwoHAWBsTwoMAWBs()
		{
			Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_HouseBill = "MB1";
			shipment2.Consols.Add(consol);
			shipment2.JS_RL_NKDestination = "AUBNE";
			shipment2.JS_HouseBill = "MB1";
			consol.JK_DatePortOfFirstArrival = ZDateTime.Now;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_SupplierReference = "Test";
			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			line.DL_JS_ApprovedShipment = shipment.PK;
			line.DL_DH_BookingHeader = header.PK;
			var hawb = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			var mawb = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			mawb.CM_MasterHouseBill = "MB1";
			mawb.CM_ArrivalDate = ZDateTime.Now;

			line.DL_ConsigneeReference = "HB1";
			hawb.CS_HAWB = "HB1";
			hawb.CS_JS = shipment.PK;
			hawb.CS_CM = mawb.PK;

			var mawb2 = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			mawb2.CM_MasterHouseBill = "MB1";
			mawb2.CM_ArrivalDate = ZDateTime.Now.AddMonths(-24);

			var hawb2 = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			hawb2.CS_HAWB = "HB1";
			hawb2.CS_JS = shipment2.PK;

			hawb2.CS_CM = mawb2.PK;
			Factory.Save();

			AssertCollectionContains(hawb, line.BusinessObjectsWithRelatedEvents);
			AssertCollectionNotContains(hawb2, line.BusinessObjectsWithRelatedEvents);
		}

		public void TestLoadAUCusHAWBInRelatedEventsTwoMAWBsCreatedAfterConsol()
		{
			Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_HouseBill = "MB1";
			shipment2.Consols.Add(consol);
			shipment2.JS_RL_NKDestination = "AUBNE";
			shipment2.JS_HouseBill = "MB1";
			consol.JK_DatePortOfFirstArrival = ZDateTime.Now;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_SupplierReference = "Test";
			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			line.DL_JS_ApprovedShipment = shipment.PK;
			line.DL_DH_BookingHeader = header.PK;
			var hawb = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			var mawb = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			mawb.CM_MasterHouseBill = "MB1";
			mawb.CM_ArrivalDate = ZDateTime.Now;

			line.DL_ConsigneeReference = "HB1";
			hawb.CS_HAWB = "HB1";
			hawb.CS_JS = shipment.PK;
			hawb.CS_CM = mawb.PK;

			var mawb2 = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			mawb2.CM_MasterHouseBill = "MB1";

			var hawb2 = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			hawb2.CS_HAWB = "HB1";
			hawb2.CS_JS = shipment2.PK;

			hawb2.CS_CM = mawb2.PK;

			mawb2.CM_ArrivalDate = ZDateTime.Now.AddMonths(24);

			Factory.Save();

			AssertCollectionContains(hawb, line.BusinessObjectsWithRelatedEvents);
			AssertCollectionNotContains(hawb2, line.BusinessObjectsWithRelatedEvents);
		}

		public void TestDocumentSupporter()
		{
			var line = Factory.New<SupplierBookingLine>();
			AssertEquals("Document Supporter should be of type", typeof(SupplierBookingLineDocumentSupporter), line.DocumentSupporter.GetType());
		}

		public void TestHasValidGS1Prefix()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorMainAddressPK = consignor.MainAddress.PK;
			var consignorGS1Prefix = consignor.CustomsCodes.AddNew();
			consignorGS1Prefix.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			consignorGS1Prefix.OK_OA_PremisesAddress = consignorMainAddressPK;
			consignorGS1Prefix.OK_CustomsRegNo = "";
			AssertHasValidGS1Prefix(consignorMainAddressPK, false, "Expected false as the GS1 prefix should not be empty");

			consignorGS1Prefix.OK_CustomsRegNo = "123456";
			AssertHasValidGS1Prefix(consignorMainAddressPK, false, "Expected false as the GS1 prefix on the consignor is not between 7-11 digits long");

			consignorGS1Prefix.OK_CustomsRegNo = "7654321";
			AssertHasValidGS1Prefix(consignorMainAddressPK, true, "Expected true as the GS1 prefix on the consignor is 7 digits long");

			consignorGS1Prefix.OK_CustomsRegNo = "87654321";
			AssertHasValidGS1Prefix(consignorMainAddressPK, true, "Expected true as the GS1 prefix on the consignor is 8 digits long");

			consignorGS1Prefix.OK_CustomsRegNo = "987654321";
			AssertHasValidGS1Prefix(consignorMainAddressPK, true, "Expected true as the GS1 prefix on the consignor is 9 digits long");

			consignorGS1Prefix.OK_CustomsRegNo = "9876543210";
			AssertHasValidGS1Prefix(consignorMainAddressPK, true, "Expected true as the GS1 prefix on the consignor is 10 digits long");

			consignorGS1Prefix.OK_CustomsRegNo = "98765432101";
			AssertHasValidGS1Prefix(consignorMainAddressPK, true, "Expected true as the GS1 prefix on the consignor is 11 digits long");

			consignorGS1Prefix.OK_CustomsRegNo = "987654321012";
			AssertHasValidGS1Prefix(consignorMainAddressPK, false, "Expected false as the GS1 prefix on the consignor is not between 7-11 digits long");

			consignorGS1Prefix.OK_CustomsRegNo = "ABC1234";
			AssertHasValidGS1Prefix(consignorMainAddressPK, false, "Expected false as the GS1 prefix is a valid length but is not purely numeric characters");
		}

		void AssertHasValidGS1Prefix(ZGuid consignorMainAddressPK, bool expectedResult, string testMessage)
		{
			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_OA_Consignor = consignorMainAddressPK;
			var line = header.BookingLines.AddNew();

			AssertEquals(testMessage, expectedResult, line.HasValidGS1Prefix);
		}

		#region Test Consignment Reference Generation

		#region Generation On Saved Succeeded

		public void TestConsignmentReferenceGenerationOnSaved_GS1FromBookingHeader()
		{
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var shipperAddressLessCode = shipper.CustomsCodes.AddNew();
			shipperAddressLessCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			shipperAddressLessCode.OK_CustomsRegNo = "7654321";
			shipperAddressLessCode.OK_OA_PremisesAddress = ZGuid.Empty;

			Factory.Save();

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_OA_Consignor = shipper.MainAddress.PK;

			AssertGeneratedReference(header.PK, "00076543210000000015");
			AssertGeneratedReference(header.PK, "00076543210000000022");
		}

		public void TestConsignmentReferenceGenerationOnSaved_GS1FromBookingHeaderWithOrgFountain()
		{
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var shipperAddressLessCode = shipper.CustomsCodes.AddNew();
			shipperAddressLessCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			shipperAddressLessCode.OK_CustomsRegNo = "7654321";
			shipperAddressLessCode.OK_OA_PremisesAddress = ZGuid.Empty;

			var fountain = shipper.OrgFountains.AddNew();
			fountain.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			fountain.SN_Prefix = "7654321";
			fountain.SN_MinimumValue = 5001;
			fountain.SN_MaximumValue = 10000000;

			var anotherFountain = shipper.OrgFountains.AddNew();
			anotherFountain.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			anotherFountain.SN_Prefix = "1231234";
			anotherFountain.SN_MinimumValue = 9999;
			anotherFountain.SN_MaximumValue = 10000000;

			Factory.Save();

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_OA_Consignor = shipper.MainAddress.PK;

			AssertGeneratedReference(header.PK, "00076543210000050010");
			AssertGeneratedReference(header.PK, "00076543210000050027");
		}

		public void TestConsignmentReferenceGenerationOnSaved_GS1FromBookingHeader_PrefersConsignorAddress()
		{
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var shipperAddressLessCode = shipper.CustomsCodes.AddNew();
			shipperAddressLessCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			shipperAddressLessCode.OK_CustomsRegNo = "7777777";
			shipperAddressLessCode.OK_OA_PremisesAddress = ZGuid.Empty;

			var shipperMainAddressCode = shipper.CustomsCodes.AddNew();
			shipperMainAddressCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			shipperMainAddressCode.OK_CustomsRegNo = "7654321";
			shipperMainAddressCode.OK_OA_PremisesAddress = shipper.MainAddress.PK;

			Factory.Save();

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_OA_Consignor = shipper.MainAddress.PK;

			AssertGeneratedReference(header.PK, "00076543210000000015");
			AssertGeneratedReference(header.PK, "00076543210000000022");
		}

		public void TestConsignmentReferenceGenerationOnSaved_GS1FromBookingHeader_FallBackIfNoConsignorAddress()
		{
			var alternativeAddress = Factory.NewWithValidTestData<OrgAddress>();
			var shipper = Factory.Load<OrgHeader>(alternativeAddress.OA_OH);

			var codeWithAlternativeAddress = shipper.CustomsCodes.AddNew();
			codeWithAlternativeAddress.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			codeWithAlternativeAddress.OK_CustomsRegNo = "987654321";
			codeWithAlternativeAddress.OK_OA_PremisesAddress = alternativeAddress.PK;

			var codeWithNoAddress = shipper.CustomsCodes.AddNew();
			codeWithNoAddress.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			codeWithNoAddress.OK_CustomsRegNo = "123456789";
			codeWithNoAddress.OK_OA_PremisesAddress = ZGuid.Empty;

			Factory.Save();

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>();
			header.DH_OA_Consignor = shipper.MainAddress.PK;

			AssertGeneratedReference(header.PK, "00012345678900000012");
			AssertGeneratedReference(header.PK, "00012345678900000029");
		}

		public void TestConsignmentReferenceGenerationOnSaved_GS1FromCompanyOrgProxy_PrefersBranch()
		{
			var orgProxy = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			var orgAddressWithBranchUnloco = Factory.NewWithValidTestData<OrgAddress>();
			orgAddressWithBranchUnloco.OA_OH = orgProxy.PK;
			orgAddressWithBranchUnloco.OA_RL_NKRelatedPortCode = Env.CurrentBranch.NKUNLOCO;

			var customCode = orgProxy.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			customCode.OK_CustomsRegNo = "7777777";
			customCode.OK_OA_PremisesAddress = ZGuid.Empty;

			customCode = orgProxy.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			customCode.OK_CustomsRegNo = "999999999";
			customCode.OK_OA_PremisesAddress = orgAddressWithBranchUnloco.PK;

			Factory.Save();

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>().PK;

			AssertGeneratedReference(header, "00099999999900000014");
		}

		public void TestConsignmentReferenceGenerationOnSaved_GS1FromCompanyOrgProxyWithOrgFountain()
		{
			var orgProxy = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);

			var customCode = orgProxy.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			customCode.OK_CustomsRegNo = "7777777";
			customCode.OK_OA_PremisesAddress = ZGuid.Empty;

			var fountain = orgProxy.OrgFountains.AddNew();
			fountain.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			fountain.SN_Prefix = "7777777";
			fountain.SN_MinimumValue = 5001;
			fountain.SN_MaximumValue = 1000000;

			Factory.Save();

			var header = Factory.NewWithValidTestData<SupplierBookingHeader>().PK;

			AssertGeneratedReference(header, "00077777770000050011");
		}

		void AssertGeneratedReference(ZGuid headerPK, ZString expectedReference)
		{
			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			line.DL_DH_BookingHeader = headerPK;
			line.DL_ConsigneeReference = "";

			Factory.Save();

			Assert("Expected booking line to have been saved successfully", line.IsInDatabase);
			Assert("Expected to have generated a reference number", !line.DL_ConsigneeReference.IsEmpty);
			AssertEquals("Expected to have found the proxy number with the same country location as the current branch", expectedReference, line.DL_ConsigneeReference);

			var expectedLog = ZString.Format("SSCC '{0}' has been generated", expectedReference);
			var log = line.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, expectedLog));
			AssertNotNull("Expected to have generated a log as saving was successful", log);
		}

		#endregion

		#region Generation On Saved Failed

		public void TestConsignmentReferenceGenerationOnSaved_FailedSaving()
		{
			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			var bookingLine1 = bookingHeader.BookingLines.AddNew();
			bookingLine1.DL_ConsigneeReference = "SH4712337134963294";
			var bookingLine2 = bookingHeader.BookingLines.AddNew();
			bookingLine2.DL_ConsigneeReference = "";

			TrySavingAndFail(bookingHeader, bookingLine1, bookingLine2);
			TrySavingAndFail(bookingHeader, bookingLine1, bookingLine2);
		}

		void TrySavingAndFail(SupplierBookingHeader bookingHeader, SupplierBookingLine bookingLineWithReference, SupplierBookingLine bookingLineWithoutReference)
		{
			try
			{
				bookingHeader.DH_OA_Consignor = ZGuid.Empty;

				Factory.Save();

				Fail("Should not reach this point as we cannot save when booking header address is empty");
			}
			catch (ZSaveException) { }

			Assert("Expected booking header not to have been saved", !bookingHeader.IsInDatabase);
			Assert("Expected booking line not to have been saved", !bookingLineWithReference.IsInDatabase);
			Assert("Expected booking line not to have been saved", !bookingLineWithoutReference.IsInDatabase);

			AssertEquals("Expected not to have changed the reference number", "SH4712337134963294", bookingLineWithReference.DL_ConsigneeReference);
			AssertEquals("Expected the reference to remain blank as saving was not successful", ZString.Empty, bookingLineWithoutReference.DL_ConsigneeReference);
		}

		#endregion

		#region Generated References do not conflict with Imported References

		public void TestConsignmentReferenceGeneration_GeneratedAndImportedNumbersDoNoConflict_WithGS1Number()
		{
			var currentBranchCountryCode = ((ZString)Env.CurrentBranch.NKUNLOCO).SubstringSafe(0, 2);
			var refCountry = RefCountry.LoadFromCountryCode(Factory, currentBranchCountryCode);

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.SetCustomsCode(OrgCusCode.CodeTypes.GS1, refCountry, "7654321");

			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			bookingHeader.DH_OA_Consignor = shipper.MainAddress.PK;

			Factory.Save();

			const string reference1 = "00076543210000000015";
			const string reference2 = "00076543210000000022";

			var bookingLine1 = bookingHeader.BookingLines.AddNew();
			var bookingLine2 = bookingHeader.BookingLines.AddNew();
			var bookingLine3 = bookingHeader.BookingLines.AddNew();
			var bookingLine4 = bookingHeader.BookingLines.AddNew();

			bookingLine1.DL_ConsigneeReference = reference1;
			bookingLine2.DL_ConsigneeReference = "";
			bookingLine3.DL_ConsigneeReference = reference2;
			bookingLine4.DL_ConsigneeReference = "";

			Factory.Save();

			foreach (SupplierBookingLine line in bookingHeader.BookingLines)
			{
				Assert(line.IsInDatabase);
				Assert(!line.DL_ConsigneeReference.IsEmpty);

				AssertStartsWith("Expected all references to begin with shipper's GS1", "0007654321", line.DL_ConsigneeReference);
				Assert("Expected number fountain to have generated unique numbers for all shipments", !line.DL_ConsigneeReferenceInfo.Notifications.HasErrors());
			}
		}

		#endregion

		#endregion

		#region TestIsPackageIdValidSSCCBarCode

		public void TestIsPackageIdValidSSCCBarCode()
		{
			var supplierBookingLine = Factory.NewWithValidTestData<SupplierBookingLine>();

			supplierBookingLine.DL_ConsigneeReference = "AA6765167700000010"; // invalid check-digit
			AssertEquals(false, supplierBookingLine.IsPackageIdValidSSCCBarCode);

			supplierBookingLine.DL_ConsigneeReference = "016765167700000011";
			AssertEquals(true, supplierBookingLine.IsPackageIdValidSSCCBarCode);

			supplierBookingLine.DL_ConsigneeReference = "00016765167700000011";
			AssertEquals(true, supplierBookingLine.IsPackageIdValidSSCCBarCode);
		}

		#endregion

		#region CarrierServiceLevel

		public void TestCarrierServiceLevelNoExceptionFromNonUniqueCode()
		{
			var bookingLine = Factory.New<SupplierBookingLine>();
			bookingLine.DL_PL_NKCarrierServiceLevel = "ABC";

			var serviceLevel = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "ABC";

			var serviceLevel2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel2.PL_Code = "ABC";

			AssertNoExceptionThrown("Accessing CarrierServiceLevel should not throw exception", () => { _ = bookingLine.CarrierServiceLevel; });
		}

		#endregion

		public override void TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed()
		{
			// TODO: This should be undo when when Enterprise.Integration.Customs.AU.ICustomsManifestLineSequence is changed to make JobConsol it's parent; there is a requirement that a CustomsManifestLineSequence record still exists after this bizobj is deleted and this record is can be linked back JobConsol
			Assert(true);
		}
	}
}

