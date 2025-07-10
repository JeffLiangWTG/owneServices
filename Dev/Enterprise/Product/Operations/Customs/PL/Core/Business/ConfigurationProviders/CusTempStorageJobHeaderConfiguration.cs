using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.Business;

[CodeAlive("Used in CusTempStorageJobHeaderConfigurationTest")]
public class CusTempStorageJobHeaderConfiguration : EU.Business.CusTempStorageJobHeaderConfiguration
{
	protected override ZBool IsUCC6Core(BusinessObject businessObject) => true;
}
