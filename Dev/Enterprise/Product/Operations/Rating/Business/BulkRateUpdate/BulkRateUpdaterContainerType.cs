using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class BulkRateUpdaterContainerType : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BulkRateUpdaterContainerType(BusinessObjectFactory factory, RateEntryLookups parentLookups, RefContainerCollection modeRestrictedContainersForValidation) : base(factory)
		{
			Lookups = parentLookups;
			this.modeRestrictedContainersForValidation = modeRestrictedContainersForValidation;
		}

		public RateEntryLookups Lookups { get; }

		public ZString RC_Description => Container?.RC_DescriptionMultilingual ?? ZString.Empty;

		public ZPropertyInfo RC_DescriptionInfo => GetZPropertyInfo(nameof(RC_Description));

		public RefContainer Container => Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, RC_Code);

		[List("Lookups.Containers")]
		public ZString RC_Code
		{
			get { return code; }
			set
			{
				SetNonPersistentPropertyValue(RC_CodeInfo, ref code, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateRC_Code();
				}
			}
		}

		ZString code;

		public RefContainerCollection modeRestrictedContainersForValidation;

		public ZPropertyInfo RC_CodeInfo => GetZPropertyInfo(nameof(RC_Code));

		public BulkRateUpdaterContainerTypeValidation Validation => new BulkRateUpdaterContainerTypeValidation(this);
	}
}
