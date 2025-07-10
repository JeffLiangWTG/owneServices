using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class VNEAdditionalNumberValidation : CusCodeDataValidation
	{
		public VNEAdditionalNumberValidation(AutoCusCodeData parent)
			: base(parent)
		{
		}

		public VehicleDetails VehicleDetail
		{
			get { return (VehicleDetails)((VNEAdditionalNumber)base.Parent).Parent; }
		}

		protected override void CheckCY_Code()
		{
			var code = (VNEAdditionalNumber)Parent;
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo, code.NumberTypeList);

			if (Parent.CY_Code.IsEmpty)
			{
				if (!Parent.CY_Data.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_CodeInfo);
				}
			}
			else
			{
				var vehicleDetail = VehicleDetail;
				if ((Parent.CY_Code == ItemIdentityNumberQualifierList.Codes.SerialNumber || Parent.CY_Code == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN) && vehicleDetail != null)
				{
					if (!vehicleDetail.US_IdentityNumberQualifier.IsEmpty)
					{
						if (Parent.CY_Code != vehicleDetail.US_IdentityNumberQualifier)
						{
							Parent.CY_CodeInfo.AddMessageError(NumberTypeMessageText);
						}
					}
					else
					{
						Parent.CY_CodeInfo.AddWarning(ZString.Format(NumberNotSendInMessageText, "vehicle"));
					}
				}
				else
				{
					if (vehicleDetail.US_EngineNumber.IsEmpty)
					{
						Parent.CY_CodeInfo.AddWarning(ZString.Format(NumberNotSendInMessageText, "engine"));
					}
				}
			}

			Parent.Validation.ValidateCY_Data();
		}
		internal const string NumberTypeMessageText = "The additional number type should be the same as Vehicle ID Type.";
		internal const string NumberNotSendInMessageText = "There is no {0} details entered, The additional number will not be sent in message.";

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (!Parent.CY_Code.IsEmpty)
			{
				if (Parent.CY_Data.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
				}
				else
				{
					var code = (VNEAdditionalNumber)Parent;
					var vehicleDetail = VehicleDetail;
					if (vehicleDetail != null)
					{
						var iVNEDetails = vehicleDetail as IVNEDetails;
						if (code.CY_Code == ItemIdentityNumberQualifierList.Codes.EngineNumber)
						{
							var engineAdditionalNumber = iVNEDetails.EngineAdditionalNumbers;
							if (engineAdditionalNumber.Count(x => x.Number == code.CY_Data) > 1 || code.CY_Data == iVNEDetails.EngineNumber)
							{
								Parent.CY_DataInfo.AddMessageError(NumberMessageText);
							}
						}
						else
						{
							if (Parent.CY_Data.Length != 17 && code.CY_Code == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN)
							{
								Parent.CY_DataInfo.AddMessageError(NumberLengthText);
							}
							else
							{
								var vehicleAdditionalNumber = iVNEDetails.VehicleAdditionalNumbers;
								if (vehicleAdditionalNumber.Count(x => x.Number == code.CY_Data) > 1 || (code.CY_Data == iVNEDetails.IdentityNumber && code.CY_Code == iVNEDetails.IdentityNumberQualifier))
								{
									Parent.CY_DataInfo.AddMessageError(NumberMessageText);
								}
							}
						}
					}
				}
			}
			Parent.Validation.ValidateCY_Code();
		}
		internal const string NumberMessageText = "You have already entered an additional number with same type and number.";
		internal const string NumberLengthText = "Number format is invalid, it should be 17 characters.";
	}
}
