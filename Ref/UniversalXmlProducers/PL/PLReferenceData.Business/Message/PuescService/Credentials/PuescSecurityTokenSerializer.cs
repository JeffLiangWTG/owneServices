using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;

class PuescSecurityTokenSerializer : SecurityTokenSerializer
{
	protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader) => false;

	protected override bool CanReadKeyIdentifierCore(XmlReader reader) => false;

	protected override bool CanReadTokenCore(XmlReader reader) => false;

	protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause) => false;

	protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier) => false;

	protected override bool CanWriteTokenCore(SecurityToken token) => true;

	protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader) => null;

	protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader) => null;

	protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver) => null;

	protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause) {}

	protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier) {}

	protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
	{
		if (token is UserNameSecurityToken userNameToken)
		{
			var provider = new SecurityTokenProvider();
			writer.WriteRaw(provider.GetSecurityTokenElements(userNameToken.Id, userNameToken.UserName, userNameToken.Password));
		}
	}
}
