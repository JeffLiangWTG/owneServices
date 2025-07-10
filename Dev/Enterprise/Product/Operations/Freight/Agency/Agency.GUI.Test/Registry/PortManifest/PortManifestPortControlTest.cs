using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(PortManifestPortControl))]
	internal class PortManifestPortControlTest : RegistryZUserControlTestCase
	{
		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new PortManifestPortCollection();
			AddSetting(collection, "AUBNE", 1);
			AddSetting(collection, "AUSYD", 2);
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PortManifestPortCollection)businessEntity).ReadOnly;
		}

		void AddSetting(PortManifestPortCollection collection, ZString port, int num)
		{
			var setting = collection.AddNew();
			setting.Port = port;
			setting.PrincipalPK = ZGuid.Empty;
			setting.SenderID = string.Format("SenderID_{0}", num);
			setting.Enabled = true;
		}

		#endregion
	}
}
