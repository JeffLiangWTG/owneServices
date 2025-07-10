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
	public class AutoContainerAvailabilityFilterBusinessObject : FilterBusinessObject
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "vw_List_ContainerAvailability";
			public const string PK = "PK";

			public const string LCV_ContainerNum = "LCV_ContainerNum";
			public const string LCV_Vessel = "LCV_Vessel";
			public const string LCV_VoyageFlight = "LCV_VoyageFlight";
			public const string FromDate = "FromDate";
			public const string ToDate = "ToDate";
			public const string ContainerDateType = "ContainerDateType";
		}

		#endregion

		public AutoContainerAvailabilityFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region PK

		public override string PK_ColumnName
		{
			get { return Schema.PK; }
		}

		#endregion

		#region Properties

		#region LCV_ContainerNum

		[MaxLength(Autovw_List_ContainerAvailability.Schema.LCV_ContainerNumMaxLength)]
		public virtual ZString LCV_ContainerNum
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(LCV_ContainerNumInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				SetPropertyValue(LCV_ContainerNumInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateLCV_ContainerNum();
				}
			}
		}

		public virtual void ValidateLCV_ContainerNum()
		{
			LCV_ContainerNumInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo LCV_ContainerNumInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LCV_ContainerNum); }
		}

		#endregion

		#region LCV_Vessel

		[MaxLength(Autovw_List_ContainerAvailability.Schema.LCV_VesselMaxLength)]
		public virtual ZString LCV_Vessel
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(LCV_VesselInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				SetPropertyValue(LCV_VesselInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateLCV_Vessel();
				}
			}
		}

		public virtual void ValidateLCV_Vessel()
		{
			LCV_VesselInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo LCV_VesselInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LCV_Vessel); }
		}

		#endregion

		#region LCV_VoyageFlight

		[MaxLength(Autovw_List_ContainerAvailability.Schema.LCV_VoyageFlightMaxLength)]
		public virtual ZString LCV_VoyageFlight
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(LCV_VoyageFlightInfo)); }
			set
			{
				value = value.TrimEndSpaceTab();
				SetPropertyValue(LCV_VoyageFlightInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateLCV_VoyageFlight();
				}
			}
		}

		public virtual void ValidateLCV_VoyageFlight()
		{
			LCV_VoyageFlightInfo.ClearAllNotifications();
		}

		public virtual ZPropertyInfo LCV_VoyageFlightInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LCV_VoyageFlight); }
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

		protected ContainerDateList fDateList;
		public ContainerDateList DateList
		{
			get
			{
				if (fDateList == null)
				{
					fDateList = new ContainerDateList();
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

				AddIfNotEmpty(query, vw_List_ContainerAvailabilitySchema.LCV_ContainerNum, SQLComparisonOperator.Contains, LCV_ContainerNum);
				AddIfNotEmpty(query, vw_List_ContainerAvailabilitySchema.LCV_Vessel, SQLComparisonOperator.Contains, LCV_Vessel);
				AddIfNotEmpty(query, vw_List_ContainerAvailabilitySchema.LCV_VoyageFlight, SQLComparisonOperator.Contains, LCV_VoyageFlight);
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
					case ContainerDateList.Codes.Availability:
						return vw_List_ContainerAvailabilitySchema.LCV_AvailabilityDate;
					case ContainerDateList.Codes.Arrival:
						return vw_List_ContainerAvailabilitySchema.LCV_EstimatedArrivalDate;
					case ContainerDateList.Codes.Storage:
						return vw_List_ContainerAvailabilitySchema.LCV_StorageDate;
					default:
						return null;
				}
			}
		}

		#endregion

		protected override void SetDefaultValues()
		{
			((IBusinessObjectInternals)this).Row[Schema.LCV_ContainerNum] = "";
			((IBusinessObjectInternals)this).Row[Schema.LCV_Vessel] = "";
			((IBusinessObjectInternals)this).Row[Schema.LCV_VoyageFlight] = "";
			((IBusinessObjectInternals)this).Row[Schema.FromDate] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.ToDate] = DBNull.Value;
			((IBusinessObjectInternals)this).Row[Schema.ContainerDateType] = "";
		}
	}
}
