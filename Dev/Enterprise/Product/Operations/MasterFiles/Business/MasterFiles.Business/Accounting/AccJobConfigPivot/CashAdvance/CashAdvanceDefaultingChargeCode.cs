using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CashAdvanceDefaultingChargeCode : CashAdvanceDefaultingJobConfigPivot
	{
		public CashAdvanceDefaultingChargeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JCT_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("0f345f2c-79cb-4715-ad8c-b6ef552e8295", "Advance Payment Defaulting Charge Code");

		#endregion

		#region Properties

		#region JCT_ParentId

		[ResourceStringData("CashAdvanceDefaultingChargeCode|JCT_Code", Caption = "Charge Code", ShortCaption = "Code")]
		[List(nameof(ChargeCodes))]
		public override ZGuid JCT_ParentId
		{
			get => base.JCT_ParentId;
			set => base.JCT_ParentId = value;
		}

		public AccChargeCodeCollection ChargeCodes
		{
			get { return chargeCodes ?? (chargeCodes = new AccChargeCodeCollection(Factory, GlbCompany.CurrentCompany)); }
		}
		AccChargeCodeCollection chargeCodes;

		#endregion

		#region ChargeCodeDescription

		AccChargeCode ChargeCode => Factory.Load<AccChargeCode>(JCT_ParentId);

		[ResourceStringData("CashAdvanceDefaultingChargeCode|ChargeCodeDescription", Caption = "Charge Code Description", ShortCaption = "Description")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Baseline")]
		public ZString ChargeCodeDescription => ChargeCode?.AC_Desc ?? ZString.Empty;

		#endregion

		#endregion

		#region Implementation

		public bool IsDuplicateOf(CashAdvanceDefaultingChargeCode other) => PK != other.PK && JCT_ParentId == other.JCT_ParentId;

		#endregion
	}
}
