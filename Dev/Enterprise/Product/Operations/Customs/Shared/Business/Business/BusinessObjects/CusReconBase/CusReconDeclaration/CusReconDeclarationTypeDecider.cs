using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.CusReconBase
{
	public class CusReconDeclarationTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(CusReconDeclaration);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (ConsolidatedDeclaration.ApplicationCodes.IsConsolidatedDeclarationApplicationCode(((string)row[CusReconDeclaration.Schema.CRD_ApplicationCode])))
			{
				return new ConsolidatedDeclarationTypeDecider().GetTypeForLoad(row, factory);
			}
			else
			{
				return new Business.CusReconDeclarationTypeDecider().GetTypeForLoad(row, factory);
			}
		}

		public override Type GetTypeForNew()
		{
			return typeof(CusReconDeclaration);
		}
	}
}
