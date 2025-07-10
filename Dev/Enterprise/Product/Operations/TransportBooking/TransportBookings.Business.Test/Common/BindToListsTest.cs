using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared.Lists;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business.Test.Common
{
	public class BindToListsTest : TransportBindToListsTest
	{
		public void TestBookingTemplates()
		{
			var template = Factory.New<DtbBookingTmpl>();
			template.KT_IsActive = true;
			template.KT_Code = "AAAA";
			template.KT_Description = "AAAA Description";
			BindToLists.BookingTemplatesAdditionalQuery = null;
			AssertEquals(33, BindToLists.BookingTemplates.Count);
			AssertEquals("AAAA", BindToLists.BookingTemplates[0].KT_Code);

			BindToLists.BookingTemplatesAdditionalQuery = new ZQuery(DtbBookingTmplSchema.KT_Code, SQLComparisonOperator.Equal, "AAAA");
			AssertEquals(1, BindToLists.BookingTemplates.Count);
			AssertEquals("AAAA", BindToLists.BookingTemplates[0].KT_Code);

			template.KT_IsActive = false;
			AssertEquals(0, BindToLists.BookingTemplates.Count);
		}

		public void TestBookingConsolidatedStatusesCount()
		{
			AssertEquals(3, BindToLists.BookingConsolidatedStatuses.Count);
		}

		public void TestBookingConsolidatedStatuses()
		{
			AssertContainsExactElementsInAnyOrder(
				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair("All", "All"),
					new CodeDescriptionPair("UNC", "Unconsolidated"),
					new CodeDescriptionPair("CON", "Consolidated"),
				},

				BindToLists.BookingConsolidatedStatuses);
		}

		public void TestConfirmationTypes()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReference = dateAndReferences.AddNew();
			dateAndReference.Code = "CUS";
			dateAndReference.Description = (NoResString)"Custom";
			dateAndReference.AllowActualDate = true;
			dateAndReference.AllowEstimatedDate = false;
			dateAndReference.AllowRequiredFromDate = true;
			dateAndReference.AllowRequiredToDate = false;
			dateAndReference.AllowReference = true;
			dateAndReference.AllowReceivedBy = false;
			dateAndReference.IsSystemDefined = false;
			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			var expected = new CodeDescriptionPairList();
			expected.AddPair(ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp);
			expected.AddPair(ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery);
			expected.AddPair(ConfirmationTypes.Codes.ConNoteNo, ConfirmationTypes.Descriptions.ConnoteNo);
			expected.AddPair("CUS", "Custom");

			AssertContainsExactElementsInAnyOrder(expected, BindToLists.ConfirmationTypes);
		}

		public void TestGetConfirmationTypes()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReferenceCFS = dateAndReferences.AddNew();
			dateAndReferenceCFS.Code = "CUS";
			dateAndReferenceCFS.Description = (NoResString)"Custom";
			dateAndReferenceCFS.BookingDirection = Constants.CartageDirection.Import;
			dateAndReferenceCFS.InstructionType = InstructionTypes.Codes.Delivery;
			dateAndReferenceCFS.OrganisationType = "CFS";
			dateAndReferenceCFS.AllowActualDate = true;
			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			AssertEquals(true, BindToLists.GetConfirmationTypes(Constants.CartageDirection.Import, InstructionTypes.Codes.Delivery, "CFS").ContainsCode("CUS"));
			AssertEquals(false, BindToLists.GetConfirmationTypes(Constants.CartageDirection.Import, "", "CTO").ContainsCode("CUS"));
			AssertEquals(false, BindToLists.GetConfirmationTypes("", InstructionTypes.Codes.Delivery, "CTO").ContainsCode("CUS"));
			AssertEquals(false, BindToLists.GetConfirmationTypes("AAA", "BBB", "CTO").ContainsCode("CUS"));
			AssertEquals(false, BindToLists.GetConfirmationTypes("", "BBB", "CTO").ContainsCode("CUS"));
			AssertEquals(false, BindToLists.GetConfirmationTypes("AAA", "", "CTO").ContainsCode("CUS"));
			AssertEquals(false, BindToLists.GetConfirmationTypes("", "", "CTO").ContainsCode("CUS"));

			var dateAndReferenceANY = dateAndReferences.AddNew();
			dateAndReferenceANY.Code = "CUS";
			dateAndReferenceANY.Description = (NoResString)"Custom";
			dateAndReferenceANY.BookingDirection = DatesAndReference.Any;
			dateAndReferenceANY.InstructionType = DatesAndReference.Any;
			dateAndReferenceANY.OrganisationType = DatesAndReference.Any;
			dateAndReferenceANY.AllowActualDate = true;
			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			AssertEquals(true, BindToLists.GetConfirmationTypes(Constants.CartageDirection.Import, InstructionTypes.Codes.Delivery, "CFS").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("", InstructionTypes.Codes.Delivery, "CFS").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes(Constants.CartageDirection.Import, "", "CFS").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("AAA", "BBB", "CFS").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("", "BBB", "CFS").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("AAA", "", "CFS").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("", "", "CFS").ContainsCode("CUS"));

			AssertEquals(true, BindToLists.GetConfirmationTypes(Constants.CartageDirection.Import, InstructionTypes.Codes.Delivery, "CTO").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("AAA", InstructionTypes.Codes.Delivery, "CTO").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes(Constants.CartageDirection.Import, "BBB", "CTO").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("AAA", "BBB", "CTO").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("", "BBB", "CTO").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("AAA", "", "CTO").ContainsCode("CUS"));
			AssertEquals(true, BindToLists.GetConfirmationTypes("", "", "CTO").ContainsCode("CUS"));
		}

		public void TestGetConfirmationDescriptions()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReferenceCFS = dateAndReferences.AddNew();
			dateAndReferenceCFS.Code = "CUS";
			dateAndReferenceCFS.BookingDirection = Constants.CartageDirection.Origin;
			dateAndReferenceCFS.InstructionType = InstructionTypes.Codes.Delivery;
			dateAndReferenceCFS.Code = "CUS";
			dateAndReferenceCFS.Description = (NoResString)"Custom";
			dateAndReferenceCFS.OrganisationType = "CFS";
			dateAndReferenceCFS.AllowActualDate = true;

			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			AssertEquals(true, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, InstructionTypes.Codes.Delivery, "CFS").ContainsCode("Custom"));
			AssertEquals(false, BindToLists.GetConfirmationDescriptions("", InstructionTypes.Codes.Delivery, "CFS").ContainsCode("Custom"));
			AssertEquals(false, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, "", "CFS").ContainsCode("Custom"));
			AssertEquals(false, BindToLists.GetConfirmationDescriptions("AAA", InstructionTypes.Codes.Delivery, "CFS").ContainsCode("Custom"));
			AssertEquals(false, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, "BBB", "CFS").ContainsCode("Custom"));
			AssertEquals(false, BindToLists.GetConfirmationDescriptions("AAA", "BBB", "CFS").ContainsCode("Custom"));
			AssertEquals(false, BindToLists.GetConfirmationDescriptions("", "BBB", "CFS").ContainsCode("Custom"));
			AssertEquals(false, BindToLists.GetConfirmationDescriptions("AAA", "", "CFS").ContainsCode("Custom"));
			AssertEquals(false, BindToLists.GetConfirmationDescriptions("", "", "CFS").ContainsCode("Custom"));

			var dateAndReferenceALL = dateAndReferences.AddNew();
			dateAndReferenceALL.Code = "CUS";
			dateAndReferenceALL.Description = (NoResString)"Custom";
			dateAndReferenceALL.BookingDirection = DatesAndReference.Any;
			dateAndReferenceALL.InstructionType = DatesAndReference.Any;
			dateAndReferenceALL.OrganisationType = DatesAndReference.Any;
			dateAndReferenceALL.AllowActualDate = true;
			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			AssertEquals(true, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, InstructionTypes.Codes.Delivery, "CFS").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("", InstructionTypes.Codes.Delivery, "CFS").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, "", "CFS").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("AAA", InstructionTypes.Codes.Delivery, "CFS").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, "BBB", "CFS").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("AAA", "BBB", "CFS").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("", "BBB", "CFS").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("AAA", "", "CFS").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("", "", "CFS").ContainsCode("Custom"));

			AssertEquals(true, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, InstructionTypes.Codes.Delivery, "CTO").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("", InstructionTypes.Codes.Delivery, "CTO").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, "", "CTO").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("AAA", InstructionTypes.Codes.Delivery, "CTO").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions(Constants.CartageDirection.Origin, "BBB", "CTO").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("AAA", "BBB", "CTO").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("", "BBB", "CTO").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("AAA", "", "CTO").ContainsCode("Custom"));
			AssertEquals(true, BindToLists.GetConfirmationDescriptions("", "", "CTO").ContainsCode("Custom"));
		}

		public void TestDateAndReferenceRegistry()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReference = dateAndReferences.AddNew();
			dateAndReference.Code = "CUS";
			dateAndReference.Description = (NoResString)"Custom";
			dateAndReference.AllowActualDate = true;
			dateAndReference.AllowEstimatedDate = false;
			dateAndReference.AllowRequiredFromDate = true;
			dateAndReference.AllowRequiredToDate = false;
			dateAndReference.AllowReference = true;
			dateAndReference.AllowReceivedBy = false;
			dateAndReference.IsSystemDefined = false;

			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();

			confirmation.KK_ConfirmationType = "CUS";
			AssertEquals(false, confirmation.KK_ActualInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_EstimatedInfo.ReadOnly);
			AssertEquals(false, confirmation.KK_RequiredFromInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_RequiredToInfo.ReadOnly);
			AssertEquals(false, confirmation.KK_ReferenceNumInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_ReceivedByInfo.ReadOnly);

			confirmation.KK_ConfirmationType = "";
			AssertEquals(true, confirmation.KK_ActualInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_EstimatedInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_RequiredFromInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_RequiredToInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_ReferenceNumInfo.ReadOnly);
			AssertEquals(true, confirmation.KK_ReceivedByInfo.ReadOnly);
		}

		public void TestContainerTypes()
		{
			AssertEquals(OLookUpEditType.ContainerType, BindToLists.ContainerTypes.LookupEditType);
		}

		public void TestDirections()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair(Constants.CartageDirection.Import, Constants.CartageDirectionDescription.Import);
			expected.AddPair(Constants.CartageDirection.Export, Constants.CartageDirectionDescription.Export);
			expected.AddPair(Constants.CartageDirection.Origin, Constants.CartageDirectionDescription.Origin);
			expected.AddPair(Constants.CartageDirection.Destination, Constants.CartageDirectionDescription.Destination);
			expected.AddPair(Constants.CartageDirection.Local, Constants.CartageDirectionDescription.Local);

			AssertContainsExactElementsInAnyOrder(expected, BindToLists.Directions);
		}

		public void TestDirectionsChar()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair(Constants.CartageDirectionChar.Import, Constants.CartageDirection.Import);
			expected.AddPair(Constants.CartageDirectionChar.Export, Constants.CartageDirection.Export);
			expected.AddPair(Constants.CartageDirectionChar.Origin, Constants.CartageDirection.Origin);
			expected.AddPair(Constants.CartageDirectionChar.Destination, Constants.CartageDirection.Destination);
			expected.AddPair(Constants.CartageDirectionChar.Local, Constants.CartageDirection.Local);

			AssertContainsExactElementsInAnyOrder(expected, BindToLists.DirectionsChar);
		}

		public void TestBookingConsolidationJobDirections()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair(nameof(DtbBookingDirection.PIC), DtbBookingDirectionDescription.GetDescription(DtbBookingDirection.PIC));
			expected.AddPair(nameof(DtbBookingDirection.DLV), DtbBookingDirectionDescription.GetDescription(DtbBookingDirection.DLV));

			AssertContainsExactElementsInAnyOrder(expected, BindToLists.BookingConsolidationJobDirections);
		}

		public void TestDropModes_Containerized()
		{
			AssertContainsExactElementsInAnyOrder(new FCLEquipmentNeededList(), BindToLists.DropModes_Containerized);
		}

		public void TestDropModes_Loose()
		{
			AssertContainsExactElementsInAnyOrder(new LCLAIREquipmentNeededList(), BindToLists.DropModes_Loose);
		}

		public void TestIsHazardousStatuses()
		{
			var statusesCache = BindToLists.IsHazardousStatuses;
			AssertContainsExactElementsInAnyOrder(new IsHazardousStatuses().List, statusesCache);

			AssertEquals("Should be cached.", statusesCache, BindToLists.IsHazardousStatuses);
		}

		public void TestOrganisationTypes()
		{
			AssertContainsExactElementsInAnyOrder(OrganisationTypesList.Instance, BindToLists.OrganisationTypes);
		}

		public void TestParentJobTypes()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair("STB", "Standalone Booking");
			expected.AddPair("CUS", "Customs Declaration");
			expected.AddPair("SHP", "Forwarding Shipment");
			expected.AddPair("ASH", "Liner and Agency");
			expected.AddPair("WHO", "Warehouse Order");
			expected.AddPair("WHR", "Warehouse Receipt");
			expected.AddPair("CON", "Forwarding Consolidation");
			expected.AddPair("TWD", "Transit Warehouse Dispatch");
			expected.AddPair("HVH", "HVLV Booking Header");
			expected.AddPair("HVC", "HVLV Consignment");
			AssertContainsExactElementsInAnyOrder(expected, BindToLists.ParentJobTypes);
		}

		public void TestPackageTypes()
		{
			var packages = new RefPackTypeCollection(Factory, excludeCNT: false);
			AssertContainsExactElementsInAnyOrder(packages, BindToLists.PackageTypes);
		}

		public void TestDefaultPackageTypes()
		{
			AssertContainsExactElementsInAnyOrder(new PackageCategories(), BindToLists.DefaultPackageTypes);
		}

		public void TestRatingFreightModes()
		{
			AssertContainsExactElementsInAnyOrder(

				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair("CNT", "Rate Containers Only"),
					new CodeDescriptionPair("LSE", "Rate Loose Only"),
					new CodeDescriptionPair("BTH", "Rate Both Containers and Loose"),
				},

				BindToLists.RatingFreightModes.List);
		}

		public void TestRequiresRefrigerationStatuses()
		{
			var statusesCache = BindToLists.RequiresRefrigerationStatuses;
			AssertContainsExactElementsInAnyOrder(new RequiresRefrigerationStatuses().List, statusesCache);

			AssertEquals("Should be cached.", statusesCache, BindToLists.RequiresRefrigerationStatuses);
		}

		public void TestBookingShowStandaloneValues()
		{
			AssertContainsExactElementsInAnyOrder(

				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair("INC", "Show Standalone Bookings Only"),
					new CodeDescriptionPair("EXC", "Exclude Standalone Bookings"),
					new CodeDescriptionPair("ALL", "Show All")
				},

				BindToLists.BookingShowStandaloneValues.List);
		}

		public void TestIsMasterBookingStatuses()
		{
			var statusesCache = BindToLists.IsMasterBookingStatuses;
			AssertContainsExactElementsInAnyOrder(new IsMasterBookingStatuses().List, statusesCache);

			AssertEquals("Should be cached.", statusesCache, BindToLists.IsMasterBookingStatuses);
		}

		public void TestIsSubBookingStatuses()
		{
			var statusesCache = BindToLists.IsSubBookingStatuses;
			AssertContainsExactElementsInAnyOrder(new IsSubBookingStatuses().List, statusesCache);

			AssertEquals("Should be cached.", statusesCache, BindToLists.IsSubBookingStatuses);
		}

		public void TestIsSubBookingStatusesValuesAreCorrect()
		{
			AssertContainsExactElementsInAnyOrder(

				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair("ALL", "Show ALL Bookings"),
					new CodeDescriptionPair("SUB", "Show Sub Bookings"),
					new CodeDescriptionPair("NOS", "Show NOT Sub Bookings")
				},

				BindToLists.IsSubBookingStatuses);
		}

		public void TestIsMasterBookingStatusesValuesAreCorrect()
		{
			AssertContainsExactElementsInAnyOrder(

				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair("ALL", "Show ALL Bookings"),
					new CodeDescriptionPair("MST", "Show Master Bookings"),
					new CodeDescriptionPair("NOT", "Show NOT Master Bookings")
				},

				BindToLists.IsMasterBookingStatuses);
		}

		public void TestBookingTransportModes()
		{
			var statusesCache = BindToLists.BookingTransportModes.List;
			AssertContainsExactElementsInAnyOrder(new BookingTransportModes().List, statusesCache);

			AssertEquals("Should be cached.", statusesCache, BindToLists.BookingTransportModes.List);
		}

		public void TestBookingTransportModesValuesAreCorrect()
		{
			AssertContainsExactElementsInAnyOrder(

				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair("ROA", "Road Transport"),
					new CodeDescriptionPair("RAI", "Rail Transport"),
					new CodeDescriptionPair("IWT", "Inland Waterways"),
				},

				BindToLists.BookingTransportModes.List);
		}

		protected new BindToLists BindToLists
		{
			get { return (BindToLists)base.BindToLists; }
		}

		protected override TransportBindToLists GetNewBindToLists()
		{
			return new BindToLists(Factory);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
