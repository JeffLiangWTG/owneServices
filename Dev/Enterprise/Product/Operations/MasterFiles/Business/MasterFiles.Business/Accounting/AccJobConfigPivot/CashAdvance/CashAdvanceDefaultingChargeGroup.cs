using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class CashAdvanceDefaultingChargeGroup : CashAdvanceDefaultingJobConfigPivot
	{
		public CashAdvanceDefaultingChargeGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Validation

		protected override AccJobConfigPivotValidation GetNewValidation() => new CashAdvanceDefaultingChargeGroupValidation(this);

		#endregion

		#region Properties

		#region JCT_Code

		[ResourceStringData("CashAdvanceDefaultingChargeGroup|JCT_Code", Caption = "Charge Group Code", ShortCaption = "Code")]
		[MaxLength(3)]
		[List(nameof(ChargeGroups))]
		public override ZString JCT_Code
		{
			get => base.JCT_Code;
			set => base.JCT_Code = value;
		}

		public CodeDescriptionPairList ChargeGroups
		{
			get { return chargeGroups ?? (chargeGroups = new ChargeCodeGroupList()); }
		}
		CodeDescriptionPairList chargeGroups;

		#endregion

		#region ChargeGroupDescription

		[ResourceStringData("CashAdvanceDefaultingChargeGroup|ChargeGroupDescription", Caption = "Charge Group Description", ShortCaption = "Description")]
		public ZString ChargeGroupDescription => JCT_Code.IsEmpty ? ZString.Empty : (ZString)ChargeGroups.GetDescriptionFromCode(JCT_Code);

		#endregion

		#endregion

		#region Implementation

		public override bool ReadOnly
		{
			get => JobConfiguration?.ReadOnly ?? base.ReadOnly;
			set => base.ReadOnly = value;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("903DB9E9-DE82-4149-B167-FEE05EA7453F", "Advance Payment Defaulting Charge Group");

		public bool IsDuplicateOf(CashAdvanceDefaultingChargeGroup other) => PK != other.PK && JCT_Code == other.JCT_Code;

		#endregion
	}
}
