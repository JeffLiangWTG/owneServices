using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using JobSailing = Enterprise.Freight.Business.JobSailing;
using JobVoyage = Enterprise.Freight.Business.JobVoyage;

namespace Enterprise.WebCFS.Business
{
	public class SailingFilterBusinessObject : AutoJobSailingFilterBusinessObject
	{
		public SailingFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ShipStatus = "CURRENT";
			JX_DateFilterType = DateFilterTypes.ETD;
			JX_PortFilterType = PortFilterTypes.LoadDischarge;
			JX_CountryFilterType = CountryFilterTypes.LoadDischarge;
			JX_IsPublished = ZBool.True;
			OrgFilter = OrgFilterTypes.None;
			NumberFilter = NumberFilterTypes.None;
			CharterStatus = FreightConstants.CharterFilter.NonCharterOnlyCode;
			JX_IsPublished = ZBool.False;
		}

		#region Filter overrides

		public override ZQuery Filter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));
				query.AddToFilter(base.Filter);
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				ZDBOnlySubQuery subSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
				subSubQuery.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, SQLComparisonOperator.Equal, TransportMode);
				subQuery.AddSubQuery(subSubQuery, JoinCondition.And);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		protected override ZQuery ExcludedSailingsPanelFilter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));
				if (!JX_IsPublished)
				{
					query.AddToFilter(JobSailingSchema.JX_IsPublished, SQLComparisonOperator.NotEqual, JX_IsPublished);
				}
				return query;
			}
		}

		ShippingProviderCollection fShippingProviders;
		public ShippingProviderCollection ShippingProviders
		{
			get
			{
				if (fShippingProviders == null)
				{
					fShippingProviders = new ShippingProviderCollection(Factory);
				}
				return fShippingProviders;
			}
		}

		public bool JX_FromDate_ReadOnly
		{
			get { return JX_DateFilterType == DateFilterTypes.None; }
		}

		public bool JX_ToDate_ReadOnly
		{
			get { return JX_DateFilterType == DateFilterTypes.None; }
		}

		protected virtual ZString TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		#region NumberFilter_List

		public override ZQueryProviderCodeDescriptionListBase NumberFilter_List
		{
			get
			{
				ZQueryProviderCodeDescriptionList result = new ZQueryProviderCodeDescriptionList();

				result.AddEmptySelection();
				result.Add(NumberFilterTypes.ReservedMasterBill, ResString.GetMultilingualString("03a3c884-7980-45c3-9143-afbb3ed021eb", "Reserved Master Bill"), FilterOperator, JobSailingSchema.JX_ReservedMasterBill);
				return result;
			}
		}

		#endregion

		#region VesselPanelFilter

		protected override ZQuery VesselPanelFilter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));

				if (!JX_JV_NKVessel.IsEmpty)
				{
					ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					ZDBOnlySubQuery voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
					voyageQuery.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, SearchComparisonOperator, JX_JV_NKVessel);
					originQuery.AddSubQuery(voyageQuery, JoinCondition.And);
					query.AddSubQuery(originQuery, JoinCondition.And);
				}
				return query;
			}
		}

		#endregion

		#region VoyageNoPanelFilter
		protected override ZQuery VoyageNoPanelFilter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));

				if (!JX_JV_VoyageFlight.IsEmpty)
				{
					ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					ZDBOnlySubQuery voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
					voyageQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, SQLComparisonOperator.Contains, JX_JV_VoyageFlight);
					originQuery.AddSubQuery(voyageQuery, JoinCondition.And);
					query.AddSubQuery(originQuery, JoinCondition.And);
				}
				return query;
			}
		}
		#endregion

		#region SailingDateTimeFilterControlFilter

		protected override ZQuery SailingDateTimeFilterControlFilter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));

				if (JX_DateFilterType == DateFilterTypes.ETD)
				{
					if (JX_FromDate.IsValid)
					{
						ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
						subQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.GreaterThanOrEqualTo, JX_FromDate);
						query.AddSubQuery(subQuery, JoinCondition.And);
					}
					if (JX_ToDate.IsValid)
					{
						ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
						subQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, JX_ToDate);
						query.AddSubQuery(subQuery, JoinCondition.And);
					}
				}
				else if (JX_DateFilterType == DateFilterTypes.ETA)
				{
					if (JX_FromDate.IsValid)
					{
						ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
						subQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.GreaterThanOrEqualTo, JX_FromDate);
						query.AddSubQuery(subQuery, JoinCondition.And);
					}

					if (JX_ToDate.IsValid)
					{
						ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
						subQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, JX_ToDate);
						query.AddSubQuery(subQuery, JoinCondition.And);
					}
				}
				return query;
			}
		}

		#endregion

		#region JX_CountryCodeFilterControlFilter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification alters desired class name")]
		protected override ZQuery JX_CountryCodeFilterControlFilter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));
				if (!JX_RN_Country1.IsEmpty)
				{
					query.AddFilterAndZSQLParameterCollection(JobSailing.Schema.JX_JA + " IN (SELECT " + VoyageOrigin.Schema.PK +
						" FROM " + VoyageOrigin.Schema.TableName + " WHERE " + VoyageOrigin.Schema.JA_RL_NKPortOfLoading + " IN ( " +
						"SELECT " + RefUNLOCO.Schema.RL_Code + " FROM " + RefUNLOCOSchema.Constants.SqlSchemaName + "." + RefUNLOCOSchema.Constants.TableName + " WHERE " + RefUNLOCO.Schema.RL_RN_NKCountryCode + " = @CountryCode1 ) )", new ZSqlParameterCollection(ZSqlParameter.New("@CountryCode1", JX_RN_Country1, RefCountrySchema.RN_Code)));
				}
				if (!JX_RN_Country2.IsEmpty)
				{
					query.AddFilterAndZSQLParameterCollection(JobSailing.Schema.JX_JB + " IN (SELECT " + VoyageDestination.Schema.PK +
						" FROM " + VoyageDestination.Schema.TableName + " WHERE " + VoyageDestination.Schema.JB_RL_NKPortOfDischarge + " IN ( " +
						"SELECT " + RefUNLOCO.Schema.RL_Code + " FROM " + RefUNLOCOSchema.Constants.SqlSchemaName + "." + RefUNLOCOSchema.Constants.TableName + " WHERE " + RefUNLOCO.Schema.RL_RN_NKCountryCode + " = @CountryCode2 ) )", new ZSqlParameterCollection(ZSqlParameter.New("@CountryCode2", JX_RN_Country2, RefCountrySchema.RN_Code)));

					//redo z-query when natural key sub queries are possible.
					//				ZDBOnlySubQuery DestinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), Enterprise.Freight.Business.JobSailing.Schema.JX_JB);
					//				ZDBOnlySubQuery UNLOCOQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), Enterprise.Freight.Business.VoyageDestination.Schema.JB_RL_NKPortOfDischarge);
					//				ZDBOnlySubQuery CountryQuery = new ZDBOnlySubQuery(typeof(RefCountry), Enterprise.MasterFiles.Business.RefUNLOCO.Schema.RL_RN);
					//				CountryQuery.AddToFilter(RefCountry.Schema.RN_Code, JX_RN_Country2);
					//				UNLOCOQuery.AddSubQuery(CountryQuery, JoinCondition.And);
					//				DestinationQuery.AddSubQuery(UNLOCOQuery, JoinCondition.And);
					//				Query.AddSubQuery(DestinationQuery, JoinCondition.And);
				}
				return query;
			}
		}

		#endregion

		#region JX_PortCodeFilterControlFilter

		protected override ZQuery JX_PortCodeFilterControlFilter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));

				if (!JX_RL_NKPort1.IsEmpty)
				{
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					subQuery.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, SearchComparisonOperator, JX_RL_NKPort1);
					query.AddSubQuery(subQuery, JoinCondition.And);
				}
				if (!JX_RL_NKPort2.IsEmpty)
				{
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
					subQuery.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, SearchComparisonOperator, JX_RL_NKPort2);
					query.AddSubQuery(subQuery, JoinCondition.And);
				}
				return query;
			}
		}

		#endregion

		#region Ship Status
		protected override ZQuery ShipStatusPanelFilter
		{
			get
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));

				if (ShipStatus == "CURRENT")
				{
					ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					originQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today);

					ZDBOnlySubQuery destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
					destinationQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today);

					query.AddSubQuery(originQuery, JoinCondition.And);
					query.AddSubQuery(destinationQuery, JoinCondition.Or);
				}
				else if (ShipStatus == "ARRIVED")
				{
					//					ZDBOnlySubQuery SubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), Enterprise.Freight.Business.JobSailing.Schema.JX_JA);
					//					SubQuery.AddToFilter(VoyageDestination.Schema.JB_E_ARV, SQLComparisonOperator.LessThan, ZDateTime.Today);
					//					Query.AddSubQuery(SubQuery, JoinCondition.And);

					ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					originQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.LessThan, ZDateTime.Today);

					ZDBOnlySubQuery destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
					destinationQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.LessThan, ZDateTime.Today);

					query.AddSubQuery(originQuery, JoinCondition.And);
					query.AddSubQuery(destinationQuery, JoinCondition.And);
				}
				return query;
			}
		}
		#endregion

		#region CharterFilter

		protected override ZQuery CharterFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				if (CharterStatus != FreightConstants.CharterFilter.All)
				{
					ZDBOnlySubQuery voyageFilter = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyageSchema.PK);

					switch (CharterStatus)
					{
						case FreightConstants.CharterFilter.CharterOnlyCode:
							voyageFilter.AddToFilter(JobVoyageSchema.JV_IsChartered, true);
							break;

						case FreightConstants.CharterFilter.NonCharterOnlyCode:
							voyageFilter.AddToFilter(JobVoyageSchema.JV_IsChartered, false);
							break;
					}

					ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
					originFilter.AddSubQuery(JobVoyOriginSchema.JA_JV, voyageFilter, JoinCondition.And);

					ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
					sailingFilter.AddSubQuery(JobSailingSchema.JX_JA, originFilter, JoinCondition.And);

					result.AddToFilter(sailingFilter);
				}

				return result;
			}
		}

		#endregion

		protected SQLComparisonOperator SearchComparisonOperator
		{
			get { return SQLComparisonOperator.StartsWith; }
		}

		#endregion

		#region Lists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public CodeDescriptionPairList CharterStatus_List
		{
			get
			{
				if (fCharterStatus_List == null)
				{
					fCharterStatus_List = new CodeDescriptionPairList();
					fCharterStatus_List.AddPair(FreightConstants.CharterFilter.All);
					fCharterStatus_List.AddPair(FreightConstants.CharterFilter.CharterOnlyCode, FreightConstants.CharterFilter.CharterOnlyDesc);
					fCharterStatus_List.AddPair(FreightConstants.CharterFilter.NonCharterOnlyCode, FreightConstants.CharterFilter.NonCharterOnlyDesc);
				}
				return fCharterStatus_List;
			}
		}

		CodeDescriptionPairList fCharterStatus_List;

		#region Ship Status list
		public CodeDescriptionPairList JA_E_DEP_List
		{
			get
			{
				if (fJA_E_DEP_List == null)
				{
					fJA_E_DEP_List = new CodeDescriptionPairList(OLookUpEditType.CustomType);
					fJA_E_DEP_List.AddPair("CURRENT", Res.GetString("a3bd026c-b251-4df5-bfbb-a83dcaa6e1cb", "Current"));
					fJA_E_DEP_List.AddPair("ARRIVED", Res.GetString("bf418e1a-24da-4fa5-9f7b-79eeae884fe7", "Arrived"));
					fJA_E_DEP_List.AddPair("ALL", Res.GetString("6cff60bb-fe8c-4639-943e-f86fa12e8aa9", "All"));
				}
				return fJA_E_DEP_List;
			}
		}

		#endregion

		#region UNLOCO List
		protected RefUNLOCOCollection fUNLOCO_List;
		public RefUNLOCOCollection UNLOCO_List
		{
			get
			{
				if (fUNLOCO_List == null)
				{
					fUNLOCO_List = new RefUNLOCOCollection(Factory);
				}
				return fUNLOCO_List;
			}
		}

		#endregion

		#region Vessel List
		protected RefVesselCollection fVessel_List;
		public RefVesselCollection Vessel_List
		{
			get
			{
				if (fVessel_List == null)
				{
					fVessel_List = new RefVesselCollection(Factory);
				}
				return fVessel_List;
			}
		}

		#endregion

		#region OrgFilter_List

		public override ZQueryProviderCodeDescriptionListBase OrgFilter_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments result = new ZQueryProviderCodeDescriptionListWith2FilterArguments();

				result.AddEmptySelection();
				result.Add(OrgFilterTypes.Line, ResString.GetMultilingualString("f3dcd7d1-3207-49c3-b464-9ad02daa604b", "Carrier"), new AddToQueryDelegate(AddLineToFilter), new AddToQueryDelegate(AddLineToFilter));

				return result;
			}
		}

		#endregion

		#region Country_List

		public RefCountryCollection Country_List
		{
			get { return BindingLists.RefCountry_List; }
		}

		#endregion

		#region CTO_List

		protected OrgHeaderCollection fCTO_List;
		public virtual OrgHeaderCollection CTO_List
		{
			get
			{
				if (fCTO_List == null)
				{
					fCTO_List = new SeaCTOCollection(Factory);
				}
				return fCTO_List;
			}
		}

		#endregion

		#region JX_PortFilterType_List

		public override ZQueryProviderCodeDescriptionListBase JX_PortFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments result = new ZQueryProviderCodeDescriptionListWith2FilterArguments();

				result.AddEmptySelection();
				result.Add(PortFilterTypes.LoadDischarge, ResString.GetMultilingualString("4ff61857-66a7-436f-aba5-a88446d9e7e6", "Load / Discharge"), JobVoyOriginSchema.JA_RL_NKPortOfLoading, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge);
				return result;
			}
		}

		#endregion

		#region JX_CountryFilterType_List

		public override ZQueryProviderCodeDescriptionListBase JX_CountryFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments result = new ZQueryProviderCodeDescriptionListWith2FilterArguments();

				result.AddEmptySelection();
				result.Add(CountryFilterTypes.LoadDischarge, ResString.GetMultilingualString("4ff61857-66a7-436f-aba5-a88446d9e7e6", "Load / Discharge"), RefUNLOCOSchema.RL_Code, RefUNLOCOSchema.RL_Code);
				return result;
			}
		}

		#endregion

		#region JX_DateFilterType_List

		public override ZQueryProviderCodeDescriptionListBase JX_DateFilterType_List
		{
			get
			{
				ZQueryProviderCodeDescriptionListWith2FilterArguments result = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
				result.Add(DateFilterTypes.ETD, ResString.GetMultilingualString("2048e1c0-c239-4b94-9ec5-a905bf131d18", "ETD"), JobVoyOriginSchema.JA_E_DEP, JobVoyOriginSchema.JA_E_DEP);
				result.Add(DateFilterTypes.ETA, ResString.GetMultilingualString("fae2934c-2c9f-4fb6-bed3-6f53be9beca3", "ETA"), JobVoyDestinationSchema.JB_E_ARV, JobVoyDestinationSchema.JB_E_ARV);
				return result;
			}
		}

		#endregion

		#endregion

		#region AddToQueryDelegates

		protected void AddLineToFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZGuid)value).IsEmpty)
			{
				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(JobSailing));

				ZDBOnlySubQuery jobVoySubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				ZDBOnlySubQuery jobVoyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
				jobVoyageSubQuery.AddToFilter(JoinCondition.And, JobVoyageSchema.JV_OH_Line, @operator, value);

				jobVoySubQuery.AddSubQuery(jobVoyageSubQuery, JoinCondition.And);
				dbOnlyResult.AddSubQuery(jobVoySubQuery, JoinCondition.And);

				query.AddToFilter(dbOnlyResult, JoinCondition.And);
			}
		}

		#endregion

		#region Validation
		public override void ValidateJX_ToDate()
		{
			base.ValidateJX_ToDate();
			if (JX_ToDate < JX_FromDate)
			{
				JX_ToDateInfo.AddError(Res.GetString("9a02142e-bbaa-43a6-867e-b63a8210e741", "To date must be after from date."));
			}
		}

		public override void ValidateJX_JV_NKVessel()
		{
			base.ValidateJX_JV_NKVessel();
			ListValidation.ErrorIfInvalidCode(JX_JV_NKVesselInfo, Vessel_List);
		}

		#endregion

		#region Implementation
		protected CodeDescriptionPairList fVoyageType_List;
		protected CodeDescriptionPairList fJA_E_DEP_List;
		#endregion

		#region Filter Types
		#region SuppressResourceStringsCheckRegion

		public static class NumberFilterTypes
		{
			public const string None = "None";
			public const string ReservedMasterBill = "Reserved Master Bill";
		}
		public static class PortFilterTypes
		{
			public const string None = "None";
			public const string LoadDischarge = "Load / Discharge";
		}

		public static class CountryFilterTypes
		{
			public const string None = "None";
			public const string LoadDischarge = "Load / Discharge";
		}

		public static class DateFilterTypes
		{
			public const string None = "None";
			public const string ETD = "ETD";
			public const string ETA = "ETA";
		}

		public static class OrgFilterTypes
		{
			public const string None = "None";
			public const string Line = "Line";
		}

		#endregion
		#endregion
	}
}
