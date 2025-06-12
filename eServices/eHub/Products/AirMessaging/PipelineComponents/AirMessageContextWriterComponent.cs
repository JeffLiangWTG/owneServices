using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[System.Runtime.InteropServices.Guid("09980516-e103-4337-9f2a-942bcab01620")]
	[ComponentCategory(CategoryTypes.CATID_Encoder)]
	public class AirMessageContextWriterComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent members
		[Browsable(false)]
		public string Name
		{
			get
			{
				return "Air Send EDI Message";
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
				return "Air Send EDI Message";
			}
		}
		#endregion

		#region IComponentUI members
		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		public IntPtr Icon { get; private set; }
		#endregion

		#region IPersistPropertyBag members
		public void GetClassID(out Guid classID)
		{
			classID = new Guid("09980516-e103-4337-9f2a-942bcab01620");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
            var var = LoadProperty(propertyBag, "FTPConfigurationID", errorLog);
            if (var != null) FTPConfigurationID = (string)var;
        }

        object LoadProperty(IPropertyBag propertyBag, string propertyName, int errorLog)
        {
            object result = null;
            try
            {
                propertyBag.Read(propertyName, out result, errorLog);
            }
            catch { }
            return result;
        }

        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
            object val = FTPConfigurationID;
            propertyBag.Write("FTPConfigurationID", ref val);
        }
        #endregion

        #region IComponent members
        public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			var logger = LoggerHelpers.GetPipelineLogger(message);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				var sourceParty = PipelineComponentsExtenstions.ReadPropertyString<BTS.SourceParty>(message.Context);
				var overrideEmailSubject = PipelineComponentsExtenstions.ReadPropertyString<OverrideEmailSubject>(message.Context);
				var trackingID = PipelineComponentsExtenstions.ReadPropertyString<MessageTrackingID>(message.Context);

                var pima = overrideEmailSubject.Replace(' ', '/');
				var clientRegistrationAccessor = GetClientRegistrationAccessor();
                var ftpConnectionInfo =
                    clientRegistrationAccessor.ReadAttr1Password1FirstOrDefault(sourceParty, FTPConfigurationID, code: pima, flag2: 1);

                if (ftpConnectionInfo == null)
				{
					GetExceptionAccessor().SubmitErrorAndUpdateStatus(Guid.NewGuid(), "BIZ", "Fai", "Could not find connection detail with PIMA: " + pima, Guid.Empty, Guid.Empty, Guid.Parse(trackingID), Guid.Parse(trackingID), null);
					return null;
				}

				var ftpUri = new UriBuilder(ftpConnectionInfo[0]);
				var decryptedPassword = EhubServerDecryptor.Decrypt(ftpConnectionInfo[1]);

                var overrideFilename = PipelineComponentsExtenstions.ReadPropertyString<OverrideFilename>(message.Context) ?? string.Empty;

                PipelineComponentsExtenstions.WriteProperty<OverrideFilename>(message.Context, overrideFilename.Replace('_', '.'));
                PipelineComponentsExtenstions.WriteProperty<BTS.OutboundTransportType>(message.Context, ftpUri.Scheme);
				PipelineComponentsExtenstions.WriteProperty<BTS.OutboundTransportLocation>(message.Context, ftpUri.ToString());
                message.Context.Write("Server", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.Host);
				message.Context.Write("Folder", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.Path);
				message.Context.Write("Port", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.Port);
				message.Context.Write("User", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.UserName);
				message.Context.Write("UserName", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", ftpUri.UserName);
				message.Context.Write("Password", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", decryptedPassword);
				message.Context.Write("AckRequired", "http://schemas.microsoft.com/BizTalk/2003/system-properties", true);
				message.Context.Write("CorrelationToken", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "UseDeliveryNotificationUpdateStatus");
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

		public virtual IExceptionsAccessor GetExceptionAccessor()
		{
			return DataAccessFactories.NewExceptionsAccessorInstance();
		}
        #endregion

        #region Properties
        public string FTPConfigurationID { get; set; }
        #endregion
    }
}
