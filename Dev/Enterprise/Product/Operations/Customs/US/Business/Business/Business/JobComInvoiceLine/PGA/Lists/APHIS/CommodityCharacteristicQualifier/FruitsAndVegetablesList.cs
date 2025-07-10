namespace Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier
{
	public  class FruitsAndVegetablesList : FruitsAndVegetablesListBase
	{
		public FruitsAndVegetablesList()
		{
			if (ZZCustomsFunctionality.IsAPHIS2024Effective)
			{
				RemoveCode(Codes.Shredded);
				RemoveCode(Codes.FreshChilled);
				RemoveCode(Codes.FreshFrozen);
			}
			else
			{
				RemoveCode(Codes.Fresh);
				RemoveCode(Codes.FreshCut);
				RemoveCode(Codes.QuickFrozen);
			}
		}
	}
}
