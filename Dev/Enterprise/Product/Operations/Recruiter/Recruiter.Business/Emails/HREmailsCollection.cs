using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HREmailsCollection : ActiveBusinessObjectCollection<HREmails>
	{
		public HREmailsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public HREmailsCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public HREmailsCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(MailDBItemsSchema.MI_Application, HREmails.Code);
			return query;
		}
	}
}
