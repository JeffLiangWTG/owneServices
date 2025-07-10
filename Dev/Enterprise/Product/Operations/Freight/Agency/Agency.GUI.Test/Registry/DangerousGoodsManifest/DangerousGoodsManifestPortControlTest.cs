using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(DangerousGoodsManifestPortControl))]
	internal class DangerousGoodsManifestPortControlTest : RegistryZUserControlTestCase
	{
		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			DangerousGoodsManifestPortCollection collection = new DangerousGoodsManifestPortCollection();
			AddSetting(collection, "AUBNE", 1);
			AddSetting(collection, "AUSYD", 2);
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DangerousGoodsManifestPortCollection)businessEntity).ReadOnly;
		}

		void AddSetting(DangerousGoodsManifestPortCollection collection, ZString port, int num)
		{
			DangerousGoodsManifestPort setting = collection.AddNew();
			setting.Port = port;
			setting.PrincipalPK = ZGuid.Empty;
			setting.SenderID = string.Format("SenderID_{0}", num);
			setting.Enabled = true;
		}
		#endregion
	}
}
