using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Telematics.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Business
{
	[CodeProperty(GlbDeviceSchema.Constants.V3_HumanReadableIdentifier)]
	public class GlbDevice : AutoGlbDevice, IDevice
	{
		public GlbDevice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			lazyBatteries = new Lazy<GlbDeviceBatteryCollection>(() => new GlbDeviceBatteryCollection(Factory, new ZQuery(GlbDeviceBatterySchema.GDB_V3_Device, PK)));
			lazyCombinationReports = new Lazy<GlbDeviceCombinationReportCollection>(() => new GlbDeviceCombinationReportCollection(Factory, new ZQuery(GlbDeviceExternalVoltageSchema.GDV_V3_Device, PK)));
			lazyExternalVoltages = new Lazy<GlbDeviceExternalVoltageCollection>(() => new GlbDeviceExternalVoltageCollection(Factory, new ZQuery(GlbDeviceExternalVoltageSchema.GDV_V3_Device, PK)));
			lazyIgnitions = new Lazy<GlbDeviceIgnitionCollection>(() => new GlbDeviceIgnitionCollection(Factory, new ZQuery(GlbDeviceIgnitionSchema.GDI_V3_Device, PK)));
			lazyLocations = new Lazy<GlbDeviceLocationCollection>(() => new GlbDeviceLocationCollection(Factory, new ZQuery(GlbDeviceLocationSchema.V2_V3_Device, PK)));
			lazyLogs = new Lazy<GlbDeviceLogCollection>(() => new GlbDeviceLogCollection(Factory, new ZQuery(GlbDeviceLogSchema.GDL_V3_Device, PK)));
			lazyOdometers = new Lazy<GlbDeviceOdometerCollection>(() => new GlbDeviceOdometerCollection(Factory, new ZQuery(GlbDeviceOdometerSchema.GDO_V3_Device, PK)));
			lazyOnboardMasses = new Lazy<GlbDeviceOnboardMassCollection>(() => new GlbDeviceOnboardMassCollection(Factory, new ZQuery(GlbDeviceOnboardMassSchema.GDM_V3_Device, PK)));
			lazyTemperatures = new Lazy<GlbDeviceTemperatureCollection>(() => new GlbDeviceTemperatureCollection(Factory, new ZQuery(GlbDeviceTemperatureSchema.GDT_V3_Device, PK)));
			lazyTyreAlerts = new Lazy<GlbDeviceTyreAlertCollection>(() => new GlbDeviceTyreAlertCollection(Factory, new ZQuery(GlbDeviceTyreAlertSchema.GDA_V3_Device, PK)));
			lazyTyreReports = new Lazy<GlbDeviceTyreReportCollection>(() => new GlbDeviceTyreReportCollection(Factory, new ZQuery(GlbDeviceTyreReportSchema.GDR_V3_Device, PK)));

			lazyAssignments = new Lazy<GlbDeviceAssignmentDivotCollection>(() =>
			{
				var query = new ZQuery(GlbDeviceAssignmentDivotSchema.V7_V3_Device, PK);
				return new GlbDeviceAssignmentDivotCollection(Factory, query);
			});
		}

		public void UpdateDeviceKey(DeviceKey device_key)
		{
			if (device_key == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(device_key.identifier))
			{
				return;
			}

			var expectedDeviceKind = GlbDeviceKindCodes.Get(device_key.kind);
			var updateDeviceKind = false;
			var updateDeviceID = false;

			if (V3_HardwareKind != expectedDeviceKind)
			{
				if (V3_HardwareKind == GlbDeviceKindCodes.Unknown)
				{
					updateDeviceKind = true;
				}
				else
				{
					ErrorReporter.ReportOnce(
						key: "DeviceManagementInvalidDeviceKindReassignment",
						message: FormattableString.Invariant(
							$"Attempted to reassign device ({V3_HardwareKind}, {V3_HardwareIdentifier}) to ({expectedDeviceKind}, {device_key.identifier})"));
					return;
				}
			}

			if (V3_HardwareIdentifier.IsEmpty)
			{
				updateDeviceID = true;
			}
			else if (!string.Equals(V3_HardwareIdentifier, device_key.identifier, StringComparison.OrdinalIgnoreCase))
			{
				ErrorReporter.ReportOnce(
					key: "DeviceManagementInvalidDeviceIdentifierReassignment",
					message: FormattableString.Invariant(
						$"Attempted to reassign device ({V3_HardwareKind}, {V3_HardwareIdentifier}) to ({expectedDeviceKind}, {device_key.identifier})"));
				return;
			}

			if (updateDeviceKind)
			{
				V3_HardwareKind = expectedDeviceKind;
			}

			if (updateDeviceID)
			{
				V3_HardwareIdentifier = device_key.identifier;
			}
		}

		#region AutoGlbDevice

		protected override ZString HumanReadableNameCore => V3_HumanReadableIdentifier;

		void CalculateAssignedParentID()
		{
			if (hasCalculatedAssignedParentID)
			{
				return;
			}

			var assignment = GetLatestAssignment();
			if (assignment != null && assignment.V7_EndTimeUtc.IsEmpty)
			{
				switch (assignment.V7_ParentTableCode)
				{
					case GlbStaffSchema.Constants.Prefix:
						assignedParentStaffID = assignment.V7_ParentID;
						break;

					case RefEquipmentSchema.Constants.Prefix:
						assignedParentEquipmentID = assignment.V7_ParentID;
						break;
				}
			}

			hasCalculatedAssignedParentID = true;
		}
		bool hasCalculatedAssignedParentID;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			SaveAssignedParent();
		}

		#endregion

		#region Properties

		public GlbDeviceBatteryCollection Batteries => lazyBatteries.Value;
		readonly Lazy<GlbDeviceBatteryCollection> lazyBatteries;

		public GlbDeviceCombinationReportCollection CombinationReports => lazyCombinationReports.Value;
		readonly Lazy<GlbDeviceCombinationReportCollection> lazyCombinationReports;

		public GlbDeviceExternalVoltageCollection ExternalVoltages => lazyExternalVoltages.Value;
		readonly Lazy<GlbDeviceExternalVoltageCollection> lazyExternalVoltages;

		public GlbDeviceIgnitionCollection Ignitions => lazyIgnitions.Value;
		readonly Lazy<GlbDeviceIgnitionCollection> lazyIgnitions;

		public GlbDeviceLocationCollection Locations => lazyLocations.Value;
		readonly Lazy<GlbDeviceLocationCollection> lazyLocations;

		public GlbDeviceLogCollection DeviceLogs => lazyLogs.Value;
		readonly Lazy<GlbDeviceLogCollection> lazyLogs;

		public GlbDeviceOdometerCollection Odometers => lazyOdometers.Value;
		readonly Lazy<GlbDeviceOdometerCollection> lazyOdometers;

		public GlbDeviceOnboardMassCollection OnboardMasses => lazyOnboardMasses.Value;
		readonly Lazy<GlbDeviceOnboardMassCollection> lazyOnboardMasses;

		public GlbDeviceTemperatureCollection Temperatures => lazyTemperatures.Value;
		readonly Lazy<GlbDeviceTemperatureCollection> lazyTemperatures;

		public GlbDeviceTyreAlertCollection TyreAlerts => lazyTyreAlerts.Value;
		readonly Lazy<GlbDeviceTyreAlertCollection> lazyTyreAlerts;

		public GlbDeviceTyreReportCollection TyreReports => lazyTyreReports.Value;
		readonly Lazy<GlbDeviceTyreReportCollection> lazyTyreReports;

		public GlbDeviceAssignmentDivotCollection Assignments => lazyAssignments.Value;
		readonly Lazy<GlbDeviceAssignmentDivotCollection> lazyAssignments;

		[ReadOnly(isReadOnly: true)]
		public override ZString V3_HumanReadableIdentifier
		{
			get => base.V3_HumanReadableIdentifier;
			set => base.V3_HumanReadableIdentifier = value;
		}

		[ReadOnlyMember(nameof(IsRimRegistered))]
		public override ZString V3_HardwareIdentifier
		{
			get => base.V3_HardwareIdentifier;
			set => base.V3_HardwareIdentifier = value;
		}

		[ReadOnlyMember(nameof(IsRimRegistered))]
		public override ZString V3_HardwareKind
		{
			get => base.V3_HardwareKind;
			set => base.V3_HardwareKind = value;
		}

		[ReadOnlyMember(nameof(IsRimRegistered))]
		public override ZString V3_Model
		{
			get => base.V3_Model;
			set => base.V3_Model = value;
		}

		public bool IsRimRegistered
		{
			get
			{
				var query = new ZQuery();
				query.AddToFilter(TelEdgeSchema.TE_EntityIdTo, SQLComparisonOperator.Equal, PK);
				query.AddToFilter(TelEdgeSchema.TE_RelationshipType, SQLComparisonOperator.Equal, TelEdgeRelationshipTypes.Codes.RIM);
				query.AddToFilter(TelEdgeSchema.TE_EndTime, SQLComparisonOperator.Equal, ZDateTime.Empty);
				return Factory.Exists(typeof(TelEdge), query);
			}
		}

		#endregion

		#region Find By Identifier

		public static GlbDevice FindDeviceByMobileServicesIdentifier(BusinessObjectFactory factory, byte[] identifier)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(identifier, nameof(identifier));

			var primaryKey = FindDevicePKByMobileServicesIdentifier(identifier);
			if (primaryKey == ZGuid.Empty)
			{
				return null;
			}

			return factory.Load<GlbDevice>(primaryKey);
		}

		public static GlbDevice FindBYODByHumanReadableIdentifier(BusinessObjectFactory factory, string humanReadableIdentifier)
		{
			return factory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HumanReadableIdentifier, humanReadableIdentifier).AddToFilter(GlbDeviceSchema.V3_IsBYOD, true));
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "MSSQL can do equality on byte[]/varbinary, but ADO.NET cannot.")]
		static ZGuid FindDevicePKByMobileServicesIdentifier(byte[] identifier)
		{
			using (var cmd = Db.Connection.Command(FindDeviceByMobileServicesIdentifierSqlScript))
			{
				cmd.AddParameter("@deviceIdentifier", SqlDbType.VarBinary, identifier);

				var result = cmd.ExecuteScalar();
				return (Guid?)result ?? Guid.Empty;
			}
		}

		static string FindDeviceByMobileServicesIdentifierSqlScript
		{
			get
			{
				return string.Format(
#pragma warning disable CW1161 // ResGetStringAnalyzer
					"SELECT [{0}] FROM [{1}] WHERE [{2}] = @deviceIdentifier",
#pragma warning restore CW1161 // ResGetStringAnalyzer
					GlbDeviceSchema.Constants.PK,
					GlbDeviceSchema.Constants.TableName,
					GlbDeviceSchema.Constants.V3_MobileServicesIdentifier);
			}
		}

		#endregion

		#region Assigned Parent

		#region Properties for Module Grid

		[ResourceStringData("GlbDevice|AssignedParent", Caption = "Assigned To")]
		public ZString AssignedParentName
		{
			get
			{
				if (!assignedParentName.HasValue)
				{
					assignedParentName = CalculateAssignedParentName();
				}
				return assignedParentName ?? ZString.Empty;
			}
		}
		ZString? assignedParentName;

		ZString CalculateAssignedParentName()
		{
			CalculateAssignedParentID();

			if (!assignedParentEquipmentID.IsEmpty && assignedParentEquipmentID.IsValid)
			{
				return GetAssignedParentName(RefEquipmentSchema.Constants.Prefix, assignedParentEquipmentID);
			}

			if (!assignedParentStaffID.IsEmpty && assignedParentStaffID.IsValid)
			{
				return GetAssignedParentName(GlbStaffSchema.Constants.Prefix, assignedParentStaffID);
			}

			return string.Empty;
		}

		ZString GetAssignedParentName(string parentTableCode, ZGuid parentID)
		{
			var lookup = GetTelematicsLookup(parentTableCode);
			var bizo = lookup.GetBusinessObject(Factory, parentID);

			return GetAssignedParentName(bizo);
		}

		static ZString GetAssignedParentName(ITelematicsBusinessObject bizo)
		{
			if (bizo == null)
			{
				return ZString.Empty;
			}

			return string.Format("{0} - {1} ({2})", bizo.TypeIdentifier, bizo.Code, bizo.DescriptionForInterface);
		}

		#endregion

		[ReadOnlyMember(nameof(IsRimRegistered))]
		[ResourceStringData("GlbDevice|AssignedParentEquipmentID", Caption = "Assigned Equipment")]
		[List("Lookups.ParentEquipmentLookups")]
		public ZGuid AssignedParentEquipmentID
		{
			get
			{
				CalculateAssignedParentID();
				return assignedParentEquipmentID;
			}
			set
			{
				SetAssignedParentID(ref assignedParentEquipmentID, value);
				hasCalculatedAssignedParentID = true;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAssignedParentEquipmentID();
				}
				AssignedParentEquipmentIDInfo.RefreshBinding();
			}
		}
		ZGuid assignedParentEquipmentID;

		public ZPropertyInfo AssignedParentEquipmentIDInfo => GetZPropertyInfo(nameof(AssignedParentEquipmentID));

		[ResourceStringData("GlbDevice|AssignedParentStaffID", Caption = "Assigned Staff")]
		[List("Lookups.ParentStaffLookups")]
		public ZGuid AssignedParentStaffID
		{
			get
			{
				CalculateAssignedParentID();
				return assignedParentStaffID;
			}
			set
			{
				SetAssignedParentID(ref assignedParentStaffID, value);
				hasCalculatedAssignedParentID = true;
				if (!IsValidationSuspended)
				{
					Validation.ValidateAssignedParentStaffID();
				}
				AssignedParentStaffIDInfo.RefreshBinding();
			}
		}
		ZGuid assignedParentStaffID;

		public ZPropertyInfo AssignedParentStaffIDInfo => GetZPropertyInfo(nameof(AssignedParentStaffID));

		public void ClearAssignedParent()
		{
			AssignedParentEquipmentID = ZGuid.Empty;
			AssignedParentStaffID = ZGuid.Empty;
		}

		void ClearAssignedParentCore()
		{
			AssignedParentEquipmentID = ZGuid.Empty;
			AssignedParentStaffID = ZGuid.Empty;
		}

		void SetAssignedParentID(ref ZGuid member, ZGuid value)
		{
			if (member == value)
			{
				return;
			}

			if (!isSettingParentID)
			{
				isSettingParentID = true;
				try
				{
					ClearAssignedParentCore();
				}
				finally
				{
					isSettingParentID = false;
				}
			}

			member = value;

			if (!HasChanges)
			{
				HasChanges = true;
			}

			RefreshBinding();
		}
		bool isSettingParentID;

		void SaveAssignedParent()
		{
			// If we haven't touched the AssignedParentXXXXXID properties, this will be false.
			// If nothing has changed, there's nothing to do.
			if (!hasCalculatedAssignedParentID)
			{
				return;
			}

			if (!assignedParentStaffID.IsEmpty)
			{
				SetAssignedParent(GlbStaffSchema.Constants.Prefix, assignedParentStaffID);
			}
			else if (!assignedParentEquipmentID.IsEmpty)
			{
				SetAssignedParent(RefEquipmentSchema.Constants.Prefix, assignedParentEquipmentID);
			}
			else
			{
				SetAssignedParent(ZString.Empty, ZGuid.Empty);
			}
		}

		void SetAssignedParent(ZString parentTableCode, ZGuid primaryKey)
		{
			var currentAssignment = GetLatestAssignment();
			if (currentAssignment != null && !currentAssignment.V7_EndTimeUtc.IsValid && currentAssignment.V7_ParentTableCode == parentTableCode && currentAssignment.V7_ParentID == primaryKey)
			{
				return;
			}

			SealAssignmentIfOpen(currentAssignment);

			var telematicsNotifier = ObjectFactory.Get<ITelematicsNotifier>();
			if (primaryKey.IsEmpty)
			{
				if (telematicsNotifier.ShouldNotifySynchronously)
				{
					telematicsNotifier.NotifyMobileServicesOfNewDeviceParent(Factory, V3_HumanReadableIdentifier, string.Empty, string.Empty, string.Empty);
				}
				return;
			}

			var assignment = Assignments.AddNew();
			assignment.V7_V3_Device = PK;
			assignment.V7_ParentID = primaryKey;
			assignment.V7_ParentTableCode = parentTableCode;
			assignment.V7_StartTimeUtc = ZDateTime.UtcNow;

			if (telematicsNotifier.ShouldNotifySynchronously)
			{
				var telematicsLookup = GetTelematicsLookup(parentTableCode);
				var bizo = telematicsLookup.GetBusinessObject(Factory, primaryKey);
				if (bizo != null)
				{
					telematicsNotifier.NotifyMobileServicesOfNewDeviceParent(Factory, V3_HumanReadableIdentifier, bizo.Code, bizo.DescriptionInEnglish, parentTableCode);
				}
			}

			assignedParentName = null;
		}

		static ITelematicsBusinessObjectLookupProvider GetTelematicsLookup(string parentTableCode)
		{
			var lookupName = string.Format("{0}_{1}", nameof(ITelematicsBusinessObjectLookupProvider), parentTableCode);
			var provider = ObjectFactory.Get<ITelematicsBusinessObjectLookupProvider>(lookupName);
			return provider;
		}

		void SealAssignmentIfOpen(GlbDeviceAssignmentDivot assignment)
		{
			if (assignment == null)
			{
				return;
			}

			if (!assignment.V7_EndTimeUtc.IsEmpty)
			{
				return;
			}

			assignment.V7_EndTimeUtc = ZDateTime.UtcNow;
		}

		GlbDeviceAssignmentDivot GetLatestAssignment()
		{
			var query = new ZQuery(GlbDeviceAssignmentDivotSchema.V7_V3_Device, SQLComparisonOperator.Equal, PK) { OrderBy = GlbDeviceAssignmentDivotSchema.Constants.V7_EndTimeUtc };
			return Factory.LoadTop1<GlbDeviceAssignmentDivot>(query);
		}

		#endregion
	}
}
