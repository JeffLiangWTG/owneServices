using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class JobTemplateDefault : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Parent = "Parent";
			public const string Direction = "Direction";
			public const string TransportMode = "TransportMode";
			public const string ContainerMode = "ContainerMode";
			public const string HasOrganisation = "HasOrganisation";
			public const string BookingTemplate = "BookingTemplate";
			public const string Notes = "Notes";

			public const string IsSystemDefined = "IsSystemDefined";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new JobTemplateDefault();

			using (clone.GetValidationSuspender())
			{
				clone.Parent = Parent;
				clone.Direction = Direction;
				clone.TransportMode = TransportMode;
				clone.ContainerMode = ContainerMode;
				clone.HasOrganisation = HasOrganisation;
				clone.BookingTemplate = BookingTemplate;
				clone.Notes = Notes;
				clone.IsSystemDefined = IsSystemDefined;
			}

			return clone;
		}

		#endregion

		#region Properties

		#region Parent

		[List("ParentTypes")]
		[ReadOnlyMember(nameof(IsSystemDefined))]
		[MaxLength(3)]
		public ZString Parent
		{
			get { return parent; }
			set
			{
				SetNonPersistentPropertyValue(ParentInfo, ref parent, value);
				if (!IsValidationSuspended)
				{
					ValidateParent();
				}
			}
		}

		public ZPropertyInfo ParentInfo
		{
			get { return GetZPropertyInfo(Schema.Parent); }
		}

		ZString parent;

		#endregion

		#region Direction

		[List("Directions")]
		[ReadOnlyMember(nameof(IsSystemDefined))]
		[MaxLength(3)]
		public ZString Direction
		{
			get { return direction; }
			set
			{
				SetNonPersistentPropertyValue(DirectionInfo, ref direction, value);
				if (!IsValidationSuspended)
				{
					ValidateDirection();
				}
			}
		}

		public ZPropertyInfo DirectionInfo
		{
			get { return GetZPropertyInfo(Schema.Direction); }
		}

		ZString direction;

		#endregion

		#region TransportMode

		[List("TransportModes")]
		[ReadOnlyMember(nameof(IsSystemDefined))]
		[MaxLength(3)]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);
				if (!IsValidationSuspended)
				{
					ValidateTransportMode();
				}
			}
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		ZString transportMode;

		#endregion

		#region ContainerMode

		[List("ContainerModes")]
		[ReadOnlyMember(nameof(IsSystemDefined))]
		[MaxLength(3)]
		public ZString ContainerMode
		{
			get { return containerMode; }
			set
			{
				SetNonPersistentPropertyValue(ContainerModeInfo, ref containerMode, value);
				if (!IsValidationSuspended)
				{
					ValidateContainerMode();
				}
			}
		}

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerMode); }
		}

		ZString containerMode;

		#endregion

		#region HasOrganization

		[List("OrganisationTypes")]
		[ReadOnlyMember(nameof(IsSystemDefined))]
		[MaxLength(3)]
		public ZString HasOrganisation
		{
			get { return hasOrganisation; }
			set
			{
				SetNonPersistentPropertyValue(HasOrganisationInfo, ref hasOrganisation, value);
				if (!IsValidationSuspended)
				{
					ValidateHasOrganisation();
				}
			}
		}

		public ZPropertyInfo HasOrganisationInfo
		{
			get { return GetZPropertyInfo(Schema.HasOrganisation); }
		}

		ZString hasOrganisation;

		#endregion

		#region BookingTemplate

		[List("BookingTemplates")]
		[MaxLength(4)]
		public ZString BookingTemplate
		{
			get { return bookingTemplate; }
			set
			{
				SetNonPersistentPropertyValue(BookingTemplateInfo, ref bookingTemplate, value);
				if (!IsValidationSuspended)
				{
					ValidateBookingTemplate();
				}
			}
		}

		public ZPropertyInfo BookingTemplateInfo
		{
			get { return GetZPropertyInfo(Schema.BookingTemplate); }
		}

		ZString bookingTemplate;

		#endregion

		#region Notes

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[MaxLength(80)]
		public ZString Notes
		{
			get { return notes; }
			set { SetNonPersistentPropertyValue(NotesInfo, ref notes, value); }
		}

		public ZPropertyInfo NotesInfo
		{
			get { return GetZPropertyInfo(Schema.Notes); }
		}

		ZString notes;

		#endregion

		#region IsSystemDefined

		public ZBool IsSystemDefined
		{
			get { return isSystemDefined; }
			set { isSystemDefined = value; }
		}

		ZBool isSystemDefined;

		#endregion

		#endregion

		#region Validation

		#region ValidateParent

		public void ValidateParent()
		{
			ParentInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ParentInfo);
			ListValidation.ErrorIfInvalidCode(ParentInfo);
		}

		#endregion

		#region ValidateDirection

		public void ValidateDirection()
		{
			DirectionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DirectionInfo);
			ListValidation.ErrorIfInvalidCode(DirectionInfo);
		}

		#endregion

		#region ValidateTransportMode

		public void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(TransportModeInfo);
		}

		#endregion

		#region ValidateContainerMode

		public void ValidateContainerMode()
		{
			ContainerModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ContainerModeInfo);
			ListValidation.ErrorIfInvalidCode(ContainerModeInfo);
		}

		#endregion

		#region ValidateHasOrganisation

		public void ValidateHasOrganisation()
		{
			HasOrganisationInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(HasOrganisationInfo);
		}

		#endregion

		#region ValidateBookingTemplate

		public void ValidateBookingTemplate()
		{
			BookingTemplateInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(BookingTemplateInfo);
		}

		#endregion

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return !IsSystemDefined; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("33f8cc66-5e77-40e0-8a55-335a41888234", "This is a system defined value and cannot be deleted."); }
		}

		#endregion

		#region Lists

		#region ParentTypes

		public CodeDescriptionPairList ParentTypes
		{
			get
			{
				if (parentTypes == null)
				{
					parentTypes = new CodeDescriptionPairList();
					parentTypes.AddPair("ALL", Res.GetString("5c55ae1d-43cf-46aa-b226-797590638721", "Default Fallback Template for All types of Transport Bookings."));

					foreach (CodeDescriptionPair parentType in new Shared.ParentTypes().List)
					{
						parentTypes.AddPair(parentType.Code, Res.GetString("4d93a36d-c684-46c5-aef3-b21dda54522a", "Default Template on '{0}' Transport Bookings Only.", parentType.Description));
					}
				}

				return parentTypes;
			}
		}

		CodeDescriptionPairList parentTypes;

		#endregion

		#region Directions

		public CodeDescriptionPairList Directions
		{
			get
			{
				if (directions == null)
				{
					directions = new CodeDescriptionPairList();
					directions.AddPair("ALL", Res.GetString("2587a9a2-fa54-4f48-9aed-e8fbad9e630a", "Default Fallback Template for All Directions."));

					foreach (CodeDescriptionPair jobDirection in new Shared.Directions().List)
					{
						directions.AddPair(jobDirection.Code, Res.GetString("afbdd478-d5c9-400b-9541-79df5f08f6ac", "Default Template on Parent Jobs that have a Direction of '{0}'.", jobDirection.Description));
					}
				}

				return directions;
			}
		}

		CodeDescriptionPairList directions;

		#endregion

		#region TransportModes

		public CodeDescriptionPairList TransportModes
		{
			get
			{
				if (transportModes == null)
				{
					transportModes = new CodeDescriptionPairList();
					transportModes.AddPair("ALL", Res.GetString("c03d1179-829f-4087-88cb-154e6cf66d87", "Default Fallback Template for All Transport Modes."));

					var description = Res.GetString("82c6ce59-5fa5-441a-abde-9bc1315808a8", "Default Template on Parent Jobs that have a Transport Mode '{0}'.", "{0}");
					transportModes.AddPair(Constants.TransportModes.Air, string.Format(Culture.Invariant, description, Constants.TransportModeDescriptions.Air));
					transportModes.AddPair(Constants.TransportModes.Sea, string.Format(Culture.Invariant, description, Constants.TransportModeDescriptions.Sea));
					transportModes.AddPair(Constants.TransportModes.Road, string.Format(Culture.Invariant, description, Constants.TransportModeDescriptions.Road));
					transportModes.AddPair(Constants.TransportModes.Rail, string.Format(Culture.Invariant, description, Constants.TransportModeDescriptions.Rail));
				}

				return transportModes;
			}
		}

		CodeDescriptionPairList transportModes;

		#endregion

		#region ContainerModes

		public CodeDescriptionPairList ContainerModes
		{
			get
			{
				if (containerModes == null)
				{
					containerModes = new CodeDescriptionPairList();
					containerModes.AddPair("ALL", Res.GetString("71dabd1f-cda1-4295-9ea2-d668bac54bb7", "Default Fallback Template for All Container Modes."));

					var description = Res.GetString("cb8efcf0-c3b7-479c-ac07-c598057b7416", "Default Template on Parent Jobs that have a Container Mode '{0}'.", "{0}");
					containerModes.AddPair(Constants.CartageContainerMode.Loose, string.Format(Culture.Invariant, description, Constants.CartageContainerModeDescription.Loose));
					containerModes.AddPair(Constants.CartageContainerMode.Containerized, string.Format(Culture.Invariant, description, Constants.CartageContainerModeDescription.Containerized));
				}

				return containerModes;
			}
		}
		CodeDescriptionPairList containerModes;

		#endregion

		#region OrganisationTypes

		public LocalCartageJobOrgTypeList OrganisationTypes
		{
			get { return LocalCartageJobOrgTypeList.Instance; }
		}

		#endregion

		#region BookingTemplates

		public CodeDescriptionPairList BookingTemplates
		{
			get
			{
				var bookingTemplates = new CodeDescriptionPairList();

				var dynamicCollection = new DynamicBusinessObjectCollection(CurrentFactory);
				var sql = string.Format(Culture.Invariant, "SELECT {0}, {1} FROM {2} WHERE {3} = 1",
					DtbBookingTmplSchema.Constants.KT_Code, DtbBookingTmplSchema.Constants.KT_Description, DtbBookingTmplSchema.Constants.TableName, DtbBookingTmplSchema.Constants.KT_IsActive);

				dynamicCollection.Load(sql);

				foreach (DynamicBusinessObject template in dynamicCollection)
				{
					var code = (ZString)template[DtbBookingTmplSchema.Constants.KT_Code];
					var description = (ZString)template[DtbBookingTmplSchema.Constants.KT_Description];
					bookingTemplates.AddPair(code, description);
				}

				bookingTemplates.SortByDescription();

				return bookingTemplates;
			}
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Parent, Parent.ToString());
			writer.WriteElementString(Schema.Direction, Direction.ToString());
			writer.WriteElementString(Schema.TransportMode, TransportMode.ToString());
			writer.WriteElementString(Schema.ContainerMode, ContainerMode.ToString());
			writer.WriteElementString(Schema.HasOrganisation, HasOrganisation.ToString());
			writer.WriteElementString(Schema.BookingTemplate, BookingTemplate.ToString());
			writer.WriteElementString(Schema.Notes, Notes.ToString());
			writer.WriteElementString(Schema.IsSystemDefined, IsSystemDefined.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			using (GetValidationSuspender())
			{
				Parent = wrapper.ReadElementString(Schema.Parent);
				Direction = wrapper.ReadElementString(Schema.Direction);
				TransportMode = wrapper.ReadElementString(Schema.TransportMode);
				ContainerMode = wrapper.ReadElementString(Schema.ContainerMode);
				HasOrganisation = wrapper.ReadElementString(Schema.HasOrganisation);
				BookingTemplate = wrapper.ReadElementString(Schema.BookingTemplate);
				Notes = wrapper.ReadElementString(Schema.Notes);
				IsSystemDefined = wrapper.ReadElementStringAsZBool(Schema.IsSystemDefined);
			}
		}

		#endregion
	}
}
