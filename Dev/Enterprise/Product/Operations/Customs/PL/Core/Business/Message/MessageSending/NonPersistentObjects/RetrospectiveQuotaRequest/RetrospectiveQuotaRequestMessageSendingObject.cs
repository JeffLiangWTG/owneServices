using Enterprise.ZArchitecture.Core;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.PL.Business;

public class RetrospectiveQuotaRequestMessageSendingObject : BaseMessageSendingObject
{
	public RetrospectiveQuotaRequestMessageSendingObject(CusEntryHeader header) : base(header)
	{
	}

	public static class PLRetrospectiveSchema
	{
		#region SuppressResourceStringsCheckRegion

		public const string ActionType = "ZCX05";
		public const string ActionDescription = "Retrospective Quota Request";

		#endregion
	}

	protected override void SetMessageSendingObjectDefaultValues()
	{
		base.SetMessageSendingObjectDefaultValues();
		Action = PLRetrospectiveSchema.ActionType;
	}

	public override CodeDescriptionPairList ActionList => Factory.GetCachedValue("PLRetrospectiveQuotaRequestMessageSendingObject|ActionList", () =>
		new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(PLRetrospectiveSchema.ActionType, PLRetrospectiveSchema.ActionDescription)
		});

	protected override int EntryNumberMaxLength => 18;

	public new RetrospectiveQuotaRequestMessageSendingObjectValidation Validation => (RetrospectiveQuotaRequestMessageSendingObjectValidation)base.Validation;

	protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() =>
		new RetrospectiveQuotaRequestMessageSendingObjectValidation(this);
}
