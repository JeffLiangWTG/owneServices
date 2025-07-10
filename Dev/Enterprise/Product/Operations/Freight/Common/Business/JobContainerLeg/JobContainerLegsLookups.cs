//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCartageRunSheetLookups
//
//    This class should be used for overriding collections in AutoJobCartageRunSheetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class JobContainerLegsLookups : AutoJobContainerLegsLookups
	{
		public JobContainerLegsLookups(AutoJobContainerLegs parent)
			: base(parent)
		{
		}

		#region LocalTransport_List

		public LocalTransportCollection LocalTransportList
		{
			get
			{
				if (fLocalTransport_List == null)
				{
					fLocalTransport_List = new LocalTransportCollection(Factory);
				}
				return fLocalTransport_List;
			}
		}
		LocalTransportCollection fLocalTransport_List;

		#endregion

		#region GlbStaff

		protected GlbStaffCollection fGlbStaffList;
		public GlbStaffCollection GlbStaffList
		{
			get
			{
				if (fGlbStaffList == null)
				{
					fGlbStaffList = new GlbStaffCollection(Factory);
				}
				return fGlbStaffList;
			}
		}

		#endregion

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
				}
				return fJU_Message_Status_List;
			}
		}

		CodeDescriptionPairList fJU_Message_Status_List;

		#endregion

		#region RefEquipment_List

		public RefEquipmentCollection RefEquipmentList
		{
			get
			{
				if (fRefEquipmentList == null)
				{
					fRefEquipmentList = new RefEquipmentCollection(Factory);
				}
				return fRefEquipmentList;
			}
		}

		protected RefEquipmentCollection fRefEquipmentList;

		#endregion

		#region BindToLists

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion
	}
}
