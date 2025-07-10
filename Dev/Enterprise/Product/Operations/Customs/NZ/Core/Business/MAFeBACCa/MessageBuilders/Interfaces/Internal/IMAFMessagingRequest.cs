namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

	interface IMAFMessagingRequest
	{
		ZString ApplicationName { get; } // EBACCA
		ZString ApplicationVersion { get; } // 1.0.0
		ZString DocumentType { get; } // EBACCA
		ZString SenderName { get; } // ECN
		EndPointTypeEndPointType SenderEndPointType { get; } // Email
		ZString SenderAddress { get; } // A generic actual address of the sender

		ZString CallerRefID { get; }

		IMAFMessagingMetaData MetaData { get; }
		IEnumerable<IMAFFile> Files { get; }
	}
}
