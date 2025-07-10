namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

	interface IMAFFile
	{
		ZString FileName { get; }
		ZString ContentType { get; } // O "PDF"
		ZString DocumentType { get; }
		MessagingRequestTypeBodyFileDataDataEncoding DataEncoding { get; } // M "Base64"
		ZString DataContent { get; } // M - Actual content of the file encoded Base64
	}
}
