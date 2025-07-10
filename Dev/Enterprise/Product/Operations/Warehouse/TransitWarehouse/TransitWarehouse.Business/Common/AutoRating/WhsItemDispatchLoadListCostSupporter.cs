using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchLoadListCostSupporter : GenericJobCostSupporter
	{
		public WhsItemDispatchLoadListCostSupporter(WhsItemDispatchLoadList parent)
		{
			Parent = parent;
		}

		public WhsItemDispatchLoadList Parent { get; private set; }

		public override DocumentSupporter DocumentSupporter => Parent is IDocumentSupportable docSupportable ? docSupportable.DocumentSupporter : null;

		public override ZDateTime ETA => ZDateTime.Empty;

		public override ZDateTime ETD => ZDateTime.Empty;

		public override bool HasChanges => Parent.HasChanges;

		public override bool IsInDatabase => Parent.IsInDatabase;

		public override ZString MasterBillNum => Parent.MasterBillNumber;

		public override ZGuid GetCreditorPK(ZString chargeCode, ZGuid rateProviderOrgPK) => chargeCode == ChargeCodeGroupList.Codes.TRWDispatchLoadList ? TransitWarehouseHelper.GetTransportCompanyOrganisation(Parent.Creditor).PK : ZGuid.Empty;

		public override ZGuid PK => Parent.PK;

		public override IJobInvoicingPlugIn[] ShipmentsList => shipmentList ?? (shipmentList = Parent.PackageStates.Select(p => p.DispatchConsignment).Distinct().ToArray());

		IJobInvoicingPlugIn[] shipmentList;

		public override ZGuid[] ShipmentsListPKs => ((IGenericJobCostSupporter)this).ShipmentsList.Select(b => b.PK).ToArray();

		public override ZString Type => Parent.TablePrefix;

		public override IEnumerable<ZString> ExcludedApportionmentMethods
		{
			get
			{
				yield return AllocationMethod.Revenue;
				yield return AllocationMethod.ChargeableUnits;
				yield return AllocationMethod.ContainerCount;
				yield return AllocationMethod.TwentyFootEquivalentUnit;
				yield return AllocationMethod.CapacityPerContainer;
				yield return AllocationMethod.FreeSpaceContribution;
			}
		}
	}
}
