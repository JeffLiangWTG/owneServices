using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public abstract class CharterStateSpecificCollection : JobSailingCollection
	{
		protected CharterStateSpecificCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SailingScheduleDefaultFilterProvider provider = new SailingScheduleDefaultFilterProvider();
			provider.IsChartered = IsCharter;
			provider.SetDefaultFilters(this);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZDBOnlySubQuery voyageFilter = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyageSchema.PK);
			voyageFilter.AddToFilter(JobVoyageSchema.JV_IsChartered, IsCharter);

			ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
			originFilter.AddSubQuery(JobVoyOriginSchema.JA_JV, voyageFilter, JoinCondition.And);

			ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
			sailingFilter.AddSubQuery(JobSailingSchema.JX_JA, originFilter, JoinCondition.And);

			return sailingFilter;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			JobSailing sailing = (JobSailing)selectedBusinessObject;

			if (sailing.Voyage.JV_IsChartered != IsCharter)
			{
				errors.Add(string.Format(ErrorFormat, sailing.Voyage.SailingTextLowerCase));
			}
		}

		protected abstract ZBool IsCharter { get; }
		protected abstract ZString ErrorFormat { get; }
	}
}
