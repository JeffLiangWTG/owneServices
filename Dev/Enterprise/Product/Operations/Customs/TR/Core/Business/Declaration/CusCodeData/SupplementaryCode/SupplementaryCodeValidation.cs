using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class SupplementaryCodeValidation : EU.Business.SupplementaryCodeValidation
	{
		public SupplementaryCodeValidation(BaseSupplementaryCode parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			var code = Parent.CY_Code;
			var targetInfo = Parent.CY_CodeInfo;
			MandatoryValidation.CheckEntered(targetInfo);

			var invoiceLine = Parent.Parent as JobComInvoiceLine;
			if (invoiceLine == null || invoiceLine.Declaration == null)
			{ return; }

			string shipmentType = invoiceLine.Declaration.JE_MessageType;
			var supporter = Parent.SupplementaryCodeSupporter;

			if (supporter != null)
			{
				if (shipmentType == MessageTypeList.Codes.Import && supporter.SupplementaryCodes.Count() > 5)
				{
					targetInfo.AddMessageError(IMPCodesCount(targetInfo));
				}

				if (shipmentType == MessageTypeList.Codes.Export && supporter.SupplementaryCodes.Count() > 3)
				{
					targetInfo.AddMessageError(EXPCodesCount(targetInfo));
				}

				if (supporter.SupplementaryCodes.OfType<SupplementaryCode>().Any(x => x.PK != Parent.PK && x.CY_Code == code))
				{
					targetInfo.AddMessageError(DuplicateSupplementaryCode(targetInfo));
				}
			}
		}

		public static string IMPCodesCount(ZPropertyInfo info) => Res.GetString("1161B7C6-857A-46CF-AFAF-3796AD245E20", "Up to 5 {0} can be selected for IMP entry type. Do not add more", info.HumanReadableName);

		public static string EXPCodesCount(ZPropertyInfo info) => Res.GetString("751D03F5-85F7-4D77-9F0C-E66E548F1331", "Up to 3 {0} can be selected for EXP entry type. Do not add more", info.HumanReadableName);
	}
}
