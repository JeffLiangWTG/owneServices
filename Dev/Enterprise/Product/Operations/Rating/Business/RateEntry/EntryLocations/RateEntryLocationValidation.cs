using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class RateEntryLocationValidation : ZValidation
	{
		public RateEntryLocationValidation(RateEntryLocation parentRateEntryLocation)
			: base(parentRateEntryLocation)
		{
			RateEntryLocation = Argument.NotNull(parentRateEntryLocation, nameof(parentRateEntryLocation));

			ZValidationInternals = this;
		}

		RateEntryLocation RateEntryLocation { get; }

		public override Type AutoValidationType => typeof(RateEntryLocationValidation);

		#region ValidateLocation

		public void ValidateLocation()
		{
			ZValidationInternals.Validate(RateEntryLocation.LocationInfo, GetLocationValidationInvoker());
		}

		RunValidationInvoker GetLocationValidationInvoker()
		{
			return delegate
			{
				var info = RateEntryLocation.LocationInfo;

				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info, RateEntryLocation.Lookups.Locations);
			};
		}

		#endregion

		#region ValidateLocationSourceOption

		public void ValidateLocationSourceOption()
		{
			ZValidationInternals.Validate(RateEntryLocation.LocationSourceOptionInfo, GetLocationSourceOptionValidationInvoker());
		}

		RunValidationInvoker GetLocationSourceOptionValidationInvoker()
		{
			return delegate
			{
				var info = RateEntryLocation.LocationSourceOptionInfo;

				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info, RateEntryLocation.Lookups.LocationSourceOptions);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(info);
			};
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateLocation();
			ValidateLocationSourceOption();
		}

		IValidationInternals ZValidationInternals { get; }
	}
}
