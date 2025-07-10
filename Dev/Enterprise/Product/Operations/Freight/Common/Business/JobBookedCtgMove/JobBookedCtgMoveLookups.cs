//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobBookedCtgMoveLookups
//
//    This class should be used for overriding collections in AutoJobBookedCtgMoveLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class JobBookedCtgMoveLookups : AutoJobBookedCtgMoveLookups
	{
		public JobBookedCtgMoveLookups(AutoJobBookedCtgMove parent) : base(parent)
		{
		}

		#region Message status list

		public virtual CodeDescriptionPairList MessageStatusList
		{
			get
			{
				if (fJU_Message_Status_List == null)
				{
					fJU_Message_Status_List = new CodeDescriptionPairList();
					fJU_Message_Status_List.AddPair(Constants.CartageLegDispatchStatusList.Codes.NotStarted, Constants.CartageLegDispatchStatusList.Descriptions.NotStarted);
					fJU_Message_Status_List.AddPair(Constants.CartageLegDispatchStatusList.Codes.Delivered, Constants.CartageLegDispatchStatusList.Descriptions.Delivered);
					fJU_Message_Status_List.AddPair(Constants.CartageLegDispatchStatusList.Codes.Futile, Constants.CartageLegDispatchStatusList.Descriptions.Futile);
					fJU_Message_Status_List.AddPair(Constants.CartageLegDispatchStatusList.Codes.PickedUp, Constants.CartageLegDispatchStatusList.Descriptions.PickedUp);
					fJU_Message_Status_List.AddPair(Constants.CartageLegDispatchStatusList.Codes.Rejected, Constants.CartageLegDispatchStatusList.Descriptions.Rejected);
					fJU_Message_Status_List.AddPair(Constants.CartageLegDispatchStatusList.Codes.Runsheet, Constants.CartageLegDispatchStatusList.Descriptions.Runsheet);
					fJU_Message_Status_List.AddPair(Constants.CartageLegDispatchStatusList.Codes.WIP, Constants.CartageLegDispatchStatusList.Descriptions.WIP);
				}
				return fJU_Message_Status_List;
			}
		}

		CodeDescriptionPairList fJU_Message_Status_List;

		#endregion

		#region DimensionUnits

		public CodeDescriptionPairList DimensionUnits
		{
			get { return BindToLists.DimensionUnits; }
		}

		#endregion

		#region BindToLists

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion
	}
}
