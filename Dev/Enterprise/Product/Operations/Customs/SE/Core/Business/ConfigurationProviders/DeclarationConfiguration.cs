using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SE.Business.Declaration;
public class DeclarationConfiguration : EU.Business.DeclarationConfiguration
{
	protected override ZBool IsUCC6Core(BusinessObject businessObject) => businessObject is JobDeclaration jobDeclaration && (jobDeclaration.IsExport || jobDeclaration.IsImport);
}
