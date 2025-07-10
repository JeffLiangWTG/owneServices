namespace Enterprise.Customs.TW.Manifest.Business
{
	public class UNDGDataItemValidation : MasterFiles.Business.UNDGDataItemValidation
	{
		public UNDGDataItemValidation(UNDGDataItem parent)
			: base(parent)
		{
		}

		protected override bool UNDGSubstanceIsRequired => false;
	}
}
