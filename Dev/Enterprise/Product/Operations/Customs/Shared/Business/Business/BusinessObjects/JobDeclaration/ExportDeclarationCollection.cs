using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// This is for module grid.
	/// </summary>
	public class ExportDeclarationCollection : BusinessObjectCollection<BaseJobDeclaration>
	{
		public ExportDeclarationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ExportDeclarationCollection(BusinessObjectFactory factory, BaseJobDeclaration parentDeclaration)
			: base(factory, GetExportDeclarations())
		{
			this.parentDeclaration = parentDeclaration;
			DefaultModuleFilterFields();
		}
		readonly BaseJobDeclaration parentDeclaration;

		static ZQuery GetExportDeclarations()
		{
			ZQuery result = new ZQuery(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);
			result.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.ExportDeclarationByExternalBroker);
			return result;
		}

		public void DefaultModuleFilterFields()
		{
			if (parentDeclaration != null)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Shipment Type", "Property", (ZString)JobMessageTypeList.Codes.Export));
			}
		}
	}
}
