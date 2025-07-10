using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class RequestForGuaranteeInformationWrapper : IRequestForGuaranteeInformation
{
	public RequestForGuaranteeInformationWrapper(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	public string JobReference => declaration.JE_DeclarationReference;

	public string FunctionCode => "92";

	public string Declarant => declaration.PaymentPartyEORINumber;
}
