
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageContentFilterLine))]
	class EDIMessageContentFilterLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<EDIMessageContentFilter>().UniversalEvent.Lines.AddNew();

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return nameof(EDIMessageContentFilterLine.DataContext);
				yield return nameof(EDIMessageContentFilterLine.Depth);
				yield return nameof(EDIMessageContentFilterLine.SchemaElement);
			}
		}

		public void TestDataContextIsReadOnly_UniversalShipment()
		{
			var ediMessageContentFilter = Factory.New<EDIMessageContentFilter>().UniversalShipment;

			ediMessageContentFilter.FilterType = EDIMessageContentFilterTypes.Codes.Include;
			var line = ediMessageContentFilter.Lines.AddNew();
			Assert("Data context should be readonly when not excluding", line.DataContext_ReadOnly);

			ediMessageContentFilter.FilterType = EDIMessageContentFilterTypes.Codes.Exclude;
			line.SchemaElement = "SubShipmentCollection";
			Assert("Data Context should not be read only when schema element is SubShipmentCollection", !line.DataContext_ReadOnly);

			line.SchemaElement = "RelatedShipmentCollection";
			Assert("Data Context should be read only when schema element is not SubShipmentCollection", line.DataContext_ReadOnly);
		}

		public void TestDataContextIsReadOnly_UniversalTransaction()
		{
			var ediMessageContentFilter = Factory.New<EDIMessageContentFilter>().UniversalTransaction;

			ediMessageContentFilter.FilterType = EDIMessageContentFilterTypes.Codes.Include;
			var line = ediMessageContentFilter.Lines.AddNew();
			Assert("Data context should be readonly when not excluding", line.DataContext_ReadOnly);

			ediMessageContentFilter.FilterType = EDIMessageContentFilterTypes.Codes.Exclude;
			line.SchemaElement = "SubShipmentCollection";
			Assert("Data Context should not be read only when schema element is SubShipmentCollection", !line.DataContext_ReadOnly);

			line.SchemaElement = "RelatedShipmentCollection";
			Assert("Data Context should be read only when schema element is not SubShipmentCollection", line.DataContext_ReadOnly);
		}

		public void TestDataContextIsCleared_WhenSchemaElementIsNotSubShipmentCollection()
		{
			var ediMessageContentFilter = Factory.New<EDIMessageContentFilter>().UniversalShipment;
			ediMessageContentFilter.FilterType = EDIMessageContentFilterTypes.Codes.Exclude;

			var line = ediMessageContentFilter.Lines.AddNew();
			line.SchemaElement = "SubShipmentCollection";
			line.DataContext = "ForwardingShipment";

			line.SchemaElement = "RelatedShipmentCollection";
			AssertEquals("DataContext should be cleared when SchemaElement's value is not SubShipmentCollection", string.Empty, line.DataContext);
		}
	}
}
