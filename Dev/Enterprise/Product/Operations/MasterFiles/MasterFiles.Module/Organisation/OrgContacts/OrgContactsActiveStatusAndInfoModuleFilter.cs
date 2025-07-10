using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactsActiveStatusAndInfoModuleFilter : ModuleTextFilter
	{
		public OrgContactsActiveStatusAndInfoModuleFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
			DefaultActiveStatus = StatusActive;
		}

		public OrgContactsActiveStatusAndInfoModuleFilter(ZString description)
			: this(description, EmptyQuery)
		{
		}

		public static string StatusActive => Res.GetString("86f775b7-08ef-40ae-830a-61346ecffcd6", "Active");
		public static string StatusInactive => Res.GetString("9b4670bc-cba1-4808-a6ab-acea85ea9d5d", "Inactive");
		public static string StatusAll => Res.GetString("3c7b7675-05bb-461d-94be-7588b13d928c", "All");

		CodeDescriptionPairList activeStatusList;

		public CodeDescriptionPairList ActiveStatusList
		{
			get
			{
				if (activeStatusList == null)
				{
					activeStatusList = new CodeDescriptionPairList();
					activeStatusList.AddPair(StatusActive, Res.GetString("78b6fc94-f907-4a81-b23e-4a17322a78e9", "Show Active Only"));
					activeStatusList.AddPair(StatusInactive, Res.GetString("6750d737-c37c-40ae-9cec-489145c813e8", "Show Inactive Only"));
					activeStatusList.AddPair(StatusAll, Res.GetString("8aa9c089-a126-4a6c-a7eb-156bb81c5103", "Show all records"));
				}

				return activeStatusList;
			}
		}

		ZString defaultActiveStatus;

		public ZString DefaultActiveStatus
		{
			get
			{
				return defaultActiveStatus;
			}
			set
			{
				defaultActiveStatus = value;
				ActiveStatus = value;
			}
		}

		ZString activeStatus;

		[List("ActiveStatusList")]
		public ZString ActiveStatus
		{
			get
			{
				return activeStatus;
			}
			set
			{
				if (SetNonPersistentPropertyValue(ActiveStatusInfo, ref activeStatus, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateContactActiveStatus();
					}

					ActiveStatusInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ActiveStatusInfo => GetZPropertyInfo(Constants.ActiveStatus);

		protected override int Property_MaxLength => 50;

		#region Validation

		public new OrgContactsActiveStatusAndInfoModuleFilterValidation Validation =>
			(OrgContactsActiveStatusAndInfoModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgContactsActiveStatusAndInfoModuleFilterValidation(this);
		}

		#endregion

		#region Query

		static ZQuery EmptyQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			var query = base.GetQuery();
			if (!IsEmpty)
			{
				query.AddToFilter(GetActiveStatusQuery());
			}

			return query;
		}

		ZQuery GetActiveStatusQuery()
		{
			var query = new ZQuery();

			if (ActiveStatus.IsEmpty || !ActiveStatusList.ContainsCode(ActiveStatus))
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				if (ActiveStatus == StatusActive)
				{
					query.AddToFilter(OrgContactSchema.OC_IsActive, true);
				}
				else if (ActiveStatus == StatusInactive)
				{
					query.AddToFilter(OrgContactSchema.OC_IsActive, false);
				}

				var filterContactQuery = new ZQuery();
				filterContactQuery.AddToFilter(OrgContactSchema.OC_ContactName,
				SqlComparisonOperator, Property.SubstringSafe(0,
				OrgContactSchema.OC_ContactName.MaxLength));

				query.AddToFilter(filterContactQuery);
			}

			return query;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString(Constants.ActiveStatus, ActiveStatus);
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == Constants.ActiveStatus)
			{
				ActiveStatus = reader.ReadElementString(Constants.ActiveStatus);
			}
		}

		#endregion

		#region Implementation

		protected override bool IsEmptyCore => base.IsEmptyCore && Property.IsEmpty;

		protected override void ClearCore()
		{
			base.ClearCore();
			ActiveStatus = DefaultActiveStatus;
		}

		#endregion

		static class Constants
		{
			public const string ActiveStatus = "ActiveStatus";
		}
	}
}
