using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class EntryCusContainerEntryHeaderPivotCollection : DependentBusinessObjectCollection<CusContainerEntryHeaderPivot, CusEntryHeader>
	{
		public EntryCusContainerEntryHeaderPivotCollection(CusEntryHeader entry)
			: base(entry)
		{
		}

		public CusContainerEntryHeaderPivot GetOrCreatePivotFor(BaseCusContainer container)
		{
			var result = this.Cast<CusContainerEntryHeaderPivot>().FirstOrDefault(x => x.CCE_CO_Container == container.PK);
			if (result == null)
			{
				result = AddNew();
				result.CCE_CO_Container = container.PK;
			}
			return result;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusContainerEntryHeaderPivotSchema.CCE_CH_EntryHeader;
	}
}
