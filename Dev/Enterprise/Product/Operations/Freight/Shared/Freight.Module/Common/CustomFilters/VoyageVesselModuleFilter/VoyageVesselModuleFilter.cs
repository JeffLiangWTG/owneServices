using System;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class VoyageVesselModuleFilter : ModuleTextBaseFilter, IModuleFilterWithModuleID
	{
		public delegate ZQuery GetVoyageVesselQueryDelegate(SQLComparisonOperator voyageFlightComparisonOperator, ZString voyageFlight, ZString vessel, ZBool includeArchived);

		public VoyageVesselModuleFilter(ZString description, GetVoyageVesselQueryDelegate queryDelegate, RefVesselCollection vesselsList) : base(description, queryDelegate)
		{
			this.vesselsList = Argument.NotNull(vesselsList, "vessels");
		}

		readonly RefVesselCollection vesselsList;

		public ZString VoyageFlightNo
		{
			get { return Property; }
			set
			{
				Property = value;
				VoyageFlightNoInfo.RefreshBinding();
			}
		}

		public bool VoyageFlightNo_ReadOnly
		{
			get
			{
				return PropertyInfo.ReadOnly;
			}
		}

		public ZPropertyInfo VoyageFlightNoInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(VoyageFlightNo));
			}
		}

		[List("Vessels")]
		public ZString Vessel
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.IsBlank || ComparisonOperator == ComparisonConstants.IsNotBlank)
				{
					return GetEmptyPropertyValue();
				}
				return vessel;
			}
			set
			{
				if (vessel != value)
				{
					vessel = value;
					VesselInfo.RefreshBinding();
				}
			}
		}

		public bool Vessel_ReadOnly
		{
			get
			{
				return PropertyInfo.ReadOnly;
			}
		}

		public ZPropertyInfo VesselInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Vessel));
			}
		}

		ZString vessel;

		public ZBool IncludeArchived { get; set; }

		public RefVesselCollection Vessels
		{
			get { return vesselsList; }
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			Vessel = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && VoyageFlightNo.IsEmpty && Vessel.IsEmpty
			&& SqlComparisonOperator != SpecialComparisonOperator.IsBlank
			&& SqlComparisonOperator != SpecialComparisonOperator.IsNotBlank;

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, VoyageFlightNo, Vessel, IncludeArchived }; }
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("Vessel", Vessel);
			writer.WriteElementString("IncludeArchived", IncludeArchived ? "Y" : "N");
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			Vessel = reader.ReadElementString("Vessel");
			IncludeArchived = reader.ReadElementString("IncludeArchived") == "Y";
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);

			var nkFilter = filterToCopyFrom as ModuleTextAndNkFilter;

			if (nkFilter != null)
			{
				Vessel = nkFilter.NkProperty;
			}
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException();
		}

		public ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.RefVessel;
			}
		}

		#region MaxLength

		public VoyageVesselModuleFilter WithMaxLengthOf(SchemaColumn voyageColumn, SchemaColumn vesselColumn)
		{
			MaxLength = voyageColumn.MaxLength;
			VesselMaxLength = vesselColumn.MaxLength;
			return this;
		}

		public int VesselMaxLength
		{
			get => vesselMaxLength > 0 ? vesselMaxLength : MaxLength;
			set => vesselMaxLength = value;
		}

		int vesselMaxLength;

		#endregion
	}
}
