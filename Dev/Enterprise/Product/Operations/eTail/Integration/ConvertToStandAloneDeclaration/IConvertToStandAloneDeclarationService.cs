using System;
using CargoWise.EntityFramework;

namespace Enterprise.eTail.Integration
{
	public interface IConvertToStandAloneDeclarationService
	{
		IConvertToStandAloneDeclarationResponse ConvertToStandAloneDeclaration(Guid consignmentPK, BusinessObjectFactory factory);

		IConvertToStandAloneDeclarationResponse ConvertToStandAloneDeclaration(IHVLVConsignment consignment);
	}
}
