using System;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business
{
	public class LocalCountryCustomsInterfaceIntegrationProvider : CusIntegrationProvider
	{
		#region Constants

		const string RootXmlTag = "UniversalInterchange";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml Content")]
		const string RootNamespce = "xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\"";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml Content")]
		const string HeaderXmlTag = "Header";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml Content")]
		const string BodyXmlTag = "Body";

		#endregion

		protected override ZString ApplicationCode
		{
			get { return ApplicationCodeList.Codes.UniversalDataMessaging; }
		}

		protected override ZString ProviderName
		{
			get { return Res.GetString("233a8fbc-d20a-49ff-83fd-c08c81d50674", "Local Country Customs Interface"); }
		}

		protected override ZString RegistryLocation
		{
			get { return Res.GetString("054c9729-a59e-4b2c-95ea-9afd6186ed56", "System > Registry > Customs > Integration > Local Country Customs Interface"); }
		}

		protected override XElement Submit(ZString submittedData)
		{
			return null;
		}

		protected override bool SubmitSucceeded(XElement submissionResult)
		{
			return true;
		}

		protected override ZBool WriteXMLDeclaration => false;

		protected override EDIMessage CreateEDIMessage(ZString submittedData)
		{
			var message = base.CreateEDIMessage(submittedData);

			var interchange = declaration.Factory.New<XmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_From = EnterpriseSenderID;
			interchange.EI_To = CustomsReceipientID;
			interchange.EI_Status = CustomsInterfaceType == LocalCountryCustomsInterfaceTypeCodeList.Codes.eAdaptor
				? EDIInterchangeStatusList.Codes.eAdaptorQueued
				: EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = CustomsInterfaceType == LocalCountryCustomsInterfaceTypeCodeList.Codes.eAdaptor
				? EDIInterchangeTransportTypeList.Codes.eAdaptor
				: EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_GB = declaration.Branch?.PK ?? ZGuid.Empty;
			interchange.EI_BodyText = GetMessageBody(submittedData);

			message.EM_GB = interchange.EI_GB;
			message.EM_EI = interchange.PK;

			return message;
		}

		ZString EnterpriseSenderID
		{
			get
			{
				if (enterpriseSenderID.IsEmpty)
				{
					enterpriseSenderID = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
				}
				return enterpriseSenderID;
			}
		}
		ZString enterpriseSenderID;

		ZString CustomsReceipientID
		{
			get
			{
				if (customsReceipientID.IsEmpty)
				{
					var customsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
					if (customsInterface != null)
					{
						customsReceipientID = customsInterface.RecipientID;
					}
				}
				return customsReceipientID;
			}
		}
		ZString customsReceipientID;

		ZString CustomsInterfaceType
		{
			get
			{
				if (customsInterfaceType.IsEmpty)
				{
					var customsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
					if (customsInterface != null)
					{
						customsInterfaceType = customsInterface.InterfaceType;
					}
				}
				return customsInterfaceType;
			}
		}
		ZString customsInterfaceType;

		ZString GetMessageBody(ZString submittedData)
		{
			var stringBuilder = new ZStringBuilder();
			stringBuilder.AppendFormat("<{0} {1}>", RootXmlTag, RootNamespce);
			stringBuilder.AppendFormat("<{0}>", HeaderXmlTag);
			stringBuilder.AppendFormat("<{0}>{1}</{0}>", "SenderID", EnterpriseSenderID);
			stringBuilder.AppendFormat("<{0}>{1}</{0}>", "RecipientID", CustomsReceipientID);
			stringBuilder.AppendFormat("</{0}>", HeaderXmlTag);
			stringBuilder.AppendFormat("<{0}>", BodyXmlTag);
			stringBuilder.Append(submittedData);
			stringBuilder.AppendFormat("</{0}>", BodyXmlTag);
			stringBuilder.AppendFormat("</{0}>", RootXmlTag);
			return stringBuilder.ToString();
		}
	}
}
