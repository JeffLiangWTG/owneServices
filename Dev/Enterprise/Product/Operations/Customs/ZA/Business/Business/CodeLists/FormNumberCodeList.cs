using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class FormNumberCodeList : CodeDescriptionPairList
	{
		public FormNumberCodeList()
		{
			AddPair("500", "DA500 (BOE - Direct)");
			AddPair("504", "DA504 (BOE - Direct - VOC)");
			AddPair("510", "DA510 (BOE - Direct - Transfer of liability)");
			AddPair("514", "DA514 (BOE - Direct - Transfer of liability - VOC)");
			AddPair("600", "DA600 (BOE - Ex warehouse – Imported goods)");
			AddPair("604", "DA604 (BOE - Ex warehouse – Imported goods - VOC)");
			AddPair("610", "DA610 (BOE - Ex warehouse - South African products)");
			AddPair("614", "DA614 (BOE - Ex warehouse - South African products - VOC)");
		}
	}
}
