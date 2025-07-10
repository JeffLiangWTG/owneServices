using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Lists;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingFilterBusinessObject))]
	public sealed class DtbBookingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTextFiltersMaxLength()
		{
			CombineAssertions(() =>
			{
				var filterBizO = GetNewFilterStripBusinessObject();
				TextFilterNameAndMaxLengthDictionary.ForEach(pair => AssertEquals($"The max length of filter {pair.Key} should be set as {pair.Value}.", Math.Min(pair.Value, ModuleFilter.MaxMaximumLength), filterBizO[pair.Key].MaxLength));
			});
		}

		IDictionary<string, int> TextFilterNameAndMaxLengthDictionary => new Dictionary<string, int>
		{
			{ FilterNameConstants.ParentJobNumber, ModuleNumberFilter.MultiplyMaxLength(ViewTransportBookingParentsSchema.VP_JobNumber.MaxLength) },
			{ FilterNameConstants.MasterBillNumber, ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength) },
			{ FilterNameConstants.HouseBillNumber, ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength) },
			{ FilterNameConstants.MultiBookingID, ModuleNumberFilter.MultiplyMaxLength(DtbBookingConsolidationSchema.KB_JobID.MaxLength) },
			{ FilterNameConstants.PackageID, 46 },
			{ FilterNameConstants.PackageID_Assigned, 3998 },
			{ FilterNameConstants.BookingID, ModuleNumberFilter.MultiplyMaxLength(DtbBookingSchema.KM_JobID.MaxLength) },
			{ FilterNameConstants.InstructionCompanyName, Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength) },
			{ FilterNameConstants.InstructionCompanyRelatedPort, Math.Min(OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength, OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength) },
			{ FilterNameConstants.BookingTransportReference, DtbBookingSchema.KM_TransportReference.MaxLength },
			{ FilterNameConstants.BookingConsolidationTemplate, DtbBookingSchema.KM_KT_NKBookingTemplate.MaxLength },
			{ FilterNameConstants.BookingConsolidationJobDirection, DtbBookingConsolidationSchema.KB_JobDirection.MaxLength },
			{ FilterNameConstants.InstructionType, DtbBookingInstructionSchema.KN_InstructionType.MaxLength },
			{ FilterNameConstants.ConfirmationReferenceNumber, DtbBookingConfirmationSchema.KK_ReferenceNum.MaxLength },
			{ FilterNameConstants.ConfirmationReceivedBy, DtbBookingConfirmationSchema.KK_ReceivedBy.MaxLength },
			{ FilterNameConstants.ConfirmationSlotReference, DtbBookingConfirmationSchema.KK_SlotReference.MaxLength },
			{ FilterNameConstants.InstructionNotes, Math.Min(ModuleFilter.MaxMaximumLength, DtbBookingInstructionSchema.KN_ServiceInstruction.MaxLength) },
			{ FilterNameConstants.InstructionAddressCity, Math.Min(JobDocAddressSchema.E2_City.MaxLength, OrgAddressSchema.OA_City.MaxLength) },
			{ FilterNameConstants.InstructionAddressState, Math.Min(JobDocAddressSchema.E2_State.MaxLength, OrgAddressSchema.OA_State.MaxLength) },
			{ FilterNameConstants.InstructionAddressPostCode, ModuleNumberFilter.MultiplyMaxLength(Math.Min(JobDocAddressSchema.E2_Postcode.MaxLength, OrgAddressSchema.OA_PostCode.MaxLength)) }
		};

		public void TestParentJobNumberFilter()
		{
			var shipment1 = Factory.New<IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S1";

			var shipment2 = Factory.New<IForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S2";

			var shipment3 = Factory.New<IForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "CS1";

			var jobConsol = Factory.New<IForwardingConsol>();
			jobConsol.JK_UniqueConsignRef = "C1";
			jobConsol.JK_IsCancelled = false;
			jobConsol.JK_IsForwarding = true;

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = ((BusinessObject)shipment1).TablePrefix;
			Booking1.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, "CS1");
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = ((BusinessObject)shipment2).TablePrefix;
			Booking2.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, "CS2");
			Booking3.ConsolidationSingleJob.KB_ParentID = shipment3.PK;
			Booking3.ConsolidationSingleJob.KB_ParentTableCode = ((BusinessObject)shipment3).TablePrefix;
			Booking3.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, "S1");

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			booking6.ConsolidationSingleJob.KB_ParentID = jobConsol.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = jobConsol.TablePrefix;
			booking6.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, "C1");

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterStrip[FilterNameConstants.ParentJobNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'Equal' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property = "S1";
			Asserter.AssertMatches("Filter: 'Equal' | Value: 'S1'", filter, Booking1, Booking3);

			filter.Property = "CS1";
			Asserter.AssertMatches("Filter: 'Equal' | Value: 'CS1'", filter, Booking1, Booking3);

			filter.Property = "C1";
			Asserter.AssertMatches("Filter: 'Equal' | Value: 'C1'", filter, booking6);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property = "C";
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: 'C'", filter, Booking1, Booking2, Booking3, booking6);

			filter.Property = "S";
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: 'S'", filter, Booking1, Booking2, Booking3);

			filter.Property = "S1";
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: 'S1'", filter, Booking1, Booking3);

			filter.Property = "CS2";
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: 'CS2'", filter, Booking2);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'Contains' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property = "1";
			Asserter.AssertMatches("Filter: 'Contains' | Value: '1'", filter, Booking1, Booking3, booking6);

			filter.Property = "2";
			Asserter.AssertMatches("Filter: 'Contains' | Value: '2'", filter, Booking2);

			filter.Property = "S";
			Asserter.AssertMatches("Filter: 'Contains' | Value: 'S'", filter, Booking1, Booking2, Booking3);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "";
			Asserter.AssertMatches("Filter: 'NotEqual' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property = "CS1";
			Asserter.AssertMatches("Filter: 'NotEqual' | Value: 'CS1'", filter, Booking2, Booking4, booking6);

			filter.Property = "S1";
			Asserter.AssertMatches("Filter: 'NotEqual' | Value: 'S1'", filter, Booking2, Booking4, booking6);

			filter.Property = "C1";
			Asserter.AssertMatches("Filter: 'NotEqual' | Value: 'C1'", filter, Booking1, Booking2, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "";
			Asserter.AssertMatches("Filter: 'DoesNotStartWith' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property = "C";
			Asserter.AssertMatches("Filter: 'DoesNotStartWith' | Value: 'C'", filter, Booking4);

			filter.Property = "S1";
			Asserter.AssertMatches("Filter: 'DoesNotStartWith' | Value: 'S1'", filter, Booking2, Booking4, booking6);

			filter.Property = "CS";
			Asserter.AssertMatches("Filter: 'DoesNotStartWith' | Value: 'CS'", filter, Booking4, booking6);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "";
			Asserter.AssertMatches("Filter: 'NotContains' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property = "CS";
			Asserter.AssertMatches("Filter: 'NotContains' | Value: 'CS'", filter, Booking4, booking6);

			filter.Property = "1";
			Asserter.AssertMatches("Filter: 'NotContains' | Value: '1'", filter, Booking2, Booking4);

			filter.Property = "S";
			Asserter.AssertMatches("Filter: 'NotContains' | Value: 'S'", filter, Booking4, booking6);
		}

		public void TestMasterBillFilter()
		{
			Booking1.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "bill1");
			Booking1.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "bill2");
			Booking2.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "bill1");
			Booking1.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "CB1");
			Booking2.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "CB2");
			Booking3.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "CB1");
			Factory.Save();

			var filter = (ModuleNumberFilter)FilterStrip[FilterNameConstants.MasterBillNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'Equal' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "bill1";
			Asserter.AssertMatches("Filter: 'Equal' | Value: 'bill1'", filter, Booking1, Booking2);

			filter.Property = "CB2";
			Asserter.AssertMatches("Filter: 'Equal' | Value: 'CB2'", filter, Booking2);

			filter.Property = "CB1";
			Asserter.AssertMatches("Filter: 'Equal' | Value: 'CB1'", filter, Booking1, Booking3);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "bill1";
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: 'bill1'", filter, Booking1, Booking2);

			filter.Property = "bill2";
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: 'bill2'", filter, Booking1);

			filter.Property = "CB1";
			Asserter.AssertMatches("Filter: 'StartsWith' | Value: 'CB1'", filter, Booking1, Booking3);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'Contains' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "CB";
			Asserter.AssertMatches("Filter: 'Contains' | Value: 'CB'", filter, Booking1, Booking2, Booking3);

			filter.Property = "bill";
			Asserter.AssertMatches("Filter: 'Contains' | Value: 'bill'", filter, Booking1, Booking2);

			filter.Property = "1";
			Asserter.AssertMatches("Filter: 'Contains' | Value: '1'", filter, Booking1, Booking2, Booking3);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'NotEqual' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "CB1";
			Asserter.AssertMatches("Filter: 'NotEqual' | Value: 'CB1'", filter, Booking2, Booking4);

			filter.Property = "bill1";
			Asserter.AssertMatches("Filter: 'NotEqual' | Value: 'bill1'", filter, Booking3, Booking4);

			filter.Property = "CB2";
			Asserter.AssertMatches("Filter: 'NotEqual' | Value: 'CB2'", filter, Booking1, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'DoesNotStartWith' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "CB";
			Asserter.AssertMatches("Filter: 'DoesNotStartWith' | Value: 'CB'", filter, Booking4);

			filter.Property = "bill";
			Asserter.AssertMatches("Filter: 'DoesNotStartWith' | Value: 'bill'", filter, Booking3, Booking4);

			filter.Property = "CB1";
			Asserter.AssertMatches("Filter: 'DoesNotStartWith' | Value: 'CB1'", filter, Booking2, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Filter: 'NotContains' | Value: ''", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "1";
			Asserter.AssertMatches("Filter: 'NotContains' | Value: '1'", filter, Booking4);

			filter.Property = "2";
			Asserter.AssertMatches("Filter: 'NotContains' | Value: '2'", filter, Booking3, Booking4);

			filter.Property = "bill";
			Asserter.AssertMatches("Filter: 'NotContains' | Value: 'bill'", filter, Booking3, Booking4);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("Filter: 'IsBlank", filter, Booking4);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("Filter: 'IsNotBlank'", filter, Booking1, Booking2, Booking3);
		}

		public void TestHouseBill()
		{
			Booking1.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "bill1");
			Booking1.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "CB1");
			Booking2.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "bill2");
			Booking2.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "CB2");
			Factory.Save();

			var filter = (ModuleNumberFilter)FilterStrip[FilterNameConstants.HouseBillNumber];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "bill1";
			Asserter.AssertMatches("bill1", filter, Booking1);

			filter.Property = "CB1";
			Asserter.AssertMatches("CB1", filter, Booking1);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("Not equal to CB1", filter, Booking2, Booking3, Booking4);

			filter.Property = "CB2";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("CB2", filter, Booking2);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank", filter, Booking3, Booking4);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank", filter, Booking1, Booking2);
		}

		public void TestReferenceNumberFilter()
		{
			NewReferenceNumber(Booking1, "AU", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "TRF1");
			NewReferenceNumber(Booking2, "US", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "MTRF2");
			NewReferenceNumber(Booking3, "AU", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "MTRF2");
			NewReferenceNumber(Booking4, "AU", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "TRF4");

			NewReferenceNumber(Booking1, "AU", TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "HSB1");
			NewReferenceNumber(Booking2, "AU", TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "HSB2");

			NewReferenceNumber(Booking1, "AU", TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "MSB1");
			NewReferenceNumber(Booking2, "AU", TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "");

			Booking1.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "CB1");
			Booking1.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "MasterBill1s");
			Booking2.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "CB2");
			Booking2.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "MasterBill2");
			Factory.Save();

			Asserter.AddFieldOfInterest("AU:" + TransportCommonAdditionalReferenceTypes.Codes.TransportReference, (b) => GetValue(b, "AU", TransportCommonAdditionalReferenceTypes.Codes.TransportReference));
			Asserter.AddFieldOfInterest("US:" + TransportCommonAdditionalReferenceTypes.Codes.TransportReference, (b) => GetValue(b, "US", TransportCommonAdditionalReferenceTypes.Codes.TransportReference));
			Asserter.AddFieldOfInterest("AU:" + TransportCommonAdditionalReferenceTypes.Codes.HouseBill, (b) => GetValue(b, "AU", TransportCommonAdditionalReferenceTypes.Codes.HouseBill));
			Asserter.AddFieldOfInterest("AU:" + TransportCommonAdditionalReferenceTypes.Codes.MasterBill, (b) => GetValue(b, "AU", TransportCommonAdditionalReferenceTypes.Codes.MasterBill));

			var filter = (ReferenceNumberFilter)FilterStrip[FilterNameConstants.TransportAdditionalReference];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("empty", filter, Booking1, Booking2, Booking3, Booking4);

			SetFilter(filter, "AU", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "TRF1");
			Asserter.AssertMatches("AU:" + TransportCommonAdditionalReferenceTypes.Codes.TransportReference + ":TRF1*", filter, Booking1);

			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "MTRF2");
			Asserter.AssertMatches(TransportCommonAdditionalReferenceTypes.Codes.TransportReference + ":MTRF2*", filter, Booking2, Booking3);

			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "CB1");
			Asserter.AssertMatches("CB1", filter, Booking1);

			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "CB2");
			Asserter.AssertMatches("CB2", filter, Booking2);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "");
			Asserter.AssertMatches("Without " + TransportCommonAdditionalReferenceTypes.Codes.MasterBill, filter, Booking3, Booking4);

			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.CommercialInvoiceNumber, "");
			Asserter.AssertMatches("Without " + TransportCommonAdditionalReferenceTypes.Codes.MasterBill, filter, Booking1, Booking2, Booking3, Booking4);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "");
			Asserter.AssertMatches("With " + TransportCommonAdditionalReferenceTypes.Codes.MasterBill, filter, Booking1, Booking2);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "TRF1");
			Asserter.AssertMatches("Without " + TransportCommonAdditionalReferenceTypes.Codes.TransportReference + ":TRF1", filter, Booking2, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "Test");
			Asserter.AssertMatches("Without " + TransportCommonAdditionalReferenceTypes.Codes.TransportReference + ":*Test", filter, Booking1, Booking2, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			SetFilter(filter, "", TransportCommonAdditionalReferenceTypes.Codes.TransportReference, "M");
			Asserter.AssertMatches("Without " + TransportCommonAdditionalReferenceTypes.Codes.TransportReference + ":M*", filter, Booking1, Booking4);
		}

		void NewReferenceNumber(DtbBooking booking, string countryCode, string type, string number)
		{
			var result = booking.AdditionalReferenceNumbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
		}

		string GetValue(DtbBooking booking, string country, string type)
		{
			foreach (Customs.ICusEntryNumber number in booking.AdditionalReferenceNumbers)
			{
				if (number.CE_RN_NKCountryCode == country && number.CE_EntryType == type)
				{
					return number.CE_EntryNum;
				}
			}

			return null;
		}

		void SetFilter(ReferenceNumberFilter filter, string country, string type, string number)
		{
			filter.Country = country;
			filter.Type = type;
			filter.Property = number;
		}

		public void TestConsolNumberFilter()
		{
			SetupConsolAndShipmentNumberCommonTestData();

			var shipment3 = Factory.New<IForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S3";

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = shipment3.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterStrip[FilterNameConstants.ConsolNumber];

			// StartsWith
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Starts with Empty", filter, Booking1, Booking2, Booking3, Booking4, Booking5, booking6);

			filter.Property = "C";
			Asserter.AssertMatches("Starts with 'C'", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "C1";
			Asserter.AssertMatches("Starts with 'C1'", filter, Booking1, Booking2, Booking4);

			filter.Property = "C2";
			Asserter.AssertMatches("Starts with 'C2'", filter, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "C";
			Asserter.AssertMatches("Equals 'C'", filter);

			filter.Property = "C2";
			Asserter.AssertMatches("Equals 'C2'", filter, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "C1";
			Asserter.AssertMatches("Does not equal 'C1'", filter, Booking3, Booking5, booking6);
		}

		public void TestShipmentNumberFilter()
		{
			SetupConsolAndShipmentNumberCommonTestData();

			var consol3 = Factory.New<IForwardingConsol>();
			consol3.JK_UniqueConsignRef = "C3";

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol3.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterStrip[FilterNameConstants.ShipmentNumber];

			// StartsWith
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Starts with Empty", filter, Booking1, Booking2, Booking3, Booking4, Booking5, booking6);

			filter.Property = "S";
			Asserter.AssertMatches("Starts with 'S'", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "S1";
			Asserter.AssertMatches("Starts with 'S1'", filter, Booking1, Booking2);

			filter.Property = "S2";
			Asserter.AssertMatches("Starts with 'S2'", filter, Booking1, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "S";
			Asserter.AssertMatches("Equals 'S'", filter);

			filter.Property = "S2";
			Asserter.AssertMatches("Equals 'S2'", filter, Booking1, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "S1";
			Asserter.AssertMatches("Does not equal 'S1'", filter, Booking3, Booking4, Booking5, booking6);
		}

		void SetupConsolAndShipmentNumberCommonTestData()
		{
			var shipment1 = Factory.New<IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S1";
			var shipment2 = Factory.New<IForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S2";

			var consol1 = Factory.New<IForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C1";
			consol1.JK_IsForwarding = true;
			consol1.JK_IsCancelled = false;
			var consol2 = Factory.New<IForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C2";
			consol2.JK_IsForwarding = true;
			consol2.JK_IsCancelled = false;

			LinkConsolAndShipment(consol1, shipment1);
			LinkConsolAndShipment(consol1, shipment2);
			LinkConsolAndShipment(consol2, shipment2);

			Booking1.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking3.ConsolidationSingleJob.KB_ParentID = consol2.PK;
			Booking3.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Booking4.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking4.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking5 = Booking5; // to kick off lazy initialisation for Booking5

			Factory.Save();
		}

		IJobConShipLink LinkConsolAndShipment(IForwardingConsol consol, IForwardingShipment shipment)
		{
			var conShipLink = Factory.New<IJobConShipLink>();
			conShipLink.JN_JK = consol.PK;
			conShipLink.JN_JS = shipment.PK;
			return conShipLink;
		}

		public void TestOriginDestinationFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment1[JobShipmentSchema.JS_RL_NKDestination] = "GBLON";

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			shipment2[JobShipmentSchema.JS_RL_NKDestination] = "NLAMS";

			BusinessObject shipment3 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment3[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			shipment3[JobShipmentSchema.JS_RL_NKDestination] = "NLAMS";

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = shipment2.TablePrefix;
			Booking3.ConsolidationSingleJob.KB_ParentID = shipment3.PK;
			Booking3.ConsolidationSingleJob.KB_ParentTableCode = shipment3.TablePrefix;

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[FilterNameConstants.OriginDestination];

			filter.Property1 = ZString.Empty;
			filter.Property2 = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property1 = "AUBNE";
			Asserter.AssertMatches("Origin", filter, Booking1, Booking2);

			filter.Property2 = "NLAMS";
			Asserter.AssertMatches("Origin-Destination", filter, Booking2);

			filter.Property1 = ZString.Empty;
			Asserter.AssertMatches("Destination", filter, Booking2, Booking3);
		}

		public void TestLoadDischargeFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "NLAMS";
			transportC1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC2 = (BusinessObject)((IBusinessObjectCollection)consol2["Transports"])[0];
			transportC2[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transportC2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transportC2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transportC2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "GBLON";
			transportC2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);

			BusinessObject shipment3 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transportS3 = ((IBusinessObjectCollection)shipment3["Transports"]).AddNew();
			transportS3[JobConsolTransportSchema.JW_IsLinked] = false;
			transportS3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportS3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUSYD";
			transportS3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "GBLON";

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking3.ConsolidationSingleJob.KB_ParentID = shipment3.PK;
			Booking3.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterStrip[FilterNameConstants.LoadDischarge];

			filter.Property1 = ZString.Empty;
			filter.Property2 = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = "AUBNE";
			Asserter.AssertMatches("Load", filter, Booking1, Booking2, booking6);

			filter.Property2 = "GBLON";
			Asserter.AssertMatches("Load-Discharge", filter, Booking2);

			filter.Property1 = ZString.Empty;
			Asserter.AssertMatches("Discharge", filter, Booking2, Booking3);
		}

		public void TestBookingConsolidationJobDirectionFilter()
		{
			TestBookingConsolidationTextFilter(FilterNameConstants.BookingConsolidationJobDirection, DtbBookingConsolidationSchema.KB_JobDirection);
			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingConsolidationJobDirection];
			var bookingConsolidationJobDirectionCodes = filter.List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			CombineAssertions("Does not contain the codes expected for booking consolidation job direction", () =>
			{
				AssertCollectionContains("Should contain booking consolidation job direction codes", nameof(DtbBookingDirection.PIC), bookingConsolidationJobDirectionCodes);
				AssertCollectionContains("Should contain booking consolidation job direction codes", nameof(DtbBookingDirection.DLV), bookingConsolidationJobDirectionCodes);
				AssertCollectionNotContains("Should not contain booking job direction codes", Constants.CartageDirection.Import, bookingConsolidationJobDirectionCodes);
				AssertCollectionNotContains("Should not contain booking job direction codes", Constants.CartageDirection.Export, bookingConsolidationJobDirectionCodes);
				AssertCollectionNotContains("Should not contain booking job direction codes", Constants.CartageDirection.Origin, bookingConsolidationJobDirectionCodes);
				AssertCollectionNotContains("Should not contain booking job direction codes", Constants.CartageDirection.Destination, bookingConsolidationJobDirectionCodes);
				AssertCollectionNotContains("Should not contain booking job direction codes", Constants.CartageDirection.Local, bookingConsolidationJobDirectionCodes);
			});
		}

		public void TestBookingJobDirectionFilter_BookingConsolidationJobDirectionFilter_UseDifferentCodeDescriptionPairLists()
		{
			var bookingConsolidationJobDirectionFilter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingConsolidationJobDirection];
			var bookingConsolidationJobDirectionCodes = bookingConsolidationJobDirectionFilter.List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			var bookingDirectionFilter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingDirection];
			var bookingDirectionCodes = bookingDirectionFilter.List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			var codeIntersection = bookingConsolidationJobDirectionCodes.Intersect(bookingDirectionCodes);
			AssertEquals("Overlap exists between the codes for booking consolidation job direction and for booking direction", 0, codeIntersection.Count());
		}

		public void TestBookingIDFilter()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.Bookings.Add(Booking1);
			consolidation.Bookings.Add(Booking2);
			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingID];
			filter.Property = booking1.KM_JobID;
			Asserter.AssertMatches("Should only find out Booking1", filter, Booking1);
		}

		public void TestMultiBookingIDFilter()
		{
			var bookingConsolidation1 = Helper.CreateConsolidation();
			var bookingConsolidation2 = Helper.CreateConsolidation();

			bookingConsolidation1.Bookings.Add(Booking1);
			bookingConsolidation2.Bookings.Add(Booking2);

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.MultiBookingID];
			filter.Property = bookingConsolidation1.KB_JobID;
			Asserter.AssertMatches("Should only find Booking1", filter, Booking1);

			filter.Property = bookingConsolidation2.KB_JobID;
			Asserter.AssertMatches("Should only find Booking2", filter, Booking2);
		}

		public void TestConsolidatedBookingIDFilter()
		{
			var bookingConsolidation1 = Helper.CreateConsolidation();
			var bookingConsolidation2 = Helper.CreateConsolidation();
			var bookingConsolidation3 = Helper.CreateConsolidation();

			Booking1.KM_KB_BookingConsolidationMultiJob = bookingConsolidation1.PK;
			Booking2.KM_KB_BookingConsolidationMultiJob = bookingConsolidation2.PK;
			Booking3.KM_KB_BookingConsolidationMultiJob = bookingConsolidation3.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.ConsolidatedBookingID];
			filter.Property = bookingConsolidation1.KB_JobID;
			Asserter.AssertMatches("Should only find Booking1", filter, Booking1);

			filter.Property = bookingConsolidation2.KB_JobID;
			Asserter.AssertMatches("Should only find Booking2", filter, Booking2);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.Property = "";
			Asserter.AssertMatches("Should only find Booking4, the only booking with a null value for KM_KB_BookingConsolidationMultiJob", filter, Booking4);
		}

		public void TestBookingJobDirectionFilter()
		{
			TestBookingTextFilter(FilterNameConstants.BookingDirection, DtbBookingSchema.KM_Direction);
			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingDirection];
			var bookingDirectionCodes = filter.List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			CombineAssertions("Does not contain the codes expected for booking consolidation job direction", () =>
			{
				AssertCollectionContains("Should contain booking job direction codes", Constants.CartageDirection.Import, bookingDirectionCodes);
				AssertCollectionContains("Should contain booking job direction codes", Constants.CartageDirection.Export, bookingDirectionCodes);
				AssertCollectionContains("Should contain booking job direction codes", Constants.CartageDirection.Origin, bookingDirectionCodes);
				AssertCollectionContains("Should contain booking job direction codes", Constants.CartageDirection.Destination, bookingDirectionCodes);
				AssertCollectionContains("Should contain booking job direction codes", Constants.CartageDirection.Local, bookingDirectionCodes);
				AssertCollectionNotContains("Should not contain booking consolidation job direction codes", nameof(DtbBookingDirection.PIC), bookingDirectionCodes);
				AssertCollectionNotContains("Should not contain booking consolidation job direction codes", nameof(DtbBookingDirection.DLV), bookingDirectionCodes);
			});
		}

		public void TestBookingBookingTemplateFilter()
		{
			TestBookingTextFilter(FilterNameConstants.BookingConsolidationTemplate, DtbBookingSchema.KM_KT_NKBookingTemplate);
		}

		public void TestInstructionTypeFilter()
		{
			TestInstructionTextFilter(FilterNameConstants.InstructionType, DtbBookingInstructionSchema.KN_InstructionType);
		}

		public void TestConfirmationDescriptionFilter()
		{
			TestConfirmationTextFilter(FilterNameConstants.ConfirmationType, DtbBookingConfirmationSchema.KK_ConfirmationType);
		}

		public void TestConfirmationReferenceNumberFilter()
		{
			TestConfirmationTextFilter(FilterNameConstants.ConfirmationReferenceNumber, DtbBookingConfirmationSchema.KK_ReferenceNum);
		}

		public void TestConfirmationReceivedByFilter()
		{
			TestConfirmationTextFilter(FilterNameConstants.ConfirmationReceivedBy, DtbBookingConfirmationSchema.KK_ReceivedBy);
		}

		public void TestConfirmationSlotReferenceFilter()
		{
			TestConfirmationTextFilter(FilterNameConstants.ConfirmationSlotReference, DtbBookingConfirmationSchema.KK_SlotReference);
		}

		public void TestVesselVoyageFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transportC1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transportC1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC2 = (BusinessObject)((IBusinessObjectCollection)consol2["Transports"])[0];
			transportC2[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transportC2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transportC2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transportC2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transportC2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol2);
			BusinessObject transportS1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transportS1[JobConsolTransportSchema.JW_IsLinked] = false;
			transportS1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Road;
			transportS1[JobConsolTransportSchema.JW_VoyageFlight] = "BLAT";
			transportS1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUSYD";
			transportS1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUBNE";

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = shipment2.TablePrefix;

			Factory.Save();

			ModuleTextAndNkFilter filter = (ModuleTextAndNkFilter)FilterStrip[FilterNameConstants.VoyageVessel];

			filter.Property = ZString.Empty;
			filter.NkProperty = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "001";
			Asserter.AssertMatches("Voyage", filter, Booking1);

			filter.NkProperty = "MAJAPAHIT";
			Asserter.AssertMatches("Conflicting Vessel Voyage", filter);

			filter.Property = ZString.Empty;
			filter.NkProperty = "BANOWATI";
			Asserter.AssertMatches("Vessel", filter, Booking1);

			filter.Property = ZString.Empty;
			filter.NkProperty = "BANO";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches("Vessel starts with comparison", filter, Booking1);

			filter.Property = "00";
			filter.NkProperty = ZString.Empty;
			Asserter.AssertMatches("Voyage starts with comparison", filter, Booking1, Booking2);

			filter.Property = ZString.Empty;
			filter.NkProperty = ZString.Empty;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank test", filter, Booking1, Booking2);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank test", filter, Array.Empty<DtbBooking>());
		}

		public void TestVesselVoyageFilter_CaptionAndDescription()
		{
			var filter = (ModuleTextAndNkFilter)FilterStrip[FilterNameConstants.VoyageVessel];
			AssertEquals("Vessel and Flight/Voyage #", filter.Description);
			AssertEquals("Vessel and Flight/Voyage #", filter.MultilingualDescription);
		}

		public void TestVesselVoyageFilter_OriginDoesNotMatchPortOfLoading_Import()
		{
			var voyageNumber = "9999";
			var vessel = "ABC 123";
			var booking = GetShipmentDtbBookingParent("AUBNE", "USLAX", vessel, voyageNumber, "AUMEL", "USLAX", "PIC");
			Asserter.AddToScope(booking);
			var filter = (ModuleTextAndNkFilter)FilterStrip[FilterNameConstants.VoyageVessel];

			filter.Property = ZString.Empty;
			filter.NkProperty = ZString.Empty;
			Asserter.AssertMatches("Empty filter", filter, booking, Booking1, Booking2, Booking3, Booking4);

			filter.Property = voyageNumber;
			Asserter.AssertMatches("Voyage", filter, booking);

			filter.Property = ZString.Empty;
			filter.NkProperty = vessel;
			Asserter.AssertMatches("Vessel", filter, booking);
		}

		public void TestVesselVoyageFilter_OriginDoesNotMatchPortOfLoading_Export()
		{
			var voyageNumber = "9999";
			var vessel = "ABC 123";
			var booking = GetShipmentDtbBookingParent("AUBNE", "USLAX", vessel, voyageNumber, "AUMEL", "USLAX", "DLV");
			Asserter.AddToScope(booking);
			var filter = (ModuleTextAndNkFilter)FilterStrip[FilterNameConstants.VoyageVessel];

			filter.Property = ZString.Empty;
			filter.NkProperty = ZString.Empty;
			Asserter.AssertMatches("Empty filter", filter, booking, Booking1, Booking2, Booking3, Booking4);

			filter.Property = voyageNumber;
			Asserter.AssertMatches("Voyage", filter, booking);

			filter.Property = ZString.Empty;
			filter.NkProperty = vessel;
			Asserter.AssertMatches("Vessel", filter, booking);
		}

		public void TestVesselVoyageFilter_DestinationDoesNotMatchPortOfDischarge_Import()
		{
			var voyageNumber = "9999";
			var vessel = "ABC 123";
			var booking = GetShipmentDtbBookingParent("AUBNE", "USLAX", vessel, voyageNumber, "AUBNE", "AUMEL", "PIC");
			Asserter.AddToScope(booking);
			var filter = (ModuleTextAndNkFilter)FilterStrip[FilterNameConstants.VoyageVessel];

			filter.Property = ZString.Empty;
			filter.NkProperty = ZString.Empty;
			Asserter.AssertMatches("Empty filter", filter, booking, Booking1, Booking2, Booking3, Booking4);

			filter.Property = voyageNumber;
			Asserter.AssertMatches("Voyage", filter, booking);

			filter.Property = ZString.Empty;
			filter.NkProperty = vessel;
			Asserter.AssertMatches("Vessel", filter, booking);
		}

		public void TestVesselVoyageFilter_DestinationDoesNotMatchPortOfDischarge_Export()
		{
			var voyageNumber = "9999";
			var vessel = "ABC 123";
			var booking = GetShipmentDtbBookingParent("AUBNE", "USLAX", vessel, voyageNumber, "AUBNE", "AUMEL", "DLV");
			Asserter.AddToScope(booking);
			var filter = (ModuleTextAndNkFilter)FilterStrip[FilterNameConstants.VoyageVessel];

			filter.Property = ZString.Empty;
			filter.NkProperty = ZString.Empty;
			Asserter.AssertMatches("Empty filter", filter, booking, Booking1, Booking2, Booking3, Booking4);

			filter.Property = voyageNumber;
			Asserter.AssertMatches("Voyage", filter, booking);

			filter.Property = ZString.Empty;
			filter.NkProperty = vessel;
			Asserter.AssertMatches("Vessel", filter, booking);
		}

		DtbBooking GetShipmentDtbBookingParent(string shipmentOrigin, string shipmentDestination, string vessel, string voyageNo, string portOfLoading, string portOfDischarge, string direction)
		{
			var bookingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			bookingShipment[JobShipmentSchema.JS_IsShipping] = true;
			bookingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			bookingShipment[JobShipmentSchema.JS_IsBooking] = true;
			bookingShipment[JobShipmentSchema.JS_RL_NKOrigin] = shipmentOrigin;
			bookingShipment[JobShipmentSchema.JS_RL_NKDestination] = shipmentDestination;

			var voyage = (BusinessObject)Factory.New<IJobVoyage>();
			voyage[JobVoyageSchema.JV_RV_NKVessel] = vessel;
			voyage[JobVoyageSchema.JV_VoyageFlight] = voyageNo;
			var origin = (BusinessObject)Factory.New<IVoyageOrigin>();
			origin[JobVoyOriginSchema.JA_E_DEP] = ZDateTime.Now.AddDays(-5);
			origin[JobVoyOriginSchema.JA_JV] = voyage.PK;
			origin[JobVoyOriginSchema.JA_RL_NKPortOfLoading] = portOfLoading;

			var destination = (BusinessObject)Factory.New<IVoyageDestination>();
			destination[JobVoyDestinationSchema.JB_E_ARV] = ZDateTime.Now;
			destination[JobVoyDestinationSchema.JB_JV] = voyage.PK;
			destination[JobVoyDestinationSchema.JB_RL_NKPortOfDischarge] = portOfDischarge;

			var sailing = (BusinessObject)Factory.New<IJobSailing>();
			sailing[JobSailingSchema.JX_JA] = origin.PK;
			sailing[JobSailingSchema.JX_JB] = destination.PK;

			var shipment = bookingShipment;
			shipment[JobShipmentSchema.JS_JX] = sailing.PK;

			var tbConsol = Helper.CreateConsolidation((IDtbBookingParent)bookingShipment);
			tbConsol.KB_JobDirection = direction;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var booking = Helper.CreateBooking(tbConsol);
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, "");
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "");

			var packageJob = booking.PackageJob;
			var package1 = Helper.CreatePackage("P123", 1, "BOX");
			var package10 = Helper.CreatePackage("", 10, "PLT");
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			package10.KP_KJ_ParentPackageJob = packageJob.PK;

			var orgType = direction == "PIC" ? "CNR" : "CFS";
			var instruction = Helper.CreateInstruction(booking, direction, orgType, org.MainAddress);
			Helper.CreatePackageDivot(instruction, package1);
			Helper.CreatePackageDivot(instruction, package10, 6);

			if (!instruction.Confirmations.Any(c => c.KK_ConfirmationType == direction))
			{
				Helper.CreateConfirmation(instruction, direction);
			}

			Factory.Save();

			return booking;
		}

		public void TestDeliveryEstimatedFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.DeliveryEstimated, "Delivery Estimated", ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_Estimated);
		}

		public void TestDeliveryActualFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.DeliveryActual, "Delivery Actual", ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_Actual);
		}

		public void TestDeliveryRequiredFromFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.DeliveryRequiredFrom, "Delivery Required From", ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_RequiredFrom);
		}

		public void TestDeliveryRequiredToFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.DeliveryRequiredTo, "Delivery Required To", ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_RequiredTo);
		}

		public void TestDeliverySlotDateTimeFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.DeliverySlotDate, "Delivery Slot Date", ConfirmationTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_SlotDateTime);
		}

		public void TestPickupEstimatedFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.PickupEstimated, "Pickup Estimated", ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_Estimated);
		}

		public void TestPickupActualFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.PickupActual, "Pickup Actual", ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_Actual);
		}

		public void TestPickupRequiredFromFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.PickupRequiredFrom, "Pickup Required From", ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_RequiredFrom);
		}

		public void TestPickupRequiredToFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.PickupRequiredTo, "Pickup Required To", ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_RequiredTo);
		}

		public void TestPickupSlotDateTimeFilter()
		{
			TestConfirmationDateFilter(FilterNameConstants.PickupSlotDate, "Pickup Slot Date", ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_SlotDateTime);
		}

		public void TestETDFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transportC1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transportC1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC2 = (BusinessObject)((IBusinessObjectCollection)consol2["Transports"])[0];
			transportC2[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transportC2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transportC2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transportC2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transportC2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);
			BusinessObject transportS1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transportS1[JobConsolTransportSchema.JW_IsLinked] = false;
			transportS1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Road;
			transportS1[JobConsolTransportSchema.JW_VoyageFlight] = "BLAT";
			transportS1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUSYD";
			transportS1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUBNE";

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.ETD];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1, booking6);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			// not sure if this is right, but this returns Booking1 because shipment has no ETD.
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking1, Booking3, Booking4);
		}

		public void TestETDFilterBookingConsolidation()
		{
			ZDateTime now = ZDateTime.Now;
			var consolidation1 = Helper.CreateConsolidation();
			var consolidation2 = Helper.CreateConsolidation();
			var consolidation3 = Helper.CreateConsolidation();
			consolidation1.Bookings.Add(Booking1);
			consolidation2.Bookings.Add(Booking2);
			consolidation3.Bookings.Add(Booking3);
			consolidation3.Bookings.Add(Booking4);

			var transportC1 = Factory.New<ITransport>();
			transportC1.ParentType = typeof(DtbBookingConsolidation);
			var bizo = transportC1 as BusinessObject;
			bizo[JobConsolTransportSchema.JW_ParentGUID] = consolidation1.PK;
			bizo[JobConsolTransportSchema.JW_ParentType] = "DTB"; //"KB"
			bizo[JobConsolTransportSchema.JW_IsLinked] = true;
			bizo[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			bizo[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			bizo[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			bizo[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			bizo[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			bizo[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			bizo[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			var transportC2 = Factory.New<ITransport>();
			transportC2.ParentType = typeof(DtbBookingConsolidation);
			bizo = transportC2 as BusinessObject;
			bizo[JobConsolTransportSchema.JW_ParentGUID] = consolidation2.PK;
			bizo[JobConsolTransportSchema.JW_ParentType] = "DTB"; //"KB"
			bizo[JobConsolTransportSchema.JW_IsLinked] = true;
			bizo[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			bizo[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			bizo[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			bizo[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			bizo[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			bizo[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			bizo[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			var transportS1 = Factory.New<ITransport>();
			transportS1.ParentType = typeof(DtbBookingConsolidation);
			bizo = transportS1 as BusinessObject;
			bizo[JobConsolTransportSchema.JW_ParentGUID] = consolidation1.PK;
			bizo[JobConsolTransportSchema.JW_ParentType] = "DTB"; //"KB"
			bizo[JobConsolTransportSchema.JW_IsLinked] = false;
			bizo[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Road;
			bizo[JobConsolTransportSchema.JW_VoyageFlight] = "BLAT";
			bizo[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUSYD";
			bizo[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUBNE";

			Factory.Save();

			var fac = Factory.Load<ITransport>(new ZQuery());
			AssertEquals(3, fac.Length);

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.ETD];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2);
		}

		public void TestBookingRequestedDateFilter()
		{
			ZDateTime now = ZDateTime.Now;

			Booking1.KM_BookingOfTransportRequestedDate = now.AddDays(-15);
			Booking2.KM_BookingOfTransportRequestedDate = now.AddDays(1);
			Booking3.KM_BookingOfTransportRequestedDate = ZDateTime.Empty;
			Booking4.KM_BookingOfTransportRequestedDate = ZDateTime.Empty;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.BookingRequestedDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2);
		}

		public void TestATDFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transportC1[JobConsolTransportSchema.JW_ATD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transportC1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC2 = (BusinessObject)((IBusinessObjectCollection)consol2["Transports"])[0];
			transportC2[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transportC2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transportC2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC2[JobConsolTransportSchema.JW_ATD] = now.AddDays(1);
			transportC2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transportC2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);
			BusinessObject transportS1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transportS1[JobConsolTransportSchema.JW_IsLinked] = false;
			transportS1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Road;
			transportS1[JobConsolTransportSchema.JW_VoyageFlight] = "BLAT";
			transportS1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUSYD";
			transportS1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUBNE";

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.ATD];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1, booking6);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			// not sure if this is right, but this returns Booking1 because shipment has no ATD.
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking1, Booking3, Booking4);
		}

		public void TestETAFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transportC1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transportC1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC2 = (BusinessObject)((IBusinessObjectCollection)consol2["Transports"])[0];
			transportC2[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transportC2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transportC2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transportC2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transportC2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);
			BusinessObject transportS1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transportS1[JobConsolTransportSchema.JW_IsLinked] = false;
			transportS1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Road;
			transportS1[JobConsolTransportSchema.JW_VoyageFlight] = "BLAT";
			transportS1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUSYD";
			transportS1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUBNE";

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.ETA];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-4);
			Asserter.AssertMatches("From", filter, Booking2);

			filter.Property2 = now.AddDays(4);
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1, booking6);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(6);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			// not sure if this is right, but this returns Booking1 because shipment has no ETA.
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking1, Booking3, Booking4);
		}

		public void TestATAFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transportC1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transportC1[JobConsolTransportSchema.JW_ATA] = now.AddDays(-5);

			BusinessObject consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC2 = (BusinessObject)((IBusinessObjectCollection)consol2["Transports"])[0];
			transportC2[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transportC2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transportC2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transportC2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transportC2[JobConsolTransportSchema.JW_ATA] = now.AddDays(5);

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);
			BusinessObject transportS1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transportS1[JobConsolTransportSchema.JW_IsLinked] = false;
			transportS1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Road;
			transportS1[JobConsolTransportSchema.JW_VoyageFlight] = "BLAT";
			transportS1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUSYD";
			transportS1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUBNE";

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.ATA];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-4);
			Asserter.AssertMatches("From", filter, Booking2);

			filter.Property2 = now.AddDays(4);
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1, booking6);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(6);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			// not sure if this is right, but this returns Booking1 because shipment has no ATA.
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking1, Booking3, Booking4);
		}

		public void TestFclReceivalFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transport1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject origin1 = (BusinessObject)((BusinessObject)transport1["Sailing"])["Origin"];
			origin1[JobVoyOriginSchema.JA_ReceivalCommences] = now.AddDays(-15);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transport2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject origin2 = (BusinessObject)((BusinessObject)transport2["Sailing"])["Origin"];
			origin2[JobVoyOriginSchema.JA_ReceivalCommences] = now.AddDays(2);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject origin3 = (BusinessObject)((BusinessObject)transport3["Sailing"])["Origin"];
			origin3[JobVoyOriginSchema.JA_ReceivalCommences] = now.AddDays(1);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.FclReceival];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestFclDgReceivalFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transport1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject origin1 = (BusinessObject)((BusinessObject)transport1["Sailing"])["Origin"];
			origin1[JobVoyOriginSchema.JA_DGReceivalCommences] = now.AddDays(-15);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transport2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject origin2 = (BusinessObject)((BusinessObject)transport2["Sailing"])["Origin"];
			origin2[JobVoyOriginSchema.JA_DGReceivalCommences] = now.AddDays(2);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject origin3 = (BusinessObject)((BusinessObject)transport3["Sailing"])["Origin"];
			origin3[JobVoyOriginSchema.JA_DGReceivalCommences] = now.AddDays(1);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.FclDgReceival];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestFclCutOffFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transport1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject origin1 = (BusinessObject)((BusinessObject)transport1["Sailing"])["Origin"];
			origin1[JobVoyOriginSchema.JA_CutOff] = now.AddDays(-15);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transport2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject origin2 = (BusinessObject)((BusinessObject)transport2["Sailing"])["Origin"];
			origin2[JobVoyOriginSchema.JA_CutOff] = now.AddDays(2);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject origin3 = (BusinessObject)((BusinessObject)transport3["Sailing"])["Origin"];
			origin3[JobVoyOriginSchema.JA_CutOff] = now.AddDays(1);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.FclCutOff];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestFclDgCutOffFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transport1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject origin1 = (BusinessObject)((BusinessObject)transport1["Sailing"])["Origin"];
			origin1[JobVoyOriginSchema.JA_DGCutOff] = now.AddDays(-15);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transport2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject origin2 = (BusinessObject)((BusinessObject)transport2["Sailing"])["Origin"];
			origin2[JobVoyOriginSchema.JA_DGCutOff] = now.AddDays(2);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject origin3 = (BusinessObject)((BusinessObject)transport3["Sailing"])["Origin"];
			origin3[JobVoyOriginSchema.JA_DGCutOff] = now.AddDays(1);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.FclDgCutOff];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestFclAvailabilityFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject origin1 = (BusinessObject)((BusinessObject)transport1["Sailing"])["Destination"];
			origin1[JobVoyDestinationSchema.JB_AvailabilityDate] = now.AddDays(-5);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";

			BusinessObject origin2 = (BusinessObject)((BusinessObject)transport2["Sailing"])["Destination"];
			origin2[JobVoyDestinationSchema.JB_AvailabilityDate] = now.AddDays(5);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject origin3 = (BusinessObject)((BusinessObject)transport3["Sailing"])["Destination"];
			origin3[JobVoyDestinationSchema.JB_AvailabilityDate] = now.AddDays(4);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.FclAvailability];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-3);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now.AddDays(3);
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(6);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestFclStorageFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject origin1 = (BusinessObject)((BusinessObject)transport1["Sailing"])["Destination"];
			origin1[JobVoyDestinationSchema.JB_StorageDate] = now.AddDays(-5);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";

			BusinessObject origin2 = (BusinessObject)((BusinessObject)transport2["Sailing"])["Destination"];
			origin2[JobVoyDestinationSchema.JB_StorageDate] = now.AddDays(5);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject origin3 = (BusinessObject)((BusinessObject)transport3["Sailing"])["Destination"];
			origin3[JobVoyDestinationSchema.JB_StorageDate] = now.AddDays(4);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.FclStorage];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-3);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now.AddDays(3);
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(6);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestLclReceivalFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transport1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject sailing1 = (BusinessObject)transport1["Sailing"];
			sailing1[JobSailingSchema.JX_DepotReceivalCommences] = now.AddDays(-15);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transport2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject sailing2 = (BusinessObject)transport2["Sailing"];
			sailing2[JobSailingSchema.JX_DepotReceivalCommences] = now.AddDays(2);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject sailing3 = (BusinessObject)transport3["Sailing"];
			sailing3[JobSailingSchema.JX_DepotReceivalCommences] = now.AddDays(1);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.LclReceival];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestLclCutOffFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";
			transport1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			BusinessObject sailing1 = (BusinessObject)transport1["Sailing"];
			sailing1[JobSailingSchema.JX_DepotCutOff] = now.AddDays(-15);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";
			transport2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject sailing2 = (BusinessObject)transport2["Sailing"];
			sailing2[JobSailingSchema.JX_DepotCutOff] = now.AddDays(2);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject sailing3 = (BusinessObject)transport3["Sailing"];
			sailing3[JobSailingSchema.JX_DepotCutOff] = now.AddDays(1);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.LclCutOff];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-14);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now;
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-16);
			filter.Property2 = now.AddDays(2);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestLclAvailabilityFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject sailing1 = (BusinessObject)transport1["Sailing"];
			sailing1[JobSailingSchema.JX_DepotAvailabilityDate] = now.AddDays(-5);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";

			BusinessObject sailing2 = (BusinessObject)transport2["Sailing"];
			sailing2[JobSailingSchema.JX_DepotAvailabilityDate] = now.AddDays(5);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject sailing3 = (BusinessObject)transport3["Sailing"];
			sailing3[JobSailingSchema.JX_DepotAvailabilityDate] = now.AddDays(4);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.LclAvailability];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-3);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now.AddDays(3);
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(6);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestLclStorageFilter()
		{
			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport1 = ((IBusinessObjectCollection)shipment1["Transports"]).AddNew();
			transport1[JobConsolTransportSchema.JW_IsLinked] = true;
			transport1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transport1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transport1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "NZAKL";
			transport1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transport1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject sailing1 = (BusinessObject)transport1["Sailing"];
			sailing1[JobSailingSchema.JX_DepotStorageDate] = now.AddDays(-5);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			BusinessObject transport2 = ((IBusinessObjectCollection)shipment2["Transports"]).AddNew();
			transport2[JobConsolTransportSchema.JW_IsLinked] = true;
			transport2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transport2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transport2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transport2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transport2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "SGSIN";

			BusinessObject sailing2 = (BusinessObject)transport2["Sailing"];
			sailing2[JobSailingSchema.JX_DepotStorageDate] = now.AddDays(5);

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol1Transports = (IBusinessObjectCollection)consol1["Transports"];
			BusinessObject transport3 = (BusinessObject)(consol1Transports.Count == 0 ? consol1Transports.AddNew() : consol1Transports[0]);
			transport3[JobConsolTransportSchema.JW_IsLinked] = true;
			transport3[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transport3[JobConsolTransportSchema.JW_Vessel] = "XXXX1234";
			transport3[JobConsolTransportSchema.JW_VoyageFlight] = "003";
			transport3[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUPOR";
			transport3[JobConsolTransportSchema.JW_ETD] = now.AddDays(-6);
			transport3[JobConsolTransportSchema.JW_RL_NKDiscPort] = "AUSYD";

			BusinessObject sailing3 = (BusinessObject)transport3["Sailing"];
			sailing3[JobSailingSchema.JX_DepotStorageDate] = now.AddDays(4);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[FilterNameConstants.LclStorage];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property1 = now.AddDays(-3);
			Asserter.AssertMatches("From", filter, Booking2, booking6);

			filter.Property2 = now.AddDays(3);
			Asserter.AssertMatches("From-To", filter);

			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("To", filter, Booking1);

			filter.Property1 = now.AddDays(-6);
			filter.Property2 = now.AddDays(6);
			Asserter.AssertMatches("From-To (2)", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, Booking1, Booking2, booking6);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, Booking3, Booking4);
		}

		public void TestBookingTransportCompanyFilter()
		{
			var orgHeader = Helper.CreateOrganisation("TESTORG");
			var orgHeader1 = Helper.CreateOrganisation("TESTORG1");

			Booking1.Address.OrganisationPK = orgHeader.PK;
			Booking2.Address.OrganisationPK = orgHeader.PK;
			Booking3.Address.OrganisationPK = orgHeader1.PK;
			Booking4.Address.OrganisationPK = ZGuid.Empty;
			Booking5.Address.OrganisationPK = ZGuid.Empty;

			Factory.Save();

			AssertGuidFilter(FilterNameConstants.TransportCompany, orgHeader.PK, SQLComparisonOperator.Equal, new[] { Booking1, Booking2 }, typeof(LocalTransportCollection));
			AssertGuidFilter(FilterNameConstants.TransportCompany, orgHeader1.PK, SQLComparisonOperator.Equal, new[] { Booking3 }, typeof(LocalTransportCollection));

			AssertGuidFilter(FilterNameConstants.TransportCompany, orgHeader.PK, SQLComparisonOperator.NotEqual, new[] { Booking3 }, typeof(LocalTransportCollection));
			AssertGuidFilter(FilterNameConstants.TransportCompany, orgHeader1.PK, SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2 }, typeof(LocalTransportCollection));

			AssertGuidFilter(FilterNameConstants.TransportCompany, orgHeader1.PK, SQLComparisonOperator.IsBlank, new[] { Booking4, Booking5 }, typeof(LocalTransportCollection));
			AssertGuidFilter(FilterNameConstants.TransportCompany, orgHeader1.PK, SQLComparisonOperator.IsNotBlank, new[] { Booking1, Booking2, Booking3 }, typeof(LocalTransportCollection));
		}

		public void TestBookingTransportCompanyFilter_WithOtherDocAddresses()
		{
			var booking = Helper.CreateBooking();
			var transportCo = Helper.CreateOrganisation("TCO");
			var billingParty = Helper.CreateOrganisation("CRB");
			var notifyParty = Helper.CreateOrganisation("NOT");
			var cne = Helper.CreateOrganisation("CNE");
			var cnr = Helper.CreateOrganisation("CNR");

			booking.Address.OrganisationPK = transportCo.PK;
			booking.BillingPartyAddress.OrganisationPK = billingParty.PK;
			booking.DocAddresses.AddNew(DocAddressType.NotifyParty).OrganisationPK = notifyParty.PK;
			booking.DocAddresses.AddNew(DocAddressType.FinalConsigneeAddress).OrganisationPK = cne.PK;
			booking.DocAddresses.AddNew(DocAddressType.OriginatingConsignorAddress).OrganisationPK = cnr.PK;
			Factory.Save();

			Asserter.AddToScope(booking);

			var filter = (ModuleGuidFilter)FilterStrip.ModuleFilters[FilterNameConstants.TransportCompany];
			filter.Property = billingParty.PK;
			Asserter.AssertMatches("Should *not* have matched on DocAddress types besides TransportCompany.", filter);

			filter.Property = notifyParty.PK;
			Asserter.AssertMatches("Should *not* have matched on DocAddress types besides TransportCompany.", filter);

			filter.Property = cne.PK;
			Asserter.AssertMatches("Should *not* have matched on DocAddress types besides TransportCompany.", filter);

			filter.Property = cnr.PK;
			Asserter.AssertMatches("Should *not* have matched on DocAddress types besides TransportCompany.", filter);

			filter.Property = transportCo.PK;
			Asserter.AssertMatches("Should have matched.", filter, booking);
		}

		public void TestInstructionAddressTypeFilter()
		{
			var instruction = Booking1.Instructions.AddNew();
			instruction.OrganisationType = "CTO";
			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.InstructionOrgType];
			filter.Property = "CTO";
			Asserter.AssertMatches("", filter, Booking1);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.InstructionOrgType];
			filter.Property = "LCT";
			Asserter.AssertMatches("", filter);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.InstructionOrgType];
			filter.Property = "NotACode";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3, Booking4);
		}

		public void TestInstructionCompanyNameFilter()
		{
			TestInstructionAddressTextFilter(FilterNameConstants.InstructionCompanyName, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName);
		}

		public void TestInstructionCompanyCodeFilter()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var organisation = Helper.CreateOrganisation("CLIENT");
			var otherClient = Helper.CreateOrganisation("OTHER");
			var address = organisation.MainAddress;
			organisation.OH_Code = "Smith Inc.";
			instruction.Address.OrganisationPK = organisation.PK;

			Factory.Save();

			AssertGuidFilter(FilterNameConstants.InstructionCompanyCode, organisation.PK, SQLComparisonOperator.Equal, new DtbBooking[] { booking });
			AssertGuidFilter(FilterNameConstants.InstructionCompanyCode, otherClient.PK, SQLComparisonOperator.Equal, Array.Empty<DtbBooking>());
		}

		public void TestRelatedPortFilter()
		{
			var organisation1 = Helper.CreateOrganisation("CLIENT");
			var organisation2 = Helper.CreateOrganisation("CLIENT2");
			var address1a = organisation1.MainAddress;
			var address1b = Helper.AddAddressToOrganisation(organisation1, "123 Fake St", OrgAddressType.PickupAndDelivery);
			var address2a = organisation2.MainAddress;
			var address2b = Helper.AddAddressToOrganisation(organisation2, "456 Fake St", OrgAddressType.PickupAndDelivery);

			organisation1.OH_RL_NKClosestPort = "AUSYD";
			address1a.OA_RL_NKRelatedPortCode = "AUSYD";
			address1b.OA_RL_NKRelatedPortCode = "";

			organisation2.OH_RL_NKClosestPort = "NZAKL";
			address2a.OA_RL_NKRelatedPortCode = "NZAKL";
			address2b.OA_RL_NKRelatedPortCode = "UZLAX";

			var instruction1 = Booking1.Instructions.AddNew();
			var instruction2 = Booking2.Instructions.AddNew();

			instruction1.Address.E2_OA_Address = address1a.PK;
			instruction2.Address.E2_OA_Address = address2a.PK;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.InstructionCompanyRelatedPort];
			filter.Property = "AUSYD";
			Asserter.AssertMatches("", filter, Booking1);

			filter.Property = "NZAKL";
			Asserter.AssertMatches("", filter, Booking2);

			filter.Property = "UZLAX";
			Asserter.AssertMatches("", filter);

			filter.Property = "ZZZZ";
			Asserter.AssertMatches("", filter);
		}

		public void TestCarrierFilter()
		{
			OrgHeader carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			ZDateTime now = ZDateTime.Now;

			BusinessObject consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			BusinessObject transportC1 = (BusinessObject)((IBusinessObjectCollection)consol1["Transports"])[0];
			transportC1[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC1[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC1[JobConsolTransportSchema.JW_Vessel] = "BANOWATI";
			transportC1[JobConsolTransportSchema.JW_VoyageFlight] = "001";
			transportC1[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC1[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transportC1[JobConsolTransportSchema.JW_RL_NKDiscPort] = "NLAMS";
			transportC1[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);
			transportC1[JobConsolTransportSchema.JW_OA_CarrierAddress] = carrier1.MainAddress.PK;

			BusinessObject consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			consol2[JobConsolSchema.JK_OA_ShippingLineAddress] = carrier2.MainAddress.PK;
			BusinessObject transportC2 = (BusinessObject)((IBusinessObjectCollection)consol2["Transports"])[0];
			transportC2[JobConsolTransportSchema.JW_IsLinked] = true;
			transportC2[JobConsolTransportSchema.JW_TransportMode] = Constants.TransportModes.Sea;
			transportC2[JobConsolTransportSchema.JW_Vessel] = "MAJAPAHIT";
			transportC2[JobConsolTransportSchema.JW_VoyageFlight] = "002";
			transportC2[JobConsolTransportSchema.JW_RL_NKLoadPort] = "AUBNE";
			transportC2[JobConsolTransportSchema.JW_ETD] = now.AddDays(1);
			transportC2[JobConsolTransportSchema.JW_RL_NKDiscPort] = "GBLON";
			transportC2[JobConsolTransportSchema.JW_ETA] = now.AddDays(5);

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking6 = GetNewBookingAndAddToScope("Booking6");
			booking6.ConsolidationSingleJob.KB_ParentID = consol1.PK;
			booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.Carrier];

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, booking6);

			filter.Property = carrier1.PK;
			Asserter.AssertMatches("On Transport", filter, Booking1, booking6);

			filter.Property = carrier2.PK;
			Asserter.AssertMatches("On Consol", filter, Booking2);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank", filter, Booking3, Booking4);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank", filter, Booking1, Booking2, booking6);
		}

		public void TestConsignorConsigneeFilter()
		{
			OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();

			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((JobDocAddress)shipment1["ConsignorDocumentaryAddress"]).E2_OA_Address = consignor1.MainAddress.PK;
			((JobDocAddress)shipment1["ConsigneeDocumentaryAddress"]).E2_OA_Address = consignee1.MainAddress.PK;

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((JobDocAddress)shipment2["ConsignorDocumentaryAddress"]).E2_OA_Address = consignor1.MainAddress.PK;
			((JobDocAddress)shipment2["ConsigneeDocumentaryAddress"]).E2_OA_Address = consignee2.MainAddress.PK;

			BusinessObject shipment3 = (BusinessObject)Factory.New<IForwardingShipment>();
			((JobDocAddress)shipment3["ConsignorDocumentaryAddress"]).E2_OA_Address = consignor2.MainAddress.PK;
			((JobDocAddress)shipment3["ConsigneeDocumentaryAddress"]).E2_OA_Address = consignee2.MainAddress.PK;

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = shipment2.TablePrefix;
			Booking3.ConsolidationSingleJob.KB_ParentID = shipment3.PK;
			Booking3.ConsolidationSingleJob.KB_ParentTableCode = shipment3.TablePrefix;

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStrip[FilterNameConstants.ConsignorConsignee];

			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property1 = consignor1.PK;
			Asserter.AssertMatches("Consignor", filter, Booking1, Booking2);

			filter.Property2 = consignee2.PK;
			Asserter.AssertMatches("Consignor-Consignee", filter, Booking2);

			filter.Property1 = ZGuid.Empty;
			Asserter.AssertMatches("Consignee", filter, Booking2, Booking3);
		}

		public void TestConsignorConsigneeFilter_StandaloneBooking()
		{
			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();

			var pickupInstruction = Helper.CreateInstruction(Booking1, InstructionTypes.Codes.PickUp);
			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			pickupInstruction.Address.OrganisationPK = consignor1.PK;

			var deliverInstruction = Helper.CreateInstruction(Booking2, InstructionTypes.Codes.Delivery);
			deliverInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			deliverInstruction.Address.OrganisationPK = consignee1.PK;

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStrip[FilterNameConstants.ConsignorConsignee];

			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property1 = consignor1.PK;
			Asserter.AssertMatches("Consignor", filter, Booking1);

			filter.Property1 = ZGuid.Empty;
			filter.Property2 = consignee1.PK;
			Asserter.AssertMatches("Consignee", filter, Booking2);
		}

		public void TestConsignorConsigneeFilter_AllowNonConsignorConsigneeOrganisations()
		{
			var filter = (ModuleGuidsFilter)FilterStrip[FilterNameConstants.ConsignorConsignee];

			AssertEquals(typeof(ConsignorCollection), filter.List1.GetType());
			AssertEquals(typeof(ConsigneeCollection), filter.List2.GetType());

			var consignors = (OrganisationsFindBoxCollection)filter.List1;
			var consignees = (OrganisationsFindBoxCollection)filter.List2;

			AssertEquals(true, consignors.AllowOtherOrgTypes);
			AssertEquals(true, consignees.AllowOtherOrgTypes);
		}

		public void TestSendingReceivingAgentFilter()
		{
			OrgHeader sendingAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader receivingAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader receivingAgent2 = Factory.NewWithValidTestData<OrgHeader>();

			ZDateTime now = ZDateTime.Now;

			BusinessObject consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			consol1[JobConsolSchema.Constants.JK_OA_SendingForwarderAddress] = sendingAgent1.MainAddress.PK;
			consol1[JobConsolSchema.Constants.JK_OA_ReceivingForwarderAddress] = receivingAgent1.MainAddress.PK;

			BusinessObject consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			consol2[JobConsolSchema.Constants.JK_OA_SendingForwarderAddress] = sendingAgent1.MainAddress.PK;
			consol2[JobConsolSchema.Constants.JK_OA_ReceivingForwarderAddress] = receivingAgent2.MainAddress.PK;

			BusinessObject consol3 = (BusinessObject)Factory.New<IForwardingConsol>();
			consol3[JobConsolSchema.Constants.JK_OA_SendingForwarderAddress] = sendingAgent2.MainAddress.PK;
			consol3[JobConsolSchema.Constants.JK_OA_ReceivingForwarderAddress] = receivingAgent2.MainAddress.PK;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment1["Consols"]).Add(consol1);

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment2["Consols"]).Add(consol2);

			BusinessObject shipment3 = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)shipment3["Consols"]).Add(consol3);

			BusinessObject shipment4 = (BusinessObject)Factory.New<IForwardingShipment>();

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = shipment2.TablePrefix;
			Booking3.ConsolidationSingleJob.KB_ParentID = shipment3.PK;
			Booking3.ConsolidationSingleJob.KB_ParentTableCode = shipment3.TablePrefix;
			Booking4.ConsolidationSingleJob.KB_ParentID = shipment4.PK;
			Booking4.ConsolidationSingleJob.KB_ParentTableCode = shipment4.TablePrefix;

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterStrip[FilterNameConstants.SendingReceivingAgent];

			filter.Property1 = ZGuid.Empty;
			filter.Property2 = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property1 = sendingAgent1.PK;
			Asserter.AssertMatches("Sending Agent", filter, Booking1, Booking2);

			filter.Property2 = receivingAgent2.PK;
			Asserter.AssertMatches("Sending-Receiving Agents", filter, Booking2);

			filter.Property1 = ZGuid.Empty;
			Asserter.AssertMatches("Receiving Agents", filter, Booking2, Booking3);
		}

		public void TestControllingCustomerFilter()
		{
			OrgHeader party1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader party2 = Factory.NewWithValidTestData<OrgHeader>();

			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			((JobDocAddress)shipment1["ControllingCustomerAddress"]).E2_OA_Address = party1.MainAddress.PK;

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			((JobDocAddress)shipment2["ControllingCustomerAddress"]).E2_OA_Address = party2.MainAddress.PK;

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = shipment2.TablePrefix;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.ControllingCustomer];

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = party1.PK;
			Asserter.AssertMatches("Controlling Customer", filter, Booking1);
		}

		public void TestBookedByFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			booking1.ConsolidationSingleJob.BookedByAddress.OrganisationPK = org1.PK;
			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.BookedBy];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, booking1, booking2, booking3, booking4);

			filter.Property = org1.PK;
			Asserter.AssertMatches("org1", filter, booking1);

			filter.Property = org2.PK;
			Asserter.AssertMatches("org2", filter);
		}

		public void TestCustomsBrokerFilter()
		{
			OrgHeader importBroker1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader importBroker2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader exportBroker1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader exportBroker2 = Factory.NewWithValidTestData<OrgHeader>();

			ZDateTime now = ZDateTime.Now;

			BusinessObject shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment1[JobShipmentSchema.JS_OH_ImportBroker] = importBroker1.PK;
			shipment1[JobShipmentSchema.JS_OH_ExportBroker] = exportBroker1.PK;

			BusinessObject shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment2[JobShipmentSchema.JS_OH_ImportBroker] = importBroker1.PK;
			shipment2[JobShipmentSchema.JS_OH_ExportBroker] = exportBroker2.PK;

			BusinessObject shipment3 = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment3[JobShipmentSchema.JS_OH_ImportBroker] = importBroker2.PK;
			shipment3[JobShipmentSchema.JS_OH_ExportBroker] = exportBroker2.PK;

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = shipment2.TablePrefix;
			Booking3.ConsolidationSingleJob.KB_ParentID = shipment3.PK;
			Booking3.ConsolidationSingleJob.KB_ParentTableCode = shipment3.TablePrefix;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.CustomsBroker];

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = importBroker1.PK;
			Asserter.AssertMatches("Import Broker", filter, Booking1, Booking2);

			filter.Property = exportBroker2.PK;
			Asserter.AssertMatches("Export Broker", filter, Booking2, Booking3);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLocalClientFilter()
		{
			var now = ZDateTime.Now;

			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();

			var otherBranch = GetBranchInAnotherCompany(GlbCompany.CurrentCompany);

			var shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();

			var shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();

			var header1B = new JobHeader.Loader((IJobHeaderParent)shipment1).TryLoadOrCreate(otherBranch);
			header1B.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header1B.JH_OA_LocalChargesAddr = client2.MainAddress.PK;

			var header1A = new JobHeader.Loader((IJobHeaderParent)shipment1).TryLoadOrCreate(GlbBranch.CurrentBranch);
			header1A.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header1A.JH_OA_LocalChargesAddr = client1.MainAddress.PK;

			var header2B = new JobHeader.Loader((IJobHeaderParent)shipment2).TryLoadOrCreate(otherBranch);
			header2B.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header2B.JH_OA_LocalChargesAddr = client1.MainAddress.PK;

			var header2A = new JobHeader.Loader((IJobHeaderParent)shipment2).TryLoadOrCreate(GlbBranch.CurrentBranch);
			header2A.JH_GE = GlbDepartment.CurrentDepartment.PK;
			header2A.JH_OA_LocalChargesAddr = client2.MainAddress.PK;

			Booking1.ConsolidationSingleJob.KB_ParentID = shipment1.PK;
			Booking1.ConsolidationSingleJob.KB_ParentTableCode = shipment1.TablePrefix;
			Booking2.ConsolidationSingleJob.KB_ParentID = shipment2.PK;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = shipment2.TablePrefix;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.LocalClient];

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, Booking5);

			filter.Property = client1.PK;
			Asserter.AssertMatches("Client1", filter, Booking1);

			filter.Property = client2.PK;
			Asserter.AssertMatches("Client2", filter, Booking2);
		}

		public void TestLocalClientFilter_StandaloneBooking()
		{
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = new JobHeader.Loader(Booking1).TryLoadOrCreate(GlbBranch.CurrentBranch);
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.JH_OA_LocalChargesAddr = client1.MainAddress.PK;

			Booking1.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty).E2_OA_Address = client2.MainAddress.PK;
			Booking2.DocAddresses.AddNew(DocAddressType.ClientRequestedBillingParty).E2_OA_Address = client2.MainAddress.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.LocalClient];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = client1.PK;
			Asserter.AssertMatches("Client1", filter, Booking1);

			filter.Property = client2.PK;
			Asserter.AssertMatches("Client2", filter, Booking2);
		}

		public void TestCTOReferenceFilters()
		{
			TestReferenceNumFiltersCore(FilterNameConstants.CTOReference, OrganisationTypesList.Codes.CTO);
		}

		public void TestCYDReferenceFilters()
		{
			TestReferenceNumFiltersCore(FilterNameConstants.CYDReference, OrganisationTypesList.Codes.CYD);
		}

		void TestReferenceNumFiltersCore(string filterName, string addressType)
		{
			var bookingWithNoInstructions = Helper.CreateBooking();
			var bookingWithNoConfirmation = Helper.CreateBooking();
			var instructionWithNoConfirmation = Helper.CreateInstruction(bookingWithNoConfirmation, InstructionTypes.Codes.PickUp);
			instructionWithNoConfirmation.OrganisationType = addressType;

			var bookingWithCFSConfirmation = Helper.CreateBooking();
			var cfsInstruction = Helper.CreateInstruction(bookingWithCFSConfirmation, InstructionTypes.Codes.PickUp);
			var cfsConfirmation = Helper.CreateConfirmation(cfsInstruction, InstructionTypes.Codes.PickUp);
			cfsConfirmation.KK_ReferenceNum = "R1";
			cfsInstruction.OrganisationType = OrganisationTypesList.Codes.CFS;

			var bookingWithOneEnteredAddressTypeReference = Helper.CreateBooking();
			var addressTypeOneInstruction = Helper.CreateInstruction(bookingWithOneEnteredAddressTypeReference, InstructionTypes.Codes.PickUp);
			addressTypeOneInstruction.OrganisationType = addressType;
			var addressTypeOneConfirmation = Helper.CreateConfirmation(addressTypeOneInstruction, InstructionTypes.Codes.PickUp);
			addressTypeOneConfirmation.KK_ReferenceNum = "R2";

			var bookingWithTwoEnteredAddressTypeReferences = Helper.CreateBooking();
			var instructionInBookingWithTwoAddressTypeRefs = Helper.CreateInstruction(bookingWithTwoEnteredAddressTypeReferences, InstructionTypes.Codes.PickUp);
			instructionInBookingWithTwoAddressTypeRefs.OrganisationType = addressType;
			var addressTypeConfirmation1 = Helper.CreateConfirmation(instructionInBookingWithTwoAddressTypeRefs, InstructionTypes.Codes.PickUp);
			addressTypeConfirmation1.KK_ReferenceNum = "R2";
			var addressTypeConfirmation2 = Helper.CreateConfirmation(instructionInBookingWithTwoAddressTypeRefs, InstructionTypes.Codes.PickUp);
			addressTypeConfirmation2.KK_ReferenceNum = "R4";
			Factory.Save();
			Asserter.AddToScope(bookingWithNoInstructions, bookingWithNoConfirmation, bookingWithCFSConfirmation, bookingWithOneEnteredAddressTypeReference, bookingWithTwoEnteredAddressTypeReferences);

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[filterName];
			filter.Property = "R1";
			Asserter.AssertMatches($"There is no R1 reference for {addressType} and filter name {filterName}.", filter);

			filter.Property = "R2";
			Asserter.AssertMatches($"There are two bookings with R2 reference for {addressType} and filter name {filterName}.", filter,
				bookingWithOneEnteredAddressTypeReference, bookingWithTwoEnteredAddressTypeReferences);

			filter.Property = "R4";
			Asserter.AssertMatches($"There are one booking with R4 reference for {addressType} and filter name {filterName}.", filter,
				bookingWithTwoEnteredAddressTypeReferences);

			filter.Property = "R";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Asserter.AssertMatches($"There are two bookings starts with reference number R for {addressType} and filter name {filterName}.", filter,
				bookingWithOneEnteredAddressTypeReference, bookingWithTwoEnteredAddressTypeReferences);

			filter.Property = "R";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches($"There are no bookings with reference number R for {addressType} and filter name {filterName}.", filter);

			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches($"There are no bookings with reference number XXX for {addressType} and filter name {filterName}.", filter);
		}

		public void TestBookingStatusFilter()
		{
			TestBookingTextFilter(FilterNameConstants.BookingStatus, DtbBookingSchema.KM_Status);
			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingStatus];
			var bookingStatusCodes = filter.List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			CombineAssertions("Should only contain the codes related to booking status", () =>
			{
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status", TransportStatuses.Codes.Allocated, bookingStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status", TransportStatuses.Codes.Booked, bookingStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status", TransportStatuses.Codes.DeliveryAllocated, bookingStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status", TransportStatuses.Codes.Incomplete, bookingStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status", TransportStatuses.Codes.PickUpAllocated, bookingStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status", TransportStatuses.Codes.PickUpCommenced, bookingStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status", TransportStatuses.Codes.PickUpConfirmed, bookingStatusCodes);
			});
			var bookingStatuses = new BookingStatuses();
			AssertContainsExactElementsInAnyOrder("Should contain the status codes listed in BookingStatuses", bookingStatuses.GetAllCodes(), bookingStatusCodes);
		}

		public void TestInstructionStatusFilter()
		{
			TestInstructionTextFilter(FilterNameConstants.InstructionStatus, DtbBookingInstructionSchema.KN_Status);
			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.InstructionStatus];
			var bookingInstructionStatusCodes = filter.List.Cast<CodeDescriptionPair>().Select(pair => pair.Code);
			CombineAssertions("Should only contain the codes related to booking instruction status", () =>
			{
				AssertCollectionNotContains("Should not contain status codes unrelated to booking instruction status", TransportStatuses.Codes.Deactivated, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking instruction status", TransportStatuses.Codes.DeliveredEmptyNotReturned, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking instruction status", TransportStatuses.Codes.ActionRequired, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking instruction status", TransportStatuses.Codes.Held, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking instruction status", TransportStatuses.Codes.Quote, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking instruction status", TransportStatuses.Codes.ServiceCommenced, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking instruction status", TransportStatuses.Codes.Allocated, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking instruction status", TransportStatuses.Codes.Booked, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking instruction status", TransportStatuses.Codes.DeliveryAllocated, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking instruction status", TransportStatuses.Codes.Incomplete, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking instruction status", TransportStatuses.Codes.PickUpAllocated, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking instruction status", TransportStatuses.Codes.PickUpCommenced, bookingInstructionStatusCodes);
				AssertCollectionNotContains("Should not contain status codes unrelated to booking status, let alone booking instruction status", TransportStatuses.Codes.PickUpConfirmed, bookingInstructionStatusCodes);
			});
			var bookingInstructionStatuses = new BookingInstructionStatuses();
			AssertContainsExactElementsInAnyOrder("Should contain the status codes listed in BookingInstructionStatuses", bookingInstructionStatuses.GetAllCodes(), bookingInstructionStatusCodes);
		}

		public void TestBookingConsolidatedFilter()
		{
			var consolidation1 = Helper.CreateConsolidationMultiJob();
			var consolidation2 = Helper.CreateConsolidationMultiJob();

			consolidation1.Bookings.Add(Booking1);
			consolidation2.Bookings.Add(Booking2);

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingConsolidated];
			filter.Property = BookingConsolidatedStatuses.Codes.All;
			Asserter.AssertMatches("All", filter, Booking1, Booking2, Booking3, Booking4);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingConsolidated];
			filter.Property = BookingConsolidatedStatuses.Codes.Consolidated;
			Asserter.AssertMatches("All", filter, Booking1, Booking2);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingConsolidated];
			filter.Property = BookingConsolidatedStatuses.Codes.Unconsolidated;
			Asserter.AssertMatches("All", filter, Booking3, Booking4);
		}

		public void TestIsOverriddenFilter()
		{
			var consolidation1 = Helper.CreateConsolidation();
			var consolidation2 = Helper.CreateConsolidation();
			consolidation1.Bookings.Add(Booking1);
			consolidation2.Bookings.Add(Booking2);

			var shipment1 = (BusinessObject)Factory.New<IForwardingShipment>();
			var shipment2 = (BusinessObject)Factory.New<IForwardingShipment>();
			consolidation1.KB_ParentID = shipment1.PK;
			consolidation1.KB_ParentTableCode = shipment1.TablePrefix;
			consolidation2.KB_ParentID = shipment2.PK;
			consolidation2.KB_ParentTableCode = shipment2.TablePrefix;
			consolidation1.KB_IsOverridden = false;
			consolidation2.KB_IsOverridden = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.IsOverridden];
			filter.Property = IsOverriddenStatuses.Codes.All;
			Asserter.AssertMatches("All", filter, Booking1, Booking2, Booking3, Booking4);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.IsOverridden];
			filter.Property = IsOverriddenStatuses.Codes.OverridenOrStandalone;
			Asserter.AssertMatches("OverridenOrStandalone", filter, Booking2, Booking3, Booking4);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.IsOverridden];
			filter.Property = IsOverriddenStatuses.Codes.NotOverridden;
			Asserter.AssertMatches("NotOverridden", filter, Booking1);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.IsOverridden];
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter.Visibility);
		}

		public void TestBookingShowStandaloneFilter()
		{
			var job = Factory.New<DummyWithDtbBooking>();
			var bookingForJob = Helper.CreateBooking(Helper.CreateConsolidation(job));
			bookingForJob.KM_JobID = "bookingForJob";
			var bookingStandalone = Helper.CreateBooking();
			bookingStandalone.KM_JobID = "bookingStandalone";
			Factory.Save();

			var asserter = new FilterStripAsserter<DtbBooking>(Factory, b => b.KM_JobID);
			asserter.AddToScope(bookingForJob);
			asserter.AddToScope(bookingStandalone);

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingShowStandalone];
			filter.Property = BookingShowStandaloneValues.Codes.All;
			asserter.AssertMatches("All", filter, bookingForJob, bookingStandalone);

			filter.Property = BookingShowStandaloneValues.Codes.ExcludeStandaloneBookings;
			asserter.AssertMatches("ExcludeStandaloneBookings", filter, bookingForJob);

			filter.Property = BookingShowStandaloneValues.Codes.ShowStandaloneBookingsOnly;
			asserter.AssertMatches("ShowStandaloneBookingsOnly", filter, bookingStandalone);
		}

		public void TestIsHazardousFilter()
		{
			Booking1.KM_IsHazardous = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsHazardous];
			filter.Property = IsHazardousStatuses.Codes.All;
			Asserter.AssertMatches("All", filter, Booking1, Booking2, Booking3, Booking4);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsHazardous];
			filter.Property = IsHazardousStatuses.Codes.Hazardous;
			Asserter.AssertMatches("Hazardous", filter, Booking1);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsHazardous];
			filter.Property = IsHazardousStatuses.Codes.NotHazardous;
			Asserter.AssertMatches("NotHazardous", filter, Booking2, Booking3, Booking4);
		}

		public void TestRequiresRefrigerationFilter()
		{
			Booking1.KM_RequiresRefrigeration = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingRequiresRefrigeration];
			filter.Property = RequiresRefrigerationStatuses.Codes.All;
			Asserter.AssertMatches("All", filter, Booking1, Booking2, Booking3, Booking4);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingRequiresRefrigeration];
			filter.Property = RequiresRefrigerationStatuses.Codes.RequiresRefrigeration;
			Asserter.AssertMatches("Refrigeration", filter, Booking1);

			filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingRequiresRefrigeration];
			filter.Property = RequiresRefrigerationStatuses.Codes.NotRequiresRefrigeration;
			Asserter.AssertMatches("Requires NO Refrigeration", filter, Booking2, Booking3, Booking4);
		}

		public void TestQuoteStatusFilter()
		{
			var bookingWithNoCharges = Booking1;
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = bookingWithNoCharges.PK;

			var bookingWithNoChargesAndNoJob = Booking2;

			var bookingWithCharges = Booking3;
			jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = bookingWithCharges.PK;
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingShowQuotes];
			filter.Property = BookingQuoteStatuses.Codes.All;
			Asserter.AssertMatches("All", filter, bookingWithNoCharges, bookingWithNoChargesAndNoJob, bookingWithCharges, Booking4);

			filter.Property = BookingQuoteStatuses.Codes.QuotesOnly;
			Asserter.AssertMatches("Quotes only", filter, bookingWithCharges);

			filter.Property = BookingQuoteStatuses.Codes.BookingsOnly;
			Asserter.AssertMatches("Bookings only", filter, bookingWithNoCharges, bookingWithNoChargesAndNoJob, Booking4);
		}

		public void TestInstructionCityFilter()
		{
			TestInstructionAddressTextFilter(FilterNameConstants.InstructionAddressCity, null, JobDocAddressSchema.E2_City);
		}

		public void TestInstructionStateFilter()
		{
			TestInstructionAddressTextFilter(FilterNameConstants.InstructionAddressState, null, JobDocAddressSchema.E2_State);
		}

		public void TestInstructionPostCode()
		{
			TestInstructionAddressTextFilter(FilterNameConstants.InstructionAddressPostCode, null, JobDocAddressSchema.E2_Postcode);
		}

		public void TestPackageIdFilter()
		{
			AssertPackageFilter(PkgPackage.Schema.KP_PackageID, "Package ID/Container #");
		}

		public void TestPackageId_DirectFilter()
		{
			AssertPackageFilter(PkgPackage.Schema.KP_PackageID, "Package ID/Container # (Assigned)", assignedToBookingOnly: true);
		}

		public void TestPackageTypeFilter()
		{
			var filter = (ModuleTextFilter)FilterStrip["Package Type"];
			var packTypeCodes = filter.List.Cast<RefPackType>().Select(r => r.F3_Code);
			AssertCollectionContains("PLT", packTypeCodes);
			AssertCollectionContains("BOX", packTypeCodes);
			AssertCollectionContains("CNT", packTypeCodes);

			AssertPackageFilter(PkgPackageSchema.KP_F3_NKPackType.Name, "Package Type");
		}

		void AssertPackageFilter(ZString packageColumn, ZString filterName, bool assignedToBookingOnly = false)
		{
			var consolidation1 = Helper.CreateConsolidation();
			var booking1a = Helper.CreateBooking(consolidation1);
			var booking1b = Helper.CreateBooking(consolidation1);
			var packageJob1 = consolidation1.PackageJob;
			var consolidation1Package = packageJob1.Packages.AddNew("PLT");
			var booking1aPackage = packageJob1.Packages.AddNew("PLT");
			var booking1bPackage = packageJob1.Packages.AddNew("PLT");
			booking1a.Instructions.AddNew().DivotsWithPackages.AddPackage(booking1aPackage);
			booking1b.Instructions.AddNew().DivotsWithPackages.AddPackage(booking1bPackage);

			booking1aPackage[packageColumn] = "p1a";
			booking1bPackage[packageColumn] = "p1b";
			consolidation1Package[packageColumn] = "p1c";

			var consolidation2 = Helper.CreateConsolidation();
			var booking2a = Helper.CreateBooking(consolidation2);
			var booking2b = Helper.CreateBooking(consolidation2);
			var packageJob2 = consolidation2.PackageJob;
			var consolidation2Package = packageJob2.Packages.AddNew("PLT");
			var booking2abPackage = packageJob2.Packages.AddNew("PLT");
			booking2a.Instructions.AddNew().DivotsWithPackages.AddPackage(booking2abPackage);
			booking2b.Instructions.AddNew().DivotsWithPackages.AddPackage(booking2abPackage);

			booking2abPackage[packageColumn] = "p2a";
			consolidation2Package[packageColumn] = "p2c";

			Factory.Save();

			Asserter.AddToScope(booking1a, booking1b, booking2a, booking2b);
			var filter = (ModuleTextFilter)FilterStrip[filterName];

			if (assignedToBookingOnly)
			{
				filter.Property = "p1a";
				Asserter.AssertMatches("Equals 'p1a' should return only Booking1a.", filter, booking1a);
				filter.Property = "p1b";
				Asserter.AssertMatches("Equals 'p1b' should return only Booking1b.", filter, booking1b);
				filter.Property = "p1c";
				Asserter.AssertMatches("Equals 'p1c' should not match any Bookings as this Package is not directly assigned.", filter, Array.Empty<DtbBooking>());
			}
			else
			{
				filter.Property = "p1a";
				Asserter.AssertMatches("Equals 'p1a' should return both Booking1a and Booking1b.", filter, booking1a, booking1b);
				filter.Property = "p1b";
				Asserter.AssertMatches("Equals 'p1b' should return both Booking1a and Booking1b.", filter, booking1a, booking1b);
				filter.Property = "p1c";
				Asserter.AssertMatches("Equals 'p1c' should return both Booking1a and Booking1b.", filter, booking1a, booking1b);
			}

			filter.Property = "p2a";
			Asserter.AssertMatches("Equals 'p2a' should return only Booking2a and Booking2b.", filter, booking2a, booking2b);
			filter.Property = "p2c";
			if (assignedToBookingOnly)
			{
				Asserter.AssertMatches("Equals 'p2c' should not match any Bookings as this Package is not directly assigned.", filter, Array.Empty<DtbBooking>());
			}
			else
			{
				Asserter.AssertMatches("Equals 'p2c' should return only Booking2a and Booking2b.", filter, booking2a, booking2b);
			}

			filter.Property = "1";
			Asserter.AssertMatches("Equals '1' should return no Bookings. No bookings contain a Package with this ID.", filter);
		}

		public void TestInstructionDropModeFilter()
		{
			TestInstructionTextFilter(FilterNameConstants.InstructionDropMode, DtbBookingInstructionSchema.KN_DropMode);
		}

		public void TestContainerTypeFilter()
		{
			var bookingConsolidation1 = Helper.CreateConsolidation();
			bookingConsolidation1.Bookings.Add(Booking1);
			var packageJob1 = Helper.CreatePackageJob(bookingConsolidation1);
			var container1 = bookingConsolidation1.PackageJob.Packages.AddNew("CNT");
			container1.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK; // ContainerType will be DRY

			var bookingConsolidation2 = Helper.CreateConsolidation();
			bookingConsolidation2.Bookings.Add(Booking2);
			var packageJob2 = Helper.CreatePackageJob(bookingConsolidation2);
			var container2 = bookingConsolidation2.PackageJob.Packages.AddNew("CNT");
			container2.Container.K0_RC_ContainerType = Helper.LoadRefContainer("40FR").PK; // ContainerType will be FLT

			var bookingConsolidation3 = Helper.CreateConsolidation();
			bookingConsolidation3.Bookings.Add(Booking3);
			var packageJob3 = Helper.CreatePackageJob(bookingConsolidation3);
			var container3 = bookingConsolidation3.PackageJob.Packages.AddNew("CNT");
			container3.Container.K0_RC_ContainerType = Helper.LoadRefContainer("20GP").PK; // ContainerType will be DRY

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.ContainerType];
			filter.Property = "DRY";
			Asserter.AssertMatches("", filter, Booking1, Booking3);

			filter.Property = "FLT";
			Asserter.AssertMatches("", filter, Booking2);

			filter.Property = "notACode";
			Asserter.AssertMatches("", filter);
		}

		public void TestBookingTransportModeFilter()
		{
			Booking1.KM_TransportMode = "ROA";
			Booking2.KM_TransportMode = "ROA";
			Booking3.KM_TransportMode = "RAI";
			Booking4.KM_TransportMode = "RAI";
			Booking5.KM_TransportMode = "IWT";
			Booking6.KM_TransportMode = "IWT";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingTransportMode];
			filter.Property = BookingTransportModes.Codes.RoadTransport;
			Asserter.AssertMatches("Should match all bookings with Transport Mode 'ROA'.", filter, Booking1, Booking2);

			filter.Property = BookingTransportModes.Codes.RailTransport;
			Asserter.AssertMatches("Should match all bookings with Transport Mode 'RAI'.", filter, Booking3, Booking4);

			filter.Property = BookingTransportModes.Codes.InlandWaterways;
			Asserter.AssertMatches("Should match all bookings with Transport Mode 'IWT'.", filter, Booking5, Booking6);
		}

		void SetupParentJobTypeData()
		{
			var forwardingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipment.FillWithValidTestData();

			var agencyShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			agencyShipment[JobShipmentSchema.JS_IsShipping] = true;
			agencyShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			agencyShipment.FillWithValidTestData();

			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();

			var customsDeclaration = (BusinessObject)Factory.New<Customs.IBaseJobDeclaration>();
			customsDeclaration.FillWithValidTestData();

			var jobConsol = Factory.New<IForwardingConsol>();
			jobConsol.JK_IsForwarding = true;
			jobConsol.JK_IsCancelled = false;

			var quotedBooking = (BusinessObject)Factory.New<IForwardingShipment>();
			quotedBooking[JobShipmentSchema.JS_IsBooking] = true;
			quotedBooking[JobShipmentSchema.JS_IsForwardRegistered] = false;
			quotedBooking.FillWithValidTestData();

			var whsReceive = (BusinessObject)Factory.New<IWhsReceive>();
			whsReceive.FillWithValidTestData();

			var agentBooking = (BusinessObject)Factory.New<DtbAgentBooking>();
			agentBooking.FillWithValidTestData();
			agentBooking[DtbAgentBookingSchema.LTB_KM_TransportBooking] = Booking9.PK;

			var dispatchConsignment = (BusinessObject)Factory.New<ITransitDispatchConsignment>();
			dispatchConsignment.FillWithValidTestData();

			var hvlvConsignment = (BusinessObject)Factory.New<IHVLVConsignment>();
			hvlvConsignment.FillWithValidTestData();

			var hvlvBookingHeader = (BusinessObject)Factory.New<IHVLVBookingHeader>();
			hvlvBookingHeader.FillWithValidTestData();

			Booking1.ConsolidationSingleJob.KB_ParentID = forwardingShipment.PK;
			Booking2.ConsolidationSingleJob.KB_ParentID = agencyShipment.PK;
			Booking3.ConsolidationSingleJob.KB_ParentID = whsOrder.PK;
			Booking4.ConsolidationSingleJob.KB_ParentID = customsDeclaration.PK;
			Booking5.ConsolidationSingleJob.KB_ParentID = ZGuid.Empty;
			Booking6.ConsolidationSingleJob.KB_ParentID = jobConsol.PK;
			Booking7.ConsolidationSingleJob.KB_ParentID = quotedBooking.PK;
			Booking8.ConsolidationSingleJob.KB_ParentID = whsReceive.PK;
			Booking9.ConsolidationSingleJob.KB_ParentID = agentBooking.PK;
			Booking10.ConsolidationSingleJob.KB_ParentID = dispatchConsignment.PK;
			Booking11.ConsolidationSingleJob.KB_ParentID = hvlvConsignment.PK;
			Booking12.ConsolidationSingleJob.KB_ParentID = hvlvBookingHeader.PK;

			Booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking3.ConsolidationSingleJob.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			Booking4.ConsolidationSingleJob.KB_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			Booking5.ConsolidationSingleJob.KB_ParentTableCode = string.Empty;
			Booking6.ConsolidationSingleJob.KB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Booking7.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Booking8.ConsolidationSingleJob.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			Booking9.ConsolidationSingleJob.KB_ParentTableCode = DtbAgentBookingSchema.Constants.Prefix;
			Booking10.ConsolidationSingleJob.KB_ParentTableCode = WhsItemDispatchConsignmentSchema.Constants.Prefix;
			Booking11.ConsolidationSingleJob.KB_ParentTableCode = HVLVConsignmentSchema.Constants.Prefix;
			Booking12.ConsolidationSingleJob.KB_ParentTableCode = HVLVBookingHeaderSchema.Constants.Prefix;

			Factory.Save();
		}

		void ParentJobTypeFilterTestCore(string filterProperty, SQLComparisonOperator comparisonOperator, DtbBooking[] expectedBookings, string assertMessage)
		{
			SetupParentJobTypeData();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.ParentJobType];
			filter.Property = filterProperty;
			filter.SqlComparisonOperator = comparisonOperator;
			Asserter.AssertMatches(assertMessage, filter, expectedBookings);
		}

		public void TestParentJobTypeFilterDoesntBreak()
		{
			using (SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = true;

				SetupParentJobTypeData();

				using (var dtbBookingModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.DtbBooking))
				using (var form = new ZForm())
				{
					form.Controls.Add(dtbBookingModule.EmbeddedControl);
					form.Show();

					((ModuleTextFilter)((ZFilterStripCommonControl)dtbBookingModule.EmbeddedControl).FilterBusinessObject[FilterNameConstants.ParentJobType]).Property = "CUS";
					((ModuleTextFilter)((ZFilterStripCommonControl)dtbBookingModule.EmbeddedControl).FilterBusinessObject[FilterNameConstants.ParentJobType]).SqlComparisonOperator = SQLComparisonOperator.Equal;
					((ModuleTextFilter)((ZFilterStripCommonControl)dtbBookingModule.EmbeddedControl).FilterBusinessObject[FilterNameConstants.ParentJobType]).IsActive = true;

					((ZFilterStripCommonControl)dtbBookingModule.EmbeddedControl).Find();

					const string expectedCUSFilter = @"((KM_KB_Booking IN (SELECT KB_PK FROM dbo.DtbBookingConsolidation WHERE KB_IsOverridden = 1 or KB_ParentID is null)) and KM_IsActive = 1 and (KM_KB_Booking IN (SELECT KB_PK FROM dbo.DtbBookingConsolidation WHERE KB_ParentID In (SELECT VP_PK FROM dbo.ViewTransportBookingParents WHERE VP_JobType = 'CUS'
)))) and (KM_JobType = 'BKG')";
					AssertEquals("Relationship filter should include correct filter for CUS transport bookings.", expectedCUSFilter, ((ZFilterStripCommonControl)dtbBookingModule.EmbeddedControl).GridCollection.RelationshipFilter.LiteralTextADO);

					((ModuleTextFilter)((ZFilterStripCommonControl)dtbBookingModule.EmbeddedControl).FilterBusinessObject[FilterNameConstants.ParentJobType]).Property = "STB";

					((ZFilterStripCommonControl)dtbBookingModule.EmbeddedControl).Find();

					AssertNotEquals("Relationship filter should have been refreshed.", expectedCUSFilter, ((ZFilterStripCommonControl)dtbBookingModule.EmbeddedControl).GridCollection.RelationshipFilter.LiteralTextADO);
				}
			}
		}

		public void TestParentJobTypeEqualsForwardingShipment()
		{
			ParentJobTypeFilterTestCore("SHP", SQLComparisonOperator.Equal, new[] { Booking1, Booking7 }, "Expected only Forwarding Shipments and Forwarding Bookings/ Forwarding Bookings With Quote.");
		}

		public void TestParentJobTypeEqualsAgencyShipment()
		{
			ParentJobTypeFilterTestCore("ASH", SQLComparisonOperator.Equal, new[] { Booking2 }, "Expected only Agency Shipments.");
		}

		public void TestParentJobTypeEqualsWarehouseOrders()
		{
			ParentJobTypeFilterTestCore("WHO", SQLComparisonOperator.Equal, new[] { Booking3 }, "Expected only Warehouse Orders.");
		}

		public void TestParentJobTypeEqualsWarehouseReceipts()
		{
			ParentJobTypeFilterTestCore("WHR", SQLComparisonOperator.Equal, new[] { Booking8 }, "Expected only Warehouse Receipts.");
		}

		public void TestParentJobTypeEqualsCustomsJobs()
		{
			ParentJobTypeFilterTestCore("CUS", SQLComparisonOperator.Equal, new[] { Booking4 }, "Expected only CustomsJobs.");
		}

		public void TestParentJobTypeEqualsForwardingConsolidations()
		{
			ParentJobTypeFilterTestCore("CON", SQLComparisonOperator.Equal, new[] { Booking6 }, "Expected only Forwarding Consolidations.");
		}

		public void TestParentJobTypeEqualsStandaloneBookings()
		{
			ParentJobTypeFilterTestCore("STB", SQLComparisonOperator.Equal, new[] { Booking5, Booking9 }, "Expected only Standalone Bookings/ Agent Bookings.");
		}

		public void TestParentJobTypeEqualsDispatchConsignment()
		{
			ParentJobTypeFilterTestCore("TWD", SQLComparisonOperator.Equal, new[] { Booking10 }, "Expected only Dispatch Consignments.");
		}

		public void TestParentJobTypeEqualsHVLVConsignment()
		{
			ParentJobTypeFilterTestCore("HVC", SQLComparisonOperator.Equal, new[] { Booking11 }, "Expected only HVLV Consignments.");
		}

		public void TestParentJobTypeEqualsHVLVBookingHeader()
		{
			ParentJobTypeFilterTestCore("HVH", SQLComparisonOperator.Equal, new[] { Booking12 }, "Expected only HVLV Booking Headers.");
		}

		public void TestParentJobTypeNotEqualsForwardingShipment()
		{
			ParentJobTypeFilterTestCore("SHP", SQLComparisonOperator.NotEqual, new[] { Booking2, Booking3, Booking4, Booking5, Booking6, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Forwarding Bookings/ Forwarding Bookings With Quote.");
		}

		public void TestParentJobTypeNotEqualsAgencyShipment()
		{
			ParentJobTypeFilterTestCore("ASH", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Agency Shipments.");
		}

		public void TestParentJobTypeNotEqualsWarehouseOrders()
		{
			ParentJobTypeFilterTestCore("WHO", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Warehouse Orders.");
		}

		public void TestParentJobTypeNotEqualsWarehouseReceipts()
		{
			ParentJobTypeFilterTestCore("WHR", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Warehouse Receipts.");
		}

		public void TestParentJobTypeNotEqualsCustomsJobs()
		{
			ParentJobTypeFilterTestCore("CUS", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2, Booking3, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except CustomsJobs.");
		}

		public void TestParentJobTypeNotEqualsForwardingConsolidations()
		{
			ParentJobTypeFilterTestCore("CON", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Forwarding Consolidations.");
		}

		public void TestParentJobTypeNotEqualsStandaloneBookings()
		{
			ParentJobTypeFilterTestCore("STB", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2, Booking3, Booking4, Booking6, Booking7, Booking8, Booking10, Booking11, Booking12 }, "Expected all booking types except Standalone Bookings/ Agent Bookings.");
		}

		public void TestParentJobTypeNotEqualsDispatchConsignment()
		{
			ParentJobTypeFilterTestCore("TWD", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking11, Booking12 }, "Expected all booking types except Dispatch Consignments.");
		}

		public void TestParentJobTypeNotEqualsHVLVConsignment()
		{
			ParentJobTypeFilterTestCore("HVC", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10, Booking12 }, "Expected all booking types except HVLV Consignments.");
		}

		public void TestParentJobTypeNotEqualsHVLVBookingHeader()
		{
			ParentJobTypeFilterTestCore("HVH", SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10, Booking11 }, "Expected all booking types except HVLV Booking Headers.");
		}

		public void TestParentJobTypeStartsWithS()
		{
			ParentJobTypeFilterTestCore("S", SQLComparisonOperator.StartsWith, new[] { Booking1, Booking7, Booking5, Booking9 }, "Expected only Forwarding Shipments, Standalone Bookings/ Agent Bookings and Forwarding Bookings/ Forwarding Bookings With Quote.");
		}

		public void TestParentJobTypeStartsWithA()
		{
			ParentJobTypeFilterTestCore("A", SQLComparisonOperator.StartsWith, new[] { Booking2 }, "Expected only Agency Shipments.");
		}

		public void TestParentJobTypeStartsWithW()
		{
			ParentJobTypeFilterTestCore("W", SQLComparisonOperator.StartsWith, new[] { Booking3, Booking8 }, "Expected only Warehouse Orders and Warehouse Receipts.");
		}

		public void TestParentJobTypeStartsWithC()
		{
			ParentJobTypeFilterTestCore("C", SQLComparisonOperator.StartsWith, new[] { Booking4, Booking6 }, "Expected only CustomsJobs and Forwarding Consolidations.");
		}

		public void TestParentJobTypeStartsWithT()
		{
			ParentJobTypeFilterTestCore("T", SQLComparisonOperator.StartsWith, new[] { Booking10 }, "Expected only Dispatch Consignments.");
		}

		public void TestParentJobTypeStartsWithH()
		{
			ParentJobTypeFilterTestCore("H", SQLComparisonOperator.StartsWith, new[] { Booking11, Booking12 }, "Expected only HVLV Consignments and HVLV Booking Headers.");
		}

		public void TestParentJobTypeDoesNotStartWithS()
		{
			ParentJobTypeFilterTestCore("S", SQLComparisonOperator.DoesNotStartWith, new[] { Booking2, Booking3, Booking4, Booking6, Booking8, Booking10, Booking11, Booking12 }, "Expected all booking types except Forwarding Shipments, Standalone Bookings and Forwarding Bookings/ Forwarding Bookings With Quote.");
		}

		public void TestParentJobTypeDoesNotStartWithA()
		{
			ParentJobTypeFilterTestCore("A", SQLComparisonOperator.DoesNotStartWith, new[] { Booking1, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Agency Shipments.");
		}

		public void TestParentJobTypeDoesNotStartWithW()
		{
			ParentJobTypeFilterTestCore("W", SQLComparisonOperator.DoesNotStartWith, new[] { Booking1, Booking2, Booking4, Booking5, Booking6, Booking7, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Warehouse Orders and Warehouse Receipts.");
		}

		public void TestParentJobTypeDoesNotStartWithC()
		{
			ParentJobTypeFilterTestCore("C", SQLComparisonOperator.DoesNotStartWith, new[] { Booking1, Booking2, Booking3, Booking5, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except CustomsJobs and Forwarding Consolidations.");
		}

		public void TestParentJobTypeDoesNotStartWithT()
		{
			ParentJobTypeFilterTestCore("T", SQLComparisonOperator.DoesNotStartWith, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking11, Booking12 }, "Expected all booking types except Dispatch Consignments.");
		}

		public void TestParentJobTypeDoesNotStartWithH()
		{
			ParentJobTypeFilterTestCore("H", SQLComparisonOperator.DoesNotStartWith, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10 }, "Expected all booking types except HVLV Consignments and HVLV Booking Headers.");
		}

		public void TestParentJobTypeContainsT()
		{
			ParentJobTypeFilterTestCore("T", SQLComparisonOperator.Contains, new[] { Booking5, Booking9, Booking10 }, "Expected Standalone Bookings and Dispatch Consignments.");
		}

		public void TestParentJobTypeContainsU()
		{
			ParentJobTypeFilterTestCore("U", SQLComparisonOperator.Contains, new[] { Booking4 }, "Expected only CustomsJobs.");
		}

		public void TestParentJobTypeContainsH()
		{
			ParentJobTypeFilterTestCore("H", SQLComparisonOperator.Contains, new[] { Booking1, Booking2, Booking3, Booking7, Booking8, Booking11, Booking12 }, "Expected all booking types except CustomsJobs, Standalone Bookings, Dispatch Consignments and Forwarding Consolidations.");
		}

		public void TestParentJobTypeContainsO()
		{
			ParentJobTypeFilterTestCore("O", SQLComparisonOperator.Contains, new[] { Booking3, Booking6 }, "Expected only Warehouse Orders and Forwarding Consolidations.");
		}

		public void TestParentJobTypeContainsS()
		{
			ParentJobTypeFilterTestCore("S", SQLComparisonOperator.Contains, new[] { Booking1, Booking2, Booking4, Booking5, Booking7, Booking9 }, "Expected all booking types except Warehouse Orders, Forwarding Consolidations and Warehouse Receipts.");
		}

		public void TestParentJobTypeContainsSH()
		{
			ParentJobTypeFilterTestCore("SH", SQLComparisonOperator.Contains, new[] { Booking1, Booking2, Booking7 }, "Expected only Agency Shipments, Forwarding Shipments and Forwarding Bookings/ Forwarding Bookings With Quote.");
		}

		public void TestParentJobTypeContainsD()
		{
			ParentJobTypeFilterTestCore("D", SQLComparisonOperator.Contains, new[] { Booking10 }, "Expected only Dispatch Consignments.");
		}

		public void TestParentJobTypeContainsV()
		{
			// VB, HVC, HVH
			ParentJobTypeFilterTestCore("V", SQLComparisonOperator.Contains, new[] { Booking7, Booking11, Booking12 }, "Expected bookings containing V.");
		}

		public void TestParentJobTypeContainsC()
		{
			ParentJobTypeFilterTestCore("C", SQLComparisonOperator.Contains, new[] { Booking4, Booking6, Booking11 }, "Expected only HVLV Consignments, Customs Declarations and Forwarding Consolidations..");
		}

		public void TestParentJobTypeDoesNotContainT()
		{
			ParentJobTypeFilterTestCore("T", SQLComparisonOperator.NotContains, new[] { Booking1, Booking2, Booking3, Booking4, Booking6, Booking7, Booking8, Booking11, Booking12 }, "Expected all booking types except Standalone Bookings.");
		}

		public void TestParentJobTypeDoesNotContainU()
		{
			ParentJobTypeFilterTestCore("U", SQLComparisonOperator.NotContains, new[] { Booking1, Booking2, Booking3, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except CustomJobs.");
		}

		public void TestParentJobTypeDoesNotContainH()
		{
			ParentJobTypeFilterTestCore("H", SQLComparisonOperator.NotContains, new[] { Booking4, Booking5, Booking6, Booking9, Booking10 }, "Expected only CustomsJobs, Standalone Bookings/ Agent Bookings and Forwarding Consolidations.");
		}

		public void TestParentJobTypeDoesNotContainO()
		{
			ParentJobTypeFilterTestCore("O", SQLComparisonOperator.NotContains, new[] { Booking1, Booking2, Booking4, Booking5, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Warehouse Orders and Forwarding Consolidations.");
		}

		public void TestParentJobTypeDoesNotContainS()
		{
			ParentJobTypeFilterTestCore("S", SQLComparisonOperator.NotContains, new[] { Booking3, Booking6, Booking8, Booking10, Booking11, Booking12 }, "Expected only Warehouse Orders, Forwarding Consolidations and Warehouse Receipts.");
		}

		public void TestParentJobTypeDoesNotContainSH()
		{
			ParentJobTypeFilterTestCore("SH", SQLComparisonOperator.NotContains, new[] { Booking3, Booking4, Booking5, Booking6, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all booking types except Agency Shipments, Forwarding Shipments and Forwarding Bookings/ Forwarding Bookings With Quote.");
		}

		public void TestParentJobTypeDoesNotContainD()
		{
			ParentJobTypeFilterTestCore("D", SQLComparisonOperator.NotContains, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking11, Booking12 }, "Expected all booking types except Dispatch Consignments");
		}

		public void TestParentJobTypeDoesNotContainV()
		{
			ParentJobTypeFilterTestCore("V", SQLComparisonOperator.NotContains, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10 }, "Expected all booking types except HVLV Consignments and HVLV Booking Headers.");
		}

		public void TestParentJobTypeDoesNotContainC()
		{
			ParentJobTypeFilterTestCore("C", SQLComparisonOperator.NotContains, new[] { Booking1, Booking2, Booking3, Booking5, Booking7, Booking8, Booking9, Booking10, Booking12 }, "Expected all booking types except HVLV Consignments.");
		}

		public void TestParentJobTypeIsBlank()
		{
			ParentJobTypeFilterTestCore(null, SQLComparisonOperator.IsBlank, Array.Empty<DtbBooking>(), "Expected no bookings.");
		}

		public void TestParentJobTypeIsNotBlank()
		{
			ParentJobTypeFilterTestCore(null, SQLComparisonOperator.IsNotBlank, new[] { Booking1, Booking2, Booking3, Booking4, Booking5, Booking6, Booking7, Booking8, Booking9, Booking10, Booking11, Booking12 }, "Expected all bookings.");
		}

		public void TestRelatedWorkflowFilters()
		{
			var relatedFilters = FilterStrip.ModuleFilters.Where(f => f.Category.Description.ToString() == "Workflow Milestones (Related)").ToArray();
			AssertEquals(4, relatedFilters.Length);
			relatedFilters.Single(f => f.Description.ToString() == "Milestone Date (Related)");
			relatedFilters.Single(f => f.Description.ToString() == "Milestone Completed (Related)");
			relatedFilters.Single(f => f.Description.ToString() == "Next Milestone (Related)");
			relatedFilters.Single(f => f.Description.ToString() == "Last Completed Milestone (Related)");
		}

		public void TestRelatedMilestoneDates()
		{
			var year = ZDateTime.Now.Year;

			var booking1 = CreateBookingWithRelatedProcessTask();
			var booking2 = CreateBookingWithRelatedProcessTask();
			booking1.Item2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 2)));
			booking2.Item2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 7)));

			Asserter.AddToScope(booking1.Item1);
			Asserter.AddToScope(booking2.Item1);

			Factory.Save();

			var workflowFilter = (WorkflowModuleFilter)FilterStrip.ModuleFilters["Milestone Date (Related)"];

			workflowFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			workflowFilter.Property1 = new ZDateTime(year, 1, 1);
			workflowFilter.Property2 = new ZDateTime(year, 1, 3);
			workflowFilter.IsActive = true;

			Asserter.AssertMatches("Should only find booking1", workflowFilter, booking1.Item1);
		}

		public void TestRelatedMilestoneCompleted()
		{
			var year = ZDateTime.Now.Year;

			var booking1 = CreateBookingWithRelatedProcessTask();
			var booking2 = CreateBookingWithRelatedProcessTask();
			booking1.Item2.SetMilestoneActualDateForTest(new ZDateTime(year, 1, 2));
			Asserter.AddToScope(booking1.Item1);
			Asserter.AddToScope(booking2.Item1);
			Factory.Save();

			var textFilter = (ModuleTextFilter)FilterStrip.ModuleFilters["Milestone Completed (Related)"];
			textFilter.Property = "Completed";
			textFilter.IsActive = true;
			Asserter.AssertMatches("Should only find booking1", textFilter, booking1.Item1);

			textFilter.Property = "Not Completed";
			textFilter.IsActive = true;
			Asserter.AssertMatches("Should only find booking2", textFilter, booking2.Item1);
		}

		public void TestRelatedMilestoneNext()
		{
			var year = ZDateTime.Now.Year;

			var booking1 = CreateBookingWithRelatedProcessTask();
			var booking2 = CreateBookingWithRelatedProcessTask();

			booking1.Item2.P9_Status = "NXT";
			booking2.Item2.P9_Status = "NXT";
			booking1.Item2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 2)));
			booking2.Item2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 7, 1)));

			Factory.Save();

			Asserter.AddToScope(booking1.Item1);
			Asserter.AddToScope(booking2.Item1);

			var filter = (WorkflowModuleFilter)FilterStrip.ModuleFilters["Next Milestone (Related)"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(year, 1, 1);
			filter.Property2 = new ZDateTime(year, 1, 3);
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, booking1.Item1);
		}

		public void TestRelatedMilestoneLastCompleted()
		{
			var year = ZDateTime.Now.Year;

			var booking1 = CreateBookingWithRelatedProcessTask();
			var booking2 = CreateBookingWithRelatedProcessTask();
			booking1.Item2.P9_Status = "LST";
			booking2.Item2.P9_Status = "LST";
			booking1.Item2.SetMilestoneActualDateForTest(new ZDateTime(year, 1, 2));
			booking2.Item2.SetMilestoneActualDateForTest(new ZDateTime(year, 7, 1));

			Factory.Save();

			Asserter.AddToScope(booking1.Item1);
			Asserter.AddToScope(booking2.Item1);

			var filter = (WorkflowModuleFilter)FilterStrip.ModuleFilters["Last Completed Milestone (Related)"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(year, 1, 1);
			filter.Property2 = new ZDateTime(year, 1, 3);
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, booking1.Item1);
		}

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<DtbBooking>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.DtbBookingCRMSecurity);
		}

		Tuple<DtbBooking, ProcessTask> CreateBookingWithRelatedProcessTask()
		{
			var dummyBizO = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_ParentID = dummyBizO.PK;
			consolidation.KB_ParentTableCode = dummyBizO.TablePrefix;
			var milestone = dummyBizO.WorkflowItems.Milestones.AddNew();

			return Tuple.Create(Helper.CreateBooking(consolidation), milestone);
		}

		public void TestCarrierAccountFilter()
		{
			var transportCo1 = Helper.CreateOrganisation("ORG1");
			var transportCo2 = Helper.CreateOrganisation("ORG2");

			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_AccountNumber = "123";
			carrierAccount.OAN_OH_Carrier = transportCo1.PK;
			carrierAccount.OAN_OH_BillToParty = transportCo2.PK;

			Booking1.KM_OAN_CarrierAccount = carrierAccount.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.CarrierAccount];
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "";
			Asserter.AssertMatches("No filter -- expect all Bookings.", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "123";
			Asserter.AssertMatches("Filtering on Carrier Account -- expect Booking1 only.", filter, Booking1);
		}

		public void TestCartageCoordinatorFilter()
		{
			var orgConsignor = Factory.NewWithValidTestData<OrgHeader>();
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgBookedBy = Factory.NewWithValidTestData<OrgHeader>();
			var orgBillingParty = Factory.NewWithValidTestData<OrgHeader>();

			var staffConsignor = CreateNewStaff("NA1", "Not a name 1", "Not.a.name1");
			AssignStaff(orgConsignor, staffConsignor, "ALL", "CAR");
			AssignStaff(orgBillingParty, staffConsignor, "ALL", "PRJ");

			var staffConsignee = CreateNewStaff("NA2", "Not a name 2", "Not.a.name2");
			AssignStaff(orgConsignee, staffConsignee, "ALL", "CAR");
			AssignStaff(orgBookedBy, staffConsignee, "ALL", "ACT");

			var staffBookedBy = CreateNewStaff("NA3", "Not a name 3", "Not.a.name3");
			AssignStaff(orgBookedBy, staffBookedBy, "ALL", "CAR");
			AssignStaff(orgConsignee, staffBookedBy, "ALL", "CON");

			var staffBillingParty = CreateNewStaff("NA4", "Not a name 4", "Not.a.name4");
			AssignStaff(orgBillingParty, staffBillingParty, "ALL", "CAR");
			AssignStaff(orgConsignor, staffBillingParty, "ALL", "CRE");

			var instruction1 = Booking1.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			instruction1.Address.OrganisationPK = orgConsignor.PK;
			var instruction2 = Booking2.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR);
			instruction2.Address.OrganisationPK = orgConsignor.PK;
			var instruction3 = Booking2.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE);
			instruction3.Address.OrganisationPK = orgConsignee.PK;
			var instruction4 = Booking3.Instructions.AddNew(InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE);
			instruction4.Address.OrganisationPK = orgConsignee.PK;

			Booking1.ConsolidationSingleJob.BookedByAddress.OrganisationPK = orgBookedBy.PK;
			Booking3.BillingPartyOrLocalClientPK = orgBillingParty.MainAddress.PK;
			new JobHeader.Loader(Booking3).TryCreate();

			// New booking from created from other module
			var shipment = Factory.New<IForwardingShipment>();
			var shipmentJob = new JobHeader.Loader((IJobHeaderParent)shipment).TryLoadOrCreate();
			shipmentJob.JH_GC = GlbCompany.CurrentCompany.PK;
			shipmentJob.JH_OA_LocalChargesAddr = orgBillingParty.MainAddress.PK;
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var bookingFromShipment = Helper.CreateBooking(consolidation);
			bookingFromShipment.KM_JobID = "TEST001";
			Asserter.AddToScope(bookingFromShipment);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStrip[FilterNameConstants.CartageCoordinator];

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Booking1, Booking2, Booking3, Booking4, bookingFromShipment);

			filter.Property = staffConsignor.PK;
			Asserter.AssertMatches("Consignor", filter, Booking1, Booking2);

			filter.Property = staffConsignee.PK;
			Asserter.AssertMatches("Consignee", filter, Booking2, Booking3);

			filter.Property = staffBookedBy.PK;
			Asserter.AssertMatches("BookedBy", filter, Booking1);

			filter.Property = staffBillingParty.PK;
			Asserter.AssertMatches("BillingParty", filter, Booking3, bookingFromShipment);
		}

		void AssignStaff(OrgHeader org, GlbStaff staff, ZString department, ZString role)
		{
			var assignment = org.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			assignment.O8_Department = department;
			assignment.O8_Role = role;
			assignment.O8_GC = GlbCompany.CurrentCompany.PK;
		}

		GlbStaff CreateNewStaff(ZString code, ZString name, ZString loginName)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = name;
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;
			return staff;
		}

		public void TestCarrierBookingAgentFilter()
		{
			var orgHeader = Helper.CreateOrganisation("TESTORG");
			var orgHeader1 = Helper.CreateOrganisation("TESTORG1");

			Booking1.CarrierBookingAgentDocAddress.OrganisationPK = orgHeader.PK;
			Booking2.CarrierBookingAgentDocAddress.OrganisationPK = orgHeader.PK;
			Booking3.CarrierBookingAgentDocAddress.OrganisationPK = orgHeader1.PK;
			Booking4.CarrierBookingAgentDocAddress.OrganisationPK = ZGuid.Empty;
			Booking5.CarrierBookingAgentDocAddress.OrganisationPK = ZGuid.Empty;

			Factory.Save();

			AssertGuidFilter(FilterNameConstants.CarrierBookingAgent, orgHeader.PK, SQLComparisonOperator.Equal, new[] { Booking1, Booking2 }, typeof(OrgHeaderCollection));
			AssertGuidFilter(FilterNameConstants.CarrierBookingAgent, orgHeader1.PK, SQLComparisonOperator.Equal, new[] { Booking3 }, typeof(OrgHeaderCollection));

			AssertGuidFilter(FilterNameConstants.CarrierBookingAgent, orgHeader.PK, SQLComparisonOperator.NotEqual, new[] { Booking3 }, typeof(OrgHeaderCollection));
			AssertGuidFilter(FilterNameConstants.CarrierBookingAgent, orgHeader1.PK, SQLComparisonOperator.NotEqual, new[] { Booking1, Booking2 }, typeof(OrgHeaderCollection));

			AssertGuidFilter(FilterNameConstants.CarrierBookingAgent, orgHeader1.PK, SQLComparisonOperator.IsBlank, new[] { Booking4, Booking5 }, typeof(OrgHeaderCollection));
			AssertGuidFilter(FilterNameConstants.CarrierBookingAgent, orgHeader1.PK, SQLComparisonOperator.IsNotBlank, new[] { Booking1, Booking2, Booking3 }, typeof(OrgHeaderCollection));
		}

		public void TestTransportBookingsWFCustomFilter()
		{
			var name = "Test";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = name;
			columnDef.XC_Type = MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Integer;

			template.Factory.Save();
			Assert("template should have been saved", template.IsInDatabase);

			Factory.Save();

			var collection = FilterStrip.ModuleFilters;

			AssertNotNull(collection[name]);
			AssertNull(collection[name + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		public void TestAccountingFilterSuffix()
		{
			AssertEquals("Suffix should be added to some accounting filters.", "Job Open (Standalone Booking)", FilterStrip["Job Open"].LocalizedDescription);
		}

		public void TestAccountingRevenueFiltersShouldBeRemoved()
		{
			var expectedAccountingFilterCategoryFilterNames = new[] { "Job Cost Amount", "Job Accrual Amount" };
			var accountingFilter = FilterStrip[expectedAccountingFilterCategoryFilterNames[0]];
			AssertNotNull("Any filter from any accounting filter category", accountingFilter);
			var accountingCategory = accountingFilter.Category;
			var accountingCategoryFilterNames = from filter in FilterStrip
												where filter.Category == accountingCategory
												select filter.Description.ToString();

			AssertContainsExactElementsInAnyOrder("Revenue filters should be removed.", expectedAccountingFilterCategoryFilterNames, accountingCategoryFilterNames);
		}

		public void TestAccountingBillingCategoryName()
		{
			var billingCategoryFilter = FilterStrip["AP Invoice #"];
			AssertNotNull("Any filter from billing category", billingCategoryFilter);
			AssertEquals("Standalone Billing", billingCategoryFilter.Category.Description.ToString());
		}

		public void TestAccountingAmountsCategoryName()
		{
			var amountsCategoryFilter = FilterStrip["Job Cost Amount"];
			AssertNotNull("Any filter from amounts category", amountsCategoryFilter);
			AssertEquals("Standalone Booking Amounts", amountsCategoryFilter.Category.Description.ToString());
		}

		public void TestAccountingJobInvoicingStatus_ParentJobHeader()
		{
			var bookingWithParent = Helper.CreateBooking();
			var bookingStandAlone = Helper.CreateBooking();
			var parent = Helper.CreateForwardingShipment(null, null, null, null);
			bookingWithParent.ConsolidationSingleJob.KB_ParentID = parent.PK;
			bookingWithParent.ConsolidationSingleJob.KB_ParentTableCode = "JS";
			var parentJH = new JobHeader.Loader((IJobHeaderParent)parent).TryLoadOrCreate();
			var bookingJH = new JobHeader.Loader(bookingStandAlone).TryLoadOrCreate();
			parentJH.JH_Status = JobHeaderStatus.Working.Code;
			bookingJH.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			Asserter.AddToScope(bookingWithParent, bookingStandAlone);

			var invoicingStatusFilter = (ModuleTextFilter)FilterStrip["Invoice Status"];
			invoicingStatusFilter.Property = JobHeaderStatus.Working.Code;
			Asserter.AssertMatches(
				"Invoicing status filter applies to parent job header (when there's a parent) and stand alone TB job header (when there's no parent).",
				invoicingStatusFilter,
				bookingWithParent,
				bookingStandAlone);
		}

		void TestBookingConsolidationTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			var bookingConsolidation1 = Helper.CreateConsolidation();
			var bookingConsolidation2 = Helper.CreateConsolidation();
			var bookingConsolidation3 = Helper.CreateConsolidation();

			bookingConsolidation1.Bookings.Add(Booking1);
			bookingConsolidation2.Bookings.Add(Booking2);
			bookingConsolidation3.Bookings.Add(Booking3);

			bookingConsolidation1[schemaColumn.Name] = "abc";
			bookingConsolidation2[schemaColumn.Name] = "def";
			bookingConsolidation3[schemaColumn.Name] = "";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[filterName];
			filter.Property = "abc";
			Asserter.AssertMatches("", filter, Booking1);

			filter.Property = "def";
			Asserter.AssertMatches("", filter, Booking2);

			filter.Property = "notAcode";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("", filter, Booking3, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("", filter, Booking1, Booking2);
		}

		void TestBookingTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			Booking1[schemaColumn.Name] = "i1a";
			Booking2[schemaColumn.Name] = "i2a";
			Booking3[schemaColumn.Name] = "i3";
			Booking4[schemaColumn.Name] = "";

			Factory.Save();

			var value1 = "notAnId";
			value1 = value1.Length > schemaColumn.MaxLength ? value1.Substring(0, schemaColumn.MaxLength) : value1;

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[filterName];
			filter.Property = "i2a";
			Asserter.AssertMatches("", filter, Booking2);

			filter.Property = "a";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Asserter.AssertMatches("", filter, Booking1, Booking2);

			filter.Property = "i";
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3);

			filter.Property = value1;
			Asserter.AssertMatches("", filter);

			filter.Property = "i2a";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("", filter, Booking1, Booking3, Booking4);

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("", filter, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3);
		}

		void TestInstructionTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			var booking5 = GetNewBookingAndAddToScope("Booking5");

			var instruction1 = Booking1.Instructions.AddNew();
			var instruction2 = Booking2.Instructions.AddNew();
			var instruction3 = Booking3.Instructions.AddNew();
			var instruction4 = booking5.Instructions.AddNew(); //Booking4 has no instruction

			instruction1[schemaColumn.Name] = "a1b";
			instruction2[schemaColumn.Name] = "a2b";
			instruction3[schemaColumn.Name] = "a3";
			instruction4[schemaColumn.Name] = "";

			Factory.Save();

			var value1 = "notAnId";
			value1 = value1.Length > schemaColumn.MaxLength ? value1.Substring(0, schemaColumn.MaxLength) : value1;

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[filterName];

			filter.Property = "a1b";
			Asserter.AssertMatches("", filter, Booking1);

			filter.Property = value1;
			Asserter.AssertMatches("", filter);

			filter.Property = "b";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Asserter.AssertMatches("", filter, Booking1, Booking2);

			filter.Property = "a";
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3);

			filter.Property = value1;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3, Booking4, booking5);

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("", filter, booking5);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3);
		}

		void TestInstructionAddressTextFilter(ZString filterName, SchemaColumn orgHeaderSchemaColoumn, SchemaColumn jobDocAddressSchemaColoumn)
		{
			var instruction1 = Booking1.Instructions.AddNew();
			var instruction2 = Booking2.Instructions.AddNew();

			// instruction 1 - DocAddress - Overriden
			instruction1.Address.E2_AddressOverride = true;
			if (jobDocAddressSchemaColoumn != null)
			{
				instruction1.Address[jobDocAddressSchemaColoumn] = "a1";
			}

			// instruction 2 - OrgHeader
			var organisation1 = Helper.CreateOrganisation("CLIENT");
			if (orgHeaderSchemaColoumn != null)
			{
				organisation1[orgHeaderSchemaColoumn] = "a2";
			}
			instruction2.Address.OrganisationPK = organisation1.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[filterName];

			if (jobDocAddressSchemaColoumn != null)
			{
				filter.Property = "a1";
				Asserter.AssertMatches("", filter, Booking1);
			}

			if (orgHeaderSchemaColoumn != null)
			{
				filter.Property = "a2";
				Asserter.AssertMatches("", filter, Booking2);
			}

			if (jobDocAddressSchemaColoumn != null & orgHeaderSchemaColoumn != null)
			{
				filter.Property = "a";
				Asserter.AssertMatches("", filter, Booking1, Booking2);
			}

			filter.Property = "NotACode";
			Asserter.AssertMatches("", filter);

			filter.Property = "NotACode";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3, Booking4);

			// Not Blank

			filter.Property = "";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			if (jobDocAddressSchemaColoumn != null & orgHeaderSchemaColoumn != null)
			{
				Asserter.AssertMatches("", filter, Booking1, Booking2);
			}
			else if (jobDocAddressSchemaColoumn != null)
			{
				Asserter.AssertMatches("", filter, Booking1);
			}
			else if (orgHeaderSchemaColoumn != null)
			{
				Asserter.AssertMatches("", filter, Booking2);
			}

			// Blank

			if (jobDocAddressSchemaColoumn != null)
			{
				instruction1.Address[jobDocAddressSchemaColoumn] = "";
			}

			if (orgHeaderSchemaColoumn != null)
			{
				organisation1[orgHeaderSchemaColoumn] = "";
			}

			Factory.Save();

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("Changed to blank, if they are null, they are blank by default", filter, Booking1, Booking2);
		}

		void TestConfirmationTextFilter(ZString filterName, SchemaColumn schemaColumn)
		{
			var instruction1 = Booking1.Instructions.AddNew();
			var instruction2 = Booking2.Instructions.AddNew();
			var instruction3 = Booking3.Instructions.AddNew();
			var instruction4 = Booking4.Instructions.AddNew();

			var confirmation1 = instruction1.Confirmations.AddNew();
			var confirmation2 = instruction2.Confirmations.AddNew();
			var confirmation3 = instruction3.Confirmations.AddNew();
			var confirmation4 = instruction4.Confirmations.AddNew();

			confirmation1[schemaColumn.Name] = "a1b";
			confirmation2[schemaColumn.Name] = "a2b";
			confirmation3[schemaColumn.Name] = "a3";
			confirmation4[schemaColumn.Name] = "";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[filterName];

			filter.Property = "a1b";
			Asserter.AssertMatches("", filter, Booking1);

			filter.Property = "b";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			Asserter.AssertMatches("", filter, Booking1, Booking2);

			filter.Property = "a";
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3);

			filter.Property = "NotAnId";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("", filter);

			filter.Property = "NotAnId";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3, Booking4);

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("", filter, Booking4);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("", filter, Booking1, Booking2, Booking3);
		}

		void AssertGuidFilter(ZString filterName, ZGuid value, SQLComparisonOperator comparisonOperator, DtbBooking[] expectedResult, Type expectedListType = null)
		{
			var filterBusinessObject = new DtbBookingFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBusinessObject[filterName];

			filter.IsActive = true;
			filter.SqlComparisonOperator = comparisonOperator;
			filter.Property = value;

			var bookingCollection = new DtbBookingCollection(Factory, filterBusinessObject.Filter);
			AssertContainsExactElementsInAnyOrder(expectedResult, bookingCollection);

			if (expectedListType != null)
			{
				AssertEquals(expectedListType, filter.List.GetType());
			}
		}

		void TestConfirmationDateFilter(ZString filterName, ZString filterDescription, ZString confirmationType, SchemaColumn schemaColumn)
		{
			var instruction1 = Booking1.Instructions.AddNew();
			var instruction2 = Booking2.Instructions.AddNew();
			var instruction3 = Booking3.Instructions.AddNew();
			var instruction4 = Booking4.Instructions.AddNew();

			var confirmation1 = instruction1.Confirmations.AddNew();
			var confirmation2 = instruction2.Confirmations.AddNew();
			var confirmation3a = instruction3.Confirmations.AddNew();
			var confirmation3b = instruction3.Confirmations.AddNew();
			var confirmation4 = instruction4.Confirmations.AddNew();

			var now = ZDateTime.Now;

			confirmation1.KK_ConfirmationType = confirmationType;
			confirmation1[schemaColumn.Name] = now.AddDays(4).AddMinutes(30);

			confirmation2.KK_ConfirmationType = confirmationType;
			confirmation2[schemaColumn.Name] = now.AddDays(6);

			confirmation3a.KK_ConfirmationType = confirmationType;
			confirmation3a[schemaColumn.Name] = now.AddDays(8);
			confirmation3b.KK_ConfirmationType = confirmationType;
			confirmation3b[schemaColumn.Name] = ZDateTime.Empty;

			confirmation4.KK_ConfirmationType = confirmationType == ConfirmationTypes.Codes.Delivery ? ConfirmationTypes.Codes.PickUp : ConfirmationTypes.Codes.Delivery;
			confirmation4[schemaColumn.Name] = now.AddDays(4);

			Factory.Save();

			AssertDateRangeFilter(filterDescription, filterName, now.AddDays(3), now.AddDays(5), ModuleDateFilter.SpecifiedDateRange, Booking1);
			AssertDateRangeFilter(filterDescription, filterName, now.AddDays(3), now.AddDays(7), ModuleDateFilter.SpecifiedDateRange, Booking1, Booking2);
			AssertDateRangeFilter(filterDescription, filterName, now.AddDays(7), now.AddDays(9), ModuleDateFilter.SpecifiedDateRange, Booking3);
			AssertDateRangeFilter(filterDescription, filterName, now.AddDays(2), now.AddDays(3), ModuleDateFilter.SpecifiedDateRange);
			AssertDateRangeFilter(filterDescription, filterName, now.AddDays(4).AddMinutes(31), now.AddDays(5), ModuleDateFilter.SpecifiedDateTimeRange);

			AssertNoDateFilter(filterDescription, filterName, Booking3, Booking4);
			AssertHasDateFilter(filterDescription, filterName, Booking1, Booking2, Booking3);
		}

		void AssertDateRangeFilter(ZString filterDescription, ZString filterName, ZDateTime date1, ZDateTime date2, ZString propertySearch, params DtbBooking[] expectedResult)
		{
			var filter = (ModuleDateFilter)FilterStrip.ModuleFilters[filterName];

			filter.IsActive = true;
			filter.PropertySearch = propertySearch;
			filter.Property1 = date1;
			filter.Property2 = date2;
			Asserter.AssertMatches($"{filterDescription}, From {date1} To {date2}", filter, expectedResult);
		}

		void AssertNoDateFilter(ZString filterDescription, ZString filterName, params DtbBooking[] expectedResult)
		{
			var filter = (ModuleDateFilter)FilterStrip.ModuleFilters[filterName];

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches($"{filterDescription}, No Date Entered", filter, expectedResult);
		}

		void AssertHasDateFilter(ZString filterDescription, ZString filterName, params DtbBooking[] expectedResult)
		{
			var filter = (ModuleDateFilter)FilterStrip.ModuleFilters[filterName];

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches($"{filterDescription}, Has Date Entered", filter, expectedResult);
		}

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var jobOpen = "Job Open";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = jobOpen;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Datetime;

			template.Factory.Save();
			Assert("template should have been saved", template.IsInDatabase);

			Factory.Save();

			var collection = new DtbBookingFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[jobOpen]);
			AssertNotNull(collection[jobOpen + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		public void TestWorkflowFiltersPresent()
		{
			var relatedFilters = FilterStrip.ModuleFilters.Where(f => f.Category.Description.ToString() == "Workflow Milestones").ToArray();
			AssertEquals(4, relatedFilters.Length);
			relatedFilters.Single(f => f.Description.ToString() == "Milestone Date");
			relatedFilters.Single(f => f.Description.ToString() == "Milestone Completed");
			relatedFilters.Single(f => f.Description.ToString() == "Next Milestone");
			relatedFilters.Single(f => f.Description.ToString() == "Last Completed Milestone");
		}

		public void TestMilestoneDateFilter()
		{
			var year = ZDateTime.Now.Year;

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			var milestone1 = transport1.WorkflowItems.Milestones.AddNew();
			var milestone2 = transport2.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 7, 1)));

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBizO.ModuleFilters["Milestone Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(year, 1, 1);
			filter.Property2 = new ZDateTime(year, 1, 3);
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport1);
		}

		public void TestMilestoneCompletedFilter()
		{
			var year = ZDateTime.Now.Year;

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			var milestone1 = transport1.WorkflowItems.Milestones.AddNew();
			var milestone2 = transport2.WorkflowItems.Milestones.AddNew();

			var trigger = transport1.WorkflowItems.Triggers.AddNew();
			var exception = transport2.WorkflowItems.Exceptions.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(year, 1, 2));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			exception.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 2)));
			trigger.SetMilestoneActualDateForTest(ZDateTime.Empty);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO.ModuleFilters["Milestone Completed"];
			filter.Property = "Completed";
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport1);

			filter.Property = "Not Completed";
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport2);
		}

		public void TestMilestoneNextFilter()
		{
			var year = ZDateTime.Now.Year;

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			var milestone1 = transport1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "AID";
			var milestone2 = transport2.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "AID";
			milestone1.P9_Type = "MIL";
			milestone2.P9_Type = "MIL";

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 7, 1)));

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBizO.ModuleFilters["Next Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(year, 1, 1);
			filter.Property2 = new ZDateTime(year, 1, 3);
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport1);
		}

		public void TestMilestoneLastCompletedFilter()
		{
			var year = ZDateTime.Now.Year;

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			var milestone1 = transport1.WorkflowItems.Milestones.AddNew();
			var milestone2 = transport2.WorkflowItems.Milestones.AddNew();

			milestone1.P9_Type = "MIL";
			milestone2.P9_Type = "MIL";

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(year, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(year, 7, 1));

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBizO.ModuleFilters["Last Completed Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(year, 1, 1);
			filter.Property2 = new ZDateTime(year, 1, 3);
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport1);
		}

		public void TestJobInvoicingStatusFilter()
		{
			var transport1 = GetNewTransport();
			Asserter.AddToScope(transport1);

			var filterBizO = GetNewFilterStripBusinessObject();
			AssertNotNull(filterBizO.ModuleFilters["Invoice Status"]);
			var jobstatusFilter = (ModuleTextFilter)filterBizO.ModuleFilters["Invoice Status"];

			var job = new JobHeader.Loader(transport1).TryLoadOrCreate();
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			jobstatusFilter.Property = JobHeaderStatus.Working.Code;
			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobstatusFilter.IsActive = true;

			Asserter.AssertMatches("", jobstatusFilter, transport1);

			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			Asserter.AssertMatches("", jobstatusFilter);
		}

		public void TestInvoicedChargesFilter()
		{
			var transport1 = GetNewTransport();
			Asserter.AddToScope(transport1);

			var filterBizO = GetNewFilterStripBusinessObject();
			AssertNotNull(filterBizO.ModuleFilters["Invoiced / Charges / Billing"]);
			var invoicedChargesFilter = (ModuleFlagsFilter)filterBizO.ModuleFilters["Invoiced / Charges / Billing"];

			var job = new JobHeader.Loader(transport1).TryLoadOrCreate();
			AssertNotNull(job);

			Factory.Save();

			invoicedChargesFilter.IsActive = true;
			invoicedChargesFilter["No Charges"] = true;
			Asserter.AssertMatches("transport1 has no charges and should be find", invoicedChargesFilter, transport1);

			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_LocalSellAmt = 10m;
			Factory.Save();

			Asserter.AssertMatches("transport1 has one charge and should not be in the search results", invoicedChargesFilter);
		}

		public void TestBranch()
		{
			var branch1 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			var branch2 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, new[] { GlbBranch.CurrentBranch.PK, branch1.PK }));

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			transport1.KM_GB_Branch = branch1.PK;
			transport2.KM_GB_Branch = branch2.PK;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO.ModuleFilters["Branch"];
			filter.Property = branch1.PK;
			filter.IsActive = true;
			Asserter.AssertMatches("transport1 has branch1", filter, transport1);

			filter.Property = branch2.PK;
			Asserter.AssertMatches("transport2 has branch2", filter, transport2);
		}

		public void TestParentJobFilterWorksWithLargeAmountOfParameters()
		{
			var filter = (ModuleTextFilter)FilterStrip[FilterNameConstants.ParentJobNumber];

			var amountOfPropertiesToCreate = ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION + 5;
			var property = ZString.Empty;

			for (var i = 0; i < amountOfPropertiesToCreate; i++)
			{
				property += $"{i}, ";
			}

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = property;

			Asserter.AssertMatches("Should not throw an (invalid column name) error", filter, Array.Empty<DtbBooking>());
		}

		public void TestIsMasterBookingFilter()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Booking1.KM_IsMaster = true;

				Factory.Save();

				var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsMaster];
				filter.Property = IsMasterBookingStatuses.Codes.All;
				Asserter.AssertMatches("All", filter, Booking1, Booking2, Booking3, Booking4);

				filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsMaster];
				filter.Property = IsMasterBookingStatuses.Codes.IsMasterBooking;
				Asserter.AssertMatches("Master Booking", filter, Booking1);

				filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsMaster];
				filter.Property = IsMasterBookingStatuses.Codes.NotMasterBooking;
				Asserter.AssertMatches("NOT Master Booking", filter, Booking2, Booking3, Booking4);
			}
		}

		public void TestIsSubBookingFilter()
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Booking1.KM_IsMaster = true;
				Booking1.KM_KM_MasterBooking = ZGuid.Empty;

				Booking2.KM_IsMaster = false;
				Booking2.KM_KM_MasterBooking = ZGuid.Empty;
				Booking3.KM_IsMaster = false;
				Booking3.KM_KM_MasterBooking = ZGuid.Empty;

				Booking4.KM_IsMaster = false;
				Booking4.KM_KM_MasterBooking = Booking1.PK;

				Factory.Save();

				var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsSub];
				filter.Property = IsSubBookingStatuses.Codes.All;
				Asserter.AssertMatches("All", filter, Booking1, Booking2, Booking3, Booking4);

				filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsSub];
				filter.Property = IsSubBookingStatuses.Codes.IsSubBooking;
				Asserter.AssertMatches("Sub Booking", filter, Booking4);

				filter = (ModuleTextFilter)FilterStrip.ModuleFilters[FilterNameConstants.BookingIsSub];
				filter.Property = IsSubBookingStatuses.Codes.NotSubBooking;
				Asserter.AssertMatches("NOT Sub Booking", filter, Booking1, Booking2, Booking3);
			}
		}

		public void TestIsMasterIsSubFiltersWithMasterBookingsEnabled()
		{
			BookingFilterItemsTestCore(true);
		}

		public void TestIsMasterIsSubFiltersWithMasterBookingsDisabled()
		{
			BookingFilterItemsTestCore(false);
		}

		void BookingFilterItemsTestCore(bool isMasterBookingsEnabled)
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isMasterBookingsEnabled))
			{
				var isSubFilter = (ModuleTextFilter)FilterStrip["Is Sub"];
				var isMasterFilter = (ModuleTextFilter)FilterStrip["Is Master"];

				if (isMasterBookingsEnabled)
				{
					AssertCollectionContains(isSubFilter, FilterStrip);
					AssertCollectionContains(isMasterFilter, FilterStrip);
				}
				else
				{
					AssertCollectionNotContains(isSubFilter, FilterStrip);
					AssertCollectionNotContains(isMasterFilter, FilterStrip);
				}
			}
		}

		public void TestFilteringOfCO2eWithEnabledGreenhouseGasEmission_WithEnabledTransportBookingGHGCalculation()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				Booking1.SetCO2eStatus(CO2eStatusList.Codes.Current);
				Booking1.SetTotalCO2e(10m);

				Booking2.SetCO2eStatus(CO2eStatusList.Codes.Current);
				Booking2.SetTotalCO2e(20m);

				Booking3.SetCO2eStatus(CO2eStatusList.Codes.Current);
				Booking3.SetTotalCO2e(50m);

				Booking4.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
				Booking4.SetTotalCO2e(25m);

				Factory.Save();

				var filter = (CO2eStatusAndCO2eKgRangeNumberFilter)FilterStrip.ModuleFilters["CO2e"];
				filter.Property1 = 5.0;
				filter.Property2 = 45.0;

				Asserter.AssertMatches("Filter based on Total CO2e value failed", filter, Booking1, Booking2);

				var filter2 = (CO2eStatusAndCO2eKgRangeNumberFilter)FilterStrip.ModuleFilters["CO2e"];
				filter2.Property1 = 5.0;
				filter2.Property2 = 100.0;

				Asserter.AssertMatches("Filter based on Total CO2e value failed", filter2, Booking1, Booking2, Booking3);

				var filter3 = (CO2eStatusAndCO2eKgRangeNumberFilter)FilterStrip.ModuleFilters["CO2e"];
				filter3.Property1 = 5.0;
				filter3.Property2 = 100.0;
				filter3.CO2eStatus = CO2eStatusList.Codes.Rejected;

				Asserter.AssertMatches("Filter based on CO2e Status failed", filter3, Booking4);
			}
		}

		public void TestFilteringOfCO2eWithDisabledGreenhouseGasEmission()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				var filter = (CO2eStatusAndCO2eKgRangeNumberFilter)FilterStrip.ModuleFilters["CO2e"];
				AssertNull(filter);
			}
		}

		FilterStripAsserter<DtbBooking> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<DtbBooking>(Factory, b => b.KM_JobID)); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DtbBookingFilterBusinessObject();
		}

		protected override ModuleFilter SetupFilterForMatchesFilterTest(ModuleFilter moduleFilter)
		{
			var onlyAllowedComparisonOperatorForParentJobType = SQLComparisonOperator.Equal;
			moduleFilter = base.SetupFilterForMatchesFilterTest(moduleFilter);
			if (moduleFilter is ModuleFilterWithListAndComparisonOperators<ZString> moduleFilterWithListAndComparisonOperators && moduleFilterWithListAndComparisonOperators.Code.EqualsIgnoringCase("Parent Job Type"))
			{
				moduleFilterWithListAndComparisonOperators.SqlComparisonOperator = onlyAllowedComparisonOperatorForParentJobType;
			}
			return moduleFilter;
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		DtbBookingFilterBusinessObject FilterStrip
		{
			get { return filterStrip ?? (filterStrip = new DtbBookingFilterBusinessObject()); }
		}

		DtbBooking GetNewBookingAndAddToScope(ZString bookingID)
		{
			var booking = Helper.CreateBooking();
			booking.KM_JobID = bookingID;
			Asserter.AddToScope(booking);

			return booking;
		}

		DtbBooking Booking1
		{
			get { return booking1 ?? (booking1 = GetNewBookingAndAddToScope("TB1234")); }
		}

		DtbBooking Booking2
		{
			get { return booking2 ?? (booking2 = GetNewBookingAndAddToScope("TB2345")); }
		}

		DtbBooking Booking3
		{
			get { return booking3 ?? (booking3 = GetNewBookingAndAddToScope("TB3456")); }
		}

		DtbBooking Booking4
		{
			get { return booking4 ?? (booking4 = GetNewBookingAndAddToScope("TB4567")); }
		}

		DtbBooking Booking5
		{
			get { return booking5 ?? (booking5 = GetNewBookingAndAddToScope("Booking5")); }
		}

		DtbBooking Booking6
		{
			get { return booking6 ?? (booking6 = GetNewBookingAndAddToScope("Booking6")); }
		}

		DtbBooking Booking7
		{
			get { return booking7 ?? (booking7 = GetNewBookingAndAddToScope("Booking7")); }
		}

		DtbBooking Booking8
		{
			get { return booking8 ?? (booking8 = GetNewBookingAndAddToScope("Booking8")); }
		}

		DtbBooking Booking9
		{
			get { return booking9 ?? (booking9 = GetNewBookingAndAddToScope("Booking9")); }
		}

		DtbBooking Booking10
		{
			get { return booking10 ?? (booking10 = GetNewBookingAndAddToScope("Booking10")); }
		}

		DtbBooking Booking11
		{
			get { return booking11 ?? (booking11 = GetNewBookingAndAddToScope("Booking11")); }
		}

		DtbBooking Booking12
		{
			get { return booking12 ?? (booking12 = GetNewBookingAndAddToScope("Booking12")); }
		}

		protected override void SetUp()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;

			base.SetUp();

			// tests rely on the lazy initialisation having occured
			var booking1 = Booking1;
			var booking2 = Booking2;
			var booking3 = Booking3;
			var booking4 = Booking4;
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
		}

		DtbBooking GetNewTransport()
		{
			return Helper.CreateBooking();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		GlbBranch GetBranchInAnotherCompany(GlbCompany company)
		{
			ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			return Factory.LoadTop1<GlbBranch>(filter);
		}

		FilterStripAsserter<DtbBooking> asserter;
		TransportBookingTestHelper helper;
		DtbBookingFilterBusinessObject filterStrip;
		DtbBooking booking1;
		DtbBooking booking2;
		DtbBooking booking3;
		DtbBooking booking4;
		DtbBooking booking5;
		DtbBooking booking6;
		DtbBooking booking7;
		DtbBooking booking8;
		DtbBooking booking9;
		DtbBooking booking10;
		DtbBooking booking11;
		DtbBooking booking12;
	}

	class DtbBookingFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<DtbBooking>
	{
		protected override DtbBooking GetNewBusinessObjectForFilterCollection()
		{
			return Helper.CreateBooking();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.DtbBooking; }
		}

		protected override object GetCustomFormWithJobInvoicing()
		{
			return new TransportBookingForm(GetNewBusinessObjectForFilterCollection());
		}

		protected override bool IsRevenueFiltersAdded
		{
			get { return false; }
		}

		protected override void SetUp()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			base.SetUp();
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
