using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbDeptCharges : AutoGlbDeptCharges
	{
		public GlbDeptCharges(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GD_GC = GlbCompany.CurrentCompany.PK;
		}

		#endregion

		#region Properties

		#region GD_GE

		public override ZGuid GD_GE
		{
			get { return base.GD_GE; }
			set
			{
				if (base.GD_GE != value)
				{
					base.GD_GE = value;
					if (Department != null)
					{
						GD_SequenceNumber = GetHighestSeqNum();
					}
				}
			}
		}

		byte GetHighestSeqNum()
		{
			byte max = 0;
			for (byte i = 1; i <= Department.DeptCharges.Count; i++)
			{
				if (Department.DeptCharges[i - 1].GD_SequenceNumber > max)
				{
					max = Department.DeptCharges[i - 1].GD_SequenceNumber;
				}
			}
			return ++max;
		}

		#endregion

		#region GD_AC

		[List("AccChargeCodes")]
		public override ZGuid GD_AC
		{
			get { return base.GD_AC; }
			set { base.GD_AC = value; }
		}

		#endregion

		#region GD_SequenceNumber

		[BusinessObjectTestExclude]
		public override ZByte GD_SequenceNumber
		{
			get { return base.GD_SequenceNumber; }
			set
			{
				base.GD_SequenceNumber = value;
			}
		}

		//#if DEBUG
		//		internal
		//#endif

		#endregion

		#region Charge Code Description

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString AccChargeCodeDescription
		{
			get { return ChargeCode != null ? ChargeCode.AC_Desc : ZString.Empty; }
		}

		public ZPropertyInfo AccChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(AccChargeCodeDescription)); }
		}

		#endregion

		#endregion

		#region Lookups

		public AccChargeCodeCollection AccChargeCodes
		{
			get
			{
				if (fAccChargeCodes == null)
				{
					ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, new string[] { Core.Constants.ChargeType.Margin, Core.Constants.ChargeType.Revenue, Core.Constants.ChargeType.Disbursement, Core.Constants.ChargeType.ManualJobAccrual });

					GlbDepartment dept = Factory.Load(typeof(GlbDepartment), GD_GE) as GlbDepartment;
					ZQuery deptListFilter = new ZQuery();
					if (dept != null)
					{
						deptListFilter = new ZQuery(AccChargeCodeSchema.AC_DepartmentFilterList, SQLComparisonOperator.Contains, dept.GE_Code);
					}
					deptListFilter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_DepartmentFilterList, SQLComparisonOperator.Equal, "ALL");

					filter.AddToFilter(deptListFilter, JoinCondition.And);

					fAccChargeCodes = new AccChargeCodeCollection(Factory, filter);
				}
				return fAccChargeCodes;
			}
		}

		AccChargeCodeCollection fAccChargeCodes;

		#endregion

	}
}
