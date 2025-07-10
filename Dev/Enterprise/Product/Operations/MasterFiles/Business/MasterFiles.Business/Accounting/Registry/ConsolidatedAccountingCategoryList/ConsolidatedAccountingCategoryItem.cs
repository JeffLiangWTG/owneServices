using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <remarks>
	/// This class is designed to be used only for the System level Registry Item as its IsCodeInUse method checks usage on the OrgCompanyData records for ALL Companies.
	/// If registry is extened to system & company level one day, do not forget setting correct FallbackLevel to base field and also update IsCodeInUse method's filtering logic.
	/// </remarks>
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public sealed class ConsolidatedAccountingCategoryItem : CodeDescriptionWithGroup, ICanDelete
	{
		public ConsolidatedAccountingCategoryItem()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new ConsolidatedAccountingCategoryItem();

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();

			if (OriginalCode != Code && IsCodeInUse())
			{
				CodeInfo.AddError(Res.GetString("9878331F-8461-4AE8-8DCC-C33514614C52", "This code cannot be changed as it is in use by at least one organization record in the system."));
			}
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var itemTo = (ConsolidatedAccountingCategoryItem)clone;
			itemTo.OriginalCode = OriginalCode;
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete => ReasonForNotAbleToDelete;

		public override MultilingualString ReasonForNotAbleToDelete => IsCodeInUse()
			? ResString.GetMultilingualString("5B11D574-6488-4DF4-9B98-5EFD68BDF317", "This code cannot be deleted as it is in use by at least one organization record in the system.")
			: base.ReasonForNotAbleToDelete;

		public override bool CanDelete => !IsCodeInUse() && base.CanDelete;

		bool IsCodeInUse()
		{
			if (string.IsNullOrWhiteSpace(OriginalCode))
			{
				return false;
			}

			//Currently we check OrgCompanyData records for all Companies because the ConsolidatedAccountingCategoryList registry is only used on System level.
			//If registry is extened to system & company level one day, do not forget to add filter to query GC via CurrentFallbackLevel
			//However, to get correct CurrentFallbackLevel, do not forget to implement related constructor and update "GetClone" method to set correct value to base field.

			var query = new ZQuery(OrgCompanyDataSchema.OB_ARConsolidatedAccountingCategory, OriginalCode);
			return new BusinessObjectFactory().Exists(typeof(OrgCompanyData), query);
		}

		public void SetOriginalValue(ICodeDescriptionWithGroup pair)
		{
			OriginalCode = pair.Code;
		}

		public ZString OriginalCode { get; private set; } = string.Empty;
	}
}