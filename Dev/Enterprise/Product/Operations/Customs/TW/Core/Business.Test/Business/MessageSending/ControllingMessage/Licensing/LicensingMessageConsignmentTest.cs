using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageConsignment))]
	sealed class LicensingMessageConsignmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			declaration.CusEntryInstruction.CEI_GoodsLocation = "USLAX";
			declaration.JE_RL_NKOrigin = "TWKEL";

			var invoiceLine1 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_BrandName = "name 1";
			invoiceLine1.AssignCMHeaderToInvoices(header);
			var invoiceLine2 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.AssignCMHeaderToInvoices(header);
			invoiceLine2.JI_BrandName = "name 2";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.ManifestSerialNumber.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ManifestSerialNumber - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignment.AdditionalInformations.Count(), NUnit.Framework.Is.EqualTo(0), "AdditionalInformations");
				NUnit.Framework.Assert.That(consignment.ArrivalTransportMeansTypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ArrivalTransportMeansTypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignment.ConsignmentItem.Commodity.Name, NUnit.Framework.Is.EqualTo("name 1").Using(CustomComparers.TypeComparison), "ConsignmentItem");
				NUnit.Framework.Assert.That(consignment.GoodsLocation, NUnit.Framework.Is.EqualTo("USLAX").Using(CustomComparers.TypeComparison), "GoodsLocation");
				NUnit.Framework.Assert.That(consignment.TransportContractDocuments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ITransportContractDocument>)), "TransportContractDocuments - should be [null]");
				NUnit.Framework.Assert.That(consignment.TransportEquipments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ITransportEquipment>)), "TransportEquipments - should be [null]");
				NUnit.Framework.Assert.That(consignment.BondedGoods, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IBondedGoods)), "BondedGoods - should be [null]");
				NUnit.Framework.Assert.That(consignment.TransitTransportMeansTypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TransitTransportMeansTypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(consignment.GoodsLocations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<CargoWise.Types.ZString>)), "GoodsLocations - should be [null]");
			});

			CombineAssertions("BorderTransportMeans", () =>
			{
				var borderTransportMeans = consignment.BorderTransportMeans;
				NUnit.Framework.Assert.That(borderTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDate.Empty), "ArrivalDateTime");
				NUnit.Framework.Assert.That(borderTransportMeans.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(borderTransportMeans.ItineraryRoutingCountryCodes.Count(), NUnit.Framework.Is.EqualTo(0), "ItineraryRoutingCountryCodes count");
				NUnit.Framework.Assert.That(borderTransportMeans.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(borderTransportMeans.JourneyID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "JourneyID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(borderTransportMeans.Registration.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Registration - should be [null] or [empty]");
				NUnit.Framework.Assert.That(borderTransportMeans.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(borderTransportMeans.CallSignID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CallSignID - should be [null] or [empty]");
			});

			CombineAssertions("Carrier", () =>
			{
				var carrier = consignment.Carrier;
				NUnit.Framework.Assert.That(carrier.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.ChineseName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ChineseName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.CustomsControlID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CustomsControlID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.PaymentOnAccountBusinessID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "PaymentOnAccountBusinessID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.RoleCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "RoleCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.SubBoxID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "SubBoxID - should be [null] or [empty]");
				var address = carrier.Address;
				NUnit.Framework.Assert.That(address.Line.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address Line - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.ChineseLine.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address ChineseLine - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountryCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountrySubDivisionID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountrySubDivisionID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(address.CountrySubDivisionName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Address CountrySubDivisionName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ILPCOAuthorizedParty)), "LPCOAuthorizedParty - should be [null]");
				NUnit.Framework.Assert.That(carrier.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ICommunication>)), "Communications - should be [null]");
				NUnit.Framework.Assert.That(carrier.ContactName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ContactName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.OwnerName.ToString(), NUnit.Framework.Is.Null.Or.Empty, "OwnerName - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.MainManufacturer.ToString(), NUnit.Framework.Is.Null.Or.Empty, "MainManufacturer - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.UndertakeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "UndertakeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(carrier.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IAdditionalInformation>)), "AdditionalInformations - should be [null]");
			});

			CombineAssertions("LoadingLocation", () =>
			{
				var loadingLocation = consignment.LoadingLocation;
				NUnit.Framework.Assert.That(loadingLocation.ID, NUnit.Framework.Is.EqualTo("TWKEL").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(loadingLocation.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(loadingLocation.LoadingDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty).Using(CustomComparers.TypeComparison), "LoadingDateTime");
				NUnit.Framework.Assert.That(loadingLocation.EstimatedLoadingCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "EstimatedLoadingCode - should be [null] or [empty]");
			});

			CombineAssertions("DepartureTransportMeans", () =>
			{
				var departureTransportMeans = consignment.DepartureTransportMeans;
				NUnit.Framework.Assert.That(departureTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDate.Empty), "ArrivalDateTime");
				NUnit.Framework.Assert.That(departureTransportMeans.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TypeCode - should be [null] or [empty]");
				NUnit.Framework.Assert.That(departureTransportMeans.ItineraryRoutingCountryCodes.Count(), NUnit.Framework.Is.EqualTo(0), "ItineraryRoutingCountryCodes count");
				NUnit.Framework.Assert.That(departureTransportMeans.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(departureTransportMeans.JourneyID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "JourneyID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(departureTransportMeans.Registration.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Registration - should be [null] or [empty]");
				NUnit.Framework.Assert.That(departureTransportMeans.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(departureTransportMeans.CallSignID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "CallSignID - should be [null] or [empty]");
			});

			CombineAssertions("UnloadingLocation", () =>
			{
				var unloadingLocation = consignment.UnloadingLocation;
				NUnit.Framework.Assert.That(unloadingLocation.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(unloadingLocation.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(unloadingLocation.LoadingDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty).Using(CustomComparers.TypeComparison), "LoadingDateTime");
				NUnit.Framework.Assert.That(unloadingLocation.EstimatedLoadingCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "EstimatedLoadingCode - should be [null] or [empty]");
			});

			CombineAssertions("TranshipmentLocation", () =>
			{
				var transhipmentLocation = consignment.TranshipmentLocation;
				NUnit.Framework.Assert.That(transhipmentLocation.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transhipmentLocation.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transhipmentLocation.LoadingDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty).Using(CustomComparers.TypeComparison), "LoadingDateTime");
				NUnit.Framework.Assert.That(transhipmentLocation.EstimatedLoadingCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "EstimatedLoadingCode - should be [null] or [empty]");
			});

			CombineAssertions("TransitDeparture", () =>
			{
				var transitDeparture = consignment.TransitDeparture;
				NUnit.Framework.Assert.That(transitDeparture.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transitDeparture.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(transitDeparture.LoadingDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty).Using(CustomComparers.TypeComparison), "LoadingDateTime");
				NUnit.Framework.Assert.That(transitDeparture.EstimatedLoadingCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "EstimatedLoadingCode - should be [null] or [empty]");
			});

			header.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().ForEach(c => c.Link = false);
			NUnit.Framework.Assert.That(consignment.ConsignmentItem, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IConsignmentItem)), "ConsignmentItem Type - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestPropertyTypes()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.GovernmentAgencyGoodsItem, NUnit.Framework.Is.TypeOf<LicensingMessageConsignmentGovernmentAgencyGoodsItem>(), "GovernmentAgencyGoodsItem");

				NUnit.Framework.Assert.That(consignment.ConsignmentItem, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IConsignmentItem)), "ConsignmentItem must be null when header do not link any invoiceLine - should be [null]");
				var invoiceLine1 = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine1.AssignCMHeaderToInvoices(header);
				NUnit.Framework.Assert.That(consignment.ConsignmentItem, NUnit.Framework.Is.TypeOf<LicensingMessageConsignmentItem>(), "ConsignmentItem type must be LicensingMessageConsignmentItem when header link any invoiceLine");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			consignment = new LicensingMessageConsignment(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		LicensingMessageConsignment consignment;
	}
}
