using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Rating.Business
{
	sealed public class RateEntryLocation : NonPersistentBusinessObject<RateEntryLocationValidation>
	{
		#region Schema

		public abstract class Schema
		{
			public const string Location = nameof(Location);
			public const string LocationSourceOption = nameof(LocationSourceOption);

			public const int LocationMaxLength = 5;
			public const int LocationSourceOptionMaxLength = 3;
		}

		#endregion

		public RateEntryLocation(RateEntryLocationCollection parentCollection)
			: base(parentCollection?.Factory)
		{
			ParentCollection = Argument.NotNull(parentCollection, nameof(parentCollection));
		}

		RateEntryLocationCollection ParentCollection { get; }
		RateEntry ParentRateEntry => ParentCollection.ParentRateEntry;

		#region Location

		[MaxLength(Schema.LocationMaxLength)]
		[List("Lookups.Locations")]
		[ResourceStringData("RateEntryLocation|Location", Caption = "Location", ShortCaption = "Loc.")]
		public ZString Location
		{
			get => location;
			set
			{
				SetNonPersistentPropertyValue(LocationInfo, ref location, value);
				Validation.ValidateLocation();
			}
		}
		ZString location;

		public ZPropertyInfo LocationInfo
			=> GetZPropertyInfo(nameof(Location));

		#endregion

		#region LocationSourceOption

		[MaxLength(Schema.LocationSourceOptionMaxLength)]
		[List("Lookups.LocationSourceOptions")]
		[ResourceStringData("RateEntryLocation|LocationSourceOption", Caption = "Related Job Field", MediumCaption = "Job Field", ShortCaption = "Field")]
		public ZString LocationSourceOption
		{
			get => locationSourceOption;
			set
			{
				SetNonPersistentPropertyValue(LocationSourceOptionInfo, ref locationSourceOption, value);
				Validation.ValidateLocationSourceOption();
			}
		}
		ZString locationSourceOption;

		public ZPropertyInfo LocationSourceOptionInfo
			=> GetZPropertyInfo(nameof(LocationSourceOption));

		#endregion

		#region Validations

		public override RateEntryLocationValidation GetNewValidation()
			=> new RateEntryLocationValidation(this);

		#endregion

		#region Lookups

		public RateEntryLookups Lookups => ParentRateEntry.Lookups;

		#endregion
	}
}
