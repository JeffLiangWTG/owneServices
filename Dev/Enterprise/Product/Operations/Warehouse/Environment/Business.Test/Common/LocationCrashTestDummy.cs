using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class LocationCrashTestDummy : DummyBusinessObject, ILocationConsumer
	{
		public LocationCrashTestDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		#region Location

		public ZGuid LocationWhsGuid { get; set; }

		public ZPropertyInfo LocationWhsGuidInfo => GetZPropertyInfo(nameof(LocationWhsGuid));

		public ZGuid WLV_FK { get; set; }

		public ZPropertyInfo WLV_FKInfo => GetZPropertyInfo(nameof(WLV_FK));

		public WhsLocation Location => Factory.Load<WhsLocation>(WLV_FK);

		[MaxLength(36)]
		public ZString LocationString
		{
			get
			{
				var location = Location;
				return location != null ? location.WLV_LocationString : locationString;
			}
			set
			{
				var location = Location;
				if (location != null && location.WLV_LocationString != value || locationString != value || !WLV_FK.IsValid)
				{
					//prevalidation
					CheckMaximumLength(LocationStringInfo, value);

					locationString = value;
					WLV_FK = FindLocationPK(Factory, value, LocationWhsGuid);

					if (!IsValidationSuspended)
					{
						ValidateLocationString();
					}
				}
			}
		}

		ZString locationString;

		public ZPropertyInfo LocationStringInfo => GetZPropertyInfo(nameof(LocationString));

		public ZGuid LocationPK => WLV_FK;

		public ZString LocationTypeForMessages => "Object";

		public ZString LocationTitle { get; set; }

		public ZGuid WarehousePK => LocationWhsGuid;

		protected abstract ZGuid FindLocationPK(BusinessObjectFactory factory, ZString locationString, ZGuid whsPK);

		protected abstract void ValidateLocationString();

		#endregion
	}

	public abstract class LocationCrashTestDummyValidation : DummyBizoValidation
	{
		public LocationCrashTestDummyValidation(LocationCrashTestDummy parent)
				: base(parent)
		{
		}

		public void ValidateLocationString()
		{
			ValidateCalculatedProperty(((LocationCrashTestDummy)Parent).LocationStringInfo);
		}

		protected abstract void CheckLocationString();
	}
}
