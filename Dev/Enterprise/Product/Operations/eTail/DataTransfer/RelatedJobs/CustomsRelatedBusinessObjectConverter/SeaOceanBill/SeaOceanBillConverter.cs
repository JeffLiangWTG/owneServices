using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class SeaOceanBillConverter : CustomsRelatedBusinessObjectConverter
	{
		public SeaOceanBillConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override RecipientRoleType? RecipientRoleType => UniversalDataBuss.Integration.RecipientRoleType.HSA;

		public override DataContextType MasterBillDataContextType => DataContextType.SeaOceanBill;
	}
}
