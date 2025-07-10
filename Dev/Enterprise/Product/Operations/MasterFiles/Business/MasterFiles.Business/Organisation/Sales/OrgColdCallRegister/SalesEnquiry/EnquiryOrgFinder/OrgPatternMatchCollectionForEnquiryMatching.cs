using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPatternMatchCollectionForEnquiryMatching : OrgPatternMatchCollection
	{
		public OrgPatternMatchCollectionForEnquiryMatching(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		public override ZBool AllowEmptyAddresses
		{
			get { return true; }
		}

		protected override ZQuery GetUnlocoFilter(ZString unloco)
		{
			return new ZQuery();
		}

		protected override ZQuery GetBusinessRegNoFilter(ZString businessRegNo)
		{
			ZQuery result = new ZQuery();

			if (!businessRegNo.IsEmpty)
			{
				result.AddToFilter(OrgPatternMatch.CreateFilterAllowBlanks(OrgPatternMatch.CustomParameterOperators.Equals, OrgPatternMatchSchema.OS_BusinessRegNo, businessRegNo));
			}

			return result;
		}

		protected override JoinCondition GetBusinessRegNoJoinCondition()
		{
			return JoinCondition.Or;
		}
	}
}
