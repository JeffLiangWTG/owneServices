//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProcedureAttributeLookups
//
//    This class should be used for overriding collections in AutoRefCusProcedureAttributeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class RefCusProcedureAttributeLookups : AutoRefCusProcedureAttributeLookups
	{
		public RefCusProcedureAttributeLookups(AutoRefCusProcedureAttribute parent) : base(parent)
		{
		}

		public new RefCusProcedureAttribute Parent => (RefCusProcedureAttribute)base.Parent;

		public CodeDescriptionPairList AttributeNames => Factory.GetCachedValue<AttributeNames>();
	}
}
