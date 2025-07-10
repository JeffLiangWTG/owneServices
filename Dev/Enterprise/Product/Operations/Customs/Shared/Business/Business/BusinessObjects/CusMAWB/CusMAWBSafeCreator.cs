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
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Creates CusMAWB for a Consol and CusHAWBs for its Shipments using mutex locks.
	/// </summary>
	public class CusMAWBSafeCreator : ICargoSafeCreator
	{
		public CusMAWBSafeCreator(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
		}
		readonly ForwardingConsol consol;

		DisposableAction TryCreateOrUpdateCusMAWBAndChildBills(INotifications notifications, out BusinessObject outMawb)
		{
			outMawb = null;
			var result = DisposableAction.NoAction;

			if (IsSupported(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				mutexForConsol = CusMAWB.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				if (mutexForConsol.Lock())
				{
					outMawb = CreateOrUpdateCusMAWB(notifications);
				}
				else
				{
					notifications?.AddError(Res.GetString("8880DAF1-9FD9-4C62-BFB1-31B0EB31BCAC", "A Customs master bill record cannot be created as someone else is trying to create a master bill for this consol {0}.", consol.HumanReadableName));
				}

				result = new DisposableAction(DisposeMutexes);
			}

			return result;
		}
		TriLockMutex mutexForConsol;

		CusMAWB CreateOrUpdateCusMAWB(INotifications notifications)
		{
			CusMAWB result = null;

			var useConsolFactory = !consol.IsInDatabase;
			var factory = useConsolFactory ? consol.Factory : new BusinessObjectFactory();

			var mawb = GetExistingCusMAWB(factory);
			if (mawb == null)
			{
				mawb = (CusMAWB)factory.New(CusMAWBTypeDecider.GetTypeForForwarding());
				mawb.CM_JK = consol.PK;
				mawb.DefaultFromConsol();
			}

			var existingHouseBills = mawb.ChildBills.Cast<CusHAWB>().ToArray();
			var shipmentsWithoutHAWB = consol.Shipments.Cast<CommonShipment>()
										.Where(s => !existingHouseBills.Any(hawb => hawb.CS_JS == s.PK))
										.Select(s => s.PK);

			foreach (var shipmentPK in shipmentsWithoutHAWB)
			{
				var hawb = GetExistingCusHAWB(factory, shipmentPK, mawb.CM_ApplicationCode);
				if (hawb != null)
				{
					mawb.ChildBills.Add(hawb);
				}
				else
				{
					var hawbMutex = GetMutexForShipment(shipmentPK);
					if (hawbMutex.Lock())
					{
						var houseBill = mawb.ChildBills.AddNew();
						houseBill.CS_JS = shipmentPK;
						houseBill.DefaultFromShipment();
					}
					else
					{
						notifications?.AddError(Res.GetString("4F534133-BA56-42DF-B812-14CB67F0C1C8", "A Customs house bill record for one of shipments cannot be created as someone else is trying to create a house bill for this shipment (consol : {0}).", consol.HumanReadableName));
					}
				}
			}

			if (useConsolFactory)
			{
				result = mawb;
			}
			else
			{
				try
				{
					factory.Saving += OnTemporaryFactorySaving;
					factory.Save();
				}
				catch (Exception ex)
				{
					notifications?.AddError(Res.GetString("6B061458-3D8B-4CC7-B5B6-73CEF4B334AE", "An error occurred when creating a new Customs master bill for this consol {0}.\r\n{1}", consol.HumanReadableName, ex.Message));
				}
				finally
				{
					factory.Saving -= OnTemporaryFactorySaving;
					result = GetExistingCusMAWB(consol.Factory);
				}
			}

			return result;
		}

		protected virtual void OnTemporaryFactorySaving(BusinessObjectFactory tempFactory)
		{
		}

		public bool IsSupported(string countryCode)
		{
			return consol.IsAir && SupportedCountries.Contains(countryCode);
		}

		static IEnumerable<string> SupportedCountries
		{
			get
			{
				yield return Core.Constants.CountryCodes.Australia;
			}
		}

		void DisposeMutexes()
		{
			DisposeMutex(mutexForConsol);
			mutexForConsol = null;
			shipmentsMutexes.Values.ForEach(DisposeMutex);
			shipmentsMutexes.Clear();
		}

		void DisposeMutex(IDisposable mutex) => mutex?.Dispose();

		CusMAWB GetExistingCusMAWB(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<CusMAWB>(MawbFilter);
		}

		ZQuery MawbFilter
		{
			get
			{
				if (mawbFilter == null)
				{
					var applicationCodes = CusMAWBTypeDecider.GetApplicationCodesForForwarding().Select(x => new ZString(x)).ToArray();
					mawbFilter = ForwardingConsol.GetMAWBsFilter(consol.PK, false, applicationCodes);
				}
				return mawbFilter;
			}
		}
		ZQuery mawbFilter;

		CusHAWB GetExistingCusHAWB(BusinessObjectFactory factory, ZGuid shipmentPK, string applicationCode)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			query.AddToFilter(CusHAWBSchema.CS_JS, shipmentPK);
			var hawbSubQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.PK);
			hawbSubQuery.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_ApplicationCode, applicationCode);
			var mawbSubQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			mawbSubQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, applicationCode);
			hawbSubQuery.AddSubQuery(CusHAWBSchema.CS_CM, mawbSubQuery, JoinCondition.Or);
			query.AddSubQuery(CusHAWBSchema.PK, hawbSubQuery, JoinCondition.And);
			return factory.LoadTop1<CusHAWB>(query);
		}

		ZGlobalMutex GetMutexForShipment(ZGuid shipmentPK)
		{
			if (!shipmentsMutexes.ContainsKey(shipmentPK))
			{
				shipmentsMutexes.Add(shipmentPK, CusHAWB.CreateMutexForShipment(shipmentPK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			}
			return shipmentsMutexes[shipmentPK];
		}
		readonly Dictionary<ZGuid, ZGlobalMutex> shipmentsMutexes = new Dictionary<ZGuid, ZGlobalMutex>();

		public DisposableAction TryCreateOrUpdateCargoAndChildBills()
		{
			return TryCreateOrUpdateCargoAndChildBills(null, out _);
		}

		public DisposableAction TryCreateOrUpdateCargoAndChildBills(INotifications notifications, out BusinessObject masterBill)
		{
			return TryCreateOrUpdateCusMAWBAndChildBills(notifications, out masterBill);
		}

		public IBranchProvider GetExistingCargo()
		{
			return GetExistingCusMAWB(consol.Factory);
		}
	}
}
