using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusContainerEntryInstructionPivot : AutoCusContainerEntryInstructionPivot
	{
		public CusContainerEntryInstructionPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Container))]
		public override ZGuid CEP_CO_Container
		{
			get { return base.CEP_CO_Container; }
			set { base.CEP_CO_Container = value; }
		}

		[RelatedBusinessObject(nameof(EntryInstruction))]
		public override ZGuid CEP_CEI_EntryInstruction
		{
			get { return base.CEP_CEI_EntryInstruction; }
			set { base.CEP_CEI_EntryInstruction = value; }
		}

		public BaseCusContainer Container => Factory.Load<BaseCusContainer>(CEP_CO_Container);

		public CusEntryInstruction EntryInstruction => Factory.Load<CusEntryInstruction>(CEP_CEI_EntryInstruction);
	}
}
