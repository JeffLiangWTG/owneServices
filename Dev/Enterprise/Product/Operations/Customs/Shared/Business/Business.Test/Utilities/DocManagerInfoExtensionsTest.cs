using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Extensions.Testing
{
	class DocManagerInfoExtensionsTest : TestCaseWithFactory
	{
		public void TestForceToUseAnotherFactoryGuardClauses()
		{
			AssertExceptionThrown<ArgumentNullException>("When docManagerInfo is null", () => DocManagerInfoExtensions.ForceToUseAnotherFactory(null, Factory));
			AssertExceptionThrown<ArgumentNullException>("When mainFactory is null", () => DocManagerInfoExtensions.ForceToUseAnotherFactory(bizObjForTest.DocManagerInfo, null));
		}

		public void TestForceToUseAnotherFactory()
		{
			var separateFactory = new BusinessObjectFactory();
			var docManagerInfoFactory = (BusinessObjectFactory)bizObjForTest.DocManagerInfo.MasterFactory;

			CombineAssertions("PRE-CONDITIONS", () =>
			{
				AssertEquals("Should docManagerInfoFactory contain separateFactory as child factory?", false, docManagerInfoFactory.ChildFactories.Contains(separateFactory));
				AssertEquals("Should separateFactory contain docManagerInfoFactory as child factory?", false, separateFactory.ChildFactories.Contains(docManagerInfoFactory));
			});

			bizObjForTest.DocManagerInfo.ForceToUseAnotherFactory(separateFactory);

			CombineAssertions("POST-CONDITIONS", () =>
			{
				AssertEquals("Should docManagerInfoFactory contain separateFactory as child factory?", false, docManagerInfoFactory.ChildFactories.Contains(separateFactory));
				AssertEquals("Should separateFactory contain docManagerInfoFactory as child factory?", true, separateFactory.ChildFactories.Contains(docManagerInfoFactory));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			bizObjForTest = Factory.New<DummyBusinessObjectWithDocManagerInfo>();
		}
		DummyBusinessObjectWithDocManagerInfo bizObjForTest;

		#region DummyBusinessObjectWithDocManagerInfo

		class DummyBusinessObjectWithDocManagerInfo : DummyBusinessObject
		{
			public DummyBusinessObjectWithDocManagerInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, "XXX"));
			DocManagerInfo docManagerInfo;
		}

		#endregion
	}
}
