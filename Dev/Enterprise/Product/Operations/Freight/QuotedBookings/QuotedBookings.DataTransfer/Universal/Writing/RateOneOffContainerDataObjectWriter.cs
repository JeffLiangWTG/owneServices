using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	internal class RateOneOffContainerDataObjectWriter : DataObjectWriter<RateOneOffContainers, Container>
	{
		internal RateOneOffContainerDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override Container PopulateDataObject(RateOneOffContainers rateOneOffContainers)
		{
			var containerData = new Container(writeManager.WriterStrategy);
			containerData.ContainerCount = rateOneOffContainers.TC_ContainerCount;
			containerData.ContainerType = ContainerType.New(rateOneOffContainers.Container);

			return containerData;
		}
	}
}
