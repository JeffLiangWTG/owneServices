using System;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public abstract class UniversalShipmentDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[TestDate]
		public void TestWriteToDataObject()
		{
			TestDateAttribute.Date = DateTime.Today;
			var bizObj = GetShipmentBusinessObject();
			AssertNotNull("business object to write should not be null", bizObj);

			var writer = GetWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, bizObj)));
			var dataObject = writer.GetDataObject(bizObj);

			var actualXml = UniversalTestHelper.GetXml(dataObject).Trim();
			var expectedXml = GetExpectedDataObjectXml().Trim();

			AssertMultilineASCIIEquals("Data object for " + bizObj.GetType().FullName, expectedXml, actualXml);
		}

		protected abstract ITopLevelDataObjectWriter GetWriter(IDataWritingManager manager);
		protected abstract BusinessObject GetShipmentBusinessObject();
		protected abstract string GetExpectedDataObjectXml();
	}
}
