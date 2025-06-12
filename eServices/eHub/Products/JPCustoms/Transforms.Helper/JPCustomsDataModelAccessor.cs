using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.JPCustoms.Transforms.Helper
{
    public class JPCustomsDataModelAccessor
    {
        const string ClientRegistrationType = "JPCustomsAccount_ClientLevel";
        const string SystemRegistrationType = "JPCustomsAccount_SystemLevel";

		public string GetUsername(string senderID)
		{
			var systemID = senderID.Substring(0, 3) + senderID.Substring(6, 3);
            var clientReg = DataModelAccessor.GetClientRegistration(senderID, ClientRegistrationType);
            if (clientReg != null && !string.IsNullOrWhiteSpace(clientReg.CX_Code)) return clientReg.CX_Code;
            var systemReg = DataModelAccessor.GetClientSystemRegistration(systemID, null, SystemRegistrationType);
            if (systemReg != null && !string.IsNullOrWhiteSpace(systemReg.CD_Code)) return systemReg.CD_Code.Trim();
            return CodeMapper.CallActionProcedureHelper("GetUserIDPasswordReference", "@Result", "@ApplicationCode", "JPC", "@eHubID", senderID, "@IsGettingPassword", "0").Trim();
        }

        public string GetPassword(string senderID)
		{
			var systemID = senderID.Substring(0, 3) + senderID.Substring(6, 3);
            var clientReg = DataModelAccessor.GetClientRegistration(senderID, ClientRegistrationType);
            if (clientReg != null && !string.IsNullOrWhiteSpace(clientReg.CX_Password1)) return clientReg.CX_Password1;
            var systemReg = DataModelAccessor.GetClientSystemRegistration(systemID, null, SystemRegistrationType);
            if (systemReg != null && !string.IsNullOrWhiteSpace(systemReg.CD_Attr1)) return systemReg.CD_Attr1.Trim();
			return CodeMapper.CallActionProcedureHelper("GetUserIDPasswordReference", "@Result", "@ApplicationCode", "JPC", "@eHubID", senderID, "@IsGettingPassword", "1").Trim();
		}

        public string GetSPID()
        {
            var clientReg = DataModelAccessor.GetClientRegistration("eHub", ClientRegistrationType);
            if (clientReg != null && !string.IsNullOrWhiteSpace(clientReg.CX_Code)) return clientReg.CX_Code;
            return CodeMapper.CallActionProcedureHelper("GetUserIDPasswordReference", "@Result", "@ApplicationCode", "JPC", "@eHubID", "eHub", "@IsGettingPassword", "0").Trim();
        }

        public string GetSPPassword()
        {
            var clientReg = DataModelAccessor.GetClientRegistration("eHub", ClientRegistrationType);
            if (clientReg != null && !string.IsNullOrWhiteSpace(clientReg.CX_Password1)) return clientReg.CX_Password1;
            return CodeMapper.CallActionProcedureHelper("GetUserIDPasswordReference", "@Result", "@ApplicationCode", "JPC", "@eHubID", "eHub", "@IsGettingPassword", "1").Trim();
        }

		DataModelAccessor dataModelAccessor;
		public DataModelAccessor DataModelAccessor { get { return dataModelAccessor ?? (dataModelAccessor = new DataModelAccessor()); } set { dataModelAccessor = value; } }
		CodeMapper codeMapper;
		public CodeMapper CodeMapper { get { return codeMapper ?? (codeMapper = new CodeMapper()); } set { codeMapper = value; } }
    }
}
