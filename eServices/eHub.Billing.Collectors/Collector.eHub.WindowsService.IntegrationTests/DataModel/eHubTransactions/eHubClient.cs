using System;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions
{
	public class eHubClient
	{
		public eHubClient(string id)
		{
			CC_ID = id;
			CC_FriendlyName = friendlyName ?? id + " Friendly Name";
			CC_PK = Guid.NewGuid();
			CC_Odyssey_OH = Guid.NewGuid();
		}

		public Guid CC_PK { get; set; }
		public string CC_ID { get; set; }
		public Guid CC_Odyssey_OH { get; set; }
		public Guid? CC_DistributionZone { get; set; }
		public bool? CC_IsAirServiceProvider { get; set; }
		public string CC_AirlineCode { get; set; }
		public Guid? CC_AirServiceProvider { get; set; }
		public string CC_AirlinePrefix { get; set; }
		public bool? CC_USCustomsRecipient { get; set; }
		public string CC_AS2_Code { get; set; }
		public string CC_SCAC_Code { get; set; }
		public string CC_OwnerCategory { get; set; }
		public string CC_SystemCategory { get; set; }
		public Guid? CC_RR { get; set; }
		public bool CC_RequireStatusResponse { get; set; }
		public bool CC_NotificationForInboxRecipient { get; set; }
		public string CC_FriendlyName
		{
			get
			{
				return friendlyName ?? this.CC_ID + " Friendly Name";
			}
			set
			{
				friendlyName = value;
			}
		}
		public string CC_EmailAddress
		{
			get
			{
				return email ?? this.CC_ID + "@server.com";
			}
			set
			{
				email = value;
			}
		}
		public string CC_Password
		{
			get
			{
				using (var provider = new SHA512CryptoServiceProvider())
				{
					var actualPassword = password ?? "password";
					return CC_Password = BitConverter.ToString(provider.ComputeHash(Encoding.Default.GetBytes(this.CC_ID + actualPassword)));
				}
			}
			set
			{
				password = value;
			}
		}

		string password;
		string friendlyName;
		string email;
	}
}