using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IExtendedFieldsHumanFriendlySerialiserSupporter
	{
		ZString ExtendedFieldNameToSerialise { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Dictionary<ZString, Dictionary<ZString, MessageBlockAttribute>> GetExtendedFieldsMappings(ZString extendedFieldName);
	}
}
