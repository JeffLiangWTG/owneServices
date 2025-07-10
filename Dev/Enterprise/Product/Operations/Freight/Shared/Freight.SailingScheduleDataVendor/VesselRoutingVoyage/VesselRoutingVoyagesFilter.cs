using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Freight.SailingScheduleDataVendor.Res;
using ResString = Enterprise.Freight.SailingScheduleDataVendor.ResString;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class VesselRoutingVoyagesFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public VesselRoutingVoyagesFilter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constants

		public static class Schema
		{
			public const string TableName = "VesselRoutingPortPairFilterBusinessObject";
			public const string PK = "PK";

			public const string E9_DateFrom = "E9_DateFrom";
			public const string E9_DateTo = "E9_DateTo";
			public const string E9_DateType = "E9_DateType";

			public const string E9_Carrier = "E9_Carrier";
			public const string E9_LloydsNumber = "E9_LloydsNumber";

			public const string E9_Port = "E9_Port";

			public const string E9_RV_NKVesselName = "E9_RV_NKVesselName";
			public const string E9_Voyage = "E9_Voyage";
			public const string E9_DataProvider = "E9_DataProvider";
		}

		internal static class Constants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
			public static class Dates
			{
				public const string All = "All";
				public const string EstimatedDeparture = "Estimated Departure";
				public const string EstimatedArrival = "Estimated Arrival";
				public const string ActualDeparture = "Actual Departure";
				public const string ActualArrival = "Actual Arrival";
			}
		}

		#endregion

		#region Search Voyages

		public void PerformSearch()
		{
			Voyages.PortPairTypeFilter = PortPairTypes;

			Voyages.SwapFactoryAndRemoveAll(new BusinessObjectFactory());
			Voyages.Load(Filter);

			foreach (VesselRoutingVoyage voyage in Voyages)
			{
				voyage.Validation.ValidateE8_LloydsNumber();
			}
		}

		public VesselRoutingVoyageCollection Voyages
		{
			get
			{
				if (fVoyages == null)
				{
					fVoyages = new VesselRoutingVoyageCollection(Factory);
				}

				return fVoyages;
			}
		}
		VesselRoutingVoyageCollection fVoyages;

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			E9_DateType = Constants.Dates.All;

			E9_Port = "";
			E9_DateFrom = ZDateTime.Empty;
			E9_DateTo = ZDateTime.Empty;
			E9_Voyage = "";
			E9_RV_NKVesselName = "";
			E9_LloydsNumber = "";
			E9_Carrier = "";
			E9_DataProvider = "";
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode.Equals(Enterprise.Core.Constants.CountryCodes.Germany))
			{
				E9_DataProvider = FreightConstants.VesselDataProviders.DAKOSY;
			}
		}

		public void ResetToDefaultValues()
		{
			SetDefaultValues();
			RefreshBinding();
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateE9_DateType();
			ValidateE9_DateFrom();
			ValidateE9_DateTo();
			ValidateE9_Port();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Properties

		#region E9_DateType

		[List("DateTypeList")]
		[MaxLength(20)]
		public ZString E9_DateType
		{
			get { return fE9_DateType; }
			set
			{
				CheckMaximumLength(E9_DateTypeInfo, value);
				fE9_DateType = value;
				if (!IsValidationSuspended)
				{
					ValidateE9_DateType();
				}
				E9_DateTypeInfo.RefreshBinding();
			}
		}

		ZString fE9_DateType;

		public ZPropertyInfo E9_DateTypeInfo
		{
			get { return GetZPropertyInfo(Schema.E9_DateType); }
		}

		void ValidateE9_DateType()
		{
			E9_DateTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(E9_DateTypeInfo, DateTypeList);
		}

		#endregion

		#region E9_DateFrom

		public ZDateTime E9_DateFrom
		{
			get { return fE9_DateFrom; }
			set
			{
				fE9_DateFrom = value.Date;
				if (!IsValidationSuspended)
				{
					ValidateE9_DateFrom();
				}
				E9_DateFromInfo.RefreshBinding();
			}
		}

		ZDateTime fE9_DateFrom;

		public ZPropertyInfo E9_DateFromInfo
		{
			get { return GetZPropertyInfo(Schema.E9_DateFrom); }
		}

		void ValidateE9_DateFrom()
		{
			E9_DateFromInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(E9_DateFromInfo);

			if (!E9_DateFrom.IsValid || E9_DateFrom < ZDateTime.Now.AddMonths(-6).AddDays(-7))
			{
				E9_DateFromInfo.AddError(Res.GetString("c8371f2c-edbb-496e-9fad-83a3c1c3db40", "You must enter a from date greater than 6 months ago."));
			}
		}

		#endregion

		#region E9_DateTo

		public ZDateTime E9_DateTo
		{
			get { return fE9_DateTo; }
			set
			{
				fE9_DateTo = value.Date;
				if (!IsValidationSuspended)
				{
					ValidateE9_DateTo();
				}
				E9_DateToInfo.RefreshBinding();
			}
		}

		ZDateTime fE9_DateTo;

		void ValidateE9_DateTo()
		{
			E9_DateToInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(E9_DateToInfo);
			if (E9_DateFrom.IsValid && E9_DateTo < E9_DateFrom)
			{
				E9_DateToInfo.AddError(Res.GetString("20224201-66b0-44df-80bb-87148ab7ae06", "The 'from' date cannot be greater than the 'to' date."));
			}
		}

		public ZPropertyInfo E9_DateToInfo
		{
			get { return GetZPropertyInfo(Schema.E9_DateTo); }
		}

		#endregion

		#region E9_Port

		[List("PortList")]
		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public ZString E9_Port
		{
			get { return fE9_Port; }
			set
			{
				CheckMaximumLength(E9_PortInfo, value);
				fE9_Port = value;
				if (!IsValidationSuspended)
				{
					ValidateE9_Port();
				}
				E9_PortInfo.RefreshBinding();
			}
		}

		ZString fE9_Port;

		public ZPropertyInfo E9_PortInfo
		{
			get { return GetZPropertyInfo(Schema.E9_Port); }
		}

		void ValidateE9_Port()
		{
			E9_PortInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(E9_PortInfo, PortList);
		}

		#endregion

		#region E9_RV_NKVesselName

		[List("VesselList")]
		[MaxLength(RefVessel.Schema.RV_CodeMaxLength)]
		public ZString E9_RV_NKVesselName
		{
			get { return fE9_RV_NKVesselName; }
			set
			{
				CheckMaximumLength(E9_RV_NKVesselNameInfo, value);
				if (value != E9_RV_NKVesselName)
				{
					fE9_RV_NKVesselName = value;
					DefaultLloydsNumberFromVesselName();
				}
				E9_RV_NKVesselNameInfo.RefreshBinding();
			}
		}

		ZString fE9_RV_NKVesselName;

		public ZPropertyInfo E9_RV_NKVesselNameInfo
		{
			get { return GetZPropertyInfo(Schema.E9_RV_NKVesselName); }
		}

		void DefaultLloydsNumberFromVesselName()
		{
			if (!isApplyingVesselDefault)
			{
				isApplyingVesselDefault = true;
				try
				{
					var vessel = RefVessel.LookupVesselByName(E9_RV_NKVesselName, Factory).FirstOrDefault();
					E9_LloydsNumber = (vessel == null) ? ZString.Empty : vessel.RV_LloydsNumber;
				}
				finally
				{
					isApplyingVesselDefault = false;
				}
			}
		}

		bool isApplyingVesselDefault;

		#endregion

		#region E9_LloydsNumber

		[MaxLength(RefVessel.Schema.RV_LloydsNumberMaxLength)]
		public ZString E9_LloydsNumber
		{
			get { return fE9_LloydsNumber; }
			set
			{
				CheckMaximumLength(E9_LloydsNumberInfo, value);
				if (value != E9_LloydsNumber)
				{
					fE9_LloydsNumber = value;
					DefaultVesselNameFromLloydsNumber();
				}
				E9_LloydsNumberInfo.RefreshBinding();
			}
		}

		ZString fE9_LloydsNumber;

		public ZPropertyInfo E9_LloydsNumberInfo
		{
			get { return GetZPropertyInfo(Schema.E9_LloydsNumber); }
		}

		void DefaultVesselNameFromLloydsNumber()
		{
			if (!isApplyingVesselDefault)
			{
				isApplyingVesselDefault = true;
				try
				{
					if (!E9_LloydsNumber.IsEmpty)
					{
						RefVessel vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, E9_LloydsNumber));
						E9_RV_NKVesselName = (vessel == null) ? ZString.Empty : vessel.RV_Name;
					}
				}
				finally
				{
					isApplyingVesselDefault = false;
				}
			}
		}

		#endregion

		#region E9_Carrier

		[MaxLength(35)]
		public ZString E9_Carrier
		{
			get { return fE9_Carrier; }
			set
			{
				CheckMaximumLength(E9_CarrierInfo, value);
				fE9_Carrier = value;
				E9_CarrierInfo.RefreshBinding();
			}
		}

		ZString fE9_Carrier;

		public ZPropertyInfo E9_CarrierInfo
		{
			get { return GetZPropertyInfo(Schema.E9_Carrier); }
		}

		#endregion

		#region E9_Voyage

		[MaxLength(100)]
		public ZString E9_Voyage
		{
			get { return fE9_Voyage; }
			set
			{
				CheckMaximumLength(E9_VoyageInfo, value);
				fE9_Voyage = value;
				E9_VoyageInfo.RefreshBinding();
			}
		}

		ZString fE9_Voyage;

		public ZPropertyInfo E9_VoyageInfo
		{
			get { return GetZPropertyInfo(Schema.E9_Voyage); }
		}

		#endregion

		#region E9_DataProvider

		[List("DataSourceList")]
		[MaxLength(3)]
		public ZString E9_DataProvider
		{
			get { return fE9_DataProvider; }
			set
			{
				CheckMaximumLength(E9_DataProviderInfo, value);
				fE9_DataProvider = value;
				if (!IsValidationSuspended)
				{
					ValidateE9_DataProvider();
				}
				E9_DataProviderInfo.RefreshBinding();
			}
		}

		ZString fE9_DataProvider;

		public ZPropertyInfo E9_DataProviderInfo
		{
			get { return GetZPropertyInfo(Schema.E9_DataProvider); }
		}

		void ValidateE9_DataProvider()
		{
			E9_DataProviderInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(E9_DataProviderInfo, DataSourceList);
		}

		#endregion

		#endregion

		#region Filter

		public ZQuery Filter
		{
			get
			{
				var subQuery = new ZDBOnlySubQuery(typeof(VesselRoutingVoyage), ViewVesselRoutingVoyagesSchema.E8_NaturalKey);

				AddDatesFilter(subQuery);
				AddPortPairTypeFilter(subQuery);
				AddVesselNameAndLloydsFilter(subQuery);
				AddVoyageFilter(subQuery);
				AddDataProviderFilter(subQuery);
				AddLineOperatorFilter(subQuery);

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(VesselRoutingVoyage));
				result.AddSubQuery(ViewVesselRoutingVoyagesSchema.E8_NaturalKey, subQuery, JoinCondition.And);

				return result;
			}
		}

		#region Dates

		void AddDatesFilter(ZDBOnlyQuery query)
		{
			ZQuery subQuery = new ZQuery();

			if (E9_DateType == Constants.Dates.All)
			{
				if (!E9_DateFrom.IsEmpty)
				{
					ZQuery fromDateQuery = new ZQuery();
					fromDateQuery.AddToFilter(JoinCondition.Or, JobVesselScheduleSchema.EV_ETD, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, E9_DateFrom);
					fromDateQuery.AddToFilter(JoinCondition.Or, JobVesselScheduleSchema.EV_ETA, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, E9_DateFrom);
					fromDateQuery.AddToFilter(JoinCondition.Or, JobVesselScheduleSchema.EV_ActualDeparture, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, E9_DateFrom);
					fromDateQuery.AddToFilter(JoinCondition.Or, JobVesselScheduleSchema.EV_ActualArrival, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, E9_DateFrom);
					subQuery.AddToFilter(fromDateQuery);
				}

				if (!E9_DateTo.IsEmpty)
				{
					ZQuery toDateQuery = new ZQuery();
					toDateQuery.AddToFilter(JoinCondition.Or, JobVesselScheduleSchema.EV_ETD, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, E9_DateTo);
					toDateQuery.AddToFilter(JoinCondition.Or, JobVesselScheduleSchema.EV_ETA, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, E9_DateTo);
					toDateQuery.AddToFilter(JoinCondition.Or, JobVesselScheduleSchema.EV_ActualDeparture, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, E9_DateTo);
					toDateQuery.AddToFilter(JoinCondition.Or, JobVesselScheduleSchema.EV_ActualArrival, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, E9_DateTo);
					subQuery.AddToFilter(toDateQuery);
				}
			}
			else if (E9_DateType == Constants.Dates.EstimatedDeparture)
			{
				if (!E9_DateFrom.IsEmpty)
				{
					subQuery.AddToFilter(JobVesselScheduleSchema.EV_ETD, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, E9_DateFrom);
				}

				if (!E9_DateTo.IsEmpty)
				{
					subQuery.AddToFilter(JobVesselScheduleSchema.EV_ETD, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, E9_DateTo);
				}
			}
			else if (E9_DateType == Constants.Dates.EstimatedArrival)
			{
				if (!E9_DateFrom.IsEmpty)
				{
					subQuery.AddToFilter(JobVesselScheduleSchema.EV_ETA, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, E9_DateFrom);
				}

				if (!E9_DateTo.IsEmpty)
				{
					subQuery.AddToFilter(JobVesselScheduleSchema.EV_ETA, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, E9_DateTo);
				}
			}
			else if (E9_DateType == Constants.Dates.ActualDeparture)
			{
				if (!E9_DateFrom.IsEmpty)
				{
					subQuery.AddToFilter(JobVesselScheduleSchema.EV_ActualDeparture, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, E9_DateFrom);
				}

				if (!E9_DateTo.IsEmpty)
				{
					subQuery.AddToFilter(JobVesselScheduleSchema.EV_ActualDeparture, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, E9_DateTo);
				}
			}
			else if (E9_DateType == Constants.Dates.ActualArrival)
			{
				if (!E9_DateFrom.IsEmpty)
				{
					subQuery.AddToFilter(JobVesselScheduleSchema.EV_ActualArrival, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, E9_DateFrom);
				}

				if (!E9_DateTo.IsEmpty)
				{
					subQuery.AddToFilter(JobVesselScheduleSchema.EV_ActualArrival, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, E9_DateTo);
				}
			}

			if (!subQuery.IsEmpty)
			{
				AddVesselRoutingPortSubQuery(query, subQuery);
			}
		}

		#endregion

		#region Vessel Name / Lloyds Number

		void AddVesselNameAndLloydsFilter(ZDBOnlyQuery query)
		{
			ZQuery vesselNameAndLloydsQuery = new ZQuery();
			vesselNameAndLloydsQuery.DefaultJoinCondition = JoinCondition.Or;

			if (!E9_RV_NKVesselName.IsEmpty)
			{
				vesselNameAndLloydsQuery.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_VesselName, SQLComparisonOperator.StartsWith, E9_RV_NKVesselName);
			}

			if (!E9_LloydsNumber.IsEmpty)
			{
				vesselNameAndLloydsQuery.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_LloydsNumber, E9_LloydsNumber);
			}

			query.AddToFilter(vesselNameAndLloydsQuery, JoinCondition.And);
		}

		#endregion

		#region Voyage

		void AddVoyageFilter(ZQuery query)
		{
			if (!E9_Voyage.IsEmpty)
			{
				query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_Voyage, E9_Voyage);
			}
		}

		#endregion

		#region Data Provider

		void AddDataProviderFilter(ZQuery query)
		{
			if (!E9_DataProvider.IsEmpty)
			{
				query.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_DataProvider, E9_DataProvider);
			}
		}

		#endregion

		#region Line Operator

		void AddLineOperatorFilter(ZDBOnlyQuery query)
		{
			if (!E9_Carrier.IsEmpty)
			{
				ZQuery lineOperatorQuery = new ZQuery();
				lineOperatorQuery.DefaultJoinCondition = JoinCondition.Or;

				if (E9_Carrier.Length <= ViewVesselRoutingVoyagesSchema.E8_LineOperator.MaxLength)
				{
					lineOperatorQuery.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_LineOperator, SQLComparisonOperator.StartsWith, E9_Carrier);
				}

				lineOperatorQuery.AddToFilter(ViewVesselRoutingVoyagesSchema.E8_OperatorsDescription, SQLComparisonOperator.StartsWith, E9_Carrier);

				query.AddToFilter(lineOperatorQuery, JoinCondition.And);
			}
		}

		#endregion

		#region Port Pair Types

		#region IsImport

		public ZBool IsImport
		{
			get { return IsPortPairTypeInFilter(PortPairTypes.Import); }
		}

		#endregion

		#region IsExport

		public ZBool IsExport
		{
			get { return IsPortPairTypeInFilter(PortPairTypes.Export); }
		}

		#endregion

		public bool IsPortPairTypeInFilter(PortPairTypes portPairType)
		{
			return (PortPairTypes & portPairType) == portPairType;
		}

		void AddPortPairTypeFilter(ZDBOnlyQuery query)
		{
			if (PortPairTypes != PortPairTypes.All)
			{
				var portQuery = new ZDBOnlyQuery(typeof(VesselRoutingVoyage));

				if (IsImport)
				{
					portQuery.AddFilterAndZSQLParameterCollection("E8_VoyageIn = E8_Voyage", new ZSqlParameterCollection(), JoinCondition.Or); // Doesnt exist in Enterprise.ZArchitecture.Schema columns
				}

				if (IsExport)
				{
					portQuery.AddFilterAndZSQLParameterCollection("E8_VoyageOut = E8_Voyage", new ZSqlParameterCollection(), JoinCondition.Or); // Doesnt exist in Enterprise.ZArchitecture.Schema columns
				}

				query.AddToFilter(portQuery);
			}
		}

		#endregion

		void AddVesselRoutingPortSubQuery(ZQuery query, ZQuery vesselRoutingPortDatesQuery)
		{
			var routingQuery = new ZDBOnlyQuery(typeof(VesselRoutingVoyage));

			var voyageSubQuery = new ZDBOnlyQuery(typeof(VesselRoutingVoyage));

			var voyageInSubQuery = new ZDBOnlySubQuery(typeof(JobVesselSchedule), JobVesselScheduleSchema.EV_ShipOperatorVoyageIn);
			voyageInSubQuery.AddToFilter(vesselRoutingPortDatesQuery);
			AddPortQuery(voyageInSubQuery);

			var voyageOutSubQuery = new ZDBOnlySubQuery(typeof(JobVesselSchedule), JobVesselScheduleSchema.EV_ShipOperatorVoyageOut);
			voyageOutSubQuery.AddToFilter(vesselRoutingPortDatesQuery);
			AddPortQuery(voyageOutSubQuery);

			if (PortPairTypes == PortPairTypes.All)
			{
				voyageInSubQuery.AddAsUnionQuery(voyageOutSubQuery, true);
				voyageSubQuery.AddSubQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, voyageInSubQuery, JoinCondition.Or);
			}
			else
			{
				if (IsImport)
				{
					voyageSubQuery.AddSubQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, voyageInSubQuery, JoinCondition.And);
				}
				else if (IsExport)
				{
					voyageSubQuery.AddSubQuery(ViewVesselRoutingVoyagesSchema.E8_Voyage, voyageOutSubQuery, JoinCondition.And);
				}
			}

			routingQuery.AddToFilter(voyageSubQuery, JoinCondition.And);
			query.AddToFilter(routingQuery);
		}

		void AddPortQuery(ZQuery query)
		{
			if (!E9_Port.IsEmpty)
			{
				query.AddToFilter(JobVesselScheduleSchema.EV_RL_NKPortCode, E9_Port);
			}
		}

		#endregion

		#region Lookups

		public PortPairTypes PortPairTypes
		{
			get
			{
				PortPairTypes result = PortPairTypes.All;
				if (!E9_DateType.IsEmpty)
				{
					switch (E9_DateType)
					{
						case Constants.Dates.ActualArrival:
						case Constants.Dates.EstimatedArrival:
							return PortPairTypes.Import;
						case Constants.Dates.ActualDeparture:
						case Constants.Dates.EstimatedDeparture:
							return PortPairTypes.Export;
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList DataSourceList
		{
			get { return Factory.GetCachedValue("VesselRoutingVoyagesFilter.DataSourceList", SailingScheduleHelper.GetDataSourceList); }
		}

		public CodeDescriptionPairList DateTypeList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(ResString.GetMultilingualString("A09E6E63-06C2-43F9-8868-049BBD3A13C3", Constants.Dates.All));
				list.AddPair(ResString.GetMultilingualString("4D7B4AF1-26C1-4D63-82DC-8229321ECB87", Constants.Dates.EstimatedDeparture));
				list.AddPair(ResString.GetMultilingualString("973F2C48-B466-4551-86CD-955048A07837", Constants.Dates.EstimatedArrival));
				list.AddPair(ResString.GetMultilingualString("B3811080-221D-40FF-824C-4AC653F8D40C", Constants.Dates.ActualDeparture));
				list.AddPair(ResString.GetMultilingualString("D7AC5B26-1656-4AEC-916E-AF161F3CE1B7", Constants.Dates.ActualArrival));
				return list;
			}
		}

		public LocationCollection PortList
		{
			get
			{
				if (portList == null)
				{
					portList = new LocationCollection(Factory);
				}
				return portList;
			}
		}
		LocationCollection portList;

		public RefVesselCollection VesselList
		{
			get
			{
				if (vesselList == null)
				{
					vesselList = new RefVesselCollection(Factory);
				}
				return vesselList;
			}
		}
		RefVesselCollection vesselList;

		#endregion
	}
}
