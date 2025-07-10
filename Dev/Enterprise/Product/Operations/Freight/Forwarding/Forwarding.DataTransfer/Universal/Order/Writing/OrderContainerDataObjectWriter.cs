using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderContainerDataObjectWriter<T> : DataObjectWriter<T, Container>
		where T : AutoJobOrderContainer
	{
		public OrderContainerDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		#region PopulateDataObject

		protected override Container PopulateDataObject(T containerBO)
		{
			var container = new Container(writeManager.WriterStrategy);
			container.ContainerCount = containerBO.J1_ContainerCount;
			container.ContainerNumber = containerBO.J1_ContainerNumber;
			container.ContainerType = ContainerType.New(containerBO.Container);
			container.Seal = containerBO.J1_SealNum;
			container.SecondSeal = containerBO.J1_AdditionalSealNum;
			container.ThirdSeal = containerBO.J1_Additional2SealNum;

			return container;
		}

		#endregion
	}
}

