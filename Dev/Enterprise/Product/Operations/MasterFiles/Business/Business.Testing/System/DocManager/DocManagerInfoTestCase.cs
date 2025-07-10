using System;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(DocManagerInfo))]
	public abstract class DocManagerInfoTestCase : TestCaseWithFactory
	{
		/// <summary>
		/// Return a business object that gets passed into the DocManagerInfo object's constructor. 
		/// Don't set up any related bizos in the related objects array yet
		/// </summary>
		public abstract BusinessObject GetEmptyParentBusinessObject();

		/// <summary>
		/// Return a business object that gets pass into the DocManagerInfo object's constructor.
		/// Set up any related objects that are included in the RelatedObjects array
		/// </summary>
		public abstract BusinessObject GetPopulatedParentBusinessObject();

		/// <summary>
		/// The type of DocManagerInfo subclass to test
		/// </summary>
		public Type GetDocManagerInfoType() => TestedTypeHelper.GetTestedType(GetType());

		protected DocManagerInfo GetDocManagerInfo(bool populateObject)
		{
			BusinessObject parent = populateObject ? GetPopulatedParentBusinessObject() : GetEmptyParentBusinessObject();
			Assert(parent.GetType() + " does not implement IDocManagerSupport, check that you are passing the correct type of object into the DocManagerInfo", parent is IDocManagerSupport);
			return ((IDocManagerSupport)parent).DocManagerInfo;
		}

		public void TestMasterFactoryShouldUseBizoFactoryDocManagerInfoNewFactorySuspended()
		{
			var info = GetDocManagerInfo(true);
			using (ServiceContainerSuspenderHelper.GetDocManagerInfoNewFactorySuspender(info.BusinessEntity.Factory))
			{
				AssertEquals(info.BusinessEntity.Factory, info.MasterFactory.FactoryForEverythingExceptEDocs);
			}

			CleanUp(info.RelatedObjects);
		}

		public void TestRelatedObjectsNotNull()
		{
			DocManagerInfo info = GetDocManagerInfo(true);
			AssertNotNull("RelatedObjects should never be null", info.RelatedObjects);

			CleanUp(info.RelatedObjects);
		}

		public void TestNoElementsReturnNull()
		{
			DocManagerInfo info = GetDocManagerInfo(false);

			foreach (BusinessObject bizO in info.RelatedObjects)
			{
				AssertNotNull("There are null elements in the DocManagerInfo.RelatedObjects collection. Please check for null before adding the object to the related objects array.", bizO);
			}

			CleanUp(info.RelatedObjects);
		}

		public void TestRelatedObjectsContentsAllImplementIDocManagerSupport()
		{
			DocManagerInfo info = GetDocManagerInfo(true);

			ZString errorMessage = ZString.Empty;
			foreach (BusinessObject bizO in info.RelatedObjects)
			{
				if (!(bizO is IDocManagerSupport))
				{
					errorMessage += bizO.GetType() + " does not implement IDocManagerSupport. Please remove it from the DocManagerInfo.RelatedObjects property." + System.Environment.NewLine;
				}
			}
			Assert(errorMessage, errorMessage.IsEmpty);

			CleanUp(info.RelatedObjects);
		}

		public void TestNoDuplicatedRelatedObjects()
		{
			var relatedObjects = GetDocManagerInfo(true).RelatedObjects;
			AssertEquals(relatedObjects.Length, relatedObjects.DistinctBy(x => x.PK).Count());

			CleanUp(relatedObjects);
		}

		public void TestDocManagerInfoPropertyIsCached()
		{
			IDocManagerSupport parent = (IDocManagerSupport)GetPopulatedParentBusinessObject();
			Assert("You should return the same instance of the DocManagerInfo object in the DocManagerInfo property. Please make sure you are lazy creating and caching this object.", parent.DocManagerInfo == parent.DocManagerInfo);

			CleanUp(parent.DocManagerInfo.RelatedObjects);
		}

		public void TestRelatedObjectTypesAlwaysShow()
		{
			DocManagerInfo info = GetDocManagerInfo(false);
			Assert("should not always show related object edocs", !info.RelatedObjectTypesAlwaysShow(null));

			CleanUp(info.RelatedObjects);
		}

		protected virtual void CleanUp(BusinessObject[] relatedObjects) { }
	}
}
