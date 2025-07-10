using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCustomsNumberViewStmNumsWrapper : CustomsNumberViewStmNumsCompanyWrapper
	{
		public SGCustomsNumberViewStmNumsWrapper(CustomsNumberViewStmNums stmNums)
			: base(stmNums)
		{
		}

		public new SGCustomsNumberViewStmNumsWrapperLookups Lookups => (SGCustomsNumberViewStmNumsWrapperLookups)base.Lookups;

		protected override CustomsNumberViewStmNumsWrapperLookups GetNewLookups()
		{
			return new SGCustomsNumberViewStmNumsWrapperLookups(this);
		}

		public new SGCustomsNumberViewStmNumsWrapperValidation Validation => (SGCustomsNumberViewStmNumsWrapperValidation)base.Validation;

		public override CustomsNumberViewStmNumsWrapperValidation GetNewValidation()
		{
			return new SGCustomsNumberViewStmNumsWrapperValidation(this);
		}

		[ReadOnlyMember(nameof(SN_FountainNameReadOnly))]
		[ResourceStringData("SGCustomsNumberViewStmNumsWrapper|SN_FountainName", Caption = "Customs Registration Number")]
		public override ZString SN_FountainName
		{
			get => base.SN_FountainName;
			set => base.SN_FountainName = value;
		}

		public bool SN_FountainNameReadOnly => true;

		protected override ZString HumanReadableNameCore => Res.GetString("59c14051-b5e5-4551-81ea-d056f29b6831", "Number Range");

		public static CustomsNumberViewStmNums GetSingaporeMessageNumber(GlbCompany company)
		{
			return company?.CustomsNumberProvider?.CustomsNumbers.OrderBy(x => x.SN_SystemCreateTimeUtc).FirstOrDefault(x =>
				x.SN_Type == NumberRangeTypeList.Codes.SingaporeMessageNumber &&
				x.SN_Prefix.StartsWith(company.GC_CustomsRegistrationNo + CustomsNumberViewStmNums.Schema.SequenceSeparator, StringComparison.OrdinalIgnoreCase));
		}
	}
}
