
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoCusEntryLine : AddInfo
	{
		public AddInfoCusEntryLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoCusEntryLineLookups Lookups
		{
			get { return (AddInfoCusEntryLineLookups)base.Lookups; }
		}

		public new AddInfoCusEntryLineValidation Validation
		{
			get { return (AddInfoCusEntryLineValidation)base.Validation; }
		}

		protected override SGAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusEntryLineLookups(this);
		}

		protected override SGAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusEntryLineValidation(this);
		}
	}
}
