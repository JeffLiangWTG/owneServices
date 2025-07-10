using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class DangerousGoodsManifestMessageLookups : ZLookups
	{
		public DangerousGoodsManifestMessageLookups(DangerousGoodsManifestMessage parent) : base(parent)
		{
			Argument.NotNull(parent, "DangerousGoodsManifestMessage");
			Argument.NotNull(parent.Voyage, "voyage");
		}

		#region Port_List

		public DangerousGoodsManifestMessageTargetPortList Port_List
		{
			get
			{
				if (portList == null)
				{
					portList = new DangerousGoodsManifestMessageTargetPortList(Parent.Voyage);
					portList.Load();
				}

				return portList;
			}
		}

		DangerousGoodsManifestMessageTargetPortList portList;

		#endregion

		#region Principal_List

		public OrgHeaderCollection Principal_List
		{
			get
			{
				var principalList = new OrgHeaderCollection(Factory);
				var port = Port_List[Parent.Port];
				var isDirectionValid = Parent.Direction.IsEmpty || Direction_List[Parent.Direction] != null;

				if (port != null && isDirectionValid)
				{
					var direction = Parent.Direction;

					if (direction.IsEmpty && (port.Directions == PortDirections.Load || port.Directions == PortDirections.Discharge))
					{
						direction = port.Directions == PortDirections.Load ? Constants.PortDirection.Load : Constants.PortDirection.Discharge;
					}

					var filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, Parent.Voyage, Parent.Port, direction);
					var relatedBillOfLadings = Factory.Load<BillOfLading>(filter).Where(x => x.Principal != null);

					foreach (var billOfLading in relatedBillOfLadings)
					{
						if (!principalList.Contains(billOfLading.Principal))
						{
							principalList.Add(billOfLading.Principal);
						}
					}
				}

				return principalList;
			}
		}

		#endregion

		#region Direction_List

		public CodeDescriptionPairList Direction_List
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var port = Port_List[Parent.Port];

				if (port != null)
				{
					if (port.Directions.HasFlag(PortDirections.Load))
					{
						result.AddPair(Constants.PortDirection.Load);
					}

					if (port.Directions.HasFlag(PortDirections.Transit))
					{
						result.AddPair(Constants.PortDirection.Transit);
					}

					if (port.Directions.HasFlag(PortDirections.Discharge))
					{
						result.AddPair(Constants.PortDirection.Discharge);
					}
				}

				return result;
			}
		}

		#endregion

		#region MessageType_List

		public CodeDescriptionPairList MessageType_List
		{
			get
			{
				if (messageType_List == null)
				{
					messageType_List = new CodeDescriptionPairList();

					messageType_List.AddPair(DangerousGoodsManifestMessageTypeList.Codes.Original, DangerousGoodsManifestMessageTypeList.Descriptions.Original);
					messageType_List.AddPair(DangerousGoodsManifestMessageTypeList.Codes.Amendment, DangerousGoodsManifestMessageTypeList.Descriptions.Amendment);
					messageType_List.AddPair(DangerousGoodsManifestMessageTypeList.Codes.Withdrawal, DangerousGoodsManifestMessageTypeList.Descriptions.Withdrawal);
				}

				return messageType_List;
			}
		}

		CodeDescriptionPairList messageType_List;

		#endregion

		#region Implementation

		public new DangerousGoodsManifestMessage Parent
		{
			get { return (DangerousGoodsManifestMessage)base.Parent; }
		}

		#endregion
	}
}
