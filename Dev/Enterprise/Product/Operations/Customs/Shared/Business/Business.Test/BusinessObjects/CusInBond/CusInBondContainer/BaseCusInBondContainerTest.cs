using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class BaseCusInBondContainerBaseOnlyTest : TestCaseWithFactory
	{
		public void TestCanLoadUsingBaseClass()
		{
			var header = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var container = Factory.New<BaseCusInBondContainerTestClass>();
			container.BC_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix;
			container.BC_ParentID = header.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedContainer = newFactory.Load<BaseCusInBondContainerTestClass>(container.PK);
			AssertNotNull(reloadedContainer);
			AssertSame("newFactory.Load(reloadedContainer.TablePrefix, container.PK)", reloadedContainer, newFactory.Load(reloadedContainer.TablePrefix, container.PK));
			AssertSame("newFactory.Load<BaseCusInBondContainer>(container.PK)", reloadedContainer, newFactory.Load<BaseCusInBondContainer>(container.PK));
		}

		public class BaseCusInBondContainerTestClass : BaseCusInBondContainer
		{
			public BaseCusInBondContainerTestClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}

	[TestsSubclassesOf(typeof(BaseCusInBondContainer))]
	public abstract class BaseCusInBondContainerTest<TCusInBondContainer> : EnterpriseBusinessObjectTestCase
		where TCusInBondContainer : BaseCusInBondContainer
	{
		public void TestBC_DataModel_SetOnSaving()
		{
			var container = (BaseCusInBondContainer)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("Not set", ZString.Empty, container.BC_DataModel);
			Factory.Save();
			AssertEquals("set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode + container.GetHeader().BH_ApplicationCode, container.BC_DataModel);
		}

		public void TestBC_DataModel_ReportErrorWhenUpdated() =>
			DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<TCusInBondContainer>(Factory, GetNewBusinessObject);

		public void TestBC_DataModel_CanSaveTwice() =>
			DataModelTestHelper.RunDataModelTest_CanSaveTwice<TCusInBondContainer>(Factory, GetNewBusinessObject);

		public void TestBC_DataModel_InvalidParent()
		{
			var container = Factory.New<TCusInBondContainer>();
			container.BC_ParentTableCode = "BH";
			container.BC_ContainerNum = "ABC";
			AssertEquals("Not set", ZString.Empty, container.BC_DataModel);
			Factory.Save();
			Assertion.AssertEquals("BC_DataModel was set without a valid CusInBondHeader.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertEquals("set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, container.BC_DataModel);
		}

		public void TestCorrectlyTypeDecided()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var bizObj = (TCusInBondContainer)GetNewBusinessObjectForDeleteTest(factory);
				factory.Save();
				AssertEquals("Something is not right, data was not saved correctly. Please check that the correct factory was used", true, bizObj.IsInDatabase);
				var newFactory = new BusinessObjectFactory();
				var expectedType = typeof(TCusInBondContainer);
				var bizObjFullName = bizObj.GetType().FullName;
				foreach (var baseType in GetBaseTypes().Union(new[] { expectedType }))
				{
					BaseCusInBondContainer bizObjInDiffFactory = null;
					var baseTypeFullName = baseType.FullName;
					AssertNoExceptionThrown($"Loading {bizObjFullName} using type {baseTypeFullName}", () => bizObjInDiffFactory = (BaseCusInBondContainer)newFactory.Load(baseType, bizObj.PK));
					AssertEquals(baseTypeFullName, expectedType, bizObjInDiffFactory.GetType());
				}
			});
		}

		protected virtual IEnumerable<Type> GetBaseTypes()
		{
			yield return typeof(BaseCusInBondContainer);
		}

		protected abstract BusinessObject GetNewBusinessObject(BusinessObjectFactory factory);

		protected sealed override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected sealed override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected sealed override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);
	}
}
