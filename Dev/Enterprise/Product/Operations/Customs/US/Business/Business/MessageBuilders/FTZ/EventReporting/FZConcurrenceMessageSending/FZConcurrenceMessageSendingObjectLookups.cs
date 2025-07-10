using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZConcurrenceMessageSendingObjectLookups : ZLookups
	{
		public FZConcurrenceMessageSendingObjectLookups(FZConcurrenceMessageSendingObject parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList UQ_List => RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
	}
}
