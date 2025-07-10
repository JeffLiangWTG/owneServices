using System;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Shared;
using Enterprise.ZArchitecture.Web.Shared.Testing;
using Moq;
using NUnit.Framework;
using WTG.Foundation.Cryptography;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class TryToTransferStockNonTransactionedTest : TestCase
	{
		#region TestTryToTransferStock_PutawayPartialStock_DataIntegrity

		[TestDate(2014, 11, 13)]
		[UseSnapshotProtection]
		public void TestTryToTransferStock_PutawayPartialStock_DataIntegrity()
		{
			var webService = new WhsSecureService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
			var now = ZDateTime.Now;
			var user = helper.CreateGlbStaff("A.A", "AAA");
			var password = "s3cret";
			user.StaffPlainTextPassword = password;

			webService.SecurityHeader = new SecuritySOAPHeader();
			webService.SecurityHeader.BranchCode = EnvProxy.Instance.CurrentBranch.Code;
			webService.SecurityHeader.DepartmentCode = EnvProxy.Instance.CurrentDepartment.Code;
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.SecurityHeader.UserName = user.GS_LoginName;
			var encoder = new AESCryptographicProvider(LoginHelper.CryptographyKey.Value, LoginHelper.CryptographyIv.Value);
			webService.SecurityHeader.Password = Convert.ToBase64String(encoder.Encrypt(Encoding.Unicode.GetBytes(password)));
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			webService.AllowedToRunServiceHasBeenCalled = false;
			webService.SecurityHeader.DeviceVersion = new DataService().WinCEWebServiceVersion;

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
			transfer.RunPreSaveValidation(); // to generate pick lines
			transferLine.PickedTime = ZDateTimeOffset.Now;
			webService.Factory.Save();

			var transferLineInfo = WhsTransferSecureServiceTestCase.CreatePutawayTransferLineInfo(data.Part1, "", "", "A-2", 1m);
			webService.CreateTestDataForIntergrityTestDuringTransferPutaway += delegate
			{
				throw new ZCannotSaveException("Test - Cannot Save", "Test");
			};

			AssertExceptionThrown<SoapException>(() => webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty));
			AssertEquals("Error reported.", typeof(ZCannotSaveException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("An unhandled exception occurred while processing warehouse web requests. Test - Cannot Save", ErrorReporter.LastMessageReported);
			AssertNotNull("Error reported.", ErrorReporter.LastExceptionReported.StackTrace);
			AssertEquals("Error reported.", "Test - Cannot Save", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			AssertEquals("Precondition - Transfer must not be finalised.", false, transferInNewFactory.IsFinalised);
			AssertEquals("Precondition - Transfer must have only one line.", 1, transferInNewFactory.Lines.Count);
			WhsTransferSecureServiceTestCase.AssertPutawayTransferLineData((WhsTransferLine)transferInNewFactory.Lines.Single(), false, "", "", ZDateTimeOffset.Empty, "");

			LoginHelper.ClearAllActiveSemaphoreHandlers_ForTesting();
		}

		#endregion

		#region TestTryToTransferStock_PutawayPartialStock_TransferValidationFails

		[TestDate(2014, 11, 13)]
		[UseSnapshotProtection]
		public void TestTryToTransferStock_PutawayPartialStock_TransferValidationFails()
		{
			var connectionTypeSupporterMock = new Mock<IHttpRequestManager>();
			connectionTypeSupporterMock
				.Setup(c => c.GetHttpContextBase())
				.Returns(MockHttpContext.PrepareMockHttpContextWrapper(true, false));

			using (ObjectFactory.Substitute(connectionTypeSupporterMock.Object))
			using (var webService = new WhsSecureService())
			{
				var helper = new WhsTestHelperFunctions(webService.Factory);
				var data = new TestDataSimpleEnvironment(webService.Factory, 2, 1);
				var now = ZDateTime.Now;
				var user = helper.CreateGlbStaff("A.A", "AAA");
				var password = "s3cret";
				user.StaffPlainTextPassword = password;

				webService.SecurityHeader = new SecuritySOAPHeader();
				webService.SecurityHeader.BranchCode = EnvProxy.Instance.CurrentBranch.Code;
				webService.SecurityHeader.DepartmentCode = EnvProxy.Instance.CurrentDepartment.Code;
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				webService.SecurityHeader.UserName = user.GS_LoginName;
				var encoder = new AESCryptographicProvider(LoginHelper.CryptographyKey.Value, LoginHelper.CryptographyIv.Value);
				webService.SecurityHeader.Password = Convert.ToBase64String(encoder.Encrypt(Encoding.Unicode.GetBytes(password)));
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				webService.SecurityHeader.DeviceVersion = new DataService().WinCEWebServiceVersion;
				webService.AllowedToRunServiceHasBeenCalled = false;

				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
				var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
				var transferLine = helper.CreateWhsTransferLine(transfer, data.Part1, 50m, "A-1", "");
				transfer.RunPreSaveValidation(); // to generate pick lines
				transferLine.PickedTime = ZDateTimeOffset.Now;
				webService.Factory.Save();

				var transferLineInfo = WhsTransferSecureServiceTestCase.CreatePutawayTransferLineInfo(data.Part1, "", "", "A-2", 1m);
				webService.CreateInvalidTransferDataForTest += t =>
				{
					t.AddRowError("Test Error");
				};

				TransferPutawayWebServiceResponse response = null;
				AssertNoExceptionThrown(() => { response = webService.TryToTransferStock(transfer.PK.ToGuid(), transferLineInfo, true, Guid.Empty); });

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
				AssertEquals("Precondition - Transfer must not be finalised.", false, transferInNewFactory.IsFinalised);
				AssertEquals("Precondition - Transfer must have only one line.", 1, transferInNewFactory.Lines.Count);
				AssertEquals(string.Format("Unknown errors occur when transferring stock, Please open transfer {0} in the Desktop application to fix them.", transfer.WD_DocketID), response.ErrorMessage);
				WhsTransferSecureServiceTestCase.AssertPutawayTransferLineData((WhsTransferLine)transferInNewFactory.Lines.Single(), false, "", "", ZDateTimeOffset.Empty, "");

				LoginHelper.ClearAllActiveSemaphoreHandlers_ForTesting();
			}
		}

		#endregion
	}
}
