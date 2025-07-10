using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class CommonCartageXmlExportDirectorTest : TestCaseWithFactory
	{
		public void TestMessageIsNotShownWhenUserCancelsExport()
		{
			var (cartage, cartageType, dummyCartageParent, orgProxyHeader) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			ICartageExporter cartageExporter = manager;
			Factory.Save();
			var buffer = new NotificationBuffer();
			var director = new CommonCartageXmlExportDirectorForTest(new CommonCartageBookingValueObjectDataAdapter(), false);
			director.CancelExport();
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			AssertEquals("There should be no notifications.", 0, buffer.Events.Length);
		}

		public void TestLocalTransportBookingAccepted()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			cartage.JJ_ConsignmentID = TestConsignmentID;
			var manager = new InternalCartageManagerForTest(cartageType, cartage);
			var cartageExporter = (ICartageExporter)manager;
			dummyParent.HasChanges = false;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "ABC Company";
			cartageType.SetCartageOrganisation(org);
			dummyParent.Logs.AddNew(Events.StatusUpdated, "ACC-Booking Request Accepted (PortTransport) bla bla ABC - ABC Company.");
			Factory.Save();
			var buffer = new NotificationBuffer();
			var director = new CommonCartageXmlExportDirectorAccepted(new CommonCartageBookingValueObjectDataAdapter());
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			AssertEquals("There should be 1 notifications.", 1, buffer.Events.Length);
			AssertEquals("There should be 1 notifications.", true, buffer.Events.ContainsNotificationContaining("The Port Transport Booking has already been accepted by ABC - ABC Company. No modifications can be made. Please contact them directly."));
		}

		public void TestLocalTransportBookingRejected()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			cartage.JJ_ConsignmentID = TestConsignmentID;
			var manager = new InternalCartageManagerForTest(cartageType, cartage);
			var cartageExporter = (ICartageExporter)manager;
			dummyParent.HasChanges = false;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "ABC Company";
			cartageType.SetCartageOrganisation(org);
			dummyParent.Logs.AddNew(Events.StatusUpdated, "REJ-Booking Request Rejected (PortTransport) bla bla ABC - ABC Company.");
			Factory.Save();
			var buffer = new NotificationBuffer();
			var director = new CommonCartageXmlExportDirectorAccepted(new CommonCartageBookingValueObjectDataAdapter());
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			AssertEquals("There should be 1 notifications.", 1, buffer.Events.Length);
			AssertEquals("There should be 1 notifications.", true, buffer.Events.ContainsNotificationContaining("The Port Transport Booking has already been rejected by ABC - ABC Company. No modifications can be made. Please contact them directly."));
		}

		public class InternalCartageManagerForTest : InternalCartageManager
		{
			public InternalCartageManagerForTest(CartageType cartageType, CommonCartage cartage) : base(cartageType)
			{
				this.cartage = cartage;
			}

			readonly CommonCartage cartage;
			protected override CommonCartage GetCartageForExport(NotificationBuffer buffer)
			{
				return cartage;
			}
		}

		public class CommonCartageXmlExportDirectorAccepted : CommonCartageXmlExportDirector
		{
			public CommonCartageXmlExportDirectorAccepted(CommonCartageValueObjectDataAdapter adapter) : base(adapter)
			{
			}

			protected override void ExportToXmlCore(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer, CancellationToken token)
			{
				StringWriter writer = new StringWriter();
				XmlExporter.Export(cartageExporter, cartage, writer, buffer, token);
			}
		}

		public void TestMessageShownWhenSourceHasChanges()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			dummyParent.HasChanges = true;
			Assert("Precondition", dummyParent.HasChanges);
			NotificationBuffer buffer = new NotificationBuffer();
			CommonCartageXmlExportDirectorForTest director = new CommonCartageXmlExportDirectorForTest(new CommonCartageBookingValueObjectDataAdapter(), false);
			director.RunExport(manager, buffer, CancellationToken.None);
			Assert(buffer.HasErrors);
			Assert(buffer.Events.ContainsNotificationContaining("Please save your changes before you continue."));
		}

		public void TestExportWhenNoCartageCo()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			ICartageExporter cartageExporter = manager;
			dummyParent.HasChanges = false;
			Assert("Precondition", !dummyParent.HasChanges);
			cartageType.RemoveCartageOrganisation();
			AssertNull("Precondition", cartageType.LocalTransportProviderAddress);
			NotificationBuffer buffer = new NotificationBuffer();
			CommonCartageXmlExportDirectorForTest director = new CommonCartageXmlExportDirectorForTest(new CommonCartageBookingValueObjectDataAdapter(), false);
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			Assert(buffer.HasErrors);
			Assert(buffer.Events.ContainsNotificationContaining("Please enter a valid " + cartageExporter.Description + " Local Transport Company"));
		}

		public void TestExportSuccessful()
		{
			var (cartage, cartageType, dummyCartageParent, orgProxyHeader) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			ICartageExporter cartageExporter = manager;
			Factory.Save();
			NotificationBuffer buffer = new NotificationBuffer();
			CommonCartageXmlExportDirectorForTest director = new CommonCartageXmlExportDirectorForTest(new CommonCartageBookingValueObjectDataAdapter(), false);
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			AssertEquals("Buffer should have no errors", false, buffer.HasErrors);
			Assert("Buffer contains notification about Port Transport Job being successfully exported to XML", buffer.Events.ContainsNotificationContaining("Port Transport Job successfully exported to XML."));
		}

		public void TestExportWithError()
		{
			var (cartage, cartageType, dummyCartageParent, orgProxyHeader) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			ICartageExporter cartageExporter = manager;
			NotificationBuffer buffer = new NotificationBuffer();
			CommonCartageXmlExportDirectorForTest director = new CommonCartageXmlExportDirectorForTest(new CommonCartageBookingValueObjectDataAdapter(), true);
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			AssertEquals("Buffer should contain test error", true, buffer.HasErrors);
			Assert("Buffer should contain notification of test error", buffer.Events.ContainsNotificationContaining("test error"));
			CombineAssertions("Ensure that there is no notification claiming that there was a successful export to XML, by checking the message of both events", () =>
			{
				AssertEquals("Wrong number of events", 2, buffer.Events.Count());
				AssertEquals(buffer.Events[0].Message, "Error: test error");
				AssertEquals(buffer.Events[1].Message, "Error: test error");
			});
		}

		const string TestConsignmentID = "ABC0123";

		public class CommonCartageXmlExportDirectorForTest : CommonCartageXmlExportDirector
		{
			public CommonCartageXmlExportDirectorForTest(CommonCartageValueObjectDataAdapter adapter, bool withError) : base(adapter)
			{
				WithError = withError;
			}

			public void CancelExport()
			{
				SetWasCancelled(true);
			}

			protected override void ExportToXmlCore(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer, CancellationToken token)
			{
				StringWriter writer = new StringWriter();
				XmlExporter.Export(cartageExporter, cartage, writer, buffer, token);
			}

			protected override CommonCartageXmlExporter GetNewCommonCartageXmlExporter()
			{
				return new CommonCartageXmlExporterForTest(Adapter, WithError);
			}

			protected override bool QueryAdditionalBookingInformation(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer)
			{
				return true;
			}

			readonly bool WithError;
		}

		class CommonCartageXmlExporterForTest : CommonCartageXmlExporter
		{
			public CommonCartageXmlExporterForTest(CommonCartageValueObjectDataAdapter adapter, bool withError) : base(adapter)
			{
				this.WithError = withError;
			}

			protected override void ExportCore(ICartageExporter cartageExporter, CommonCartage cartage, TextWriter writer, CartageXmlMessageDeliver processor, INotifications notify, CancellationToken token)
			{
				if (WithError)
				{
					notify.Notify(new ErrorNotification(ErrorType.Error, "test error"));
				}
			}

			readonly bool WithError;
		}
	}
}
