using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class USLowValueConverter : CustomsRelatedBusinessObjectConverter
	{
		public USLowValueConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override RecipientRoleType? RecipientRoleType
		{
			get
			{
				RecipientRoleType? result = null;
				if (Shipment.IsSea)
				{
					result = UniversalDataBuss.Integration.RecipientRoleType.HSA;
				}
				else if (Shipment.IsAir)
				{
					result = UniversalDataBuss.Integration.RecipientRoleType.HCA;
				}

				return result;
			}
		}

		public override DataContextType MasterBillDataContextType => DataContextType.USCustomsLowValueEntriesClearance;
	}
}
