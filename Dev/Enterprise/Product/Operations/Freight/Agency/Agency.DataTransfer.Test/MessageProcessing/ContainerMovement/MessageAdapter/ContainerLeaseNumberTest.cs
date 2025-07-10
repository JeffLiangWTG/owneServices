using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal class ContainerLeaseNumberTest : TestCaseWithFactory
	{
		public void TestContainerLeaseNumber()
		{
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_Code = "RANDEP1";
			sender.OH_FullName = "Random Depot1";
			sender.OH_IsUnpackDepot = true;
			sender.OH_RL_NKClosestPort = "AUSYD";
			var customsCode = sender.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			customsCode.OK_CustomsRegNo = "SHCB1";
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, TestMessageWithContainerLeaseNumber);
			var adapter = new UniversalCMMProcessingAdapter(Factory, universalEvent);
			adapter.Load();
			AssertEquals("MessageSenderCode", "SHCB1", adapter.MessageSenderCode);
			AssertEquals("MessageType", CMMMessageType.GateIn, adapter.MessageType);
			AssertEquals("Containers Count", 1, adapter.Containers.Count);
			CMMMessageContainer container = adapter.Containers[0];
			AssertEquals("container.ContainerNumber", "CCLU4214635", container.ContainerNumber);
			AssertEquals("container.ContainerLeaseNumber", "CONTRACT1", container.ContainerLeaseNumber);
		}

		#region Implementation
		EDIMessage TestMessageWithContainerLeaseNumber
		{
			get
			{
				#region messageText
				const string messageText = @"
<UniversalEvent>
  <Event>
	<EventType>GIN</EventType>
	<EventTime>2019-01-11T16:48:01.234</EventTime>
	<EventReference>Dummy Description</EventReference>
	<DataProvider>ediEnterprise</DataProvider>
	<ContextCollection>
	  <Context>
		<Type>DepotCode</Type>
		<Value>SHCB1</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214635</Value>
	  </Context>
	  <Context>
		<Type>ContainerLeaseNumber</Type>
		<Value>CONTRACT1</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";
				#endregion
				return UniversalCMMTestHelper.CreateEDIMessage(Factory, messageText);
			}
		}
		#endregion
	}
}
