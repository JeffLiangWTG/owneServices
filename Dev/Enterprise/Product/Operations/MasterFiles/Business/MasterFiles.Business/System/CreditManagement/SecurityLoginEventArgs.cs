using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.CreditControl.Business
{
	public interface ICustomMessageBox
	{
		CustomMessageBoxCallback CustomMessageBox { get; }
		MultilingualString LoginPromptMessage { get; }
	}

	public class SecurityLoginEventArgs : EventArgs
	{
		public SecurityLoginEventArgs(MultilingualString loginPromptMessage, MultilingualString messageToShowWhenNotPrinting, Func<SecurityCore, SecurityCheckpoint> getSecurityCheckpoint)
		{
			this.IsAllowedToProceed = false;

			this.LoginPromptMessage = loginPromptMessage;
			this.MessageToShowWhenNotAllowed = messageToShowWhenNotPrinting;
			this.securityCheckpoints = new List<Func<SecurityCore, SecurityCheckpoint>>();
			if (getSecurityCheckpoint != null)
			{
				this.securityCheckpoints.Add(getSecurityCheckpoint);
			}
		}

		public bool IsAllowedToProceed { get; set; }

		public bool WasCancelled
		{
			get { return ((ZString)MessageToShowWhenNotAllowed).Contains(SecurityLogin.CancelledText, StringComparison.OrdinalIgnoreCase); }
		}

		public MultilingualString LoginPromptMessage { get; private set; }

		public List<Func<SecurityCore, SecurityCheckpoint>> SecurityCheckPoints
		{
			get { return this.securityCheckpoints; }
		}
		readonly List<Func<SecurityCore, SecurityCheckpoint>> securityCheckpoints;

		public MultilingualString Message
		{
			get { return IsAllowedToProceed ? (NoResString)"" : MessageToShowWhenNotAllowed; }
		}

		public MultilingualString MessageToShowWhenNotAllowed { get; set; }

		public ZString AuthorisingStaffLogin { get; set; }

		public bool HideApprovalRequestButton { get; set; }
	}

	public delegate ZDialogResult CustomMessageBoxCallback(ZString message, ZString caption);

	public class SecurityLoginEventArgsWithCustomMessageBox : SecurityLoginEventArgs, ICustomMessageBox
	{
		public SecurityLoginEventArgsWithCustomMessageBox(MultilingualString loginPromptMessage, MultilingualString messageToShowWhenNotPrinting, Func<SecurityCore, SecurityCheckpoint> getSecurityCheckpoint, CustomMessageBoxCallback messageBoxCallback)
			: base(loginPromptMessage, messageToShowWhenNotPrinting, getSecurityCheckpoint)
		{
			this.CustomMessageBox = messageBoxCallback;
		}

		public CustomMessageBoxCallback CustomMessageBox { get; private set; }
	}
}
