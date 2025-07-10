using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaTransferHeaderValidation : ASYCUDA.Business.AsycudaTransferHeaderValidation
	{
		public AsycudaTransferHeaderValidation(AutoAsycudaTransferHeader parent) : base(parent)
		{
		}

		protected override void CheckATF_RL_NKDestinationPortCode()
		{
			base.CheckATF_RL_NKDestinationPortCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ATF_RL_NKDestinationPortCodeInfo);
		}

		protected override void CheckATF_CarrierID()
		{
			base.CheckATF_CarrierID();
			ValidateATF_OnwardCarrier();
			const string CarrierIDReg_Default = "[0-9]{2}-[0-9]{7}[A-Z]{2}"; // NN-NNNNNNNAA
			const string CarrierIDReg_Importer = "[0-9]{2}-[0-9]{9}"; // NN-NNNNNNNNN
			const string CarrierIDReg_SSN = "[0-9]{3}-[0-9]{2}-[0-9]{4}"; // NNN-NN-NNNN
			const string CarrierIDReg_CBPAssigned = "[0-9]{6}-[0-9]{5}"; // NNNNNN-NNNNN
			if (!Parent.ATF_CarrierID.IsEmpty && !new Regex($"^({CarrierIDReg_Default}|{CarrierIDReg_Importer}|{CarrierIDReg_SSN}|{CarrierIDReg_CBPAssigned})$").IsMatch(Parent.ATF_CarrierID))
			{
				Parent.ATF_CarrierIDInfo.AddMessageError("Carrier ID must be of formats NN-NNNNNNNAA, NN-NNNNNNNNN, NNN-NN-NNNN or NNNNNN-NNNNN.");
			}
		}

		protected override void CheckATF_OnwardCarrier()
		{
			base.CheckATF_OnwardCarrier();
			if (!Parent.ATF_CarrierID.IsEmpty && !Parent.ATF_OnwardCarrier.IsEmpty)
			{
				Parent.ATF_OnwardCarrierInfo.AddWarning("You cannot send both In-bond Carrier and Onward Carrier to CBP. In-Bond Carrier ID will be sent to CBP in this case.");
			}
			if (Parent is AsycudaTransferHeader header && header.OnwardCarrier != null && header.OnwardCarrier.UI_ModeOfTransportation != TransportModeCodes.Codes.AirNonContainer)
			{
				Parent.ATF_OnwardCarrierInfo.AddMessageError($"The Transportation Mode of Onward Carrier should be '{TransportModeCodes.Codes.AirNonContainer}'.");
			}
		}

		protected override void CheckATF_OA_Carrier()
		{
			base.CheckATF_OA_Carrier();
			if (Parent is AsycudaTransferHeader header && header.InBondCarrierOrg != null)
			{
				var customsRegNo = header.InBondCarrierOrg.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber);
				if (!CusInBondMoveHeader.IsValidCodeToPopulateCarrierID(customsRegNo))
				{
					header.InBondCarrierOrgPKInfo.AddError("The Carrier does not have valid code to populate In-Bond Carrier ID.");
					header.ATF_OA_CarrierInfo.AddError("The Carrier does not have valid code to populate In-Bond Carrier ID.");
				}
			}
		}

		protected override void CheckATF_DestinationWarehouseID()
		{
			base.CheckATF_DestinationWarehouseID();
			if (!Parent.ATF_DestinationWarehouseID.IsEmpty && Parent.TransferBills.Any(bill => !(bill as AsycudaTransferBill).InBondNumber.IsEmpty))
			{
				Parent.ATF_DestinationWarehouseIDInfo.AddWarning("You cannot send both Bonded Premises ID and In-Bond Numbers to CBP. In-Bond Numbers will be sent to CBP in this case.");
			}
		}

		protected override void CheckATF_TransferType()
		{
			base.CheckATF_TransferType();
			if (Parent.TransferBills.Count == 0)
			{
				Parent.ATF_TransferTypeInfo.AddMessageError("A Transfer must contain at least one bill.");
			}
		}
	}
}
