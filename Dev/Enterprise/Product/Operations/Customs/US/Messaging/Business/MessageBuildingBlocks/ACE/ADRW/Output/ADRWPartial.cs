using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQueryResponse)]
	public partial class ADRWE0 : IExtendedFieldsHumanFriendlySerialiserSupporter
	{
		ZString IExtendedFieldsHumanFriendlySerialiserSupporter.ExtendedFieldNameToSerialise
		{
			get { return nameof(ReferenceDataText); }
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		Dictionary<ZString, Dictionary<ZString, MessageBlockAttribute>> IExtendedFieldsHumanFriendlySerialiserSupporter.GetExtendedFieldsMappings(ZString extendedFieldName)
		{
			var result = new Dictionary<ZString, Dictionary<ZString, MessageBlockAttribute>>();
			switch (ReferenceDataTypeCode)
			{
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.SUMMRY:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_Summry());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.BNDDTL:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_BNDDTL());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.IMPORT:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_IMPORT());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.CLASSI:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_CLASSI());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.QTYUOM:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_QTYUOM());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.HDRREV:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_HDRREV());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.MANUFD:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_MANUFD());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.EXPDES:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_EXPDES());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.NOIHDR:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_NOIHDR());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.NOIEWR:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_NOIEWR());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.NAFTAD:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_NAFTAD());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.TFTEAE:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_TFTEAE());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.TOTALS:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_TOTALS());
					break;
				case DrawbackReturnedEntrySummaryReferenceDataTypeList.Codes.REVTOT:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_REVTOT());
					break;
			}

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_Summry()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "EntryFilerCode", new MessageBlockStringAttribute(3, 26, "C") },
				{ "EntryNumber", new MessageBlockStringAttribute(8, 30, "C") },
				{ "BrokerReferenceNumber", new MessageBlockStringAttribute(12, 39, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_BNDDTL()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "BondTypeCode", new MessageBlockStringAttribute(1, 26, "C") },
				{ "BondDesignationTypeCode", new MessageBlockStringAttribute(1, 28, "C") },
				{ "SuretyCompanyCode", new MessageBlockStringAttribute(3, 30, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_IMPORT()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "ReleaseEntryFilerCode", new MessageBlockStringAttribute(3, 26, "C") },
				{ "ImportEntrySummaryNumber", new MessageBlockStringAttribute(8, 31, "C") },
				{ "ImportEntrySummaryLineNumber", new MessageBlockStringAttribute(5, 40, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_CLASSI()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "HTSNumber", new MessageBlockStringAttribute(10, 26, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_QTYUOM()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "UnitofMeasureCode", new MessageBlockStringAttribute(3, 26, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_HDRREV()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "AccountingClassCode", new MessageBlockStringAttribute(3, 26, "C") },
				{ "RevenueAmount", new MessageBlockStringAttribute(8, 30, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_MANUFD()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "HTSNumber", new MessageBlockStringAttribute(10, 26, "C") },
				{ "ManufacturingRulingNumber", new MessageBlockStringAttribute(12, 37, "C") },
				{ "ProductionDate", new MessageBlockDateAttribute(49, "C") },
				{ "MTIN", new MessageBlockStringAttribute(5, 55, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_EXPDES()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "ExportDestroy Indicator", new MessageBlockStringAttribute(1, 26, "C") },
				{ "HTSNumber", new MessageBlockStringAttribute(10, 27, "C") },
				{ "ExportDestroy Date", new MessageBlockDateAttribute(38, "C") },
				{ "UniqueIdentifier", new MessageBlockStringAttribute(28, 44, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_NOIHDR()
		{
			return new Dictionary<ZString, MessageBlockAttribute>();
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_NOIEWR()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "RecordIndicator", new MessageBlockStringAttribute(1, 26, "C") },
				{ "CBPPersonnelBadge", new MessageBlockStringAttribute(23, 27, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_NAFTAD()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "EntryNumber", new MessageBlockStringAttribute(20, 26, "C") },
				{ "EntryDate", new MessageBlockDateAttribute(46, "C") },
				{ "HTS 1", new MessageBlockStringAttribute(10, 52, "C") },
				{ "CountryofExport", new MessageBlockStringAttribute(2, 63, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_TFTEAE()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "ExportDestroyIndicator", new MessageBlockStringAttribute(1, 26, "C") },
				{ "HTSNumber", new MessageBlockStringAttribute(10, 27, "C") },
				{ "ExportDestroyDate", new MessageBlockDateAttribute(38, "C") },
				{ "UniqueIdentifier", new MessageBlockStringAttribute(28, 44, "C") }
			};

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_TOTALS()
		{
			return new Dictionary<ZString, MessageBlockAttribute>();
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_REVTOT()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "AccountingClassCode", new MessageBlockStringAttribute(3, 26, "C") },
				{ "TotalFeeAmount", new MessageBlockStringAttribute(11, 30, "C") }
			};

			return result;
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.DrawbackEntrySummaryQueryResponse)]
	public partial class ADRWE1 : MessageBlock
	{
	}
}
