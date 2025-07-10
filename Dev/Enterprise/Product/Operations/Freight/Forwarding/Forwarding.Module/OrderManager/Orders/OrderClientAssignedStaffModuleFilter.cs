using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrderClientAssignedStaffModuleFilter : OrgClientAssignedStaffModuleFilter
	{
		public OrderClientAssignedStaffModuleFilter(ZString description, GetClientAssignedStaffQuery queryDelegate)
			: base(description, queryDelegate, ClientTypes)
		{
		}

		static CodeDescriptionPairList ClientTypes
		{
			get { return new OrderClientTypesList(); }
		}

		protected override ClientTypesList GetNewClientTypesList()
		{
			return new OrderClientTypesList();
		}

		protected override ZString GetAddressType()
		{
			if (ClientType == OrderClientTypesList.Codes.ControllingCustomer)
			{
				return DocAddressTypes.Codes.ControllingCustomer;
			}

			return ZString.Empty;
		}

		protected override ZDBOnlySubQuery GetClientAssignedStaffFilter()
		{
			if (ClientType == OrderClientTypesList.Codes.ControllingCustomer)
			{
				return base.GetClientAssignedStaffFilter();
			}

			var query = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);

			if (OrgAddressSchemaColumn != null)
			{
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchemaColumn);

				var clientAssignedStaffQuery = GetClientAssignedStaffSubQuery();
				orgAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_OH, clientAssignedStaffQuery, JoinCondition.And);

				AddControllingBranchSubQuery(orgAddressSubQuery, OrgAddressSchema.OA_OH);

				query.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			}

			return query;
		}

		SchemaColumn OrgAddressSchemaColumn
		{
			get
			{
				if (ClientType == OrderClientTypesList.Codes.Buyer || ClientType == OrderClientTypesList.Codes.Consignee)
				{
					return JobOrderHeaderSchema.JD_OA_BuyerAddress;
				}
				else if (ClientType == OrderClientTypesList.Codes.Supplier || ClientType == OrderClientTypesList.Codes.Consignor)
				{
					return JobOrderHeaderSchema.JD_OA_SupplierAddress;
				}

				return null;
			}
		}
	}
}
