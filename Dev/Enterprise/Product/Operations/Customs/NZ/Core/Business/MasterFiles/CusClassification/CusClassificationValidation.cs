using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	public class CusClassificationValidation : AutoNZCusClassificationValidation
	{
		public CusClassificationValidation(CusClassification parent)
			: base(parent)
		{
		}

		public new CusClassification Parent
		{
			get { return (CusClassification)base.Parent; }
		}

		protected override void CheckCC_TariffNum()
		{
			// Do not want to call base on this one.... Please don't add it back in.
			new TariffValidator(Parent).CheckMainTariff();
		}

		protected override void CheckCC_PartsOfClassification()
		{
			base.CheckCC_PartsOfClassification();

			if (Parent is ITariffValidationData validationData)
			{
				new TariffValidator(validationData).CheckPartsOfTariff();
			}
		}

		protected override void CheckCC_ConcessionCode()
		{
			base.CheckCC_ConcessionCode();
			if (!Parent.CC_ConcessionCode.IsEmpty)
			{
				if (UniversalTariffHelper.UseRefDatabaseData)
				{
					var concessionList = Parent.ConcessionList as ICodeDescriptionPairList;

					if (concessionList != null)
					{
						ListValidation.WarnIfInvalidCode(Parent.CC_ConcessionCodeInfo, concessionList, UniversalTariffHelper.GetInvalidConcessionCodeWarningMessage(Parent.CC_ConcessionCode));
					}
				}
				else
				{
					var dateForDutyRate = ZDateTime.Now;
					var concessionCodeWarning = IsConcessionCodeValid(Parent.CC_ConcessionCode, dateForDutyRate);
					if (!concessionCodeWarning.IsEmpty)
					{
						Parent.CC_ConcessionCodeInfo.AddWarning(concessionCodeWarning.Replace("~", Parent.CC_ConcessionCode) + ConcessionCodeWarningMessage);
					}
				}
			}
		}

		ZString IsConcessionCodeValid(ZString concessionCode, ZDateTime validDate)
		{
			var result = ZString.Empty;
			var concessionFilter = new ZQuery(NZCConcessionSchema.U2_Code, concessionCode);
			var concessions = new NonDependentNZCConcessionCollection(Parent.Factory);
			concessions.Load(concessionFilter);
			if (concessions.Count == 0)
			{
				result = ConcessionCodeWarningNotRecognisedMessage;
			}

			foreach (NZCConcession concession in concessions)
			{
				if (concession.U2_DateActiveFrom > validDate)
				{
					result = ConcessionCodeWarningNotActiveMessage;
					break;
				}
				else if (concession.U2_DateActiveTo.IsValid && concession.U2_DateActiveTo < validDate)
				{
					result = ConcessionCodeErrorExpired;
					break;
				}
			}
			return result;
		}

		public const string ConcessionCodeWarningMessage = " - This either means you're using an invalid concession code, the concession code is unpublished, or your Tariff Data is out of date.";
		const string ConcessionCodeWarningNotRecognisedMessage = "Concession Code [~] not recognised";
		const string ConcessionCodeWarningNotActiveMessage = "Concession Code [~] not active yet";
		const string ConcessionCodeErrorExpired = "Concession Code [~] has expired";
	}
}
