using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

public abstract class WhsExportEDIProcessorTest<TBusinessObject, TValueObject> : WhsTestCaseWithFactory
	where TBusinessObject : WhsDocket
	where TValueObject : IValueObject
{
	#region  TestProcess()

	[TestDate(2006, 10, 5, 12, 0, 0, 0)]
	public void TestProcess()
	{
		var wDFEDIInterchangeCountBefore = Factory.GetDatabaseCount(typeof(WDFEDIInterchange));
		var buffer = new NotificationBuffer(Notify);

		//Export Docket once so that all subsequent exports have the 'InitialDataExported' attribute set in the events XML
		XmlExporter.Export(Docket, new StringWriter(), Notify);

		try
		{ File.Delete(GetExpectedFileFullPath()); }
		catch { }
		try
		{
			Processor.Process(buffer);
		}
		finally
		{
			DeleteIfExists(GetExpectedFileFullPath());
		}

		AssertEquals("Should have one new EDI Interchange", 1, Factory.GetDatabaseCount(typeof(WDFEDIInterchange)) - wDFEDIInterchangeCountBefore);
		var eDIInterchanges = Factory.Load<WDFEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeNum, GetExptedFileName()));
		AssertEquals("Should have only one EDI Interchange for the Docket", 1, eDIInterchanges.Length);
		AssertEDIInterchange(eDIInterchanges[0]);
	}

	protected virtual void AssertEDIInterchange(WDFEDIInterchange interchange)
	{
		XmlExporter.Export(Docket, new StringWriter(), Notify);
		AssertEquals("EI_InterchangeNum", GetExptedFileName(), interchange.EI_InterchangeNum);
		AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
		AssertEquals("EI_BodyText", XmlExporter.XmlInterchange.Payload.ToXmlElement().OuterXml, interchange.EI_BodyText);

		AssertEquals("Should have one EDI Message", 1, interchange.ContainedMessages.Count);
		var message = (WDFEDIMessage)interchange.ContainedMessages[0];
		AssertEquals("EM_MessageType", GetExpectedEDIMessageType(), message.EM_MessageType);
		AssertEquals("EM_ApplicationReference", Docket.WD_ExternalReference, message.EM_ApplicationReference);
		AssertEquals("EM_MessageNum", "1", message.EM_MessageNum);
		AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		AssertEquals("EM_MessageText", XmlExporter.XmlInterchange.Payload.ToXmlElement().FirstChild.OuterXml, message.EM_MessageText);
	}

	#endregion

	#region Overrides

	protected override void SetUp()
	{
		base.SetUp();
		SetupClient();
		SetupWarehouse();
		SetupDocket();
		SetupXmlExporter();
		SetupProcessor();
		Factory.Save();
		SystemDataRegistry.Instance.WarehouseExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Env.TempPath);
	}

	protected override void TearDown()
	{
		base.TearDown();
		try
		{ File.Delete(GetExpectedFileFullPath()); }
		catch { }
	}

	#endregion

	#region Implementation

	protected virtual ZString GetExpectedFileFullPath() => Path.Combine(SystemDataRegistry.Instance.WarehouseExportDirectory.Value, GetExptedFileName() + ".xml");

	protected virtual ZString GetExptedFileName() => Docket.WD_DocketID.Trim() + "_" + "20061005120000";

	protected abstract ZString GetExpectedEDIMessageType();

	protected virtual void SetupProcessor() => Processor = GetNewProcessor();

	protected abstract IProcessor GetNewProcessor();
	protected abstract WhsXmlExporter<TBusinessObject, TValueObject> GetNewXmlExporter();

	protected virtual void SetupClient() => Client = Helper.CreateClient("CLT", "Test Client");

	protected virtual void SetupXmlExporter() => XmlExporter = GetNewXmlExporter();

	protected virtual void SetupWarehouse() => Warehouse = Helper.CreateWarehouse("TST WHS", "ROW", 2, 2);

	protected abstract void SetupDocket();

	protected OrgHeader Client;
	protected WhsWarehouse Warehouse;
	protected TBusinessObject Docket;
	protected IProcessor Processor;
	protected WhsXmlExporter<TBusinessObject, TValueObject> XmlExporter;

	#endregion
}
