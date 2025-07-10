using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business;

public sealed class NonPersistentRuleValidationResultCollection : NonPersistentBusinessObjectCollection<NonPersistentRuleValidationResult>
{
	readonly bool isFailureOnly;

	public NonPersistentRuleValidationResultCollection(BusinessObjectFactory factory, bool isFailureOnly = false) : base(factory)
	{
		this.isFailureOnly = isFailureOnly;
	}

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;

	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException();

	public IEnumerable<NonPersistentRuleValidationResult> MessageErrors => Find(x => x.Severity == ProcessTemplateValidationSeverityList.Codes.Message);

	public IEnumerable<NonPersistentRuleValidationResult> Warnings => Find(x => x.Severity == ProcessTemplateValidationSeverityList.Codes.Warning);

	public IEnumerable<NonPersistentRuleValidationResult> Errors => Find(x => x.Severity == ProcessTemplateValidationSeverityList.Codes.Error);

	public override void Add(BusinessObject businessObject)
	{
		if (isFailureOnly)
		{
			var validationResult = (NonPersistentRuleValidationResult)businessObject;
			if (validationResult.Passed)
			{
				throw new ArgumentException("Please enter a failed result", nameof(businessObject));
			}
		}
		base.Add(businessObject);
	}
}
