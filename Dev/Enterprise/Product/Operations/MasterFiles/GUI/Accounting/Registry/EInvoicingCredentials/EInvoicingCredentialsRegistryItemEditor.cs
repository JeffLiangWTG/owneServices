using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class EInvoicingCredentialsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public EInvoicingCredentialsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
			if (dataType is EInvoicingCredentialsRegistryDataType eInvoicingCredentialsRegistryDataType)
			{
				apiKeyGenerator = eInvoicingCredentialsRegistryDataType.APIKeyGenerator;
				behavior = eInvoicingCredentialsRegistryDataType.Behavior;
			}
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new EInvoicingCredentialsControl(apiKeyGenerator, behavior, Factory, Level);
		}

		readonly IAPIKeyGeneratorStrategy apiKeyGenerator;
		readonly EInvoicingCredentialsRegistryItem.Behavior behavior;
	}
}
