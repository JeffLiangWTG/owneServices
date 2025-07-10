using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal class CMMContainerMovementCountryProcessorFactoryTest : ContainerMovementCountryProcessorFactoryTest
	{
		public override ICMMProcessingAdapter NewAdapter(string country)
		{
			const string messageTextFormat = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+{0}:160:184'" + "NAD+CF+CSH:160:184'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+2++5'" + "DTM+7:200806271010:203'" + // footer
			"CNT+16:1'" + "UNT+8+1'" + "";
			string acosCode = "XX" + country + "XX";
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			if (!string.IsNullOrEmpty(country))
			{
				var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, country));
				sender.OH_RL_NKClosestPort = port.RL_Code;
			}

			OrgCusCode code = sender.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			code.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			code.OK_CustomsRegNo = acosCode;
			var message = Factory.New<ContainerManagementEDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.ContainerManagement;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = string.Format(messageTextFormat, acosCode);
			var adapter = new EdifactCMMProcessingAdapter(message);
			adapter.Load();
			return adapter;
		}
	}
}
