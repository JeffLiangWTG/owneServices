using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusContainerEntryInstructionPiovtCollection : DependentBusinessObjectCollection<CusContainerEntryInstructionPivot, CusEntryInstruction>
	{
		public CusContainerEntryInstructionPiovtCollection(CusEntryInstruction instruction)
			: base(instruction)
		{
		}

		public CusContainerEntryInstructionPivot AddPivotFor(BaseCusContainer container)
		{
			var result = GetRelatedPivot(container);
			if (result == null)
			{
				result = AddNew();
				result.CEP_CO_Container = container.PK;
				result.CEP_CEI_EntryInstruction = Master.PK;
			}
			return result;
		}

		public void DeletePivotFor(BaseCusContainer container)
		{
			GetRelatedPivot(container)?.Delete();
		}

		public CusContainerEntryInstructionPivot GetRelatedPivot(BaseCusContainer container)
		{
			return this.Cast<CusContainerEntryInstructionPivot>().FirstOrDefault(p => p.CEP_CO_Container == container.PK);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusContainerEntryInstructionPivotSchema.CEP_CEI_EntryInstruction; }
		}
	}
}
