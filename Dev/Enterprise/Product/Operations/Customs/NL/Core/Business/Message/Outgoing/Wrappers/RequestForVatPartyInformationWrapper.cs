using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class RequestForVatPartyInformationWrapper : IRequestForVatPartyInformation
{
	public RequestForVatPartyInformationWrapper(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	public string JobReference => declaration.JE_DeclarationReference;

	public string FunctionCode => "90";

	public string Consignee => declaration.PaymentPartyEORINumber;

	public string DomesticDutyTaxParty => declaration.VATPartyTaxNumber;
}
