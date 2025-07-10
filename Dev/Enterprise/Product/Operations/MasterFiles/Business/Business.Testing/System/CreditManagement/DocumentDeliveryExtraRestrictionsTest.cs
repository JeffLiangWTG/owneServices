using System;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.TransportBooking;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DocumentDeliveryExtraRestrictionsTest : TestCase
	{
		[TestCase(false, false, typeof(IForwardingConsol), false)]
		[TestCase(true, true, typeof(IForwardingConsol), true)]
		[TestCase(true, false, typeof(IForwardingConsol), true)]
		[TestCase(false, true, typeof(IForwardingConsol), true)]
		public void TestIsRestrictedForForwarding(
			bool isDPSFreightMovementRestricted,
			bool isAviationSecurityFreightMovementRestricted,
			Type bizOType,
			bool expectedIsRestricted)
		{
			var extraRestrictions = GetDocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted, isAviationSecurityFreightMovementRestricted, bizOType);

			AssertEquals(expectedIsRestricted, extraRestrictions.IsRestricted);
		}

		[TestCase(typeof(IForwardingConsol), true, false, false, false, false)]
		[TestCase(typeof(IForwardingShipment), false, true, false, false, false)]
		[TestCase(typeof(IQuotedBooking), false, false, true, false, false)]
		[TestCase(typeof(IDtbBooking), false, false, false, true, false)]
		[TestCase(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), false, false, false, false, true)]
		public void TypeSpecificRestrictionTypes(
			Type bizOType,
			bool expectedIsRestrictionForConsol,
			bool expectedIsRestrictionForShipment,
			bool expectedIsRestrictionForQuotedBooking,
			bool expectedIsRestrictionForDtbBooking,
			bool expectedIsRestrictionForDeclaration)
		{
			var extraRestrictions = GetDocumentDeliveryExtraRestrictions(false, false, bizOType);

			AssertEquals(expectedIsRestrictionForConsol, extraRestrictions.IsRestrictionForConsol);
			AssertEquals(expectedIsRestrictionForShipment, extraRestrictions.IsRestrictionForShipment);
			AssertEquals(expectedIsRestrictionForQuotedBooking, extraRestrictions.IsRestrictionForQuotedBooking);
			AssertEquals(expectedIsRestrictionForDtbBooking, extraRestrictions.IsRestrictionForDtbBooking);
			AssertEquals(expectedIsRestrictionForDeclaration, extraRestrictions.IsRestrictedForDeclaration);
		}

		static DocumentDeliveryExtraRestrictions GetDocumentDeliveryExtraRestrictions(bool isDPSFreightMovementRestricted, bool isAviationSecurityFreightMovementRestricted, Type bizOType)
		{
			return new DocumentDeliveryExtraRestrictions(isDPSFreightMovementRestricted, isAviationSecurityFreightMovementRestricted, bizOType);
		}
	}
}
