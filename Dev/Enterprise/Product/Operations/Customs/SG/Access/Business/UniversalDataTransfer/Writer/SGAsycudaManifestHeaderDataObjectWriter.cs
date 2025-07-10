using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class SGAsycudaManifestHeaderDataObjectWriter : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeader>
	{
		public SGAsycudaManifestHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriterHelper CreateAsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header)
		{
			return new SGAsycudaManifestHeaderDataObjectWriterHelper(header);
		}
	}
}
