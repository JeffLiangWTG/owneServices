using System;

namespace CargoWise.eHub.DataAccess.Integration
{
	public static class DataAccessFactories
	{
		private const string INBOX_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.InboxAccessor, Version=3.0.0.0";
		private const string OUTBOX_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.OutboxAccessor, Version=3.0.0.0";
		private const string PARTY_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.PartyAccessor, Version=3.0.0.0";
		private const string TRANSFORM_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.TransformAccessor, Version=3.0.0.0";
		private const string EXCEPTIONS_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.ExceptionsAccessor, Version=3.0.0.0";
		private const string SECURITY_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.SecurityAccessor, Version=3.0.0.0";
		private const string REGISTRY_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.RegistryAccessor, Version=3.0.0.0";
		private const string SUBSCRIPTION_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.SubscriptionAccessor, Version=3.0.0.0";
		private const string ENTERPRISE_EXE_DETAIL_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.EnterpriseExeDetailAccessor, Version=3.0.0.0";
		private const string EHUBCLIENTSYSTEM_ACCESSOR_KEY = "CargoWise.eHub.DataAccess.EHubClientSystemAccessor, Version=3.0.0.0";

		#region Party Accessor

		public static IPartyAccessor NewPartyAccessorInstance()
		{
			return NewPartyAccessorInstance(PARTY_ACCESSOR_KEY);
		}

		public static IPartyAccessor NewPartyAccessorInstance(string key)
		{
			Type accessorType = TypeCache.Retrieve(key);
			return Activator.CreateInstance(accessorType) as IPartyAccessor;
		}

		#endregion

		#region Transform Accessor

		public static ITransformAccessor NewTransformAccessorInstance()
		{
			return NewTransformAccessorInstance(TRANSFORM_ACCESSOR_KEY);
		}

		public static ITransformAccessor NewTransformAccessorInstance(string key)
		{
			Type accessorType = TypeCache.Retrieve(key);
			return Activator.CreateInstance(accessorType) as ITransformAccessor;
		}

		#endregion

		#region Exceptions Accessor

		public static IExceptionsAccessor NewExceptionsAccessorInstance()
		{
			return NewExceptionsAccessorInstance(EXCEPTIONS_ACCESSOR_KEY);
		}

		public static IExceptionsAccessor NewExceptionsAccessorInstance(string key)
		{
			Type accessorType = TypeCache.Retrieve(key);
			return Activator.CreateInstance(accessorType) as IExceptionsAccessor;
		}

		#endregion

		#region Outbox Accessor

		public static IOutboxAccessor NewOutboxAccessorInstance()
		{
			Type accessorType = TypeCache.Retrieve(OUTBOX_ACCESSOR_KEY);
			return Activator.CreateInstance(accessorType) as IOutboxAccessor;
		}

		#endregion

		#region Inbox Accessor

		public static IInboxAccessor NewInboxAccessorInstance()
		{
			Type accessorType = TypeCache.Retrieve(INBOX_ACCESSOR_KEY);
			return Activator.CreateInstance(accessorType) as IInboxAccessor;
		}

		#endregion

		#region Security Accessor

		public static ISecurityAccessor NewSecurityAccessorInstance()
		{
			Type accessorType = TypeCache.Retrieve(SECURITY_ACCESSOR_KEY);
			return Activator.CreateInstance(accessorType) as ISecurityAccessor;
		}

		#endregion

		#region Registry Accessor

		public static IRegistryAccessor NewRegistryAccessorInstance()
		{
			Type accessorType = TypeCache.Retrieve(REGISTRY_ACCESSOR_KEY);
			return Activator.CreateInstance(accessorType) as IRegistryAccessor;
		}

		public static IRegistryAccessor NewRegistryAccessorInstance(string key)
		{
			Type accessorType = TypeCache.Retrieve(key);
			return Activator.CreateInstance(accessorType) as IRegistryAccessor;
		}

		#endregion

		#region Subscription Accessor

		public static ISubscriptionAccessor NewSubscriptionAccessorInstance()
		{
			Type accessorType = TypeCache.Retrieve(SUBSCRIPTION_ACCESSOR_KEY);
			return Activator.CreateInstance(accessorType) as ISubscriptionAccessor;
		}

		public static ISubscriptionAccessor NewSubscriptionAccessorInstance(string key)
		{
			Type accessorType = TypeCache.Retrieve(key);
			return Activator.CreateInstance(accessorType) as ISubscriptionAccessor;
		}

		#endregion

		#region EnterpriseExeDetail Accessor

		public static IEnterpriseExeDetailAccessor NewEnterpriseExeDetailAccessorInstance()
		{
			Type accessorType = TypeCache.Retrieve(ENTERPRISE_EXE_DETAIL_ACCESSOR_KEY);
			return Activator.CreateInstance(accessorType) as IEnterpriseExeDetailAccessor;
		}

		public static IEnterpriseExeDetailAccessor NewEnterpriseExeDetailAccessorInstance(string key)
		{
			Type accessorType = TypeCache.Retrieve(key);
			return Activator.CreateInstance(accessorType) as IEnterpriseExeDetailAccessor;
		}

		#endregion

		#region EHubClientSystem Accessor

		public static IEHubClientSystemAccessor NewEHubClientSystemAccessorInstance()
		{
			Type accessorType = TypeCache.Retrieve(EHUBCLIENTSYSTEM_ACCESSOR_KEY);
			return Activator.CreateInstance(accessorType) as IEHubClientSystemAccessor;
		}

		#endregion

	}
}
