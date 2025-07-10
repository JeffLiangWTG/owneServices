using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class AddInfoGenAddOnColumnSynchroniser
	{
		public AddInfoGenAddOnColumnSynchroniser(BusinessObject addInfoParent, BusinessObject addInfo)
		{
			this.addInfo = addInfo;
			this.addInfoParent = addInfoParent;
		}

		readonly BusinessObject addInfo;
		readonly BusinessObject addInfoParent;

		public void Synchronise(SchemaColumn[] columnsToSync)
		{
			Synchronise(columnsToSync.Select(x => (x.Name, (IZType)addInfo[x])));
		}

		public void Synchronise(IEnumerable<(string genAddOnColumnName, IZType addInfoValue)> columnsToSync)
		{
			foreach ((string genAddOnColumnName, IZType addInfoValue) in columnsToSync)
			{
#if DEBUG
				if (genAddOnColumnName.Length > GenAddOnColumnSchema.XA_Name.MaxLength)
				{
					ErrorReporter.ReportOnce($"{addInfo.GetType().FullName}.{genAddOnColumnName}", $"It is specified to be persisted into GenAddOnColumn, however the name '{genAddOnColumnName}' is too long for XA_Name");
				}
#endif
				addInfoParent.SetSystemDefinedValue(genAddOnColumnName, null, addInfoValue);
			}
		}
	}
}
