using System;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.XLANGs.BaseTypes;
using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.Shared.ClientSpecificFTP.PipelineComponents
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [Guid("EA28EF80-C108-4CB5-B0BA-8F590A326F8D")]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    public class PromoteFtpDetails : ComponentBase, IComponent
    {
        #region ComponentBase Members

        protected override Guid ClassID
        {
            get { return new Guid("EA28EF80-C108-4CB5-B0BA-8F590A326F8D"); }
        }

        protected override string DisplayName
        {
            get { return "Promote FTP Connection Details"; }
        }

        #endregion IBaseComponent Members

        #region IComponent Members

        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            if (Enabled)
            {
                var logger = LoggerHelpers.GetPipelineLogger(pInMsg);
                LoggerHelpers.LogComponentStart(logger, this);
                try
                {
                    var sourceParty = pInMsg.Context.Read("SourceParty", "http://cargowise.com/ehub/system-properties/2010/06");
                    var destinationParty = pInMsg.Context.Read("DestinationParty", "http://cargowise.com/ehub/system-properties/2010/06");
                    //var eventBranch = pInMsg.Context.Read("EventBranch", "http://cargowise.com/ehub/ocm-properties/2010/06");
                    if (sourceParty == null || destinationParty == null)
                    {
                        return null;
                    }
                    var registrationAccessor = GetClientRegistrationAccessor();
                    var registration = registrationAccessor.ReadRegistrations(client: sourceParty.ToString(), registrationType: "CSFTP", flag1: 1, flag2: 1)
                        .Where(x => Regex.IsMatch(destinationParty.ToString(), x["CX_Code"].ToString()))
                        .FirstOrDefault();
                    if (registration == null)
                    {
                        return null;
                    }
                    var uriBuilder = new UriBuilder(registration["CX_Qualifier"].ToString());
                    pInMsg.Context.Write("User", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", uriBuilder.UserName);
                    pInMsg.Context.Write("UserName", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", uriBuilder.UserName);
                    pInMsg.Context.Write("Password", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", registration["CX_Password1"].ToString()); //EhubServerDecryptor.Decrypt(registration["CX_Password1"].ToString()));
                    pInMsg.Context.Write("Server", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", uriBuilder.Host);
                    pInMsg.Context.Write("Folder", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", uriBuilder.Path);
                    pInMsg.Context.Write("Port", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", uriBuilder.Port);
                    return pInMsg;
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
            return pInMsg;
        }

        #endregion IComponent Members

        #region Properties

        public bool Enabled { get; set; }

        #endregion Properties

        #region Implementation

        void PromoteIfExistAndNotPromoted<T>(IBaseMessage pInMsg) where T : PropertyBase, new()
        {
            var obj = pInMsg.Context.ReadPropertyString<T>();
            if (obj != null)
            {
                var t = new T();
                var isPromoted = pInMsg.Context.IsPromoted(t.Name.Name, t.Name.Namespace);
                if (!isPromoted)
                {
                    pInMsg.Context.PromoteProperty<T>(obj);
                }
            }
        }

        protected internal virtual IClientRegistrationAccessor GetClientRegistrationAccessor()
        {
            return new ClientRegistrationAccessor();
        }

        #endregion Implementation
    }
}
