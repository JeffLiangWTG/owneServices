using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusSCAOceanBillSafeCreator : ICargoSafeCreator
	{
		public CusSCAOceanBillSafeCreator(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
		}
		readonly ForwardingConsol consol;

		DisposableAction TryCreateOrUpdateCusSCAOceanBillAndChildBills(INotifications notifications, out BusinessObject outOceanBill)
		{
			outOceanBill = null;
			var result = DisposableAction.NoAction;

			if (IsSupported(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				var mutexForConsol = BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				if (mutexForConsol.Lock())
				{
					outOceanBill = CreateOrUpdateOceanBill(notifications);
				}
				else
				{
					notifications?.AddError(Res.GetString("4C62F8E8-658C-4CEA-8212-8267949442E4", "A Customs master bill record cannot be created or updated as someone else is trying to create or update a master bill for this consol {0}.", consol.HumanReadableName));
				}

				result = new DisposableAction(() => ((IDisposable)mutexForConsol).Dispose());
			}

			return result;
		}

		BaseCusSCAOceanBill CreateOrUpdateOceanBill(INotifications notifications)
		{
			BaseCusSCAOceanBill result = null;

			var useConsolFactory = !consol.IsInDatabase;
			var factory = useConsolFactory ? consol.Factory : new BusinessObjectFactory();

			var oceanBill = GetExistingCusSCAOceanBill(factory);
			if (oceanBill == null)
			{
				oceanBill = factory.New<BaseCusSCAOceanBill>();
				oceanBill.CB_ParentId = consol.PK;
				oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
				oceanBill.DefaultFromConsol();
			}

			var existingHouseBills = factory.Load<BaseCusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, oceanBill.PK));
			var shipmentsWithoutHouseBill = consol.Shipments.Cast<CommonShipment>()
											.Where(s => !existingHouseBills.Any(house => house.CA_JS == s.PK))
											.Select(s => s.PK);

			foreach (var shipmentPK in shipmentsWithoutHouseBill)
			{
				var house = factory.New<BaseCusSCAHouse>();
				house.CA_JS = shipmentPK;
				house.CA_CB = oceanBill.PK;
				house.DefaultFromShipment();
			}
			oceanBill.LoadHouseBills();

			if (useConsolFactory)
			{
				result = oceanBill;
			}
			else
			{
				try
				{
					factory.Save();
				}
				catch (Exception ex)
				{
					notifications?.AddError(Res.GetString("08FEA597-91F5-4A2A-8654-35C6925C4EE6", "An error occurred when creating a new Customs master bill for this consol {0}. {1}", consol.HumanReadableName, ex.Message));
				}
				finally
				{
					result = GetExistingCusSCAOceanBill(consol.Factory);
				}
			}

			return result;
		}

		public bool IsSupported(string countryCode)
		{
			return consol.IsSea && SupportedCountries.Contains(countryCode);
		}

		IEnumerable<string> SupportedCountries
		{
			get
			{
				yield return Core.Constants.CountryCodes.Australia;
			}
		}

		BaseCusSCAOceanBill GetExistingCusSCAOceanBill(BusinessObjectFactory factory)
		{
			var applicationCodes = BaseCusSCAOceanBillTypeDecider.GetApplicationCodesForForwarding().Select(x => new ZString(x)).ToArray();
			return new BaseCusSCAOceanBill.Loader(factory).LoadFromConsolAndApplicationCode(consol, applicationCodes);
		}

		public DisposableAction TryCreateOrUpdateCargoAndChildBills(INotifications notifications, out BusinessObject masterBill)
		{
			return TryCreateOrUpdateCusSCAOceanBillAndChildBills(notifications, out masterBill);
		}

		public IBranchProvider GetExistingCargo()
		{
			return GetExistingCusSCAOceanBill(consol.Factory);
		}
	}
}
