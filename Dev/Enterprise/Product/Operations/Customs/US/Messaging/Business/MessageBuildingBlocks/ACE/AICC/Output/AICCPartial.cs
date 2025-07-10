using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.AddNew5106toImporterFileProcessingResults)]
	public partial class AICCE0 : IExtendedFieldsHumanFriendlySerialiserSupporter
	{
		ZString IExtendedFieldsHumanFriendlySerialiserSupporter.ExtendedFieldNameToSerialise
		{
			get { return nameof(ReferenceDataText); }
		}

		Dictionary<ZString, Dictionary<ZString, MessageBlockAttribute>> IExtendedFieldsHumanFriendlySerialiserSupporter.GetExtendedFieldsMappings(ZString extendedFieldName)
		{
			var result = new Dictionary<ZString, Dictionary<ZString, MessageBlockAttribute>>();
			switch (ReferenceDataTypeCode)
			{
				case ImporterConsigneeCreateUpdateReferenceDataTypeList.Codes.IMPACC:
					result.Add(extendedFieldName, GetExtendedFieldsMapping_IMPACC());
					break;
			}

			return result;
		}

		Dictionary<ZString, MessageBlockAttribute> GetExtendedFieldsMapping_IMPACC()
		{
			var result = new Dictionary<ZString, MessageBlockAttribute>
			{
				{ "ImporterNumber", new MessageBlockStringAttribute(12, 26, "C") },
				{ "AbbreviatedImporterName", new MessageBlockStringAttribute(32, 38, "C") }
			};

			return result;
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.AddNew5106toImporterFileProcessingResults)]
	public partial class AICCE1
	{
	}
}
