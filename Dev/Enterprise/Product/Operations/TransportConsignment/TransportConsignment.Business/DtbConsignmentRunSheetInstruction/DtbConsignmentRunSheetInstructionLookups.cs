//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentRunSheetInstructionLookups
//
//    This class should be used for overriding collections in AutoDtbConsignmentRunSheetInstructionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.TransportConsignment.Registry;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetInstructionLookups : AutoDtbConsignmentRunSheetInstructionLookups
	{
		public DtbConsignmentRunSheetInstructionLookups(AutoDtbConsignmentRunSheetInstruction parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList FailureReasons
		{
			get
			{
				return LandTransportRegistry.Instance.TransportFailureReasons.Value;
			}
		}
	}
}
