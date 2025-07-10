using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ApplicationManager.Common;

namespace Enterprise.CryptoUtilities
{
	public class X509CertificateManager : IAppManagerInvocable
	{
		enum Action
		{
			None,
			Add,
			Remove,
		}

		public AppManagerResult Invoke(bool waitedForMutex, object state)
		{
			X509Certificate2 certificate;
			Action action;
			var result = TryParseState(state, out certificate, out action);
			if (result.Status == AppManagerResultStatus.Success)
			{
				try
				{
					var store = new X509Store(System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine);
					store.Open(OpenFlags.ReadWrite);
					try
					{
						switch (action)
						{
							case Action.Add:
								store.Add(certificate);
								break;
							case Action.Remove:
								store.Remove(certificate);
								break;
						}
					}
					finally
					{
						store.Close();
					}
				}
				catch (Exception e)
				{
					result = new AppManagerResult(AppManagerResultStatus.Error, e.Message);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer error message, switch case label")]
		AppManagerResult TryParseState(object state, out X509Certificate2 certificate, out Action action)
		{
			certificate = null;
			action = Action.None;

			var parameters = state as object[];
			if (parameters == null)
			{
				return new AppManagerResult(AppManagerResultStatus.Error, "state must be an array");
			}
			if (parameters.Length != 2)
			{
				return new AppManagerResult(AppManagerResultStatus.Error, "Length of a state array must be 2");
			}

			certificate = parameters[0] as X509Certificate2;
			if (certificate == null)
			{
				return new AppManagerResult(AppManagerResultStatus.Error, "The first element in a state array must be a X509Certificate2 to add or remove.");
			}

			var actionStr = parameters[1] as string;
			if (actionStr == null)
			{
				return new AppManagerResult(AppManagerResultStatus.Error, @"The second element in a state array must be a string.");
			}
			switch (actionStr)
			{
				case "add":
					action = Action.Add;
					break;
				case "remove":
					action = Action.Remove;
					break;
				default:
					return new AppManagerResult(AppManagerResultStatus.Error, "Unknown action \"" + actionStr + "\". It must be \"add\" or \"remove\".");
			}
			return new AppManagerResult(AppManagerResultStatus.Success);
		}
	}
}
