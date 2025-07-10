using System;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	internal class EBondRequestMessageBuilder
	{
		public EBondRequestMessageBuilder(JobDeclaration declaration, CusEntryHeader entryHeader)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			this.entryHeader = entryHeader;

			factory = declaration.Factory;
		}

		readonly JobDeclaration declaration;
		readonly BusinessObjectFactory factory;
		readonly CusEntryHeader entryHeader;

		const string USCustomsEBond = "USCustomsEBond";

		public ZBool Generate()
		{
			var hasMessageCreated = false;

			try
			{
				var messageText = GetMessageTextFromShipment();
				if (!messageText.IsEmpty)
				{
					var interchange = GenerateInterchange(messageText);
					GenerateMessage(interchange.PK, messageText);
					hasMessageCreated = true;
				}
				else
				{
					throw new InvalidMessageContentException("Cannot create message context from universal shipment.");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Invalid message context for US eBond", ex);
			}

			return hasMessageCreated;
		}

		#region Implement

		EDIInterchange GenerateInterchange(string messageText)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USeBond;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			interchange.EI_To = USCustomsEBond;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_GB = declaration.JE_GB;

			var credential = declaration.GetCredential();

			var password = credential != null && !credential.CurrentDecryptedCertificatePassphrase.IsEmpty
				? CredentialSender.EncryptPasswordAsString(credential.CurrentDecryptedCertificatePassphrase)
				: string.Empty;

			interchange.EI_BodyText = string.Format(CultureInfo.InvariantCulture
				, @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>{0}</SenderID>
    <RecipientID>{1}</RecipientID>
    <DeliveryMetadata>
      <ValueCollection>
        <Value>
          <Name>InsuranceAgent</Name>
          <Type>String</Type>
          <Data>{2}</Data>
        </Value>
        <Value>
          <Name>UserName</Name>
          <Type>String</Type>
          <Data>{3}</Data>
        </Value>
        <Value>
          <Name>Password</Name>
          <Type>Base64Binary</Type>
          <Data>{4}</Data>
        </Value>
      </ValueCollection>
    </DeliveryMetadata>
  </Header>
  <Body>
    {5}
  </Body>
</UniversalInterchange>"
				, interchange.EI_From
				, USCustomsEBond
				, declaration.US_InsuranceAgent
				, credential?.GP_UserID ?? ZString.Empty
				, password
				, messageText);

			return interchange;
		}

		void GenerateMessage(ZGuid interChangePk, string messageText)
		{
			var message = factory.New<EBondEDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIInterchangeStatusList.Codes.Sent;
			message.EM_MessageText = messageText;
			message.EM_EI = interChangePk;
			message.EM_GB = declaration.JE_GB;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = entryHeader.PK;
			message.Saving += Message_Saving;
			message.Saved += Message_Saved;
		}

		protected virtual void Message_Saving(Enterprise.Messaging.Business.EDIMessage message)
		{
			oldInsuranceDisposition = declaration.US_InsuranceDisposition;
			declaration.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.SentToSurety;
		}
		ZString oldInsuranceDisposition;

		void Message_Saved(Enterprise.Messaging.Business.EDIMessage message, bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				message.Saving -= Message_Saving;
				message.Saved -= Message_Saved;

				declaration.InBondRelatedRecords.ReBuild(MessagesToShowCollection.MessagesStatus.ActiveOnly);
			}
			else
			{
				declaration.US_InsuranceDisposition = oldInsuranceDisposition;

				if (!message.IsDeleted && !message.IsInDatabase)
				{
					var interchange = message.Interchange;
					if (interchange != null && !interchange.IsDeleted)
					{
						interchange.Delete();
					}

					message.Delete();
				}
			}
		}

		public virtual ZString GetMessageTextFromShipment()
		{
			var universalShipment = BuildUniversalShipment();
			var result = ZString.Empty;

			if (universalShipment != null)
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var writer = new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter();
					writer.WriteXML(universalShipment, stream, false);

					using (var reader = new StreamReader(stream))
					{
						result = reader.ReadToEnd();
					}
				}
			}

			return result;
		}

		UniversalShipment BuildUniversalShipment()
		{
			UniversalShipment universalShipment = null;

			var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager?.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)) { FilteredDataContextType = DataContextType.CustomsDeclaration });

			if (writer != null)
			{
				using (((IExternalFetchHintSupporter)declaration.Factory).SetupCreator())
				{
					universalShipment = writer.GetDataObject(declaration) as UniversalShipment;
				}
			}

			return universalShipment;
		}

		#endregion
	}
}
