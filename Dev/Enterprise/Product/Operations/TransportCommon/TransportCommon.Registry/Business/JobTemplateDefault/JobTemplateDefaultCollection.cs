using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class JobTemplateDefaultCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Overrides

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobTemplateDefaultCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobTemplateDefault();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		#endregion

		#region Index

		public new JobTemplateDefault this[int i]
		{
			get { return (JobTemplateDefault)Elements[i]; }
		}

		public new JobTemplateDefault AddNew()
		{
			return (JobTemplateDefault)base.AddNew();
		}

		#endregion

		#region GetBookingTemplate

		public ZString GetBookingTemplate(
			ZString parent,
			ZString direction,
			ZString transportMode,
			ZString containerMode,
			ZBool hasCFS)
		{
			var highestScore = 0;
			var result = ZString.Empty;

			foreach (JobTemplateDefault jobTemplateDefault in this.Cast<JobTemplateDefault>())
			{
				// exact matches
				var isParent = jobTemplateDefault.Parent == parent;
				var isDirection = jobTemplateDefault.Direction == direction;
				var isTransportMode = jobTemplateDefault.TransportMode == transportMode;
				var isContainerMode = jobTemplateDefault.ContainerMode == containerMode;
				var isCFS = jobTemplateDefault.HasOrganisation == "CFS" && hasCFS;

				if ((jobTemplateDefault.Parent == "ALL" || isParent) &&
					(jobTemplateDefault.Direction == "ALL" || isDirection) &&
					(jobTemplateDefault.TransportMode == "ALL" || isTransportMode) &&
					(jobTemplateDefault.ContainerMode == "ALL" || isContainerMode) &&
					(jobTemplateDefault.HasOrganisation.IsEmpty || isCFS))
				{
					var score = isParent ? 100 : 1;
					score += isDirection ? 10 : 1;
					score += isTransportMode ? 10 : 1;
					score += isContainerMode ? 10 : 1;
					score += isCFS ? 10 : 1;

					if (score > highestScore)
					{
						highestScore = score;
						result = jobTemplateDefault.BookingTemplate;
					}
				}
			}

			return result;
		}

		#endregion

		#region Defaults

		public static JobTemplateDefaultCollection GetDefault()
		{
			var result = new JobTemplateDefaultCollection();

			// origin
			SetupDefaultFields(result.AddNew(), "ALL", Constants.CartageDirection.Origin, "ALL", Constants.ContainerModes.Containerised, OrganisationTypesList.Codes.CFS, "EFPL", "", true);
			SetupDefaultFields(result.AddNew(), "ALL", Constants.CartageDirection.Origin, "ALL", Constants.ContainerModes.Containerised, "", "EFPR", "", true);
			SetupDefaultFields(result.AddNew(), "ALL", Constants.CartageDirection.Origin, "ALL", Constants.ContainerModes.Loose, "", "EFPU", "", true);
			SetupDefaultFields(result.AddNew(), ParentTypes.Codes.ForwardingConsol, Constants.CartageDirection.Origin, "ALL", Constants.ContainerModes.Containerised, "", "EECS", "", true);

			// destination
			SetupDefaultFields(result.AddNew(), "ALL", Constants.CartageDirection.Destination, "ALL", Constants.ContainerModes.Containerised, OrganisationTypesList.Codes.CFS, "IFUD", "", true);
			SetupDefaultFields(result.AddNew(), "ALL", Constants.CartageDirection.Destination, "ALL", Constants.ContainerModes.Containerised, "", "IFCD", "", true);
			SetupDefaultFields(result.AddNew(), "ALL", Constants.CartageDirection.Destination, "ALL", Constants.ContainerModes.Loose, "", "ILDV", "", true);
			SetupDefaultFields(result.AddNew(), ParentTypes.Codes.ForwardingConsol, Constants.CartageDirection.Destination, "ALL", Constants.ContainerModes.Containerised, "", "IECS", "", true);

			// warehouse order
			SetupDefaultFields(result.AddNew(), ParentTypes.Codes.WarehouseOrder, "ALL", "ALL", "ALL", "", "DLCW", "", true);

			// warehouse receive
			SetupDefaultFields(result.AddNew(), ParentTypes.Codes.WarehouseReceive, "ALL", "ALL", Constants.ContainerModes.Containerised, "", "PFCW", "", true);
			SetupDefaultFields(result.AddNew(), ParentTypes.Codes.WarehouseReceive, "ALL", "ALL", Constants.ContainerModes.Loose, "", "PLCW", "", true);

			// transit dispatch
			SetupDefaultFields(result.AddNew(), ParentTypes.Codes.TransitDispatch, "ALL", "ALL", "ALL", "", "DLTW", "", true);

			// fallback
			SetupDefaultFields(result.AddNew(), "ALL", "ALL", "ALL", "ALL", "", "", "", true);

			return result;
		}

		static void SetupDefaultFields(JobTemplateDefault jobTemplateDefault,
			ZString parent,
			ZString direction,
			ZString transportMode,
			ZString containerMode,
			ZString hasOrganisation,
			ZString bookingTemplate,
			ZString notes,
			ZBool isSystemDefined)
		{
			using (jobTemplateDefault.GetValidationSuspender())
			{
				jobTemplateDefault.Parent = parent;
				jobTemplateDefault.Direction = direction;
				jobTemplateDefault.TransportMode = transportMode;
				jobTemplateDefault.ContainerMode = containerMode;
				jobTemplateDefault.HasOrganisation = hasOrganisation;
				jobTemplateDefault.BookingTemplate = bookingTemplate;
				jobTemplateDefault.Notes = notes;
				jobTemplateDefault.IsSystemDefined = isSystemDefined;
			}
		}

		#endregion
	}
}
