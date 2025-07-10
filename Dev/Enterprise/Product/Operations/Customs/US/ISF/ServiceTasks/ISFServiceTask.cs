using System;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.US.ISF.ServiceTasks.ISFServiceTask.ServiceTaskCode,
	"United States ISF Customs Messaging",
	"USC",
	typeof(Enterprise.Customs.US.ISF.ServiceTasks.ISFServiceTask),
	MinimumPeriod = "30Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ISF.ServiceTasks.ISFServiceTask.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + CBPEDIInterchange.ApplicationCodes.USCustomsImport,
				EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse,
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"US Customs ISF response messages inbound"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ISF.ServiceTasks.ISFServiceTask.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + CBPEDIInterchange.ApplicationCodes.USCustomsImport,
				EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory,
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"US Customs ISF status messages inbound"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.US.ISF.ServiceTasks.ISFServiceTask.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + CBPEDIInterchange.ApplicationCodes.USCustomsImport,
				EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling,
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"US Customs ISF messages inbound"
	)]

namespace Enterprise.Customs.US.ISF.ServiceTasks
{
	public class ISFServiceTask : Customs.ServiceTasks.NudgeCustomsServiceTask
	{
		internal const string ServiceTaskCode = "ISF";
		protected override string CurrentServiceTaskCode => ServiceTaskCode;
		protected override void RunMainTask(CancellationToken token)
		{
			if (!ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.UCMPServiceTask, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now))
			{
				foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
				{
					token.ThrowIfCancellationRequested();
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						var processor = new ISFIncomingMessageProcessor() { Logger = this.Logger };
						processor.ExecuteBatch(token);
					}
				}
			}

			var branchForEmail = GlbBranch.GetFirstActiveBranch();
			if (branchForEmail != null)
			{
				using (DisposableEnvironment.ForBranch(branchForEmail.PK.ToGuid()))
				{
					GenerateISFMessageUsageReportIfNeeded();
				}
			}
		}

		void GenerateISFMessageUsageReportIfNeeded()
		{
			var today = ZDateTime.Today;
			try
			{
				if (ShouldRunISFMessageUsageReport(today))
				{
					var endDate = new DateTime(today.Year, today.Month, 1);
					try
					{
						var startDate = endDate.AddMonths(-3);
						var factory = new BusinessObjectFactory();
						var collection = new DynamicBusinessObjectCollection(factory);
						collection.Load(ImportSecurityFilingSQLLoad, new ZSqlParameter[] {
								ZSqlParameter.New("@ApplicationCode", "USI", EDIMessageSchema.EM_ApplicationCode),
								ZSqlParameter.New("@ISFAccepted", "%ISF ACCEPTED%", EDIMessageSchema.EM_MessageText),
								ZSqlParameter.New("@MessageType", "SN", EDIMessageSchema.EM_MessageType),
								ZSqlParameter.New("@MessageSubType", "ADD", EDIMessageSchema.EM_MessageSubType),
								ZSqlParameter.New("@ReportDateStart", startDate, EDIMessageSchema.EM_SystemCreateTimeUtc),
								ZSqlParameter.New("@ReportDateEnd", endDate, EDIMessageSchema.EM_SystemCreateTimeUtc)
							});

						var body = ZString.Empty;
						AttachmentDef attachment = null;
						if (collection.Count > 0)
						{
							var creator = new HtmlTableCreator(ColumnHeadings);

							var attachmentBuilder = new ZStringBuilder(new OCsvLine(ColumnHeadings).ToString());
							foreach (DynamicBusinessObject bizObj in collection)
							{
								var companyPk = new ZGuid(bizObj["CompanyPK"]);
								var company = factory.Load<GlbCompany>(companyPk);
								InsertData(creator, attachmentBuilder, company, bizObj["Year"], bizObj["Month"], bizObj["PortCode"], bizObj["Count"]);
							}
							body = creator.ToHtml();
							attachment = new AttachmentDef("DataReport.CSV", Encoding.ASCII.GetBytes(attachmentBuilder.ToStringWithNewLineBetweenAppends()));
						}
						else
						{
							body = "No record of transactions were found.";
						}
						var emailSender = new HtmlNotificationEmailSender();
						var email = emailSender.CreateEmail("ISF Data Report", body);
						email.AddRecipientForSystemCommunication("transactionbilling@cargowise.com");
						if (attachment != null)
						{
							email.Attachments.Add(attachment);
						}
						Env.OutgoingCustomsMailManager.CreateAndSave(email);
					}
					finally
					{
						ISFRegistry.Instance.ImporterSecurityFilingMessageUsageReportDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, endDate.AddMonths(1));
					}
				}
			}
			catch (EmailHasNoFromAddressException ex)
			{
				Logger.LogError("Could not send an email because the 'from address' was empty. Message: " + ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Generating ISF Message Usage Report - " + today.ToString("MMM yyyy"), "Gerating ISF Message Usage Report", ex);
			}
		}

		bool ShouldRunISFMessageUsageReport(ZDateTime today) => ISFRegistry.Instance.ImporterSecurityFilingMessageUsageReportDate.Value < today.ToDateTime();

		void InsertData(HtmlTableCreator creator, ZStringBuilder attachmentBuilder, GlbCompany company, object year, object month, object port, object count)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			creator.WriteRow(company.GC_Name, company.GC_Code, registrationKey.EnterpriseCode, registrationKey.ServerCode, year, month, port, count);
			attachmentBuilder.Append(new OCsvLine(new string[] { company.GC_Name, company.GC_Code, registrationKey.EnterpriseCode, registrationKey.ServerCode, year.ToString(), month.ToString(), port.ToString(), count.ToString() }).ToString());
		}

		string[] ColumnHeadings => new string[] { "Company Name", "Company Code", "Enterprise Code", "Server Code", "Year", "Month", "Port Code", "Count" };

		const string ImportSecurityFilingSQLLoad = @"
			select
				GC_PK [CompanyPK],
				DATEPART(YEAR, EM_SystemCreateTimeUtc) [Year],
				DATEPART(MONTH, EM_SystemCreateTimeUtc) [Month],
				SUBSTRING(EM_MessageText, 4, 4) AS PortCode,
				COUNT(*) [Count]
			from dbo.EDIMessage join dbo.GlbBranch on (EM_GB = GB_PK) join dbo.GlbCompany on (GB_GC = GC_PK)
			where EM_ApplicationCode = @ApplicationCode
			and EM_MessageType = @MessageType
			and EM_MessageText LIKE @ISFAccepted
			and EM_MessageSubType = @MessageSubType
			and EM_SystemCreateTimeUtc >= @ReportDateStart
			and EM_SystemCreateTimeUtc < @ReportDateEnd
			group by GC_PK, DATEPART(YEAR, EM_SystemCreateTimeUtc), DATEPART(MONTH, EM_SystemCreateTimeUtc), SUBSTRING(EM_MessageText, 4, 4)
			order by GC_PK, DATEPART(YEAR, EM_SystemCreateTimeUtc), DATEPART(MONTH, EM_SystemCreateTimeUtc), SUBSTRING(EM_MessageText, 4, 4)
			";
	}
}
