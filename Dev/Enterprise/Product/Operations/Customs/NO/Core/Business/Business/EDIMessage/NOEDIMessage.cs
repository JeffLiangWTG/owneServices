using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NO.Business;

public class NOEDIMessage : EDIMessage, Integration.Customs.NO.INOMessage, ICusCodeDataTypeSupporter
{
	public NOEDIMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public const string JobReferenceNumberPlaceHolder = $"<<JOB REFERENCE NUMBER PLACE HOLDER>>";

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodeList.Codes.NOCustoms;
	}

	internal IEDIMessagePrettier Prettier => prettier ??= GetNewPrettier();
	IEDIMessagePrettier prettier;

	protected virtual IEDIMessagePrettier GetNewPrettier() => new NOEDIMessagePrettier(this);

	public new static readonly TypeDecider TypeDecider = new NOEDIMessageTypeDecider();

	public IDictionary<ZString, Type> GetCusCodeDataTypes() => new Dictionary<ZString, Type>();

	public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

	protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
	{
		yield return new CusCodeDataTypeSupporterFetchStrategy(this);
	}

	public override ZString EM_MessageInterpretation
	{
		get => !EM_MessageText.IsEmpty ? Prettier.MakeHumanReadable() : base.EM_MessageInterpretation;
		set
		{
			if (!IsMessageInterpretationSetterSupported && !EM_MessageText.IsEmpty)
			{
				throw new NotSupportedException("Setting EM_MessageInterpretation is not supported.");
			}
			base.EM_MessageInterpretation = value;
		}
	}

	public virtual ZBool IsMessageInterpretationSetterSupported => true;

	protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
	{
		base.GetNumberFountainNumbersAndFillInPlaceHolders();
		if (EM_MessageText.IndexOf(JobReferenceNumberPlaceHolder, StringComparison.InvariantCulture) != -1)
		{
			EM_MessageText = EM_MessageText.Replace(JobReferenceNumberPlaceHolder, GetJobReferenceNumber());
		}
	}

	string GetJobReferenceNumber()
	{
		if (EM_LinkedObject is CusEntryHeader entryHeader)
		{
			return entryHeader.CH_BGMReference;
		}

		throw new ArgumentException("CusEntryHeader expected as Linked Object on an NOMessage");
	}

	protected override string GetMessageReferenceNumber()
	{
		return ZDateTime.Now.Ticks.ToString();
	}
}
