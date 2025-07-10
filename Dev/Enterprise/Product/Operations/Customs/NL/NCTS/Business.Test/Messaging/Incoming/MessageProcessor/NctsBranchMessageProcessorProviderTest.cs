using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NctsBranchMessageProcessorProviderTest : TestCase
{
	public void TestNctsMessageProcessors()
	{
		AssertEquals(21, messageProcessors.Count);
	}

	public void TestCC004C()
	{
		AssertEquals(typeof(CC004CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC004C]);
	}

	public void TestCC006C()
	{
		AssertEquals(typeof(CC006CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC006C]);
	}

	public void TestCC009C()
	{
		AssertEquals(typeof(CC009CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC009C]);
	}

	public void TestCC019C()
	{
		AssertEquals(typeof(CC019CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC019C]);
	}

	public void TestCC022C()
	{
		AssertEquals(typeof(CC022CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC022C]);
	}
	public void TestCC025C()
	{
		AssertEquals(typeof(CC025CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC025C]);
	}

	public void TestCC028C()
	{
		AssertEquals(typeof(CC028CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC028C]);
	}

	public void TestCC029C()
	{
		AssertEquals(typeof(CC029CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC029C]);
	}

	public void TestCC043C()
	{
		AssertEquals(typeof(CC043CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC043C]);
	}

	public void TestCC045C()
	{
		AssertEquals(typeof(CC045CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC045C]);
	}

	public void TestCC051C()
	{
		AssertEquals(typeof(CC051CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC051C]);
	}

	public void TestCC055C()
	{
		AssertEquals(typeof(CC055CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC055C]);
	}

	public void TestCC056C()
	{
		AssertEquals(typeof(CC056CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC056C]);
	}

	public void TestCC057C()
	{
		AssertEquals(typeof(CC057CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC057C]);
	}

	public void TestCC060C()
	{
		AssertEquals(typeof(CC060CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC060C]);
	}

	public void TestCC061C()
	{
		AssertEquals(typeof(CC061CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC061C]);
	}

	public void TestCC140C()
	{
		AssertEquals(typeof(CC140CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC140C]);
	}

	public void TestCC182C()
	{
		AssertEquals(typeof(CC182CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC182C]);
	}

	public void TestCC906C()
	{
		AssertEquals(typeof(CC906CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC906C]);
	}

	public void TestCC917C()
	{
		AssertEquals(typeof(CC917CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC917C]);
	}

	public void TestCC928C()
	{
		AssertEquals(typeof(CC928CMessageProcessor), messageProcessors[NLNctsConstants.NctsMessageTypes.Incoming.CC928C]);
	}
	protected override void SetUp()
	{
		base.SetUp();
		messageProcessors = new NctsBranchMessageProcessorProvider().NctsMessageProcessors;
	}

	Dictionary<string, Type> messageProcessors;
}
