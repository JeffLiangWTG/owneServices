//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManTranHeadLookups
//
//    This class should be used for overriding collections in AutoCusSeaManTranHeadLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusSeaManTranHeadLookups : AutoCusSeaManTranHeadLookups
	{
		public CusSeaManTranHeadLookups(AutoCusSeaManTranHead parent) : base(parent)
		{
		}

		public CodeDescriptionPairList AllArrivalPorts
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				foreach (CusSeaManArrivalPort arrival in TranHead.Arrivals)
				{
					result.AddPair(arrival.BA_RL_NKArrivalPort, arrival.BA_RL_NKArrivalPort);
				}
				return result;
			}
		}

		#region Implementation

		CusSeaManTranHead TranHead
		{
			get
			{
				return (CusSeaManTranHead)Parent;
			}
		}

		#endregion
	}
}
