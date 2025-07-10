using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(JobTemplateDefault))]
	public class JobTemplateDefaultTest : RegistryBusinessObjectTemplateTestCase<JobTemplateDefault>
	{
		#region Properties and Validation

		public void TestParent()
		{
			AssertEquals("", BizObj.Parent);

			BizObj.ValidateParent();
			AssertEquals(true, BizObj.ParentInfo.HasErrors());

			BizObj.Parent = "XXX";
			AssertEquals(true, BizObj.ParentInfo.HasErrors());

			BizObj.Parent = Shared.ParentTypes.Codes.ForwardingShipment;
			AssertEquals(Shared.ParentTypes.Codes.ForwardingShipment, BizObj.Parent);
			AssertEquals(false, BizObj.ParentInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals(true, BizObj.ParentInfo.ReadOnly);
		}

		public void TestDirection()
		{
			AssertEquals("", BizObj.Direction);

			BizObj.ValidateDirection();
			AssertEquals(true, BizObj.DirectionInfo.HasErrors());

			BizObj.Direction = "XXX";
			AssertEquals(true, BizObj.DirectionInfo.HasErrors());

			BizObj.Direction = Constants.CartageDirection.Import;
			AssertEquals(Constants.CartageDirection.Import, BizObj.Direction);
			AssertEquals(false, BizObj.DirectionInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals(true, BizObj.DirectionInfo.ReadOnly);
		}

		public void TestTransportMode()
		{
			AssertEquals("", BizObj.TransportMode);

			BizObj.ValidateTransportMode();
			AssertEquals(true, BizObj.TransportModeInfo.HasErrors());

			BizObj.TransportMode = "XXX";
			AssertEquals(true, BizObj.TransportModeInfo.HasErrors());

			BizObj.TransportMode = Constants.TransportModes.Sea;
			AssertEquals(Constants.TransportModes.Sea, BizObj.TransportMode);
			AssertEquals(false, BizObj.TransportModeInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals(true, BizObj.TransportModeInfo.ReadOnly);
		}

		public void TestContainerMode()
		{
			AssertEquals("", BizObj.ContainerMode);

			BizObj.ValidateContainerMode();
			AssertEquals(true, BizObj.ContainerModeInfo.HasErrors());

			BizObj.ContainerMode = "XXX";
			AssertEquals(true, BizObj.ContainerModeInfo.HasErrors());

			BizObj.ContainerMode = Constants.CartageContainerMode.Loose;
			AssertEquals(Constants.CartageContainerMode.Loose, BizObj.ContainerMode);
			AssertEquals(false, BizObj.ContainerModeInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals(true, BizObj.ContainerModeInfo.ReadOnly);
		}

		public void TestHasOrganisation()
		{
			AssertEquals("", BizObj.HasOrganisation);

			BizObj.ValidateHasOrganisation();
			AssertEquals(false, BizObj.HasOrganisationInfo.HasErrors());

			BizObj.HasOrganisation = "XXX";
			AssertEquals(true, BizObj.HasOrganisationInfo.HasErrors());

			BizObj.HasOrganisation = OrganisationTypesList.Codes.CFS;
			AssertEquals(OrganisationTypesList.Codes.CFS, BizObj.HasOrganisation);
			AssertEquals(false, BizObj.HasOrganisationInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals(true, BizObj.HasOrganisationInfo.ReadOnly);
		}

		public void TestBookingTemplate()
		{
			AssertEquals("", BizObj.BookingTemplate);

			BizObj.ValidateBookingTemplate();
			AssertEquals(false, BizObj.BookingTemplateInfo.HasErrors());

			BizObj.BookingTemplate = "XXX";
			AssertEquals(true, BizObj.BookingTemplateInfo.HasErrors());

			BizObj.BookingTemplate = BizObj.BookingTemplates[0].Code;
			AssertEquals(BizObj.BookingTemplates[0].Code, BizObj.BookingTemplate);
			AssertEquals(false, BizObj.BookingTemplateInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals(false, BizObj.BookingTemplateInfo.ReadOnly);
		}

		public void TestNotes()
		{
			BizObj.Notes = "This is a Note";
			AssertEquals("This is a Note", BizObj.Notes);
			AssertEquals(false, BizObj.NotesInfo.ReadOnly);

			BizObj.IsSystemDefined = true;
			AssertEquals(true, BizObj.NotesInfo.ReadOnly);
		}

		public void TestIsSystemDefined()
		{
			BizObj.IsSystemDefined = true;
			AssertEquals(true, BizObj.IsSystemDefined);

			BizObj.IsSystemDefined = false;
			AssertEquals(false, BizObj.IsSystemDefined);
		}

		#endregion

		#region Lists

		#region TestParentTypes

		public void TestParentTypes()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair("ALL", "Default Fallback Template for All types of Transport Bookings.");
			expected.AddPair("AGB", "Default Template on 'Liner & Agency Booking' Transport Bookings Only.");
			expected.AddPair("AGS", "Default Template on 'Liner & Agency Bill Of Lading' Transport Bookings Only.");
			expected.AddPair("BRK", "Default Template on 'Declaration Job' Transport Bookings Only.");
			expected.AddPair("QSH", "Default Template on 'Quick Booking' Transport Bookings Only.");
			expected.AddPair("SHP", "Default Template on 'Forwarding Shipment' Transport Bookings Only.");
			expected.AddPair("WOU", "Default Template on 'Warehouse Release' Transport Bookings Only.");
			expected.AddPair("WIN", "Default Template on 'Warehouse Receive' Transport Bookings Only.");
			expected.AddPair("FCN", "Default Template on 'Consol' Transport Bookings Only.");
			expected.AddPair("TDC", "Default Template on 'Transit Dispatch' Transport Bookings Only.");

			AssertContainsExactElementsInAnyOrder(expected, BizObj.ParentTypes);
		}

		#endregion

		#region TestDirections

		public void TestDirections()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair("ALL", "Default Fallback Template for All Directions.");

			foreach (CodeDescriptionPair direction in new Directions().List)
			{
				expected.AddPair(direction.Code, string.Format("Default Template on Parent Jobs that have a Direction of '{0}'.", direction.Description));
			}

			AssertContainsExactElementsInAnyOrder(expected, BizObj.Directions);
		}

		#endregion

		#region TestTransportModes

		public void TestTransportModes()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair("ALL", "Default Fallback Template for All Transport Modes.");
			expected.AddPair(Constants.TransportModes.Air, string.Format("Default Template on Parent Jobs that have a Transport Mode '{0}'.", Constants.TransportModeDescriptions.Air));
			expected.AddPair(Constants.TransportModes.Sea, string.Format("Default Template on Parent Jobs that have a Transport Mode '{0}'.", Constants.TransportModeDescriptions.Sea));
			expected.AddPair(Constants.TransportModes.Road, string.Format("Default Template on Parent Jobs that have a Transport Mode '{0}'.", Constants.TransportModeDescriptions.Road));
			expected.AddPair(Constants.TransportModes.Rail, string.Format("Default Template on Parent Jobs that have a Transport Mode '{0}'.", Constants.TransportModeDescriptions.Rail));

			AssertContainsExactElementsInAnyOrder(expected, BizObj.TransportModes);
		}

		#endregion

		#region TestContainerModes

		public void TestContainerModes()
		{
			var expected = new CodeDescriptionPairList();
			expected.AddPair("ALL", "Default Fallback Template for All Container Modes.");
			expected.AddPair(Constants.CartageContainerMode.Loose, string.Format("Default Template on Parent Jobs that have a Container Mode '{0}'.", Constants.CartageContainerModeDescription.Loose));
			expected.AddPair(Constants.CartageContainerMode.Containerized, string.Format("Default Template on Parent Jobs that have a Container Mode '{0}'.", Constants.CartageContainerModeDescription.Containerized));

			AssertContainsExactElementsInAnyOrder(expected, BizObj.ContainerModes);
		}

		#endregion

		#region TestOrganisationTypes

		public void TestOrganisationTypes()
		{
			AssertContainsExactElementsInAnyOrder(OrganisationTypesList.Instance, BizObj.OrganisationTypes);
		}

		#endregion

		#region TestBookingTemplates

		public void TestBookingTemplates()
		{
			var tmplsCount = BizObj.BookingTemplates.Count;

			var dtbBookingTmplPK = new DtbBookingTmpl()
			{
				KT_IsSystem = true,
				KT_Code = "ABCD",
				KT_Description = "Test Description",
				KT_Direction = "ORG",
				KT_RatingFreightMode = "LSE",
				KT_IsActive = true
			}.InsertAndReturnObject(TestConnection).PK;
			AssertEquals("Active DtbBookingTmpl added therefore count should be incremented by 1", tmplsCount + 1, BizObj.BookingTemplates.Count);

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.DtbBookingTmpl SET KT_IsActive = 0 WHERE KT_PK = '{dtbBookingTmplPK}';");
			AssertEquals("Newly added DtbBookingTmpl made inactive therefore count should be unchanged", tmplsCount, BizObj.BookingTemplates.Count);
		}

		#endregion

		#endregion

		#region Implementation

		protected override JobTemplateDefault GetBusinessObjectToClone()
		{
			return new JobTemplateDefault();
		}

		protected override JobTemplateDefault GetBusinessObjectToSerialise()
		{
			return new JobTemplateDefault();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new JobTemplateDefault BizObj
		{
			get { return base.BizObj; }
		}

		#endregion
	}
}
