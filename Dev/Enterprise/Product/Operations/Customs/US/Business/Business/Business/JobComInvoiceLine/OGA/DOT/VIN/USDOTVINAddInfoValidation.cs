//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDOTVINAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSDOTVINAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USDOTVINAddInfoValidation : AutoUSDOTVINAddInfoValidation
	{
		public USDOTVINAddInfoValidation(AutoUSDOTVINAddInfo parent)
			: base(parent)
		{
			if (!(parent is DOTVINAddInfo))
			{
				throw new ArgumentException("Parent should be DOTVINAddInfo");
			}
		}

		DOT DOT
		{
			get { return Parent.Factory.Load<DOT>(((DOTVINAddInfo)Parent).Parent.B7_ParentID); }
		}

		bool IsACECargoCertificationMode
		{
			get { return DOT.InvoiceLine != null && DOT.InvoiceLine.IsACECargoCertificationMode; }
		}

		DOTVIN DOTVIN
		{
			get { return ((DOTVINAddInfo)Parent).Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			DoRowValidation();
		}

		void DoRowValidation()
		{
			DOTVIN.ClearRowNotifications();

			if (DOT.US_DOTClarCode != ClarificationCodeList.Codes.Vehicle && DOT.US_DOTBoxNo != DepartmentOfTransportBoxNumberList.Codes._05 && !IsACECargoCertificationMode)
			{
				DOTVIN.AddRowMessageError("Vehicle Details are only required when Clarification Code is 'V(ehicle)' or Box Number is 05");
			}
		}

		protected override void CheckUS_DOTMake()
		{
			base.CheckUS_DOTMake();
			CheckMakeModelYearVIN("Make", Parent.US_DOTMakeInfo);
		}

		protected override void CheckUS_DOTModel()
		{
			base.CheckUS_DOTModel();
			CheckMakeModelYearVIN("Model", Parent.US_DOTModelInfo);
		}

		protected override void CheckUS_DOTYear()
		{
			base.CheckUS_DOTYear();
			CheckMakeModelYearVIN("Year", Parent.US_DOTYearInfo);
		}

		protected override void CheckUS_DOTVIN()
		{
			base.CheckUS_DOTVIN();
			CheckMakeModelYearVIN("VIN", Parent.US_DOTVINInfo);
			CheckForDuplicateVIN();
		}

		void CheckForDuplicateVIN()
		{
			foreach (DOTVIN dotvin in DOT.DOTVINs)
			{
				if (dotvin.PK != DOTVIN.PK && dotvin.US_DOTVIN == DOTVIN.US_DOTVIN)
				{
					DOTVIN.US_DOTVINInfo.AddMessageError(ValidationConstants.DOT.DuplicateVIN);
					break;
				}
			}
		}

		void CheckMakeModelYearVIN(string fieldName, ZPropertyInfo info)
		{
			if (!IsACECargoCertificationMode)
			{
				if (DOT.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._03
					|| DOT.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._05
					|| DOT.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._12)
				{
					if (info.Value.IsEmpty)
					{
						info.AddMessageError(fieldName + " is required when Box Number is 03, 05 or 12.");
					}
				}
			}
			DoRowValidation();
		}

		protected override void CheckUS_DOTRINo()
		{
			base.CheckUS_DOTRINo();
			CheckRIVEN("RI Number", Parent.US_DOTRINoInfo);

			if (!Parent.US_DOTRINo.IsEmpty)
			{
				if (!Regex.IsMatch(Parent.US_DOTRINo, @"^[A-Z0-9]-[0-9]{2}-[0-9]{3}$", RegexOptions.IgnoreCase))
				{
					Parent.US_DOTRINoInfo.AddMessageError("The RI Number must be in the following format; 'R-YY-NNN'.");
				}
			}
		}

		protected override void CheckUS_DOTVEN()
		{
			base.CheckUS_DOTVEN();
			CheckRIVEN("Vehicle Eligibility Number", Parent.US_DOTVENInfo);

			if (!Parent.US_DOTVEN.IsEmpty && !IsACECargoCertificationMode)
			{
				if (!Regex.IsMatch(Parent.US_DOTVEN, @"^((VSA|VSP|VCP)[0-9]{3})|PET$", RegexOptions.IgnoreCase))
				{
					Parent.US_DOTVENInfo.AddMessageError("The VEN Number must be in the following format; 'VSA' or 'VSP' or 'VCP' followed by 3 numerics or 'PET'.");
				}
			}
		}

		void CheckRIVEN(string fieldName, ZPropertyInfo info)
		{
			if (!IsACECargoCertificationMode)
			{
				if (DOT.US_DOTBoxNo == DepartmentOfTransportBoxNumberList.Codes._03)
				{
					if (info.Value.IsEmpty)
					{
						info.AddMessageError(fieldName + " is required when Box Number is 03.");
					}
				}
				else
				{
					if (!info.Value.IsEmpty)
					{
						info.AddMessageError(fieldName + " is not required when Box Number is other than 03.");
					}
				}
			}

			DoRowValidation();
		}
	}
}
