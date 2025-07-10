using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport
{
	/// <summary>
	/// This class is for Outward Report Status & Entry Number.
	/// </summary>
	public class CusEntryNumber : Common.CusEntryNumber
	{
		public new class Schema : Common.CusEntryNumber.Schema
		{
			public const string CE_ConsolID = "CE_ConsolID";
			public const string CE_MasterBillNumber = "CE_MasterBillNumber";
			public const string CE_StatusDescription = "CE_StatusDescription";
		}

		public CusEntryNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static ZQuery GetEntryNumberFilter(CommonConsol consol)
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, consol.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypeList.Codes.OutwardReportNumber);
			return filter;
		}

		public static ZQuery GetEntryNumberFilter(ForwardingShipment shipment)
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, shipment.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			ZQuery subFilter = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypeList.Codes.ECIWriteOff);
			subFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypeList.Codes.FormalEntry);
			subFilter.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypeList.Codes.OutwardReportNumber);

			filter.AddToFilter(subFilter, JoinCondition.And);
			return filter;
		}

		#region New Properties

		public ZString CE_ConsolID
		{
			get { return Consol != null ? Consol.JK_UniqueConsignRef : ZString.Empty; }
		}

		public ZPropertyInfo CE_ConsolIDInfo
		{
			get { return GetZPropertyInfo(Schema.CE_ConsolID); }
		}

		public ZString CE_MasterBillNumber
		{
			get { return Consol != null ? Consol.JK_MasterBillNum : ZString.Empty; }
		}

		public ZPropertyInfo CE_MasterBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CE_MasterBillNumber); }
		}

		public ZString CE_StatusDescription
		{
			get { return StatusList.GetDescriptionFromCode(CE_EntryStatus); }
		}

		public ZPropertyInfo CE_StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CE_StatusDescription); }
		}

		#endregion

		#region Related BOs

		ForwardingConsol fConsol;
		public ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null || fConsol.PK != CE_ParentID)
				{
					fConsol = (ForwardingConsol)Factory.Load(typeof(ForwardingConsol), CE_ParentID);
				}
				return fConsol == null || fConsol.IsDeleted ? null : fConsol;
			}
		}

		OutwardReportStatusList fStatusList;
		public OutwardReportStatusList StatusList
		{
			get
			{
				if (fStatusList == null)
				{
					fStatusList = new OutwardReportStatusList();
				}
				return fStatusList;
			}
		}

		#endregion
	}
}
