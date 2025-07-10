//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUnderbondLookups
//
//    This class should be used for overriding collections in AutoCusUnderbondLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusUnderbondLookups : AutoCusUnderbondLookups
	{
		public CusUnderbondLookups(AutoCusUnderbond parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList ModeOfTransportList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public CodeDescriptionPairList UnderbondForList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				foreach (ICusUnderbondDependentCollectionParent provided in ((CusUnderbond)Parent).GetAllPossibleCollectionProviders())
				{
					result.Add(new CodeDescriptionPair(provided.UnderbondHumanReadableName.ToString(), provided.UnderbondHumanReadableName.ToString()));
				}
				return result;
			}
		}

		public virtual CodeDescriptionPairList RequestReasonList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList UnderbondStatusList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList OutturnStatusList
		{
			get { return new CodeDescriptionPairList(); }
		}

		#region TranshipBySeaVessels

		public virtual RefVesselCollection TranshipBySeaVessels
		{
			get { return new RefVesselCollection(Factory); }
		}

		#endregion

		#region UnderbondBySeaVessels

		public virtual RefVesselCollection UnderbondBySeaVessels
		{
			get { return new RefVesselCollection(Factory); }
		}

		#endregion

		OrgHeaderCollection fAirCTOs;
		public OrgHeaderCollection AirCTOs
		{
			get
			{
				if (fAirCTOs == null)
				{
					fAirCTOs = new AirCTOCollection(Factory);
				}
				return fAirCTOs;
			}
		}

		OrgHeaderCollection fCustomsControlledPremises;
		public OrgHeaderCollection CustomsControlledPremisesList
		{
			get
			{
				if (fCustomsControlledPremises == null)
				{
					fCustomsControlledPremises = new CTOOrDepotOrWarehouseCollection(Factory);
				}
				return fCustomsControlledPremises;
			}
		}
	}
}
