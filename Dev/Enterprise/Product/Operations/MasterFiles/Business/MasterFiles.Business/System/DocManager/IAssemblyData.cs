namespace Enterprise.ZArchitecture.Modules.DocumentScanning
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public interface IAssemblyData
	{
		IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory);
		IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams);
		ZQuery GetQuery(AssemblyDataParams assemblyDataParams);
		IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport();
		Type BusinessObjectType { get; }
		string DocManagerCode { get; }
		string HumanReadableName { get; }
		bool IsAllowedForUnallocatedeDocs { get; }
		ModuleIdentifier ModuleID { get; }
		string ReferenceType { get; }
		bool AllowLookupOfBizOFromPk { get; }
		ZString GetFriendlyName(BusinessObject businessObject);
	}
}
