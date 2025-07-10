using System;
using System.Linq;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class CodeDescriptionPairInfoCollectionTestCase : DataObjectInfoCollectionTestCase<CodeDescriptionPairInfo>
	{
		#region TestAddRange

		public void TestAddRange()
		{
			var collection = new CodeDescriptionPairList();
			collection.Add(new CodeDescriptionPair("Code1", "Description1"));
			collection.Add(new CodeDescriptionPair("Code2", "Description2"));
			collection.Add(new CodeDescriptionPair("Code3", "Description3"));

			var webServiceCollection = new CodeDescriptionPairInfoCollection();
			webServiceCollection.AddRange(collection);
			AssertEquals(3, webServiceCollection.Count);
			AssertNotNull(webServiceCollection.SingleOrDefault(c => c.Code == "Code1" && c.Description == "Description1"));
			AssertNotNull(webServiceCollection.SingleOrDefault(c => c.Code == "Code2" && c.Description == "Description2"));
			AssertNotNull(webServiceCollection.SingleOrDefault(c => c.Code == "Code3" && c.Description == "Description3"));
		}

		#endregion

		#region TestConstructor

		public void TestConstructor()
		{
			var collection = new CodeDescriptionPairList();
			collection.Add(new CodeDescriptionPair("Code1", "Description1"));
			collection.Add(new CodeDescriptionPair("Code2", "Description2"));
			collection.Add(new CodeDescriptionPair("Code3", "Description3"));

			var webServiceCollection = new CodeDescriptionPairInfoCollection(collection);
			AssertEquals(3, webServiceCollection.Count);
			AssertNotNull(webServiceCollection.SingleOrDefault(c => c.Code == "Code1" && c.Description == "Description1"));
			AssertNotNull(webServiceCollection.SingleOrDefault(c => c.Code == "Code2" && c.Description == "Description2"));
			AssertNotNull(webServiceCollection.SingleOrDefault(c => c.Code == "Code3" && c.Description == "Description3"));
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(CodeDescriptionPairInfo);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CodeDescriptionPairInfoCollection);
		}

		protected override CodeDescriptionPairInfo GetNewObjectInfo()
		{
			return new CodeDescriptionPairInfo();
		}

		protected new CodeDescriptionPairInfoCollection Parent
		{
			get { return (CodeDescriptionPairInfoCollection)base.Parent; }
		}

		protected override DataObjectInfoCollection<CodeDescriptionPairInfo> GetNewObjectInfoCollection()
		{
			return new CodeDescriptionPairInfoCollection();
		}

		#endregion
	}
}
