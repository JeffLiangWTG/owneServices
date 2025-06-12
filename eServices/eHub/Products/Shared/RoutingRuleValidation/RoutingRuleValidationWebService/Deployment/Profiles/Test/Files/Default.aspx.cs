using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.eHub.Shared.RoutingRuleEngine;

public partial class _Default : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{

	}

	protected void btnSubmit_Click(object sender, EventArgs e)
	{
		var msgs = new List<string>();
		if (string.IsNullOrWhiteSpace(txtUsername.Text))
			msgs.Add("Username is required");
		if (string.IsNullOrWhiteSpace(txtPassword.Text))
			msgs.Add("Password is required");
		if (!fleMessage.HasFile)
			msgs.Add("Test Message is required");

		if (msgs.Count > 0)
		{
			lblResult.Text = string.Join("<br/>", msgs);
			return;
		}

		Dictionary<string, string> facts = null;
		if (!string.IsNullOrWhiteSpace(txtSender.Text))
		{
			facts = new Dictionary<string, string> { { "SourceParty", txtSender.Text } };
		}

		var endpoint = new EndpointAddress("https://ehub-routingws-test.wisegrid.net/RoutingRuleValidationWebService.svc");
		var binding = new CustomBinding()
		{
			Elements =
			{
				new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8),
				SecurityBindingElement.CreateUserNameOverTransportBindingElement(),
				new HttpsTransportBindingElement(),
			}
		};

		var channelFactory = new ChannelFactory<IRoutingRuleValidationWebService>(binding, endpoint);
		channelFactory.Credentials.UserName.UserName = txtUsername.Text;
		channelFactory.Credentials.UserName.Password = txtPassword.Text;
		var client = channelFactory.CreateChannel();

		Result[] result;

		switch (TestMethod.SelectedValue)
		{
			case "EvaluateOCM":
				var rrInput = new RoutingEvaluationInput() { PropertyFacts = facts, Message = fleMessage.FileBytes };
				result = client.EvaluateOCM(rrInput);
				break;
			case "Evaluate":
			default:
				var rrGenericInput = new RoutingEvaluationGenericInput() { RuleId = txtRuleId.Text, PropertyFacts = facts, Message = fleMessage.FileBytes };
				result = client.Evaluate(rrGenericInput);
				break;
		}

		lblResult.Text = string.Join("<br/>", result.Select(r => Newtonsoft.Json.JsonConvert.SerializeObject(r)));
		channelFactory.Close();
	}

	[ServiceContract]
	public interface IRoutingRuleValidationWebService
	{
		[OperationContract(Action = "http://tempuri.org/IRoutingRuleValidationWebService/Evaluate", ReplyAction = "http://tempuri.org/IRoutingRuleValidationWebService/EvaluateResponse")]
		Result[] Evaluate(RoutingEvaluationGenericInput routingEvaluationInput);

		[OperationContract(Action = "http://tempuri.org/IRoutingRuleValidationWebService/EvaluateOCM", ReplyAction = "http://tempuri.org/IRoutingRuleValidationWebService/EvaluateOCMResponse")]
		Result[] EvaluateOCM(RoutingEvaluationInput routingEvaluationInput);
	}

	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService")]
	[Serializable]
	public class RoutingEvaluationInput
	{
		[DataMember]
		public Dictionary<string, string> PropertyFacts { get; set; }
		[DataMember]
		public byte[] Message { get; set; }
	}

	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService")]
	[Serializable]
	public class RoutingEvaluationGenericInput : RoutingEvaluationInput
	{
		[DataMember(IsRequired = true)]
		public string RuleId { get; set; }
	}
}
