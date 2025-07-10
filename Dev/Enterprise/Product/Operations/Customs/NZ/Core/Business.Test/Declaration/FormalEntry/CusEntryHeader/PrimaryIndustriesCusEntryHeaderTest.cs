using Enterprise.Customs.Common.NZ;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(PrimaryIndustriesCusEntryHeader))]
	public class PrimaryIndustriesCusEntryHeaderTest : CusEntryHeaderTest<PrimaryIndustriesCusEntryHeader>
	{
		#region Implementation
		protected override string ExpectedCusEntryNumType
		{
			get { return CusEntryNumberTypeList.Codes.PrimaryIndustriesEntry; }
		}

		protected override string ExpectedCusEntryHeaderType
		{
			get { return CusEntryHeader.EntryHeaderTypes.NZ.PrimaryIndustries; }
		}

		protected new PrimaryIndustriesCusEntryHeader EntryHeader
		{
			get { return base.EntryHeader; }
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			return declaration;
		}
		#endregion
	}
}
