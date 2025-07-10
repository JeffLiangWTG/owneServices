using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(JobTemplateDefaultCollection))]
	public class JobTemplateDefaultCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JobTemplateDefaultCollection>
	{
		#region Defaults

		public void TestGetDefault()
		{
			var defaultValue = JobTemplateDefaultCollection.GetDefault();
			AssertEquals(13, defaultValue.Count);

			// origin
			AssertJobTemplateDefault(defaultValue[0], "ALL", Constants.CartageDirection.Origin, "ALL", Constants.ContainerModes.Containerised, OrganisationTypesList.Codes.CFS, "EFPL", "", true);
			AssertJobTemplateDefault(defaultValue[1], "ALL", Constants.CartageDirection.Origin, "ALL", Constants.ContainerModes.Containerised, "", "EFPR", "", true);
			AssertJobTemplateDefault(defaultValue[2], "ALL", Constants.CartageDirection.Origin, "ALL", Constants.ContainerModes.Loose, "", "EFPU", "", true);
			AssertJobTemplateDefault(defaultValue[3], ParentTypes.Codes.ForwardingConsol, Constants.CartageDirection.Origin, "ALL", Constants.ContainerModes.Containerised, "", "EECS", "", true);

			// destination
			AssertJobTemplateDefault(defaultValue[4], "ALL", Constants.CartageDirection.Destination, "ALL", Constants.ContainerModes.Containerised, OrganisationTypesList.Codes.CFS, "IFUD", "", true);
			AssertJobTemplateDefault(defaultValue[5], "ALL", Constants.CartageDirection.Destination, "ALL", Constants.ContainerModes.Containerised, "", "IFCD", "", true);
			AssertJobTemplateDefault(defaultValue[6], "ALL", Constants.CartageDirection.Destination, "ALL", Constants.ContainerModes.Loose, "", "ILDV", "", true);
			AssertJobTemplateDefault(defaultValue[7], ParentTypes.Codes.ForwardingConsol, Constants.CartageDirection.Destination, "ALL", Constants.ContainerModes.Containerised, "", "IECS", "", true);

			// warehouse order
			AssertJobTemplateDefault(defaultValue[8], ParentTypes.Codes.WarehouseOrder, "ALL", "ALL", "ALL", "", "DLCW", "", true);

			// warehouse receive
			AssertJobTemplateDefault(defaultValue[9], ParentTypes.Codes.WarehouseReceive, "ALL", "ALL", Constants.ContainerModes.Containerised, "", "PFCW", "", true);
			AssertJobTemplateDefault(defaultValue[10], ParentTypes.Codes.WarehouseReceive, "ALL", "ALL", Constants.ContainerModes.Loose, "", "PLCW", "", true);

			// transit dispatch
			AssertJobTemplateDefault(defaultValue[11], ParentTypes.Codes.TransitDispatch, "ALL", "ALL", "ALL", "", "DLTW", "", true);

			// fallback
			AssertJobTemplateDefault(defaultValue[12], "ALL", "ALL", "ALL", "ALL", "", "", "", true);
		}

		void AssertJobTemplateDefault(JobTemplateDefault jobTemplateDefault,
			ZString parent,
			ZString direction,
			ZString transportMode,
			ZString containerMode,
			ZString hasOrganisation,
			ZString bookingTemplate,
			ZString notes,
			ZBool isSystemDefined)
		{
			AssertEquals(parent, jobTemplateDefault.Parent);
			AssertEquals(direction, jobTemplateDefault.Direction);
			AssertEquals(transportMode, jobTemplateDefault.TransportMode);
			AssertEquals(containerMode, jobTemplateDefault.ContainerMode);
			AssertEquals(hasOrganisation, jobTemplateDefault.HasOrganisation);
			AssertEquals(bookingTemplate, jobTemplateDefault.BookingTemplate);
			AssertEquals(notes, jobTemplateDefault.Notes);
			AssertEquals(isSystemDefined, jobTemplateDefault.IsSystemDefined);
		}

		#endregion

		#region AllowNew

		public void TestAllowNew()
		{
			AssertEquals("Must NOT allow new rows", true, Collection.AllowNew);
		}

		#endregion

		#region GetBookingTemplate

		public void TestGetBookingTemplate()
		{
			var defaultValue = JobTemplateDefaultCollection.GetDefault();

			// exact match
			AssertEquals("EFPL", defaultValue.GetBookingTemplate("", Constants.CartageDirection.Origin, "", Constants.ContainerModes.Containerised, true));
			AssertEquals("EFPR", defaultValue.GetBookingTemplate("", Constants.CartageDirection.Origin, "", Constants.ContainerModes.Containerised, false));
			AssertEquals("EFPU", defaultValue.GetBookingTemplate("", Constants.CartageDirection.Origin, "", Constants.ContainerModes.Loose, false));
			AssertEquals("EECS", defaultValue.GetBookingTemplate(ParentTypes.Codes.ForwardingConsol, Constants.CartageDirection.Origin, "", Constants.ContainerModes.Containerised, false));
			AssertEquals("IFUD", defaultValue.GetBookingTemplate("", Constants.CartageDirection.Destination, "", Constants.ContainerModes.Containerised, true));
			AssertEquals("IFCD", defaultValue.GetBookingTemplate("", Constants.CartageDirection.Destination, "", Constants.ContainerModes.Containerised, false));
			AssertEquals("ILDV", defaultValue.GetBookingTemplate("", Constants.CartageDirection.Destination, "", Constants.ContainerModes.Loose, false));
			AssertEquals("DLCW", defaultValue.GetBookingTemplate(ParentTypes.Codes.WarehouseOrder, Constants.CartageDirection.Destination, "", Constants.ContainerModes.Loose, false));
			AssertEquals("DLTW", defaultValue.GetBookingTemplate(ParentTypes.Codes.TransitDispatch, Constants.CartageDirection.Destination, "", Constants.ContainerModes.Loose, false));
			AssertEquals("IECS", defaultValue.GetBookingTemplate(ParentTypes.Codes.ForwardingConsol, Constants.CartageDirection.Destination, "", Constants.ContainerModes.Containerised, false));
			AssertEquals("", defaultValue.GetBookingTemplate("", "", "", "", false));

			// loose match
			AssertEquals("EFPL", defaultValue.GetBookingTemplate("SHP", Constants.CartageDirection.Origin, "ROA", Constants.ContainerModes.Containerised, true));
			AssertEquals("EFPR", defaultValue.GetBookingTemplate("SHP", Constants.CartageDirection.Origin, "ROA", Constants.ContainerModes.Containerised, false));
			AssertEquals("EFPU", defaultValue.GetBookingTemplate("SHP", Constants.CartageDirection.Origin, "ROA", Constants.ContainerModes.Loose, false));
			AssertEquals("EECS", defaultValue.GetBookingTemplate(ParentTypes.Codes.ForwardingConsol, Constants.CartageDirection.Origin, "ROA", Constants.ContainerModes.Containerised, false));
			AssertEquals("IFUD", defaultValue.GetBookingTemplate("SHP", Constants.CartageDirection.Destination, "ROA", Constants.ContainerModes.Containerised, true));
			AssertEquals("IFCD", defaultValue.GetBookingTemplate("SHP", Constants.CartageDirection.Destination, "ROA", Constants.ContainerModes.Containerised, false));
			AssertEquals("ILDV", defaultValue.GetBookingTemplate("SHP", Constants.CartageDirection.Destination, "ROA", Constants.ContainerModes.Loose, false));
			AssertEquals("IECS", defaultValue.GetBookingTemplate(ParentTypes.Codes.ForwardingConsol, Constants.CartageDirection.Destination, "ROA", Constants.ContainerModes.Containerised, false));
			AssertEquals("DLCW", defaultValue.GetBookingTemplate(ParentTypes.Codes.WarehouseOrder, "", "", "", false));
			AssertEquals("DLTW", defaultValue.GetBookingTemplate(ParentTypes.Codes.TransitDispatch, "", "", "", false));
		}

		#endregion

		#region Overrides

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override JobTemplateDefaultCollection GetCollectionToTest()
		{
			return new JobTemplateDefaultCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JobTemplateDefault();
		}

		#endregion
	}
}
