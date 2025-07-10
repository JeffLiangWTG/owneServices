using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusEntryInstructionCollection<out TCusEntryInstruction> : IBusinessObjectCollection<TCusEntryInstruction>
	where TCusEntryInstruction : CusEntryInstruction
	{
		new TCusEntryInstruction this[int index] { get; }
		new TCusEntryInstruction AddNew();
	}

	public class CusEntryInstructionCollection<TCusEntryInstruction> : DependentBusinessObjectCollection<TCusEntryInstruction, BaseJobDeclaration>, ICusEntryInstructionCollection<TCusEntryInstruction>
		where TCusEntryInstruction : CusEntryInstruction
	{
		public CusEntryInstructionCollection(BaseJobDeclaration master) : base(master)
		{
		}

		public IEnumerator<TCusEntryInstruction> GetEnumerator() => Elements.Cast<TCusEntryInstruction>().GetEnumerator();

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusEntryInstructionSchema.CEI_JE;
	}
}
