using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

sealed class TestDataPoint : IDataPoint
{
	public long hjid { get; set; }

	public metainfo metainfo { get; set; }

	public override string ToString()
	{
		return $"{nameof(hjid)}: {hjid}, " +
				$"{nameof(metainfo.opType)}: {metainfo?.opType}, " +
				$"{nameof(metainfo.origin)}: {metainfo?.origin}, " +
				$"{nameof(metainfo.status)}: {metainfo?.status}, " +
				$"{nameof(metainfo.transactionDate)}: {metainfo?.transactionDate}";
	}
}
