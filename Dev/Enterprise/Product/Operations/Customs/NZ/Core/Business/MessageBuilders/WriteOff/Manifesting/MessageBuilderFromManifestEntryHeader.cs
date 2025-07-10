using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting
{
	public class MessageBuilderFromManifestEntryHeader : MessageBuilderFromEntryHeader
	{
		public MessageBuilderFromManifestEntryHeader(CusEntryHeader entryHeader, ECIMessageGenerator.MessageTypes messageType)
			: base(entryHeader, messageType)
		{
			this.entryHeader = entryHeader;
		}
		readonly CusEntryHeader entryHeader;

		#region SetParentMessagingStatusAfterMessagePosting
		protected override void SetParentMessagingStatusAfterMessagePosting()
		{
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			foreach (JobDeclaration declaration in entryHeader.Declarations)
			{
				declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
				declaration.JE_EDITransmitDate = entryHeader.Declaration.CachedTodaysDate;
				declaration.JE_EntrySubmittedDate = entryHeader.Declaration.CachedTodaysDate;
				declaration.LogCustomsCommencedIfNeeded();
			}
		}
		#endregion

		#region GenerateGroup7ForAllConsignments
		protected override void GenerateGroup7ForAllConsignments(ECIMessageGenerator generator)
		{
			foreach (JobDeclaration declaration in entryHeader.Declarations.OrderBy(x => ((JobDeclaration)x).ECIManifestLineNumber.PadLeft(5, '0')))
			{
				generator.GenerateGroup7(declaration, declaration.ECIManifestLineNumber);
			}
		}
		#endregion

		#region GetConsignmentCount
		protected override ZInt GetConsignmentCount()
		{
			return entryHeader.Declarations.Count;
		}
		#endregion
	}
}
