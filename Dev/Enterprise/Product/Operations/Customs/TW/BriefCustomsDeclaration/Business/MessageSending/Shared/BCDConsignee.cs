using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class BCDConsignee : BCDPartyShared
	{
		public BCDConsignee(AsycudaBill bill)
		{
			Bill = Argument.NotNull(bill, nameof(bill));
		}

		AsycudaBill Bill { get; }

		protected override ZString GetIDCore() => Bill.ABL_ConsigneeRegNo;

		protected override ZString GetNameCore() => Bill.ABL_ConsigneeName;

		protected override ZString GetChineseNameCore() => Bill.ABL_ConsigneeLocalName;

		protected override ZString RegNoType => Bill.ABL_ConsigneeRegNoType;

		protected override ZString GetCustomsControlIDCore() => ZString.Empty;

		protected override ZString GetLineCore() => GetLine(Bill.ABL_ConsigneePostcode, Bill.ConsigneeCountry, Bill.ABL_ConsigneeState, Bill.ABL_ConsigneeCity, Bill.ABL_ConsigneeStreet1, Bill.ABL_ConsigneeStreet2);

		protected override ZString GetChineseLineCore() => GetChineseLine(Bill.ABL_ConsigneePostcode, Bill.ConsigneeCountry, Bill.ABL_ConsigneeLocalState, Bill.ABL_ConsigneeLocalCity, Bill.ABL_ConsigneeLocalStreet1, Bill.ABL_ConsigneeLocalStreet2);

		protected override ZString GetCountryCodeCore() => Bill.ABL_RN_NKConsigneeCountry;
	}
}
