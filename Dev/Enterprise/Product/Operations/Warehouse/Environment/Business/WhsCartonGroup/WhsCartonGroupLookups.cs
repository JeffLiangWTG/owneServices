//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsCartonGroupLookups
//
//    This class should be used for overriding collections in AutoWhsCartonGroupLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsCartonGroupLookups : AutoWhsCartonGroupLookups
	{
		public WhsCartonGroupLookups(AutoWhsCartonGroup parent) : base(parent)
		{
		}

		#region CartonSizes

		public WhsCartonSizeCollection CartonSizes
		{
			get { return Factory.GetCachedValue("WhsCartonGroupLookups|CartonSizes", () => new WhsCartonSizeCollection(Factory)); }
		}

		#endregion

		#region Organisations

		public ParentOrgLookupCollection Organisations
		{
			get { return Factory.GetCachedValue("WhsCartonGroupLookups|Organisations", () => new ParentOrgLookupCollection(Factory)); }
		}

		#endregion

		#region OptimizationModes

		public CartonizationOptimizationModes OptimizationModes
		{
			get { return Factory.GetCachedValue("WhsCartonGroupLookups|OptimizationModes", () => new CartonizationOptimizationModes()); }
		}

		#endregion
	}
}
