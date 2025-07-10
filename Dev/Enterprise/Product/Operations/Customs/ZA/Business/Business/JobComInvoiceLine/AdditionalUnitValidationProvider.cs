using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class AdditionalUnitValidationProvider : ValidationProvider
	{
		public AdditionalUnitValidationProvider()
		{
		}

		public void ValidateDuplicateAdditionalUnit(ZPropertyInfo targetInfo, params ZPropertyInfo[] otherUnitInfos)
		{
			if (!targetInfo.Value.IsEmpty)
			{
				foreach (ZPropertyInfo unitInfo in otherUnitInfos)
				{
					if (targetInfo.Value.ToString() == unitInfo.Value.ToString())
					{
						targetInfo.AddMessageError("You have entered a duplicate unit code, " + targetInfo.Value);
						break;
					}
				}
			}
		}
	}
}
