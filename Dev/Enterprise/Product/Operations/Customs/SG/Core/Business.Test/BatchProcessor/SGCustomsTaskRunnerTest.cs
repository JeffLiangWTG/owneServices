using System;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	sealed class SGCustomsTaskRunnerTest : TestCaseWithFactory
	{
		public void TestTestSystemAlwaysSendsTestMessages()
		{
			var databaseTypes = new DatabaseTypes().GetAllCodes();
			foreach (var dataType in databaseTypes)
			{
				if (dataType == DatabaseTypes.Codes.Production)
				{
					AssertResetToTestSystem(dataType, registryValue: false);
				}
				else
				{
					AssertResetToTestSystem(dataType, registryValue: true);
				}
			}
		}

		void AssertResetToTestSystem(string databaseType, bool registryValue)
		{
			LicenceTypeChanger.SetSystemLicence(databaseType);
			SGCustomsDataRegistry.Instance.SendTestMessages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false); // set messages to send to production
			var stRunner = new SGCustomsTaskRunner(Logger);
			stRunner.Run(new CancellationToken());
			AssertEquals("ResetToTestSystemIfNeeded", registryValue, SGCustomsDataRegistry.Instance.SendTestMessages.Value);
		}

		ILogger Logger
		{
			get
			{
				return logger ?? (logger = new LoggerForTest());
			}
		}

		ILogger logger;
	}
}
