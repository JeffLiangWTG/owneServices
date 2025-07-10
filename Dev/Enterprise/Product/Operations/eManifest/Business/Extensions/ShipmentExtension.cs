
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eManifest.Business
{
	public static class ShipmentExtension
	{
		public static RefCurrency GetCurrencyAtDestination(this CommonShipment shipment)
		{
			Argument.NotNull(shipment, "shipment");

			RefCurrency currency = null;

			if (!shipment.JS_RL_NKDestination.IsEmpty)
			{
				var localCountry = shipment.Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, shipment.JS_RL_NKDestination.Substring(0, 2)));
				if (localCountry != null)
				{
					currency = localCountry.LocalCurrency;
				}
			}

			if (currency == null)
			{
				currency = shipment.Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			}

			return currency;
		}

		public static IEnumerable<SupplierBookingLine> GetSupplierBookingLines(this CommonShipment shipment)
		{
			Argument.NotNull(shipment, "shipment");

			return shipment.Factory.Load<SupplierBookingLine>(new ZQuery(SupplierBookingLineSchema.DL_JS_ApprovedShipment, shipment.PK));
		}

		public static object NullIfEmpty(this ZGuid guid)
		{
			if (guid.IsEmpty)
			{
				return null;
			}
			else
			{
				return guid;
			}
		}

		public static void CreateBillingJobFromBranch(this CommonShipment shipment, GlbBranch branch)
		{
			if (branch == null)
			{
				return;
			}

			var tempBranchContext = new TemporaryUserContext { BranchPK = branch.PK.ToGuid() }.Set();

			//there's no gaurentee here this will still be the context at the time of save
			shipment.Factory.Saved += (factory, successfully) =>
			{
				if (tempBranchContext != null)
				{
					tempBranchContext.Dispose();
					tempBranchContext = null;
				}
			};

			if (ObjectFactory.Get<IAccounting>().ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(shipment))
			{
				var header = new JobHeader.Loader(shipment).TryCreate();

				if (header != null && header.JH_GB.IsEmpty)
				{
					header.JH_GB = GlbBranch.CurrentBranch.PK;
				}

				if (header != null && header.JH_GE.IsEmpty)
				{
					header.JH_GE = GlbDepartment.CurrentDepartment.PK;
				}
			}
		}
	}
}
