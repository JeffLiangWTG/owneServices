//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbPortDeliveryTimeLookups
//
//    This class should be used for overriding collections in AutoGlbPortDeliveryTimeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPortDeliveryTimeLookups : AutoGlbPortDeliveryTimeLookups
	{
		public GlbPortDeliveryTimeLookups(AutoGlbPortDeliveryTime parent) : base(parent)
		{
		}

		#region Code Description Pair Lists

		CodeDescriptionPairList fFreightMode;
		public CodeDescriptionPairList FreightMode
		{
			get
			{
				if (fFreightMode == null)
				{
					fFreightMode = new CodeDescriptionPairList();
					fFreightMode.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
					fFreightMode.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
					fFreightMode.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
					fFreightMode.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
					fFreightMode.AddPair(Constants.ContainerModes.LCL, Constants.TransportModeDescriptions.Sea + " - " + Constants.ContainerModeDescriptions.LCL);
					fFreightMode.AddPair(Constants.ContainerModes.FCL, Constants.TransportModeDescriptions.Sea + " - " + Constants.ContainerModeDescriptions.FCL);
				}
				return fFreightMode;
			}
		}

		CodeDescriptionPairList fJobMode;
		public CodeDescriptionPairList JobMode
		{
			get
			{
				if (fJobMode == null)
				{
					fJobMode = new CodeDescriptionPairList();
					fJobMode.AddPair("FWD", Res.GetString("0e1848af-8c4e-4e7d-983d-353dae9c3e89", "Forwarding Only"));
					fJobMode.AddPair("CUS", Res.GetString("ec26515a-bbb0-4517-a0e6-eef09410b127", "Customs Only"));
					fJobMode.AddPair("ALL", Res.GetString("f91a1322-fd01-46ce-b5c8-60be7764a328", "Forwarding and Customs"));
				}
				return fJobMode;
			}
		}

		ConsigneeCollection fConsignees;
		public ConsigneeCollection Consignees
		{
			get
			{
				if (fConsignees == null)
				{
					fConsignees = new ConsigneeCollection(Factory);
				}
				return fConsignees;
			}
		}

		#endregion
	}
}
