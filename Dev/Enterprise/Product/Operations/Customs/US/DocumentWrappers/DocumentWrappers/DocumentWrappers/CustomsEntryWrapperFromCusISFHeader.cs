using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class CustomsEntryWrapperFromCusISFHeader : CustomsEntryWrapper
	{
		public CustomsEntryWrapperFromCusISFHeader(CusISFHeader headerBO, BusinessObjectFactory factory)
			: base(headerBO, factory)
		{
			HeaderBO = headerBO ?? Factory.GetNull<CusISFHeader>();
		}
		protected readonly CusISFHeader HeaderBO;

		protected override CodeAndDescriptionWrapper GetEntryType()
		{
			return new CodeAndDescriptionWrapper(ISFEntryType, ISFEntryTypeDescription, Factory);
		}
		const string ISFEntryType = "ISF";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		const string ISFEntryTypeDescription = "Importer Security Filing";

		protected override ZString GetEntryNumber()
		{
			return HeaderBO.BF_CustomsReference;
		}

		protected override ZString GetInformation()
		{
			return HeaderBO.BF_HouseBill;
		}

		protected override ZDateTime GetIssueDate()
		{
			return HeaderBO.BF_FirstAcceptedDate;
		}
	}
}
