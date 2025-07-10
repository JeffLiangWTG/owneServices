using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondContainerDataObjectWriter : DataTransfer.Universal.CusInBondContainerDataObjectWriter
	{
		public CusInBondContainerDataObjectWriter(IDataWritingManager manager, InBondDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected new InBondDataObjectWriterHelper Helper
		{
			get { return (InBondDataObjectWriterHelper)base.Helper; }
		}
		protected override void PopulateInBondSpecificData(Customs.Business.CusInBondContainer containerBO, Container containerData)
		{
			base.PopulateInBondSpecificData(containerBO, containerData);
			var inBondContainerBO = (CusInBondContainer)containerBO;
			Helper.PopulateDispositions(inBondContainerBO, containerData);
		}
	}
}
