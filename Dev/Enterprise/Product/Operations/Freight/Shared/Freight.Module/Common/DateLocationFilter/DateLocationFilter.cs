using System;
using System.Diagnostics;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Module
{
	public class DateLocationFilter : ModuleDateFilter
	{
		#region LocationTypes

		public enum LocationTypes
		{
			Load,
			Discharge
		}

		#endregion

		#region TargetFilterTypes

		public enum TargetFilterTypes
		{
			Consol,
			Shipment
		}

		#endregion

		#region Construction

		public DateLocationFilter(ZString description, SailingFilterBuilder.Dates dateType, IBusinessObjectCollection locationList, LocationTypes locationType, TargetFilterTypes targetFilterType, BusinessObjectFactory factory)
			: base(description, delegate
			{ return new ZQuery(); })
		{
			EnsureListIsLocationCollection(locationList);

			this.locationList = locationList;
			this.LocationType = locationType;
			this.dateType = dateType;
			this.factory = factory;
			this.TargetFilterType = targetFilterType;
		}

		void EnsureListIsLocationCollection(IBusinessObjectCollection businessObjectCollection)
		{
			var iLocationCollectionType = ObjectFactory.GetType<ILocationCollection>();
			Argument.NotNull(businessObjectCollection, "locationList");

			if (!iLocationCollectionType.IsInstanceOfType(businessObjectCollection))
			{
				throw new ArgumentException("locationList is not a LocationCollection.");
			}
		}

		#endregion

		#region Properties

		public IBusinessObjectCollection LocationList
		{
			get { return locationList; }
		}
		readonly IBusinessObjectCollection locationList;

		public readonly LocationTypes LocationType;

		readonly SailingFilterBuilder.Dates dateType;

		readonly BusinessObjectFactory factory;

		readonly TargetFilterTypes TargetFilterType;

		[BusinessObjectTestExclude] // doesn't need a maxlength
		public ZString Property3
		{
			get { return property3; }
			set
			{
				if (property3 != value)
				{
					property3 = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty3();
					}
					Property3Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property3Info
		{
			get { return GetZPropertyInfo(nameof(Property3)); }
		}

		ZString property3;

		public Validation Property3Validation
		{
			[DebuggerStepThrough]
			get { return property3Validation; }
			[DebuggerStepThrough]
			set { property3Validation = value; }
		}
		Validation property3Validation;

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Dates; }
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			Property3 = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && Property3.IsEmpty;

		#endregion

		#region GetQuery

		protected override ZQuery GetQuery()
		{
			DateComparisonOperator comparisonOperator = GetDateLocationComparisonOperator();
			SailingFilterBuilder builder = new SailingFilterBuilder(factory);

			builder.SetDateRange(dateType, comparisonOperator, FromDate, ToDate);

			if (!Property3.IsEmpty)
			{
				if (LocationType == LocationTypes.Load)
				{
					builder.LoadPort = Property3;
				}
				else
				{
					builder.DischargePort = Property3;
				}
			}

			builder.ShouldSearchForUnlinkedOrNoTransports = ShouldSearchForUnlinkedOrNoTransports;

			if (TargetFilterType == TargetFilterTypes.Consol)
			{
				return builder.ToConsolFilter();
			}
			else if (TargetFilterType == TargetFilterTypes.Shipment)
			{
				return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.AllSchedules);
			}

			return null;
		}

		DateComparisonOperator GetDateLocationComparisonOperator()
		{
			if (IsPropertySearchUsingHasNoDateEntered)
			{
				return DateComparisonOperator.HasNoDateEntered;
			}
			else if (IsPropertySearchUsingHasDateEntered)
			{
				return DateComparisonOperator.HasDateEntered;
			}

			return DateComparisonOperator.HasDateInRange;
		}

		bool ShouldSearchForUnlinkedOrNoTransports
		{
			get
			{
				return IsPropertySearchUsingHasNoDateEntered
					&& Property3.IsEmpty
					&& IsDateTypeSpecificToLinkedTransports;
			}
		}

		bool IsDateTypeSpecificToLinkedTransports
		{
			get
			{
				return dateType == SailingFilterBuilder.Dates.LoadATA || dateType == SailingFilterBuilder.Dates.LoadETA;
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("Property3", Property3);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "Property3")
			{
				Property3 = reader.ReadElementString("Property3");
			}
		}

		#endregion

		#region Validation

		public new DateLocationFilterValidation Validation
		{
			get { return (DateLocationFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new DateLocationFilterValidation(this);
		}

		public class DateLocationFilterValidation : ModuleFilterDateValidation
		{
			public DateLocationFilterValidation(DateLocationFilter parent)
				: base(parent)
			{
				this.Parent = parent;
			}

			public void ValidateProperty3()
			{
				ValidateCalculatedProperty(Parent.Property3Info);
			}

			public override void ValidateAll()
			{
				base.ValidateAll();
				ValidateProperty3();
			}

			protected virtual void CheckProperty3()
			{
				ListValidation.ErrorIfInvalidCode(Parent.Property3Info, Parent.LocationList);

				if (Parent.Property3Validation != null)
				{
					Parent.Property3Validation(Parent.Property3Info);
				}
			}

			public override Type AutoValidationType
			{
				get { return this.GetType(); }
			}

			protected new readonly DateLocationFilter Parent;
		}

		#endregion

	}
}
