using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrderAutoPackAllLinesApplicator : WhsOperationalActionMethodApplicator
	{
		public OrderAutoPackAllLinesApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("07b1a252-asdf-418d-a54f-b2d8461b6ea7", "Auto Pack Orders"), factory) // text used for logging
		{
		}

		const string OutputTextFormat = "{0} {1} - {2}"; // eg. Order 123 - Was Packed ok.

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] orders)
		{
			log.SetSectionProgressMax(orders.Length);
			var currentSection = 0;

			var packageJobsByOrder = new Dictionary<WhsOrder, PkgPackageJob>();
			var typedOrders = orders.Cast<WhsOrder>().ToArray();
			if (typedOrders.Length > 0)
			{
				AddFetchHints(typedOrders, packageJobsByOrder);
			}

			var disablePrinting = WarehouseDataRegistry.Instance.DisablePrintingLabelsOnAutoPack.Value;
			if (!disablePrinting)
			{
				// when printing, Factory Save is done per Order, which resets the query cache.
				// If so, we should poke all eligible order's packableItems upfront
				foreach (var order in typedOrders.Where(IsValidOrder))
				{
					_ = order.PackableItemParents;
				}
			}

			foreach (var order in typedOrders)
			{
				BumpSectionProgress();
				var continueBatch = TryToAutoPack(order, packageJobsByOrder, disablePrinting, log);

				if (!continueBatch)
				{
					var packageJob = GetPackageJobFromOrder(order, packageJobsByOrder);
					if (packageJob != null && packageJob.Packages.Count > 0)
					{
						// prevent saving bad packages
						packageJob.Packages.DeleteAll();
					}

					while (currentSection < typedOrders.Length)
					{
						BumpSectionProgress();
					}
					break;
				}
			}

			bool IsValidOrder(WhsOrder order)
			{
				var packageJob = GetPackageJobFromOrder(order, packageJobsByOrder);
				return
					!order.IsCancelled &&
					!order.IsFinalised &&
					order.WD_WP.IsValid &&
					packageJob != null &&
					packageJob.Packages.Count == 0;
			}

			void BumpSectionProgress()
			{
				currentSection++;
				log.BumpSectionProgress();
			}
		}

		void AddFetchHints(IReadOnlyCollection<WhsOrder> orders, Dictionary<WhsOrder, PkgPackageJob> packageJobsByOrder)
		{
			var factory = orders.First().Factory;

			foreach (var order in orders)
			{
				order.Factory.AddFetchHint(PkgPackageJobSchema.KJ_ParentID, order.PK);
				order.Factory.AddFetchHint(WhsWarehouseSchema.PK, order.WD_WW_Whs);
				order.Factory.AddFetchHint(WhsDocketLineSchema.WE_WD, order.PK);
				order.Factory.AddFetchHint(WhsPickSchema.PK, order.WD_WP);
				order.Factory.AddFetchHint(OrgHeaderSchema.PK, order.WD_OH_Client);
				order.Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, order.WD_OH_Client);
				order.Factory.AddFetchHint(OrgMiscServSchema.OM_OH, order.WD_OH_Client);
				order.Factory.AddFetchHint(JobDocAddressSchema.Instance, FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, order.PK));
				order.Factory.AddFetchHint(WhsDocketSchema.WD_WP, order.WD_WP);
				order.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
			}

			var orgAddressPKsFromWarehouse = AddFetchHintsForWarehouse(orders);
			var orgAddressPKsFromJobDocAddress = AddFetchHintsForJobDocAddress(orders);

			foreach (var orgAddressPK in orgAddressPKsFromWarehouse)
			{
				var orgAddress = factory.Load<OrgAddress>(orgAddressPK);
				factory.AddFetchHint(OrgCusCodeSchema.OK_OH, orgAddress.OA_OH);
			}

			foreach (var orgAddressPK in orgAddressPKsFromJobDocAddress)
			{
				var orgAddress = factory.Load<OrgAddress>(orgAddressPK);
				factory.AddFetchHint(StmEntityScreeningLogSchema.PJ_ParentID, orgAddress.OA_OH);
				factory.AddFetchHint(OrgCusCodeSchema.OK_OH, orgAddress.OA_OH);
				factory.AddFetchHint(OrgMiscServSchema.OM_OH, orgAddress.OA_OH);
				// These OrgAddress rows already have fetch hints but via the PK rather than OA_OH and the actual code filters for OrgAddresses via both fields at different times.
				factory.AddFetchHint(OrgAddressSchema.OA_OH, orgAddress.OA_OH);
			}

			foreach (var line in orders.SelectMany(o => o.Lines))
			{
				factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, line.PK);
			}

			var pickLines = orders.SelectMany(o => o.Lines).SelectMany(p => p.PickLines).ToArray();
			foreach (var pickLine in pickLines)
			{
				factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, pickLine.WZ_WE_InventoryLine);
			}

			foreach (var order in orders)
			{
				var packageJob = GetPackageJobFromOrder(order, packageJobsByOrder);
				if (packageJob != null)
				{
					factory.AddFetchHint(PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, packageJob.PK);
				}
			}

			AddFetchHintsForOriginalInventory(factory, pickLines);
		}

		static PkgPackageJob GetPackageJobFromOrder(WhsOrder order, Dictionary<WhsOrder, PkgPackageJob> packageJobsByOrder)
		{
			if (!packageJobsByOrder.TryGetValue(order, out var result))
			{
				packageJobsByOrder[order] = result = order.Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, order.PK));
			}

			return result;
		}

		HashSet<ZGuid> AddFetchHintsForWarehouse(IEnumerable<WhsOrder> orders)
		{
			var orgAddressPKs = new HashSet<ZGuid>();
			foreach (var order in orders)
			{
				var warehouse = order.Warehouse;
				if (warehouse != null)
				{
					order.Factory.AddFetchHint(GlbBranchSchema.PK, warehouse.WW_GB_RelatedCompanyBranch);
					order.Factory.AddFetchHint(OrgAddressSchema.PK, warehouse.WW_OA_WarehouseAddress);
					orgAddressPKs.Add(warehouse.WW_OA_WarehouseAddress);
				}
			}
			return orgAddressPKs;
		}

		HashSet<ZGuid> AddFetchHintsForJobDocAddress(IEnumerable<WhsOrder> orders)
		{
			var orgAddressPKs = new HashSet<ZGuid>();
			foreach (var order in orders)
			{
				var jobDocAddresses = order.Factory.Load<JobDocAddress>(FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, order.PK));
				foreach (var jobDocAddress in jobDocAddresses)
				{
					if (!jobDocAddress.E2_OA_Address.IsEmpty)
					{
						order.Factory.AddFetchHint(OrgAddressSchema.PK, jobDocAddress.E2_OA_Address);
						orgAddressPKs.Add(jobDocAddress.E2_OA_Address);
					}
				}
			}
			return orgAddressPKs;
		}

		void AddFetchHintsForOriginalInventory(BusinessObjectFactory factory, IEnumerable<WhsPickLine> pickLines)
		{
			var inventoryPKs = pickLines.Select(pl => pl.InventoryLinePKForAvailableInventory).ToArray();
			var inventorySubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			inventorySubQuery.AddToFilter(WhsDocketLineSchema.PK, inventoryPKs);

			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQuery.AddSubQuery(inventorySubQuery, JoinCondition.And);
			factory.AddFetchHint(WhsDocketSchema.Instance, docketQuery);
		}

		#region Auto Pack Picks

		bool TryToAutoPack(WhsOrder order, Dictionary<WhsOrder, PkgPackageJob> packageJobsByOrder, bool disablePrinting, IOperationalActionSectionLog log)
		{
			var continueBatch = true;
			var orderLink = GetDocketIdLink(order);
			var packageJob = GetPackageJobFromOrder(order, packageJobsByOrder);
			if (order.IsCancelled)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink,
					Res.GetString("ddbddb57-91af-asdf-86f5-bfdab319651e", "is canceled and cannot be auto packed"));
			}
			else if (order.IsFinalised)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink,
					Res.GetString("ca52a92b-fdasf-4f97-8fad-422a5c4c9e43", "is already finalized and cannot be auto packed."));
			}
			else if (order.Pick == null)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink,
					Res.GetString("0fc70583-d6f7-4179-81a7-aff1b2bfcb36", "cannot be Auto-Packed as it has not been Picked."));
			}
			else if (packageJob == null)
			{
				// Safeguard -- this should not actually happen because a PackingJob is created on Pick.
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink, Res.GetString("f4ff94aa-2cb5-4e97-979d-e29e84716185",
						"does not have a Packing Job. To rectify, open the Release, click on the Packing tab and then click Save. This will create the necessary Packing information."));
			}
			else
			{
				// If we are printing then we need to save immediately after packing the order which resets the PkgPackage fetch hints
				if (!disablePrinting)
				{
					order.Factory.AddFetchHint(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.PK);
				}

				continueBatch = AutoPackPick(order, log, orderLink, packageJob, disablePrinting);
			}

			return continueBatch;
		}

		bool AutoPackPick(WhsOrder order, IOperationalActionSectionLog log, LogControllerLink orderLink, PkgPackageJob packageJob, bool disablePrinting)
		{
			var continueBatch = true;

			if (packageJob.Packages.Count > 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink,
					Res.GetString("b0d785fa-2ad4-4249-b34f-f8adda16ec34", "has already been packed and was not auto-packed."));
			}
			else
			{
				var notify = new NotificationBuffer();
				if (order.AutoPackAndPrintAllLabels(notify, disablePrinting))
				{
					if (notify.HasWarnings)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink, notify.AsString);
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, order.HumanReadableName, orderLink,
							Res.GetString("e96d16bd-9f0d-asdfasdf-9146-00563893ce43", "was successfully auto-packed."));
					}
				}
				else
				{
					if (notify.HasErrors)
					{
						continueBatch = false;
						log.NotifyFormat(OperationalActionLogErrorLevel.Error, OutputTextFormat, order.HumanReadableName, orderLink,
							Res.GetString("13abd964-f74d-4a27-be8e-asdfasdfasdf", "was not auto-packed.\r\n{0}", notify.AsString));
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, order.HumanReadableName, orderLink,
							Res.GetString("13abd964-f74d-4a27-be8e-asdfasdfasdf", "was not auto-packed.\r\n{0}", notify.AsString));
					}
				}
			}

			return continueBatch;
		}

		#endregion

		#region Validation

		public OrderAutoPackAllLinesApplicatorValdidation Validation => new OrderAutoPackAllLinesApplicatorValdidation(this);

		#endregion
	}

	#region OrderAutoPackAllLinesApplicatorValdidation

	/// <summary>
	/// This class is unnecessary and only exists to satisfy the Z-test for IObsoleteValidation.
	/// </summary>
	public class OrderAutoPackAllLinesApplicatorValdidation : ZValidation
	{
		public OrderAutoPackAllLinesApplicatorValdidation(OrderAutoPackAllLinesApplicator parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType => typeof(OrderAutoPackAllLinesApplicator);

		public override void ValidateAll()
		{
		}
	}

	#endregion
}
