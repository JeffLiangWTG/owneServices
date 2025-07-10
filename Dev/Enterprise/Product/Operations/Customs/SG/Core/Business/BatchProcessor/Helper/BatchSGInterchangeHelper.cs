using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ILoggingInformation = Enterprise.Integration.BatchProcessor.ILoggingInformation;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor
{
	public abstract class BatchSGInterchangeHelper
	{
		protected BatchSGInterchangeHelper(LoggingInformation logger)
		{
			this.logger = logger;
		}
		protected LoggingInformation logger;

		public virtual bool IsEnvironmentDataValid()
		{
			return true;
		}

		public void VerboseLog(ILoggingInformation log, string logmessage)
		{
			if (ShowVerboseLogging)
			{
				log.Log(logmessage);
			}
		}

		public virtual bool ShowVerboseLogging
		{
			get { return false; }
		}

		public GlbStaffCollection GetValidBrokerMailboxes()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(GlbStaff));
			filter.AddToFilter(GlbStaffSchema.GS_IsActive, true);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbExternalPassword), GlbExternalPasswordSchema.GP_GS);
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordType);
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_UserID, SQLComparisonOperator.NotEqual, "");
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_CurrentPassword, SQLComparisonOperator.NotEqual, "");
			subQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, Core.Constants.PasswordOK);

			filter.AddSubQuery(subQuery, JoinCondition.And);

			GlbStaffCollection brokerCollection = new GlbStaffCollection(factory, filter);

			if (brokerCollection.Count == 0)
			{
				logger.Log(string.Format(CultureInfo.InvariantCulture, "No registered, active {0} brokers exist in Company: {1}", ApplicationDescription, GlbCompany.CurrentCompany.GC_Code));
			}

			return brokerCollection;
		}

		#region Test Connection Command

		public virtual bool UseTestConnection => false;

		#endregion

		#region Mailbox Checker

		public CheckBrokerMailbox MailboxChecker
		{
			get { return mailboxChecker ?? (mailboxChecker = new CheckBrokerMailbox(logger, this)); }
		}
		CheckBrokerMailbox mailboxChecker;

		#endregion

		#region GetGlbExternalPassword

		public GlbExternalPassword_SGv4 GetGlbExternalPassword(GlbStaff broker) => GetGlbExternalPassword(SGGlbStaffWrapper.Get(broker));
		public abstract GlbExternalPassword_SGv4 GetGlbExternalPassword(SGGlbStaffWrapper brokerWrapper);

		public abstract ZString PasswordType { get; }
		public abstract ZString ApplicationDescription { get; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public abstract ZString[] ApplicationCodes { get; }

		#endregion
	}
}
