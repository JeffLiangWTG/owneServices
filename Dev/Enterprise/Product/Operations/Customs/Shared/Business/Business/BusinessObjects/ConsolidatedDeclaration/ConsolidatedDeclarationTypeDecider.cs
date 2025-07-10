using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Business
{
	[Immutable]
	public class ConsolidatedDeclarationTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(ConsolidatedDeclaration);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string applicationCode = row[ConsolidatedDeclaration.Schema.CRD_ApplicationCode].ToString().Trim();
			return GetTypeForApplicationCode(applicationCode, row, factory);
		}

		public override Type GetTypeForNew()
		{
			return typeof(ConsolidatedDeclaration);
		}

		Type GetTypeForApplicationCode(string applicationCode, DataRow row, BusinessObjectFactory factory)
		{
			switch (applicationCode)
			{
				case ConsolidatedDeclaration.ApplicationCodes.TSW:
					return ObjectFactory.GetType<Integration.Customs.NZ.IConsolidatedDeclaration>();
				case ConsolidatedDeclaration.ApplicationCodes.CMR:
					return ObjectFactory.GetType<Integration.Customs.AU.IConsolidatedDeclaration>();
				default:
					return typeof(ConsolidatedDeclaration);
			}
		}
	}
}
