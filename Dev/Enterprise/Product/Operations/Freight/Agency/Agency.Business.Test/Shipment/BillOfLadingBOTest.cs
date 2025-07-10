using System;
using System.Reflection;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLading))]
	public class BillOfLadingBOTest : EnterpriseBusinessObjectTestCase
	{
		public void TestVisualizableDocumentsSupportableAttribute()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var attribute = billOfLading.GetType().GetCustomAttribute<VisualizableDocumentsSupportableAttribute>();
			AssertNotNull("Has VisualizableDocumentsSupportable attribute", attribute);
			AssertEquals("BillOfLadingVisualizableDocumentSupporter", attribute.SupporterType.Name);
		}

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.BillOfLading);
			}
		}

		#endregion
	}
}
