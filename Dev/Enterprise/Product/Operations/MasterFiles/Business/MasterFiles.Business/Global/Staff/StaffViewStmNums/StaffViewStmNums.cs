using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class StaffViewStmNums : ViewStmNums
	{
		public StaffViewStmNums(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new class Schema : ViewStmNums.Schema
		{
			public const string SN_NamePrefix = "StaffOwned_";
			public new const int SN_PrefixMaxLength = 22;
			public new const int SN_TypeMaxLength = 3;

			public const long DefaultPATMaximumValue = 999999;
		}

		public GlbStaff Staff => Factory.Load<GlbStaff>(SN_Owner);

		public override BusinessObject Owner => Staff;

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SN_Name = Schema.SN_NamePrefix;
			SN_Value = SN_MinimumValue;
		}

		protected override ZLong DefaultTypeRangeMaxCore => SN_Type == OrgConstants.NumberFountains.Code.PatentNumber ? Schema.DefaultPATMaximumValue : Schema.DefaultFountainMaximumValue;

		protected override ZString NamePrefix => Schema.SN_NamePrefix;

		protected override int SN_Prefix_MaxLength => Schema.SN_PrefixMaxLength;

		protected override int SN_Type_MaxLength => Schema.SN_TypeMaxLength;

		protected override FormattedNumberFountainFactory GetNumberFountainFactory(BusinessObject ownerBizObj)
		{
			var fountainFactory = new FormattedNumberFountainFactory(SN_Name, ownerBizObj.PK.ToGuid(), SN_Prefix, SN_CanRollover, (long)SN_MinimumValue, (long)SN_MaximumValue, CalculateRequiredDigit((long)SN_MaximumValue));
			return fountainFactory;
		}

		int CalculateRequiredDigit(long value) => value.ToString(Culture.Invariant).Length;

		public new StaffViewStmNumsLookups Lookups => (StaffViewStmNumsLookups)base.Lookups;

		protected override ViewStmNumsLookups GetNewLookups() => new StaffViewStmNumsLookups(this);
	}
}
