using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmTemplateCollection))]
	sealed class StmTemplateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAdditionalFilter_TemplateTypeIsDocument()
		{
			Assert(Collection.CompleteFilter.LiteralTextSqlFormatted.Contains("SO_TemplateType = 'DOC'"));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmTemplateCollection(Factory);
		}

		#endregion
	}
}
