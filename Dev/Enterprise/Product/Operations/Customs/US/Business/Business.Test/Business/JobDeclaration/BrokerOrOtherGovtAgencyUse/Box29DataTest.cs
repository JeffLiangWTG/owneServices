using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Box29Data))]
	internal class Box29DataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBox29DataWorksAsProxy()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			IBox29Supportable iBox29Supportable = declaration;
			AssertNotNull("Precondition: JobDeclaration should implement IBox29Supportable", iBox29Supportable);

			Box29Data box29Data = new Box29Data(iBox29Supportable);
			iBox29Supportable.US_Box29IncludeContainers = false;
			Assert("Precondition: iBox29Supportable.US_Box29IncludeContainers should be false", !iBox29Supportable.US_Box29IncludeContainers);
			AssertEquals("Precondition: iBox29Supportable.US_Box29IncludeContainers == ZString.Empty", ZString.Empty, iBox29Supportable.US_Box29Text);

			box29Data.US_Box29IncludeContainers = true;
			Assert("iBox29Supportable.US_Box29IncludeContainers should be true because box29Data works like proxy", iBox29Supportable.US_Box29IncludeContainers);

			box29Data.US_Box29Text = StringForWorksAsProxyTest;
			AssertEquals("iBox29Supportable.US_Box29Text should not be empty because box29Data works like proxy", StringForWorksAsProxyTest, iBox29Supportable.US_Box29Text);
		}
		const string StringForWorksAsProxyTest = "blah blah blah";

		protected override BusinessObject GetNewBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return new Box29Data(declaration);
		}
	}
}
