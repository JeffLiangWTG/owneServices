using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class PartyDetails : PartyDetailsWrapper
	{
		public PartyDetails(JobDeclaration declaration, OrgHeader orgHeader) : this(declaration, orgHeader, orgHeader?.MainAddress)
		{
		}

		public PartyDetails(JobDeclaration declaration, OrgHeader orgHeader, OrgAddress address)
			: base(address, orgHeader)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			orgCusCodeCollection = orgHeader?.CustomsCodes;
			fVATCustomsRegNo = GetCustomsRegNo(orgCusCodeCollection, OrgCusCode.CodeTypes.VATCode);
			fPassportIDCustomsRegNo = GetCustomsRegNo(orgCusCodeCollection, OrgCusCode.CodeTypes.PassportID);
			fPIDCustomsRegNo = GetCustomsRegNo(orgCusCodeCollection, OrgCusCode.TaiwanCodeTypes.PID);
		}

		protected readonly JobDeclaration declaration;
		protected readonly OrgCusCodeCollection orgCusCodeCollection;
		protected readonly ZString fVATCustomsRegNo;
		readonly ZString fPassportIDCustomsRegNo;
		readonly ZString fPIDCustomsRegNo;

		protected override ZString IDCore
		{
			get
			{
				var result = fVATCustomsRegNo;

				if (result.IsEmpty)
				{
					result = fPassportIDCustomsRegNo;
				}

				if (result.IsEmpty)
				{
					result = fPIDCustomsRegNo;
				}

				return result;
			}
		}

		protected override ZString TypeCodeCore
		{
			get
			{
				var result = ZString.Empty;

				if (!fVATCustomsRegNo.IsEmpty)
				{
					result = PartyIdentifierCodeList.Codes._58;
				}
				else if (!fPassportIDCustomsRegNo.IsEmpty)
				{
					result = PartyIdentifierCodeList.Codes._53;
				}
				else if (!fPIDCustomsRegNo.IsEmpty)
				{
					result = PartyIdentifierCodeList.Codes._174;
				}

				return result;
			}
		}

		protected override ZString LPCOAuthorizedPartyIDCore => GetAEONumber();
	}
}
