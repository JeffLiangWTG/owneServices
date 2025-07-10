using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(CodeDescriptionPairInfo))]
	public class CodeDescriptionPairInfoTestCase : DataObjectInfoTestCase<CodeDescriptionPairInfo>
	{
		#region Constructor

		public void TestConstructor()
		{
			var codeDescription = new CodeDescriptionPair("ABC", "123");
			var codeDescriptionInfo = new CodeDescriptionPairInfo(codeDescription);
			AssertEquals("ABC", codeDescriptionInfo.Code);
			AssertEquals("123", codeDescriptionInfo.Description);
			AssertEquals("ABC - 123", codeDescriptionInfo.CodeAndDescription);
		}

		#endregion

		#region Properties

		#region TestCode

		public void TestCode()
		{
			var codeDescriptionPairInfo = new CodeDescriptionPairInfo();
			codeDescriptionPairInfo.Code = "DEF";
			AssertEquals("DEF", codeDescriptionPairInfo.Code);
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			var codeDescriptionPairInfo = new CodeDescriptionPairInfo();
			codeDescriptionPairInfo.Description = "456";
			AssertEquals("456", codeDescriptionPairInfo.Description);
		}

		#endregion

		#region TestCodeAndDescription

		public void TestCodeAndDescription()
		{
			var codeDescriptionPairInfo = new CodeDescriptionPairInfo();
			codeDescriptionPairInfo.CodeAndDescription = "ABC - 123";
			AssertEquals("ABC - 123", codeDescriptionPairInfo.CodeAndDescription);
		}

		public void TestConstructorBothBlankCodeAndDescription()
		{
			var codeDescription = new CodeDescriptionPair("", "");
			var codeDescriptionPairInfo = new CodeDescriptionPairInfo(codeDescription);
			AssertEquals("", codeDescriptionPairInfo.CodeAndDescription);
		}

		public void TestConstructorCodeAndBlankDescription()
		{
			var codeDescription = new CodeDescriptionPair("ABC", "");
			var codeDescriptionPairInfo = new CodeDescriptionPairInfo(codeDescription);
			AssertEquals("ABC - ", codeDescriptionPairInfo.CodeAndDescription);
		}

		public void TestConstructorBlankCodeAndDescription()
		{
			var codeDescription = new CodeDescriptionPair("", "123");
			var codeDescriptionPairInfo = new CodeDescriptionPairInfo(codeDescription);
			AssertEquals("", codeDescriptionPairInfo.CodeAndDescription);
		}

		#endregion

		#endregion

		#region Implementation

		protected new CodeDescriptionPairInfo Parent
		{
			get { return (CodeDescriptionPairInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new CodeDescriptionPairInfo();
		}

		#endregion
	}
}
