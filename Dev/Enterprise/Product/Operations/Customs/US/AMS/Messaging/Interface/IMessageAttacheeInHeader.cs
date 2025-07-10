using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public enum MessageAttacheeRecordType { VesselMovement, PermitToTransferMovement, InBondMovement }

	public interface IMessageAttacheeInHeader : IMessageAttachee
	{
		ZString RecordIdentifier { get; }
		MessageAttacheeRecordType RecordType { get; }
		ZString RecordTypeDescription { get; }
		ZGuid HeaderPK { get; }
		ZGuid PK { get; }
	}

	public interface IMessageActionHeader
	{
		BusinessObjectFactory Factory { get; }
		IReadOnlyList<IMessageAttacheeInHeader> MessageAttachees { get; }
		void RebuildMessageAttacheesRelatedRecords(IMessageAttacheeInHeader messageAttachee);
	}
}
