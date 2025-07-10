using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.MarketingManager.Integration
{
	public interface IValueAnalysisModuleHelper
	{
		void AddValueAnalysisModuleFilterStrips(SchemaGuidColumn primaryKeyColumn, IModuleFilterCollection filterCollection, BusinessObjectFactory factory, Type parentBusinessObjectType);
	}
}
