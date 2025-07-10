//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefTransitTimeLookups
//
//    This class should be used for overriding collections in AutoRefTransitTimeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTransitTimeLookups : AutoRefTransitTimeLookups
	{
		public RefTransitTimeLookups(AutoRefTransitTime parent) : base(parent)
		{
		}

		protected override BusinessObjectFactory Factory
		{
			get
			{
				if (Parent == null)
				{
					return new BusinessObjectFactory();
				}

				return Parent.Factory;
			}
		}

		public CodeDescriptionPairList Modes
		{
			get { return Factory.GetCachedValue("TransitTimes_Modes", () => GetModesPairList()); }
		}

		static CodeDescriptionPairList GetModesPairList()
		{
			return new CodeDescriptionPairList(OLookUpEditType.RateModes);
		}

		public override RefServiceLevelCollection ServiceLevels
		{
			get
			{
				return new RefServiceLevelCollection(Factory,
					new ZQuery(new ZQuery(RefServiceLevelSchema.RS_DeliverOnSaturday, false), JoinCondition.And,
						new ZQuery(RefServiceLevelSchema.RS_DeliverOnSunday, false)));
			}
		}
	}
}
