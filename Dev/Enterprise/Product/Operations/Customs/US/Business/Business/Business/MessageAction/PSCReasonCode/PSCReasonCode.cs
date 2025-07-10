using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PSCReasonCode : AutoPSCReasonCode
	{
		public PSCReasonCode(CusEntryHeader entry, PSCReasonCodeCollection parentCollection)
			: base(entry.Factory)
		{
			this.entry = entry;
			this.ParentCollection = parentCollection;
		}
		public readonly CusEntryHeader entry;
		public readonly PSCReasonCodeCollection ParentCollection;

		[List(nameof(Lookups) + "." + nameof(PSCReasonCodeLookups.ParentTypeIndicatorList))]
		public override ZString ParentTypeIndicator
		{
			get { return base.ParentTypeIndicator; }
			set
			{
				base.ParentTypeIndicator = value;

				if (ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.Entry)
				{
					LineNumber = ZString.Empty;
				}
			}
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsLineNumberIrrelevant))]
		public override ZString LineNumber
		{
			get { return base.LineNumber; }
			set
			{
				if (!value.IsEmpty)
				{
					value = value.PadLeft(CusEntryLine.EntryLineNumberLength, '0');
				}
				base.LineNumber = value;
				EntryLineDescriptionInfo.RefreshBinding();
			}
		}

		bool IsLineNumberIrrelevant
		{
			get { return ParentTypeIndicator != PSCReasonCodeParentTypeList.Codes.EntryLine; }
		}

		[List(nameof(Lookups) + "." + nameof(PSCReasonCodeLookups.ReasonCodeList))]
		public override ZString Reason1
		{
			get { return base.Reason1; }
			set { base.Reason1 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(PSCReasonCodeLookups.ReasonCodeList))]
		public override ZString Reason2
		{
			get { return base.Reason2; }
			set { base.Reason2 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(PSCReasonCodeLookups.ReasonCodeList))]
		public override ZString Reason3
		{
			get { return base.Reason3; }
			set { base.Reason3 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(PSCReasonCodeLookups.ReasonCodeList))]
		public override ZString Reason4
		{
			get { return base.Reason4; }
			set { base.Reason4 = value; }
		}

		[List(nameof(Lookups) + "." + nameof(PSCReasonCodeLookups.ReasonCodeList))]
		public override ZString Reason5
		{
			get { return base.Reason5; }
			set { base.Reason5 = value; }
		}

		internal IEnumerable<ZString> GetReasonCodes()
		{
			if (!Reason1.IsEmpty)
			{
				yield return Reason1;
			}

			if (!Reason2.IsEmpty)
			{
				yield return Reason2;
			}

			if (!Reason3.IsEmpty)
			{
				yield return Reason3;
			}

			if (!Reason4.IsEmpty)
			{
				yield return Reason4;
			}

			if (!Reason5.IsEmpty)
			{
				yield return Reason5;
			}
		}

		[MaxLength(1000)]
		public ZString EntryLineDescription
		{
			get
			{
				CusEntryLine entryLine = null;

				if (ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.EntryLine)
				{
					entryLine = entry.MergedLines.FindByFormattedLineNumber(LineNumber);

					if (entryLine != null)
					{
						entryLine = entryLine.ParentLine ?? entryLine;
					}
				}

				return entryLine != null ? entryLine.EntryLineDescription : new ZString("N/R");
			}
		}

		public ZPropertyInfo EntryLineDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(EntryLineDescription)); }
		}

		public PSCReasonCodeLookups Lookups
		{
			get { return lookups ?? (lookups = new PSCReasonCodeLookups(this)); }
		}
		PSCReasonCodeLookups lookups;

		public void CopyPSCReasonCodes()
		{
			var pscReasonCodes = PSCReasonCodes;
			if (!pscReasonCodes.IsEmpty)
			{
				var cusCodeData = CreateNewPSCReasonCusCodeDataIfThereIsNoneToUpdate();
				if (cusCodeData != null)
				{
					cusCodeData.CY_Data = pscReasonCodes;
				}
			}
		}

		ZString PSCReasonCodes
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(Reason1);
				builder.AppendIfNotEmpty(Reason2);
				builder.AppendIfNotEmpty(Reason3);
				builder.AppendIfNotEmpty(Reason4);
				builder.AppendIfNotEmpty(Reason5);
				return builder.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		PSCReasonCusCodeData CreateNewPSCReasonCusCodeDataIfThereIsNoneToUpdate()
		{
			PSCReasonCusCodeData cusCodeData = null;
			if (ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.Entry)
			{
				cusCodeData = entry.PSCReasonCodes;
				if (cusCodeData == null)
				{
					cusCodeData = Factory.New<PSCReasonCusCodeData>();
					cusCodeData.Parent = entry;
				}
			}
			else if (ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.EntryLine)
			{
				CusEntryLine entryLine = entry.MergedLines.FindByFormattedLineNumber(LineNumber);
				if (entryLine != null)
				{
					cusCodeData = entryLine.PSCReasonCodes;
					if (cusCodeData == null)
					{
						cusCodeData = Factory.New<PSCReasonCusCodeData>();
						cusCodeData.Parent = entryLine;
					}
				}
			}
			return cusCodeData;
		}
	}
}
