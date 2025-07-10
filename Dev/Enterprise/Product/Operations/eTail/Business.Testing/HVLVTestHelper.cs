using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.Business.Testing
{
	public static class HVLVTestHelper
	{
		public static IDisposable SuspendTrigger(string triggerName, string tableName)
		{
			var connection = Db.Connection;
			return new DisposableAction(
				() => connection.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName}"),
				() => connection.ExecuteNonQuery($"ENABLE TRIGGER {triggerName} ON {tableName}"));
		}

		public static ForwardingShipment GetNewShipment(BusinessObjectFactory factory, string shipmentNo, string shipmentType)
		{
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = shipmentNo;
			shipment.JS_ShipmentType = shipmentType;

			return shipment;
		}

		public static ForwardingShipment CreateShipmentWithHLREvent(BusinessObjectFactory factory, string shipmentType, string transportMode, string reason = "Cargo Reporting")
		{
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_MasterBillNum = "12345678";

			var shipment = factory.NewWithValidTestData<HVLVForwardingShipment>();
			consol.Shipments.Add(shipment);
			shipment.JS_UniqueConsignRef = "SHIP123";
			shipment.JS_HouseBill = "HBL123";
			shipment.JS_ShipmentType = shipmentType;
			shipment.JS_TransportMode = transportMode;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1231231";
			shipment.OuterPackLines.AddNew().SetContainer(container.PK);

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			var item = consignment.Items.AddNew();
			item.HVI_ItemId = "ITEM1";
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var reasonParameter = reason != null ? $"{shipment.JS_HouseBill}|RES={reason}" : string.Empty;

			shipment.Logs.AddNew(AutoEvents.HVLVReady, reasonParameter);

			factory.Save();

			return shipment;
		}

		public static void SetGS1FountainOnOrgProxy(BusinessObjectFactory factory, string prefix)
		{
			var orgProxy = factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			SetGS1FountainOnOrg(orgProxy, prefix);
		}

		public static void SetGS1FountainOnOrg(OrgHeader org, string prefix)
		{
			var orgCustomCode = org.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(cusCode => cusCode.OK_CodeType == OrgCusCode.CodeTypes.GS1);
			if (orgCustomCode == null)
			{
				orgCustomCode = org.CustomsCodes.AddNew();
				orgCustomCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			}

			orgCustomCode.OK_CustomsRegNo = prefix;

			var orgStmNumSSCC = org.OrgFountains.TryGetStmNums(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, prefix);
			if (orgStmNumSSCC == null)
			{
				orgStmNumSSCC = org.OrgFountains.AddNew();
				orgStmNumSSCC.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
				orgStmNumSSCC.SN_Prefix = prefix;
			}

			org.Factory.Save();
			org.OrgFountains.RefreshFromDb();
		}

		public static IDisposable AssertStatusUpdatedEvent<T>(T enterpriseBusinessObject, string oldStatus, string newStatus, bool reloadFromDB = false)
			where T : EnterpriseBusinessObject
		{
			return new StatusUpdatedEventAssertion<T>(enterpriseBusinessObject, oldStatus, newStatus, reloadFromDB);
		}

		public static void CreateDummyConsignments<T>(DependentBusinessObjectCollection<HVLVConsignment, T> collection, int countConsignments) where T : BusinessObject
		{
			for (var i = 0; i < countConsignments; i++)
			{
				var consignment = collection.AddNew();

				consignment.HVC_WaybillNumber = i.ToString();
				consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
				consignment.HVC_WeightUQ = Weight.Kilograms;

				var item = consignment.Items.AddNew();
				item.HVI_ManifestedVolume = 1;
				item.HVI_ActualVolume = 1;
				item.HVI_ManifestedWeight = 1;
				item.HVI_ActualWeight = 1;
			}
		}

		public static RefZoneHeader SetRefZoneHeader(ZGuid ownerPK, BusinessObjectFactory factory)
		{
			var zoneHeader = factory.New<RefZoneHeader>();
			zoneHeader.FZ_Code = "TEST";
			zoneHeader.FZ_Description = "Sumink to copy like";
			zoneHeader.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway;

			var losAngeles = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var sydney = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var rome = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "ITROM");

			zoneHeader.UNLOCOs.Add(losAngeles);
			zoneHeader.UNLOCOs.Add(sydney);
			zoneHeader.UNLOCOs.Add(rome);
			zoneHeader.FZ_OH_RelatedParty = ownerPK;

			return zoneHeader;
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, string currencyCode, decimal sellRate)
		{
			var usdCurrency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
			var exchangeRate = usdCurrency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.SellRate;
			exchangeRate.RE_StartDate = ZDateTime.Now.AddMonths(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddMonths(1);
			exchangeRate.RE_SellRate = sellRate;
		}

		public static int GetRecordCountFromHXUTable()
		{
			return Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.HVLVUsage");
		}

		public static string GetUserFromHXUTable()
		{
			return Db.Connection.ExecuteScalar<string>("SELECT HXU_GS_NKUser FROM dbo.HVLVUsage");
		}

		public static string GetUsageCodeFromHXUTable()
		{
			return Db.Connection.ExecuteScalar<string>("SELECT HXU_Code from dbo.HVLVUsage");
		}

		public static bool ExistsUsageCodeWithItemPK(string pk)
		{
			return Db.Connection.ExecuteScalar<int>($"IF EXISTS(SELECT null from dbo.HVLVUsage WHERE HXU_HVI_ParentItem = '{pk}') SELECT 1 ELSE SELECT 0") == 1;
		}

		class StatusUpdatedEventAssertion<T> : IDisposable where T : EnterpriseBusinessObject
		{
			public StatusUpdatedEventAssertion(T enterpriseBusinessObject, string oldStatus, string newStatus, bool reloadFromDB)
			{
				this.enterpriseBusinessObject = enterpriseBusinessObject;
				this.oldStatus = oldStatus;
				this.newStatus = newStatus;
				this.reloadFromDB = reloadFromDB;

				filter = new ZQuery(StmALogSchema.SL_Parent, enterpriseBusinessObject.PK);
				filter.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusUpdated.Code);
				logsAtTheBeginning = enterpriseBusinessObject.Logs.Find(filter);
			}

			public void Dispose()
			{
				if (reloadFromDB)
				{
					enterpriseBusinessObject = new BusinessObjectFactory().Load<T>(enterpriseBusinessObject.PK);
				}

				var logsAtTheEnd = enterpriseBusinessObject.Logs.Find(filter);
				var newLogs = logsAtTheEnd.Except(logsAtTheBeginning, Comparer<StmALog>.Create((logA, logB) => logA.PK == logB.PK ? 0 : -1));
				NUnit.Framework.Assertion.AssertNotNull(newLogs.Single(log =>
					log.Parameters[EventReferenceParameters.Codes.New] == newStatus &&
					log.Parameters[EventReferenceParameters.Codes.Old] == oldStatus));
			}

			T enterpriseBusinessObject;
			readonly IEnumerable<StmALog> logsAtTheBeginning;
			readonly string oldStatus;
			readonly string newStatus;
			readonly ZQuery filter;
			readonly bool reloadFromDB;
		}
	}

	public class PerformanceStatisticsCollectorForTest : IPerformanceStatisticsCollector
	{
		EnabledState IPerformanceStatisticsCollector.StatisticMode => EnabledState.Simple;

		void IPerformanceStatisticsCollector.AttemptFlush(bool flush)
		{
		}

		IDisposable IPerformanceStatisticsCollector.Exclude()
		{
			return DisposableAction.NoAction;
		}

		IDisposable IPerformanceStatisticsCollector.StartMonitoring(string statisticName, string subName)
		{
			var stat = statisticName + '_' + subName;

			CollectedStats.Add(stat);

			return null;
		}

		bool IPerformanceStatisticsCollector.IsMonitoring => false;

		public List<string> CollectedStats { get; } = new List<string>();
	}
}
