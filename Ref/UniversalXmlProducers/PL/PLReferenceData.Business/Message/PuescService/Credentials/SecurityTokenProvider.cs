using System.Xml.Linq;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;

sealed class SecurityTokenProvider(IDigestBuilder digestBuilder = null)
{
	readonly static XName UsernameXName = Constants.Soap.SoapNamespaces.Security + Constants.Soap.XConstants.Username;
	readonly static XName PasswordXName = Constants.Soap.SoapNamespaces.Security + Constants.Soap.XConstants.Password;
	readonly static XName NonceXName = Constants.Soap.SoapNamespaces.Security + Constants.Soap.XConstants.Nonce;
	readonly static XName CreatedXName = Constants.Soap.SoapNamespaces.SecurityUtility + Constants.Soap.XConstants.Created;
	readonly static XName UsernameTokenXName = Constants.Soap.SoapNamespaces.Security + Constants.Soap.XConstants.UsernameToken;
	readonly static XName SecurityXName = Constants.Soap.SoapNamespaces.Security + Constants.Soap.XConstants.Security;

	public string GetSecurityTokenElements(string tokenId, string user, string password)
	{
		var (nonce, digest, created) = (digestBuilder ?? new DigestBuilder()).Build(password);

		var usernameElement = new XElement(UsernameXName, user);
		var passwordElement = new XElement(
			PasswordXName,
			new XAttribute(Constants.Soap.XConstants.Type, Constants.Soap.XAttributeValues.PasswordType),
			digest
		);
		var nonceElement = new XElement(
			NonceXName,
			new XAttribute(Constants.Soap.XConstants.EncodingType, Constants.Soap.XAttributeValues.EncodingType),
			nonce
		);
		var createdElement = new XElement(CreatedXName, created);

		var usernameTokenElement = new XElement(
			UsernameTokenXName,
			new XAttribute(Constants.Soap.SoapNamespaces.SecurityUtility + Constants.Soap.XConstants.Id, tokenId),
			new XAttribute(XNamespace.Xmlns + Constants.Soap.SoapNamespaces.SecurityUtilityAlias, Constants.Soap.SoapNamespaces.SecurityUtility.NamespaceName),
			usernameElement,
			passwordElement,
			nonceElement,
			createdElement
		);

		return GetUsernameTokenAsString(usernameTokenElement);
	}

	static string GetUsernameTokenAsString(XElement tokenElement)
	{
		var securityElement = new XElement(
			SecurityXName,
			new XAttribute(Constants.Soap.SoapNamespaces.SoapEnvelope + Constants.Soap.XConstants.MustUnderstand, "1"),
			new XAttribute(XNamespace.Xmlns + Constants.Soap.SoapNamespaces.SecurityAlias, Constants.Soap.SoapNamespaces.Security.NamespaceName),
			tokenElement
		);

		return securityElement.Element(UsernameTokenXName)?.ToString();
	}
}
