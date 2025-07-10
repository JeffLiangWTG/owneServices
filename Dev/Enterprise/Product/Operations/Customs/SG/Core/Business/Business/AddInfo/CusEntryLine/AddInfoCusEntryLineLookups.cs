

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoCusEntryLineLookups : SGAddInfoLookups
	{
		public AddInfoCusEntryLineLookups(AddInfoCusEntryLine parent)
			: base(parent)
		{
		}

		public new AddInfoCusEntryLine Parent
		{
			get { return (AddInfoCusEntryLine)base.Parent; }
		}
	}
}
