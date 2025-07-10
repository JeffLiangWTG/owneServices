
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoCusEntryHeader : AddInfo
	{
		public AddInfoCusEntryHeader(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoCusEntryHeaderLookups Lookups
		{
			get { return (AddInfoCusEntryHeaderLookups)base.Lookups; }
		}

		public new AddInfoCusEntryHeaderValidation Validation
		{
			get { return (AddInfoCusEntryHeaderValidation)base.Validation; }
		}

		protected override SGAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusEntryHeaderLookups(this);
		}

		protected override SGAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusEntryHeaderValidation(this);
		}
	}
}
