using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(JobServiceTypesInfo))]
	public class JobServiceTypesInfoInfoTestCase : DataObjectInfoTestCase<JobServiceTypesInfo>
	{
		#region Constructor

		public void TestConstructor()
		{
			var codeDescription = new CodeDescriptionPair("ABC", "123");
			var codeDescriptionInfo = new JobServiceTypesInfo(codeDescription);
			AssertEquals("ABC", codeDescriptionInfo.Code);
			AssertEquals("123", codeDescriptionInfo.Description);
			AssertNull(codeDescriptionInfo.ExistingJobService);
		}

		#endregion

		#region Properties

		#region TestCode

		public void TestCode()
		{
			var codeDescriptionPairInfo = new JobServiceTypesInfo();
			codeDescriptionPairInfo.Code = "DEF";
			AssertEquals("DEF", codeDescriptionPairInfo.Code);
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			var codeDescriptionPairInfo = new JobServiceTypesInfo();
			codeDescriptionPairInfo.Description = "456";
			AssertEquals("456", codeDescriptionPairInfo.Description);
		}

		#endregion

		#endregion

		#region Implementation

		protected new JobServiceTypesInfo Parent
		{
			get { return (JobServiceTypesInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new JobServiceTypesInfo();
		}

		#endregion
	}
}
