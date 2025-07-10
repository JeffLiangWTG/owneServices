using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.MHUB;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class BatchSG4InterchangeHelper : BatchSGInterchangeHelper
	{
		public BatchSG4InterchangeHelper(LoggingInformation logger)
			: base(logger)
		{
		}

		public override bool IsEnvironmentDataValid()
		{
			var useMhaccessScripting = !SGCustomsDataRegistry.Instance.UseMhxDirectWebServicesInsteadOfScripting.Value;
			if (useMhaccessScripting)
			{
				GetPathToMhaccessTwoWays();
			}
			return true;
		}

		public override bool ShowVerboseLogging
		{
			get { return SGCustomsDataRegistry.Instance.VerboseLogging.Value; }
		}

		public override GlbExternalPassword_SGv4 GetGlbExternalPassword(SGGlbStaffWrapper brokerWrapper)
		{
			return brokerWrapper.Tradenetv4Password;
		}

		public override ZString PasswordType => PasswordTypesList.Codes.SG4;

		public override ZString ApplicationDescription => "TradeNet";

		public override ZString[] ApplicationCodes => new ZString[] { ApplicationCodeList.Codes.SGCustomsTradenet4, ApplicationCodeList.Codes.SGCustomsTradenetXML };

		#region Test Connection Command

		public TestConnectionCommand TestConnectionCommand
		{
			get
			{
				if (testConnectionCommand == null)
				{
					var partialPathForTestConnectionServlet = SGCustomsDataRegistry.Instance.Mhx4EndpointPartialPathForTestConnectionServlet.Value;
					if (SGCustomsDataRegistry.Instance.SendTestMessages.Value)
					{
						testConnectionCommand = new TestConnectionCommand(SGCustomsDataRegistry.Instance.WebAddressTrial.Value.EffectiveValueForToday + partialPathForTestConnectionServlet, new MHUBSettingsProvider(), logger, ShowVerboseLogging);
					}
					else
					{
						testConnectionCommand = new TestConnectionCommand(SGCustomsDataRegistry.Instance.WebAddress.Value.EffectiveValueForToday + partialPathForTestConnectionServlet, new MHUBSettingsProvider(), logger, ShowVerboseLogging);
					}
				}

				return testConnectionCommand;
			}
		}
		TestConnectionCommand testConnectionCommand;

		public override bool UseTestConnection => TestConnectionCommand.Execute();

		#endregion

		#region GetPathToMhaccessTwoWays

		public string GetPathToMhaccessTwoWays()
		{
			var fullPath = GetPathToMhaccessViaWindowsRegistry();
			if (string.IsNullOrEmpty(fullPath))
			{
				fullPath = GetPathToMhaccessViaCW1Rego();
			}
			if (string.IsNullOrEmpty(fullPath))
			{
				throw new HostedServiceException("Path to MHAccess.exe could not be determined, see above logs for details.");
			}
			return fullPath;
		}

		string GetPathToMhaccessViaWindowsRegistry()
		{
			var result = "";
			var path = PathToMhaccessFolderViaWindowsRegistry;
			var mhxNotFoundMessage = ZString.Empty;
			if (string.IsNullOrEmpty(path))
			{
				mhxNotFoundMessage = "MHX 4 is not installed.  No 'Mhx_Home' value was found in the Windows registry. Looked for " + expectedWindowsRegistryNode;
			}
			else
			{
				var mhaccessDir = new DirectoryInfo(path);
				if (mhaccessDir.Exists)
				{
					var mhaccessFile = new FileInfo(Path.Combine(path, "MHAccess.exe"));
					if (!mhaccessFile.Exists)
					{
						mhxNotFoundMessage = "MHAccess.exe was not found. Expected=" + mhaccessFile.FullName;
					}
					else
					{
						result = mhaccessFile.FullName;
					}
				}
				else
				{
					mhxNotFoundMessage = "The directory corresponding to Windows Registry value 'Mhx_Home' was not found. Path=" + path;
				}
			}
			if (!mhxNotFoundMessage.IsEmpty)
			{
				logger.LogWarning(mhxNotFoundMessage);
			}
			return result;
		}

		string GetPathToMhaccessViaCW1Rego()
		{
			string result = "";
			var path = SGCustomsDataRegistry.Instance.MhaccessExePathFallback.Value;
			var mhxNotFoundMessage = ZString.Empty;
			if (!string.IsNullOrEmpty(path))
			{
				var mhaccessDir = new DirectoryInfo(path);
				if (mhaccessDir.Exists)
				{
					var mhaccessFile = new FileInfo(Path.Combine(path, "MHAccess.exe"));
					if (!mhaccessFile.Exists)
					{
						mhxNotFoundMessage = "MHAccess.exe was not found. Expected=" + mhaccessFile.FullName;
					}
					else
					{
						result = mhaccessFile.FullName;
					}
				}
				else
				{
					mhxNotFoundMessage = "The directory corresponding to CW1 registry was not found. Path=" + path;
				}
			}
			if (!mhxNotFoundMessage.IsEmpty)
			{
				logger.LogWarning(mhxNotFoundMessage);
			}
			return result;
		}

		static string PathToMhaccessFolderViaWindowsRegistry
		{
			get { return (string)Microsoft.Win32.Registry.GetValue(expectedWindowsRegistryNode, "Mhx_Home", ""); }
		}

		const string expectedWindowsRegistryNode = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\MHAccess\Info";

		#endregion
	}
}
