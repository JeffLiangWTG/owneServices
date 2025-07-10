using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(OrderData),
	Enterprise.Core.Constants.DocManagerCodes.Order)]

namespace Enterprise.Freight.Forwarding.Business
{
	class OrderData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Order); } }

		protected override Type CollectionType
		{
			get { return typeof(OrderCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new OrderCollection(factory);
		}

		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("eeb309ca-c93e-465e-95f7-ee03d77a21a4", "Order"); } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			if (assemblyDataParams != null && assemblyDataParams.CompanyCode.IsValid && !assemblyDataParams.CompanyCode.IsEmpty)
			{
				var queryOrders = new ZDBOnlyQuery(BusinessObjectType);
				var subQueryOrgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_BuyerAddress);
				var subQueryOrgHeader = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				subQueryOrgHeader.AddToFilter(OrgHeaderSchema.OH_Code, assemblyDataParams.CompanyCode);
				subQueryOrgAddress.AddSubQuery(subQueryOrgHeader, JoinCondition.And);
				queryOrders.AddSubQuery(subQueryOrgAddress, JoinCondition.And);

				return new OrderCollection(factory, queryOrders);
			}
			else
			{
				return base.GetBusinessObjectCollection(factory, assemblyDataParams);
			}
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Orders; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
