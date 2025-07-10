using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	public class ClientContractNumberCustomisation : BillOfLadingNumberCustomisation
	{
		public ClientContractNumberCustomisation()
		{
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			SetUnFilteredElement(BillOfLadingNumberCustomisationElement.Keys.GlobalOrLocal, e => { e.Include = ZBool.True; e.Order = 2; });
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) =>
			new ClientContractNumberCustomisation();

		void SetUnFilteredElement(ZString key, Action<BillOfLadingNumberCustomisationElement> setter)
		{
			var element = UnFilteredElements[key];
			if (element != null)
			{
				using (element.SuspendSettingHasChanges())
				using (element.GetValidationSuspender())
				{
					setter?.Invoke(element);
				}
			}
		}
	}
}
