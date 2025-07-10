using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class CommonCartageXmlExportToFileDirectorTest : TestCaseWithFactory
	{
		public void TestExportDoesNothingIfUserCancelsExport()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			var manager = new InternalCartageManager(cartageType);
			var cartageExporter = (ICartageExporter)manager;
			dummyParent.HasChanges = false;
			Assert("Precondition", !dummyParent.HasChanges);
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ConsignmentID = TestConsignmentID;
			cartage.SetParent(dummyCartageParent);
			Factory.Save();
			var buffer = new NotificationBuffer();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var director = new CartageXmlExportToFileDirectorForTest(new CommonCartageBookingValueObjectDataAdapter());
			director.CancelExport = true;
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			AssertEquals("There should be no notifications.", 0, buffer.Events.Length);
		}

		[TestDate(2006, 12, 13)]
		public void TestExportSavesTheFileToThePathProvided()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			ICartageExporter cartageExporter = manager;
			dummyParent.HasChanges = false;
			Assert("Precondition", !dummyParent.HasChanges);
			CommonCartage cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ConsignmentID = TestConsignmentID;
			cartage.SetParent(dummyParent);
			Factory.Save();
			NotificationBuffer buffer = new NotificationBuffer();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			CartageXmlExportToFileDirectorForTest director = new CartageXmlExportToFileDirectorForTest(new CommonCartageBookingValueObjectDataAdapter());
			ZString expectedFileName = Path.Combine(Env.TempPath, dummyCartageParent.UniqueConsignmentID + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss"));
			try
			{
				director.RunExport(cartageExporter, buffer, CancellationToken.None);
				Assert(File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		public void TestExportIsCancelledIfConcurrencyErrorOccurs()
		{
			var buffer = new NotificationBuffer();
			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			logFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			AssertEquals(0, jobDeclaration.GetLogs().Find(logFilter).Length);
			var cartageParent = (ICartageParent)jobDeclaration;
			var cartageType = (DeclarationExportCartageType)cartageParent.CartageTypes.First();
			var cartageExporter = (ICartageExporter)new InternalCartageManager(cartageType);
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsLocalTransport = true;
			transportCo.OH_Code = "TRANSCOSYD";
			transportCo.OH_FullName = "TransCo";
			jobDeclaration.JE_OA_DeliveryOrPickupCartageCoAddr = transportCo.Addresses.First().PK;
			var mode = ((OrgAddress)transportCo.Addresses.First()).Header.EDICommunicationsModes.AddNew();
			mode.EK_Module = JobInvoicingConsumerTypes.LocalCartage.Code;
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			mode.EK_Destination = "9CHARCODE";
			var depotCo = Factory.NewWithValidTestData<OrgHeader>();
			depotCo.OH_Code = "DEPOTCOSYD";
			depotCo.OH_FullName = "DepotCo";
			jobDeclaration.DepotDocAddress.E2_OA_Address = depotCo.Addresses.First().PK;
			var supplierCo = Factory.NewWithValidTestData<OrgHeader>();
			supplierCo.OH_Code = "SUPPCOSYD";
			supplierCo.OH_FullName = "SuppCo";
			jobDeclaration.SupplierPickupAddress.E2_OA_Address = supplierCo.Addresses.First().PK;
			var jobContainer = Factory.NewWithValidTestData<CommonContainer>();
			var cusContainer = Factory.NewWithValidTestData<CusContainer>();
			cusContainer.CO_JC = jobContainer.PK;
			cusContainer.CO_JE = jobDeclaration.PK;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form)
			{
				var infoForm = (CommonBookingInformationForm)form;
				var ccbi = (CommonCartageBookingInformation)infoForm.BusinessEntity;
				ccbi.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.FirmBookingRequest;
			});
			Factory.Save();
			var conflictingFactory = new BusinessObjectFactory();
			conflictingFactory.RefreshEnabled = false;
			var conflictingJobContainer = conflictingFactory.Load<CommonContainer>(jobContainer.PK);
			conflictingJobContainer.JC_DepartureCartageAdvised = ZDate.BrettsBirthday;
			conflictingFactory.Save();
			var director = new CartageXmlExportToFileDirectorForTest(new CommonCartageBookingValueObjectDataAdapter());
			CombineAssertions(() =>
			{
				try
				{
					director.RunExport(cartageExporter, buffer, CancellationToken.None);
					AssertEquals("The xml file should be empty", 0, File.ReadAllLines(director.NameOfLastSavedFile).Length);
					Assert("User should not get a message notifying them of a successful export", !buffer.Events.ContainsNotificationContaining("Port Transport Job successfully exported to XML."));
					Assert("Error should be visible in the case of a concurrency error", buffer.Events.ContainsNotificationContaining("Please resolve concurrency issues before exporting to XML"));
				}
				finally
				{
					DeleteIfExists(director.NameOfLastSavedFile);
				}
			});
		}

		class CartageXmlExportToFileDirectorForTest : CommonCartageXmlExportToFileDirector
		{
			public CartageXmlExportToFileDirectorForTest(CommonCartageValueObjectDataAdapter adapter) : base(adapter)
			{
			}

			public bool CancelExport { get; set; }

			public string NameOfLastSavedFile { get; set; }

			protected override Stream QueryUserForFileStream(ICartageExporter cartageExporter)
			{
				NameOfLastSavedFile = Path.Combine(Env.TempPath, cartageExporter.ParentJobNumber.Trim() + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss"));
				return CancelExport ? null : File.OpenWrite(NameOfLastSavedFile);
			}
		}

		const string TestConsignmentID = "ABC0123";
	}
}
