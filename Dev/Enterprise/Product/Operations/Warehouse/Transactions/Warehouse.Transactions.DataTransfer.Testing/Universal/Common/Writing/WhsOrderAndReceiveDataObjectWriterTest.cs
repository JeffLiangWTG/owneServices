using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsOrderAndReceiveDataObjectWriterTest<TDocket, TWriter> : WhsDocketDataObjectWriterTest<TDocket, TWriter>
		where TDocket : WhsDocket, IJobWithTransportCompany
		where TWriter : WhsOrderAndReceiveDataObjectWriter<TDocket>
	{
		#region TestCarrierServiceLevel

		public void TestCarrierServiceLevel()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "ABC";
			serviceLevel.PL_CarrierServiceLevelDescription = "service level description";
			serviceLevel.PL_CarrierServiceCode = "1234";
			serviceLevel.PL_ChargeCode = "1212121212";
			serviceLevel.PL_ProductCode = "555555";
			serviceLevel.PL_APProfileID = "AP3333";
			serviceLevel.PL_IsSignatureRequired = true;

			var whsDocketBO = GetNewDocket();
			whsDocketBO.TransportCoPK = carrier.PK;
			whsDocketBO.WD_PL_NKCarrierServiceLevel = "ABC";

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals("serviceLevelDataObject.Code", "ABC", whsDocketData.CarrierServiceLevel.Code);
			AssertEquals("serviceLevelDataObject.Description", "service level description", whsDocketData.CarrierServiceLevel.Description);
			AssertEquals("serviceLevelDataObject.CarrierServiceCode", "1234", whsDocketData.CarrierServiceLevel.CarrierServiceCode);
			AssertEquals("serviceLevelDataObject.CarrierProductCode", "555555", whsDocketData.CarrierServiceLevel.CarrierProductCode);
			AssertEquals("serviceLevelDataObject.CarrierChargeCode", "1212121212", whsDocketData.CarrierServiceLevel.CarrierChargeCode);
			AssertEquals("serviceLevelDataObject.CarrierProfileID", "AP3333", whsDocketData.CarrierServiceLevel.CarrierProfileID);
			AssertEquals("whsDocketData.IsSignatureRequired", true, whsDocketData.IsSignatureRequired);
		}

		#endregion

		#region TestScreeningStatus

		public void TestGetDataObject_WhenScreeningStatusSet()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);

			AssertEquals("whsOrderData.ScreeningStatus.Code", ScreeningStatusesList.Codes.Matched, whsDocketData.ScreeningStatus.Code);
			AssertEquals("whsOrderData.ScreeningStatus.Description", ScreeningStatusesList.Descriptions.Matched, whsDocketData.ScreeningStatus.Description);
		}

		#endregion

		#region TestCollections

		#region TestAdditionalReferences

		public void TestAdditionalReferences()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.WD_BOLNo = "BILL";

			var reference = whsDocketBO.References.AddNew();
			reference.WX_RefType = "MAR";
			reference.WX_Reference = "100001133";

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals("whsDocketData.AdditionalReferenceCollection.Count", 2, whsDocketData.AdditionalReferenceCollection.Count);

			CombineAssertions(delegate
			{
				var reference1 = whsDocketData.AdditionalReferenceCollection[0];
				AssertEquals("reference1.Type.Code", "HSB", reference1.Type.Code);
				AssertEquals("reference1.Type.Description", "House Bill", reference1.Type.Description);
				AssertEquals("reference1.ReferenceNumber", "BILL", reference1.ReferenceNumber);

				var reference2 = whsDocketData.AdditionalReferenceCollection[1];
				AssertEquals("reference2.Type.Code", "MAR", reference2.Type.Code);
				AssertEquals("reference2.Type.Description", "Marks and Numbers", reference2.Type.Description);
				AssertEquals("reference2.ReferenceNumber", "100001133", reference2.ReferenceNumber);
			});
		}

		#endregion

		#region TestServices

		public void TestServices()
		{
			var whsDocketBO = GetNewDocket();
			var service1 = whsDocketBO.Services.AddNew();
			service1.ES_Booked = new ZDateTime(2011, 1, 1);
			service1.ES_ServiceCode = "TAI";
			service1.ES_Completed = new ZDateTime(2011, 1, 2);
			service1.ES_ServiceCount = 4.3m;
			service1.ES_OH_Contractor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			service1.ES_Duration = new ZDateTime(2011, 1, 3);
			service1.ES_ServiceNote = "NOTETHIS";
			service1.ES_References = "REFREF";
			service1.ES_OA_Location = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress.PK;

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals("whsDocketData.LocalProcessing.AdditionalServiceCollection.Count", 1, whsDocketData.LocalProcessing.AdditionalServiceCollection.Count);

			var serviceDataObject = whsDocketData.LocalProcessing.AdditionalServiceCollection[0];
			CombineAssertions(delegate
			{
				AssertEquals("serviceDataObject.Booked", new ZDateTime(2011, 1, 1), serviceDataObject.Booked);
				AssertEquals("serviceDataObject.Completed", new ZDateTime(2011, 1, 2), serviceDataObject.Completed);
				AssertEquals("serviceDataObject.Duration", (ZDateTime)TimeSpan.FromDays(2), serviceDataObject.Duration);
				AssertEquals("serviceDataObject.References", "REFREF", serviceDataObject.References);
				AssertEquals("serviceDataObject.ServiceCode.Code", "TAI", serviceDataObject.ServiceCode.Code);
				AssertEquals("serviceDataObject.ServiceCode.Description", "Tailgate", serviceDataObject.ServiceCode.Description);
				AssertEquals("serviceDataObject.ServiceCount", 4.3m, serviceDataObject.ServiceCount);
				AssertEquals("serviceDataObject.ServiceNote", "NOTETHIS", serviceDataObject.ServiceNote);
			});

			AssertOrganizationBO_WUFSHIJNB("additionalService.Contractor", serviceDataObject.Contractor, "Contractor");
			AssertOrganizationBO_CRAHOLSYD("additionalService.Location", serviceDataObject.Location, "Location");
		}

		#endregion

		#region TestContainers

		public void TestContainers()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.Containers.Add(WhsDocketContainerDataObjectWriterTest.GetContainer(Factory.BOFactory));
			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals("whsDocketData.Order.ContainerCollection.Count", 1, whsDocketData.ContainerCollection.Count);

			var containerData = whsDocketData.ContainerCollection[0];
			CombineAssertions(delegate
			{
				WhsDocketContainerDataObjectWriterTest.AssertContents(containerData);
			});
		}

		#endregion

		#region TestTotalOrderValue

		public void TestTotalOrderValueAndCurr()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.WD_TotalOrderValue = 10m;
			whsDocketBO.WD_RX_NKTotalOrderCurrency = "USD";
			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals(10m, whsDocketData.GoodsValue);
			AssertEquals(" whsDocketData.TotalOrderCurr.Code", "USD", whsDocketData.GoodsValueCurrency.Code);
		}

		#endregion

		#region TestOrganisations

		public void TestOrganisations()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.WD_WW_Whs = ZGuid.Empty;
			whsDocketBO.TransportCoDocAddress.E2_OA_Address = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;
			whsDocketBO.WD_OH_Client = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;
			var header = new JobHeader.Loader(whsDocketBO).TryLoadOrCreate();
			header.JH_OA_LocalChargesAddr = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);

			AssertEquals("whsDocketData.OrganizationAddressCollection.Count", 3, whsDocketData.OrganizationAddressCollection.Count);

			var consignorAddress = whsDocketData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "ConsignorDocumentaryAddress");
			var sendersLocalClientAddress = whsDocketData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "SendersLocalClient");
			var transportCoAddress = whsDocketData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "TransportCompanyDocumentaryAddress");
			AssertOrganizationBO_INTHEMSYD("ConsignorDocumentaryAddress", consignorAddress, "ConsignorDocumentaryAddress");
			AssertOrganizationBO_WUFSHIJNB("SendersLocalClient", sendersLocalClientAddress, "SendersLocalClient");
			AssertOrganizationBO_WUFSHIJNB("TransportCompanyDocumentaryAddress", transportCoAddress, "TransportCompanyDocumentaryAddress", true);
		}

		#endregion

		#region TestTotalVolumeAndWeight

		public void TestTotalVolumeAndWeight()
		{
			var whsDocketBO = GetNewDocket();
			var line = whsDocketBO.Lines.AddNew();
			line.WE_WD = whsDocketBO.PK;

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals(null, whsDocketData.OuterPacks);

			whsDocketBO.WD_TotalCubic = 1m;
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals(1m, whsDocketData.TotalVolume);

			whsDocketBO.WD_CubicSent = 2m;
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals(2m, whsDocketData.TotalVolume);

			whsDocketBO.WD_TotalWeight = 3m;
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals(3m, whsDocketData.TotalWeight);

			whsDocketBO.WD_WeightSent = 4m;
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals(4m, whsDocketData.TotalWeight);
		}

		#endregion

		#region TestOuterPacks

		public void TestOuterPacks()
		{
			var whsDocketBO = GetNewDocket();
			var line = whsDocketBO.Lines.AddNew();
			line.WE_WD = whsDocketBO.PK;

			line.WE_TransactionQuantity = 1;
			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals(1, whsDocketData.OuterPacks);

			whsDocketBO.WD_TotalUnits = 2;
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals(2, whsDocketData.OuterPacks);

			whsDocketBO.WD_PackagesSent = 3;
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals(3, whsDocketData.OuterPacks);

			SetPalletsSent(whsDocketBO, 4);
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals(4, whsDocketData.OuterPacks);
		}

		public void TestOuterPacks_TotalUnitsExceedsMaximumIntQuantity()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.WD_TotalUnits = int.MaxValue + 5000m;

			var notifications = new TestNotificationBuffer();
			var whsDocketData = GetNewDataObjectWriter(whsDocketBO, notifications).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals("whsDocketData.OuterPacks", null, whsDocketData.OuterPacks);

			var notification = notifications.LastEvent;
			AssertEquals(NotificationType.Warning, notification.Type);
			AssertEquals($"Unable to convert WD_TotalUnits to an integer, as it has value {whsDocketBO.WD_TotalUnits}.", notification.Message);
		}

		public void TestOuterPacks_TotalOfLinesQuantitiesExceedsMaximumIntQuantity()
		{
			var whsDocketBO = GetNewDocket();

			var qtyPerLine = int.MaxValue / 5;
			for (var i = 0; i < 10; i++)
			{
				var line = whsDocketBO.Lines.AddNew();
				line.WE_WD = whsDocketBO.PK;
				line.WE_TransactionQuantity = qtyPerLine;
			}

			var totalQty = whsDocketBO.Lines.Sum(l => l.WE_TransactionQuantity);
			Assert("Precondition", totalQty > int.MaxValue);

			var notifications = new TestNotificationBuffer();
			var whsDocketData = GetNewDataObjectWriter(whsDocketBO, notifications).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals("whsDocketData.OuterPacks", null, whsDocketData.OuterPacks);

			var notification = notifications.LastEvent;
			AssertEquals(NotificationType.Warning, notification.Type);
			AssertEquals($"Unable to convert WE_TransactionQuantity to an integer, as it has value {totalQty}.", notification.Message);
		}

		#endregion

		#region TestOuterPacksPackageType

		public void TestOuterPacksPackageType()
		{
			var whsDocketBO = GetNewDocket();
			var packtype = whsDocketBO.Lookups.TotalPackTypes.AddNew();
			whsDocketBO.WD_PackagesSent = 1;
			packtype.F3_Code = "T1";
			packtype.F3_Description = "D1";
			whsDocketBO.WD_F3_NKTotalPackType = "T1";

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals("whsOrderData.OuterPacksPackageType.Code", "T1", whsDocketData.OuterPacksPackageType.Code);
			AssertEquals("whsOrderData.OuterPacksPackageType.Description", "D1", whsDocketData.OuterPacksPackageType.Description);

			whsDocketBO.WD_PackagesSent = 0;
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals("whsOrderData.OuterPacksPackageType.Code", Constants.PkgUnit.Piece, whsDocketData.OuterPacksPackageType.Code);
			AssertEquals("whsOrderData.OuterPacksPackageType.Description", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Piece), whsDocketData.OuterPacksPackageType.Description);

			SetPalletsSent(whsDocketBO, 1);
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals("whsOrderData.OuterPacksPackageType.Code", Constants.PkgUnit.Pallet, whsDocketData.OuterPacksPackageType.Code);
			AssertEquals("whsOrderData.OuterPacksPackageType.Description", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Pallet), whsDocketData.OuterPacksPackageType.Description);
		}

		#endregion

		#region TestOuterPacksPackageType_Default

		public void TestOuterPacksPackageType_Default()
		{
			var whsDocketBO = GetNewDocket();
			var line = whsDocketBO.Lines.AddNew();
			line.WE_WD = whsDocketBO.PK;
			line.WE_TransactionQuantity = 1;
			whsDocketBO.WD_F3_NKTotalPackType = Constants.PkgUnit.Package;
			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals("Pack type should not change", Constants.PkgUnit.Package, whsDocketData.OuterPacksPackageType.Code);

			whsDocketBO.WD_F3_NKTotalPackType = "";
			whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);
			AssertEquals("Pack type should be set to Piece change when is empty", Constants.PkgUnit.Piece, whsDocketData.OuterPacksPackageType.Code);
		}

		#endregion

		#endregion

		#region JobCosting

		#region TestGenerateChargeLines

		public void TestGenerateChargeLines()
		{
			var whsDocketBO = GetNewDocket();
			var job = Factory.BOFactory.NewJobForTesting<Job>();
			job.JH_ParentID = whsDocketBO.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_JobNum = "TESTJOB1";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var chargeGroupList = new ChargeCodeGroupList();

			var jobCharge1 = job.Charges.AddNew();
			jobCharge1.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge1.CostCurrency.RX_Code = "AUD";
			jobCharge1.CostCurrency.RX_Desc = "Australia, Dollars";
			jobCharge1.JR_OSCostAmt = 200m;
			jobCharge1.JR_LocalCostAmt = 200m;
			jobCharge1.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			jobCharge1.SellCurrency.RX_Code = "AUD";
			jobCharge1.SellCurrency.RX_Desc = "Australia, Dollars";
			jobCharge1.JR_OSSellAmt = 200m;
			jobCharge1.JR_LocalSellAmt = 200m;
			jobCharge1.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			jobCharge1.JR_InvoiceType = "FID";
			jobCharge1.JR_DisplaySequence = 1;
			jobCharge1.SellAccount.CompanyData.OB_ARExternalDebtorCode = "AAA";
			jobCharge1.CostAccount.CompanyData.OB_APExternalCreditorCode = "BBB";
			jobCharge1.ChargeCode.AC_ChargeGroup = "BRK";
			var description = chargeGroupList.GetDescriptionFromCode("BRK");

			var jobCharge2 = job.Charges.AddNew();
			jobCharge2.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge2.CostCurrency.RX_Code = "AUD";
			jobCharge2.CostCurrency.RX_Desc = "Australia, Dollars";
			jobCharge2.SellCurrency.RX_Code = "AUD";
			jobCharge2.SellCurrency.RX_Desc = "Australia, Dollars";
			jobCharge2.JR_LocalCostAmt = 100m;
			jobCharge2.JR_OSCostAmt = 100m;
			jobCharge2.JR_LocalSellAmt = 100m;
			jobCharge2.JR_OSSellAmt = 100m;
			jobCharge2.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			jobCharge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			jobCharge2.JR_InvoiceType = "FIN";
			jobCharge2.JR_DisplaySequence = 2;
			jobCharge2.SellAccount.CompanyData.OB_ARExternalDebtorCode = "CCC";
			jobCharge2.CostAccount.CompanyData.OB_APExternalCreditorCode = "DDD";
			jobCharge2.ChargeCode.AC_ChargeGroup = "BON";
			description = chargeGroupList.GetDescriptionFromCode("BON");

			Factory.SaveForTesting();

			var writer = GetNewDataObjectWriter(whsDocketBO);
			var whsDocketData = writer.GetDataObject(whsDocketBO);
			var loadedJob = whsDocketData.JobCosting;

			AssertEquals("[Charge Line 1] Branch Code", jobCharge1.Branch.GB_Code, loadedJob.ChargeLineCollection[0].Branch.Code);
			AssertEquals("[Charge Line 1] Branch Name", jobCharge1.Branch.GB_BranchName, loadedJob.ChargeLineCollection[0].Branch.Name);
			AssertEquals("[Charge Line 1] CostOSCurrency Code", jobCharge1.CostCurrency.RX_Code, loadedJob.ChargeLineCollection[0].CostOSCurrency.Code);
			AssertEquals("[Charge Line 1] CostOSCurrency Description", jobCharge1.CostCurrency.RX_Desc, loadedJob.ChargeLineCollection[0].CostOSCurrency.Description);
			AssertEquals("[Charge Line 1] Department Code", jobCharge1.Department.GE_Code, loadedJob.ChargeLineCollection[0].Department.Code);
			AssertEquals("[Charge Line 1] Department Name", jobCharge1.Department.GE_Desc, loadedJob.ChargeLineCollection[0].Department.Name);
			AssertEquals("[Charge Line 1] SellOSCurrency Code", jobCharge1.SellCurrency.RX_Code, loadedJob.ChargeLineCollection[0].SellOSCurrency.Code);
			AssertEquals("[Charge Line 1] SellOSCurrency Description", jobCharge1.SellCurrency.RX_Desc, loadedJob.ChargeLineCollection[0].SellOSCurrency.Description);
			AssertEquals("[Charge Line 1] ChargeCode Code", jobCharge1.ChargeCode.AC_Code, loadedJob.ChargeLineCollection[0].ChargeCode.Code);
			AssertEquals("[Charge Line 1] ChargeCode Description", jobCharge1.ChargeCode.AC_Desc, loadedJob.ChargeLineCollection[0].ChargeCode.Description);
			AssertEquals("[Charge Line 1] ChargeCodeGroup Code", jobCharge1.ChargeCode.AC_ChargeGroup, loadedJob.ChargeLineCollection[0].ChargeCodeGroup.Code);
			AssertEquals("[Charge Line 1] ChargeCodeGroup Description", description, loadedJob.ChargeLineCollection[0].ChargeCodeGroup.Description);
			AssertEquals("[Charge Line 1] CostLocalAmount", jobCharge1.JR_LocalCostAmt, loadedJob.ChargeLineCollection[0].CostLocalAmount);
			AssertEquals("[Charge Line 1] CostOSAmount", jobCharge1.JR_OSCostAmt, loadedJob.ChargeLineCollection[0].CostOSAmount);
			AssertEquals("[Charge Line 1] Debtor Type", nameof(DataContextType.Organization), loadedJob.ChargeLineCollection[0].Debtor.Type);
			AssertEquals("[Charge Line 1] Debtor Key", jobCharge1.SellAccount.OH_Code, loadedJob.ChargeLineCollection[0].Debtor.Key);
			AssertEquals("[Charge Line 1] SellInvoiceType", jobCharge1.JR_InvoiceType, loadedJob.ChargeLineCollection[0].SellInvoiceType);
			AssertEquals("[Charge Line 1] SellLocalAmount", jobCharge1.JR_LocalSellAmt, loadedJob.ChargeLineCollection[0].SellLocalAmount);
			AssertEquals("[Charge Line 1] SellOSAmount", jobCharge1.JR_OSSellAmt, loadedJob.ChargeLineCollection[0].SellOSAmount);
			AssertEquals("[Charge Line 1] InvoiceType", jobCharge1.JR_InvoiceType, loadedJob.ChargeLineCollection[0].SellInvoiceType);
			AssertEquals("[Charge Line 1] DisplaySequence", jobCharge1.JR_DisplaySequence, loadedJob.ChargeLineCollection[0].DisplaySequence);
			AssertEquals("[Charge Line 1] ExternalDebtorCode", jobCharge1.SellAccount.CompanyData.OB_ARExternalDebtorCode, loadedJob.ChargeLineCollection[0].ExternalDebtorCode);
			AssertEquals("[Charge Line 1] ExternalCreditorCode", jobCharge1.CostAccount.CompanyData.OB_APExternalCreditorCode, loadedJob.ChargeLineCollection[0].ExternalCreditorCode);

			AssertEquals("[Charge Line 2] Branch Code", jobCharge2.Branch.GB_Code, loadedJob.ChargeLineCollection[1].Branch.Code);
			AssertEquals("[Charge Line 2] Branch Name", jobCharge2.Branch.GB_BranchName, loadedJob.ChargeLineCollection[1].Branch.Name);
			AssertEquals("[Charge Line 2] CostOSCurrency Code", jobCharge2.CostCurrency.RX_Code, loadedJob.ChargeLineCollection[1].CostOSCurrency.Code);
			AssertEquals("[Charge Line 2] CostOSCurrency Description", jobCharge2.CostCurrency.RX_Desc, loadedJob.ChargeLineCollection[1].CostOSCurrency.Description);
			AssertEquals("[Charge Line 2] Department Code", jobCharge2.Department.GE_Code, loadedJob.ChargeLineCollection[1].Department.Code);
			AssertEquals("[Charge Line 2] Department Name", jobCharge2.Department.GE_Desc, loadedJob.ChargeLineCollection[1].Department.Name);
			AssertEquals("[Charge Line 2] SellOSCurrency Code", jobCharge2.SellCurrency.RX_Code, loadedJob.ChargeLineCollection[1].SellOSCurrency.Code);
			AssertEquals("[Charge Line 2] SellOSCurrency Description", jobCharge2.SellCurrency.RX_Desc, loadedJob.ChargeLineCollection[1].SellOSCurrency.Description);
			AssertEquals("[Charge Line 2] ChargeCode Code", jobCharge2.ChargeCode.AC_Code, loadedJob.ChargeLineCollection[1].ChargeCode.Code);
			AssertEquals("[Charge Line 2] ChargeCode Description", jobCharge2.ChargeCode.AC_Desc, loadedJob.ChargeLineCollection[1].ChargeCode.Description);
			AssertEquals("[Charge Line 2] ChargeCodeGroup Code", jobCharge2.ChargeCode.AC_ChargeGroup, loadedJob.ChargeLineCollection[1].ChargeCodeGroup.Code);
			AssertEquals("[Charge Line 2] ChargeCodeGroup Description", description, loadedJob.ChargeLineCollection[1].ChargeCodeGroup.Description);
			AssertEquals("[Charge Line 2] CostLocalAmount", jobCharge2.JR_LocalCostAmt, loadedJob.ChargeLineCollection[1].CostLocalAmount);
			AssertEquals("[Charge Line 2] CostOSAmount", jobCharge2.JR_OSCostAmt, loadedJob.ChargeLineCollection[1].CostOSAmount);
			AssertEquals("[Charge Line 2] Debtor Type", nameof(DataContextType.Organization), loadedJob.ChargeLineCollection[1].Debtor.Type);
			AssertEquals("[Charge Line 2] Debtor Key", jobCharge2.SellAccount.OH_Code, loadedJob.ChargeLineCollection[1].Debtor.Key);
			AssertEquals("[Charge Line 2] SellInvoiceType", jobCharge2.JR_InvoiceType, loadedJob.ChargeLineCollection[1].SellInvoiceType);
			AssertEquals("[Charge Line 2] SellLocalAmount", jobCharge2.JR_LocalSellAmt, loadedJob.ChargeLineCollection[1].SellLocalAmount);
			AssertEquals("[Charge Line 2] SellOSAmount", jobCharge2.JR_OSSellAmt, loadedJob.ChargeLineCollection[1].SellOSAmount);
			AssertEquals("[Charge Line 2] InvoiceType", jobCharge2.JR_InvoiceType, loadedJob.ChargeLineCollection[1].SellInvoiceType);
			AssertEquals("[Charge Line 2] DisplaySequence", jobCharge2.JR_DisplaySequence, loadedJob.ChargeLineCollection[1].DisplaySequence);
			AssertEquals("[Charge Line 2] ExternalDebtorCode", jobCharge2.SellAccount.CompanyData.OB_ARExternalDebtorCode, loadedJob.ChargeLineCollection[1].ExternalDebtorCode);
			AssertEquals("[Charge Line 2] ExternalCreditorCode", jobCharge2.CostAccount.CompanyData.OB_APExternalCreditorCode, loadedJob.ChargeLineCollection[1].ExternalCreditorCode);
		}

		#endregion

		#region TestGenerate

		public void TestGenerate()
		{
			var whsDocketBO = GetNewDocket();
			var job = Factory.BOFactory.NewJobForTesting<Job>();
			job.JH_ParentID = whsDocketBO.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_JobNum = "TESTJOB1";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			StaffSetup(job);
			BaseLineSetup(job, 120m, 35m, -140m, -65m, 135m, 50m, 35m, 55m);
			ChargeSetup(job);

			Factory.SaveForTesting();

			var writer = GetNewDataObjectWriter(whsDocketBO);
			var whsDocketData = writer.GetDataObject(whsDocketBO);
			var loadedJob = whsDocketData.JobCosting;

			AssertEquals("Branch Code", job.Branch.GB_Code, loadedJob.Branch.Code);
			AssertEquals("Branch Name", job.Branch.GB_BranchName, loadedJob.Branch.Name);
			AssertEquals("Sales Staff Code", "SS", loadedJob.SalesStaff.Code);
			AssertEquals("Sales Staff Name", "Sales", loadedJob.SalesStaff.Name);
			AssertEquals("Operations Staff Code", "OS", loadedJob.OperationsStaff.Code);
			AssertEquals("Operations Staff Name", "Operations", loadedJob.OperationsStaff.Name);
			AssertEquals("Home Branch Code", "SYD", loadedJob.HomeBranch.Code);
			AssertEquals("Home Branch Name", "Sydney Branch", loadedJob.HomeBranch.Name);
			AssertEquals("Total Revenue", 155m, loadedJob.TotalRevenue);
			AssertEquals("Total Cost", -205m, loadedJob.TotalCost);
			AssertEquals("Total WIP", 285m, loadedJob.TotalWIP);
			AssertEquals("Total Accrual", -290m, loadedJob.TotalAccrual);
			AssertEquals("WIP Recognized", 185m, loadedJob.WIPRecognized);
			AssertEquals("WIP Not Recognized", 100m, loadedJob.WIPNotRecognized);
			AssertEquals("Accrual Recognized", -90m, loadedJob.AccrualRecognized);
			AssertEquals("Accrual Not Recognized", -200m, loadedJob.AccrualNotRecognized);
			AssertEquals("Total Profit Loss", -55m, loadedJob.TotalJobProfit);
		}

		#endregion

		#region TestGenerate_AgentRevenue

		public void TestGenerate_AgentRevenue()
		{
			var whsDocketBO = GetNewDocket();
			var job = Factory.BOFactory.NewJobForTesting<Job>();
			job.JH_ParentID = whsDocketBO.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_JobNum = "TESTJOB2";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.AgentCollectPK = TestObjectCreator.AALSHI.PK;

			BaseLineSetup(job, 100m, 20m, -100m, -80m, 150m, 40m, 30m, 50m);
			ChargeSetup(job);

			Factory.SaveForTesting();

			var writer = GetNewDataObjectWriter(whsDocketBO);
			var whsDocketData = writer.GetDataObject(whsDocketBO);
			var loadedJob = whsDocketData.JobCosting;

			AssertEquals("Agent Revenue for the Overseas Agent", 120m, loadedJob.AgentRevenue);
		}

		#endregion

		#region TestGenerate_LocalClientRevenue

		public void TestGenerate_LocalClientRevenue()
		{
			var whsDocketBO = GetNewDocket();
			var job = Factory.BOFactory.NewJobForTesting<Job>();
			job.JH_ParentID = whsDocketBO.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_JobNum = "TESTJOB4";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = TestObjectCreator.AALSHI.PK;

			BaseLineSetup(job, 70m, 100m, -70m, -100m, 200m, 20m, 10m, 60m);
			ChargeSetup(job);

			Factory.SaveForTesting();

			var writer = GetNewDataObjectWriter(whsDocketBO);
			var whsDocketData = writer.GetDataObject(whsDocketBO);
			var loadedJob = whsDocketData.JobCosting;

			AssertEquals("Local Client Revenue", 170m, loadedJob.LocalClientRevenue);
		}

		#endregion

		#region TestGenerate_OtherDebtorRevenue

		public void TestGenerate_OtherDebtorRevenue()
		{
			var whsDocketBO = GetNewDocket();
			var job = Factory.BOFactory.NewJobForTesting<Job>();
			job.JH_ParentID = whsDocketBO.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_JobNum = "TESTJOB5";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			BaseLineSetup(job, 90m, 50m, -70m, -100m, 200m, 20m, 10m, 60m);
			ChargeSetup(job);

			Factory.SaveForTesting();

			var writer = GetNewDataObjectWriter(whsDocketBO);
			var whsDocketData = writer.GetDataObject(whsDocketBO);
			var loadedJob = whsDocketData.JobCosting;

			AssertEquals("Other Debtor Revenue", 140m, loadedJob.OtherDebtorRevenue);
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory.BOFactory);
		}

		TestObjectCreator TestObjectCreator;

		void StaffSetup(JobHeader jobToSave)
		{
			jobToSave.JH_GS_NKRepOps = "OS";
			jobToSave.JH_GS_NKRepSales = "SS";

			var salesStaff = Factory.NewWithValidTestData<GlbStaff>();
			salesStaff.GS_Code = "SS";
			salesStaff.GS_FullName = "Sales";

			var operationsStaff = Factory.NewWithValidTestData<GlbStaff>();
			operationsStaff.GS_Code = "OS";
			operationsStaff.GS_FullName = "Operations";

			var homeBranch = TestObjectCreator.NonCurrentBranch;
			homeBranch.GB_Code = "SYD";
			homeBranch.GB_BranchName = "Sydney Branch";

			operationsStaff.GS_GB_HomeBranch = homeBranch.PK;
		}

		void BaseLineSetup(JobHeader jobToSave, ZDecimal rev1Amt, ZDecimal rev2Amt, ZDecimal cost1Amt, ZDecimal cost2Amt, ZDecimal wIP1Amt, ZDecimal wIP2Amt, ZDecimal accrual1Amt, ZDecimal accrual2Amt)
		{
			var revRecOverride = TestObjectCreator.CC2.RevenueRecOverrides.AddNew();
			revRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			revRecOverride.Offset = 0;

			var aRInvoice = TestObjectCreator.CreateARInvoice<Accounting.Business.ARAP.Invoicing.ARInvoice>("123654" + jobToSave.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, TestObjectCreator.AALSHI);
			var rev1 = (TransactionLine)aRInvoice.Lines.AddNew();
			SetLine(jobToSave, rev1, TransactionLineTypes.Revenue, rev1Amt, TestObjectCreator.CC1);
			var rev1JobCharge = TestObjectCreator.CreateJobCharge(rev1, jobToSave, rev1.ChargeCode, rev1.TransactionCurrency);
			rev1JobCharge.JR_OSCostAmt = 0;

			var rev2 = (TransactionLine)aRInvoice.Lines.AddNew();
			SetLine(jobToSave, rev2, TransactionLineTypes.Revenue, rev2Amt, TestObjectCreator.CC2);
			var rev2JobCharge = TestObjectCreator.CreateJobCharge(rev2, jobToSave, rev2.ChargeCode, rev2.TransactionCurrency);
			rev2JobCharge.JR_OSCostAmt = 0;

			var aPInvoice = TestObjectCreator.CreateAPInvoice<Accounting.Business.ARAP.Invoicing.APInvoice>("789456" + jobToSave.JH_JobNum, GlbCompany.CurrentCompany.LocalCurrency, 1, 0, 0, 0, 0, 0, 0);
			var cost1 = (TransactionLine)aPInvoice.Lines.AddNew();
			SetLine(jobToSave, cost1, TransactionLineTypes.Cost, cost1Amt, TestObjectCreator.CC1);
			var cost1JobCharge = TestObjectCreator.CreateJobCharge(cost1, jobToSave, cost1.ChargeCode, cost1.TransactionCurrency);
			cost1JobCharge.JR_OSSellAmt = 0;

			var cost2 = (TransactionLine)aPInvoice.Lines.AddNew();
			SetLine(jobToSave, cost2, TransactionLineTypes.Cost, cost2Amt, TestObjectCreator.CC2);
			var cost2JobCharge = TestObjectCreator.CreateJobCharge(cost2, jobToSave, cost2.ChargeCode, cost2.TransactionCurrency);
			cost2JobCharge.JR_OSSellAmt = 0;

			var charge1 = Factory.NewWithValidTestData<BaseCharge>();
			charge1.JR_JH = jobToSave.PK;
			var wIP1 = Factory.New<WIP>();
			wIP1.AL_OSExTaxAmount = wIP1Amt;
			wIP1.AL_JH = jobToSave.PK;
			charge1.JR_AL_ARLine = wIP1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP1);

			var charge2 = Factory.NewWithValidTestData<BaseCharge>();
			charge2.JR_JH = jobToSave.PK;
			var wIP2 = Factory.New<WIP>();
			wIP2.AL_OSExTaxAmount = wIP2Amt;
			wIP2.AL_JH = jobToSave.PK;
			charge2.JR_AL_ARLine = wIP2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(wIP2);

			var accrual1 = Factory.New<Accrual>();
			accrual1.AL_OSExTaxAmount = accrual1Amt;
			accrual1.AL_JH = jobToSave.PK;
			charge1.JR_AL_APLine = accrual1.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual1);

			var accrual2 = Factory.New<Accrual>();
			accrual2.AL_OSExTaxAmount = accrual2Amt;
			accrual2.AL_JH = jobToSave.PK;
			charge2.JR_AL_APLine = accrual2.PK;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(accrual2);
		}

		void ChargeSetup(JobHeader jobToSave)
		{
			var revRecOverride = TestObjectCreator.CC3.RevenueRecOverrides.AddNew();
			revRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			revRecOverride.Offset = 0;

			var jobCharge1 = Factory.New<JobCharge>();
			jobCharge1.JR_AC = TestObjectCreator.CC3.PK;
			jobCharge1.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge1.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge1.JR_JH = jobToSave.PK;
			jobCharge1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge1.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge1.JR_LocalCostAmt = 200m;
			jobCharge1.JR_OSCostAmt = 200m;
			jobCharge1.JR_OSSellAmt = 0m;

			var jobCharge2 = Factory.New<JobCharge>();
			jobCharge2.JR_AC = TestObjectCreator.CC3.PK;
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge2.JR_JH = jobToSave.PK;
			jobCharge2.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge2.JR_LocalSellAmt = 100m;
			jobCharge2.JR_OSSellAmt = 100m;
			jobCharge2.JR_OSCostAmt = 0m;
		}

		void SetLine(JobHeader jobToSave, AccTransactionLines line, ZString lineType, ZDecimal amount, AccChargeCode chargeCode)
		{
			line.AL_OH = TestObjectCreator.AALSHI.PK;
			line.AL_JH = jobToSave.PK;
			line.AL_LineType = lineType;
			line.AL_LineAmount = amount;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AC = chargeCode.PK;
			if (line.AL_ExchangeRate == 1m)
			{
				line.AL_OSAmount = line.AL_LineAmount + line.AL_GSTVAT;
			}
		}

		protected abstract void SetPalletsSent(TDocket whsDocketBO, ZShort palletsSent);

		#endregion
	}
}
