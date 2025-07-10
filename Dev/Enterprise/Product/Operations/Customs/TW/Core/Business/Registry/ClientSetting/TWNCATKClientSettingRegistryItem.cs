using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	public class TWNCATKClientSettingRegistryItem : StronglyTypedRegistryItem<TWNCATKClientSetting>
	{
		public TWNCATKClientSettingRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage
		) : base(new RegistryItemImpl(
			name, category, caption, hint, new TWNCATKClientSettingRegistryDataType(), storage
		))
		{
			OnUpdateAction = UpdateAction;
			OnAllValuesSavedAction = AllValuesSavedAction;
		}

		readonly Dictionary<Guid, ZString> settingsToUpdate = new Dictionary<Guid, ZString>();
		readonly HashSet<Guid> registeredSettings = new HashSet<Guid>();

		public BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory() { NameForDebugging = "TWCustomsRegistry" });
		BusinessObjectFactory factory;

		void UpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			var setting = (TWNCATKClientSetting)newValue;
			if (setting.ShouldRegisterEHubClient || setting.ShouldUnregisterEHubClient || registeredSettings.Contains(companyPk))
			{
				settingsToUpdate.NewOrUpdateIfExist(companyPk, setting.EHubClientID);
			}
		}

		void AllValuesSavedAction()
		{
			foreach (var setting in settingsToUpdate)
			{
				if (setting.Value.IsEmpty)
				{
					TWNCATKClientCredencialSender.SendSettingDelete(Factory, setting.Key);
					if (registeredSettings.Contains(setting.Key))
					{
						registeredSettings.Remove(setting.Key);
					}
				}
				else
				{
					TWNCATKClientCredencialSender.SendSettingCreateOrUpdate(Factory, setting.Key, setting.Value);
					if (!registeredSettings.Contains(setting.Key))
					{
						registeredSettings.Add(setting.Key);
					}
				}
			}
			settingsToUpdate.Clear();
			Factory.Save();
			Factory.CleanUp();
			factory = null;
		}

		protected override void DeleteValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var setting = GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);

			if (setting.ShouldUnregisterEHubClient)
			{
				settingsToUpdate.NewOrUpdateIfExist(companyPK, ZString.Empty);
			}
			base.DeleteValueCore(companyPK, branchPK, departmentPK);
		}
	}

	[RegistryEditor("Enterprise.Customs.TW.GUI.TWNCATKClientSettingRegistryItemEditor, Enterprise.Customs.TW.GUI")]
	public class TWNCATKClientSettingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TWNCATKClientSetting>
	{
		public TWNCATKClientSettingRegistryDataType()
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, TWNCATKClientSetting proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			ValidateMachineName(proposedValue, companyPK);
		}

		void ValidateMachineName(TWNCATKClientSetting proposedValue, Guid currentCompanyPK)
		{
			if (!proposedValue.MachineName.IsEmpty)
			{
				foreach (var company in TWCompanies)
				{
					if (company.PK.ToGuid() != currentCompanyPK)
					{
						var setting = TWCustomsDataRegistry.Instance.TWNCATKClientSetting.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
						if (setting.MachineName == proposedValue.MachineName)
						{
							proposedValue.MachineNameInfo.AddError(Res.GetString("62ea4629-d87a-4d84-b740-a8fcb351c187", "There is already a company which has a same Machine Name."));
						}
					}
				}
			}
		}

		IEnumerable<GlbCompany> TWCompanies => new GlbCompany.Loader(new BusinessObjectFactory()).LoadCompanies(Core.Constants.CountryCodes.Taiwan);

		public override bool IsDefaultValueImmutable => true;
	}
}
