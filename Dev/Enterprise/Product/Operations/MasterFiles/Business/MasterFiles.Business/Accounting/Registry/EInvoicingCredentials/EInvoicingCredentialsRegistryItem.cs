using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.EInvoicingCredentialsRegistryItem;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingCredentialsRegistryItem : StronglyTypedRegistryItem<EInvoicingCredentials>
	{
		[Flags]
		public enum Behavior
		{
			Default = 0,
			SetPasswordChar = 1,
			HasAPIKey = 2
		}

		public EInvoicingCredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, Behavior behavior, IAPIKeyGeneratorStrategy apiKeyGenerator = null)
			: base(new EInvoicingCredentialsRegistryItemImpl(name, category, caption, hint, storage, option, behavior, apiKeyGenerator))
		{
		}

		public EInvoicingCredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new EInvoicingCredentialsRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}

		public EInvoicingCredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, EInvoicingCredentials defaultValue)
			: base(new EInvoicingCredentialsRegistryItemImpl(name, category, caption, hint, storage, option, defaultValue))
		{
		}

		public class EInvoicingCredentialsRegistryItemImpl : RegistryItemImpl
		{
			readonly EInvoicingCredentials defaultValue;

			public EInvoicingCredentialsRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, Behavior behavior, IAPIKeyGeneratorStrategy apiKeyGenerator)
				: base(name, category, caption, hint, new EInvoicingCredentialsRegistryDataType(behavior, apiKeyGenerator), storage, option)
			{
			}

			public EInvoicingCredentialsRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new EInvoicingCredentialsRegistryDataType(), storage, option)
			{
			}

			public EInvoicingCredentialsRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, EInvoicingCredentials defaultValue)
				: this(name, category, caption, hint, storage, option)
			{
				this.defaultValue = defaultValue;
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
				=> defaultValue ?? new EInvoicingCredentials();
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.EInvoicingCredentialsRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	public class EInvoicingCredentialsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EInvoicingCredentials>
	{
		public EInvoicingCredentialsRegistryDataType(Behavior behavior = Behavior.Default,
			IAPIKeyGeneratorStrategy apiKeyGenerator = null)
		{
			APIKeyGenerator = apiKeyGenerator;
			Behavior = behavior;
		}

		public IAPIKeyGeneratorStrategy APIKeyGenerator { get; }
		public Behavior Behavior { get; }
	}

	public static class EInvoicingCredentialsRegistryItemBehaviorExtensions
	{
		public static bool HasAPIKeyFlag(this Behavior behavior)
		{
			return (behavior & Behavior.HasAPIKey) == Behavior.HasAPIKey;
		}

		public static bool SetPasswordCharFlag(this Behavior behavior)
		{
			return (behavior & Behavior.SetPasswordChar) == Behavior.SetPasswordChar;
		}
	}
}
