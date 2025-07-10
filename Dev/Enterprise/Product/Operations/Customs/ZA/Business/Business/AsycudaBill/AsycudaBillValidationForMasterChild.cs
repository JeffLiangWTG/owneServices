using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaBillValidationForMasterChild : AsycudaBillValidation
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent)
			: base(parent)
		{
		}

		protected override void CheckABL_BillIssueDate()
		{
			base.CheckABL_BillIssueDate();

			if (HeaderNotNullWithType)
			{
				if (header.IsCOSTCO)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillIssueDateInfo);
				}
				else if (header.IsGOVGIO)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ABL_BillIssueDateInfo);
				}
			}
		}

		protected override void CheckABL_E_ARV()
		{
			base.CheckABL_E_ARV();

			if (HeaderNotNullWithType)
			{
				var dischargePortCountryCode = Parent.PortOfDischarge?.Country.Code ?? ZString.Empty;
				if (header.IsTGO || header.IsDGO || (header.IsCOSTCO && dischargePortCountryCode == CountryCodes.SouthAfrica))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_E_ARVInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ABL_E_ARVInfo);
				}

				var departureDate = Parent.ABL_E_DEP;
				var arrivalDate = Parent.ABL_E_ARV;
				if (!departureDate.IsEmpty && !arrivalDate.IsEmpty && arrivalDate < departureDate)
				{
					Parent.ABL_E_ARVInfo.AddMessageError(Res.GetString("4C71E2BB-5EFB-4DE2-AFAE-833260450E81", "{0} should not be less than {1}.", Parent.ABL_E_ARVInfo.HumanReadableName, Parent.ABL_E_DEPInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckABL_E_DEP()
		{
			base.CheckABL_E_DEP();

			if (HeaderNotNullWithType)
			{
				if (header.IsTGO || header.IsDGO)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ABL_E_DEPInfo);
				}
				else if ((header.IsCOSTCO || header.IsDGI))
				{
					var loadPortCountryCode = Parent.PortOfLoading?.Country.Code ?? ZString.Empty;
					if (loadPortCountryCode == CountryCodes.SouthAfrica)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_E_DEPInfo);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.ABL_E_DEPInfo);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_E_DEPInfo);
				}

				var departureDate = Parent.ABL_E_DEP;
				var arrivalDate = Parent.ABL_E_ARV;
				if (!departureDate.IsEmpty && !arrivalDate.IsEmpty && departureDate > arrivalDate)
				{
					Parent.ABL_E_DEPInfo.AddMessageError(Res.GetString("96DAA573-B8DD-44AC-9683-CA31328AC4BA", "{0} should not be greater than {1}.", Parent.ABL_E_DEPInfo.HumanReadableName, Parent.ABL_E_ARVInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();

			if (HeaderNotNullWithType)
			{
				if (header.IsDOR || header.IsBBB || header.IsAOR || header.IsALD || header.IsDCI || header.IsBGI)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillNumberInfo);
				}
				else if (header.IsGOVGIO)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ABL_BillNumberInfo);
				}
			}
		}

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			base.CheckABL_RL_NKPortOfDischarge();

			if (HeaderNotNullWithType)
			{
				if (header.IsGOVGIO)
				{
					if (header.IsTGO || header.IsDGI)
					{
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RL_NKPortOfDischargeInfo);
						CheckValidPort(Parent.ABL_RL_NKPortOfDischargeInfo, false);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.ABL_RL_NKPortOfDischargeInfo);
					}
				}
				else
				{
					var nature = header.AMA_Nature.ToString();
					if (nature.In(NatureList.Codes.Import23, NatureList.Codes.Transit24, NatureList.Codes.Transhipment28))
					{
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RL_NKPortOfDischargeInfo);
						CheckValidPort(Parent.ABL_RL_NKPortOfDischargeInfo, true);
					}
				}
			}
		}

		protected override void CheckABL_RL_NKPortOfLoading()
		{
			base.CheckABL_RL_NKPortOfLoading();

			if (HeaderNotNullWithType && (header.IsGOVGIO || header.AMA_Nature == NatureList.Codes.Export22))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RL_NKPortOfLoadingInfo);
				CheckValidPort(Parent.ABL_RL_NKPortOfLoadingInfo, false);
			}
		}

		void CheckValidPort(ZPropertyInfo portInfo, bool checkZA)
		{
			var portCode = (ZString)portInfo.Value;
			if (portCode.IsEmpty)
			{
				return;
			}

			if (portCode.Length != 5)
			{
				portInfo.AddMessageError(Res.GetString("676B22EB-446F-452E-A228-D92424885135", "Port Code must be 5 characters."));
			}
			else
			{
				var unlocoCode = (ZString)portInfo.Value;
				var unloco = new RefUNLOCO.Loader(Parent.Factory).Load(unlocoCode);

				if (checkZA)
				{
					CheckZAPort(portInfo, unloco);
				}

				CheckIATA(portInfo, unloco);
			}
		}

		void CheckIATA(ZPropertyInfo portInfo, RefUNLOCO unloco)
		{
			if (unloco == null)
			{
				return;
			}

			var iata = unloco.RL_IATA;
			if (iata.IsEmpty)
			{
				portInfo.AddMessageError(Res.GetString("5473A9CF-C4B2-40E4-A0D5-423D09034BB0", "{0} IATA code is required.", portInfo.HumanReadableName));
			}
		}

		void CheckZAPort(ZPropertyInfo portInfo, RefUNLOCO unloco)
		{
			if (unloco == null)
			{
				return;
			}

			var dischargePortCountryCode = unloco.Country.Code;
			if (dischargePortCountryCode != CountryCodes.SouthAfrica)
			{
				portInfo.AddMessageError(Res.GetString("9C8A4625-42C6-48F7-8B3A-B2C595D53C80", "{0} must be ZA.", portInfo.HumanReadableName));
			}
		}
	}
}
