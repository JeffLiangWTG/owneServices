using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class ExternalRequestInfoTemplateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			var entity = Factory.New<ExternalRequestInfoTemplate>();
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
	}
}
