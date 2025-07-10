using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingBulkCopyCriteriaTest : BulkCopyCriteriaBusinessObjectTest
	{
		public void TestImplementsIForwardingBulkCopyCriteria()
		{
			ForwardingBulkCopyCriteria bulkCopyCriteria = new ForwardingBulkCopyCriteria(Factory.New<BaseJobSailing>().PK);
			Assert(bulkCopyCriteria is Enterprise.Integration.Forwarding.IForwardingBulkCopyCriteria);
		}

		public void TestGetNewSailingConsolGenerator()
		{
			ForwardingBulkCopyCriteria bulkCopyCriteria = new ForwardingBulkCopyCriteria(Factory.New<BaseJobSailing>().PK);
			AssertEquals(typeof(ForwardingBulkSailingConsolGenerator), bulkCopyCriteria.ConsolDetails.GetType());
		}

		public void TestDisposeGenerator()
		{
			bool disposeCalled = false;

			using (ForwardingBulkCopyCriteriaForTest bulkCopyCriteria = new ForwardingBulkCopyCriteriaForTest(Factory.New<BaseJobSailing>().PK))
			{
				BulkSailingConsolGeneratorForTest generatorForTest = new BulkSailingConsolGeneratorForTest(bulkCopyCriteria.Factory);
				generatorForTest.DisposeImplementation = () => disposeCalled = true;

				bulkCopyCriteria.GetNewSailingConsolGeneratorImplementation = () => generatorForTest;
				AssertEquals("prerequisite", generatorForTest, bulkCopyCriteria.ConsolDetails);
			}

			Assert(disposeCalled);
		}

		#region Implementation

		class ForwardingBulkCopyCriteriaForTest : ForwardingBulkCopyCriteria
		{
			public ForwardingBulkCopyCriteriaForTest(ZGuid sailingPK)
				: base(sailingPK)
			{
			}

			public Func<BulkSailingConsolGenerator> GetNewSailingConsolGeneratorImplementation { get; set; }
			protected override BulkSailingConsolGenerator GetNewSailingConsolGenerator()
			{
				return GetNewSailingConsolGeneratorImplementation != null ? GetNewSailingConsolGeneratorImplementation() : new ForwardingBulkSailingConsolGenerator(Factory);
			}
		}

		class BulkSailingConsolGeneratorForTest : BulkSailingConsolGenerator, IDisposable
		{
			public BulkSailingConsolGeneratorForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public Action DisposeImplementation { get; set; }
			void IDisposable.Dispose()
			{
				if (DisposeImplementation != null)
				{
					DisposeImplementation();
				}
			}

			protected override CommonConsol CreateConsolFromTemplate(CommonConsol templateConsol)
			{
				throw new NotImplementedException();
			}

			protected override CommonConsol CreateNewConsol()
			{
				throw new NotImplementedException();
			}

			protected override CommonConsol GetTemplateConsol()
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
