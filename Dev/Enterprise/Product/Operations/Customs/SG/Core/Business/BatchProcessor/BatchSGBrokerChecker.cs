using System;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public class BatchSGBrokerChecker
	{
		public BatchSGBrokerChecker(LoggingInformation logger, BatchSGInterchangeHelper helper)
		{
			this.logger = logger;
			this.helper = helper;
		}

		readonly BatchSGInterchangeHelper helper;

		public void CheckStaff()
		{
			foreach (GlbStaff broker in GetBrokers())
			{
				CheckBroker(broker);
			}

			if (InvalidBrokers.Length > 0)
			{
				logger.Log(InvalidBrokers.ToStringWithNewLineBetweenAppends());
				SendEmail();
			}
		}

		GlbStaffCollection GetBrokers()
		{
			GlbStaffCollection result = new GlbStaffCollection(Factory);

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(GlbStaff));
			filter.AddToFilter(GlbStaffSchema.GS_IsActive, true);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.GP_GS);
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.NotEqual, "");
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, helper.PasswordType);

			filter.AddSubQuery(subQuery, JoinCondition.And);

			result.AdditionalFilter = filter;

			return result;
		}

		void SendEmail()
		{
			if (SGCustomsDataRegistry.Instance.LastSentEmailWithBrokerErrors.Value == DateTime.MinValue || SGCustomsDataRegistry.Instance.LastSentEmailWithBrokerErrors.Value < Env.Time.CurrentLocalDateTime.Date)
			{
				try
				{
					string emailTemplateHtml;

					using (Stream stream = typeof(CheckBrokerMailbox).Assembly.GetManifestResourceStream("Enterprise.Customs.SG.V4.Business.BatchProcessor.HtmlTemplates.StaffErrors.htm"))
					{
						emailTemplateHtml = new StreamReader(stream).ReadToEnd();
					}
					emailTemplateHtml = emailTemplateHtml.Replace("<!--DynamicHtml-->", InvalidBrokers.ToStringWithDelimiterBetweenAppends("<br />"));

					HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
					EmailDef email = emailSender.CreateEmail(string.Format(CultureInfo.InvariantCulture, "Errors exist in the following Broker (s) {0} registration details", helper.ApplicationDescription), emailTemplateHtml);
					GlbGroup emailGroup = Factory.Load<GlbGroup>(SGCustomsDataRegistry.Instance.SendErrorsToGroup.Value);

					if (emailGroup != null && emailGroup.Staff != null)
					{
						foreach (GlbStaff recipient in emailGroup.Staff)
						{
							email.AddRecipientForSystemCommunication(recipient.GS_EmailAddress);
						}

						if (email.Recipients.Count > 0)
						{
							Env.OutgoingCustomsMailManager.CreateAndSave(email);
						}
					}
				}
				catch (Exception e) when (!e.IsCriticalException()) //if email fails, not a critical problem, inform us and Batch Processor continues working
				{
					ErrorReporter.ReportOnce("BatchProcessorChecer.SendEmail", e.Message);
				}

				SGCustomsDataRegistry.Instance.LastSentEmailWithBrokerErrors.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, Env.Time.CurrentLocalDateTime.Date);
			}
		}

		void CheckBroker(GlbStaff broker)
		{
			var password = helper.GetGlbExternalPassword(broker);
			if (password != null && password.GP_PasswordStatus != Core.Constants.PasswordOK)
			{
				InvalidBrokers.Append(string.Format(CultureInfo.InvariantCulture, "Broker: {0} - {1} has invalid {2} password details; Password Status = {3}"
					, broker.GS_Code, broker.GS_FullName, helper.ApplicationDescription, password.GP_PasswordStatusDescription.Trim()));
			}
		}

		#region Implementation

		ZStringBuilder InvalidBrokers
		{
			get { return invalidBrokers ?? (invalidBrokers = new ZStringBuilder()); }
		}
		ZStringBuilder invalidBrokers;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		readonly LoggingInformation logger;

		#endregion
	}
}
