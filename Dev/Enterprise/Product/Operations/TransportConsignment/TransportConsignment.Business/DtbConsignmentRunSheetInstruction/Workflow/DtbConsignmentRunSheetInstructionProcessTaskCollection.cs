using System.Diagnostics.CodeAnalysis;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentRunSheetInstructionProcessTaskCollection : ProcessTaskCollection
	{
		public DtbConsignmentRunSheetInstructionProcessTaskCollection(DtbConsignmentRunSheetInstruction instruction)
			: base(instruction)
		{
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public new DtbConsignmentRunSheetInstructionProcessTask this[int index]
		{
			get { return (DtbConsignmentRunSheetInstructionProcessTask)Elements[index]; }
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public new DtbConsignmentRunSheetInstructionProcessTask AddNew()
		{
			return (DtbConsignmentRunSheetInstructionProcessTask)base.AddNew();
		}
	}
}
