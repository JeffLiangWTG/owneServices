using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ContainerCusContainerEntryHeaderPivotCollection : DependentBusinessObjectCollection<CusContainerEntryHeaderPivot, BaseCusContainer>
	{
		public ContainerCusContainerEntryHeaderPivotCollection(BaseCusContainer container)
			: base(container)
		{
		}

		public CusContainerEntryHeaderPivot GetOrCreatePivotFor(CusEntryHeader entry)
		{
			var result = this.Cast<CusContainerEntryHeaderPivot>().FirstOrDefault(x => x.CCE_CH_EntryHeader == entry.PK);
			if (result == null)
			{
				result = AddNew();
				result.CCE_CH_EntryHeader = entry.PK;
			}
			return result;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusContainerEntryHeaderPivotSchema.CCE_CO_Container;
	}
}
