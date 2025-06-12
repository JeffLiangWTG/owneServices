using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Xml.XPath;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Core.Transforms.Helper
{
	public class DataModelAccessor
	{
		readonly ContextFactory<eHubTransactionsContext> contextFactory;

		public DataModelAccessor() : this(new ContextFactory<eHubTransactionsContext>()) { }

		internal DataModelAccessor(ContextFactory<eHubTransactionsContext> contextFactory)
		{
			this.contextFactory = contextFactory;
			Database.SetInitializer<eHubTransactionsContext>(null);
		}

        #region eHubClientRegistrations

        private eHubClientRegistration GetClientRegistration(string clientID, string qualifier, string registrationTypeID)
        {
            if (clientID == null) throw new ArgumentNullException("clientID");
            if (clientID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "clientID");
            if (registrationTypeID == null) throw new ArgumentNullException("registrationTypeID");
            if (registrationTypeID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "registrationTypeID");
            if (String.IsNullOrWhiteSpace(qualifier))
                qualifier = null;

            var clientRegistration = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
            {
                using (var context = contextFactory.CreateContext())
                {
                    var registration = context.eHubClientRegistrations.OrderByDescending(r=>r.CX_Qualifier).FirstOrDefault(
                                r => r.eHubClient.CC_ID == clientID &&
                                    r.eHubRegistrationType.RT_ID == registrationTypeID && 
                                    (r.CX_Qualifier == qualifier || r.CX_Qualifier == null));

                    return registration;
                }
            });

            return clientRegistration;
        }

		public virtual string GetClientRegistrationCode(string clientID, string qualifier, string registrationTypeID)
		{
            return GetClientRegistration(clientID, qualifier, registrationTypeID)?.CX_Code ?? string.Empty;
		}

        public virtual string GetClientRegistrationAttri1AsString(string clientID, string qualifier, string registrationTypeID)
        {
            return GetClientRegistration(clientID, qualifier, registrationTypeID)?.CX_Attr1 ?? string.Empty;
        }


        public virtual eHubClientRegistration GetClientRegistration(string clientID, string registrationTypeID)
		{
			if (clientID == null) throw new ArgumentNullException("clientID");
			if (clientID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "clientID");
			if (registrationTypeID == null) throw new ArgumentNullException("registrationTypeID");
			if (registrationTypeID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "registrationTypeID");

			var clientRegistration = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				using (var context = contextFactory.CreateContext())
				{
					var eHubClientReg = from r in context.eHubClientRegistrations
										where r.eHubClient.CC_ID == clientID && r.eHubRegistrationType.RT_ID == registrationTypeID
										select r;

					return eHubClientReg.FirstOrDefault();
				}
			});
			return clientRegistration;
		}

		public virtual string GetClientRegistrationFlag1AsString(string clientID, string qualifier, string registrationTypeID)
		{
			if (String.IsNullOrWhiteSpace(qualifier))
				qualifier = null;

            return GetClientRegistration(clientID, qualifier, registrationTypeID)?.CX_Flag1?.ToString() ?? string.Empty;
		}

		public virtual string GeteHubIDByQualifier(string qualifier, string registrationTypeID)
		{
			if (qualifier == null) throw new ArgumentNullException("qualifier");
			if (qualifier.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "qualifier");
			if (registrationTypeID == null) throw new ArgumentNullException("registrationTypeID");
			if (registrationTypeID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "registrationTypeID");

			var eHubID = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				using (var context = contextFactory.CreateContext())
				{
					var eHubIds = from r in context.eHubClientRegistrations
								  where r.eHubRegistrationType.RT_ID == registrationTypeID
										&& (r.CX_Qualifier == qualifier)
								  orderby r.CX_Qualifier descending
								  select r.eHubClient.CC_ID;

					return eHubIds.First();
				}
			});
			return eHubID;
		}

		public virtual string GeteHubIDByCode(string code, string registrationTypeID)
		{
			return GeteHubIDByCode(code, registrationTypeID, true);
		}

		public virtual string GeteHubIDByCode(string code, string registrationTypeID, bool enableCodeCheck)
		{
			if (enableCodeCheck)
			{
				if (code == null)
					throw new ArgumentNullException("code");
				if (code.Trim() == String.Empty)
					throw new ArgumentException("Argument is empty or whitespace", "code");
			}
			if (registrationTypeID == null)
				throw new ArgumentNullException("registrationTypeID");
			if (registrationTypeID.Trim() == String.Empty)
				throw new ArgumentException("Argument is empty or whitespace", "registrationTypeID");

			var eHubID = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				using (var context = contextFactory.CreateContext())
				{
					var eHubIds = from r in context.eHubClientRegistrations
								  where r.eHubRegistrationType.RT_ID == registrationTypeID
										&& (r.CX_Code == code)
								  orderby r.CX_Code descending
								  select r.eHubClient.CC_ID;

					return eHubIds.FirstOrDefault();
				}
			});

			if (enableCodeCheck)
			{
				return eHubID;
			}
			else
			{
				return eHubID ?? string.Empty;
			}
		}

		#endregion

		#region eHubClientSystemRegistrations

		public virtual eHubClientSystemRegistration GetClientSystemRegistration(string clientSystemID, string qualifier, string registrationTypeID)
		{
			if (clientSystemID == null || clientSystemID.Trim() == String.Empty) throw new ArgumentException("Argument is null or empty", "clientID");
			if (registrationTypeID == null || registrationTypeID.Trim() == String.Empty) throw new ArgumentException("Argument is null or empty", "registrationTypeID");
			if (String.IsNullOrWhiteSpace(qualifier))
				qualifier = null;

			var clientSystemRegistration = DatabaseAccessHelpers.AccessDatabaseWithRetries(() =>
			{
				using (var context = contextFactory.CreateContext())
				{
					return context.eHubClientSystemRegistrations
						.Where(r => r.eHubClientSystem.EH_ID == clientSystemID
									&& r.eHubRegistrationType.RT_ID == registrationTypeID
									&& (r.CD_Qualifier == qualifier || r.CD_Qualifier == null))
						.OrderByDescending(x => x.CD_Qualifier)
						.FirstOrDefault();
				}
			});
			return clientSystemRegistration;
		}
		#endregion

		#region eHubSubscriptions

		public virtual XPathNodeIterator SelectSubscriptions(string subscriptionType, string senderId, string value)
		{
			return DatabaseAccessHelpers.AccessDatabaseWithRetries(() => SubscriptionAccessor.SelectSubscriptions(subscriptionType, senderId, value));
		}

		public virtual XPathNodeIterator SelectSubscriptionsByValue(string subscriptionType, string value, string referenceType= null)
		{
			return  DatabaseAccessHelpers.AccessDatabaseWithRetries(() => SubscriptionAccessor.SelectSubscriptionsByValue(subscriptionType, value, referenceType));
		}

		public virtual ISubscriptionAccessor SubscriptionAccessor
		{
			get { return subscriptionAccessor ?? (subscriptionAccessor = new SubscriptionAccessor()); }
		}

		ISubscriptionAccessor subscriptionAccessor;

		#endregion

		#region eHubSubscriptionValues

		public virtual void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() => InsertSubscriptionValue(subscriptionType, providerID, subscriberID, value, null, null),null);
		}

		public virtual void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() => InsertSubscriptionValue(subscriptionType, providerID, subscriberID, value, reference, null), null);
		}

		public virtual void InsertSubscriptionValueWithRetries(string subscriptionType, string providerID, string subscriberID, string value, string reference = null, string referenceType = null)
		{
			DatabaseAccessHelpers.AccessDatabaseWithRetries(() => InsertSubscriptionValue(subscriptionType, providerID, subscriberID, value, reference, referenceType), null);
		}

		public virtual void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference, string referenceType)
		{
			if (subscriptionType == null) throw new ArgumentNullException("subscriptionType");
			if (subscriptionType.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "subscriptionType");
			if (providerID == null) throw new ArgumentNullException("providerID");
			if (providerID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "providerID");
			if (subscriberID == null) throw new ArgumentNullException("subscriberID");
			if (subscriberID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "subscriberID");
			if (value == null) throw new ArgumentNullException("value");

			using (var context = contextFactory.CreateContext())
			{
				var subType = (from s in context.eHubSubscriptionTypes where s.ST_ID == subscriptionType select s).FirstOrDefault();
				if (subType == null) throw new ArgumentException(String.Format("Subscription type not found: '{0}'", subscriptionType));

				var provider = (from c in context.eHubClients where c.CC_ID == providerID select new { c.CC_PK }).FirstOrDefault();
				if (provider == null) throw new ArgumentException(String.Format("Provider client ID not found: '{0}'", providerID));

				var subscriber = (from c in context.eHubClients where c.CC_ID == subscriberID select new { c.CC_PK }).FirstOrDefault();
				if (subscriber == null) throw new ArgumentException(String.Format("Subscriber client ID not found: '{0}'", subscriberID));

				using (var tran = context.BeginTransaction())
				{
					var subValue = (from v in context.eHubSubscriptionValues
									where v.SV_ST == subType.ST_PK
										  && v.SV_CC_Sender == provider.CC_PK
										  && v.SV_CC_Recipient == subscriber.CC_PK
										  && v.SV_Value == value
										  && v.SV_ReferenceType == referenceType
									select v).FirstOrDefault();
					if (subValue == null)
					{
						subValue = new eHubSubscriptionValue
						{
							SV_PK = Guid.NewGuid(),
							SV_ST = subType.ST_PK,
							SV_CC_Sender = provider.CC_PK,
							SV_CC_Recipient = subscriber.CC_PK,
							SV_Value = value,
							SV_Reference = reference,
							SV_ReferenceType = referenceType,
							SV_SubscribedUTC = DateTime.UtcNow,
							SV_ExpiryUTC = subType.ST_ExpiryDays.HasValue ? DateTime.UtcNow.AddDays(subType.ST_ExpiryDays.Value) : (DateTime?)null
						};
						context.eHubSubscriptionValues.Add(subValue);
						try
						{
							context.SaveChanges();
						}
						catch (DataException ex)
						{
							if (ex.IsDuplicateKeyViolation())
								return;
							else
								throw;
						}
					}
					else
					{
						if (subType.ST_ExpiryDays.HasValue)
						{
							subValue.SV_Reference = reference;
							subValue.SV_ExpiryUTC = DateTime.UtcNow.AddDays(subType.ST_ExpiryDays.Value);
							context.SaveChanges();
						}
						else
						{
							return;
						}
					}
					tran.Commit();
				}
			}
		}

		#endregion
	}
}
