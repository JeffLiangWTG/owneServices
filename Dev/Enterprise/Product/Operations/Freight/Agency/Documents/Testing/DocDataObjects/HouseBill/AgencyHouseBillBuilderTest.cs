using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Agency.Documents.DocDataObjects.Testing.AssertionHelper;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class AgencyHouseBillBuilderTest : TestCaseWithFactory
	{
		#region PopulateGeneralInfo

		public void TestPopulateGeneralInfo()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var billOfLading = GetBillOfLading();

			var billOfLadingImageCollection = new BillOfLadingImageCollection();
			var billOfLadingImage = billOfLadingImageCollection.AddNew();
			billOfLadingImage.PrincipalPK = billOfLading.Principal.PK;
			billOfLadingImage.Description = (NoResString)"XYZ";
			billOfLadingImage.Image = new Bitmap(10, 10);
			billOfLadingImage.Enabled = true;

			using (AgencyRegistry.Instance.BillOfLadingLogosImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingTermsAndConditionsImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, billOfLadingImageCollection))
			using (AgencyRegistry.Instance.BillOfLadingClause(billOfLading.Principal).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Blaticus1"))
			{
				var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, parameters).Build();

				Assert(agencyHouseBill.IsOriginal);
				AssertEquals("Refer to clause on reverse side", agencyHouseBill.ExcessValueDeclaration);
				AssertEquals(ContainerModes.FCL, agencyHouseBill.ContainerMode.Code);
				AssertEquals(ShipmentReleaseTypes.SeaWaybill, agencyHouseBill.ReleaseType.Code);

				AssertEquals("HOUSEBILL001", agencyHouseBill.OceanBillNumber);
				AssertEquals(2, agencyHouseBill.NumberOfOriginals);

				AssertNotNull(agencyHouseBill.Logo.Image);
				AssertNotNull(agencyHouseBill.TermsAndConditions.Image);
				AssertEquals("Blaticus1", agencyHouseBill.Clause);

				AssertEquals(2, agencyHouseBill.Transports.Count);
				AssertEquals("MAIN.Vessel", agencyHouseBill.Vessel);
				AssertEquals("MAINVoyage", agencyHouseBill.Voyage);

				AssertEquals(FreightConstants.ShippedOnBoardType.Shipped, agencyHouseBill.ShippedOnBoard.Code);
				AssertEquals(new ZDateTime(2024, 1, 1), agencyHouseBill.ShippedOnBoardDate);

				AssertEquals(DomesticPaymentTerms.Prepaid, agencyHouseBill.PaymentTerm.Code);

				AssertEquals(10, agencyHouseBill.OuterPacks);
				AssertEquals(PkgUnit.Bottle, agencyHouseBill.OuterPacksPackType);
			}
		}

		#endregion

		#region PopulateAddresses

		public void TestPopulateAddresses()
		{
			var billOfLading = GetBillOfLading();
			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertAddressData(billOfLading.ConsignorDocumentaryAddress, agencyHouseBill.Shipper);

			AssertAddressData(billOfLading.ConsigneeDocumentaryAddress, agencyHouseBill.Consignee);
			AssertAddressToOrder(agencyHouseBill.Consignee as Address);

			AssertAddressData(billOfLading.NotifyPartyDocumentaryAddress, agencyHouseBill.NotifyParty);
			AssertAddressSameAsConsignee(agencyHouseBill.NotifyParty as Address);
		}

		#endregion

		#region PopulateLocations

		public void TestPopulateLocations()
		{
			var billOfLading = GetBillOfLading();
			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertEquals(billOfLading.JS_RL_NKDestination, agencyHouseBill.PortOfDestination.Code);
			AssertEquals(billOfLading.Transports.Cast<Freight.Business.Transport>().First(x => x.JW_TransportType == TransportPlanningType.MainVessel).JW_RL_NKLoadPort, agencyHouseBill.PortOfLoading.Code);
			AssertEquals(billOfLading.Transports.Cast<Freight.Business.Transport>().First(x => x.JW_TransportType == TransportPlanningType.MainVessel).JW_RL_NKDiscPort, agencyHouseBill.PortOfDischarge.Code);
			AssertEquals(billOfLading.JS_RL_NKOrigin, agencyHouseBill.FreightPayableAt.Code);

			AssertEquals(billOfLading.JS_RL_NKHouseBillIssuePlace, agencyHouseBill.PlaceOfIssue.Code);
			AssertEquals(billOfLading.JS_RL_NKOrigin, agencyHouseBill.PlaceOfReceipt.Code);
			AssertEquals(billOfLading.JS_RL_NKDestination, agencyHouseBill.PlaceOfDelivery.Code);

			billOfLading.JS_HouseBillIssueDate = ZDate.Empty;
			billOfLading.JS_INCO = DomesticPaymentTerms.Collect;
			billOfLading.JS_RL_NKPlaceOfReceipt = "CNSHG";
			billOfLading.JS_RL_NKPlaceOfDischarge = "SGSIN";
			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertEquals(billOfLading.JS_RL_NKDestination, agencyHouseBill.FreightPayableAt.Code);

			AssertEquals(billOfLading.JS_RL_NKHouseBillIssuePlace, agencyHouseBill.PlaceOfIssue.Code);
			AssertEquals(billOfLading.JS_RL_NKPlaceOfReceipt, agencyHouseBill.PlaceOfReceipt.Code);
			AssertEquals(billOfLading.JS_RL_NKPlaceOfDischarge, agencyHouseBill.PlaceOfDelivery.Code);

			billOfLading.JS_INCO = DomesticPaymentTerms.CollectCOD;
			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
			AssertNullOrEmpty(agencyHouseBill.FreightPayableAt.Code);
		}

		public void TestPopulateLocations_PlaceOfIssue()
		{
			var billOfLading = GetBillOfLading();
			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertEquals(billOfLading.JS_RL_NKHouseBillIssuePlace, agencyHouseBill.PlaceOfIssue.Code);

			billOfLading.JS_HouseBillIssueDate = ZDate.Empty;
			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertEquals(billOfLading.JS_RL_NKHouseBillIssuePlace, agencyHouseBill.PlaceOfIssue.Code);

			billOfLading.JS_HouseBillIssueDate = new ZDateTime(2024, 03, 28);
			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertEquals(billOfLading.JS_RL_NKHouseBillIssuePlace, agencyHouseBill.PlaceOfIssue.Code);

			billOfLading.JS_RL_NKHouseBillIssuePlace = ZString.Empty;
			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertEquals(billOfLading.Transports.Cast<Freight.Business.Transport>().First(x => x.JW_TransportType == TransportPlanningType.MainVessel).JW_RL_NKLoadPort, agencyHouseBill.PlaceOfIssue.Code);
		}

		#endregion

		#region PopulateDates

		public void TestPopulateDates()
		{
			var billOfLading = GetBillOfLading();
			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertEquals(billOfLading.JS_HouseBillIssueDate, agencyHouseBill.DateOfIssue);
			AssertEquals(billOfLading.JS_E_DEP, agencyHouseBill.DepartureDate);
			AssertEquals(billOfLading.JS_E_ARV, agencyHouseBill.ArrivalDate);
		}

		#endregion

		#region PopulateGoodsDetails

		public void TestPopulateGoodsDetails()
		{
			var billOfLading = GetBillOfLading();

			billOfLading.JS_MarksAndNumbers = "BillOfLading MarksAndNumbers";
			billOfLading.JS_GoodsDescription = "BillOfLading GoodsDescription";

			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
			AssertEquals(billOfLading.JS_MarksAndNumbers, agencyHouseBill.MarksAndNumbers);
			AssertEquals(billOfLading.JS_GoodsDescription, agencyHouseBill.GoodsDescription);

			billOfLading.Notes.RemoveAndDeleteAll();

			var marksAndNumbersNote = billOfLading.Notes.AddNew();
			marksAndNumbersNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbersNote.ST_NoteText = "MarksAndNumbersNoteText";

			var detailedGoodsDescriptionNote = billOfLading.Notes.AddNew();
			detailedGoodsDescriptionNote.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			detailedGoodsDescriptionNote.ST_NoteText = "DetailedGoodsDescriptionText";

			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
			AssertEquals(marksAndNumbersNote.ST_NoteText, agencyHouseBill.MarksAndNumbers);
			AssertEquals(detailedGoodsDescriptionNote.ST_NoteText, agencyHouseBill.GoodsDescription);

			marksAndNumbersNote.ST_NoteText = ZString.Empty;
			detailedGoodsDescriptionNote.ST_NoteText = ZString.Empty;

			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
			AssertEquals(billOfLading.JS_MarksAndNumbers, agencyHouseBill.MarksAndNumbers);
			AssertEquals(billOfLading.JS_GoodsDescription, agencyHouseBill.GoodsDescription);

			billOfLading.JS_MarksAndNumbers = ZString.Empty;
			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
				AssertNullOrEmpty(agencyHouseBill.MarksAndNumbers);
			}

			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
				AssertNullOrEmpty(agencyHouseBill.MarksAndNumbers);
			}

			var outerPackLine = billOfLading.OuterPackLines.Cast<BillOfLadingPackLine>().First();
			outerPackLine.JL_MarksAndNumbers = "PackingLine's MarksAndNumbers";

			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
				AssertEquals("PackingLine's MarksAndNumbers", agencyHouseBill.MarksAndNumbers);
			}

			outerPackLine.JL_MarksAndNumbers = "Loose PackingLine's MarksAndNumbers";
			outerPackLine.JL_JC = ZGuid.Empty;

			using (AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
				AssertEquals("Loose PackingLine's MarksAndNumbers", agencyHouseBill.MarksAndNumbers);
			}
		}

		#region PopulateGoodsDetails FCL

		public void TestPopulateContainersAndTotalWeightAndVolumeAndNumberOfPackages()
		{
			var billOfLading = GetBillOfLading();

			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
			AssertEquals(2, agencyHouseBill.Containers.Count);
			AssertEquals(5000M, agencyHouseBill.TotalWeight.Value);
			AssertEquals(Weight.Kilograms, agencyHouseBill.TotalWeight.Unit.Code);
			AssertEquals(2.6M, agencyHouseBill.TotalVolume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.TotalVolume.Unit.Code);
			AssertEquals(2, agencyHouseBill.TotalNumberOfPackages);

			AssertEquals((ZDecimal)2000, agencyHouseBill.Containers.First(x => x.Number == "AAAA0000007").GoodsWeight.Value);
			AssertEquals(Weight.Kilograms, agencyHouseBill.Containers.First(x => x.Number == "AAAA0000007").GoodsWeight.Unit.Code);
			AssertEquals((ZDecimal)1.3, agencyHouseBill.Containers.First(x => x.Number == "AAAA0000007").Volume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.Containers.First(x => x.Number == "AAAA0000007").Volume.Unit.Code);

			AssertEquals((ZDecimal)3000, agencyHouseBill.Containers.First(x => x.Number == "BBBB0000007").GoodsWeight.Value);
			AssertEquals(Weight.Kilograms, agencyHouseBill.Containers.First(x => x.Number == "BBBB0000007").GoodsWeight.Unit.Code);
			AssertEquals((ZDecimal)1.3, agencyHouseBill.Containers.First(x => x.Number == "BBBB0000007").Volume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.Containers.First(x => x.Number == "BBBB0000007").Volume.Unit.Code);

			var outerPackLine = billOfLading.OuterPackLines.Cast<BillOfLadingPackLine>().First();
			outerPackLine.JL_ActualWeight = 3000;
			outerPackLine.JL_ActualWeightUQ = Weight.Kilograms;
			outerPackLine.JL_ActualVolume = 3.33;
			outerPackLine.JL_ActualVolumeUQ = Volume.CubicMetres;

			var newPackline = billOfLading.OuterPackLines.AddNew();
			newPackline.JL_ActualWeight = 2000;
			newPackline.JL_ActualWeightUQ = Weight.Grams;
			newPackline.JL_ActualVolume = 1.3;
			newPackline.JL_ActualVolumeUQ = Volume.Litre;
			newPackline.JL_JS = billOfLading.PK;
			outerPackLine.Container.PackLines.Add(newPackline);

			outerPackLine.Container.JC_ContainerCount = 555;

			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
			AssertEquals(2, agencyHouseBill.Containers.Count);
			AssertEquals((ZDecimal)6002, agencyHouseBill.TotalWeight.Value);
			AssertEquals(Weight.Kilograms, agencyHouseBill.TotalWeight.Unit.Code);
			AssertEquals((ZDecimal)4.6313, agencyHouseBill.TotalVolume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.TotalVolume.Unit.Code);
			AssertEquals(556, agencyHouseBill.TotalNumberOfPackages);
		}

		public void TestPopulateLoosePackingLinesAndTotalWeightAndVolumeAndNumberOfPackages()
		{
			var billOfLading = GetBillOfLading();

			var outerPackLine = billOfLading.OuterPackLines.Cast<BillOfLadingPackLine>().First();
			outerPackLine.JL_ActualWeight = 3000;
			outerPackLine.JL_ActualWeightUQ = Weight.Kilograms;
			outerPackLine.JL_ActualVolume = 3.33;
			outerPackLine.JL_ActualVolumeUQ = Volume.CubicMetres;
			outerPackLine.JL_PackageCount = 100;
			outerPackLine.JL_JC = ZGuid.Empty;

			var newPackline = billOfLading.OuterPackLines.AddNew();
			newPackline.JL_ActualWeight = 2000;
			newPackline.JL_ActualWeightUQ = Weight.Grams;
			newPackline.JL_ActualVolume = 1.3;
			newPackline.JL_ActualVolumeUQ = Volume.Litre;
			newPackline.JL_JS = billOfLading.PK;
			newPackline.JL_JC = ZGuid.Empty;

			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();
			AssertEquals(2, agencyHouseBill.LoosePackingLines.Count);

			AssertEquals(100, agencyHouseBill.LoosePackingLines.First(x => x.Weight.Value == (ZDecimal)3000).Quantity);
			AssertEquals(Weight.Kilograms, agencyHouseBill.LoosePackingLines.First(x => x.Weight.Value == (ZDecimal)3000).Weight.Unit.Code);
			AssertEquals((ZDecimal)3.33, agencyHouseBill.LoosePackingLines.First(x => x.Weight.Value == (ZDecimal)3000).Volume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.LoosePackingLines.First(x => x.Weight.Value == (ZDecimal)3000).Volume.Unit.Code);

			AssertEquals(1, agencyHouseBill.LoosePackingLines.First(x => x.Weight.Value == (ZDecimal)2).Quantity);
			AssertEquals(Weight.Kilograms, agencyHouseBill.LoosePackingLines.First(x => x.Weight.Value == (ZDecimal)2).Weight.Unit.Code);
			AssertEquals((ZDecimal)0.0013, agencyHouseBill.LoosePackingLines.First(x => x.Weight.Value == (ZDecimal)2).Volume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.LoosePackingLines.First(x => x.Weight.Value == (ZDecimal)2).Volume.Unit.Code);

			AssertEquals((ZDecimal)6002, agencyHouseBill.TotalWeight.Value);
			AssertEquals(Weight.Kilograms, agencyHouseBill.TotalWeight.Unit.Code);
			AssertEquals((ZDecimal)4.6313, agencyHouseBill.TotalVolume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.TotalVolume.Unit.Code);
			AssertEquals(103, agencyHouseBill.TotalNumberOfPackages);
		}

		#endregion

		#region PopulateGoodsDetails Non FCL

		public void TestPopulateTopLevelPackingLinesAndTotalWeightAndVolumeAndNumberOfPackages()
		{
			var billOfLading = GetBillOfLading(ContainerModes.RollOnRollOff);

			billOfLading.ShippingContainers.RemoveAndDeleteAll();

			var container1 = billOfLading.ShippingContainers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_GrossWeight = 10000;
			container1.JC_GrossWeightUQ = Weight.Grams;
			container1.JC_GrossVolume = 10000;
			container1.JC_GrossVolumeUQ = Volume.MegaLitre;

			var container2 = billOfLading.ShippingContainers.AddNew();
			container2.JC_ContainerNum = "BBBB0000007";
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_GrossWeight = 1000;
			container2.JC_GrossWeightUQ = Weight.Kilograms;
			container2.JC_GrossVolume = 1000;
			container2.JC_GrossVolumeUQ = Volume.CubicMetres;
			container2.JC_ContainerCount = 5;

			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertEquals(2, agencyHouseBill.TopLevelPackingLines.Count);

			AssertEquals((ZDecimal)10, agencyHouseBill.TopLevelPackingLines.First(x => x.ReferenceNumber == "AAAA0000007").Weight.Value);
			AssertEquals(Weight.Kilograms, agencyHouseBill.TopLevelPackingLines.First(x => x.ReferenceNumber == "AAAA0000007").Weight.Unit.Code);
			AssertEquals((ZDecimal)10000000, agencyHouseBill.TopLevelPackingLines.First(x => x.ReferenceNumber == "AAAA0000007").Volume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.TopLevelPackingLines.First(x => x.ReferenceNumber == "AAAA0000007").Volume.Unit.Code);

			AssertEquals((ZDecimal)1000, agencyHouseBill.TopLevelPackingLines.First(x => x.ReferenceNumber == "BBBB0000007").Weight.Value);
			AssertEquals(Weight.Kilograms, agencyHouseBill.TopLevelPackingLines.First(x => x.ReferenceNumber == "BBBB0000007").Weight.Unit.Code);
			AssertEquals((ZDecimal)1000, agencyHouseBill.TopLevelPackingLines.First(x => x.ReferenceNumber == "BBBB0000007").Volume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.TopLevelPackingLines.First(x => x.ReferenceNumber == "BBBB0000007").Volume.Unit.Code);

			AssertEquals((ZDecimal)1010, agencyHouseBill.TotalWeight.Value);
			AssertEquals(Weight.Kilograms, agencyHouseBill.TotalWeight.Unit.Code);
			AssertEquals((ZDecimal)10001000, agencyHouseBill.TotalVolume.Value);
			AssertEquals(Volume.CubicMetres, agencyHouseBill.TotalVolume.Unit.Code);
			AssertEquals(6, agencyHouseBill.TotalNumberOfPackages);
		}

		#endregion

		#endregion

		#region PopulateCharges

		public void TestPopulateCharges()
		{
			var billOfLading = GetBillOfLading();

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_FullName = "Ziggy Z";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			localClient.MainAddress.Address1 = "Unit 13";
			localClient.MainAddress.Address2 = "4 Lost Lane";
			localClient.MainAddress.City = "Sydney";
			localClient.MainAddress.Postcode = "2000";
			localClient.MainAddress.OA_RN_NKCountryCode = "AU";

			var agentCollect = Factory.New<OrgHeader>();
			agentCollect.OH_FullName = "Airmarine Inc.";
			agentCollect.OH_RL_NKClosestPort = "USCHI";
			agentCollect.MainAddress.Address1 = "5638 S Central Ave";
			agentCollect.MainAddress.City = "Chicago";
			agentCollect.MainAddress.Postcode = "60638";
			agentCollect.MainAddress.OA_RN_NKCountryCode = "US";

			var loader = new JobHeader.Loader(billOfLading);
			var header = loader.TryLoadOrCreate();

			header.LocalChargesPK = localClient.PK;
			header.AgentCollectPK = agentCollect.PK;

			var exRates = (BusinessObjectCollection)header["ExchangeRates"];

			var usdRate = exRates.AddNew();
			usdRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "USD";
			usdRate[JobExRateSchema.Constants.JF_BaseRate] = 1.2;

			var auRate = exRates.AddNew();
			auRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "AUD";
			auRate[JobExRateSchema.Constants.JF_BaseRate] = 1.1;

			CreateLineCharge(billOfLading.Job, billOfLading.Job.LocalChargesPK, 40m, "DDOC", "USD");
			CreateLineCharge(billOfLading.Job, billOfLading.Job.AgentCollectPK, 30m, "CAF", "AUD");
			CreateLineCharge(billOfLading.Job, billOfLading.Job.LocalChargesPK, 20m, "FRT", "USD");

			billOfLading.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidAndCollectCharges;
			AssertEquals(3, new AgencyHouseBillBuilder(billOfLading, null).Build().Charges.Count);

			billOfLading.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.PrepaidCharges;
			AssertEquals(2, new AgencyHouseBillBuilder(billOfLading, null).Build().Charges.Count);

			billOfLading.JS_HBLAWBChargesDisplay = DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges;
			AssertEquals(1, new AgencyHouseBillBuilder(billOfLading, null).Build().Charges.Count);
		}

		#endregion

		#region Validation

		#region GeneralInfoValidation

		public void TestGeneralInfoValidation()
		{
			var billOfLading = GetBillOfLading();
			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertNoMessageError(agencyHouseBill.OceanBillNumberInfo, "Ocean Bill Number is required.");
			AssertNoMessageError((agencyHouseBill.ReleaseType as CodeDescription)?.DescriptionInfo, "Release Type is required.");
			AssertNoMessageError(agencyHouseBill.VesselInfo, "Vessel is required.");
			AssertNoMessageError(agencyHouseBill.VoyageInfo, "Voyage is required.");
			AssertNoMessageError(agencyHouseBill.NumberOfOriginalsInfo, "No of Originals is required.");

			billOfLading.JS_HouseBill = ZString.Empty;
			billOfLading.JS_ReleaseType = ZString.Empty;
			billOfLading.JS_NoOriginalBills = 0;

			var mainTransport = billOfLading.Transports.Cast<Freight.Business.Transport>().First(x => x.JW_TransportType == TransportPlanningType.MainVessel);
			mainTransport.JW_Vessel = ZString.Empty;
			mainTransport.JW_VoyageFlight = ZString.Empty;

			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertHasMessageError(agencyHouseBill.OceanBillNumberInfo, "Ocean Bill Number is required.");
			AssertHasMessageError((agencyHouseBill.ReleaseType as CodeDescription)?.DescriptionInfo, "Release Type is required.");
			AssertHasMessageError(agencyHouseBill.VesselInfo, "Vessel is required.");
			AssertHasMessageError(agencyHouseBill.VoyageInfo, "Voyage is required.");
			AssertHasMessageError(agencyHouseBill.NumberOfOriginalsInfo, "No of Originals is required.");
		}

		#endregion

		#region AddressesValidation

		public void TestAddressesValidation()
		{
			var billOfLading = GetBillOfLading();
			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertNoMessageError((agencyHouseBill.Consignee as Address).AddressFormattedInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertNoMessageError((agencyHouseBill.NotifyParty as Address).AddressFormattedInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");
			AssertNoMessageError((agencyHouseBill.Shipper as Address).AddressFormattedInfo, "Shipper party name and address information is required.");

			agencyHouseBill.Consignee.CompanyName = ZString.Empty;
			agencyHouseBill.NotifyParty.CompanyName = ZString.Empty;
			agencyHouseBill.Shipper.CompanyName = ZString.Empty;

			AssertHasMessageError((agencyHouseBill.Consignee as Address).AddressFormattedInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertHasMessageError((agencyHouseBill.NotifyParty as Address).AddressFormattedInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");
			AssertHasMessageError((agencyHouseBill.Shipper as Address).AddressFormattedInfo, "Shipper party name and address information is required.");
		}

		#endregion

		#region LocationsValidation

		public void TestLocationsValidation()
		{
			var billOfLading = GetBillOfLading();
			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertNoMessageError((agencyHouseBill.PortOfLoading as Unloco)?.CodeInfo, "Port of Lading is required.");
			AssertNoMessageError((agencyHouseBill.PortOfDischarge as Unloco)?.CodeInfo, "Port of Discharge is required.");

			billOfLading.Transports.RemoveAndDeleteAll();
			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertHasMessageError((agencyHouseBill.PortOfLoading as Unloco)?.CodeInfo, "Port of Lading is required.");
			AssertHasMessageError((agencyHouseBill.PortOfDischarge as Unloco)?.CodeInfo, "Port of Discharge is required.");
		}

		#endregion

		#region GoodsDetailsValidation

		public void TestGoodsDetailsValidation()
		{
			var billOfLading = GetBillOfLading();
			billOfLading.JS_MarksAndNumbers = "BillOfLading MarksAndNumbers";
			billOfLading.JS_GoodsDescription = "BillOfLading GoodsDescription";
			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertNoMessageError(agencyHouseBill.ErrorPlaceHolderInfo, "Marks and Numbers is required.");
			AssertNoMessageError(agencyHouseBill.ErrorPlaceHolderInfo, "Description of Goods is required.");
			AssertNoMessageError(agencyHouseBill.ErrorPlaceHolderInfo, "Total Weight is required.");
			AssertNoMessageError((agencyHouseBill.TotalWeight as Measurement)?.ValueInfo, "Total Weight is required.");
			AssertNoMessageError(agencyHouseBill.ErrorPlaceHolderInfo, "Total Volume is required.");
			AssertNoMessageError((agencyHouseBill.TotalVolume as Measurement)?.ValueInfo, "Total Volume is required.");

			billOfLading.JS_MarksAndNumbers = ZString.Empty;
			billOfLading.JS_GoodsDescription = ZString.Empty;
			billOfLading.Notes.RemoveAndDeleteAll();
			billOfLading.OuterPackLines.RemoveAndDeleteAll();
			agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			AssertHasMessageError(agencyHouseBill.ErrorPlaceHolderInfo, "Marks and Numbers is required.");
			AssertHasMessageError(agencyHouseBill.ErrorPlaceHolderInfo, "Description of Goods is required.");
			AssertHasMessageError(agencyHouseBill.ErrorPlaceHolderInfo, "Total Weight is required.");
			AssertHasMessageError((agencyHouseBill.TotalWeight as Measurement)?.ValueInfo, "Total Weight is required.");
			AssertHasMessageError(agencyHouseBill.ErrorPlaceHolderInfo, "Total Volume is required.");
			AssertHasMessageError((agencyHouseBill.TotalVolume as Measurement)?.ValueInfo, "Total Volume is required.");
		}

		#endregion

		#endregion

		#region GetCustomField

		public void TestGetCustomField()
		{
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();

			var existingTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "SHP"));
			foreach (var existingTemplate in existingTemplates)
			{
				existingTemplate.P0_IsActive = false;
			}

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "BOL";
			var customField1 = template.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "Custom Number";
			customField1.XC_Type = AddOnColumnDataType.Codes.Integer;
			var customField2 = template.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "MY.Custom.Number";
			customField2.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var customField3 = template.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "Wow_My-So`Very-Custom+Number";
			customField3.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			var customBusinessObject = ((ICustomFieldProvider)billOfLading).GetCustomBusinessObject();
			customBusinessObject[((IDynamicBusinessObject)customBusinessObject).PropertyNames[0]] = 123;
			customBusinessObject[((IDynamicBusinessObject)customBusinessObject).PropertyNames[1]] = true;
			customBusinessObject[((IDynamicBusinessObject)customBusinessObject).PropertyNames[2]] = "abc567";
			Factory.Save();

			var agencyHouseBill = new AgencyHouseBillBuilder(billOfLading, null).Build();

			var expr = "GetCustomField(\"Custom Number\")".With<DataLibrary>().CreateExpression();
			var result = expr.Evaluate(agencyHouseBill);
			Assert("Evaluated result should be ZInt", result is ZInt);
			AssertEquals("Evaluated ZInt should be correct", 123, result);

			expr = "GetCustomField(\"MY.Custom.Number\")".With<DataLibrary>().CreateExpression();
			result = expr.Evaluate(agencyHouseBill);
			Assert("Evaluated result should be ZBool", result is ZBool);
			AssertEquals("Evaluated ZInt should be correct", true, result);

			expr = "GetCustomField(\"Wow_My-So`Very-Custom+Number\")".With<DataLibrary>().CreateExpression();
			result = expr.Evaluate(agencyHouseBill);
			Assert("Evaluated result should be ZString", result is ZString);
			AssertEquals("Evaluated ZInt should be correct", "abc567", result);

			expr = "GetCustomField(\"Wow_My-So`Very-Custom+Number\")".With<DataLibrary>().CreateExpression();
			result = expr.Evaluate(agencyHouseBill);
			Assert("Evaluated result should be ZString (2nd evaluation)", result is ZString);
			AssertEquals("Evaluated ZInt should be correct (2nd evaluation)", "abc567", result);

			expr = "GetCustomField(\"Blah Blah\")".With<DataLibrary>().CreateExpression();
			AssertNull("Evaluated result should be null when custom field does not exist", expr.Evaluate(agencyHouseBill));
		}

		public void TestGetCustomFieldWithNullCustomBusinessObject()
		{
			var agencyHouseBill = new AgencyHouseBill("BillOfLading", "V00012");
			var expr = "GetCustomField(\"Custom Number\")".With<DataLibrary>().CreateExpression();
			AssertNoExceptionThrown(() => expr.Evaluate(agencyHouseBill));
		}

		#endregion

		#region Implementation

		BillOfLading GetBillOfLading(string packingMode = ContainerModes.FCL)
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_UniqueConsignRef = "V0001";
			billOfLading.JS_HouseBill = "HOUSEBILL001";
			billOfLading.JS_PackingMode = packingMode;
			billOfLading.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "NZAKL";
			billOfLading.JS_HouseBillIssueDate = new ZDateTime(2024, 1, 1);
			billOfLading.JS_RL_NKHouseBillIssuePlace = "AUMEL";
			billOfLading.JS_HBLContainerPackModeOverride = HBLDeliveryModes.Codes.CY_CY;
			billOfLading.JS_INCO = DomesticPaymentTerms.Prepaid;
			billOfLading.JS_ShippedOnBoard = FreightConstants.ShippedOnBoardType.Shipped;
			billOfLading.JS_ShippedOnBoardDate = new ZDateTime(2024, 1, 1);
			billOfLading.JS_E_DEP = new ZDateTime(2024, 1, 2);
			billOfLading.JS_E_ARV = new ZDateTime(2024, 1, 3);
			billOfLading.JS_GoodsDescription = "goods description";
			billOfLading.JS_MarksAndNumbers = "marks & numbers";
			billOfLading.JS_BookingReference = "BKG000001";
			billOfLading.JS_HouseBillOfLadingType = "FIA";

			billOfLading.CustomsEntryNumber = "T7HRTXGXT";
			billOfLading.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var mainTransport = billOfLading.Transports.AddNew();
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = TransportModes.Sea;
			mainTransport.JW_TransportType = TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";
			mainTransport.JW_Vessel = "MAIN.Vessel";
			mainTransport.JW_VoyageFlight = "MAINVoyage";

			var otherTransport = billOfLading.Transports.AddNew();
			otherTransport.JW_LegOrder = 2;
			otherTransport.JW_TransportMode = TransportModes.Sea;
			otherTransport.JW_RL_NKLoadPort = "NZAKL";
			otherTransport.JW_RL_NKDiscPort = "NZALR";

			if (packingMode == ContainerModes.FCL)
			{
				var container1 = billOfLading.FCLContainers.AddNew();
				container1.JC_ContainerNum = "AAAA0000007";
				container1.JC_ContainerMode = ContainerModes.FCL;

				var container2 = billOfLading.FCLContainers.AddNew();
				container2.JC_ContainerNum = "BBBB0000007";
				container2.JC_ContainerMode = ContainerModes.FCL;

				billOfLading.OuterPackLines.RemoveAndDeleteAll();

				var packline1 = billOfLading.OuterPackLines.AddNew();
				packline1.JL_ActualWeight = 2000;
				packline1.JL_ActualWeightUQ = Weight.Kilograms;
				packline1.JL_ActualVolume = 1.3;
				packline1.JL_ActualVolumeUQ = Volume.CubicMetres;
				packline1.JL_JS = billOfLading.PK;
				container1.PackLines.Add(packline1);

				var packline2 = billOfLading.OuterPackLines.AddNew();
				packline2.JL_ActualWeight = 3000;
				packline2.JL_ActualWeightUQ = Weight.Kilograms;
				packline2.JL_ActualVolume = 1.3;
				packline2.JL_ActualVolumeUQ = Volume.CubicMetres;
				packline2.JL_JS = billOfLading.PK;
				container2.PackLines.Add(packline2);
			}

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			billOfLading.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "DUMMY";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "NZ";

			billOfLading.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "FUNNY";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			billOfLading.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "MANY";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			billOfLading.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "TOO MUCH";
			notifyParty3.OH_RL_NKClosestPort = "NZAKL";
			notifyParty3.MainAddress.Address1 = "Unit 686";
			notifyParty3.MainAddress.Address2 = "99 How Lane";
			notifyParty3.MainAddress.City = "Auckland";
			notifyParty3.MainAddress.Postcode = "5038";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = "NZ";

			billOfLading.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			var companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;

			billOfLading.JS_OH_DeliveryAgent = principal.PK;

			billOfLading.JS_NoCopyBills = 1;
			billOfLading.JS_NoOriginalBills = 2;
			billOfLading.JS_OuterPacks = 10;
			billOfLading.JS_F3_NKPackType = PkgUnit.Bottle;

			Factory.Save();

			return billOfLading;
		}

		void CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode, BusinessObjectFactory otherFactory = default)
		{
			var factory = otherFactory ?? Factory;

			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, header.JH_GC);

			var accChargeCode = factory.LoadTop1<AccChargeCode>(query);

			AssertNotNull($"prerequisite: charge code '{chargeCode}' was found", accChargeCode);

			var lineCharge = factory.New<JobCharge>();
			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_OH_SellAccount = sellAccountPK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = osSellAmount;
			lineCharge.JR_Desc = accChargeCode.AC_Desc;
		}

		#endregion
	}
}
