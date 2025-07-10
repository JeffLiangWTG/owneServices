using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	#region Abstract RatingHeaderFindBoxListProvider

	public abstract class RatingHeaderFindBoxListProvider : FindBoxListProvider
	{
		public RatingHeaderFindBoxListProvider(BusinessObjectCollection list, ZQuery relationshipFilter)
			: base(list)
		{
			this.RelationshipFilter = relationshipFilter;
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
		{
			return BizObjsFromCodeWithCompleteFilter(code);
		}

		protected ZQuery RelationshipFilter;
	}

	#endregion

	#region RatingHeaderFindBoxListProviderFromOrganisationCode

	public class RatingHeaderFindBoxListProviderForOrganisationCode : RatingHeaderFindBoxListProvider
	{
		public RatingHeaderFindBoxListProviderForOrganisationCode(BusinessObjectCollection list, ZQuery relationshipFilter)
			: base(list, relationshipFilter)
		{
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			var organisation = List.Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code));

			if (organisation != null)
			{
				var additionalQuery = new ZQuery(RatingHeaderSchema.TH_OH, organisation.PK);
				return List.Factory.Load(List.TypeOfElements, additionalQuery);
			}
			return Enumerable.Empty<BusinessObject>();
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			var organisation = List.Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code));

			if (organisation != null)
			{
				var additionalQuery = new ZQuery(RatingHeaderSchema.TH_OH, organisation.PK);
				additionalQuery.AddToFilter(RelationshipFilter, JoinCondition.And);
				return List.Factory.Load(List.TypeOfElements, additionalQuery);
			}
			return Enumerable.Empty<BusinessObject>();
		}
	}

	#endregion

	#region RatingHeaderFindBoxListProciderForRateLevel

	public class RatingHeaderFindBoxListProviderForRateLevel : RatingHeaderFindBoxListProvider
	{
		public RatingHeaderFindBoxListProviderForRateLevel(BusinessObjectCollection list, ZQuery relationshipFilter)
			: base(list, relationshipFilter)
		{
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			var parsedResult = ZByte.Zero;
			if (ZByte.TryParse(code, out parsedResult))
			{
				var additionalQuery = new ZQuery(RatingHeaderSchema.TH_GlobalRateLevel, parsedResult);
				additionalQuery.AddToFilter(RelationshipFilter, JoinCondition.And);
				return List.Factory.Load(List.TypeOfElements, additionalQuery);
			}
			return base.BizObjsFromCodeWithCompleteFilter(code);
		}
	}

	#endregion
}

