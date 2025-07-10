using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Enterprise.Registry.Business;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class MainDbUpdaterRegistration : IUpdaterRegistration
	{
		public MainDbUpdaterRegistration()
		{
		}

		public IDataSetUpdater[] Get(IServerProxy proxy, IDBHelper dbHelper)
		{
			var versionControlManager = new RefVersionControlManager(dbHelper);
			versionControlManager.IsMainDb = true;

			return new IDataSetUpdater[] {
					new OneTableDataSetUpdater<UNDGCommonData, IUNDGCommonData>(proxy, dbHelper, versionControlManager),
					new UNDGSubstanceUpdater(proxy, dbHelper, versionControlManager),
					new RefCountryStatesUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter()),
					new RefTimeZoneSetUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter()),
					new RefUNLOCOUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter(), SystemDataRegistry.Instance.TimeZoneOffsetCachePeriod.Value),
					new RefCurrencyUpdater(proxy, dbHelper, versionControlManager),
					new RefCountryUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter()),
					new RefShippingLineUpdater(proxy, dbHelper, versionControlManager),
					new RefComplianceListUpdater(proxy, dbHelper, versionControlManager),
					new RefAirlineUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter()),
					new TwoTableDataSetUpdater<UNDGCountryReference, UNDGCountryReferencePivot, IUNDGCountryReference, IUNDGCountryReferencePivot>(proxy, dbHelper, versionControlManager),
					new OneTableDataSetUpdater<RefShippingLineMessagingRequirementType, IRefShippingLineMessagingRequirementType>(proxy, dbHelper, versionControlManager),
					new TwoTableDataSetUpdater<RefFacility, RefFacilityLocalCode, IRefFacility, IRefFacilityLocalCode>(proxy, dbHelper, versionControlManager, 2),
					new OneTableDataSetUpdater<RefMaterial, IRefMaterial>(proxy, dbHelper, versionControlManager),
					new OneTableDataSetUpdater<RefDamage, IRefDamage>(proxy, dbHelper, versionControlManager),
					new OneTableDataSetUpdater<RefMRComponentCode, IRefMRComponentCode>(proxy, dbHelper, versionControlManager),
					new OneTableDataSetUpdater<RefRepairCode, IRefRepairCode>(proxy, dbHelper, versionControlManager),
					new OneTableDataSetUpdater<RefUnitSection, IRefUnitSection>(proxy, dbHelper, versionControlManager),
					new OneTableDataSetUpdater<RefEquipmentGrade, IRefEquipmentGrade>(proxy, dbHelper, versionControlManager),
					new OneTableDataSetUpdater<RefComplianceCommodityAlert, IRefComplianceCommodityAlert>(proxy, dbHelper, versionControlManager, false, true)
			};
		}
	}
}
