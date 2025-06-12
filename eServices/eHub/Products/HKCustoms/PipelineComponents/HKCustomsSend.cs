using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using System;
using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eServices.Encryption.Server.Decryptor;


namespace CargoWise.eHub.Products.HKCustoms.PipelineComponents
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[System.Runtime.InteropServices.Guid("3E1E3E45-30B8-4EB6-88E3-6748A24B46B8")]
	[ComponentCategory(CategoryTypes.CATID_Encoder)]
	public class HKCustomsSend : Microsoft.BizTalk.Component.Interop.IComponent, IBaseComponent, IPersistPropertyBag, IComponentUI
	{
		#region IBaseComponent members
		[Browsable(false)]
		public string Name
		{
			get
			{
				return "HKC Send EDI Message";
			}
		}
		[Browsable(false)]
		public string Version
		{
			get { return "1.0"; }
		}

		[Browsable(false)]
		public string Description
		{
			get
			{
				return "HKC Send EDI Message";
			}
		}
		#endregion

		#region IPersistPropertyBag members
		public void GetClassID(out Guid classID)
		{
			classID = new Guid("3E1E3E45-30B8-4EB6-88E3-6748A24B46B8");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
		}
		#endregion

		#region IComponentUI members
		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		public IntPtr Icon { get; private set; }
		#endregion

		#region IComponent members
		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var logger = LoggerHelpers.GetPipelineLogger(message);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				MessageHelper.WrapMessageInReadOnlySeekableStream(pipelineContext, message);
				var sourceParty = PipelineComponentsExtenstions.ReadPropertyString<BTS.SourceParty>(message.Context);
				var trackingID = PipelineComponentsExtenstions.ReadPropertyString<MessageTrackingID>(message.Context);
				var pima = "";

				var extractedResult = ExtractSenderPIMA(message.BodyPart.Data.ReadToEnd());

				if (!extractedResult.Success || string.IsNullOrWhiteSpace(extractedResult.Groups[1].Value))
				{
					GetExceptionAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "BIZ", "Fai", "Message does not contain valid PIMA", Guid.Empty, Guid.Empty, Guid.Parse(trackingID), Guid.Parse(trackingID), null);
					return null;
				}

				pima = extractedResult.Groups[1].Value;
				var clientRegistrationAccessor = GetClientRegistrationAccessor();
				var ftpConnectionInfo = clientRegistrationAccessor.ReadAttr1Password1FirstOrDefault(sourceParty, "HKC", code: pima, flag2: 1);

				if (ftpConnectionInfo == null)
				{
					GetExceptionAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "BIZ", "Fai", "Could not find connection detail with PIMA: " + pima, Guid.Empty, Guid.Empty, Guid.Parse(trackingID), Guid.Parse(trackingID), null);
					return null;
				}

				var ftpUri = new UriBuilder(ftpConnectionInfo[0]);
				var decryptedPassword = EhubServerDecryptor.Decrypt(ftpConnectionInfo[1]);

				PipelineComponentsExtenstions.WriteProperty<BTS.OutboundTransportType>(message.Context, "FTP");
				PipelineComponentsExtenstions.WriteProperty<BTS.OutboundTransportLocation>(message.Context, ftpUri.ToString());
				message.Context.Write("Server", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.Host);
				message.Context.Write("Folder", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.Path);
				message.Context.Write("Port", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.Port);
				message.Context.Write("User", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.UserName);
				message.Context.Write("UserName", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.UserName);
				message.Context.Write("Password", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", decryptedPassword);
				message.Context.Write("AckRequired", "http://schemas.microsoft.com/BizTalk/2003/system-properties", true);
				message.Context.Write("CorrelationToken", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "UseDeliveryNotificationUpdateStatus");
				message.BodyPart.Data.SeekBegin();
				logger.DebugFormat("Starting to send message. Tracking ID: {0}, FTP Url: {1}", trackingID, ftpUri.ToString());
				return message;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				throw;
			}
			finally
			{
				LoggerHelpers.LogComponentEnd(logger, this);
			}
		}
		#endregion

		#region Implementation
		public virtual IClientRegistrationAccessor GetClientRegistrationAccessor()
		{
			return new ClientRegistrationAccessor();
		}

		public virtual DataModelAccessor GetDataModelAccessor()
		{
			return new DataModelAccessor();
		}

		public virtual IExceptionsAccessor GetExceptionAccessor()
		{
			return DataAccessFactories.NewExceptionsAccessorInstance();
		}

		static Match ExtractSenderPIMA(string input)
		{
			string pattern = @"UNOA:1\+(.*?):PIMA";
			var regex = new Regex(pattern);
			return regex.Match(input);
		}
		#endregion
	}
}
