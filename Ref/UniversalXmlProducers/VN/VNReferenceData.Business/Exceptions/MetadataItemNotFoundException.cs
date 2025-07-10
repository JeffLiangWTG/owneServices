using System;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public class MetadataItemNotFoundException : Exception
	{
		CodeListMetadata codeListMetadata;
		int metadataId;

		public MetadataItemNotFoundException(CodeListMetadata codeListMetadata, int metadataId)
			: base($"Cannot find metadata item with id {metadataId}")
		{
			this.codeListMetadata = codeListMetadata;
			this.metadataId = metadataId;
		}

		public MetadataItemNotFoundException()
		{
		}

		public MetadataItemNotFoundException(string message) : base(message)
		{
		}

		public MetadataItemNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
