using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public partial class STATACREQDOCSendingObject : Customs.Business.BaseMessageSendingObject, IEDIFACTMessageAttachee
	{
		public STATACREQDOCSendingObject(STATACREQDOCSendingObjectParent parent, FinancialAccountNumberPortMap mapping)
			: base(parent.Factory)
		{
			using (this.GetValidationSuspender())
			using (this.SuspendSettingHasChanges())
			{
				this.Mapping = Argument.NotNull(mapping, "mapping");
			}
		}

		public readonly FinancialAccountNumberPortMap Mapping;

		#region Schema
		public static class Schema
		{
			public const string OrganizationCode = "OrganizationCode";
			public const string CustomsOfficeCode = "CustomsOfficeCode";
			public const string FinancialAccountNumber = "FinancialAccountNumber";
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
		}
		#endregion

		#region New Properties

		#region Organization

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.STATACREQDOCSendingObject|OrganizationCode", ShortCaption = "Organization", Caption = "Organization Code")]
		public ZString OrganizationCode => Mapping.Organization?.OH_Code ?? ZString.Empty;

		public ZPropertyInfo OrganizationCodeInfo => this.GetZPropertyInfo(Schema.OrganizationCode);

		#endregion

		#region CustomsOfficeCode

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.STATACREQDOCSendingObject|CustomsOfficeCode", ShortCaption = "Customs Office", Caption = "Customs Office Code")]
		public ZString CustomsOfficeCode => Mapping.CustomsOfficeCode;

		public ZPropertyInfo CustomsOfficeCodeInfo => this.GetZPropertyInfo(Schema.CustomsOfficeCode);

		#endregion

		#region FinancialAccountNumber

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.STATACREQDOCSendingObject|FinancialAccountNumber", ShortCaption = "FAN", MediumCaption = "Financial Acc. No.", Caption = "Financial Account Number (FAN)")]
		public ZString FinancialAccountNumber => Mapping.FinancialAccountNumber;

		public ZPropertyInfo FinancialAccountNumberInfo => this.GetZPropertyInfo(Schema.FinancialAccountNumber);

		#endregion

		public ZString AgentCodeLinkedToFAN => Mapping.Organization?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.AgentCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

		public ZString AgentDualProfileCodeLinkedToFAN => Mapping.Organization?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

		#region StartDate

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.STATACREQDOCSendingObject|StartDate", ShortCaption = "Start Date", Caption = "Statement Start Date")]
		public ZDate StartDate
		{
			get
			{
				var result = ZDate.Today;

				var accountStartDay = Mapping.AccountStartDay;
				if (accountStartDay > 0)
				{
					if (result.Day < accountStartDay)
					{
						result = result.AddMonths(-1);
					}
					var daysInMonth = DateTime.DaysInMonth(result.Year, result.Month);
					result = new ZDate(result.Year, result.Month, Math.Min(daysInMonth, accountStartDay));
				}

				return result;
			}
		}

		public ZPropertyInfo StartDateInfo => this.GetZPropertyInfo(Schema.StartDate);

		#endregion

		#region EndStart

		[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ZA.Business.STATACREQDOCSendingObject|EndDate", ShortCaption = "End Date", Caption = "Statement End Date")]
		public ZDate EndDate => ZDate.Today;

		public ZPropertyInfo EndDateInfo => this.GetZPropertyInfo(Schema.EndDate);

		#endregion

		#endregion

		#region IEDIMessageCollectionProvider

		public Messaging.Business.EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new Messaging.Business.EDIMessageCollection(this);
				}
				return messages;
			}
		}
		Messaging.Business.EDIMessageCollection messages;

		#endregion

		#region IEDIFACTMessageAttachee

		public ZString MessageStatus { get; set; } = ZString.Empty;

		public ZString JobStatus { get; set; } = ZString.Empty;

		public ZString JobIdentification => ZString.Empty;

		public BusinessObject TopLevelBusinessObject => this;

		public void AddMessage(EDIMessage message)
		{
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion
	}
}
