using System;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Testing
{
	public class CommonCartageXmlExporterTest : TestCaseWithFactory
	{
		[TestDate(2006, 10, 5, 12, 0, 0, 0)]
		public void TestExport()
		{
			var buffer = new NotificationBuffer();
			var dummyParent = new DummyCartageParent(Factory);
			var nonSaveFactory = new BusinessObjectFactory();
			var cartage = nonSaveFactory.New<CommonCartage>();
			var logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			logFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			AssertEquals(0, dummyParent.GetLogs().Find(logFilter).Length);
			var writer = new StringWriter();
			var tp = ((ICartageParent)dummyParent).CartageTypes.First();
			var ct = (DummyCartageType)tp;
			var cx = (ICartageExporter)new DummyCartageExporter(tp);
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsLocalTransport = true;
			transportCo.OH_Code = "TRANSCOSYD";
			transportCo.OH_FullName = "TransCo";
			ct.SetCartageOrganisation(transportCo);
			cartage.BookingInformation.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.FirmBookingRequest;
			Export(cx, cartage, writer, buffer);
			AssertEquals("Notify should not have errors", true, !buffer.HasErrors);
			AssertEquals("2 logs for this shipment", 2, dummyParent.GetLogs().Find(logFilter).Length);
			var logReferences = dummyParent.GetLogs().Find(logFilter).Select(l => l.SL_Reference);
			AssertContainsExactElementsInAnyOrder(new ZString[] { $"{Core.Constants.ProductName} Port Transport XML File", "FBR-Firm Booking Request (PortTransport) sent to TRANSCOSYD - TransCo." }, logReferences);
			AssertEquals("File should exist", false, string.IsNullOrEmpty(writer.GetStringBuilder().ToString()));
			AssertEquals(Factory, ((DummyCartageExporter)cx).CartageWasAdvisedInFactory);
		}

		public void TestPostExportSaveConcurrencyFailureIsCaught()
		{
			var factory = new OrgHeaderConcurrencyFailureOnSaveFactory(2);
			var buffer = new NotificationBuffer();
			var dummyParent = new DummyCartageParent(factory);
			var nonSaveFactory = new BusinessObjectFactory();
			var cartage = nonSaveFactory.New<CommonCartage>();
			var logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			logFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			AssertEquals(0, dummyParent.GetLogs().Find(logFilter).Length);
			var writer = new StringWriter();
			var tp = ((ICartageParent)dummyParent).CartageTypes.First();
			var ct = (DummyCartageType)tp;
			var cx = (ICartageExporter)new DummyCartageExporter(tp);
			var transportCo = factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsLocalTransport = true;
			transportCo.OH_Code = "TRANSCOSYD";
			transportCo.OH_FullName = "TransCo";
			ct.SetCartageOrganisation(transportCo);
			factory.OrgHeaderForConcurrencyError = transportCo;
			cartage.BookingInformation.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.FirmBookingRequest;
			var uncaughtExceptionOccurred = false;
			try
			{
				Export(cx, cartage, writer, buffer);
			}
			catch (Exception)
			{
				uncaughtExceptionOccurred = true;
			}

			AssertEquals("An uncaught exception occurred while exporting", false, uncaughtExceptionOccurred);
		}

		public void TestUserReceivesMessageIfConcurrencyErrorOccurs()
		{
			var buffer = new NotificationBuffer();
			var dummyParent = new DummyCartageParent(Factory);
			var nonSaveFactory = new BusinessObjectFactory();
			var cartage = nonSaveFactory.New<CommonCartage>();
			cartage.BookingInformation.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.FirmBookingRequest;
			var logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			logFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			AssertEquals(0, dummyParent.GetLogs().Find(logFilter).Length);
			var writer = new StringWriter();
			var cartageParent = (ICartageParent)dummyParent;
			var cartageType = (DummyCartageType)cartageParent.CartageTypes.First();
			var cartageExporter = (ICartageExporter)new DummyCartageExporter(cartageType);
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsLocalTransport = true;
			transportCo.OH_Code = "TRANSCOSYD";
			transportCo.OH_FullName = "TransCo";
			cartageType.SetCartageOrganisation(transportCo);
			Factory.Save();
			var conflictingFactory = new BusinessObjectFactory();
			conflictingFactory.RefreshEnabled = false;
			var conflictingTransportCo = conflictingFactory.Load<OrgHeader>(transportCo.PK);
			conflictingTransportCo.OH_FullName = "TransCo2";
			transportCo.OH_FullName = "TransCo3";
			conflictingFactory.Save();
			Export(cartageExporter, cartage, writer, buffer);
			CombineAssertions(() =>
			{
				Assert("User should not get a message notifying them of a successful export", !buffer.Events.ContainsNotificationContaining("Port Transport Job successfully exported to XML."));
				Assert("Error should be visible in the case of a concurrency error", buffer.Events.ContainsNotificationContaining("Please resolve concurrency issues before exporting to XML"));
				AssertEquals("File should not exist", true, string.IsNullOrEmpty(writer.GetStringBuilder().ToString()));
			});
		}

		public void TestExport_ShouldSaveTwice()
		{
			using (Factory.AddDisposableService())
			{
				var initialSaveCount = Factory.SaveCount;
				var buffer = new NotificationBuffer();
				var dummyParent = new DummyCartageParent(Factory);
				var cartage = Factory.NewWithValidTestData<CommonCartage>();
				var dataAdapter = new CommonCartageStatusValueObjectDataAdapter();
				var interchange = dataAdapter.GetXMLIntechangeWithTargetType(cartage, buffer);
				var cartageType = ((ICartageParent)dummyParent).CartageTypes.First();
				var cartageExporter = (ICartageExporter)new DummyCartageExporter(cartageType);
				var mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				mode.EK_Destination = "EDIDATDAT";
				var processor = new CartageXmlMessageDeliver(cartageExporter.ParentLogs.Parent, new MessageProcessorCommunicationModesResult(new[] { mode }, null), cartage, cartage, dataAdapter, interchange);
				var exporter = new CommonCartageXmlExporter(new CommonCartageBookingValueObjectDataAdapter());
				exporter.Export(cartageExporter, cartage, processor, buffer, CancellationToken.None);
				AssertEquals("Save count after Export", initialSaveCount + 2, Factory.SaveCount);
			}
		}

		public void TestExport_PreBookingAdvice()
		{
			var buffer = new NotificationBuffer();
			var dummyParent = new DummyCartageParent(Factory);
			var nonSaveFactory = new BusinessObjectFactory();
			var cartage = nonSaveFactory.New<CommonCartage>();
			var logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			logFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			AssertEquals(0, dummyParent.GetLogs().Find(logFilter).Length);
			var writer = new StringWriter();
			var tp = ((ICartageParent)dummyParent).CartageTypes.First();
			var ct = (DummyCartageType)tp;
			var cx = (ICartageExporter)new DummyCartageExporter(tp);
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_IsLocalTransport = true;
			transportCo.OH_Code = "TRANSCOSYD";
			transportCo.OH_FullName = "TransCo";
			ct.SetCartageOrganisation(transportCo);
			cartage.BookingInformation.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.PreBookingAdvice;
			Export(cx, cartage, writer, buffer);
			AssertEquals("Notify should not have errors", true, !buffer.HasErrors);
			AssertEquals("2 logs for this shipment", 2, dummyParent.GetLogs().Find(logFilter).Length);
			var logReferences = dummyParent.GetLogs().Find(logFilter).Select(l => l.SL_Reference);
			AssertContainsExactElementsInAnyOrder(new ZString[] { $"{Core.Constants.ProductName} Port Transport XML File", "PBA-Pre Booking Advice (PortTransport) sent to TRANSCOSYD - TransCo." }, logReferences);
			AssertEquals("File should exist", false, string.IsNullOrEmpty(writer.GetStringBuilder().ToString()));
			AssertEquals(null, ((DummyCartageExporter)cx).CartageWasAdvisedInFactory);
		}

		public class OrgHeaderConcurrencyFailureOnSaveFactory : BusinessObjectFactory
		{
			public OrgHeaderConcurrencyFailureOnSaveFactory(int failOnSaveNumber = 1, OrgHeader orgHeaderForConcurrencyError = null) : base()
			{
				FailOnSaveNumber = failOnSaveNumber;
				OrgHeaderForConcurrencyError = orgHeaderForConcurrencyError;
			}

			protected override void SaveCore()
			{
				if (SaveCount == FailOnSaveNumber - 1 && OrgHeaderForConcurrencyError != null)
				{
					var conflictingFactory = new BusinessObjectFactory();
					conflictingFactory.RefreshEnabled = false;
					var conflictingBizO = conflictingFactory.Load<OrgHeader>(OrgHeaderForConcurrencyError.PK);
					conflictingBizO.OH_FullName = conflictingBizO.OH_FullName == "Org2" ? "Org3" : "Org2";
					conflictingFactory.Save();
				}

				base.SaveCore();
			}

			public int FailOnSaveNumber { get; set; }

			public OrgHeader OrgHeaderForConcurrencyError { get; set; }
		}

		protected OrgHeader OrgProxy
		{
			get
			{
				if (fOrgProxy == null)
				{
					fOrgProxy = Factory.New<OrgHeader>();
					fOrgProxy.OH_RL_NKClosestPort = "AUBNE";
					fOrgProxy.OH_Code = "TESORG";
					fOrgProxy.OH_FullName = "TEST ORGPROXY";
					fOrgProxy.MainAddress.OA_Address1 = "10 HUTCHESON STREET";
					Factory.Save();
				}

				return fOrgProxy;
			}
		}

		OrgHeader fOrgProxy;

		protected CommonContainer Container
		{
			get
			{
				CommonContainer result = Factory.New<CommonContainer>();
				result.JC_ContainerNum = "Container1";
				result.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
				result.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
				result.JC_SealNum = "123456";
				return result;
			}
		}

		protected OrgHeader Client
		{
			get
			{
				if (fClient == null)
				{
					fClient = Factory.New<OrgHeader>();
					fClient.OH_Code = "LOCALCLIENT";
					fClient.OH_FullName = "Client for test";
					fClient.MainAddress.OA_Address1 = "Client Test Address Line";
					fClient.OH_RL_NKClosestPort = "AUSYD";
					Factory.Save();
				}

				return fClient;
			}
		}

		OrgHeader fClient;

		OrgHeader CurrentOrgProxy;
		protected override void SetUp()
		{
			base.SetUp();
			CurrentOrgProxy = GlbCompany.CurrentCompany.OrgProxy;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = OrgProxy.PK;
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = CurrentOrgProxy.PK;
		}

		class DummyCartageExporter : ICartageExporter // Booking
		{
			public DummyCartageExporter(CartageType cartageType)
			{
				this.cartageType = cartageType;
			}

			public CartageType CartageType
			{
				get
				{
					return cartageType;
				}
			}

			readonly CartageType cartageType;
			public OrgHeader SendTo
			{
				get
				{
					return CartageType.LocalTransportProviderAddress != null ? CartageType.LocalTransportProviderAddress.Header : null;
				}
			}

			public ZString SendToDescription
			{
				get
				{
					return "Local Transport Company";
				}
			}

			public ZString Description
			{
				get
				{
					return CartageType.Description;
				}
			}

			public ZString ParentJobNumber
			{
				get
				{
					return CartageType.CartageParent.UniqueConsignmentID;
				}
			}

			public CommonCartage GetCartageForExport(NotificationBuffer buffer)
			{
				return null;
			}

			public CommonCartageType CartageJobType
			{
				get
				{
					return !CartageType.CartageJobType.IsEmpty ? ParentFactory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, CartageType.CartageJobType)) : null;
				}
			}

			public BusinessObjectFactory ParentFactory
			{
				get
				{
					return CartageType.CartageParent.Factory;
				}
			}

			public Logs ParentLogs
			{
				get
				{
					return ((IStmALogParent)CartageType.CartageParent).Logs;
				}
			}

			public Notes ParentNotes
			{
				get
				{
					return ((BusinessObject)CartageType.CartageParent).GetNotes();
				}
			}

			public bool IsParentSaved
			{
				get
				{
					return true;
				}
			}

			public void CartageAdvised(BusinessObjectFactory factoryToCartageAdviseIn)
			{
				CartageWasAdvisedInFactory = factoryToCartageAdviseIn;
			}

			public BusinessObjectFactory CartageWasAdvisedInFactory { get; private set; }
		}

		public void Export(ICartageExporter cartageExporter, CommonCartage cartage, TextWriter writer, INotifications notify)
		{
			var exporter = new CommonCartageXmlExporter(new CommonCartageBookingValueObjectDataAdapter());
			exporter.Export(cartageExporter, cartage, writer, notify, CancellationToken.None);
		}
	}
}
