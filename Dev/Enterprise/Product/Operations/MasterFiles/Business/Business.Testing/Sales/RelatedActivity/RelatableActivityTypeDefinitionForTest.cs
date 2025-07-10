using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RelatableActivityTypeDefinitionForTest : RelatableActivityTypeDefinition
	{
		public RelatableActivityTypeDefinitionForTest(MultilingualString description, ModuleIdentifier moduleId, ControllerID controllerId, ZString tablePrefix, bool shouldUseElementTypeWhenLoading = false)
			: base(description, moduleId, controllerId, tablePrefix, shouldUseElementTypeWhenLoading)
		{
		}

		protected override FilterBusinessObject GetFilterBusinessObject(IZModule module)
		{
			var factory = new BusinessObjectFactory();
			var table = new DataTable();
			table.Columns.Add("PK");
			return new FilterBusinessObjectWithProblems(factory, table.NewRow());
		}
	}
}
