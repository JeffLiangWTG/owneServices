using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateOneOffContainers))]
	public class RateOneOffContainersTest : EnterpriseBusinessObjectTestCase
	{
		public void TestExcludedUniversalCopyFields()
		{
			var elementType = typeof(RateOneOffContainers);
			var copyTemplateTree = new CopyTemplateTree(elementType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var innerNode = copyTemplateTree.InnerNode as EntityCopyTemplateNode;

			var cusEntryLineNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "WidthImperial") as PropertyCopyTemplateNode;
			AssertNull("WidthImperial should be excluded from Universal Copy", cusEntryLineNode);

			var cusEntryLinePKNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "HeightImperial") as PropertyCopyTemplateNode;
			AssertNull("HeightImperial should be excluded from Universal Copy", cusEntryLinePKNode);

			var cusEntryInstructionNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "LengthImperial") as PropertyCopyTemplateNode;
			AssertNull("LengthImperial should be excluded from Universal Copy", cusEntryInstructionNode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var quote = Factory.New<Quote>();
			var oneOffShipment = quote.OneOffQuote.AddNew();
			return oneOffShipment.Containers.AddNew();
		}

		#endregion
	}
}
