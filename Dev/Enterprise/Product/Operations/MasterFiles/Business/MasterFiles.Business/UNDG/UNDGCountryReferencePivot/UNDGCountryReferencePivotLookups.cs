//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGCountryReferencePivotLookups
//
//    This class should be used for overriding collections in AutoUNDGCountryReferencePivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGCountryReferencePivotLookups : AutoUNDGCountryReferencePivotLookups
	{
		public UNDGCountryReferencePivotLookups(AutoUNDGCountryReferencePivot parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StorageInstructions
		{
			get { return new StorageInstructionList(); }
		}
	}
}
