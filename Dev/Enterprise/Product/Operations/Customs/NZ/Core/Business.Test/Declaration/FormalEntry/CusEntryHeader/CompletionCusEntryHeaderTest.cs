using Enterprise.Customs.Common.NZ;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.FormalEntry.Testing
{
	[TestedType(typeof(CompletionCusEntryHeader))]
	public class CompletionCusEntryHeaderTest : CusEntryHeaderTest<CompletionCusEntryHeader>
	{
		#region Implementation
		protected override string ExpectedCusEntryNumType
		{
			get { return CusEntryNumberTypeList.Codes.CompletionEntry; }
		}

		protected override string ExpectedCusEntryHeaderType
		{
			get { return CusEntryHeader.EntryHeaderTypes.NZ.Completion; }
		}

		protected new CompletionCusEntryHeader EntryHeader
		{
			get { return base.EntryHeader; }
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			return declaration;
		}
		#endregion
	}
}
