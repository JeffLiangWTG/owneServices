using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(MultiManifestBillSender))]
	class MultiManifestBillSenderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestData()
		{
			(AsycudaManifestHeader header1, AsycudaBill header1Bill1, AsycudaBill header1Bill2) = CreateManifest(Factory, "JOB3", "MB1");
			(AsycudaManifestHeader header2, AsycudaBill header2Bill1, AsycudaBill header2Bill2) = CreateManifest(Factory, "JOB1", "MB3");
			(AsycudaManifestHeader header3, AsycudaBill header3Bill1, AsycudaBill header3Bill2) = CreateManifest(Factory, "JOB2", "MB2");
			Factory.Save();
			var sender = new MultiManifestBillSender(new[] { header1Bill2, header2Bill1, header1Bill1, // missing header2Bill2
 header3Bill1, header3Bill2, header3Bill1 // duplicate header3Bill1
			});
			var masterDatas = sender.MasterDatas.ToArray();
			AssertEquals(3, masterDatas.Length);
			AssertMasterData(masterDatas[0], header2, new[] { header2Bill1 });
			AssertMasterData(masterDatas[1], header3, new[] { header3Bill2, header3Bill1 });
			AssertMasterData(masterDatas[2], header1, new[] { header1Bill2, header1Bill1 });
		}

		public static void AssertMasterData(MultiManifestBillSender.MasterData masterData, AsycudaManifestHeader header, AsycudaBill[] bills)
		{
			AssertEquals("masterData.DisplayValue", $"{header.AMA_JobReference} - {header.AMA_MasterBill}", masterData.DisplayValue);
			AssertEquals("masterData.HeaderPK", header.PK, masterData.HeaderPK);
			var billDatas = masterData.BillDatas.ToArray();
			AssertEquals("masterData.BillDatas.Length", bills.Length, billDatas.Length);
			for (var i = 0; i < bills.Length; i++)
			{
				AssertBillData(billDatas[i], bills[i]);
			}
		}

		public static void AssertBillData(MultiManifestBillSender.BillData billData, AsycudaBill bill)
		{
			AssertEquals("billData.DisplayValue", bill.ABL_BillNumber, billData.DisplayValue);
			AssertEquals("billData.BillPK", bill.PK, billData.BillPK);
		}

		public void TestSend()
		{
			SetupReferenceDataForSG(Factory);
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var data1 = CreateManifest(Factory, "JOB3", "MB1");
			var data2 = CreateManifest(Factory, "JOB1", "MB3");
			var data3 = CreateManifest(Factory, "JOB2", "MB2");
			var data5 = CreateManifest(Factory, "JOB5", "MB5");
			data3.header.AMA_Voyage = ZString.Empty;
			Factory.Save();
			var data4 = CreateManifest(Factory, "JOB4", "MB4");
			var sender = new MultiManifestBillSender(new[] { data1.bill2, data2.bill1, data1.bill1, // missing data2.bill2
 data3.bill1, data3.bill2, data3.bill1, // duplicate data3.bill1
 data4.bill1, data4.bill2, // not saved data4
 data5.bill1, data5.bill2 });
			sender.CycleDate = ZDateTime.Today.AddDays(1);
			sender.CycleNumber = "10";
			var progressData = new ZStringBuilder();
			sender.UpdateProgress = (string status, int percentComplete) =>
			{
				progressData.Append($"{status} - {percentComplete}");
				if (percentComplete == 80)
				{
					sender.CancelSendingTheRest(this, EventArgs.Empty);
				}
			};
			Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertMultilineASCIIEquals("Message Result", @"System was unable to load the following manifests:
JOB4 - MB4
You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs Global -> Global Manifest -> Send With Message Errors
The following manifests could not be sent due to errors:
JOB2 - MB2
The following manifests were sent successfully:
JOB1 - MB3
JOB3 - MB1
The following manifests sending were aborted:
JOB5 - MB5", sender.Send());
			data1 = GetDataInNewFactory(data1);
			AssertEquals(1, data1.header.Messages.Count);
			AssertCycleDetail(data1.bill1, ZDateTime.Today.AddDays(1), "10");
			AssertCycleDetail(data1.bill2, ZDateTime.Today.AddDays(1), "10");
			AddPacks(data1.bill1);
			AddPacks(data1.bill2);
			data1.header.Factory.Save();
			data2 = GetDataInNewFactory(data2);
			AssertEquals(1, data2.header.Messages.Count);
			AssertCycleDetail(data2.bill1, ZDateTime.Today.AddDays(1), "10");
			AssertCycleDetail(data2.bill2, ZDateTime.Empty, ZString.Empty);
			AddPacks(data2.bill1);
			AddPacks(data2.bill2);
			data2.header.Factory.Save();
			data3 = GetDataInNewFactory(data3);
			AssertEquals(0, data3.header.Messages.Count);
			AssertCycleDetail(data3.bill1, ZDateTime.Empty, ZString.Empty);
			AssertCycleDetail(data3.bill2, ZDateTime.Empty, ZString.Empty);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AddPacks(data3.bill1);
			AddPacks(data3.bill2);
			data3.header.Factory.Save();
			data5 = GetDataInNewFactory(data5);
			AddPacks(data5.bill1);
			AddPacks(data5.bill2);
			data5.header.Factory.Save();
			AssertEquals(0, data5.header.Messages.Count);
			AssertCycleDetail(data5.bill1, ZDateTime.Empty, ZString.Empty);
			AssertCycleDetail(data5.bill2, ZDateTime.Empty, ZString.Empty);
			AssertMultilineASCIIEquals("Progress data", @"Validating manifest 'JOB1 - MB3'. - 20
Sending manifest 'JOB1 - MB3'. - 20
Validating manifest 'JOB2 - MB2'. - 40
Validating manifest 'JOB3 - MB1'. - 60
Sending manifest 'JOB3 - MB1'. - 60
System was unable to load manifest 'JOB4 - MB4'. - 80", progressData.ToStringWithNewLineBetweenAppends());
			progressData = new ZStringBuilder();
			sender.CycleDate = ZDateTime.Today.AddDays(3);
			sender.CycleNumber = "15";
			Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertMultilineASCIIEquals("Message Result", @"System was unable to load the following manifests:
JOB4 - MB4
The following manifests could not be sent due to errors:
JOB1 - MB3
The following manifests were sent successfully:
JOB2 - MB2
JOB3 - MB1
The following manifests sending were aborted:
JOB5 - MB5", sender.Send());
			data1 = GetDataInNewFactory(data1);
			AssertEquals(2, data1.header.Messages.Count);
			AssertCycleDetail(data1.bill1, ZDateTime.Today.AddDays(3), "15");
			AssertCycleDetail(data1.bill2, ZDateTime.Today.AddDays(3), "15");
			data2 = GetDataInNewFactory(data2);
			AssertEquals(1, data2.header.Messages.Count);
			AssertCycleDetail(data2.bill1, ZDateTime.Today.AddDays(1), "10");
			AssertCycleDetail(data2.bill2, ZDateTime.Empty, ZString.Empty);
			data3 = GetDataInNewFactory(data3);
			AssertEquals(1, data3.header.Messages.Count);
			AssertCycleDetail(data3.bill1, ZDateTime.Today.AddDays(3), "15");
			AssertCycleDetail(data3.bill2, ZDateTime.Today.AddDays(3), "15");
			data5 = GetDataInNewFactory(data5);
			AssertEquals(0, data5.header.Messages.Count);
			AssertCycleDetail(data5.bill1, ZDateTime.Empty, ZString.Empty);
			AssertCycleDetail(data5.bill2, ZDateTime.Empty, ZString.Empty);
			AssertMultilineASCIIEquals("Last message", @"Manifest 'JOB3 - MB1' has the following message errors:
Goods Value: The Goods Value does not equal the sum of the Pack Lines Price. This may cause a valuation error for Singapore.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertMultilineASCIIEquals("Progress data", @"Validating manifest 'JOB1 - MB3'. - 20
Validating manifest 'JOB2 - MB2'. - 40
Sending manifest 'JOB2 - MB2'. - 40
Validating manifest 'JOB3 - MB1'. - 60
Sending manifest 'JOB3 - MB1'. - 60
System was unable to load manifest 'JOB4 - MB4'. - 80", progressData.ToStringWithNewLineBetweenAppends());
			progressData = new ZStringBuilder();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			sender.CycleDate = ZDateTime.Today.AddDays(2);
			sender.CycleNumber = "9";
			AssertMultilineASCIIEquals("Message Result", @"System was unable to load the following manifests:
JOB4 - MB4
The following manifests could not be sent due to errors:
The Bills chosen to send have no Packs requiring notification to Customs.
The Bills chosen to send have no Packs requiring notification to Customs.
The following manifests were sent successfully:
JOB1 - MB3
The following manifests sending were aborted:
JOB5 - MB5", sender.Send());
			data1 = GetDataInNewFactory(data1);
			AssertEquals(2, data1.header.Messages.Count);
			AssertCycleDetail(data1.bill1, ZDateTime.Today.AddDays(3), "15");
			AssertCycleDetail(data1.bill2, ZDateTime.Today.AddDays(3), "15");
			data2 = GetDataInNewFactory(data2);
			AssertEquals(2, data2.header.Messages.Count);
			AssertCycleDetail(data2.bill1, ZDateTime.Today.AddDays(2), "9");
			AssertCycleDetail(data2.bill2, ZDateTime.Empty, ZString.Empty);
			data3 = GetDataInNewFactory(data3);
			AssertEquals(1, data3.header.Messages.Count);
			AssertCycleDetail(data3.bill1, ZDateTime.Today.AddDays(3), "15");
			AssertCycleDetail(data3.bill2, ZDateTime.Today.AddDays(3), "15");
			data5 = GetDataInNewFactory(data5);
			AssertEquals(0, data5.header.Messages.Count);
			AssertCycleDetail(data5.bill1, ZDateTime.Empty, ZString.Empty);
			AssertCycleDetail(data5.bill2, ZDateTime.Empty, ZString.Empty);
			AssertMultilineASCIIEquals("Last message", @"Manifest 'JOB3 - MB1' has the following message errors:

Goods Value: The Goods Value does not equal the sum of the Pack Lines Price. This may cause a valuation error for Singapore.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertMultilineASCIIEquals("Progress data", @"Validating manifest 'JOB1 - MB3'. - 20
Sending manifest 'JOB1 - MB3'. - 20
Validating manifest 'JOB2 - MB2'. - 40
Sending manifest 'JOB2 - MB2'. - 40
Validating manifest 'JOB3 - MB1'. - 60
Sending manifest 'JOB3 - MB1'. - 60
System was unable to load manifest 'JOB4 - MB4'. - 80", progressData.ToStringWithNewLineBetweenAppends());
		}

		void AddPacks(AsycudaBill bill)
		{
			var newBillPack = bill.Packs.AddNew();
			newBillPack.LinePrice = 1m;
			newBillPack.LinePriceCurrency = Core.Constants.CurrencyCodes.Singapore;
			var newBillPackedItem = newBillPack.PackedItem;
			newBillPackedItem.GoodsType = Constants.GoodsType.MajorExporter;
			newBillPackedItem.API_CustomsQty = 1;
			newBillPackedItem.API_CustomsUQ = ValidCustomsUQ;
			newBillPackedItem.API_GoodsDescription = "Goods";
			newBillPackedItem.API_CustomsValue = 0m;
			newBillPackedItem.API_TaxAmount = 0m;
			newBillPackedItem.API_MessageStatus = "";
		}

		public void TestSendOnlySelectedBills()
		{
			SetupReferenceDataForSG(Factory);
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var manifest1 = CreateManifest(Factory, "MAN00001", "G40299");
			var manifestHeader = manifest1.header;
			var bill3 = AddBill(manifestHeader, "G82931", "Books");
			var bill4 = AddBill(manifestHeader, "758290", "Manuscripts");
			var bill5 = AddBill(manifestHeader, "G592248", "Manuals");
			var bill6 = AddBill(manifestHeader, "G0054288", "Documents");
			var bill7 = AddBill(manifestHeader, "4928542", "Brochures");
			var bill8 = AddBill(manifestHeader, "G84174", "Text Books");
			Factory.Save();
			var sender = new MultiManifestBillSender(new[] { manifest1.bill1, bill3, bill4, bill8 });
			sender.CycleDate = ZDateTime.Today.AddDays(1);
			sender.CycleNumber = "10";
			Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			sender.Send();
			var factory = new BusinessObjectFactory();
			var manifestHeaderReLoaded = factory.Load<AsycudaManifestHeader>(manifest1.header.PK);
			var manifestBill1 = (AsycudaBill)manifestHeaderReLoaded.Bills.FindByPK(manifest1.bill1.PK);
			var manifestBill2 = (AsycudaBill)manifestHeaderReLoaded.Bills.FindByPK(manifest1.bill2.PK);
			var manifestBill3 = (AsycudaBill)manifestHeaderReLoaded.Bills.FindByPK(bill3.PK);
			var manifestBill4 = (AsycudaBill)manifestHeaderReLoaded.Bills.FindByPK(bill4.PK);
			var manifestBill5 = (AsycudaBill)manifestHeaderReLoaded.Bills.FindByPK(bill5.PK);
			var manifestBill6 = (AsycudaBill)manifestHeaderReLoaded.Bills.FindByPK(bill6.PK);
			var manifestBill7 = (AsycudaBill)manifestHeaderReLoaded.Bills.FindByPK(bill7.PK);
			var manifestBill8 = (AsycudaBill)manifestHeaderReLoaded.Bills.FindByPK(bill8.PK);
			AssertEquals(1, manifestHeaderReLoaded.Messages.Count);
			AssertCycleDetail(manifestBill1, ZDateTime.Today.AddDays(1), "10");
			AssertCycleDetail(manifestBill2, ZDateTime.Empty, "");
			AssertCycleDetail(manifestBill3, ZDateTime.Today.AddDays(1), "10");
			AssertCycleDetail(manifestBill4, ZDateTime.Today.AddDays(1), "10");
			AssertCycleDetail(manifestBill5, ZDateTime.Empty, "");
			AssertCycleDetail(manifestBill6, ZDateTime.Empty, "");
			AssertCycleDetail(manifestBill7, ZDateTime.Empty, "");
			AssertCycleDetail(manifestBill8, ZDateTime.Today.AddDays(1), "10");
			var firstMessage = manifestHeaderReLoaded.Messages[0];
			var generatedMessage = firstMessage.EM_MessageText;
			AssertNotContains("Bill2 - G402991 - not sent", manifest1.bill2.ABL_BillNumber, generatedMessage);
			AssertEquals("", manifest1.bill2.ABL_MessageStatus);
			AssertNotContains("Bill5 - G592248 - not sent", bill5.ABL_BillNumber, generatedMessage);
			AssertEquals("", bill5.ABL_MessageStatus);
			AssertNotContains("Bill6 - G0054288 - not sent", bill6.ABL_BillNumber, generatedMessage);
			AssertEquals("", bill6.ABL_MessageStatus);
			AssertNotContains("Bill7 - 4928542 - not sent", bill7.ABL_BillNumber, generatedMessage);
			AssertEquals("", bill7.ABL_MessageStatus);
			AssertContains("Bill1 - G402992 - sent", manifest1.bill1.ABL_BillNumber, generatedMessage);
			AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, manifest1.bill1.ABL_MessageStatus);
			AssertContains("Bill3 - G82931 - sent", bill3.ABL_BillNumber, generatedMessage);
			AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, bill3.ABL_MessageStatus);
			AssertContains("Bill4 - 758290 - sent", bill4.ABL_BillNumber, generatedMessage);
			AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, bill4.ABL_MessageStatus);
			AssertContains("Bill8 - G84174 - sent", bill8.ABL_BillNumber, generatedMessage);
			AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, bill8.ABL_MessageStatus);
			Factory.Save();
			sender = new MultiManifestBillSender(new[] { manifest1.bill1, manifest1.bill2, bill3, bill4, bill5, bill6, bill7, bill8 });
			sender.CycleDate = ZDateTime.Today.AddDays(2);
			sender.CycleNumber = "15";
			Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			sender.Send();
			factory = new BusinessObjectFactory();
			var manifestHeaderReLoadedAgain = factory.Load<AsycudaManifestHeader>(manifest1.header.PK);
			var secondMessage = manifestHeaderReLoadedAgain.Messages[1].PK == firstMessage.PK ? manifestHeaderReLoadedAgain.Messages[0] : manifestHeaderReLoadedAgain.Messages[1];
			var secondGeneratedMessage = secondMessage.EM_MessageText;
			AssertNotContains("Bill1 - G402992 - already sent", manifest1.bill1.ABL_BillNumber, secondGeneratedMessage);
			AssertEquals("SNT", manifest1.bill2.ABL_MessageStatus);
			AssertNotContains("Bill3 - G82931 - already sent", bill3.ABL_BillNumber, secondGeneratedMessage);
			AssertEquals("SNT", bill5.ABL_MessageStatus);
			AssertNotContains("Bill4 - 758290 - already sent", bill4.ABL_BillNumber, secondGeneratedMessage);
			AssertEquals("SNT", bill6.ABL_MessageStatus);
			AssertNotContains("Bill8 - G84174 - already sent", bill8.ABL_BillNumber, secondGeneratedMessage);
			AssertEquals("SNT", bill7.ABL_MessageStatus);
			AssertContains("Bill2 - G402991 - now sent as well", manifest1.bill2.ABL_BillNumber, secondGeneratedMessage);
			AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, manifest1.bill2.ABL_MessageStatus);
			AssertContains("Bill5 - G592248 - now sent as well", bill5.ABL_BillNumber, secondGeneratedMessage);
			AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, bill5.ABL_MessageStatus);
			AssertContains("Bill6 - G0054288 - now sent as well", bill6.ABL_BillNumber, secondGeneratedMessage);
			AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, bill6.ABL_MessageStatus);
			AssertContains("Bill7 - 4928542 - now sent as well", bill7.ABL_BillNumber, secondGeneratedMessage);
			AssertEquals(ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, bill7.ABL_MessageStatus);
		}

		void AssertCycleDetail(AsycudaBill bill, ZDateTime cycleDate, ZString cycleNumber)
		{
			AssertEquals("CycleDate", cycleDate, bill.CycleDate);
			AssertEquals("CycleNumber", cycleNumber, bill.CycleNumber);
		}

		(AsycudaManifestHeader header, AsycudaBill bill1, AsycudaBill bill2) GetDataInNewFactory((AsycudaManifestHeader header, AsycudaBill bill1, AsycudaBill bill2) data)
		{
			var factory = new BusinessObjectFactory();
			var header = factory.Load<AsycudaManifestHeader>(data.header.PK);
			return (header, (AsycudaBill)header?.Bills.FindByPK(data.bill1.PK), (AsycudaBill)header?.Bills.FindByPK(data.bill2.PK));
		}

		public static void SetupReferenceDataForSG(BusinessObjectFactory factory)
		{
			var helper = new ZZDataTestHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, ValidCustomsUQ, "Keg", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.Port, "AUPR1", "desc1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.Port, "SGPR1", "desc2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("UEN", "11111111", "SG");
			factory.Save();
		}

		public const string ValidCustomsUQ = "K1";
		public static (AsycudaManifestHeader header, AsycudaBill bill1, AsycudaBill bill2) CreateManifest(BusinessObjectFactory factory, ZString jobRef, ZString manifestNumber)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Voyage = "QF1";
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_CustomsLoadPort = "AUPR1";
			header.AMA_RL_NKPortOfDischarge = "SGSIN";
			header.AMA_CustomsDischargePort = "SGPR1";
			header.AMA_JobReference = jobRef;
			header.AMA_MasterBill = manifestNumber;
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_ManifestType = SG.Access.Business.Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = manifestNumber + "2";
			bill1.ABL_FreightValue = 1;
			bill1.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Singapore;
			bill1.ABL_CustomsValue = 1m;
			bill1.TaxAmount = 1m;
			var pack = bill1.Packs.AddNew();
			pack.LinePrice = 1m;
			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Singapore;
			var packedItem = pack.PackedItem;
			packedItem.GoodsType = Constants.GoodsType.MajorExporter;
			packedItem.API_CustomsQty = 1;
			packedItem.API_CustomsUQ = ValidCustomsUQ;
			packedItem.API_GoodsDescription = "GOODS 1";
			packedItem.API_CustomsValue = 1m;
			packedItem.API_TaxAmount = 1m;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = manifestNumber + "1";
			bill2.ABL_FreightValue = 1;
			bill2.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Singapore;
			bill2.ABL_CustomsValue = 1m;
			bill2.TaxAmount = 1m;
			var pack2 = bill2.Packs.AddNew();
			pack2.LinePrice = 1m;
			pack2.LinePriceCurrency = Core.Constants.CurrencyCodes.Singapore;
			var packedItem2 = pack2.PackedItem;
			packedItem2.GoodsType = Constants.GoodsType.MajorExporter;
			packedItem2.API_CustomsQty = 1;
			packedItem2.API_CustomsUQ = ValidCustomsUQ;
			packedItem2.API_GoodsDescription = "GOODS 2";
			packedItem2.API_CustomsValue = 1m;
			packedItem2.API_TaxAmount = 1m;
			return (header, bill1, bill2);
		}

		public static AsycudaBill AddBill(AsycudaManifestHeader manifestHeader, string billNumber, string goodsDesc)
		{
			var newBill = manifestHeader.Bills.AddNew();
			newBill.ABL_BillNumber = billNumber;
			newBill.ABL_FreightValue = 1;
			newBill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Singapore;
			newBill.ABL_CustomsValue = 1m;
			newBill.TaxAmount = 1m;
			var newBillPack = newBill.Packs.AddNew();
			newBillPack.LinePrice = 1m;
			newBillPack.LinePriceCurrency = Core.Constants.CurrencyCodes.Singapore;
			var newBillPackedItem = newBillPack.PackedItem;
			newBillPackedItem.GoodsType = Constants.GoodsType.MajorExporter;
			newBillPackedItem.API_CustomsQty = 1;
			newBillPackedItem.API_CustomsUQ = ValidCustomsUQ;
			newBillPackedItem.API_GoodsDescription = goodsDesc;
			newBillPackedItem.API_CustomsValue = 1m;
			newBillPackedItem.API_TaxAmount = 1m;
			return newBill;
		}

		public void TestLookups()
		{
			var messageChooser = (MultiManifestBillSender)GetNewBusinessObject();
			AssertType<MultiManifestBillSenderLookups>(messageChooser.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			return new MultiManifestBillSender(new[] { bill });
		}
	}
}
