using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OrgSalesProductCustomColumnDefinitionValidation : GenCustomColumnDefinitionValidation
	{
		public OrgSalesProductCustomColumnDefinitionValidation(AutoGenCustomColumnDefinition parent)
			: base(parent)
		{
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		protected override void CheckXC_Name()
		{
			base.CheckXC_Name();

			var allColumnDefinitionsQuery = new ZQuery(GenCustomColumnDefinitionSchema.XC_ParentTableCode, Parent.XC_ParentTableCode);
			allColumnDefinitionsQuery.AddToFilter(GenCustomColumnDefinitionSchema.XC_ParentID, Parent.XC_ParentID);

			var factory = Parent.Factory;
			var allColumnDefinitions = factory.Load<OrgSalesProductCustomColumnDefinition>(allColumnDefinitionsQuery);
			if (allColumnDefinitions.Any(x => x.PK != Parent.PK && x.XC_Name.EqualsIgnoringCase(Parent.XC_Name)))
			{
				var message = PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(Parent.XC_NameInfo.HumanReadableName);
				Parent.XC_NameInfo.AddError(message);
			}
		}
	}
}
