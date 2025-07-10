using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class PackLocation : AutoJobPackLoc,
		ILocationConsumer,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn
	{
		public new class Schema : AutoJobPackLoc.Schema
		{
			public const string LocationWhsGuid = "LocationWhsGuid";
			public const string LocationString = "LocationString";
		}

		public PackLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		public override ZDecimal JQ_Weight
		{
			get { return base.JQ_Weight; }
			set { base.JQ_Weight = this.GetRoundedValue(JobPackLocSchema.JQ_Weight, JQ_WeightInfo, value); }
		}

		public override ZString JQ_WeightUW
		{
			get { return base.JQ_WeightUW; }
			set
			{
				base.JQ_WeightUW = value;
				this.SetRoundedValue(JobPackLocSchema.JQ_Weight, JQ_WeightInfo);
			}
		}

		public override ZDecimal JQ_Volume
		{
			get { return base.JQ_Volume; }
			set { base.JQ_Volume = this.GetRoundedValue(JobPackLocSchema.JQ_Volume, JQ_VolumeInfo, value); }
		}

		public override ZString JQ_VolumeUV
		{
			get { return base.JQ_VolumeUV; }
			set
			{
				base.JQ_VolumeUV = value;
				this.SetRoundedValue(JobPackLocSchema.JQ_Volume, JQ_VolumeInfo);
			}
		}

		#endregion

		#region Related Business Objects

		[RelatedBusinessObject("PackLine")]
		public override ZGuid JQ_JL
		{
			get { return base.JQ_JL; }
			set { base.JQ_JL = value; }
		}

		public PackLine PackLine
		{
			get { return GetParentPackLine(); }
		}

		protected virtual PackLine GetParentPackLine()
		{
			return Factory.Load<PackLine>(JQ_JL);
		}

		#endregion

		#region Location

		[List("Lookups.Warehouses")]
		public ZGuid LocationWhsGuid
		{
			get { return WhsLocation?.WLV_WW_Whs ?? locationWhsGuid; }
			set
			{
				var location = WhsLocation;
				if (location != null && location.WLV_WW_Whs != value || locationWhsGuid != value)
				{
					locationWhsGuid = value;
					locationString = ZString.Empty;
					MarkAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateLocationWhsGuid();
					}
				}

				LocationWhsGuidInfo.RefreshBinding();
			}
		}

		ZGuid locationWhsGuid;

		public ZPropertyInfo LocationWhsGuidInfo
		{
			get { return GetZPropertyInfo(Schema.LocationWhsGuid); }
		}

		public IWhsLocation WhsLocation => Factory.Load<IWhsLocation>(JQ_WL);

		[MaxLength(36)]
		public virtual ZString LocationString
		{
			get { return WhsLocation?.WLV_LocationString ?? locationString; }
			set
			{
				var location = WhsLocation;
				if (location != null && location.WLV_LocationString != value || locationString != value || !JQ_WL.IsValid)
				{
					CheckMaximumLength(LocationStringInfo, value);

					locationString = value;
					var whsLocationDataHelper = ObjectFactory.New<IWhsLocationDataHelper>();
					JQ_WL = whsLocationDataHelper.FindLocationPK(Factory, value, LocationWhsGuid);
					if (!IsValidationSuspended)
					{
						Validation.ValidateLocationString();
					}
				}
				LocationStringInfo.RefreshBinding();
			}
		}

		ZString locationString;

		public virtual ZPropertyInfo LocationStringInfo
		{
			get { return GetZPropertyInfo(Schema.LocationString); }
		}

		#endregion

		#region ILocationConsumer Member

		ZGuid ILocationConsumer.LocationPK
		{
			get { return JQ_WL; }
		}

		ZGuid ILocationConsumer.WarehousePK
		{
			get { return LocationWhsGuid; }
		}

		ZString ILocationConsumer.LocationTypeForMessages
		{
			get { return ZString.Empty; }
		}

		ZString ILocationConsumer.LocationTitle { get; set; }

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get
			{
				IDefaultNumberOfDecimalsSupporter parentPackLine = PackLine;
				return (parentPackLine != null) ? parentPackLine.TransportMode : ZString.Empty;
			}
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.JQ_Volume:
					unitOfMeasure = JQ_VolumeUV;
					break;

				case Schema.JQ_Weight:
					unitOfMeasure = JQ_WeightUW;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public ZDecimal GetRoundedValue(CargoWise.Schema.SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		public void RoundMeasurePropertiesOnTransportModeChanged()
		{
			this.SetRoundedValue(JobPackLocSchema.JQ_Weight, JQ_WeightInfo);
			this.SetRoundedValue(JobPackLocSchema.JQ_Volume, JQ_VolumeInfo);
		}

		#endregion
	}
}
