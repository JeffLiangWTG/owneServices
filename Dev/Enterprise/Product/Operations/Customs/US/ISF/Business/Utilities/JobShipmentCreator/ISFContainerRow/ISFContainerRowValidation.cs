namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFContainerRowValidation : AutoISFContainerRowValidation
	{
		public ISFContainerRowValidation(AutoISFContainerRow parent)
			: base(parent) { }

		public new ISFContainerRow Parent
		{
			get { return (ISFContainerRow)base.Parent; }
		}
	}
}
