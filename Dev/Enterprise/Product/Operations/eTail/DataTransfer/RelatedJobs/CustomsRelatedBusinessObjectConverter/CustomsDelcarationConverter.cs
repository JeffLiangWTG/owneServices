using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class CustomsDeclarationConverter : CustomsRelatedBusinessObjectConverter
	{
		public CustomsDeclarationConverter(BaseHVLVRelatedJobCommand relatedJobCommand) : base(relatedJobCommand)
		{
		}

		public override RecipientRoleType? RecipientRoleType => UniversalDataBuss.Integration.RecipientRoleType.BRI;

		public override DataContextType MasterBillDataContextType => DataContextType.CustomsDeclaration;

		protected override bool ShouldStripNonWesternEuropeanCharacters => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersCAeManifest.Value;
	}
}
