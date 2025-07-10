using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RatingHeaderCollection : BusinessObjectCollection<RatingHeader>
	{
		public RatingHeaderCollection(BusinessObjectFactory factory)
			: this(factory, GlbCompany.CurrentCompany)
		{
		}

		public RatingHeaderCollection(BusinessObjectFactory factory, GlbCompany company)
			: this(factory, company, null)
		{
		}

		public RatingHeaderCollection(BusinessObjectFactory factory, GlbCompany company, ZQuery query)
			: base(factory, query)
		{
			this.Company = company ?? GlbCompany.CurrentCompany;
		}

		#region RelationshipFilter

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();

			query.AddToFilter(GetCurrentCompanyFilter());

			if (!RateType.IsEmpty)
			{
				query.AddToFilter(RatingHeaderSchema.TH_RateType, RateType);
			}

			return query;
		}

		protected virtual ZQuery GetCurrentCompanyFilter()
		{
			return new ZQuery(RatingHeaderSchema.TH_GC, Company.PK);
		}

		protected readonly GlbCompany Company;

		internal ZString RateType
		{
			get
			{
				if (!RateTypeIsSet)
				{
					var addNewMethod = GetType().GetMethod("AddNew", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (addNewMethod != null)
					{
						fRateType = RatingHeader.TypesAndCodes.GetCode(addNewMethod.ReturnType);
						RateTypeIsSet = true;
					}
				}

				return fRateType;
			}
		}

		ZString fRateType;
		bool RateTypeIsSet;

		#endregion

#if DEBUG
		public IFindBoxListProvider FindBoxListProvider_ForTest
			=> FindBoxListProvider;
#endif
	}
}

