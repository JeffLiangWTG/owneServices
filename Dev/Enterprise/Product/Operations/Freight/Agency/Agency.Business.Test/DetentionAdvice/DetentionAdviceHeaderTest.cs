using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DetentionAdviceHeader))]
	internal class DetentionAdviceHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentSupporter()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			DetentionAdviceHeader advice = new DetentionAdviceHeader(client);
			AssertType(typeof(DetentionAdviceHeaderDocumentSupporter), advice.DocumentSupporter);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DetentionAdviceHeader(Client);
		}

		OrgHeader Client
		{
			get
			{
				return client ?? (client = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader client;
		#endregion
	}
}
