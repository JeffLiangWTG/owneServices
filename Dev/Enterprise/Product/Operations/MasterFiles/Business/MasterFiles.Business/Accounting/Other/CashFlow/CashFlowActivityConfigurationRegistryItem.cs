using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CashFlowActivityConfigurationRegistryItem : TranslatableRegistryItem<CashFlowActivityConfigurationCollection, CashFlowActivityConfigurationCollection>
	{
		public CashFlowActivityConfigurationRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				RegistryOptions options)
			: base(new CashFlowActivityConfigurationRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		protected override CashFlowActivityConfigurationCollection Convert(CashFlowActivityConfigurationCollection value)
		{
			foreach (CashFlowActivityConfiguration item in value)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
				item.ActivityDescription = GetMultilingualString(item.EnglishActivityDescription);
				item.SetCustomizedDataCaptionSource(this);
			}
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				return CashFlowCodeLists.CashFlowTypeList.Cast<IMultilingualDescription>().Select(item => (ResourceString)item.MultilingualDescription);
			}
		}

		public override IEnumerable<string> GetCaptions(CashFlowActivityConfigurationCollection value)
		{
			var descriptions = value.Cast<CashFlowActivityConfiguration>()
						.Select(i => i.EnglishDescription.ToString().Trim()).Distinct()
						.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			var activityDescriptions = value.Cast<CashFlowActivityConfiguration>()
						.Select(i => i.EnglishActivityDescription.ToString().Trim()).Distinct()
						.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			return descriptions.Concat(activityDescriptions).Distinct();
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override int MaxLength
		{
			get { return CashFlowActivityConfiguration.Schema.DescriptionMaxLength; }
		}

		class CashFlowActivityConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public CashFlowActivityConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new CashFlowActivityConfigurationRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				CashFlowActivityConfigurationCollection defaultCollection = new CashFlowActivityConfigurationCollection();

				defaultCollection.SuspendValidation();

				CashFlowActivityConfiguration configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.XXX;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Undefined;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.NON;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.NonCash;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.CSH;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Cash;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.EXX;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Exchange;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.O01;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Operating;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.O02;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Operating;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.O03;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Operating;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.O04;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Operating;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.O05;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Operating;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.O06;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Operating;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.I01;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Investing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.I02;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Investing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.I03;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Investing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.I04;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Investing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.I05;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Investing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.F01;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.F02;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.F03;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.F04;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.F05;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.F06;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.F07;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing;

				configuration = defaultCollection.AddNew();
				configuration.Code = CashFlowCodeLists.Codes.F08;
				configuration.ActivityType = CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Financing;

				defaultCollection.ResumeValidation();
				return defaultCollection;
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CashFlowActivityConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	class CashFlowActivityConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CashFlowActivityConfigurationCollection>
	{
		public CashFlowActivityConfigurationRegistryDataType()
		{
		}
	}
}
