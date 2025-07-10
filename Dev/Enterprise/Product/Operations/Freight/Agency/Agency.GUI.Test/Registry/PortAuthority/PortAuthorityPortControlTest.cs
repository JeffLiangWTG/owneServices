using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(PortAuthorityPortControl))]
	internal class PortAuthorityPortControlTest : RegistryZUserControlTestCase
	{
		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			PortAuthorityPortCollection collection = new PortAuthorityPortCollection();
			AddSetting(collection, "AUBNE", 1);
			AddSetting(collection, "AUSYD", 2);
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((PortAuthorityPortCollection)businessEntity).ReadOnly;
		}

		void AddSetting(PortAuthorityPortCollection collection, ZString port, int num)
		{
			PortAuthorityPort setting = collection.AddNew();
			setting.Port = port;
			setting.Version = PortAuthorityVersionList.Codes.V11;
			setting.ProductionEmail = string.Format("manifest@server{0}.com.au", num);
			setting.ProductionID = string.Format("Prod Recipient {0}", num);
			setting.TestingEmail = string.Format("manitest@server{0}.com.au", num);
			setting.TestingID = string.Format("Test Recipient {0}", num);
		}
		#endregion
	}
}
