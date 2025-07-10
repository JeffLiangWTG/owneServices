using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class PortMessageLookups : ZLookups
	{
		public PortMessageLookups(PortMessage parent)
			: base(parent) { }

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

					if (port.Directions.HasFlag(PortDirections.Discharge))
					{
						result.AddPair(Constants.PortDirection.Discharge);
					}
				}

				return result;
			}
		}

		public virtual PortMessageTargetPortList Port_List
		{
			get
			{
				if (portList == null)
				{
					portList = new PortMessageTargetPortList(Parent.Voyage);
					portList.Load();
				}

				return portList;
			}
		}

		public PortMessageTypeList MessageType_List
		{
			get
			{
				if (messageTypeList == null)
				{
					messageTypeList = new PortMessageTypeList();
				}

				return messageTypeList;
			}
		}

		public virtual OrgHeaderCollection CTOs_List
		{
			get
			{
				return BindToLists.GetCachedLists(Factory).SeaCTO_List;
			}
		}

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

		public OrgHeaderCollection LinkedPrincipals
		{
			get
			{
				return Factory.GetCachedValue("LinkedPrincipals_" + Parent.Port + "_" + Parent.Direction, () =>
				{
					var billOfLadings = Parent.GetRelatedShipments(Factory);
					var linkedPrincipals = new OrgHeaderCollection(Factory);
					linkedPrincipals.RemoveAll();

					foreach (BillOfLading billOfLading in billOfLadings)
					{
						linkedPrincipals.Add(Factory.Load<OrgHeader>(billOfLading.JS_OH_DeliveryAgent));
					}

					return linkedPrincipals;
				});
			}
		}

		#region Version_List

		public PortAuthorityVersionList Version_List
		{
			get { return version_list ?? (version_list = new PortAuthorityVersionList()); }
		}
		PortAuthorityVersionList version_list;

		#endregion

		#region Implementation

		public new PortMessage Parent
		{
			get { return (PortMessage)base.Parent; }
		}

		PortMessageTypeList messageTypeList;

		PortMessageTargetPortList portList;

		#endregion
	}
}
