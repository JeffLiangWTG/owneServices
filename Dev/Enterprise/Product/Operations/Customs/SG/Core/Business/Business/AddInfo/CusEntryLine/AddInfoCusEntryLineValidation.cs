
namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoCusEntryLineValidation : SGAddInfoValidation
	{
		public AddInfoCusEntryLineValidation(AddInfoCusEntryLine parent)
			: base(parent)
		{
		}

		protected new AddInfoCusEntryLine Parent
		{
			get { return (AddInfoCusEntryLine)base.Parent; }
		}

		protected AddInfoCusEntryLineLookups Lookups
		{
			get { return Parent.Lookups; }
		}
	}
}
