using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class EntryInstructionsCoreDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, EntryInstructionBasicDetailsControlBag> where T : CusEntryInstruction
	{
		public override EntryInstructionBasicDetailsControlBag CommonBag { get; } = EntryInstructionBasicDetailsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
