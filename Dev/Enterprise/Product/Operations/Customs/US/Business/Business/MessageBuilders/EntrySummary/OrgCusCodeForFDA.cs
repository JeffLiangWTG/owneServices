using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public struct OrgCusCodeForFDA
	{
		public ZString Number;
		public ZString ID;

		public const string FEIEntityIdentificationCode = "47";
		public const string DUNSEntityIdentificationCode = "16";

		internal static ZString GetOrgCusCodeTypeViaEntityIdentificationCode(ZString entityIdentificationCode)
		{
			var result = ZString.Empty;
			if (entityIdentificationCode == DUNSEntityIdentificationCode)
			{
				result = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			}
			else if (entityIdentificationCode == FEIEntityIdentificationCode)
			{
				result = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			}
			return result;
		}

		static OrgCusCodeForFDA Empty => new OrgCusCodeForFDA { ID = ZString.Empty, Number = ZString.Empty };

		internal static OrgCusCodeForFDA FindCustomsNumberAndID(IDocAddress address, ZString programCode)
		{
			OrgCusCodeForFDA result;
			if (programCode == FDAProgramCodeList.Codes.DEV)
			{
				result = GetFEINumber(address);
				if (result.Number.IsEmpty)
				{
					result = GetDUNSNumber(address);
				}
			}
			else
			{
				result = GetDUNSNumber(address);
				if (result.Number.IsEmpty)
				{
					result = GetFEINumber(address);
				}
			}
			return result;
		}

		internal static OrgCusCodeForFDA FindCustomsNumberAndIDForFSVP(IDocAddress address) => GetDUNSNumber(address, makeEmptyIfUnknown: false);

		static OrgCusCodeForFDA GetDUNSNumber(IDocAddress address, bool makeEmptyIfUnknown = true)
		{
			var result = Empty;
			var dunsNumber = GetRegistrationNumberFromAddress(address, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
			if (makeEmptyIfUnknown && dunsNumber == USACEFDAAddInfoValidation.DUNSUnknown)
			{
				dunsNumber = ZString.Empty;
			}

			if (!dunsNumber.IsEmpty)
			{
				result.ID = DUNSEntityIdentificationCode;
				result.Number = dunsNumber;
			}

			return result;
		}

		static OrgCusCodeForFDA GetFEINumber(IDocAddress address)
		{
			var result = Empty;
			var feiNumber = GetRegistrationNumberFromAddress(address, OrgCusCode.CodeTypes.FDAEstablishmentIdentifier);
			if (!feiNumber.IsEmpty)
			{
				result.ID = FEIEntityIdentificationCode;
				result.Number = feiNumber;
			}
			return result;
		}

		static ZString GetRegistrationNumberFromAddress(IDocAddress address, ZString registrationType)
		{
			var result = ZString.Empty;
			if (address != null)
			{
				if (address is OrgAddress orgAddress)
				{
					result = orgAddress.CustomsCodes.GetCustomsRegNo(registrationType, Core.Constants.CountryCodes.UnitedStates);
				}
				else if (address is JobDocAddress docAddress)
				{
					if (docAddress.E2_AddressOverride)
					{
						result = docAddress.E2_GovRegNumType == registrationType ? docAddress.E2_GovRegNum : ZString.Empty;
					}
					else
					{
						result = GetRegistrationNumberFromAddress(docAddress.Address, registrationType);
					}
				}
			}

			return result;
		}
	}
}
