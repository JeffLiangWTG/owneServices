using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderCollectionBuilder : WhsDocketCollectionBuilder<WhsOrder, OrderLineData>
	{
		public WhsOrderCollectionBuilder(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void AddNewDocketCore(WhsOrder docket, OrderLineData data)
		{
			base.AddNewDocketCore(docket, data);
			docket.ConsigneeDocAddress.SetActualFieldValuesFromParent(data.ConsigneeDocAddress);
			docket.WD_WL_CrossDock = data.CrossDockLocationPK;
		}

		protected override bool AdditionalMatchingCriteria(WhsOrder docket, OrderLineData data)
			=> base.AdditionalMatchingCriteria(docket, data)
			&& IsTheSameDocAddress(data.ConsigneeDocAddress, docket.ConsigneeDocAddress);

		public static bool IsTheSameDocAddress(JobDocAddress address1, JobDocAddress address2)
		{
			return address1 == null || address2 == null ||
				(address1.E2_AddressOverride == address2.E2_AddressOverride &&
				address1.E2_AddressOverride ? address1.IsTheSameDocAddressAndContactAs(address2) : address1.E2_OA_Address == address2.E2_OA_Address);
		}
	}

	public class OrderLineData : LineData
	{
		public JobDocAddress ConsigneeDocAddress;
		public ZGuid CrossDockLocationPK;
	}
}
