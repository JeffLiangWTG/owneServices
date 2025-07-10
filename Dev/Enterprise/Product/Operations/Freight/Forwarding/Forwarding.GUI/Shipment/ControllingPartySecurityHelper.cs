using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct ControllingPartyAuthorizationResult
	{
		public ControllingPartyAuthorizationResult(ContinueWithSave continueWithSave, bool authorizationWasRun) : this()
		{
			ContinueWithSave = continueWithSave;
			AuthorizationWasRun = authorizationWasRun;
		}

		public ContinueWithSave ContinueWithSave { get; private set; }
		public bool AuthorizationWasRun { get; private set; }
	}

	public static class ControllingPartySecurityHelper
	{
		public static ControllingPartyAuthorizationResult RequestSaveWithEmptyControllingPartyAuthorization(ForwardingShipment shipment, DocAddressType docAddressType, SecurityCheckpoint securityCheckPoint)
		{
			Argument.NotNull(shipment, nameof(shipment));
			Argument.NotNull(securityCheckPoint, nameof(securityCheckPoint));
			if (!(docAddressType == DocAddressType.ControllingAgent || docAddressType == DocAddressType.ControllingCustomer))
			{
				throw new ArgumentException("Invalid argument.", nameof(docAddressType));
			}

			Argument.NotNullOrEmpty(shipment.HumanReadableName, nameof(shipment.HumanReadableName));

			var docAddress = docAddressType == DocAddressType.ControllingCustomer ? shipment.ControllingCustomerAddress : shipment.ControllingAgentDocumentaryAddress;

			if (docAddress.IsEmpty && !securityCheckPoint.IsAllowed && CheckEffectiveDateRequiresSecurityCheck(shipment, docAddressType))
			{
				using (var logForm = new LoginForm())
				{
					var continueWithSave = ContinueWithSave.Yes;

					logForm.Message = Res.GetString("aab18ea7-a384-47e3-b551-6dd720f662b2",
						"You do not have the appropriate security rights to save a {0} with empty {1}. Please contact your system administrator or a user who has security rights to override this restriction.", shipment.HumanReadableName, docAddress.AddressCaption);

					var dlgResult = ZFormModaliser.ShowDialogWithoutDispose(logForm);

					if (dlgResult != DialogResult.OK)
					{
						continueWithSave = ContinueWithSave.No;
					}
					else if (logForm.Credentials == null || logForm.Credentials.UserSecurity == null)
					{
						Globals.Message.Show(Res.GetString("fc0fa8d8-8886-4818-a695-e3acf1b0c068", "User does not exist, password is invalid or password is expired."),
							Res.GetString("e248bf6d-e5e1-4691-8572-1bbec946f176", "Saving {0} with empty {1}", shipment.HumanReadableName, docAddress.AddressCaption),
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);

						continueWithSave = ContinueWithSave.No;
					}
					else if (!logForm.Credentials.UserSecurity.FindCheckPoint(securityCheckPoint.LookupKey).IsAllowed)
					{
						Globals.Message.Show(Res.GetString("5cc80760-81f7-418c-849b-2de3d7ffa1ab", "User does not have security rights to override this security policy."),
							Res.GetString("e248bf6d-e5e1-4691-8572-1bbec946f176", "Saving {0} with empty {1}", shipment.HumanReadableName, docAddress.AddressCaption),
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);

						continueWithSave = ContinueWithSave.No;
					}

					return new ControllingPartyAuthorizationResult(continueWithSave, true);
				}
			}

			return new ControllingPartyAuthorizationResult(ContinueWithSave.Yes, false);
		}

		static bool CheckEffectiveDateRequiresSecurityCheck(ForwardingShipment shipment, DocAddressType docAddressType)
		{
			var effectiveDateRegistry = docAddressType == DocAddressType.ControllingCustomer ? shipment.GetMandatoryControllingCustomerEffectiveDateRegistry() : shipment.GetMandatoryControllingAgentEffectiveDateRegistry();

			if (effectiveDateRegistry == null || effectiveDateRegistry.Value == DateTime.MinValue)
			{
				return true;
			}

			DateTime effectiveDateUtc = effectiveDateRegistry.Value;
			return shipment.IsInDatabase
					? shipment.JS_SystemCreateTimeUtc.IsValid && effectiveDateUtc.Date <= shipment.JS_SystemCreateTimeUtc.Date.ToDateTime()
					: effectiveDateUtc.Date <= ZDateTime.UtcToday.ToDateTime();
		}
	}
}
