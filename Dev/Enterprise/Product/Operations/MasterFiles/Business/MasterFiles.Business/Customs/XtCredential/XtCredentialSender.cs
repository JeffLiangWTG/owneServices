using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.Common.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business.Customs.XtCredential
{
	public static class XtCredentialSender
	{
		public static bool SendXtCredential(IxTCredentialProvider provider)
		{
			var currentCompany = (IGlbCompany)Env.CurrentCompany;
			var factory = provider.Factory;

			var interchange = factory.New<IXmlEDIInterchange>();

			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XHCredentialConfig;
			interchange.EI_From = currentCompany.LicenceKeyIdentifier;
			interchange.EI_To = XtCredentialConstants.CustomsCredentialChange;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_InterchangeType = provider.Identifier;
			interchange.EI_BodyText = provider.CreateXmlObject().ToXml();
			interchange.EI_GB = currentCompany.GetActiveBranches().OrderBy(branch => branch.GB_Code).FirstOrDefault(b => !string.IsNullOrEmpty(b.GB_Code)).PK;
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_IsActive = true;

			var message = factory.New<IXmlEDIMessage>();
			message.EM_EI = interchange.PK;
			message.EM_ApplicationCode = interchange.EI_ApplicationCode;
			message.EM_ReceiveTransmit = interchange.EI_ReceiveTransmit;
			message.EM_Status = interchange.EI_Status;
			message.EM_MessageText = interchange.EI_BodyText;
			message.EM_GB = interchange.EI_GB;
			message.EM_IsActive = interchange.EI_IsActive;
			message.EM_LinkUniqueID = provider.LinkUniqueID;
			message.EM_LinkTable = provider.LinkTableName;
			message.EM_MessageType = provider.Identifier;
			message.EM_MessageSubType = GetInterchangeType(provider.RequiredAction);

			return true;
		}

		static string GetInterchangeType(XtCredentialAction action)
		{
			string result = string.Empty;
			switch (action)
			{
				case XtCredentialAction.New:
					result = XtCredentialConstants.MessageSubTypes.New;
					break;
				case XtCredentialAction.Delete:
					result = XtCredentialConstants.MessageSubTypes.Delete;
					break;
				case XtCredentialAction.Update:
					result = XtCredentialConstants.MessageSubTypes.Update;
					break;
			}

			return result;
		}

		static CredentialChanges CreateXmlObject(this IxTCredentialProvider provider)
		{
			var change = new CredentialChangesCredentialChange();
			change.ChangeType = provider.RequiredAction.ToString();
			change.ChangeDateTime = provider.ChangeDateTime;
			change.EnterpriseCode = provider.EnterpriseCode;
			change.DatabaseCode = provider.DatabaseCode;
			change.DatabaseNumber = provider.DatabaseNumber;
			change.LicenceType = XtCredentialLicenceType.GetCode();
			change.Password = provider.Password;
			change.OldPassword = provider.OldPassword;

			var result = new CredentialChanges();
			result.CredentialChange = change;

			return result;
		}

		static ZString ToXml(this CredentialChanges credential)
		{
			using (var stream = new MemoryStream())
			using (var xmlWriter = XmlWriter.Create(stream, GetDefaultXmlWriterSettings()))
			{
				new XmlSerializer(typeof(CredentialChanges)).Serialize(xmlWriter, credential, GetDefaultNamespaces());
				xmlWriter.Flush();

				return stream.ReadToEnd();
			}
		}

		static XmlWriterSettings GetDefaultXmlWriterSettings()
		{
			return new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
			};
		}

		static XmlSerializerNamespaces GetDefaultNamespaces()
		{
			var result = new XmlSerializerNamespaces();
			if (typeof(CredentialChanges).GetAttribute<XmlTypeAttribute>() is XmlTypeAttribute xmlTypeAttr)
			{
				result.Add(string.Empty, xmlTypeAttr.Namespace);
			}
			return result;
		}
	}
}
