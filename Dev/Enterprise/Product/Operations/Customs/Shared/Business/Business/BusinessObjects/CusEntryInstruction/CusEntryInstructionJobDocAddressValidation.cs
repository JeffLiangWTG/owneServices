using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusEntryInstructionJobDocAddressValidation : JobDocAddressValidation
	{
		public CusEntryInstructionJobDocAddressValidation(JobDocAddress address, CusEntryInstruction entryInstruction) : base(address) { }
	}
}
