
namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoCusEntryHeaderValidation : SGAddInfoValidation
	{
		public AddInfoCusEntryHeaderValidation(AddInfoCusEntryHeader parent)
			: base(parent)
		{
		}

		protected new AddInfoCusEntryHeader Parent
		{
			get { return (AddInfoCusEntryHeader)base.Parent; }
		}

		protected AddInfoCusEntryHeaderLookups Lookups
		{
			get { return Parent.Lookups; }
		}
	}
}
