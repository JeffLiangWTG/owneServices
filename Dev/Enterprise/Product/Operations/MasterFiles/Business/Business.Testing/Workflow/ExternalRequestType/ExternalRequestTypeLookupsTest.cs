using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class ExternalRequestTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			var entity = Factory.New<ExternalRequestType>();
			AssertArrayEqualsByElements(new string[] {
				ExternalRequestTypeJobTypes.Codes.ALL,
				ExternalRequestTypeJobTypes.Codes.ORD,
				ExternalRequestTypeJobTypes.Codes.ORL,
				ExternalRequestTypeJobTypes.Codes.SBK,
				ExternalRequestTypeJobTypes.Codes.SBL,
				ExternalRequestTypeJobTypes.Codes.CLH,
				ExternalRequestTypeJobTypes.Codes.CLI,
				ExternalRequestTypeJobTypes.Codes.CLP,
				ExternalRequestTypeJobTypes.Codes.CPL,
				ExternalRequestTypeJobTypes.Codes.SHP,
				ExternalRequestTypeJobTypes.Codes.SPL,
				ExternalRequestTypeJobTypes.Codes.CON }, entity.Lookups.JobTypeList.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

			AssertArrayEqualsByElements(new string[] {
				ExternalRequestTypeJobTypes.Descriptions.ALL,
				ExternalRequestTypeJobTypes.Descriptions.ORD,
				ExternalRequestTypeJobTypes.Descriptions.ORL,
				ExternalRequestTypeJobTypes.Descriptions.SBK,
				ExternalRequestTypeJobTypes.Descriptions.SBL,
				ExternalRequestTypeJobTypes.Descriptions.CLH,
				ExternalRequestTypeJobTypes.Descriptions.CLI,
				ExternalRequestTypeJobTypes.Descriptions.CLP,
				ExternalRequestTypeJobTypes.Descriptions.CPL,
				ExternalRequestTypeJobTypes.Descriptions.SHP,
				ExternalRequestTypeJobTypes.Descriptions.SPL,
				ExternalRequestTypeJobTypes.Descriptions.CON }, entity.Lookups.JobTypeList.OfType<ICodeDescription>().Select(el => el.Description).ToArray());
		}

		class DummyObjectHandle : ObjectHandle
		{
			readonly object obj;
			public DummyObjectHandle(object obj)
			{
				this.obj = obj;
			}

			public override object GetObject()
			{
				return obj;
			}
		}

		public void TestAssigneeAddressTypes()
		{
			var entity = Factory.New<ExternalRequestType>();
			AssertEquals(0, entity.Lookups.AssigneeAddressTypes.Count);

			var mockedRequestTypeProviders = new Hashtable();
			var mockedRequestTypeProviderForOrder = new Moq.Mock<IExternalRequestSupportedAddressTypesProvider>();
			mockedRequestTypeProviderForOrder.SetupGet(x => x.SupportedAssigneeAddressTypes).Returns(() => {
				var result = new CodeDescriptionPairList();
				result.AddPair("XX1", "XX1 Desc");
				result.AddPair("XX2", "XX2 Desc");
				return result;
			});

			mockedRequestTypeProviders.Add(ExternalRequestTypeJobTypes.Codes.ORD, new DummyObjectHandle(mockedRequestTypeProviderForOrder.Object));
			using (ObjectFactory.Substitute("ExternalRequestSupportedAddressTypesProviders", mockedRequestTypeProviders))
			{
				entity.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;

				AssertArrayEqualsByElements(new string[] { "XX1", "XX2", }, entity.Lookups.AssigneeAddressTypes.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

				entity.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ORL;
				AssertEquals(0, entity.Lookups.AssigneeAddressTypes.Count);
			}
		}

		public void TestReviewerAddressTypes()
		{
			var entity = Factory.New<ExternalRequestType>();
			AssertEquals(0, entity.Lookups.ReviewerAddressTypes.Count);

			var mockedRequestTypeProviders = new Hashtable();
			var mockedRequestTypeProviderForOrder = new Moq.Mock<IExternalRequestSupportedAddressTypesProvider>();
			mockedRequestTypeProviderForOrder.SetupGet(x => x.SupportedReviewerAddressTypes).Returns(() => {
				var result = new CodeDescriptionPairList();
				result.AddPair("YY1", "YY1 Desc");
				result.AddPair("YY2", "YY2 Desc");
				return result;
			});

			mockedRequestTypeProviders.Add(ExternalRequestTypeJobTypes.Codes.ORD, new DummyObjectHandle(mockedRequestTypeProviderForOrder.Object));

			using (ObjectFactory.Substitute("ExternalRequestSupportedAddressTypesProviders", mockedRequestTypeProviders))
			{
				entity.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;

				AssertArrayEqualsByElements(new string[] { "YY1", "YY2", }, entity.Lookups.ReviewerAddressTypes.OfType<ICodeDescription>().Select(el => el.Code).ToArray());

				entity.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ORL;
				AssertEquals(0, entity.Lookups.ReviewerAddressTypes.Count);
			}
		}
	}
}
