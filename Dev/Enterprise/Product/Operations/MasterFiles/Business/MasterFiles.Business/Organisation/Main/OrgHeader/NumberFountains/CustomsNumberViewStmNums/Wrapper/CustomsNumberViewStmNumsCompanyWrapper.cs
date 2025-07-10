using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsCompanyWrapper : CustomsNumberViewStmNumsWrapper
	{
		public CustomsNumberViewStmNumsCompanyWrapper(CustomsNumberViewStmNums stmNums) : base(stmNums)
		{
			isBranchLevel = stmNums.SN_Owner.IsValid && !(stmNums.Owner is GlbCompany);
		}

		public new class Schema : CustomsNumberViewStmNumsWrapper.Schema
		{
			public const string IsBranchLevel = "IsBranchLevel";
		}

		[ReadOnly(true)]
		[ResourceStringData("CustomsNumberViewStmNumsCompanyWrapper|IsBranchLevel", Caption = "Is Branch Level", ShortCaption = "Branch")]
		public ZBool IsBranchLevel
		{
			get => isBranchLevel;
			set => SetNonPersistentPropertyValue(IsBranchLevelInfo, ref isBranchLevel, value);
		}
		ZBool isBranchLevel;

		public ZPropertyInfo IsBranchLevelInfo => GetZPropertyInfo(Schema.IsBranchLevel);
	}
}
