using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortManifestPortCollection))]
	internal class PortManifestPortCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PortManifestPortCollection>
	{
		public void TestPerformPostCloneAction()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			companyData.OB_OH = header.PK;
			companyData.OB_GC = glbCompany.PK;

			Factory.Save();

			var port1 = Collection.AddNew();
			port1.Port = "BEANR";
			port1.PrincipalPK = ZGuid.NewZGuid();
			port1.SenderID = "SenderID_1";
			port1.Enabled = true;

			var port2 = Collection.AddNew();
			port2.Port = "AUMEL";
			port2.PrincipalPK = ZGuid.NewZGuid();
			port2.SenderID = "SenderID_2";
			port2.Enabled = true;

			var port3 = Collection.AddNew();
			port3.Port = "AUSYD";
			port3.PrincipalPK = ZGuid.NewZGuid();
			port3.SenderID = "SenderID_3";
			port3.Enabled = true;

			var portManifestPortCollection = Collection.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory) as PortManifestPortCollection;
			AssertEquals("Cloning at CompanyLevel Should only consider ports that are Local", "AUMEL,AUSYD,BEANR", string.Join(",", portManifestPortCollection.Cast<PortManifestPort>().Select(x => x.Port).OrderBy(s => s)));

			portManifestPortCollection = Collection.Clone(new FallbackLevel(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory) as PortManifestPortCollection;
			AssertEquals("Cloning at CompanyLevel Should only consider ports that are Local", "AUMEL,AUSYD", string.Join(",", portManifestPortCollection.Cast<PortManifestPort>().Select(x => x.Port).OrderBy(s => s)));
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override PortManifestPortCollection GetCollectionToTest()
		{
			return new PortManifestPortCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PortManifestPort();
		}

		#endregion
	}
}
