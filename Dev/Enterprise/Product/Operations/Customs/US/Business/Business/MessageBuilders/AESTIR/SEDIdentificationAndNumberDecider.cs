using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business.AES;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class SEDIdentificationAndNumberDecider
	{
		public SEDIdentificationAndNumberDecider(OrgHeader organisation, OrgAddress address, string[] uSACodeTypesInSignificantOrder)
		{
			fUSACodeTypesInSignificantOrder = uSACodeTypesInSignificantOrder;
			ProcessIdentificationTypeAndIdentificationNumber(organisation, address);
		}

		public SEDIdentificationAndNumberDecider(JobDocAddress jobDocAddress, string[] uSACodeTypesInSignificantOrder)
		{
			Argument.Equals(jobDocAddress.E2_AddressOverride, true);
			fUSACodeTypesInSignificantOrder = uSACodeTypesInSignificantOrder;
			ProcessIdentificationTypeAndIdentificationNumber(jobDocAddress);
		}

		public ZString IdentificationType
		{
			get { return identificationType; }
		}
		ZString identificationType;

		public ZString IdentificationNumber
		{
			get { return identificationNumber.KeepAlphanumericCharacters(); }
		}
		ZString identificationNumber;

		void ProcessIdentificationTypeAndIdentificationNumber(OrgHeader organisation, OrgAddress address)
		{
			identificationType = ZString.Empty;
			identificationNumber = ZString.Empty;
			if (organisation != null)
			{
				foreach (string uSACodeType in USACodeTypesInSignificantOrder)
				{
					if (uSACodeType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem && address != null)
					{
						identificationNumber = address.CustomsCodes.GetCustomsRegNo(uSACodeType, Core.Constants.CountryCodes.UnitedStates);
					}
					else
					{
						identificationNumber = organisation.CustomsCodes.GetCustomsRegNo(uSACodeType, Core.Constants.CountryCodes.UnitedStates);
					}
					if (!identificationNumber.IsEmpty)
					{
						identificationType = GetIdentificationTypeMapCode(uSACodeType);
						break;
					}
				}
			}
		}

		void ProcessIdentificationTypeAndIdentificationNumber(JobDocAddress jobDocAddress)
		{
			identificationType = ZString.Empty;
			identificationNumber = ZString.Empty;
			if (jobDocAddress != null)
			{
				var govRegNumType = jobDocAddress.E2_GovRegNumType;
				var govRegNum = govRegNumType == OrgCusCode.CodeTypes.PassportID ? jobDocAddress.E2_PassportID : jobDocAddress.E2_GovRegNum;
				if (USACodeTypesInSignificantOrder.ToList().Contains(govRegNumType) && !govRegNum.IsEmpty)
				{
					identificationNumber = govRegNum;
					identificationType = GetIdentificationTypeMapCode(govRegNumType);
				}
			}
		}

		ZString GetIdentificationTypeMapCode(string uSACodeType)
		{
			ZString result = ZString.Empty;
			switch (uSACodeType)
			{
				case OrgCusCode.USACodeTypes.EmployerIdentificationNumber:
					result = AESConstants.IDTypes.EmployerIdentificationNumber;
					break;
				case OrgCusCode.USACodeTypes.SocialSecurityNumber:
					result = AESConstants.IDTypes.SocialSecurityNumber;
					break;
				case OrgCusCode.USACodeTypes.ForeignRegistrationNumber:
				case OrgCusCode.CodeTypes.PassportID:
					result = AESConstants.IDTypes.Foreign;
					break;
				default:
					result = AESConstants.IDTypes.DUNS;
					break;
			}
			return result;
		}

		protected IReadOnlyList<string> USACodeTypesInSignificantOrder
		{
			get
			{
				if (fUSACodeTypesInSignificantOrder == null)
				{
					fUSACodeTypesInSignificantOrder = new string[] {
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
						OrgCusCode.USACodeTypes.SocialSecurityNumber,
						OrgCusCode.USACodeTypes.ForeignRegistrationNumber,
						OrgCusCode.CodeTypes.PassportID,
						OrgCusCode.CodeTypes.DataUniversalNumberingSystem
					};
				}
				return fUSACodeTypesInSignificantOrder;
			}
		}
		string[] fUSACodeTypesInSignificantOrder;
	}
}
