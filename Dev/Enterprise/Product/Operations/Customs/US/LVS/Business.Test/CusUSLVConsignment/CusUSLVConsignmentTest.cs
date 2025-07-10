using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using GovernmentAgencyProgramCodeList = Enterprise.Customs.US.Business.GovernmentAgencyProgramCodeList;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVConsignment))]
	internal class CusUSLVConsignmentTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<CusUSLVClearance>().CusUSLVConsignments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var consignment = base.GetNewBusinessObject() as CusUSLVConsignment;
			consignment.CusUSLVItems.AddNew();
			return consignment;
		}

		public void TestSetDefaultEntryType()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			AssertEquals(consignment.ULB_EntryType, EntryTypeList.Codes.LowValue);

			using (USCustomsDataRegistry.Instance.DefaultEntryType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EntryTypeList.Codes.InformalFreeDutiable))
			{
				consignment = Factory.New<CusUSLVConsignment>();
				AssertEquals(consignment.ULB_EntryType, EntryTypeList.Codes.InformalFreeDutiable);
			}
		}

		public void TestIDispositionCodeDateParentMembers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);

			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO60RecordDispCode, "SO60RecordDispCode", dataGrouping.ZZZ_DataGrouping);
			var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "01", "ONEUSG", startDate, endDate);
			Factory.Save();

			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			AssertEquals(typeof(CodeDescriptionPairList), ((IDispositionCodeDateParent)consignment).DispositionCodeDescriptionList.GetType());
			AssertEquals(1, ((IDispositionCodeDateParent)consignment).DispositionCodeDescriptionList.Count);

			var disposition = consignment.DispositionCodes.AddNew();
			disposition.US_Code = "01";
			AssertEquals("ONEUSG", disposition.DispositionCodeDesc);
		}

		public void TestCanDelete()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			consignment.Shipment.ULH_EntryFilerCode = "XJ5";
			consignment.CE_EntryNum = "12345678";
			consignment.ULB_HouseBill = "1";
			AssertEquals("New Consignment can delete.", true, consignment.CanDelete);
			AssertEquals(string.Empty, consignment.ReasonForNotAbleToDelete.GetUnresolvedString());

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;
			AssertEquals("Consignment is waiting for message pending.", false, consignment.CanDelete);
			AssertEquals("House Bill 1 cannot be deleted because it is pending for sending the original message.", consignment.ReasonForNotAbleToDelete.GetUnresolvedString());

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			AssertEquals("Consignment is waiting for response message.", false, consignment.CanDelete);
			AssertEquals("House Bill 1 cannot be deleted because a message has previously been sent and it is awaiting a response from customs.", consignment.ReasonForNotAbleToDelete.GetUnresolvedString());

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			AssertEquals("Consignment is accepted by custom.", false, consignment.CanDelete);
			AssertEquals("House Bill 1 cannot be deleted because cargo release has been accepted by customs.", consignment.ReasonForNotAbleToDelete.GetUnresolvedString());

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.ErrorACECargoReleaseUpdate;
			consignment.Shipment.Logs.AddNew(AutoEvents.MessageStatusChange, $"|CRF=XJ512345678|TYP=SX|STA=CSA|RFN=1");
			AssertEquals("Consignment was accepted once cannot delete.", false, consignment.CanDelete);
			AssertEquals("House Bill 1 cannot be deleted because cargo release has been accepted by customs at least once.", consignment.ReasonForNotAbleToDelete.GetUnresolvedString());
		}

		public void TestIMessageAttacheeWithCBPSenderReferenceTransportMode()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_TransportMode = Core.Constants.TransportModes.Sea;
			var msgAttachee = clearance.CusUSLVConsignments.AddNew() as IMessageAttacheeInDeclaration;
			AssertEquals(Core.Constants.TransportModes.Sea, msgAttachee.TransportMode);
		}

		public void TestInterfaceRelatedToDIS()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);

			var consignment = GetNewBusinessObjectForDeleteTest(documentFactory) as CusUSLVConsignment;
			var entryNumber = documentFactory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "ENS";
			entryNumber.CE_RN_NKCountryCode = "US";
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_ParentID = consignment.PK;
			entryNumber.CE_ParentTable = consignment.TableName;
			entryNumber.CE_EntryNum = "";
			var shipment = consignment.Shipment;
			shipment.ULH_EntryFilerCode = "ABC";
			Env.Security.CustomsDISEdit.IsAllowed = true;

			var reqDoc = ((IHaveRequiredDocuments)consignment).RequiredDocuments.AddNew();
			reqDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ComplianceReport;
			reqDoc.EQ_DocType = Core.Constants.RefDocTypes.BillOfEntry;
			reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			reqDoc.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			reqDoc.EQ_DateReceived = DateTime.Today;
			reqDoc.EQ_ValidToDate = DateTime.Today;
			reqDoc.EQ_DocNumber = "0001";

			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			var docInfo = consignment.DocManagerInfo();
			AssertEquals(0, ((IDISHost)consignment).EDocs.Count());
			docInfo.Save();
			var filter = new ZQuery(StorageMainSchema.SM_ParentFK, consignment.PK);
			AssertNull(documentFactory.LoadTop1<IStorageMain>(filter));

			docInfo.AddFileOrDocument(contents, "eDocs.pdf", "ABC");

			var iHaveReqDocConsignment = consignment as IHaveRequiredDocuments;
			AssertEquals("HouseBill", consignment.ULB_HouseBill, iHaveReqDocConsignment.HouseBill);
			AssertEquals("MasterBill", shipment.ULH_MasterBill, iHaveReqDocConsignment.MasterBill);
			AssertEquals("UniqueConsignRef", shipment.ULH_JobNumber, iHaveReqDocConsignment.UniqueConsignRef);
			AssertNull(iHaveReqDocConsignment.ExportBroker);
			AssertEquals("RequiredDocuments", 1, iHaveReqDocConsignment.RequiredDocuments.Count);

			var disHostConsignment = consignment as IDISHost;
			AssertEquals(Env.Security.CustomsDISEdit.IsAllowed, disHostConsignment.DISEditable);
			AssertEquals("BranchPK", shipment.RegistryBranchPK, disHostConsignment.BranchPK);
			AssertEquals("CompanyPK", shipment.RegistryCompanyPK, disHostConsignment.CompanyPK);
			AssertEquals("JobNumber", shipment.ULH_JobNumber + "_" + consignment.ULB_HouseBill, disHostConsignment.JobNumber);
			AssertEquals("HumanReadable", "DIS", disHostConsignment.HumanReadable);

			var usDISHost = consignment as IUSDISHost;
			shipment.ULH_EntryFilerCode = "";
			AssertEquals("NoEntryFilerCodeAvailable warning", JobDeclaration.NoEntryFilerCodeAvailable, usDISHost.MessageSendingWarning);
			AssertEquals("NoPrepareIDAvailable warning", JobDeclaration.NoPrepareIDAvailable, usDISHost.MessageSendingError);
			AssertEquals("FormGroups", true, usDISHost.FormGroups.Contains(DISFormGroupCodes.NoGroup));
			AssertEquals("EDocs", 1, usDISHost.EDocs.Count());
		}

		public void TestConsignmentReadonlyWhenEntryLineReferenceHasValue()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			consignment.CE_EntryLineReference = "NOTEMPTYANYMORE";

			var propertyInfos = consignment.GetType().GetProperties()
								.Where(p => p.PropertyType == typeof(ZPropertyInfo))
								.Select(info => (ZPropertyInfo)info.GetValue(consignment));

			CombineAssertions(() =>
			{
				propertyInfos.ForEach(info => AssertEquals($"{info.Name} ReadOnly:", true, info.ReadOnly));
			});
			AssertEquals("NOTEMPTYANYMORE", consignment.CE_EntryLineReference);
		}

		public void TestConsignmentReadonlyWhenInactive()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			clearance.Logs.AddNew(AutoEvents.TransferToCustomsImportsDec, new KeyValuePair<string, string>("RFN", consignment.PK.ToString()));
			Factory.Save();
			var consignmentForAssert = new BusinessObjectFactory().Load<CusUSLVConsignment>(consignment.PK);
			Assert(!consignmentForAssert.ReadOnly);

			consignment.ULB_IsActive = false;
			Factory.Save();
			consignmentForAssert = new BusinessObjectFactory().Load<CusUSLVConsignment>(consignment.PK);
			Assert(consignmentForAssert.ReadOnly);

			consignment.ULB_IsActive = true;
			Factory.Save();
			consignmentForAssert = new BusinessObjectFactory().Load<CusUSLVConsignment>(consignment.PK);
			Assert(!consignmentForAssert.ReadOnly);
		}

		public void TestCanBeConvertedToStandaloneDeclaration()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var consignment3 = clearance.CusUSLVConsignments.AddNew();

			consignment1.CE_EntryLineReference = "NOTEMPTYANYMORE";
			consignment3.ULB_IsActive = false;

			CombineAssertions(() =>
			{
				Assert("Consignment1: Cannot be converted to Standalone Declaration as EntryLineReference is set", !consignment1.CanBeConvertedToStandaloneDeclaration);
				Assert("Consignment2: Can be converted to Standalone Declaration as EntryLineReference is not set", consignment2.CanBeConvertedToStandaloneDeclaration);
				Assert("Consignment3: Cannot be converted to Standalone Declaration as consignment is inactive", !consignment3.CanBeConvertedToStandaloneDeclaration);
			});
		}

		public void TestShouldConvertToStandaloneDeclaration()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			Assert(!consignment.ShouldConvertToStandaloneDeclaration);
			AssertEquals("Convert", consignment.ShouldConvertToStandaloneDeclarationInfo.Description);
		}

		public void TestAutoPopulateConsigneeAddressFromConsigneeOrgAddress()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			consignment.ULB_ConsigneeAddress1 = "basement";
			consignment.ULB_ConsigneeName = "street";

			var consigneeAddress = Factory.New<OrgAddress>();
			consigneeAddress.OA_Address1 = "living room";
			consigneeAddress.CompanyName = "home";
			consignment.ULB_OA_Consignee = consigneeAddress.PK;
			AssertEquals("Consignee Address auto populated from Consignee OrgAddress", "living room", consignment.ULB_ConsigneeAddress1);
			AssertEquals("Consignee Name auto populated from Consignee CompanyName", "home", consignment.ULB_ConsigneeName);
			Assert("Consignee Address fields should be readonly", consignment.ULB_ConsigneeAddress1Info.ReadOnly);
			Assert("Consignee OrgAddress field should stay editable", !consignment.ULB_OA_ConsigneeInfo.ReadOnly);
			Assert("Consignee OrgHeader field should stay editable", !consignment.ConsigneeOrgPKInfo.ReadOnly);

			consignment.ULB_OA_Consignee = ZGuid.Empty;
			AssertEquals(ZString.Empty, consignment.ULB_ConsigneeAddress1);
			AssertEquals(ZString.Empty, consignment.ULB_ConsigneeName);
			Assert("Consignee Address fields should be editable", !consignment.ULB_ConsigneeAddress1Info.ReadOnly);
		}

		public void TestAutoPopulateSellerAddressFromSellerOrgAddress()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			consignment.ULB_SellerAddress1 = "basement";
			consignment.ULB_SellerName = "street";

			var sellerAddress = Factory.New<OrgAddress>();
			sellerAddress.OA_Address1 = "living room";
			sellerAddress.CompanyName = "home";
			consignment.ULB_OA_Seller = sellerAddress.PK;
			AssertEquals("Seller Address auto populated from Seller OrgAddress", "living room", consignment.ULB_SellerAddress1);
			AssertEquals("Seller Name auto populated from Seller CompanyName", "home", consignment.ULB_SellerName);
			Assert("Seller Address fields should be readonly", consignment.ULB_SellerAddress1Info.ReadOnly);
			Assert("Seller OrgAddress field should stay editable", !consignment.ULB_OA_SellerInfo.ReadOnly);
			Assert("Seller OrgHeader field should stay editable", !consignment.SellerOrgPKInfo.ReadOnly);

			consignment.ULB_OA_Seller = ZGuid.Empty;
			AssertEquals(ZString.Empty, consignment.ULB_SellerAddress1);
			AssertEquals(ZString.Empty, consignment.ULB_SellerName);
			Assert("Seller Address fields should be editable", !consignment.ULB_SellerAddress1Info.ReadOnly);
		}

		#region Messages Tests

		public void TestMessages()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkTable = CusUSLVConsignmentSchema.Constants.TableName;
			message.EM_LinkUniqueID = consignment.PK;

			var messages = consignment.Messages;
			AssertCollectionContains(message, messages);
		}

		public void TestInBondRelatedRecords()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_LinkTable = CusUSLVConsignmentSchema.Constants.TableName;
			message1.EM_LinkUniqueID = consignment.PK;
			consignment.CE_EntryStatus = ImportEntryStatusList.Codes.CRN;
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			consignment.ULB_OwnerReferenceNumber = "TEST001";

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_LinkTable = CusUSLVConsignmentSchema.Constants.TableName;
			message2.EM_LinkUniqueID = consignment.PK;
			message2.EM_Status = EDIMessage.Status.Discarded;

			var records = consignment.InBondRelatedRecords;
			AssertEquals(1, records.Count);
			var record = records.OfType<MessageActionRelatedRecordWrapper>().Single();
			CombineAssertions(() =>
			{
				AssertEquals(0, record.TIBNumOfExtensions);
				AssertEquals(ZDateTime.Empty, record.TIBExpiryDate);
				AssertEquals(ZDateTime.Empty, record.ReleaseDate);
				AssertEquals(ImportEntryStatusList.Codes.CRN, record.EntryStatus);
				AssertEquals(ImportMessageStatusList.Codes.AwaitingArrival, record.Status);
				AssertEquals(ImportMessageStatusList.Descriptions.AwaitingArrival, record.StatusDesc);
				AssertEquals("TEST001", record.HumanFriendlyReference);
				AssertEquals(MessageAttacheeRecordTypeDescriptions.SimplifiedEntry, record.RecordTypeDescription);
				AssertEquals(MessageAttacheeRecordType.SimplifiedEntry, record.RecordType);
				AssertEquals(message1, record.MessagesToShow.Single());
			});
		}

		[TestDate(2020, 3, 4)]
		public void TestLatestMessageStatusDate()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			message.EM_LinkTable = CusUSLVConsignmentSchema.Constants.TableName;
			message.EM_LinkUniqueID = consignment.PK;

			message.EM_SystemCreateTimeUtc = ZDateTime.Today;

			AssertEquals(ZDateTime.Today, consignment.LatestMsgStatusDate);
		}

		public void TestDispositionCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);

			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO60RecordDispCode, "SO60RecordDispCode", dataGrouping.ZZZ_DataGrouping);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "98", "RELEASED", startDate, endDate);
			Factory.Save();

			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();

			var disposition1 = Factory.New<DispositionData>();
			disposition1.B7_ParentID = consignment.PK;
			disposition1.B7_ParentTableCode = "ULB";
			disposition1.B7_Type = "UDP";
			disposition1.B7_AddInfoData = "Code=98*DispositionDate=2019-11-05*ReleaseDate=2019-11-05*ReleaseOrigin=02";
			disposition1.B7_NAddInfoData = " ";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 disposition code", 1, consignment.DispositionCodes.Count);
				AssertEquals("Should have 1 disposition code view", 1, consignment.DispositionCodesView.Count);

				var date = new ZDateTime(2019, 11, 05);
				AssertEquals("98", consignment.DispositionCodesView[0].ErrorMessageIdentifier);
				AssertEquals("RELEASED", consignment.DispositionCodesView[0].NarrativeMessage);
				AssertEquals(date, consignment.DispositionCodesView[0].StatusDate);
				AssertEquals(date, consignment.DispositionCodesView[0].ReleaseDate);
				AssertEquals("02", consignment.DispositionCodesView[0].ReleaseOrigin);
				AssertEquals("Estimate Date of Arrival", consignment.DispositionCodesView[0].ReleaseOriginDescription);
			});
		}

		public void TestDeclarationPKShouldReturnEmptyGuid()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var messageAttachee = consignment as IMessageAttacheeInDeclaration;
			AssertEquals("DeclarationPK should return empty guid", ZGuid.Empty, messageAttachee.DeclarationPK);
		}

		#endregion

		public void TestEntryNumberProperties()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			consignment.CE_IssueDate = ZDateTime.BrettsBirthday;
			consignment.CE_EntryStatus = "HHH";
			consignment.CE_EntryNum = "258369";
			AssertEquals(ZDateTime.BrettsBirthday, consignment.CE_IssueDate);
			AssertEquals("HHH", consignment.CE_EntryStatus);
			AssertEquals("258369", consignment.CE_EntryNum);
		}

		public void TestCE_RailReferenceNumber()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();

			consignment.CE_RailReferenceNumber = "TestRFN";
			AssertEquals("TestRFN", consignment.CE_RailReferenceNumber);
		}

		public void TestGoodsValue()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			AssertEquals(0M, consignment.ULB_GoodsValue);
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_GoodsValue = 1.234;
			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_GoodsValue = 4.321;
			AssertEquals("Default currency is USD, value is 5", 5m, consignment.ULB_GoodsValue);

			var currencyCNY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
			var currencyUSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			AssertNotNull(currencyCNY);
			AssertNotNull(currencyUSD);

			currencyCNY.SetCustomsRate(ZDateTime.Now, ZDateTime.Now.AddDays(10), 0.147m);
			currencyUSD.SetCustomsRate(ZDateTime.Now, ZDateTime.Now.AddDays(10), 1.2m);
			item1.ULI_RX_NKCurrency = "CNY";
			item2.ULI_RX_NKCurrency = "USD";
			consignment.Shipment.ULH_DepartureDate = ZDate.Today;
			var expectGoodsValue = (item1 as ICusEntryLine).CL_CustomsValue + (item2 as ICusEntryLine).CL_CustomsValue;
			AssertEquals(expectGoodsValue, consignment.ULB_GoodsValue, 0.0001m);
		}

		public void TestCurrency()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			AssertEquals("USD", consignment.ULB_Currency);
		}

		public void TestFirstCusUSLVItem_ShouldRecalculate_WhenItemAdded()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVClearance>().CusUSLVConsignments.AddNew();
			AssertNull("pre condition", consignment.FirstCusUSLVItem);

			var item1 = consignment.CusUSLVItems.AddNew();
			AssertEquals("FirstUSLVItem should be item1", item1.PK, consignment.FirstCusUSLVItem.PK);

			consignment.CusUSLVItems.AddNew();
			AssertEquals("FirstUSLVItem should be unchanged", item1.PK, consignment.FirstCusUSLVItem.PK);
		}

		public void TestFirstCusUSLVItem_ShouldRecalculate_WhenItemDeleted()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVClearance>().CusUSLVConsignments.AddNew();
			var item1 = consignment.CusUSLVItems.AddNew();
			var item2 = consignment.CusUSLVItems.AddNew();

			AssertEquals("pre condition", item1.PK, consignment.FirstCusUSLVItem.PK);

			consignment.CusUSLVItems.RemoveAndDelete(item1);
			AssertEquals("FirstUSLVItem should be item2", item2.PK, consignment.FirstCusUSLVItem.PK);
		}

		public void TestFirstCusUSLVItemProductCode()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_PartNo = "Product 1";
			AssertEquals("pre condition", item1.ULI_PartNo, consignment.FirstCusUSLVItemProductCode);

			item1.ULI_PartNo = "Product 1.1";
			AssertEquals("Should change when underlying item property changed", item1.ULI_PartNo, consignment.FirstCusUSLVItemProductCode);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_PartNo = "Product 2";
			AssertEquals("Should be unchanged when new item added", item1.ULI_PartNo, consignment.FirstCusUSLVItemProductCode);

			consignment.CusUSLVItems.Sort("ULI_PartNo", ListSortDirection.Descending);
			AssertEquals("Should change when item sorted", item2.ULI_PartNo, consignment.FirstCusUSLVItemProductCode);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_PartNo, consignment.FirstCusUSLVItemProductCode);
		}

		public void TestFirstCusUSLVItemProductCode_ValueChanged()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;

			var firstValueChange = "Product 1";
			var secondValueChange = "Product 2";
			AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(consignment.FirstCusUSLVItemProductCodeInfo, delegate
			{
				consignment.FirstCusUSLVItemProductCode = firstValueChange;
				consignment.CusUSLVItems.AddNew();
				consignment.CusUSLVItems.Sort("ULI_PartNo", ListSortDirection.Ascending);
				consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
				consignment.FirstCusUSLVItemProductCode = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCusUSLVItemTariff()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_TariffFormatted = "456";
			AssertEquals("pre condition", item1.ULI_TariffFormatted, consignment.FirstCusUSLVItemTariff);

			item1.ULI_TariffFormatted = "789";
			AssertEquals("Should change when underlying item property changed", item1.ULI_TariffFormatted, consignment.FirstCusUSLVItemTariff);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_TariffFormatted = "234";
			AssertEquals("Should be unchanged when new item added", item1.ULI_TariffFormatted, consignment.FirstCusUSLVItemTariff);

			consignment.CusUSLVItems.Sort("ULI_TariffFormatted", ListSortDirection.Ascending);
			AssertEquals("Should change when item sorted", item2.ULI_TariffFormatted, consignment.FirstCusUSLVItemTariff);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_TariffFormatted, consignment.FirstCusUSLVItemTariff);
		}

		public void TestFirstCusUSLVItemTariff_ValueChanged()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;

			var firstValueChange = "567";
			var secondValueChange = "789";
			AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(consignment.FirstCusUSLVItemTariffInfo, delegate
			{
				consignment.FirstCusUSLVItemTariff = firstValueChange;
				consignment.CusUSLVItems.AddNew();
				consignment.CusUSLVItems.Sort("ULI_TariffFormatted", ListSortDirection.Ascending);
				consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
				consignment.FirstCusUSLVItemTariff = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCusUSLVItemGoodsDescription()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_GoodsDescription = "Test Goods 1";
			AssertEquals("pre condition", item1.ULI_GoodsDescription, consignment.FirstCusUSLVItemGoodsDescription);

			item1.ULI_GoodsDescription = "Test Goods 1.1";
			AssertEquals("Should change when underlying item property changed", item1.ULI_GoodsDescription, consignment.FirstCusUSLVItemGoodsDescription);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_GoodsDescription = "Test Goods 2";
			AssertEquals("Should be unchanged when new item added", item1.ULI_GoodsDescription, consignment.FirstCusUSLVItemGoodsDescription);

			consignment.CusUSLVItems.Sort("ULI_GoodsDescription", ListSortDirection.Descending);
			AssertEquals("Should change when item sorted", item2.ULI_GoodsDescription, consignment.FirstCusUSLVItemGoodsDescription);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_GoodsDescription, consignment.FirstCusUSLVItemGoodsDescription);
		}

		public void TestFirstCusUSLVItemGoodsDescription_ValueChanged()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;

			var firstValueChange = "Test Goods 1";
			var secondValueChange = "Test Goods 2";
			AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(consignment.FirstCusUSLVItemGoodsDescriptionInfo, delegate
			{
				consignment.FirstCusUSLVItemGoodsDescription = firstValueChange;
				consignment.CusUSLVItems.AddNew();
				consignment.CusUSLVItems.Sort("ULI_GoodsDescription", ListSortDirection.Ascending);
				consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
				consignment.FirstCusUSLVItemGoodsDescription = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCusUSLVItemCountryOfOrigin()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_RN_NKCountryOfOrigin = "AU";
			AssertEquals("pre condition", item1.ULI_RN_NKCountryOfOrigin, consignment.FirstCusUSLVItemCountryOfOrigin);

			item1.ULI_RN_NKCountryOfOrigin = "US";
			AssertEquals("Should change when underlying item property changed", item1.ULI_RN_NKCountryOfOrigin, consignment.FirstCusUSLVItemCountryOfOrigin);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_RN_NKCountryOfOrigin = "NZ";
			AssertEquals("Should be unchanged when new item added", item1.ULI_RN_NKCountryOfOrigin, consignment.FirstCusUSLVItemCountryOfOrigin);

			consignment.CusUSLVItems.Sort("ULI_RN_NKCountryOfOrigin", ListSortDirection.Ascending);
			AssertEquals("Should change when item sorted", item2.ULI_RN_NKCountryOfOrigin, consignment.FirstCusUSLVItemCountryOfOrigin);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_RN_NKCountryOfOrigin, consignment.FirstCusUSLVItemCountryOfOrigin);
		}

		public void TestFirstCusUSLVItemCountryOfOrigin_ValueChanged()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;

			var firstValueChange = "AU";
			var secondValueChange = "US";
			AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(consignment.FirstCusUSLVItemCountryOfOriginInfo, delegate
			{
				consignment.FirstCusUSLVItemCountryOfOrigin = firstValueChange;
				consignment.CusUSLVItems.AddNew();
				consignment.CusUSLVItems.Sort("ULI_RN_NKCountryOfOrigin", ListSortDirection.Ascending);
				consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
				consignment.FirstCusUSLVItemCountryOfOrigin = secondValueChange;
			}, firstValueChange, secondValueChange);
		}

		public void TestFirstCusUSLVItemLineValue()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_GoodsValue = 100m;
			AssertEquals("pre condition", item1.ULI_GoodsValue, consignment.FirstCusUSLVItemLineValue);

			item1.ULI_GoodsValue = 200m;
			AssertEquals("Should change when underlying item property changed", item1.ULI_GoodsValue, consignment.FirstCusUSLVItemLineValue);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_GoodsValue = 300m;
			AssertEquals("Should be unchanged when new item added", item1.ULI_GoodsValue, consignment.FirstCusUSLVItemLineValue);

			consignment.CusUSLVItems.Sort("ULI_GoodsValue", ListSortDirection.Descending);
			AssertEquals("Should change when item sorted", item2.ULI_GoodsValue, consignment.FirstCusUSLVItemLineValue);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_GoodsValue, consignment.FirstCusUSLVItemLineValue);
		}

		public void TestFirstCusUSLVItemLineValue_ValueChanged()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;

			var firstValueChange = 100m;
			var secondValueChange = 200m;
			AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(consignment.FirstCusUSLVItemLineValueInfo, delegate
			{
				consignment.FirstCusUSLVItemLineValue = firstValueChange;
				consignment.CusUSLVItems.AddNew();
				consignment.CusUSLVItems.Sort("ULI_GoodsValue", ListSortDirection.Ascending);
				consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
				consignment.FirstCusUSLVItemLineValue = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString());
		}

		public void TestFirstCusUSLVItemCurrency()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_RX_NKCurrency = "AUD";
			AssertEquals("pre condition", item1.ULI_RX_NKCurrency, consignment.FirstCusUSLVItemCurrency);

			item1.ULI_RX_NKCurrency = "USD";
			AssertEquals("Should change when underlying item property changed", item1.ULI_RX_NKCurrency, consignment.FirstCusUSLVItemCurrency);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_RX_NKCurrency = "EUR";
			AssertEquals("Should be unchanged when new item added", item1.ULI_RX_NKCurrency, consignment.FirstCusUSLVItemCurrency);

			consignment.CusUSLVItems.Sort("ULI_RX_NKCurrency", ListSortDirection.Ascending);
			AssertEquals("Should change when item sorted", item2.ULI_RX_NKCurrency, consignment.FirstCusUSLVItemCurrency);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_RX_NKCurrency, consignment.FirstCusUSLVItemCurrency);
		}

		public void TestFirstCusUSLVItemCurrency_ValueChanged()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;

			var firstValueChange = "AUD";
			var secondValueChange = "EUR";
			AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(consignment.FirstCusUSLVItemCurrencyInfo, delegate
			{
				consignment.FirstCusUSLVItemCurrency = firstValueChange;
				consignment.CusUSLVItems.AddNew();
				consignment.CusUSLVItems.Sort("ULI_RX_NKCurrency", ListSortDirection.Descending);
				consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
				consignment.FirstCusUSLVItemCurrency = secondValueChange;
			}, firstValueChange, secondValueChange, "USD");
		}

		[TestDate(2020, 6, 25)]
		public void TestFirstCusUSLVItemExchangeRate()
		{
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			audCurrency.SetCustomsRate(ZDateTime.Now, ZDateTime.Now, 5m);
			audCurrency.SetCustomsRate(ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(-1), 6m);

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			usdCurrency.SetCustomsRate(ZDateTime.Now, ZDateTime.Now, 7m);

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_RX_NKCurrency = "AUD";
			clearance.ULH_DepartureDate = ZDate.Today;

			AssertEquals("pre condition", item1.ULI_RX_NKCurrEXRate, consignment.FirstCusUSLVItemExchangeRate);

			clearance.ULH_DepartureDate = ZDate.Today.AddDays(-1);
			AssertEquals("Should change when underlying item property changed", item1.ULI_RX_NKCurrEXRate, consignment.FirstCusUSLVItemExchangeRate);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_RX_NKCurrency = "USD";
			clearance.ULH_DepartureDate = ZDate.Today;
			AssertEquals("Should be unchanged when new item added", item1.ULI_RX_NKCurrEXRate, consignment.FirstCusUSLVItemExchangeRate);

			consignment.CusUSLVItems.Sort("ULI_RX_NKCurrEXRate", ListSortDirection.Ascending);
			AssertEquals("Should change when item sorted", item2.ULI_RX_NKCurrEXRate, consignment.FirstCusUSLVItemExchangeRate);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_RX_NKCurrEXRate, consignment.FirstCusUSLVItemExchangeRate);
		}

		public void TestFirstCusUSLVItemAntiDumping()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_AntiDumping = ZBool.True;
			AssertEquals("pre condition", item1.ULI_AntiDumping, consignment.FirstCusUSLVItemAntiDumping);

			item1.ULI_AntiDumping = ZBool.False;
			AssertEquals("Should change when underlying item property changed", item1.ULI_AntiDumping, consignment.FirstCusUSLVItemAntiDumping);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_AntiDumping = ZBool.True;
			AssertEquals("Should be unchanged when new item added", item1.ULI_AntiDumping, consignment.FirstCusUSLVItemAntiDumping);

			consignment.CusUSLVItems.Sort("ULI_AntiDumping", ListSortDirection.Descending);
			AssertEquals("Should change when item sorted", item2.ULI_AntiDumping, consignment.FirstCusUSLVItemAntiDumping);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_AntiDumping, consignment.FirstCusUSLVItemAntiDumping);
		}

		public void TestFirstCusUSLVItemAntiDumping_ValueChanged()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;

			var firstValueChange = ZBool.False;
			var secondValueChange = ZBool.True;
			AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(consignment.FirstCusUSLVItemAntiDumpingInfo, delegate
			{
				consignment.FirstCusUSLVItemAntiDumping = firstValueChange;
				consignment.CusUSLVItems.AddNew();
				consignment.CusUSLVItems.Sort("ULI_AntiDumping", ListSortDirection.Descending);
				consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
				consignment.FirstCusUSLVItemAntiDumping = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString(), "Y");
		}

		public void TestFirstCusUSLVItemCountervailing()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_Countervailing = ZBool.True;
			AssertEquals("pre condition", item1.ULI_Countervailing, consignment.FirstCusUSLVItemCountervailing);

			item1.ULI_Countervailing = ZBool.False;
			AssertEquals("Should change when underlying item property changed", item1.ULI_Countervailing, consignment.FirstCusUSLVItemCountervailing);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_Countervailing = ZBool.True;
			AssertEquals("Should be unchanged when new item added", item1.ULI_Countervailing, consignment.FirstCusUSLVItemCountervailing);

			consignment.CusUSLVItems.Sort("ULI_Countervailing", ListSortDirection.Descending);
			AssertEquals("Should change when item sorted", item2.ULI_Countervailing, consignment.FirstCusUSLVItemCountervailing);

			consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
			AssertEquals("Should change when underlying item changed", item1.ULI_Countervailing, consignment.FirstCusUSLVItemCountervailing);
		}

		public void TestFirstCusUSLVItemCountervailing_ValueChanged()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;

			var firstValueChange = ZBool.False;
			var secondValueChange = ZBool.True;
			AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(consignment.FirstCusUSLVItemCountervailingInfo, delegate
			{
				consignment.FirstCusUSLVItemCountervailing = firstValueChange;
				consignment.CusUSLVItems.AddNew();
				consignment.CusUSLVItems.Sort("ULI_Countervailing", ListSortDirection.Descending);
				consignment.CusUSLVItems.RemoveAndDelete(consignment.CusUSLVItems[0]);
				consignment.FirstCusUSLVItemCountervailing = secondValueChange;
			}, firstValueChange.ToString(), secondValueChange.ToString(), "Y");
		}

		public void TestHasMultipleItemLine()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			AssertEquals(false, consignment.HasMultipleItemLine);

			consignment.CusUSLVItems.AddNew();
			AssertEquals(false, consignment.HasMultipleItemLine);

			consignment.CusUSLVItems.AddNew();
			AssertEquals(true, consignment.HasMultipleItemLine);
		}

		public void TestHasAtLeastOnePGARequirementOnAnyItemLine()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			AssertEquals(false, consignment.HasAtLeastOnePGARequirementOnAnyItemLine);

			var item = consignment.CusUSLVItems.AddNew();
			AssertEquals(false, consignment.HasAtLeastOnePGARequirementOnAnyItemLine);

			const string tariffNum = "8923894890";
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNum;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "OM1";

			item.ULI_Tariff = tariffNum;

			AssertEquals(true, consignment.HasAtLeastOnePGARequirementOnAnyItemLine);
		}

		void AssertFirstCusUSLVItemPropertyValueChangedWorkCorrectly(ZPropertyInfo info, Action action, ZString firstValueChange, ZString secondValueChange, ZString defaultValue = default)
		{
			var result = new ZStringBuilder();
			info.ValueChanged += (object sender, EventArgs e) =>
			{
				if (e is ValueChangedEventArgs va)
				{
					result.AppendLine($"Old='{va.OldValue}',New='{va.NewValue}'");
				}
			};

			var originalValue = info.Value;
			action.Invoke();

			if (string.IsNullOrEmpty(defaultValue))
			{
				defaultValue = info.DefaultValue.ToString();
			}

			AssertEquals("ValueChanged should be fired 4 times with correct parameters", $"Old='{originalValue}',New='{firstValueChange}'\r\nOld='{firstValueChange}',New='{defaultValue}'\r\nOld='{defaultValue}',New='{firstValueChange}'\r\nOld='{firstValueChange}',New='{secondValueChange}'", result.ToString().Trim());
		}

		[TestDate(2020, 08, 08, 08, 08, 08)]
		public void TestULBMessageStatusSetter_WhenValueIsASAAndULBSubmittedDateIsEmpty_WillSetULBSubmittedDate()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_SubmittedDate = ZDateTime.Empty;

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			AssertEquals(TestDateAttribute.Date, consignment.ULB_SubmittedDate);
		}

		[TestDate(2020, 08, 08, 08, 08, 08)]
		public void TestULBMessageStatusSetter_WhenValueIsNotASAAndULBSubmittedDateIsEmpty_WillNotSetULBSubmittedDate()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_SubmittedDate = ZDateTime.Empty;

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;

			AssertEquals(ZDateTime.Empty, consignment.ULB_SubmittedDate);
		}

		[TestDate(2020, 08, 08, 08, 08, 08)]
		public void TestULBMessageStatusSetter_WhenValueIsASAAndULBSubmittedDateIsNotEmpty_WillNotSetULBSubmittedDate()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var existingULBSubmittedDate = new ZDateTime(2020, 07, 07, 07, 07, 07);
			consignment.ULB_SubmittedDate = existingULBSubmittedDate;

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;

			AssertEquals(existingULBSubmittedDate, consignment.ULB_SubmittedDate);
		}

		[TestDate(2020, 08, 08, 08, 08, 08)]
		public void TestULBMessageStatusSetter_WhenValueIsNotASAAndULBSubmittedDateIsNotEmpty_WillNotSetULBSubmittedDate()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var existingULBSubmittedDate = new ZDateTime(2020, 07, 07, 07, 07, 07);
			consignment.ULB_SubmittedDate = existingULBSubmittedDate;

			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;

			AssertEquals(existingULBSubmittedDate, consignment.ULB_SubmittedDate);
		}

		public void TestReadonlyAttribute()
		{
			var type = typeof(CusUSLVConsignment);
			CombineAssertions(() =>
			{
				foreach (var propertyName in propertiesShouldBeReadonly())
				{
					AssertHasCustomAttribute<ReadOnlyAttribute>(type, propertyName, false, a => a.IsReadOnly);
				}
			});

			IEnumerable<string> propertiesShouldBeReadonly()
			{
				yield return "ULB_SubmittedDate";
				yield return "ULB_MessageStatus";
				yield return "ULB_DISIndicator";
			}
		}

		public void TestListAttributes()
		{
			var type = typeof(CusUSLVConsignment);
			AssertHasCustomAttribute<ListAttribute>(type, "ConsigneeOrgPK", false, a => a.ListDataSourceMember == "Lookups.ConsigneeOrgList");
			AssertHasCustomAttribute<ListAttribute>(type, "SellerOrgPK", false, a => a.ListDataSourceMember == "Lookups.SellerOrgList");
			AssertHasCustomAttribute<ListAttribute>(type, "ULB_HouseBillIssuerSCAC", false, a => a.ListDataSourceMember == "Lookups.ULB_HouseBillIssuerSCACList");
			AssertHasCustomAttribute<ListAttribute>(type, "ULB_MessageStatus", false, a => a.ListDataSourceMember == "Lookups.ULB_MessageStatusList");
			AssertHasCustomAttribute<ListAttribute>(type, "ULB_ConvertAction", false, a => a.ListDataSourceMember == "Lookups.ULB_ConvertActionList");
		}

		public void TestSellerIdentifierCaption()
		{
			var dataProperty = DataBoundResourceStrings.GetDataForProperty(typeof(CusUSLVConsignment), AutoCusUSLVConsignment.Schema.ULB_SellerIdentifier);

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Seller Identifier", dataProperty.Caption);
				AssertEquals("Medium Caption", "Seller ID.", dataProperty.MediumCaption);
				AssertEquals("Short Caption", "Seller ID.", dataProperty.ShortCaption);
			});
		}

		public void TestConsigneeAddressFieldsReadonly()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			Assert(!consignment.ConsigneeOrgPKInfo.ReadOnly);
			Assert(!consignment.ULB_OA_ConsigneeInfo.ReadOnly);
			Assert(!consignment.ULB_ConsigneeAddress1Info.ReadOnly);
			Assert(!consignment.ULB_ConsigneeAddress2Info.ReadOnly);
			Assert(!consignment.ULB_ConsigneeCityInfo.ReadOnly);
			Assert(!consignment.ULB_ConsigneeNameInfo.ReadOnly);
			Assert(!consignment.ULB_ConsigneePostCodeInfo.ReadOnly);
			Assert(!consignment.ULB_RN_NKConsigneeCountryInfo.ReadOnly);

			consignment.ULB_OA_Consignee = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;

			Assert(!consignment.ConsigneeOrgPKInfo.ReadOnly);
			Assert(!consignment.ULB_OA_ConsigneeInfo.ReadOnly);
			Assert(consignment.ULB_ConsigneeAddress1Info.ReadOnly);
			Assert(consignment.ULB_ConsigneeAddress2Info.ReadOnly);
			Assert(consignment.ULB_ConsigneeCityInfo.ReadOnly);
			Assert(consignment.ULB_ConsigneeNameInfo.ReadOnly);
			Assert(consignment.ULB_ConsigneePostCodeInfo.ReadOnly);
			Assert(consignment.ULB_RN_NKConsigneeCountryInfo.ReadOnly);

			consignment.ULB_OA_Consignee = ZGuid.Empty;

			Assert(!consignment.ConsigneeOrgPKInfo.ReadOnly);
			Assert(!consignment.ULB_OA_ConsigneeInfo.ReadOnly);
			Assert(!consignment.ULB_ConsigneeAddress1Info.ReadOnly);
			Assert(!consignment.ULB_ConsigneeAddress2Info.ReadOnly);
			Assert(!consignment.ULB_ConsigneeCityInfo.ReadOnly);
			Assert(!consignment.ULB_ConsigneeNameInfo.ReadOnly);
			Assert(!consignment.ULB_ConsigneePostCodeInfo.ReadOnly);
			Assert(!consignment.ULB_RN_NKConsigneeCountryInfo.ReadOnly);

			AssertOrgGuidFieldsReadonly(consignment.ULB_ConsigneeAddress1Info, true);
			AssertOrgGuidFieldsReadonly(consignment.ULB_ConsigneeAddress2Info, true);
			AssertOrgGuidFieldsReadonly(consignment.ULB_ConsigneeCityInfo, true);
			AssertOrgGuidFieldsReadonly(consignment.ULB_ConsigneeNameInfo, true);
			AssertOrgGuidFieldsReadonly(consignment.ULB_ConsigneePostCodeInfo, true);
			AssertOrgGuidFieldsReadonly(consignment.ULB_RN_NKConsigneeCountryInfo, true);

			void AssertOrgGuidFieldsReadonly(ZPropertyInfo propertyInfo, bool shouldBeReadonly)
			{
				propertyInfo.Value = (ZString)"AU";
				Assert(shouldBeReadonly);
				propertyInfo.Value = ZString.Empty;
			}
		}

		public void TestParents()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			var pga = item.CusUSLVItemPGAs.AddNew();
			AssertEquals(shipment, consignment.Shipment);
			AssertEquals(consignment, item.Consignment);
			AssertEquals(item, pga.ParentItem);
		}

		public void TestHasPGAPending_IsReadOnly()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			AssertEquals("Should be read-only", true, consignment.ULB_HasPGAPendingInfo.ReadOnly);
		}

		public void TestHasPGAPending_PopulatesOnSave()
		{
			const string tariffNumber = "8542996328";
			var tariff = CreateTariff(tariffNumber, OGARequirementList.Codes.FD1);

			Factory.Save();

			var shipment = Factory.New<CusUSLVClearance>();
			var consignment1 = shipment.CusUSLVConsignments.AddNew();
			var consignment2 = shipment.CusUSLVConsignments.AddNew();
			var consignment3 = shipment.CusUSLVConsignments.AddNew();

			var item1 = consignment1.CusUSLVItems.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();

			item1.ULI_Tariff = tariffNumber;
			item3.ULI_Tariff = tariffNumber;
			item3.ACEFDAWrapper.DisclaimReason = "A";
			item3.ACEFDAWrapper.Indicator = "C";

			(var agency, _) = item1.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FDA);
			var disclaimRequired1 = item1.PGARequirementIndicator.IsPGAProgramMayRequired(agency);
			var disclaimRequired2 = item2.PGARequirementIndicator.IsPGAProgramMayRequired(agency);
			var disclaimRequired3 = item3.PGARequirementIndicator.IsPGAProgramMayRequired(agency);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Consignment 1: PGA Disclaim is 'may required'", true, disclaimRequired1);
				AssertEquals("Consignment 2: PGA Disclaim is not 'may required'", false, disclaimRequired2);
				AssertEquals("Consignment 3: PGA Disclaim is 'may required' (but has already been provided)", true, disclaimRequired3);
			});

			CombineAssertions("Has PGA Pending not set before saving", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_HasPGAPending);
				AssertEquals("Consignment2", false, consignment2.ULB_HasPGAPending);
				AssertEquals("Consignment3", false, consignment3.ULB_HasPGAPending);
			});

			Factory.Save();

			CombineAssertions("HAS PGA Pending set for Consignment with any PGA disclaims that are 'may required' but not provided", () =>
			{
				AssertEquals("Consignment1: 'may required' and not provided", true, consignment1.ULB_HasPGAPending);
				AssertEquals("Consignment2: not 'may required'", false, consignment2.ULB_HasPGAPending);
				AssertEquals("Consignment3: 'may required' but already provided", false, consignment3.ULB_HasPGAPending);
			});
		}

		public void TestHasPGAPending_WhenConsignmentHasBeenConvertedToDeclaration_PopulatesOnSave()
		{
			const string tariffNumber = "8542996328";
			var tariff = CreateTariff(tariffNumber, OGARequirementList.Codes.FD1);

			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();

			var item1 = consignment1.CusUSLVItems.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();

			item1.ULI_Tariff = tariffNumber;
			item2.ULI_Tariff = tariffNumber;

			consignment1.CE_EntryLineReference = "NOTEMPTYANYMORE";

			CombineAssertions("Precondition:", () =>
			{
				AssertEquals("Consignment1: Cannot be converted to Standalone Declaration as already converted", false, consignment1.CanBeConvertedToStandaloneDeclaration);
				AssertEquals("Consignment2: Can be converted to Standalone Declaration as EntryLineReference is not set", true, consignment2.CanBeConvertedToStandaloneDeclaration);
			});

			CombineAssertions("Has PGA Pending not set before saving", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_HasPGAPending);
				AssertEquals("Consignment2", false, consignment2.ULB_HasPGAPending);
			});

			Factory.Save();

			CombineAssertions("Has PGA Pending only set for Consignments which have not been converted to Standalone Declaration", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_HasPGAPending);
				AssertEquals("Consignment2", true, consignment2.ULB_HasPGAPending);
			});
		}

		public void TestPGANotSupported_IsReadOnly()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			AssertEquals("Should be read-only", true, consignment.ULB_PGANotSupportedInfo.ReadOnly);
		}

		public void TestPGANotSupported_WithTaxFeeCode_PopulatesOnSave()
		{
			const string taxFeeTariffNo = "1234567890";
			const string otherTariffNo = "9876543210";

			var taxFeeTariff = Factory.New<USCTariff>();
			taxFeeTariff.UE_Tariff = taxFeeTariffNo;
			taxFeeTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			taxFeeTariff.UE_DateTo = ZDate.Today;
			var dutyRate = taxFeeTariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = "016";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;

			var otherTariff = Factory.New<USCTariff>();
			otherTariff.UE_Tariff = otherTariffNo;
			otherTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			otherTariff.UE_DateTo = ZDate.Today;

			var shipment = Factory.New<CusUSLVClearance>();
			var consignment1 = shipment.CusUSLVConsignments.AddNew();
			var consignment2 = shipment.CusUSLVConsignments.AddNew();
			var consignment3 = shipment.CusUSLVConsignments.AddNew();

			var item1a = consignment1.CusUSLVItems.AddNew();
			var item1b = consignment1.CusUSLVItems.AddNew();
			var item2a = consignment2.CusUSLVItems.AddNew();
			var item2b = consignment2.CusUSLVItems.AddNew();
			var item3a = consignment3.CusUSLVItems.AddNew();
			var item3b = consignment3.CusUSLVItems.AddNew();

			item1a.ULI_Tariff = taxFeeTariffNo;
			item1b.ULI_Tariff = taxFeeTariffNo;
			item2a.ULI_Tariff = taxFeeTariffNo;
			item2b.ULI_Tariff = otherTariffNo;
			item3a.ULI_Tariff = otherTariffNo;
			item3b.ULI_Tariff = otherTariffNo;

			CombineAssertions("PGA Not Supported not set before saving", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", false, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", false, consignment3.ULB_PGANotSupported);
			});

			Factory.Save();

			CombineAssertions("PGA Not Supported set for Consignment with any Items with TaxFee Tariff", () =>
			{
				AssertEquals("Consignment1", true, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", true, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", false, consignment3.ULB_PGANotSupported);
			});
		}

		public void TestPGANotSupported_WithDisclaimGenerallyNotAllowed_PopulatesOnSave()
		{
			const string tariffNumber1 = "8542996328";
			const string tariffNumber2 = "8542996329";

			var tariff1 = CreateTariff(tariffNumber1, OGARequirementList.Codes.FD1);
			var tariff2 = CreateTariff(tariffNumber2, OGARequirementList.Codes.FD2);

			var shipment = Factory.New<CusUSLVClearance>();
			var consignment1 = shipment.CusUSLVConsignments.AddNew();
			var consignment2 = shipment.CusUSLVConsignments.AddNew();
			var consignment3 = shipment.CusUSLVConsignments.AddNew();
			var consignment4 = shipment.CusUSLVConsignments.AddNew();
			var item1 = consignment1.CusUSLVItems.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();
			var item4 = consignment4.CusUSLVItems.AddNew();
			item1.ULI_Tariff = tariffNumber1;
			item2.ULI_Tariff = tariffNumber1;
			item3.ULI_Tariff = tariffNumber2;
			item4.ULI_Tariff = tariffNumber2;

			item2.ACEFDAWrapper.DisclaimReason = "A";
			item2.ACEFDAWrapper.Indicator = "C";
			item4.ACEFDAWrapper.DisclaimReason = "A";
			item4.ACEFDAWrapper.Indicator = "C";

			(var fdaAgency, var fdaProgram) = item1.RequirementsProvider.PopulateAgencyProgram(GovernmentAgencyProgramCodeList.Codes.FDA);

			var pgaRequired1 = item1.PGARequirementIndicator.IsPGAProgramRequired(fdaAgency);
			var pgaRequired2 = item2.PGARequirementIndicator.IsPGAProgramRequired(fdaAgency);
			var pgaRequired3 = item3.PGARequirementIndicator.IsPGAProgramRequired(fdaAgency);
			var pgaRequired4 = item4.PGARequirementIndicator.IsPGAProgramRequired(fdaAgency);

			var pgaMayRequired1 = item1.PGARequirementIndicator.IsPGAProgramMayRequired(fdaAgency);
			var pgaMayRequired2 = item2.PGARequirementIndicator.IsPGAProgramMayRequired(fdaAgency);
			var pgaMayRequired3 = item3.PGARequirementIndicator.IsPGAProgramMayRequired(fdaAgency);
			var pgaMayRequired4 = item4.PGARequirementIndicator.IsPGAProgramMayRequired(fdaAgency);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Consignment 1: FD1 PGA required", false, pgaRequired1);
				AssertEquals("Consignment 1: FD1 PGA may required", true, pgaMayRequired1);
				AssertEquals("Consignment 2: FD1 PGA required", false, pgaRequired2);
				AssertEquals("Consignment 2: FD1 PGA may required", true, pgaMayRequired2);
				AssertEquals("Consignment 3: FD2 PGA required", true, pgaRequired3);
				AssertEquals("Consignment 3: FD2 PGA may required", false, pgaMayRequired3);
				AssertEquals("Consignment 4: FD2 PGA required", true, pgaRequired4);
				AssertEquals("Consignment 4: FD2 PGA may required", false, pgaMayRequired4);
			});

			CombineAssertions("PGA Not Supported not set before saving", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", false, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", false, consignment3.ULB_PGANotSupported);
				AssertEquals("Consignment4", false, consignment4.ULB_PGANotSupported);
			});

			Factory.Save();

			CombineAssertions("PGA Not Supported set for Consignment with any PGA Disclaim 'generally not allowed'", () =>
			{
				AssertEquals("Consignment1: FD1 'May Required' and not disclaimed", true, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2: FD1 'May Required' and disclaim reason provided", false, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3: FD2 'Required' and not disclaimed", true, consignment3.ULB_PGANotSupported);
				AssertEquals("Consignment4: FD3 'Required' and disclaim reason provided", true, consignment4.ULB_PGANotSupported);
			});
		}

		public void TestPGANotSupported_WithConsignmentValueExceedesDeminimus_PopulatesOnSave()
		{
			var grouping = Factory.NewWithValidTestData<RefDataGrouping>();
			grouping.ZZZ_DataGrouping = "US";
			grouping.ZZZ_Description = "United States";
			var deminimus = Factory.New<RefCusTaxOrFee>();
			deminimus.ZZF_ZZZ_NKDataGrouping = "US";
			deminimus.ZZF_Code = "DEM";
			deminimus.ZZF_StartDate = new ZDateTime(1960, 1, 1);
			deminimus.ZZF_EndDate = ZDateTime.Today.AddYears(1);
			deminimus.ZZF_Value = 800;
			deminimus.ZZF_Description = "a value";
			Factory.Save();

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			usdCurrency.SetCustomsRate(ZDateTime.Now, ZDateTime.Now, 1m);

			var deminimusValue = FeeCalculationHelper.GetDeminimus(Factory);
			AssertEquals("Precondition, deminimus is set", 800m, deminimusValue);

			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var consignment3 = clearance.CusUSLVConsignments.AddNew();

			var item1a = consignment1.CusUSLVItems.AddNew();
			var item1b = consignment1.CusUSLVItems.AddNew();
			var item2a = consignment2.CusUSLVItems.AddNew();
			var item2b = consignment2.CusUSLVItems.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();

			item1a.ULI_RX_NKCurrency = "USD";
			item1b.ULI_RX_NKCurrency = "USD";
			item2a.ULI_RX_NKCurrency = "USD";
			item2b.ULI_RX_NKCurrency = "USD";
			item3.ULI_RX_NKCurrency = "USD";

			item1a.ULI_GoodsValue = deminimusValue;
			item1b.ULI_GoodsValue = 1;
			item2a.ULI_GoodsValue = 1;
			item2b.ULI_GoodsValue = 1;
			item3.ULI_GoodsValue = deminimusValue;

			CombineAssertions("PGA Not Supported not set before saving", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", false, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", false, consignment3.ULB_PGANotSupported);
			});

			Factory.Save();

			CombineAssertions("PGA Not Supported set for Consignment with Goods Value exceeding Deminimus", () =>
			{
				AssertEquals("Consignment1", true, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", false, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", false, consignment3.ULB_PGANotSupported);
			});
		}

		public void TestPGANotSupported_WithAnitDumpingApplicable_PopulatesOnSave()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var consignment3 = clearance.CusUSLVConsignments.AddNew();

			var item1a = consignment1.CusUSLVItems.AddNew();
			var item1b = consignment1.CusUSLVItems.AddNew();
			var item2a = consignment2.CusUSLVItems.AddNew();
			var item2b = consignment2.CusUSLVItems.AddNew();
			var item3a = consignment3.CusUSLVItems.AddNew();
			var item3b = consignment3.CusUSLVItems.AddNew();

			item1a.ULI_AntiDumping = true;
			item1b.ULI_AntiDumping = true;
			item2a.ULI_AntiDumping = true;
			item2b.ULI_AntiDumping = false;
			item3a.ULI_AntiDumping = false;
			item3b.ULI_AntiDumping = false;

			CombineAssertions("PGA Not Supported not set before saving", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", false, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", false, consignment3.ULB_PGANotSupported);
			});

			Factory.Save();

			CombineAssertions("PGA Not Supported set for Consignment with any Items with anti-dumping applicable", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", true, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", true, consignment3.ULB_PGANotSupported);
			});
		}

		public void TestPGANotSupported_WithCountervailingApplicable_PopulatesOnSave()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var consignment3 = clearance.CusUSLVConsignments.AddNew();

			var item1a = consignment1.CusUSLVItems.AddNew();
			var item1b = consignment1.CusUSLVItems.AddNew();
			var item2a = consignment2.CusUSLVItems.AddNew();
			var item2b = consignment2.CusUSLVItems.AddNew();
			var item3a = consignment3.CusUSLVItems.AddNew();
			var item3b = consignment3.CusUSLVItems.AddNew();

			item1a.ULI_Countervailing = true;
			item1b.ULI_Countervailing = true;
			item2a.ULI_Countervailing = true;
			item2b.ULI_Countervailing = false;
			item3a.ULI_Countervailing = false;
			item3b.ULI_Countervailing = false;

			CombineAssertions("PGA Not Supported not set before saving", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", false, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", false, consignment3.ULB_PGANotSupported);
			});

			Factory.Save();

			CombineAssertions("PGA Not Supported set for Consignment with any items with Countervailing applicable", () =>
			{
				AssertEquals("Consignment1", false, consignment1.ULB_PGANotSupported);
				AssertEquals("Consignment2", true, consignment2.ULB_PGANotSupported);
				AssertEquals("Consignment3", true, consignment3.ULB_PGANotSupported);
			});
		}

		USCTariff CreateTariff(string tariffNumber, string pgaCodes)
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDate.Today;
			tariff.UE_PGACodes = pgaCodes;
			return tariff;
		}

		#region IACECargoReleaseHeader

		public void TestDeclarationReferenceNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_JobNumber = "S001";
			AssertEquals("Job Number", "S001", ((IACECargoReleaseHeader)consignment).DeclarationReferenceNumber);
		}

		public void TestKnownImporterIndicator()
		{
			var org = Factory.New<OrgHeader>();
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_OH_Importer = org.PK;

			AssertEquals("Known Importer Indicator", false, ((IACECargoReleaseHeader)consignment).KnownImporterIndicator);

			shipment.IORWrapper.ZO_KnwImpInd = YesNoDefaultList.Codes.Yes;

			AssertEquals("Known Importer Indicator", true, ((IACECargoReleaseHeader)consignment).KnownImporterIndicator);
		}

		public void TestRailReferenceNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			var entryNum = consignment.CE_RailReferenceNumber;

			AssertEquals("Rail Reference Number", entryNum, ((IACECargoReleaseHeader)consignment).RailReferenceNumber);
		}

		public void TestImporterOfRecordType()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignmentWithPGA = shipment.CusUSLVConsignments.AddNew();
			var consignmentWithoutPGA = shipment.CusUSLVConsignments.AddNew();

			var pgaItem = consignmentWithPGA.CusUSLVItems.AddNew();
			consignmentWithPGA.CusUSLVItems.AddNew();
			consignmentWithoutPGA.CusUSLVItems.AddNew();
			consignmentWithoutPGA.CusUSLVItems.AddNew();

			var pga = pgaItem.CusUSLVItemPGAs.AddNew();
			pga.ULP_Agency = "EPA";
			pga.ULP_AgencyProgram = GovernmentAgencyProgramCodeList.Codes.ODS;
			pga.ULP_DisclaimReason = "C";
			pga.ULP_Indicator = "C";

			shipment.ULH_IORType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			AssertEquals("Importer Of Record Type", EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber, ((IACECargoReleaseHeader)consignmentWithPGA).ImporterOfRecordType);

			shipment.ULH_IORType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			AssertEquals("Importer Of Record Type", EntityIdentifierQualifierList.Codes.CBPAssignedNumber, ((IACECargoReleaseHeader)consignmentWithPGA).ImporterOfRecordType);

			shipment.ULH_IORType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertEquals("Importer Of Record Type", EntityIdentifierQualifierList.Codes.SocialSecurityNumber, ((IACECargoReleaseHeader)consignmentWithPGA).ImporterOfRecordType);

			AssertEquals("IOR Type should be empty if consignment has no Item's with PGA", string.Empty, ((IACECargoReleaseHeader)consignmentWithoutPGA).ImporterOfRecordType);

			consignmentWithoutPGA.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertEquals("Importer Of Record Type", EntityIdentifierQualifierList.Codes.SocialSecurityNumber, ((IACECargoReleaseHeader)consignmentWithoutPGA).ImporterOfRecordType);
		}

		public void TestPortOfUnlading()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			shipment.ULH_PortOfDischarge = "3786";

			AssertEquals("Port Of Unlading", "3786", ((IACECargoReleaseHeader)consignment).PortOfUnlading);
		}

		public void TestEstimatedDateOfArrivalForEntryType86()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			shipment.ULH_DischargeDate = ZDate.BrettsBirthday;

			AssertEquals("Estimated Date Of Arrival For Entry Type 86", ZDate.BrettsBirthday, ((IACECargoReleaseHeader)consignment).EstimatedDateOfArrivalForEntryType86);
		}

		#endregion

		#region ICusEntryHeader

		public void TestIsRemoteLocationFiling()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_RemoteLocationFiling = true;
			AssertEquals("Is Remote Location Filing", true, ((ICusEntryHeader)consignment).IsRemoteLocationFiling);

			shipment.ULH_RemoteLocationFiling = false;
			AssertEquals("Is Remote Location Filing", false, ((ICusEntryHeader)consignment).IsRemoteLocationFiling);
		}

		public void TestEntryNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			AssertNull(consignment.ENSEntryNumber);
			consignment.CE_EntryNum = "123456";

			AssertNotNull(consignment.ENSEntryNumber);
			AssertEquals("Entry Number", "123456", ((ICusEntryHeader)consignment).EntryNumber);
		}

		public void TestOTHEntryNumber_OnlyCreateNewWhenSetRailReferenceNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			AssertNull(consignment.OTHEntryNumber);

			consignment.CE_RailReferenceNumber = "AAA";
			AssertNotNull(consignment.OTHEntryNumber);
		}

		public void TestOTHEntryNumber_IfNumberAlreadyExist_WouldNotCreateNewWhenSetRailReferenceNumber()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();

			var entryNumber = CusEntryNumber.New(consignment, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Core.Constants.CountryCodes.UnitedStates);
			entryNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNumber.CE_EntryIsSystemGenerated = true;

			Factory.Save();

			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, consignment.PK);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.UnitedStates);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_Category, SQLComparisonOperator.Equal, CusEntryNumber.Categories.AdditionalReferenceNumber);
			var othEntryNumber = Factory.Load<CusEntryNumber>(filter);

			AssertEquals("pre condition: one CusEntryNumber loaded", 1, othEntryNumber.Length);

			consignment.CE_RailReferenceNumber = "RailReference";

			othEntryNumber = Factory.Load<CusEntryNumber>(filter);
			AssertEquals("Shouldn't create new CusEntryNumber", 1, othEntryNumber.Length);
		}

		public void TestImportingVesselName()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_ConveyanceName = "TITANIC";
			AssertEquals("Is Remote Location Filing", "TITANIC", ((ICusEntryHeader)consignment).ImportingVesselName);
		}

		public void TestVoyageNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			shipment.ULH_TransportMode = TransportTypeList.Codes.Mail;
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_VoyageFlightNo = "123-456";
			AssertEquals("Voyage Number", "12345", ((ICusEntryHeader)consignment).VoyageNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Voyage Number", "12345", ((ICusEntryHeader)consignment).VoyageNumber);

			shipment.ULH_VoyageFlightNo = "5X215";
			AssertEquals("Voyage Number", "215", ((ICusEntryHeader)consignment).VoyageNumber);

			shipment.ULH_VoyageFlightNo = "AA019";
			AssertEquals("Voyage Number", "019", ((ICusEntryHeader)consignment).VoyageNumber);
		}

		public void TestIsACECargoReleaseCertification()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			Assert("Is ACE Cargo Release Certification", ((ICusEntryHeader)consignment).IsACECargoReleaseCertification);
		}

		public void TestHeaderIsNonAMS()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_NonAMSIndicator = true;
			Assert("Is Non AMS", ((ICusEntryHeader)consignment).IsNonAMS);
		}

		public void TestDistrictPortOfUnlading()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_PortOfDischarge = "3786";
			AssertEquals("District Port Of Unlading", "3786", ((ICusEntryHeader)consignment).DistrictPortOfUnlading);
		}

		public void TestDateOfImportation()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			var date = new ZDate(2019, 10, 24);
			shipment.ULH_DischargeDate = date;
			AssertEquals("Date Of Importation", date, ((ICusEntryHeader)consignment).DateOfImportation);
		}

		public void TestLowestBillDetails()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			AssertEquals("Lowest Bill Details", 1, ((ICusEntryHeader)consignment).LowestBillDetails.Count());
			AssertEquals("Lowest Bill Details", consignment, ((ICusEntryHeader)consignment).LowestBillDetails.First());
		}

		public void TestIsSplitShipment()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			AssertEquals("Is Split Shipment", ZBool.False, ((ICusEntryHeader)consignment).IsSplitShipment);
		}

		public void TestEntryLines()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			var item1 = consignment.CusUSLVItems.AddNew();
			var item2 = consignment.CusUSLVItems.AddNew();

			var entryLines = ((ICusEntryHeader)consignment).EntryLines;

			AssertEquals("Entry Lines", 2, entryLines.Count());
			Assert("1 Entry Line no1", entryLines.Any(x => x.CL_LineNumber == 1));
			Assert("1 Entry Line no2", entryLines.Any(x => x.CL_LineNumber == 2));
		}

		public void TestPreparerDistrictPort()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_PreparerDistrictPort = "1101";
			AssertEquals("1101", ((ICusEntryHeader)consignment).PreparerDistrictPort);
		}

		public void TestPreparerFilerCode()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_EntryFilerCode = "SV9";
			AssertEquals("SV9", ((ICusEntryHeader)consignment).PreparerFilerCode);
		}

		public void TestPreparerOfficeCode()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_PreparerOfficeCode = "01";
			AssertEquals("01", ((ICusEntryHeader)consignment).PreparerOfficeCode);
		}

		#endregion

		#region IHeaderCommon

		public void TestEntryType()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			AssertEquals("Entry Type", EntryTypeList.Codes.LowValue, ((IHeaderCommon)consignment).EntryType);
		}

		public void TestIsEntryTypeInformalFreeDutiable()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			Assert(!consignment.IsEntryTypeInformalFreeDutiable);

			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			Assert(consignment.IsEntryTypeInformalFreeDutiable);
		}

		public void TestImporterOfRecordNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			shipment.ULH_IORReference = "A";

			var consignmentWithPGA = shipment.CusUSLVConsignments.AddNew();
			var consignmentWithoutPGA = shipment.CusUSLVConsignments.AddNew();

			var pgaItem = consignmentWithPGA.CusUSLVItems.AddNew();
			consignmentWithPGA.CusUSLVItems.AddNew();
			consignmentWithoutPGA.CusUSLVItems.AddNew();
			consignmentWithoutPGA.CusUSLVItems.AddNew();

			var pga = pgaItem.CusUSLVItemPGAs.AddNew();
			pga.ULP_Agency = "EPA";
			pga.ULP_AgencyProgram = GovernmentAgencyProgramCodeList.Codes.ODS;
			pga.ULP_DisclaimReason = "C";
			pga.ULP_Indicator = "C";

			AssertEquals("Importer Of Record Number", "A", ((IHeaderCommon)consignmentWithPGA).ImporterOfRecordNumber);
			AssertEquals("IOR Number should be empty if consignment has not Items with PGA", string.Empty, ((IHeaderCommon)consignmentWithoutPGA).ImporterOfRecordNumber);

			consignmentWithoutPGA.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertEquals("Importer Of Record Number", "A", ((IHeaderCommon)consignmentWithoutPGA).ImporterOfRecordNumber);
		}

		public void TestModeOfTransportationCode()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_TransportMode = TransportTypeList.Codes.Air;
			shipment.ULH_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;

			var modeOfTransport = TransportModeCalculator.CalculateUSTransportMode(shipment.ULH_TransportMode, shipment.ULH_ContainerMode);

			AssertEquals("Importer Of Record Number", modeOfTransport, ((IHeaderCommon)consignment).ModeOfTransportationCode);
		}

		public void TestBondType()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			AssertEquals("Bond Type", "0", ((IHeaderCommon)consignment).BondType);

			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertEquals("Bond Type", "8", ((IHeaderCommon)consignment).BondType);
		}

		public void TestDistrictPortOfEntry()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_PortOfEntry = "2704";

			AssertEquals("District Port Of Entry", "2704", ((IHeaderCommon)consignment).DistrictPortOfEntry);
		}

		public void TestTotalValueOfEntrySummary()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_GoodsValue = 10m;
			item1.ULI_RX_NKCurrency = "USD";
			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_GoodsValue = 10m;
			item2.ULI_RX_NKCurrency = "USD";

			AssertEquals("Total Value Of Entry Summary", 20m, ((ICusEntryHeader)consignment).TotalValueOfEntrySummary);
		}

		#endregion

		#region IBillDetails

		public void TestITNumberAndITDate()
		{
			var consignment = GetNewBusinessObjectForDeleteTest(Factory) as CusUSLVConsignment;
			var inBondDetail = Factory.LoadTop1<ITAndSplitDetails>(new ZQuery(CusAddInfoSchema.B7_ParentID, consignment.PK));
			AssertNull("No IT Number record exists", inBondDetail);

			consignment.ITNumber = "Test0001";
			consignment.ITDate = new ZDate(2024, 04, 19);
			AssertEquals(1, consignment.ITAndSplitDetails.Count);
			AssertEquals("Test0001", consignment.ITAndSplitDetails[0].US_ITNumber);
			AssertEquals("IT Number", "Test0001", ((IBillDetails)consignment).ITNumber);
			AssertEquals(new ZDateTime(2024, 04, 19), consignment.ITAndSplitDetails[0].US_ITDate);
			AssertEquals("IT Date", new ZDate(2024, 04, 19), ((IBillDetails)consignment).ITDate);
			inBondDetail = Factory.LoadTop1<ITAndSplitDetails>(new ZQuery(CusAddInfoSchema.B7_ParentID, consignment.PK));
			AssertNotNull("IT Number record exists", inBondDetail);

			consignment.Delete();
			Assert("IT Number record should be deleted when consignment is deleted", inBondDetail.IsDeleted);
		}

		public void TestMasterBillNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_MasterBill = "BILL1";

			AssertEquals("Master Bill Number", "BILL1", ((IBillDetails)consignment).MasterBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Truck;
			consignment.ULB_HouseBill = "HB001";
			AssertEquals("House bill will replace master bill when shipment transport mode is truck.", "HB001", ((IBillDetails)consignment).MasterBillNumber);
		}

		public void TestIssuerCodeOfMasterBillNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_MasterBillIssuerSCAC = "YOU";
			consignment.ULB_HouseBillIssuerSCAC = "THE";

			AssertEquals("Issuer Code Of Master Bill Number", "YOU", ((IBillDetails)consignment).IssuerCodeOfMasterBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("SCAC will be empty when shipment transport mode is air", ZString.Empty, ((IBillDetails)consignment).IssuerCodeOfMasterBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("SCAC will be empty when shipment transport mode is road", ZString.Empty, ((IBillDetails)consignment).IssuerCodeOfMasterBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("SCAC will be empty when shipment transport mode is mail", ZString.Empty, ((IBillDetails)consignment).IssuerCodeOfMasterBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("SCAC will be from house bill", "THE", ((IBillDetails)consignment).IssuerCodeOfMasterBillNumber);
		}

		public void TestHouseBillNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_HouseBill = "BILL1";

			AssertEquals("House Bill Number", "BILL1", ((IBillDetails)consignment).HouseBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("House bill will be emptry, and it will be represent as master bill.", ZString.Empty, ((IBillDetails)consignment).HouseBillNumber);
		}

		public void TestHouseBillNumber_DoesNotIncludeSpecialCharacters()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_HouseBill = "BILL1!@#$%^&*";

			AssertEquals("House Bill Number", "BILL1", ((IBillDetails)consignment).HouseBillNumber);
		}

		public void TestIssuerCodeOfHouseBillNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_HouseBillIssuerSCAC = "YOU";

			AssertEquals("Issuer Code Of House Bill Number", "YOU", ((IBillDetails)consignment).IssuerCodeOfHouseBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("SCAC will be empty when shipment transport mode is air", ZString.Empty, ((IBillDetails)consignment).IssuerCodeOfHouseBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("SCAC will be empty when shipment transport mode is road", ZString.Empty, ((IBillDetails)consignment).IssuerCodeOfHouseBillNumber);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("SCAC will be empty when shipment transport mode is mail", ZString.Empty, ((IBillDetails)consignment).IssuerCodeOfHouseBillNumber);
		}

		public void TestPackageQuantity()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_NumberOfPacks = 1;

			AssertEquals("Package Quantity", 1, ((IBillDetails)consignment).PackageQuantity);
		}

		public void TestBillIsNonAMS()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_NonAMSIndicator = true;

			AssertEquals("Is Non AMS", true, ((IBillDetails)consignment).IsNonAMS);
		}

		public void TestIsSplit()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			AssertEquals("Is Split", false, ((IBillDetails)consignment).IsSplit);
		}

		public void TestConveyanceOrSplitDetails()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			AssertEquals("Conveyance Or Split Details", 1, ((IBillDetails)consignment).ConveyanceOrSplitDetails.Count());
			AssertEquals("Conveyance Or Split Details", consignment, ((IBillDetails)consignment).ConveyanceOrSplitDetails.First());
		}

		public void TestContainers()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();
			AssertEquals("Containers", 0, ((IBillDetails)consignment).Containers.Count());

			consignment.ULB_EquipmentNumber = "EQP1";
			AssertEquals("Containers", 1, ((IBillDetails)consignment).Containers.Count());
			AssertEquals("Containers", consignment, ((IBillDetails)consignment).Containers.First());
		}

		#endregion

		#region IConveyanceOrSplitDetails

		public void TestArrivalDate()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			var date = new ZDate(2019, 10, 18);

			shipment.ULH_DischargeDate = date;

			AssertEquals("Arrival Date", date, ((IConveyanceOrSplitDetails)consignment).ArrivalDate);
		}

		public void TestFlightNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_VoyageFlightNo = "1234567890";

			AssertEquals("Flight Number", "12345", ((IConveyanceOrSplitDetails)consignment).FlightNumber);
		}

		public void TestQty()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_NumberOfPacks = 42;

			AssertEquals("Qty", 42, ((IConveyanceOrSplitDetails)consignment).Qty);
		}

		public void TestUQ()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_PackType = "KG";

			AssertEquals("UQ", "KG", ((IConveyanceOrSplitDetails)consignment).UQ);
		}

		public void TestCarrierCode()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			shipment.ULH_TransportMode = TransportTypeList.Codes.Air;
			var consignment = shipment.CusUSLVConsignments.AddNew();

			shipment.ULH_CarrierSCAC = "AA";

			AssertEquals("Carrier Code", "AA", ((IConveyanceOrSplitDetails)consignment).CarrierCode);

			shipment.ULH_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("Carrier Code", "AA", ((IConveyanceOrSplitDetails)consignment).CarrierCode);

			shipment.ULH_CarrierSCAC = "";
			AssertEquals("Carrier Code", Bill.Constants.N_A, ((IConveyanceOrSplitDetails)consignment).CarrierCode);
		}

		#endregion

		#region IContainer

		public void TestContainerNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignment = shipment.CusUSLVConsignments.AddNew();

			consignment.ULB_EquipmentNumber = "24";

			AssertEquals("Container Number", "24", ((US.Business.MessageBuilders.IContainer)consignment).ContainerNumber);
		}

		#endregion

		#region IEdocsProvider

		public void TestIEDocsProvider()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var docProvider = consignment as IEDocsProvider;
			AssertNotNull("Precondition: Consignment should implement IEDocsProvider", docProvider);

			AssertNotNull(docProvider.DocManagerInfo);
			AssertEquals("DocManager Code:", Core.Constants.DocManagerCodes.USLowValueEntriesConsignmentBill, docProvider.DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region IEntryHeaderParentBusinessObject

		public void TestLinkedObject()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;
			var bo = tempInterface.LinkedObject as CusUSLVConsignment;

			AssertNotNull(bo);
			AssertEquals(consignment.PK, bo.PK);
		}

		public void TestEntrySummaryEntryAndCargoReleaseEntry()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;

			var entry = tempInterface.EntrySummaryEntry;
			AssertNull(entry);

			entry = tempInterface.CargoReleaseEntry;
			AssertNull(entry);
		}

		public void TestSimplifiedEntry()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;
			var entry = tempInterface.SimplifiedEntry;

			AssertType<CusUSLVConsignment>(entry);
			AssertEquals(consignment.PK, ((CusUSLVConsignment)entry).PK);
		}

		public void TestReferenceNumber()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;

			clearance.ULH_JobNumber = "SEC000001";
			clearance.ULH_EntryFilerCode = "SV9";
			var entryNum = consignment.CE_EntryNum;

			AssertEquals("SEC000001 / SV9--" + entryNum, tempInterface.ReferenceNumber);
		}

		public void TestRegistryCompanyPK()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;

			AssertEquals(clearance.Branch.GB_GC.ToGuid(), tempInterface.RegistryCompanyPK);
		}

		public void TestRegistryBranchPK()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;

			AssertEquals(clearance.Branch.PK.ToGuid(), tempInterface.RegistryBranchPK);
		}

		public void TestCusAgent()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;

			AssertNull(tempInterface.CusAgent);
		}

		public void TestDispositionCodesInIEntryHeaderParentBusinessObject()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			var dispositionCode01 = consignment.DispositionCodes.AddNew();
			var dispositionCode02 = consignment.DispositionCodes.AddNew();

			var tempInterface = consignment as IEntryHeaderParentBusinessObject;
			var codes = tempInterface.DispositionCodes;

			AssertEquals(2, codes.Count);
		}

		public void TestReleaseStatus()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;

			var tempInterface = consignment as IEntryHeaderParentBusinessObject;

			AssertEquals(CRLReleaseStatusList.Codes.REL, tempInterface.ReleaseStatus);
		}

		public void TestReleaseDate()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;

			consignment.CE_IssueDate = new ZDateTime(2019, 12, 02);
			AssertEquals("02-Dec-19 00:00:00", tempInterface.ReleaseDateTime.ToString());

			tempInterface.ReleaseDateTime = new ZDateTime(2019, 11, 02);
			AssertEquals("02-Nov-19 00:00:00", tempInterface.ReleaseDateTime.ToString());
		}

		public void TestShouldUpdateDeclarationWithCargoReleaseResults()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;
			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(tempInterface.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);

			Assert(tempInterface.ShouldUpdateDeclarationWithCargoReleaseResults);

			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(tempInterface.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			Assert(!tempInterface.ShouldUpdateDeclarationWithCargoReleaseResults);
		}

		public void TestUpdateMessageProcessingObjectAfterReleased()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as IEntryHeaderParentBusinessObject;
			tempInterface.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(tempInterface.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);

			var block10 = new ASESSO10();
			block10.DistrictPortOfEntry = "3786";
			block10.CarrierCode = "TEST";
			block10.VoyageFlightTripManifestNumber = "TESTV";
			block10.EstimatedDateOfArrival = new ZDate(2019, 12, 12);

			var block40List = new List<ASESSO40> { new ASESSO40() };
			var block50List = new List<ASESSO50> { new ASESSO50() };

			tempInterface.UpdateMessageLinkedParentBOAfterReleased(block10, block40List, block50List);

			AssertEquals("3786", clearance.ULH_PortOfEntry);
			AssertEquals("TEST", clearance.ULH_CarrierSCAC);
			AssertEquals("TESTV", clearance.ULH_VoyageFlightNo);
			AssertEquals("12-Dec-19", clearance.ULH_DischargeDate.ToString());
		}

		#endregion

		#region ISimplifiedMessageLinkedObject

		public void TestLinkedObjectInISimplifiedMessageLinkedObject()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as ISimplifiedMessageLinkedObject;
			var bo = tempInterface.LinkedObject as CusUSLVConsignment;

			AssertNotNull(bo);
			AssertEquals(consignment.PK, bo.PK);
		}

		public void TestParentBusinessObject()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as ISimplifiedMessageLinkedObject;

			var parentObject = tempInterface.ParentBusinessObject as CusUSLVConsignment;
			AssertNotNull(parentObject);
			AssertEquals(consignment.PK, parentObject.PK);
		}

		public void TestIsCargoReleaseBeingCertified()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as ISimplifiedMessageLinkedObject;
			Assert(!tempInterface.IsCargoReleaseBeingCertified);
		}

		public void TestIsFormalEntry()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as ISimplifiedMessageLinkedObject;
			Assert(!tempInterface.IsFormalEntry);
		}

		public void TestIsBorderCargoRelease()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as ISimplifiedMessageLinkedObject;
			Assert(!tempInterface.IsBorderCargoRelease);
		}

		public void TestIsCargoRelease()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as ISimplifiedMessageLinkedObject;
			Assert(!tempInterface.IsCargoRelease);
		}

		public void TestIsACECargoRelease()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as ISimplifiedMessageLinkedObject;
			Assert(tempInterface.IsACECargoRelease);
		}

		public void TestEntryFilerCode()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var tempInterface = consignment as ISimplifiedMessageLinkedObject;

			clearance.ULH_EntryFilerCode = "ABC";
			AssertEquals("Entry Filer Code.", "ABC", tempInterface.EntryFilerCode);
		}

		public void TestGetMSCEventReferenceForCargoReleaseResponse()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "XJ5";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryNum = "71002057";
			consignment.ULB_HouseBill = "SECBH00000000002";
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;

			var messageLinkedObject = consignment as ISimplifiedMessageLinkedObject;
			AssertEquals("|CRF=XJ571002057|RFN=SECBH00000000002|TYP=SX|STA=CSA", messageLinkedObject.GetMSCEventReferenceForCargoReleaseResponse(message));
		}

		#endregion

		#region SimplifiedEntryStatusNotificationProcessor

		public void TestUseLowValueEntriesReleaseMessagesRegistrySetting_WhenSendSimplifiedEntryStatusNotificationEmailForLowValueEntries()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "SV9";
			clearance.ULH_TransportMode = "TRK";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryNum = "71002057";

			DeclarationTestHelper.SetupForSendMessage();

			AssertEmailControlledByLowValueEntriesReleaseMessageRegistrySetting(new ManifestGroupNotification("NOE", ZGuid.Empty, false), (email) => { AssertNull(email); });
			AssertEmailControlledByLowValueEntriesReleaseMessageRegistrySetting(new ManifestGroupNotification("ESG", Core.Constants.Groups.PostMastersGroupPK, false), (email) => { AssertNotNull(email); });

			void AssertEmailControlledByLowValueEntriesReleaseMessageRegistrySetting(ManifestGroupNotification manifestGroupNotification, Action<EmailDef> assertAction)
			{
				var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));

				Factory.Save();

				using (USCustomsDataRegistry.Instance.LowValueEntriesReleaseMessages.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, manifestGroupNotification))
				{
					Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
					new ABIIncomingMessageProcessor().ExecuteBatch();

					var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
					{ return emailToMatched.Subject.Contains("ACE Cargo Release Status Response"); }));

					assertAction(email);
				}
			}
		}

		public void TestUpdateConsignmentUsingSimplifiedEntryStatusNotificationProcessor()
		{
			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "SV9";
			clearance.ULH_TransportMode = "TRK";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryNum = "71002057";

			var bill = Factory.NewWithValidTestData<Bill>();
			bill.US_UI_NKBillIssuerSCAC = "C2";
			bill.CU_BillNum = "PI3151202078";
			bill.CU_BillType = "HB";
			bill.CU_NoOfPacks = 10;
			bill.CU_PackType = "AE";
			consignment.ULB_HouseBill = bill.CU_BillNum;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			orgHeader.OH_FullName = "Test Organisation";
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "Test Address.";
			consignment.ULB_OA_Consignee = orgAddress.PK;

			DeclarationTestHelper.SetupForSendMessage();
			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			USCustomsDataRegistry.Instance.LowValueEntriesReleaseMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, Core.Constants.Groups.PostMastersGroupPK, false));
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reLoadJob = new BusinessObjectFactory().Load<CusUSLVConsignment>(consignment.PK);
			var reloadBill = Factory.Load<Bill>(bill.PK);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("ACE Cargo Release Status Response"); }));

			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("Release Date.", "20-Jan-16 00:00:00", ((IEntryHeaderParentBusinessObject)reLoadJob).ReleaseDateTime.ToString());
				AssertContains("District Port of Entry.", "<tr><td>District Port of Entry</td><td>4601</td></tr>", email.Body);
				AssertContains("Entry Filer Code.", "<tr><td>Entry Filer Code</td><td>SV9</td></tr>", email.Body);
				AssertContains("Entry Number.", "<tr><td>Entry Number</td><td>71002057</td></tr>", email.Body);
				AssertContains("Entry Type.", "<tr><td>Entry Type</td><td>01</td></tr>", email.Body);
				AssertContains("Importer of Record Number.", "<tr><td>Importer of Record Number</td><td>47-299933300</td></tr>", email.Body);
				AssertContains("Carrier Code.", "<tr><td>Carrier Code</td><td>HLCU</td></tr>", email.Body);
				AssertContains("Estimated Date of Arrival.", "<tr><td>Estimated Date of Arrival</td><td>22-Jan-16</td></tr>", email.Body);
				AssertContains("Importing Conveyance Name.", "<tr><td>Importing Conveyance Name</td><td>ZIM TARRAGONA</td></tr>", email.Body);
				AssertEquals("bill1.CU_NoOfPacks is not updated for the original value is not zero", 10m, reloadBill.CU_NoOfPacks);
				AssertEquals("bill1.CU_PackType is not updated for the original value is not empty", "AE", reloadBill.CU_PackType);
			});
		}

		public void TestMSCEventReferenceAfterChangeLVSConsignmentEntryStatus()
		{
			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "SV9";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryNum = "71002057";

			var bill = Factory.NewWithValidTestData<Bill>();
			bill.CU_BillNum = "PI3151202078";
			consignment.ULB_HouseBill = bill.CU_BillNum;

			var mscEvent = Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_Code, Events.MessageStatusChange.Code)).FirstOrDefault();
			mscEvent.SE_ReferenceFormat = "<EVENT><If(DEP != \"\", \" by <DEP>\", \"\")><If(MST != \"\", \" from <MST>\", \"\")><If(TYP != \"\", \": <TYP>\", \"\")><If(RFN != \"\", \", Reference No. <RFN>,\", \"\")><If(CRF != \"\", \" <CRF>,\", \"\")><If(OLD != \"\", \" from <OLD>\", \"\")><If(OLD == \"\" && NEW != \"\", \" <NEW>\", \"\")><If(OLD != \"\" && NEW != \"\", \" to <NEW>\", \"\")><If(LOC != \"\", \" at <CityCountry(LOC)>\", \"\")><If(EQN != \"\", \", <EQN>\", \"\")><If(FAC != \"\", \" at <FAC>\", \"\")><If(VFL!=\"\",\", <VFL>\",\"\")><If(FDT!=\"\",\", <datetime.Parse(FDT).Format(\"dd-MMM-yy\")>\",\"\")><If(RES != \"\", \" because <RES>\", \"\")><If(REF != \"\", \", <REF>\", \"\")>";

			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var msc = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageStatusChangeCode)).FirstOrDefault();
			AssertNotNull(msc);
			AssertEquals("|CRF=SV971002057|MST=Cargo Release|NEW=REL|RFN=PI3151202078|TYP=SO", msc.SL_Reference);
			AssertEquals("Message: Status Change from Cargo Release: SO, Reference No. PI3151202078, SV971002057, REL", msc.DisplayEventReference);
		}

		public void TestLogPCSInMSCEventReferenceWhenJobIsCreatedFromHVLV()
		{
			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "SV9";
			clearance.ULH_UseCode = LVSConstants.ETailUseCode;

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryNum = "71002057";

			var bill = Factory.NewWithValidTestData<Bill>();
			bill.CU_BillNum = "PI3151202078";
			consignment.ULB_HouseBill = bill.CU_BillNum;

			var mscEvent = Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_Code, Events.MessageStatusChange.Code)).FirstOrDefault();
			mscEvent.SE_ReferenceFormat = "<EVENT><If(DEP != \"\", \" by <DEP>\", \"\")><If(MST != \"\", \" from <MST>\", \"\")><If(TYP != \"\", \": <TYP>\", \"\")><If(RFN != \"\", \", Reference No. <RFN>,\", \"\")><If(CRF != \"\", \" <CRF>,\", \"\")><If(OLD != \"\", \" from <OLD>\", \"\")><If(OLD == \"\" && NEW != \"\", \" <NEW>\", \"\")><If(OLD != \"\" && NEW != \"\", \" to <NEW>\", \"\")><If(LOC != \"\", \" at <CityCountry(LOC)>\", \"\")><If(EQN != \"\", \", <EQN>\", \"\")><If(FAC != \"\", \" at <FAC>\", \"\")><If(VFL!=\"\",\", <VFL>\",\"\")><If(FDT!=\"\",\", <datetime.Parse(FDT).Format(\"dd-MMM-yy\")>\",\"\")><If(RES != \"\", \" because <RES>\", \"\")><If(REF != \"\", \", <REF>\", \"\")>";

			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var msc = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageStatusChangeCode)).FirstOrDefault();
			AssertNotNull(msc);
			AssertEquals("|CRF=SV971002057|MST=Cargo Release|NEW=REL|RFN=PI3151202078|SER=PCS|TYP=SO", msc.SL_Reference);
			AssertEquals("Message: Status Change from Cargo Release: SO, Reference No. PI3151202078, SV971002057, REL", msc.DisplayEventReference);
		}

		public void TestMSCEventReferenceAfterCargoReleaseResponse()
		{
			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "SV9";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryNum = "73007229";

			var bill = Factory.NewWithValidTestData<Bill>();
			bill.CU_BillNum = "HSE01092020C";
			consignment.ULB_HouseBill = bill.CU_BillNum;

			var mscEvent = Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_Code, Events.MessageStatusChange.Code)).FirstOrDefault();
			mscEvent.SE_ReferenceFormat = "<EVENT><If(DEP != \"\", \" by <DEP>\", \"\")><If(MST != \"\", \" from <MST>\", \"\")><If(TYP != \"\", \": <TYP>\", \"\")><If(RFN != \"\", \", Reference No. <RFN>,\", \"\")><If(CRF != \"\", \" <CRF>,\", \"\")><If(OLD != \"\", \" from <OLD>\", \"\")><If(OLD == \"\" && NEW != \"\", \" <NEW>\", \"\")><If(OLD != \"\" && NEW != \"\", \" to <NEW>\", \"\")><If(LOC != \"\", \" at <CityCountry(LOC)>\", \"\")><If(EQN != \"\", \", <EQN>\", \"\")><If(FAC != \"\", \" at <FAC>\", \"\")><If(VFL!=\"\",\", <VFL>\",\"\")><If(FDT!=\"\",\", <datetime.Parse(FDT).Format(\"dd-MMM-yy\")>\",\"\")><If(RES != \"\", \" because <RES>\", \"\")><If(REF != \"\", \", <REF>\", \"\")>";

			var messageNum = "HYEDUSCMT_206678";
			var messageText = @"B  1101SV9SE                                               HYEDUSCMT_206678     
SE10ASV9  73007229 86               40000000000991101  1101                     
SE11                                   987                                      
SE13US CHICAGO FACILITATOR                  3125551212                          
SE15MAA  00100001111                                                    N       
SE15H    HSE01092020C                                      00000001     N       
SE17                                                                            
SE20CR SEC00000020                                                              
SE30SE ACE TEST SUPPLIER HK                                                     
SE3515172 GLOUCESTER ROAD                15WAN CHAI DISTRICT                    
SE36HONG KONG                                                  HK               
SE30CN ACE TEST IMPORTER 1                                                      
SE3515123 MADISON AVE                                                           
SE36NEW YORK                                    10016          US               
SE40001HK PLASTIC                                                               
SE6039209910000000000045                                                        
SE40002HK MORE STUFF                                                            
SE6039209920000000000054                                                        
Y  1101SV9SE".Replace("\r\n", "");

			var addMessage = CreateCargoReleaseMessage(messageNum, messageText);
			addMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			addMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			addMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
			addMessage.EM_SystemCreateUser = "TST";
			addMessage.EM_LinkTable = CusUSLVConsignmentSchema.Constants.TableName;
			addMessage.EM_LinkUniqueID = consignment.PK;

			var responseMessageText = @"B001101SV9SX                                               HYEDUSCMT_206678     
SE10ASV9  73007229 86               40000000000991101  1101                     
SE15MAA  00100001111                                                    N       
SE9011077BILL ISSUER CODE NOT ALLOWED                                           
SE15H    HSE01092020C                                      00000001     N       
SE20CR SEC00000020                                                              
SE9001   SE DATA REJECTED                                                       
Y  1101SV9SX00006".Replace("\r\n", "");

			var responseMessage = CreateCargoReleaseMessage(messageNum, responseMessageText);
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var msc = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageStatusChangeCode)).FirstOrDefault();
			AssertNotNull(msc);
			AssertEquals("|CRF=SV973007229|RFN=HSE01092020C|TYP=SX|STA=ESA", msc.SL_Reference);
			AssertEquals("Message: Status Change: SX, Reference No. HSE01092020C, SV973007229,", msc.DisplayEventReference);
		}

		MQEDIMessage CreateCargoReleaseMessage(ZString msgNum, ZString msgText)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageNum = msgNum;
			message.EM_MessageText = msgText;
			return message;
		}

		MQEDIMessage CreateStatusMessage(ZString msgText)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = msgText;
			return message;
		}

		#endregion

		#region IHaveAdditionalDataForBorderWise

		public void TestIHaveAdditionalDataForBorderWise()
		{
			var header = Factory.New<CusUSLVConsignment>();

			header.CusUSLVItems.AddNew();

			var addData = ((IHaveAdditionalDataForBorderWise)header).GetAdditionalDataForBorderWise(CusUSLVConsignment.Schema.FirstCusUSLVItemTariff);
			AssertEquals("2010.25.6032", addData.FormatBorderWiseInput("2010256032"));
		}

		#endregion
	}
}
