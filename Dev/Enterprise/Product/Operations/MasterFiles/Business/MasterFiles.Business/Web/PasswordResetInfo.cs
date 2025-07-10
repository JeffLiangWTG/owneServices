using System;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business
{
	public class PasswordResetInfo
	{
		public static class Schema
		{
			public const string Product = "product";
			public const string ContactEmail = "contact_email";
			public const string OrgCode = "org_code";
			public const string NavigateUrl = "navigation_url";
			public const string EmailTemplateCompanyPk = "email_template_company_pk";
		}

		[JsonProperty(Schema.Product)]
		public string Product { get; set; }

		[JsonProperty(Schema.ContactEmail)]
		public string ContactEmail { get; set; }

		[JsonProperty(Schema.OrgCode)]
		public string OrgCode { get; set; }

		[JsonProperty(Schema.NavigateUrl)]
		public Uri NavigateUrl { get; set; }

		[JsonProperty(Schema.EmailTemplateCompanyPk)]
		public string EmailTemplateCompanyPk { get; set; }
	}
}
