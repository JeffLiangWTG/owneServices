

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoCusEntryHeaderLookups : SGAddInfoLookups
	{
		public AddInfoCusEntryHeaderLookups(AddInfoCusEntryHeader parent)
			: base(parent)
		{
		}

		public new AddInfoCusEntryHeader Parent
		{
			get { return (AddInfoCusEntryHeader)base.Parent; }
		}
	}
}
