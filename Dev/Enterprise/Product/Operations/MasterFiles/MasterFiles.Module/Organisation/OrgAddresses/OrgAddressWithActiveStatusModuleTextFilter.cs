using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgAddressWithActiveStatusModuleTextFilter : ModuleTextFilter
	{
		public OrgAddressWithActiveStatusModuleTextFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
			DefaultActiveStatus = ActiveStatusList[StatusActive].Code;
		}

		internal OrgAddressWithActiveStatusModuleTextFilter(ZString description)
			: this(description, EmptyQuery)
		{
		}

		public static MultilingualString StatusActive => ResString.GetMultilingualString("01275565-5D0E-4192-AF08-2F90E6B2D0C6", "Active");
		public static MultilingualString StatusInactive => ResString.GetMultilingualString("8066AD35-4FDC-48FD-9C4E-62D421528567", "Inactive");
		public static MultilingualString StatusAll => ResString.GetMultilingualString("8D684BA9-CE51-49DD-BFC3-CDA64AF2ABB5", "All");

		CodeDescriptionPairList activeStatusList;

		public CodeDescriptionPairList ActiveStatusList
		{
			get
			{
				if (activeStatusList == null)
				{
					activeStatusList = new CodeDescriptionPairList();
					activeStatusList.AddPair(StatusActive, Res.GetString("0FE06377-CB49-435C-A43D-D039F023561B", "Show Active Only"));
					activeStatusList.AddPair(StatusInactive, Res.GetString("7E6BAEE9-6D5F-4D2D-8E10-98559834CB91", "Show Inactive Only"));
					activeStatusList.AddPair(StatusAll, Res.GetString("50AF5E33-AE32-4B23-ABFB-91CAC04B0E6D", "Show all records"));
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
						Validation.ValidateAddressActiveStatus();
					}

					ActiveStatusInfo.RefreshBinding();
				}
			}
		}

		ZString activeStatus;

		public ZPropertyInfo ActiveStatusInfo => GetZPropertyInfo(Constants.ActiveStatus);

		protected override int Property_MaxLength => 50;

		#region Validation

		public new OrgAddressWithActiveStatusModuleFilterValidation Validation => (OrgAddressWithActiveStatusModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgAddressWithActiveStatusModuleFilterValidation(this);
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
				if (StatusActive.EqualsUnresolvedOrLocalized(ActiveStatus, ignoreCase: false))
				{
					query.AddToFilter(OrgAddressSchema.OA_IsActive, true);
				}
				else if (StatusInactive.EqualsUnresolvedOrLocalized(ActiveStatus, ignoreCase: false))
				{
					query.AddToFilter(OrgAddressSchema.OA_IsActive, false);
				}
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

		protected override bool IsEmptyCore => base.IsEmptyCore && Property.IsEmpty && ComparisonOperator != ComparisonConstants.IsBlank && ComparisonOperator != ComparisonConstants.IsNotBlank;

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
