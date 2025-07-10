using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB;

namespace Enterprise.Customs.SG.V4.MHUB
{
	public class LoginDetails
	{
		public LoginDetails(string ediServlet, string userID, string password)
		{
			this.EdiServlet = ediServlet;
			this.UserID = userID;
			this.Password = password;
			this.AppID = MHUBConstants.CARGOWISE;
		}

		public LoginDetails(string ediServlet, string userID, string password, string newPassword)
			: this(ediServlet, userID, password)
		{
			this.NewPassword = newPassword;
		}

		public readonly string EdiServlet;
		public readonly string AppID;
		public readonly string UserID;
		public readonly string Password;
		public readonly ZString NewPassword;
	}

	public class LoginCommand : MHUBWebCommand
	{
		public LoginCommand(LoginDetails loginDetails, IMHUBSettings settingsProvider, LoggingInformation logger, bool verboseLogging)
			: base(loginDetails.EdiServlet, settingsProvider, logger, verboseLogging)
		{
			this.loginDetails = loginDetails;
			loginState = LoginStateType.NotLoggedIn;
		}

		public enum LoginStateType { NotLoggedIn, LoggedIn }
		public LoginStateType LoginState
		{
			get { return loginState; }
			set { loginState = value; }
		}

		public override bool Execute()
		{
			bool result = base.Execute();
			if (result)
			{
				loginState = LoginStateType.LoggedIn;
			}

			return result;
		}

		#region Implementation

		public readonly LoginDetails loginDetails;
		LoginStateType loginState;
		Dictionary<string, object> inputParameterList;

		protected override MHUBConstants.CommandType CommandToExecute
		{
			get { return MHUBConstants.CommandType.Login; }
		}

		protected override Dictionary<string, object> InputParameterList
		{
			get
			{
				if (inputParameterList == null)
				{
					inputParameterList = new Dictionary<string, object>
					{
						{ MHUBConstants.Parameters.Userid, MhubEncryption.EncryptData(loginDetails.UserID, settingsProvider.EncryptionKey) },
						{ MHUBConstants.Parameters.Password, MhubEncryption.EncryptData(loginDetails.Password, settingsProvider.EncryptionKey) },
						{ MHUBConstants.Parameters.AppId, loginDetails.AppID }
					};
					if (!loginDetails.NewPassword.IsEmpty)
					{
						inputParameterList.Add(MHUBConstants.Parameters.NewPassword, MhubEncryption.EncryptData(loginDetails.NewPassword, settingsProvider.EncryptionKey));
					}

					var versionFourSOAP = settingsProvider.UseDirectWebServicesInsteadOfScripting;

					if (versionFourSOAP)
					{
						inputParameterList.Add(MHUBConstants.Parameters.Digest, MhubEncryption.EncryptData(settingsProvider.DigestForLibs, settingsProvider.EncryptionKey));
					}

					inputParameterList.Add(MHUBConstants.Parameters.Encrypted, "true");

					if (versionFourSOAP)
					{
						inputParameterList.Add(MHUBConstants.Parameters.ClientID, MHUBConstants.MHXWIN);
						inputParameterList.Add(MHUBConstants.Parameters.CurrentVersion, settingsProvider.ClientVersion);
						inputParameterList.Add(MHUBConstants.Parameters.VndId, settingsProvider.VendorID);
					}
				}

				return inputParameterList;
			}
		}

		#endregion
	}
}
