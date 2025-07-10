using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using Country = Enterprise.Core.Constants.CountryCodes;
using Functionality = Enterprise.Customs.Universal.Constants.FunctionalityTypes;
using RefCusCodeListType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.PL.ServiceTasks;

public static class PLServiceTaskHelper
{
	public static string CheckCertificate()
	{
		return CertificateRequirementChecker.ExistsCompanyWithCertificate(Country.Poland, PasswordTypesList.Codes.PLB, PasswordStatusList.Codes.Valid)
			? null : (NoResString)"There is no Token Credential / Certificate configured in Poland.";
	}

	public static void DeactivateServiceIf_UCMP_IsActive(BusinessObjectFactory factory, string serviceTaskCode, ILogger logger)
	{
		var query = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
		query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, Country.Poland);
		query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, RefCusCodeListType.FUNCS);
		query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, Functionality.UCMPServiceTask);
		query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Now);
		query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Now);
		if (!factory.Exists(typeof(ZZRefCusCodeListCombined), query))
		{
			return;
		}

		var serviceManagerGovernor = ObjectFactory.Get<IServiceManagerGovernor>();
		serviceManagerGovernor.SetServiceTaskIsActive(serviceTaskCode, isActive: false);
		throw new OperationCanceledException((NoResString)$"UCMP functionality is active. Service {serviceTaskCode} was disabled.");
	}
}
