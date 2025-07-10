using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public interface IDeliveryStreamWrapper
	{
		abstract void PopulateStream(BusinessObjectFactory factory);

		SubStreamableStream GetMessageStream();

		void SetMessageNumber(MessageNumberType messageNumberType, ZString zString);

		void SetTimestamp(long timestamp);

		SubStreamableStream Content { get; set; }

		IEntityInfo SourceInfo { get; }
	}
}
