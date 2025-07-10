using System.Collections.Generic;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingContainerWithPackLinesDataObjectWriter : ContainerWithPackLinesDataObjectWriter<ForwardingConsol>
	{
		public ForwardingContainerWithPackLinesDataObjectWriter(IContainerLinkManager<ForwardingConsol> containerLinkManager, BindToLists listCache, IDataWritingManager manager) : base(containerLinkManager, listCache, manager)
		{
		}

		protected override Container PopulateDataObject(CommonContainer containerBo)
		{
			var containerDataObject = base.PopulateDataObject(containerBo);
			if (containerBo is ForwardingContainer forwardingContainer)
			{
				PopulateAddInfoCollection(forwardingContainer, containerDataObject);
			}
			return containerDataObject;
		}

		void PopulateAddInfoCollection(ForwardingContainer forwardingContainer, Container containerDataObject)
		{
			if (!AdvOrmFeatureHelper.IsEnabled)
			{
				return;
			}
			var addInfoCollection = new List<AddInfo>();
			if (!forwardingContainer.JC_CLH_LoadListPlan.IsEmpty && forwardingContainer.ContainerLoadPlan.CLH_Status != Constants.ContainerLoadListHeaderStatus.Cancelled)
			{
				addInfoCollection.Add(new AddInfo
				{
					Key = "ContainerLoadList",
					Value = forwardingContainer.ContainerLoadPlan.CLH_LoadListId,
				});
			}
			if (!forwardingContainer.JC_JSB_SupplierBooking.IsEmpty && forwardingContainer.SupplierBooking.JSB_Status != Constants.SupplierBookingStatus.Cancelled)
			{
				addInfoCollection.Add(new AddInfo
				{
					Key = "SupplierBooking",
					Value = forwardingContainer.SupplierBooking.JSB_BookingId,
				});

				if (forwardingContainer.ContainerLoadListLines.Count > 0 && forwardingContainer.ContainerLoadListLines[0].LoadListHeader.CLH_Status != Constants.ContainerLoadListHeaderStatus.Cancelled)
				{
					addInfoCollection.Add(new AddInfo
					{
						Key = "ContainerLoadList",
						Value = forwardingContainer.ContainerLoadListLines[0].LoadListHeader.CLH_LoadListId,
					});
				}
			}
			if (addInfoCollection.Count > 0)
			{
				containerDataObject.SetAddInfoCollection(() => addInfoCollection);
			}
		}
	}
}
