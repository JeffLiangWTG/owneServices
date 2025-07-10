using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class ContextSwitching : IDisposable
	{
		public ContextSwitching(EnterpriseServerAndCompanyID contextInfo, IXmlImportLogger logger, TopLevelDataObject topLevelDO)
		{
			DisposableLeakListenerWrapper.RegisterDisposable(this);
			Success = true;
			if (contextInfo != null)
			{
				Success = IncomingContextMatchesRegistrationKey(contextInfo, logger);
				if (Success && IsCompanyContextSufficient(contextInfo, logger))
				{
					disposableContext = GetCompanyContext(topLevelDO.DataContext.CompanyCodeToImportInto);
					if (disposableContext == null)
					{
						Success = false;
						LogFailure(logger, (NoResString)"DataContext Company was specified, however there were no matching companies with at least one active branch in the system.");
					}
				}
			}
		}

		readonly IDisposable disposableContext;

		public static bool IncomingContextMatchesRegistrationKey(EnterpriseServerAndCompanyID contextInfo, IXmlImportLogger logger)
		{
			var result = true;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			if (!contextInfo.ServerID.IsEmpty)
			{
				if (registrationKey.ServerCode != contextInfo.ServerID)
				{
					result = false;
					LogFailure(logger, (NoResString)"DataContext ServerId was specified, however it does not match the system ServerId.");
				}
			}

			if (!contextInfo.EnterpriseID.IsEmpty)
			{
				if (registrationKey.EnterpriseCode != contextInfo.EnterpriseID)
				{
					result = false;
					LogFailure(logger, (NoResString)"DataContext EnterpriseId was specified, however it does not match the system EnterpriseId.");
				}
			}
			return result;
		}

		static IDisposable GetCompanyContext(ZString companyCodeToImportInto)
		{
			return Env.CurrentCompany.Code != companyCodeToImportInto
				? DisposableEnvironment.ForCompany(companyCodeToImportInto, reportInactive: false)
				: new DisposableAction(() => { });
		}

		bool IsCompanyContextSufficient(EnterpriseServerAndCompanyID contextInfo, IXmlImportLogger logger)
		{
			if (!contextInfo.CompanyCode.IsEmpty)
			{
				if (contextInfo.EnterpriseID.IsEmpty || contextInfo.ServerID.IsEmpty)
				{
					LogFailure(logger, (NoResString)"Company was specified, however EnterpriseId or ServerId were not specified. You should specify EnterpriseId, ServerId and Company if you want to map to a particular Company.");
					Success = false;
					return false;
				}
			}

			return !contextInfo.CompanyCode.IsEmpty && !contextInfo.EnterpriseID.IsEmpty && !contextInfo.ServerID.IsEmpty;
		}

		static void LogFailure(IXmlImportLogger logger, string errorMessage)
		{
			logger.Log(LogType.Error, errorMessage);
		}

		public bool Success { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ContextSwitching()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				if (disposing)
				{
					disposableContext?.Dispose();
					DisposableLeakListenerWrapper.UnRegisterDisposable(this);
					isDisposed = true;
				}
			}
		}
		bool isDisposed;
	}
}
