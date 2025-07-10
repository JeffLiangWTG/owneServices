using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB;

namespace Enterprise.Customs.SG.V4.MHUB
{
	public class TestConnectionCommand : MHUBWebCommand
	{
		public TestConnectionCommand(string testConnectionServlet, IMHUBSettings settingsProvider, LoggingInformation logger, bool verboseLogging)
			: base(testConnectionServlet, settingsProvider, logger, verboseLogging)
		{
		}

		public override bool Execute()
		{
			bool result = false;
			string commandString = GetCommandString(true);
			try
			{
				if (VerboseLogging)
				{
					Logger.Log("GetResponseFromMHub");
				}

				WebResult response = GetResponseFromMHub(commandString);
				if (VerboseLogging)
				{
					Logger.Log("MHUB Response = " + response.ResponseString);
				}

				if (response != null && response.StatusCode == HttpStatusCode.OK)
				{
					result = response.ResponseString.Trim().EqualsIgnoringCase("Connection is OK.");
				}
			}
			catch (WebException e)
			{
				Logger.LogError("WebException: " + e.Message);
			}
			catch (SocketException e)
			{
				const int WSAETIMEDOUT = 10060; //connection timed out
				if (e.ErrorCode == WSAETIMEDOUT)
				{
					Logger.LogError("Timed Out Socket Exception: " + e.Message);
				}
				else
				{
					if (VerboseLogging)
					{
						Logger.LogError("Socket Exception: " + e.Message);
					}

					throw;
				}
			}

			return result;
		}

		protected override MHUBConstants.CommandType CommandToExecute
		{
			get { return MHUBConstants.CommandType.None; }
		}

		protected override Dictionary<string, object> InputParameterList
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}
	}
}
