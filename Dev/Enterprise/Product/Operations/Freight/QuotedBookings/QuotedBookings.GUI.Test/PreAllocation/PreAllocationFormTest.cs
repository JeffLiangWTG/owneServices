using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBookingToShipmentConverter;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	[TestedType(typeof(PreAllocationForm))]
	public class PreAllocationFormTest : ZFormBasherTest
	{
		public void TestSaveConcurrencyWithoutThrowingException()
		{
			var localFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var remoteFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK, Factory);
			quotedBooking.Mode = Core.Constants.RateMode.AIR;
			quotedBooking.Origin = "AUSYD";
			quotedBooking.Destination = "CNSHA";

			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			Factory.Save();

			var preAllocation = new PreAllocation(quotedBooking, PreAllocation.PreAllocationState.Existing);
			using (PreAllocationFormForTest preAllocationForm = new PreAllocationFormForTest(preAllocation))
			{
				preAllocation.IsPrePrinted = true;
				preAllocationForm.CreateMockPreAllocationForTest();

				var remoteQuotedBooking = QuotedBooking.New(ZGuid.Empty, quotedBooking.Booking.PK, remoteFactory);
				var converter = new QuotedBookingToShipmentConverter(remoteQuotedBooking, BookingToShipmentConversionSource.Form);
				Assert(!converter.HasAnyErrors(out string _));
				converter.ConvertBookingToShipment(remoteQuotedBooking.Booking);

				remoteFactory.Save();

				AssertNoExceptionThrown(() => preAllocationForm.SaveButton_Click(null, null));

				AssertEquals("Another user has converted the booking into a shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetMenuName()
		{
			using (PreAllocationFormForTest preAllocationForm = new PreAllocationFormForTest(PreAllocation))
			{
				PreAllocation.QuotedBooking.TransportMode = Core.Constants.TransportModes.Air;
				PreAllocation.QuotedBooking.ContainerMode = Core.Constants.ContainerModes.Loose;
				PreAllocation.QuotedBooking.Origin = "AUSYD";
				PreAllocation.QuotedBooking.Destination = "AUBNE";
				PreAllocation.IsPrePrinted = true;
				AssertEquals("Domestic HAWB", preAllocationForm.GetMenuName());

				PreAllocation.IsPrePrinted = false;
				AssertEquals("Domestic HAWB", preAllocationForm.GetMenuName());

				PreAllocation.QuotedBooking.TransportMode = Core.Constants.TransportModes.Road;
				PreAllocation.QuotedBooking.ContainerMode = Core.Constants.ContainerModes.FCL;
				AssertEquals("Domestic HAWB", preAllocationForm.GetMenuName());

				PreAllocation.QuotedBooking.TransportMode = Core.Constants.TransportModes.Sea;
				PreAllocation.QuotedBooking.ContainerMode = Core.Constants.ContainerModes.FCL;
				AssertEquals("Bill Of Lading", preAllocationForm.GetMenuName());

				PreAllocation.QuotedBooking.TransportMode = Core.Constants.TransportModes.Air;
				PreAllocation.QuotedBooking.ContainerMode = Core.Constants.ContainerModes.Loose;
				PreAllocation.QuotedBooking.Destination = "USLAX";
				AssertEquals("Laser HAWB", preAllocationForm.GetMenuName());

				PreAllocation.IsPrePrinted = true;
				AssertEquals("Neutral HAWB", preAllocationForm.GetMenuName());

				PreAllocation.QuotedBooking.TransportMode = Core.Constants.TransportModes.Sea;
				PreAllocation.QuotedBooking.ContainerMode = Core.Constants.ContainerModes.FCL;
				AssertEquals("Bill Of Lading To Preprinted", preAllocationForm.GetMenuName());

				PreAllocation.IsPrePrinted = false;
				AssertEquals("Bill Of Lading", preAllocationForm.GetMenuName());

				PreAllocation.QuotedBooking.TransportMode = Core.Constants.TransportModes.Road;
				PreAllocation.QuotedBooking.ContainerMode = Core.Constants.ContainerModes.FCL;
				PreAllocation.QuotedBooking.Destination = "AUBNE";
				AssertEquals("Domestic HAWB", preAllocationForm.GetMenuName());

				PreAllocation.IsPrePrinted = true;
				AssertEquals("Domestic HAWB", preAllocationForm.GetMenuName());
			}
		}

		public void TestPreallocationDocuments()
		{
			using (PreAllocationFormForTest preAllocationForm = new PreAllocationFormForTest(PreAllocation))
			{
				CombineAssertions(() =>
				{
					AssertPreallocationDocument(preAllocationForm.CreateDocumentCommand(PreAllocationForm.DocumentNames.BillOfLading),
						"Shipment",
						"Bill Of Lading");

					AssertPreallocationDocument(preAllocationForm.CreateDocumentCommand(PreAllocationForm.DocumentNames.BillOfLadingToPreprinted),
						"Shipment",
						"Bill Of Lading To Preprinted");

					AssertPreallocationDocument(preAllocationForm.CreateDocumentCommand(PreAllocationForm.DocumentNames.DomesticHAWB),
						"Shipment",
						"Domestic House Bill");

					AssertPreallocationDocument(preAllocationForm.CreateDocumentCommand(PreAllocationForm.DocumentNames.LaserHAWB),
						"Shipment",
						"Laser HAWB");

					AssertPreallocationDocument(preAllocationForm.CreateDocumentCommand(PreAllocationForm.DocumentNames.NeutralHAWB),
						"Shipment",
						"Neutral HAWB");
				});
			}
		}

		void AssertPreallocationDocument(DocumentCommand command, string expectedLinkedBusinessContext, string expectedLinkedDocumentName)
		{
			AssertEquals(string.Format("{0} [Parent]", command.SU_MenuName), PreAllocation, command.Parent);
			AssertEquals(string.Format("{0} [Documents.Count]", command.SU_MenuName), 0, command.Documents.Count);
			AssertEquals(string.Format("{0} [Documents.Count]", command.SU_MenuName), 0, command.Documents.Count);
			AssertEquals(string.Format("{0} [ChildMenus.Count]", command.SU_MenuName), 1, command.ChildMenus.Count);

			if (command.ChildMenus.Count > 0)
			{
				AssertEquals(string.Format("{0} [Outward.SU_BusinessContext]", command.SU_MenuName), expectedLinkedBusinessContext, command.ChildMenus[0].Outward.SU_BusinessContext);
				AssertEquals(string.Format("{0} [Outward.SU_MenuName]", command.SU_MenuName), expectedLinkedDocumentName, command.ChildMenus[0].Outward.SU_MenuName);
			}
		}

		#region Implementation

		PreAllocation PreAllocation
		{
			get { return preAllocation ?? (preAllocation = GetNewPreAllocation()); }
		}
		PreAllocation preAllocation;

		PreAllocation GetNewPreAllocation()
		{
			return new PreAllocation(GetQuotedBooking(), PreAllocation.PreAllocationState.Existing);
		}

		QuotedBooking GetQuotedBooking()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);

			QuotedBooking result = QuotedBooking.New(quote.PK, booking.PK, Factory);
			result.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.Mode = Core.Constants.RateMode.FCL;
			result.ConsignorDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			result.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, result.ConsignorDocumentaryAddress.E2_OA_Address)).PK;
			Factory.Save();

			return result;
		}

		protected override Form GetFormToBashCore()
		{
			QuotedBooking result = QuotedBooking.New(Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			result.TryLoadOrCreateJob();
			result.Job.JH_GE = Env.CurrentDepartment.PK;
			Factory.Save();
			return new PreAllocationForm(new PreAllocation(result, PreAllocation.PreAllocationState.Existing));
		}

		class PreAllocationFormForTest : PreAllocationForm
		{
			public PreAllocationFormForTest(PreAllocation preAllocation)
				: base(preAllocation)
			{
			}

			public new string GetMenuName()
			{
				return base.GetMenuName();
			}

			public new DocumentCommand CreateDocumentCommand(string documentName)
			{
				return base.CreateDocumentCommand(documentName);
			}
		}

		#endregion
	}
}
