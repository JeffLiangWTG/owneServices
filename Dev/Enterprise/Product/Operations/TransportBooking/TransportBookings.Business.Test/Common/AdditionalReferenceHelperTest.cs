using System;
using System.Linq;
using CargoWise.Data.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test.Common
{
	public class AdditionalReferenceHelperTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestGetAdditionalReferenceNumberTypeList()
		{
			var list = new TransportReferenceNumberTypeCollection
			{
				{ "AAA", (NoResString)"A Desc" },
				{ "BBB", (NoResString)"B Desc" },
			};

			TransportRegistry.Instance.AdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var expectedCodes = TransportRegistry.Instance.AdditionalReferenceNumbers.Value.Cast<TransportReferenceNumberType>().Select(r => r.Code);
			var defaultCodes = TransportRegistry.Instance.AdditionalReferenceNumbers.DefaultValue.Cast<TransportReferenceNumberType>().Select(r => r.Code);

			AssertGreaterThan("Precondition: AdditionalReferenceNumbers Value accessor should be different from AdditionalReferenceNumbers DefaultValue accessor", expectedCodes.Count(), defaultCodes.Count());

			AssertContainsExactElementsInAnyOrder("AdditionalReferenceHelper should use Value accessor on AdditionalReferenceNumbers", expectedCodes, AdditionalReferenceHelper.GetAdditionalReferenceNumberTypeList().ToArray().Select(c => c.Code));
		}

		[UseSnapshotProtection]
		public void TestGetAdditionalReferenceNumberTypeList_IncludesSystemValues()
		{
			var list = new TransportReferenceNumberTypeCollection
			{
				{ "AAA", (NoResString)"A Desc" },
				{ "BBB", (NoResString)"B Desc" },
			};

			TransportRegistry.Instance.AdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var expectedCodes = new string[]
			{
				"AAA",
				"BBB",
				TransportCommonAdditionalReferenceTypes.Codes.TransportReference,
				TransportCommonAdditionalReferenceTypes.Codes.CommercialInvoiceNumber,
				TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber,
				TransportCommonAdditionalReferenceTypes.Codes.ExternalUniqueConsignmentReference,
				TransportCommonAdditionalReferenceTypes.Codes.Client,
				TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber,
				TransportCommonAdditionalReferenceTypes.Codes.OrderNumber,
				TransportCommonAdditionalReferenceTypes.Codes.HouseBill,
				TransportCommonAdditionalReferenceTypes.Codes.MasterBill,
				TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference,
				TransportCommonAdditionalReferenceTypes.Codes.CarrierBookingReference,
			};
			AssertContainsExactElementsInAnyOrder("AdditionalReferenceHelper include system values even if they aren't saved in the database", expectedCodes, AdditionalReferenceHelper.GetAdditionalReferenceNumberTypeList().ToArray().Select(c => c.Code));
		}
	}
}
