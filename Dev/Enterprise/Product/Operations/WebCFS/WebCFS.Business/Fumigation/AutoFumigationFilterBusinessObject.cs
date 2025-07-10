using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WebCFS.Business
{
	/// <summary>
	/// Not automatically generated, but instead created to look like an Auto one, because ZRachitecture enforces 
	/// all filter business objects to be inherited from an auto one
	/// </summary>
	public class AutoFumigationFilterBusinessObject : FilterBusinessObject
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "vw_List_Fumigation";
			public const string PK = "PK";

			public const string LFV_ContainerNum = "LFV_ContainerNum";
			public const string LFV_Vessel = "LFV_Vessel";
			public const string LFV_VoyageFlight = "LFV_VoyageFlight";
			public const string FromDate = "FromDate";
			public const string ToDate = "ToDate";
			public const string ContainerDateType = "ContainerDateType";
		}

		#endregion

		public AutoFumigationFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region PK

		public override string PK_ColumnName
		{
			get { return Schema.PK; }
		}

		#endregion

		#region Properties

		#region LFV_ContainerNum

		[MaxLength(Autovw_List_Fumigation.Schema.LFV_ContainerNumMaxLength)]
		public virtual ZString LFV_ContainerNum
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(LFV_ContainerNumInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();

				SetPropertyValue(LFV_ContainerNumInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateLFV_ContainerNum();
				}
			}
		}

		public virtual void ValidateLFV_ContainerNum()
		{
			LFV_ContainerNumInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo LFV_ContainerNumInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LFV_ContainerNum); }
		}

		#endregion

		#region LFV_Vessel

		[MaxLength(Autovw_List_Fumigation.Schema.LFV_VesselMaxLength)]
		public virtual ZString LFV_Vessel
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(LFV_VesselInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				SetPropertyValue(LFV_VesselInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateLFV_Vessel();
				}
			}
		}

		public virtual void ValidateLFV_Vessel()
		{
			LFV_VesselInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo LFV_VesselInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LFV_Vessel); }
		}

		#endregion

		#region LFV_VoyageFlight

		[MaxLength(Autovw_List_Fumigation.Schema.LFV_VoyageFlightMaxLength)]
		public virtual ZString LFV_VoyageFlight
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(LFV_VoyageFlightInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				SetPropertyValue(LFV_VoyageFlightInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateLFV_VoyageFlight();
				}
			}
		}

		public virtual void ValidateLFV_VoyageFlight()
		{
			LFV_VoyageFlightInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo LFV_VoyageFlightInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LFV_VoyageFlight); }
		}

		#endregion

		#region FromDate

		public virtual ZDateTime FromDate
		{
			get { return new ZDateTime(((IBusinessObjectInternals)this).GetValueFromRowSafely(FromDateInfo)); }
			set
			{
				SetPropertyValue(FromDateInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateFromDate();
				}
			}
		}

		public virtual void ValidateFromDate()
		{
			FromDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(FromDateInfo);
		}

		public virtual ZPropertyInfo FromDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.FromDate); }
		}

		#endregion

		#region ToDate

		public virtual ZDateTime ToDate
		{
			get { return new ZDateTime(((IBusinessObjectInternals)this).GetValueFromRowSafely(ToDateInfo)); }
			set
			{
				SetPropertyValue(ToDateInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateToDate();
				}
			}
		}

		public virtual void ValidateToDate()
		{
			ToDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ToDateInfo);
		}

		public virtual ZPropertyInfo ToDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ToDate); }
		}

		#endregion

		#region Date list

		protected FumigationDateList fDateList;
		public FumigationDateList DateList
		{
			get
			{
				if (fDateList == null)
				{
					fDateList = new FumigationDateList();
				}

				return fDateList;
			}
		}

		#endregion

		#region ContainerDateType

		public virtual ZString ContainerDateType
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(ContainerDateTypeInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				CheckMaximumLength(ContainerDateTypeInfo, value);
				SetPropertyValue(ContainerDateTypeInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateContainerDateType();
				}
			}
		}

		public virtual void ValidateContainerDateType()
		{
			ContainerDateTypeInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo ContainerDateTypeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ContainerDateType); }
		}

		#endregion

		#endregion

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				AddIfNotEmpty(query, vw_List_FumigationSchema.LFV_ContainerNum, SQLComparisonOperator.Contains, LFV_ContainerNum);
				AddIfNotEmpty(query, vw_List_FumigationSchema.LFV_Vessel, SQLComparisonOperator.Contains, LFV_Vessel);
				AddIfNotEmpty(query, vw_List_FumigationSchema.LFV_VoyageFlight, SQLComparisonOperator.Contains, LFV_VoyageFlight);
				query.AddToFilter(DateSearchFilterControlFilter);

				return query;
			}
		}

		protected virtual ZQuery DateSearchFilterControlFilter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				if (DateSchema != null)
				{
					if (ToDate.IsValid)
					{
						query.AddToFilter(JoinCondition.And, DateSchema, SQLComparisonOperator.LessThanOrEqualTo, ToDate);
					}
					if (FromDate.IsValid)
					{
						query.AddToFilter(JoinCondition.And, DateSchema, SQLComparisonOperator.GreaterThanOrEqualTo, FromDate);
					}
				}
				return query;
			}
		}

		protected SchemaColumn DateSchema
		{
			get
			{
				switch (ContainerDateType)
				{
					case FumigationDateList.Codes.Arrival:
						return vw_List_FumigationSchema.LFV_EstimatedArrivalDate;
					case FumigationDateList.Codes.Booked:
						return vw_List_FumigationSchema.LFV_FumigationBooked;
					case FumigationDateList.Codes.Completed:
						return vw_List_FumigationSchema.LFV_FumigationCompleted;
					default:
						return null;
				}
			}
		}

		#endregion

		protected override void SetDefaultValues()
		{
			((IBusinessObjectInternals)this).Row[Schema.LFV_ContainerNum] = "";
			((IBusinessObjectInternals)this).Row[Schema.LFV_Vessel] = "";
			((IBusinessObjectInternals)this).Row[Schema.LFV_VoyageFlight] = "";
			((IBusinessObjectInternals)this).Row[Schema.FromDate] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.ToDate] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.ContainerDateType] = "";
		}
	}
}
