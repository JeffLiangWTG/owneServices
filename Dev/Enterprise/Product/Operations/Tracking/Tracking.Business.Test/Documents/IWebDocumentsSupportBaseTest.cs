using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestsSubclassesOf(typeof(IWebDocumentsSupport))]
	public abstract class IWebDocumentsSupportBaseTest : TestCaseWithFactory
	{
		#region Helpers

		protected DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;

		protected StorageMain Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = MasterFactory.New(typeof(StorageMain)) as StorageMain;
					fParent.SM_Type = "ORG";
					fParent.SM_DB = 1;
				}
				return fParent;
			}
		}
		StorageMain fParent;

		protected IWebDocumentsSupport BizObj
		{
			get
			{
				if (fBizObj == null)
				{
					fBizObj = GetNewBusinessObject();
				}
				return fBizObj;
			}
		}
		IWebDocumentsSupport fBizObj;

		#endregion

		#region Implementation

		public void TestDocRelatedPKs()
		{
			AssertNotNull(BizObj.DocRelatedPKs);
			AssertEquals("Expected DocRelatedPKs count", ExpectedDocRelatedPKs.Count, BizObj.DocRelatedPKs.Count);
			foreach (ZGuid expectedGuid in ExpectedDocRelatedPKs)
			{
				AssertCollectionContains("Expected DocRelatedPK not found", expectedGuid, BizObj.DocRelatedPKs);
			}
		}

		#endregion

		#region Abstract Members

		protected abstract IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs { get; }

		protected abstract IWebDocumentsSupport GetNewBusinessObject();

		#endregion
	}
}
