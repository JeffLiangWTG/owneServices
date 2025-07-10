using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SE.Business.Declaration
{
	public class CusEntryInstruction : EU.Business.Declaration.CusEntryInstruction, Integration.Customs.SE.ICusEntryInstruction
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);
	}
}
