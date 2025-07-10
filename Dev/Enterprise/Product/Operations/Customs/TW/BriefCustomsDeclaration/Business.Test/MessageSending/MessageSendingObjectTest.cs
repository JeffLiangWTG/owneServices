using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestsSubclassesOf(typeof(MessageSendingObject))]
	abstract class MessageSendingObjectTest<T> : NonPersistentBusinessObjectTestCase
		where T : MessageSendingObject
	{
		public void TestIBriefCusDeclarationMembers()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "GS1";
			var cert = staff.Certificates.AddNew();
			cert.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			cert.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			cert.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);
			cert.XZ_RefNumber = "CER1";

			(var sendingObj, var header, _) = SetupData();
			header.AMA_ManifestType = TWManifestTypes.Codes.ImportDocuments;
			header.DeclarationDate = ZDateTime.BrettsBirthday;
			header.DeclarationNumber = "TEST0001";
			header.AMA_GS_NKCustomsAgent = "GS1";
			CombineAssertions(() =>
			{
				var bcDecl = sendingObj as IBriefCusDeclaration;
				AssertEquals("AcceptanceDateTime", ZDateTime.BrettsBirthday.Date, bcDecl.AcceptanceDateTime);
				AssertEquals("FunctionCode", sendingObj.Action, bcDecl.FunctionCode);
				AssertEquals("ID", "TEST0001", bcDecl.ID);
				AssertEquals("TypeCode", "X1", bcDecl.TypeCode);
				AssertType<Consignment>(bcDecl.Consignment);
				AssertEquals("RepresentativePersonName", "CER1", bcDecl.RepresentativePersonName);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetAllEDocs()
		{
			(var sendingObj, var header, _) = SetupData();
			var docManagerInfo = ((IDocManagerSupport)header).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			docManagerInfo.AddFileOrDocument(Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			docManagerInfo.AddFileOrDocument(Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.jpg"), Core.Constants.FileFormats.JPG);
			var docs = sendingObj.GetAllEDocs();
			AssertEquals(3, docs[0].Count);
		}

		public void TestBorderTransportMeans()
		{
			var (sendingObj, header, masterBill) = SetupData();
			masterBill.ABL_E_ARV = ZDateTime.BrettsBirthday.AddDays(1);
			header.AMA_TransportMode = "SEA";
			var borderTransportMeans = ((IBriefCusDeclaration)sendingObj).BorderTransportMeans;
			CombineAssertions(() =>
			{
				AssertEquals("ArrivalDateTime", ZDateTime.BrettsBirthday.AddDays(1).Date, borderTransportMeans.ArrivalDateTime);
				AssertEquals("TypeCode", "1", borderTransportMeans.TypeCode);

				header.AMA_TransportMode = "AIR";
				AssertEquals("TypeCode", "4", borderTransportMeans.TypeCode);
			});
		}

		public void TestOnBoardCourier()
		{
			(var sendingObj, var header, _) = SetupData();
			AssertNull(((IBriefCusDeclaration)sendingObj).OnBoardCourier);

			header.Person_CPN_PER_PersonPK = Factory.NewWithValidTestData<GlbPerson>().PK;
			AssertType<OnBoardCourier>(((IBriefCusDeclaration)sendingObj).OnBoardCourier);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			(_, var header, _) = SetupData();
			return GetMessageSendingObject(header);
		}

		public void TestDefaultValues()
		{
			(var sendingObj, var header, _) = SetupData();
			CombineAssertions(() =>
			{
				Assert("ShouldSend", sendingObj.ShouldSend);
				AssertEquals("Action", ActionCodeList.Codes.Create, sendingObj.Action);
			});
		}

		public void TestActionList()
		{
			(var sendingObj, _, _) = SetupData();
			AssertEquals("9, 5, 1", sendingObj.ActionList.CodesAsString);
		}

		public void TestEntryNumber()
		{
			(var sendingObj, var header, _) = SetupData();
			header.DeclarationNumber = "TEST0001";
			AssertEquals("TEST0001", sendingObj.EntryNumber);
		}

		public virtual void TestSerializeToMessageString()
		{
			(var sendingObj, _, _) = SetupData();
			AssertNullOrEmpty("Empty", sendingObj.SerializeToMessageString());
		}

		public void TestExpressCarrier()
		{
			(var sendingObj, var header, _) = SetupData();
			var carrierOrg = new TestTWCreator(Factory).CreateOrganization();
			carrierOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", Core.Constants.CountryCodes.Taiwan);
			header.AMA_OA_Carrier = carrierOrg.MainAddress.PK;

			var carrier = (sendingObj as IBriefCusDeclaration).ExpressCarrier;
			CombineAssertions(() =>
			{
				AssertEquals("ChineseName", "綠晃科技股份有限公司", carrier.ChineseName);
				AssertEquals("Name", "HAPPY CO., LTD.", carrier.Name);
				AssertEquals("ID", "96944490", carrier.ID);
				AssertEquals("TypeID", "58", carrier.TypeCode);
			});
		}

		public void TestGetBillNumberList()
		{
			(var sendingObj, var header, _) = SetupData();
			header.Bills.AddNew().ABL_BillNumber = $"B0001001";
			header.Bills.AddNew().ABL_BillNumber = $"B0001002";
			var billNumberList = sendingObj.SupportingDocuments.AddNew().BillNumberList;
			AssertEquals("B0001001, B0001002", billNumberList.CodesAsString);
		}

		protected abstract T GetMessageSendingObject(AsycudaManifestHeader header);

		protected (T sendingObj, AsycudaManifestHeader header, AsycudaBill masterBill) SetupData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			var masterBill = header.MasterBill;
			return (GetMessageSendingObject(header), header, masterBill);
		}
	}
}
