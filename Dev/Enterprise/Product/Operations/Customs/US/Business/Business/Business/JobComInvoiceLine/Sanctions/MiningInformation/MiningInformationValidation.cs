using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class MiningInformationValidation : CusCodeDataValidation
	{
		public MiningInformationValidation(MiningInformation parent)
			: base(parent)
		{
		}

		#region CheckCY_Code

		protected override void CheckCY_Code()
		{
		}

		#endregion

		#region CheckCY_Data

		protected override void CheckCY_Data()
		{
			if (Parent is MiningInformation parent)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CY_DataInfo, parent.Lookups.Countries);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(parent.CY_DataInfo);
			}
		}

		#endregion
	}
}
