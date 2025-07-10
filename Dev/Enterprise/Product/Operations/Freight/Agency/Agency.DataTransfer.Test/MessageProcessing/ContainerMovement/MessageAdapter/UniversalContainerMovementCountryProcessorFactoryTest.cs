using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal class UniversalContainerMovementCountryProcessorFactoryTest : ContainerMovementCountryProcessorFactoryTest
	{
		public override ICMMProcessingAdapter NewAdapter(string country)
		{
			#region messageText
			const string messageText = @"
<UniversalEvent>
  <Event>
	<EventType>GIM</EventType><!-- FOB / FUL / GIM / GOM -->
	<EventTime>2012-01-12T16:48:01.234</EventTime>
	<EventReference>Dummy Description</EventReference>
	<DataProvider>ediEnterprise</DataProvider>
	<ContextCollection>
	  <Context>
		<Type>DepotCode</Type>
		<Value>{0}</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214635</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214636</Value>
	  </Context>
	  <Context>
		<Type>VoyageNumber</Type>
		<Value>0033</Value>
	  </Context>
	  <Context>
		<Type>LloydsNumber</Type>
		<Value>9290127</Value>
	  </Context>
	  <Context>
		<Type>ContainerISOCode</Type>
		<Value>42R0</Value>
	  </Context>
	  <Context>
		<Type>ContainerOwnershipType</Type>
		<Value>Carrier</Value>
	  </Context>
	  <Context>
		<Type>IsEmptyContainer</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>MBOLNumber</Type>
		<Value>BillOfLading</Value>
	  </Context>
	  <Context>
		<Type>CarriersBookingReference</Type>
		<Value>BookingReference</Value>
	  </Context>
	  <Context>
		<Type>EntryNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumberType</Type>
		<Value>CAN</Value>
	  </Context>
	  <Context>
		<Type>EntryNumberCountryOfIssue</Type>
		<Value>AU</Value>
	  </Context>
	  <Context>
		<Type>ContainerGrossWeight</Type>
		<Value>6800</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo</Type>
		<Value>123456</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo2</Type>
		<Value>654321</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo3</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>GoodsDeclarationNumber</Type>
		<Value>GoodsDeclarationNumber</Value>
	  </Context>
	  <Context>
		<Type>PositioningDateTime</Type>
		<Value>2008-06-27 10:10:0</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			if (!string.IsNullOrEmpty(country))
			{
				var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, country));
				sender.OH_RL_NKClosestPort = port.RL_Code;
			}

			var acosCode = "XX" + country + "XX";
			var code = sender.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			code.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			code.OK_CustomsRegNo = acosCode;
			var ediMessage = UniversalCMMTestHelper.CreateEDIMessage(Factory, string.Format(messageText, acosCode));
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, ediMessage);
			var adapter = new UniversalCMMProcessingAdapter(Factory, universalEvent);
			adapter.Load();
			return adapter;
		}
	}
}
