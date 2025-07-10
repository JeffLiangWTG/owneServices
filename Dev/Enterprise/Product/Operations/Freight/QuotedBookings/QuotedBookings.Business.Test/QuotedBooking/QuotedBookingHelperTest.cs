using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Freight.Integration;
using Enterprise.UniversalCopy.Business;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	class QuotedBookingHelperTest : TestCaseWithFactory
	{
		public void TestGetReceiverOrganizationTypeFromMode()
		{
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(""), QuotedBookingHelper.ReceiverOrganization.PackDepot);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.FCL), QuotedBookingHelper.ReceiverOrganization.SeaCTO);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.LSE), QuotedBookingHelper.ReceiverOrganization.PackDepot);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.ULD), QuotedBookingHelper.ReceiverOrganization.PackDepot);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.LCL), QuotedBookingHelper.ReceiverOrganization.PackDepot);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.LRO), QuotedBookingHelper.ReceiverOrganization.PackDepot);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.FTL), QuotedBookingHelper.ReceiverOrganization.PackDepot);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.LRA), QuotedBookingHelper.ReceiverOrganization.PackDepot);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.FRO), QuotedBookingHelper.ReceiverOrganization.CTO);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.COU), QuotedBookingHelper.ReceiverOrganization.CTO);
			AssertEquals(QuotedBookingHelper.GetReceiverOrganizationTypeFromMode(Core.Constants.RateMode.FRA), QuotedBookingHelper.ReceiverOrganization.CTO);
		}

		public void TestGetDeliveryOrganizationTypeFromMode()
		{
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(""), QuotedBookingHelper.DeliveryOrganization.UnpackDepot);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.LSE), QuotedBookingHelper.DeliveryOrganization.UnpackDepot);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.ULD), QuotedBookingHelper.DeliveryOrganization.UnpackDepot);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.LCL), QuotedBookingHelper.DeliveryOrganization.UnpackDepot);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.LRO), QuotedBookingHelper.DeliveryOrganization.UnpackDepot);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.FTL), QuotedBookingHelper.DeliveryOrganization.UnpackDepot);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.LRA), QuotedBookingHelper.DeliveryOrganization.UnpackDepot);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.FCL), QuotedBookingHelper.DeliveryOrganization.CTO);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.FRO), QuotedBookingHelper.DeliveryOrganization.CTO);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.COU), QuotedBookingHelper.DeliveryOrganization.CTO);
			AssertEquals(QuotedBookingHelper.GetDeliveryOrganizationTypeFromMode(Core.Constants.RateMode.FRA), QuotedBookingHelper.DeliveryOrganization.CTO);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateCopiesNone()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			var quoteNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Quote",
				CopyMethod = RelatedEntityCopyMethod.None
			};

			var bookingNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Booking",
				CopyMethod = RelatedEntityCopyMethod.None
			};

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "OH_Carrier", CopyMethod = CopyMethod.Copy });

			quotedBookingNode.Nodes.Add(quoteNode);
			quotedBookingNode.Nodes.Add(bookingNode);
			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };

			String bookingQuoteValidation = QuotedBookingHelper.ValidateUniversalCopyPreconditions(copyTree, quotedBooking);
			String expected = "This copy template cannot be used to copy the One Off Quote, because the template does not copy a Quote or Booking.";
			AssertEquals(expected, bookingQuoteValidation);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateCopiesWithQuoteNodeOrBookingNodeNull()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);

			var bookingNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Booking",
				CopyMethod = RelatedEntityCopyMethod.None
			};

			var quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "OH_Carrier", CopyMethod = CopyMethod.Copy });

			quotedBookingNode.Nodes.Add(bookingNode);
			var copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };

			AssertNoExceptionThrown(
			"ValidateUniversalCopyPreconditions should not throw exception", () =>
			{
				QuotedBookingHelper.ValidateUniversalCopyPreconditions(copyTree, quotedBooking);
			});

			var quoteNode = new RelatedEntityCopyTemplateNode
			{
				Name = "Quote",
				CopyMethod = RelatedEntityCopyMethod.None
			};

			quotedBookingNode = new EntityCopyTemplateNode { Name = "QuotedBooking" };
			quotedBookingNode.Nodes.Add(new PropertyCopyTemplateNode { Name = "OH_Carrier", CopyMethod = CopyMethod.Copy });

			quotedBookingNode.Nodes.Add(quoteNode);
			copyTree = new CopyTemplateTree { Name = "QuotedBooking", InnerNode = quotedBookingNode };

			AssertNoExceptionThrown(
			"ValidateUniversalCopyPreconditions should not throw exception", () =>
			{
				QuotedBookingHelper.ValidateUniversalCopyPreconditions(copyTree, quotedBooking);
			});
		}

		public void TestValidateUniversalCopyPreconditions_QuotedBooking_NoBookingExists_NoQuoteExists()
		{
			var ucFactory = new UniversalCopyFactory(typeof(QuotedBooking), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var innerNode = template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Booking"))).CopyMethod = RelatedEntityCopyMethod.Copy;
			Factory.Save();
			var validationErrorMessages = QuotedBookingHelper.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode, quotedBooking);
			var expectedErrorMessage = "This copy template copies only Booking but the One Off Quote is missing a Booking, and thus cannot create a valid copy.";
			AssertEquals("Validation error message thrown: Required booking is missing", expectedErrorMessage, validationErrorMessages);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateRequiresBooking_NoBookingExists()
		{
			var ucFactory = new UniversalCopyFactory(typeof(QuotedBooking), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = quote.PK;
			viewQuotedBooking.VB_JS = ZGuid.Empty;
			var quotedBooking = QuotedBooking.New(viewQuotedBooking, Factory);
			var innerNode = template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Booking"))).CopyMethod = RelatedEntityCopyMethod.Copy;
			Factory.Save();
			var validationErrorMessages = QuotedBookingHelper.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode, quotedBooking);
			var expectedErrorMessage = "This copy template copies only Booking but the One Off Quote is missing a Booking, and thus cannot create a valid copy.";
			AssertEquals("Validation error message thrown: Required booking is missing", expectedErrorMessage, validationErrorMessages);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateRequiresBooking_ABookingExists()
		{
			var ucFactory = new UniversalCopyFactory(typeof(QuotedBooking), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = ZGuid.Empty;
			viewQuotedBooking.VB_JS = booking.PK;
			var quotedBooking = QuotedBooking.New(viewQuotedBooking, Factory);
			var innerNode = template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			AssertNotNull(innerNode);
			((RelatedEntityCopyTemplateNode)innerNode.Nodes.Find(node => node.Name == "Booking")).CopyMethod = RelatedEntityCopyMethod.Copy;
			Factory.Save();
			var validationErrorMessages = QuotedBookingHelper.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode, quotedBooking);
			AssertEquals("No validation error message thrown: Required booking exists", null, validationErrorMessages);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateRequiresQuote_NoQuoteExists()
		{
			var ucFactory = new UniversalCopyFactory(typeof(QuotedBooking), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = ZGuid.Empty;
			viewQuotedBooking.VB_JS = booking.PK;
			var quotedBooking = QuotedBooking.New(viewQuotedBooking, Factory);
			var innerNode = template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			AssertNotNull(innerNode);
			((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Quote"))).CopyMethod = RelatedEntityCopyMethod.Copy;
			Factory.Save();
			var validationErrorMessages = QuotedBookingHelper.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode, quotedBooking);
			var expectedErrorMessage = "This copy template copies only Quote but the Quick Booking is missing a Quote, and thus cannot create a valid copy.";
			AssertEquals("Validation error message thrown: Required quote is missing", expectedErrorMessage, validationErrorMessages);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateRequiresQuote_AQuoteExists()
		{
			var ucFactory = new UniversalCopyFactory(typeof(QuotedBooking), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = quote.PK;
			viewQuotedBooking.VB_JS = ZGuid.Empty;
			var quotedBooking = QuotedBooking.New(viewQuotedBooking, Factory);
			var innerNode = template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			AssertNotNull(innerNode);
			((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Quote"))).CopyMethod = RelatedEntityCopyMethod.Copy;
			Factory.Save();
			var validationErrorMessages = QuotedBookingHelper.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode, quotedBooking);
			AssertEquals("No validation error message thrown: Required quote exists", null, validationErrorMessages);
		}

		public void TestValidateUniversalCopyPreconditions_TemplateCopiesBoth_OneDoesntExist()
		{
			var ucFactory = new UniversalCopyFactory(typeof(QuotedBooking), null);
			var template = ucFactory.GetNewCopyTemplate(null);
			template.PrepareForSave();
			template.Factory.Save();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var viewQuotedBooking = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = ZGuid.Empty;
			viewQuotedBooking.VB_JS = booking.PK;
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var viewQuotedBooking2 = Factory.New<ViewQuotedBooking>();
			viewQuotedBooking2.VB_TH = quote.PK;
			viewQuotedBooking2.VB_JS = ZGuid.Empty;
			var quotedBooking = QuotedBooking.New(viewQuotedBooking, Factory);
			var quotedBooking2 = QuotedBooking.New(viewQuotedBooking2, Factory);
			var innerNode = template.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Booking"))).CopyMethod = RelatedEntityCopyMethod.Copy;
			((RelatedEntityCopyTemplateNode)(innerNode.Nodes.Find(node => node.Name == "Quote"))).CopyMethod = RelatedEntityCopyMethod.Copy;
			Factory.Save();
			AssertEquals(null, QuotedBookingHelper.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode, quotedBooking));
			AssertEquals(null, QuotedBookingHelper.ValidateUniversalCopyPreconditions(template.CopyTemplateTree.CopyTemplateNode, quotedBooking2));
		}
	}
}
