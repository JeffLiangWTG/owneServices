using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CusInBondContainerDataObjectWriter : DataObjectWriter<CusInBondContainer, Container>
	{
		public CusInBondContainerDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper)
			: base(writeManager)
		{
			this.writerHelper = Argument.NotNull(helper, "helper");
		}
		readonly InBondDataObjectWriterHelper writerHelper;

		protected InBondDataObjectWriterHelper Helper
		{
			get { return writerHelper; }
		}

		protected override Container PopulateDataObject(CusInBondContainer containerBO)
		{
			var containerData = new Container(writeManager.WriterStrategy);
			containerData.ContainerNumber = containerBO.BC_ContainerNum;
			containerData.Seal = containerBO.BC_Seal1;
			containerData.SecondSeal = containerBO.BC_Seal2;
			containerData.ContainerType = ContainerType.New(containerBO.Container);
			containerData.SetUNDGCollection(() => ProcessCollection(Helper.Load<UNDGDataItem>(containerBO.UNDGs.CompleteFilter).OrderBy(x => x.UNDGSubstance?.DG_Code), new UNDGDataObjectWriter(writeManager)));
			Helper.AllocateContainerLink(containerBO, containerData);
			PopulateInBondSpecificData(containerBO, containerData);
			return containerData;
		}

		protected virtual void PopulateInBondSpecificData(CusInBondContainer containerBO, Container containerData)
		{
		}
	}
}
