using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenCustomColumnDefinitionCollection : ActiveBusinessObjectCollection<GenCustomColumnDefinition>
	{
		public GenCustomColumnDefinitionCollection(BusinessObject master)
			: base(master.Factory, master, new ZQuery(), GenCustomColumnDefinitionSchema.XC_ParentID)
		{
		}
	}
}
