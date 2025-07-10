using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed partial class DtbConsignmentCMRConsignmentNoteBuilderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSupplierAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "SupplierAddress at this stage should be empty.");

			var pickup = Factory.New<OrgHeader>();
			pickup.OH_FullName = "PickupFromCo";
			pickup.OH_RL_NKClosestPort = "AUSYD";
			pickup.MainAddress.Address1 = "Unit 13";
			pickup.MainAddress.Address2 = "4 Lost Lane";
			pickup.MainAddress.City = "Sydney";
			pickup.MainAddress.Postcode = "2000";
			pickup.MainAddress.OA_RN_NKCountryCode = "SG";

			consignment.PickupAddress.Address.E2_OA_Address = pickup.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"PICKUPFROMCO{System.Environment.NewLine}UNIT 13{System.Environment.NewLine}4 LOST LANE{System.Environment.NewLine}SYDNEY - 2000 - SINGAPORE").Using(CustomComparers.TypeComparison), "Use pickup for the Supplier Address.");

			//override consignment's pickup address
			SetOverrideAddress(consignment.PickupAddress.Address);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "SupplierAddress when E2_AddressOverride is true");

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "MAERSK Name";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "Address 1 Unit 13";
			consignor.MainAddress.Address2 = "ADDRESS 24 Lost Lane";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.Postcode = "2229";
			consignor.MainAddress.State = "NSW";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";

			var originatingConsignorAddress = consignment.DocAddresses.AddNew(consignor.MainAddress, DocAddressType.OriginatingConsignorAddress);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"MAERSK NAME{System.Environment.NewLine}ADDRESS 1 UNIT 13{System.Environment.NewLine}ADDRESS 24 LOST LANE{System.Environment.NewLine}SYDNEY - 2229 - AUSTRALIA").Using(CustomComparers.TypeComparison), "Original consignor has priority over pickup.");

			SetOverrideAddress(originatingConsignorAddress);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SupplierAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "SupplierAddress when E2_AddressOverride is true");
		}

		[ExpectNoExceptions]
		public void TestImporterAddress()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "ImporterAddress at this stage should be empty.");

			var delivery = Factory.New<OrgHeader>();
			delivery.OH_FullName = "DeliveryToCo";
			delivery.OH_RL_NKClosestPort = "AUSYD";
			delivery.MainAddress.Address1 = "Unit 13";
			delivery.MainAddress.Address2 = "4 Lost Lane";
			delivery.MainAddress.City = "Sydney";
			delivery.MainAddress.Postcode = "2000";
			delivery.MainAddress.OA_RN_NKCountryCode = "SG";
			consignment.Addresses.AddNew(InstructionTypes.Codes.Delivery);
			consignment.DeliveryAddress.Address.E2_OA_Address = delivery.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"DELIVERYTOCO{System.Environment.NewLine}UNIT 13{System.Environment.NewLine}4 LOST LANE{System.Environment.NewLine}SYDNEY - 2000 - SINGAPORE").Using(CustomComparers.TypeComparison), "Use delivery for the Import Address.");

			//override consignment's delivery address
			SetOverrideAddress(consignment.DeliveryAddress.Address);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "ImporterAddress when E2_AddressOverride is true");

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MAERSK";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.MainAddress.Address1 = "Unit 13";
			consignee.MainAddress.Address2 = "4 Lost Lane";
			consignee.MainAddress.City = "Sydney";
			consignee.MainAddress.Postcode = "2229";
			consignee.MainAddress.State = "NSW";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			var finalConsigneeAddress = consignment.DocAddresses.AddNew(consignee.MainAddress, DocAddressType.FinalConsigneeAddress);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"MAERSK{System.Environment.NewLine}UNIT 13{System.Environment.NewLine}4 LOST LANE{System.Environment.NewLine}SYDNEY - 2229 - AUSTRALIA").Using(CustomComparers.TypeComparison), "Final consignee has priority over delivery.");

			//override consignment's final consignee address
			SetOverrideAddress(finalConsigneeAddress);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo($"DHL{System.Environment.NewLine}UNIT 13-1{System.Environment.NewLine}4 LOST LANE-1{System.Environment.NewLine}BRISBANE - 1234 - HONG KONG").Using(CustomComparers.TypeComparison), "ImporterAddress when E2_AddressOverride is true");
		}

		[ExpectNoExceptions]
		public void TestInternationalConsignmentNote()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.InternationalConsignmentNote, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "InternationalConsignmentNote should be empty.");
		}

		[ExpectNoExceptions]
		public void TestPlaceOfDelivery()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "The place of delivery at this stage should be empty.");

			var delivery = Factory.New<OrgHeader>();
			delivery.OH_FullName = "MAERSK";
			delivery.OH_RL_NKClosestPort = "AUSYD";
			delivery.MainAddress.Address1 = "Unit 13";
			delivery.MainAddress.Address2 = "4 Lost Lane";
			delivery.MainAddress.City = "Sydney";
			delivery.MainAddress.Postcode = "2229";
			delivery.MainAddress.State = "NSW";
			delivery.MainAddress.OA_RN_NKCountryCode = "AU";

			consignment.Addresses.AddNew(InstructionTypes.Codes.Delivery).LTS_Sequence = 2;
			consignment.DeliveryAddress.Address.E2_OA_Address = delivery.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("2229 SYDNEY AU").Using(CustomComparers.TypeComparison), "The place of delivery must be filled in with delivery address.");

			//override consignment's delivery address
			SetOverrideAddress(consignment.DeliveryAddress.Address);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.PlaceOfDelivery, Is.EqualTo("1234 BRISBANE HK").Using(CustomComparers.TypeComparison), "The place of delivery must include the delivery address when E2_AddressOverride is true.");
		}

		[ExpectNoExceptions]
		public void TestGoodsTakingOverPlaceAndDate()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "When no address is provided, CityCountryDateOfGoodsTakingOver will be empty.");

			var pickup = Factory.New<OrgHeader>();
			pickup.OH_FullName = "MAERSK";
			pickup.OH_RL_NKClosestPort = "AUSYD";
			pickup.MainAddress.Address1 = "Unit 13";
			pickup.MainAddress.Address2 = "4 Lost Lane";
			pickup.MainAddress.City = "Sydney";
			pickup.MainAddress.Postcode = "2229";
			pickup.MainAddress.State = "NSW";
			pickup.MainAddress.OA_RN_NKCountryCode = "AU";

			consignment.PickupAddress.Address.E2_OA_Address = pickup.MainAddress.PK;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("SYDNEY AU").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled in with pickup address.");

			//override consignment's pickup address
			SetOverrideAddress(consignment.PickupAddress.Address);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("BRISBANE HK").Using(CustomComparers.TypeComparison), "The place of delivery must include the delivery address when E2_AddressOverride is true.");

			var today = ZDateTimeOffset.Today;
			var todayAsString = today.ToString("dd/MM/yyyy");

			consignment.PickupAddress.ReqFrom = today;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("BRISBANE HK").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled in with the pickup address and pickup required from when there is no consignments actions.");

			//unset the override address
			consignment.PickupAddress.Address.E2_AddressOverride = false;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("SYDNEY AU").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled in with the consignor's address when there is no Pickup From address, even if there is a JP_EstimatedPickup and E2_AddressOverride is false.");

			var action1 = consignment.PickupAddress.Actions.AddNew();
			action1.LTA_EstimatedTime = ZDateTimeOffset.Empty;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo("SYDNEY AU").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled in with the pickup address and pickup required from when there is Consignments actions with empty estimated time.");

			action1.LTA_EstimatedTime = today.AddDays(-1);

			var action2 = consignment.PickupAddress.Actions.AddNew();
			var twoDaysAgo = action2.LTA_EstimatedTime = today.AddDays(-2);
			var twoDaysAgoAsString = twoDaysAgo.ToString("dd/MM/yyyy");

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo($"SYDNEY AU {twoDaysAgoAsString}").Using(CustomComparers.TypeComparison), "CityCountryDateOfGoodsTakingOver must be filled in with the pickup address and the earliest estimated time.");

			//override consignment's pickup address
			SetOverrideAddress(consignment.PickupAddress.Address);

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CityCountryDateOfGoodsTakingOver, Is.EqualTo($"BRISBANE HK {twoDaysAgoAsString}").Using(CustomComparers.TypeComparison), "When Pickup From and JP_EstimatedPickup are provided, they will be available in CityCountryDateOfGoodsTakingOver when E2_AddressOverride is true.");
		}

		[ExpectNoExceptions]
		public void TestGoodsAttachedDocuments()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.GoodsAttachedDocuments, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "GoodsAttachedDocuments is empty");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsBox6_7_8_9()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 at this stage should be empty.");

			var packLine1 = consignment.PackageJob.Packages.AddNew();
			packLine1.KP_PackageQty = 10;
			packLine1.KP_F3_NKPackType = PkgUnit.Package;
			packLine1.KP_MarksAndNumbers = "MN1";
			packLine1.KP_PackageID = "Pkg1";
			packLine1.KP_GoodsDescription = "THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE --->THIS PART OF THE STRING WILL NOT BE SHOWN";

			var packLine2 = consignment.PackageJob.Packages.AddNew();
			packLine2.KP_PackageQty = 12;
			packLine2.KP_F3_NKPackType = PkgUnit.Skid;
			packLine2.KP_MarksAndNumbers = ZString.Empty;
			packLine2.KP_PackageID = "Pkg2";
			packLine2.KP_GoodsDescription = "Description2";

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			var expectedLineDescription = $"MN1; 10 PKG; THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE ---{System.Environment.NewLine}Pkg2; 12 SKD; Description2";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsBox6_7_8_9, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsBox6_7_8_9 provides details from the pack lines.");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsTariffCodeBox10()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsTariffCodeBox10, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "LineDetailsTariffCodeBox10 should be empty");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsGrossWeightInKGBox11()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 at this stage should be empty.");

			var packLine1 = consignment.PackageJob.Packages.AddNew();
			packLine1.KP_Weight = 10;
			packLine1.KP_WeightUQ = Weight.Kilograms;

			var packLine2 = consignment.PackageJob.Packages.AddNew();
			packLine2.KP_Weight = 100;
			packLine2.KP_WeightUQ = Weight.Grams;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			var expectedLineDescription = $"10.00    {System.Environment.NewLine}0.1     ";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsGrossWeightInKGBox11, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsGrossWeightInKGBox11 provides details from the pack lines in Kilograms (KG).");
		}

		[ExpectNoExceptions]
		public void TestLineDetailsVolumeInM3Box12()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.ImporterAddress, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3Box12 at this stage should be empty.");

			var packLine1 = consignment.PackageJob.Packages.AddNew();
			packLine1.KP_Volume = 2;
			packLine1.KP_VolumeUQ = Volume.CubicMetres;

			var packLine2 = consignment.PackageJob.Packages.AddNew();
			packLine2.KP_Volume = 100;
			packLine2.KP_VolumeUQ = Volume.Litre;

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			var expectedLineDescription = $"2.00    {System.Environment.NewLine}0.1     ";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.LineDetailsVolumeInM3Box12, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "LineDetailsVolumeInM3Box12 provides details from the pack lines in cubic meters (M3).");
		}

		[ExpectNoExceptions]
		public void TestIncotermAndTextBox14()
		{
			consignment.LTC_Incoterm = ZString.Empty;
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo(ZString.Empty), "When no Incoterm is provided, an empty string is expected.");

			consignment.LTC_Incoterm = IncoTerms.ExWorks;
			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.IncotermAndTextBox14, Is.EqualTo("EXW - Ex Works").Using(CustomComparers.TypeComparison), "When an Incoterm is provided, it must be available in the code-description.");
		}

		[ExpectNoExceptions]
		public void TestSendersInstructions()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SendersInstructions, Is.EqualTo(ZString.Empty), "When no pickup notes or DangerousGoodsAdditionalHandlingInformation is provided, an empty string is expected.");

			consignment.PickupAddress.LTS_Notes = "We have Pickup Instructions";
			consignment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "We have Dangerous Goods Additional Handling Information");

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			var expectedLineDescription = $"WE HAVE PICKUP INSTRUCTIONS{System.Environment.NewLine}WE HAVE DANGEROUS GOODS ADDITIONAL HANDLING INFORM";

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SendersInstructions, Is.EqualTo(expectedLineDescription).Using(CustomComparers.TypeComparison), "When pickup notes or Dangerous Goods Additional Handling Information is provided, it must be available.");
		}

		[ExpectNoExceptions]
		public void TestCarrierAddress()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "HK Company";
			company.GC_RN_NKCountryCode = "HK";

			var newBranch = company.Branches.AddNew();
			newBranch.GB_Code = company.GC_Code;
			newBranch.GB_BranchName = "HK Branch";
			newBranch.GB_RL_NKHomePort = "HKHKG";
			newBranch.Address1 = "Address 1";
			newBranch.Address2 = "Address 2";
			newBranch.City = "Kowloon";
			newBranch.Postcode = "2229";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
				NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.CarrierAddress, Is.EqualTo($"HK COMPANY{System.Environment.NewLine}ADDRESS 1{System.Environment.NewLine}ADDRESS 2{System.Environment.NewLine}KOWLOON - 2229 - HONG KONG").Using(CustomComparers.TypeComparison), "Carrier Address");
			}
		}

		[ExpectNoExceptions]
		public void TestSpecialAgreements()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.SpecialAgreements, Is.EqualTo(ZString.Empty), "SpecialAgreements should be empty");
		}

		[ExpectNoExceptions]
		public void TestJobNumber()
		{
			consignment.LTC_JobID = "CN00000019";
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.JobNumber, Is.EqualTo("CN00000019").Using(CustomComparers.TypeComparison), "Job Number should be the consignment number.");
		}

		[ExpectNoExceptions]
		[TestDate(2025, 05, 04)]
		public void TestEstablishedInDate()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.EstablishedInDate, Is.EqualTo("04 May 2025").Using(CustomComparers.TypeComparison), "EstablishedInDate should be in the format dd MMM yyyy.");
		}

		[ExpectNoExceptions]
		public void TestEstablishedInPlace()
		{
			var currentBranchPort = ZString.Empty;
			try
			{
				currentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "SGSIN";
				var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);

				NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.EstablishedInPlace, Is.EqualTo("Singapore").Using(CustomComparers.TypeComparison), "EstablishedInPlace should be the current branch's home port.");
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchPort;
			}
		}

		[ExpectNoExceptions]
		public void TestDangerousGoods()
		{
			var cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsClass is empty.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsNumber is empty.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter is empty.");

			var packLine1 = consignment.PackageJob.Packages.AddNew();
			packLine1.KP_PackageQty = 10;
			packLine1.KP_F3_NKPackType = PkgUnit.Package;
			packLine1.KP_MarksAndNumbers = "MN1";
			packLine1.KP_GoodsDescription = "THIS IS THE DESCRIPTION FOR ENTRY LINE 1 WHICH WILL BE CUT HERE -->THIS PART OF THE STRING WILL NOT BE SHOWN";
			packLine1.KP_PackageID = "Pkg1";

			var packLine2 = consignment.PackageJob.Packages.AddNew();
			packLine2.KP_PackageQty = 12;
			packLine2.KP_F3_NKPackType = PkgUnit.Skid;
			packLine2.KP_MarksAndNumbers = "MN2";
			packLine2.KP_GoodsDescription = "Description2";
			packLine2.KP_PackageID = "Pkg2";

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no dangerous goods, DangerousGoodsClass is empty.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no dangerous goods, DangerousGoodsNumber is empty.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "There is no dangerous goods, DangerousGoodsLetter is empty.");

			var dg = packLine2.UNDGs.AddNew();
			dg.UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstanceLoader.LoadSubstances(Factory, "0113", "", "IMO").First());
			dg.DI_IMOClass = "1.1A";

			cmrConsignmentNoteDocDataObject = GetConsignmentCMRConsignmentNoteDocData(consignment);

			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsClass, Is.EqualTo("1.1A").Using(CustomComparers.TypeComparison), "DangerousGoodsClass value comes from pack line.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsNumber, Is.EqualTo("0113").Using(CustomComparers.TypeComparison), "DangerousGoodsNumber value comes from pack line.");
			NUnit.Framework.Assert.That(cmrConsignmentNoteDocDataObject.DangerousGoodsLetter, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "DangerousGoodsLetter is empty.");
		}

		protected override void SetUp()
		{
			base.SetUp();

			consignment = Factory.NewWithValidTestData<DtbConsignment>();
		}

		CMRConsignmentNoteDocDataObject GetConsignmentCMRConsignmentNoteDocData(DtbConsignment consignment)
		{
			var consignmentCMRConsignmentNoteBuilder = new DtbConsignmentCMRConsignmentNoteBuilder(consignment);
			return consignmentCMRConsignmentNoteBuilder.Build().CMRConsignmentNoteDocuments.First();
		}

		void SetOverrideAddress(JobDocAddress jobDocAddress)
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "DHL";
			jobDocAddress.E2_Postcode = "1234";
			jobDocAddress.E2_City = "Brisbane";
			jobDocAddress.E2_RN_NKCountryCode = "HK";
			jobDocAddress.E2_Address1 = "Unit 13-1";
			jobDocAddress.E2_Address2 = "4 Lost Lane-1";
		}

		DtbConsignment consignment;
	}
}
