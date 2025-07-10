using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public static class ConfigurationHelper
{
	public static ZBool GetValueIndicatorsSupport(BusinessObject businessObject, Func<BusinessObject, ZBool> baseMethod) => businessObject is IImportExport importExport && importExport.IsImport()
		? ZBool.False
		: baseMethod(businessObject);
}
