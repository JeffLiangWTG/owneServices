using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class AddressSourceAndJobNumberModuleFilter : ModuleTextFilter
	{
		public AddressSourceAndJobNumberModuleFilter(ZString description) : base(description, EmptyQuery)
		{
		}

		public CodeDescriptionPairList AddressSourceList => AddressSourceType.AddressSourcePairList;

		[List("AddressSourceList")]
		public ZString AddressSourceCode
		{
			get
			{
				return addressSourceCode;
			}
			set
			{
				if (SetNonPersistentPropertyValue(AddressSourceCodeInfo, ref addressSourceCode, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateAddressSourceCode();
					}

					AddressSourceCodeInfo.RefreshBinding();
				}
			}
		}

		ZString addressSourceCode;

		public ZPropertyInfo AddressSourceCodeInfo => GetZPropertyInfo(Constants.AddressSourceCode);

		[MaxLength(50)]
		public ZString JobNumber
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.IsBlank)
				{
					return GetEmptyPropertyValue();
				}

				return jobNumber;
			}
			set
			{
				if (SetNonPersistentPropertyValue(JobNumberInfo, ref jobNumber, value))
				{
					JobNumberInfo.RefreshBinding();
				}
			}
		}

		ZString jobNumber;

		public ZPropertyInfo JobNumberInfo => GetZPropertyInfo(Constants.JobNumber);

		protected bool JobNumber_ReadOnly => ComparisonOperator == ComparisonConstants.IsBlank;

		#region Validation

		public new AddressSourceAndJobNumberModuleFilterValidation Validation => (AddressSourceAndJobNumberModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new AddressSourceAndJobNumberModuleFilterValidation(this);
		}

		#endregion

		#region Query

		static ZQuery EmptyQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : GetQueryCore();
		}

		ZQuery GetQueryCore()
		{
			var query = new ZQuery();

			if (AddressSourceCode.IsEmpty || !AddressSourceList.ContainsCode(AddressSourceCode))
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_TableName, SQLComparisonOperator.Equal, AddressSourceType.GetAddressSourceNameFromCode(AddressSourceCode));

				if (!JobNumber.IsEmpty || ComparisonOperator == ComparisonConstants.IsBlank)
				{
					query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_NaturalKey, SqlComparisonOperator, JobNumber);
				}

				if (AddressSourceCode == WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode)
				{
					var subQuery = new ZQuery();

					subQuery.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_TableName, SQLComparisonOperator.Equal, AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode).Concat(AddressSourceType.GetAddressSourceNameFromCode(WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode)));

					if (!JobNumber.IsEmpty || ComparisonOperator == ComparisonConstants.IsBlank)
					{
						subQuery.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_AssociateNaturalKey, SqlComparisonOperator, JobNumber);
					}
					else if (JobNumber.IsEmpty)
					{
						subQuery.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_AssociateNaturalKey, SQLComparisonOperator.NotEqual, ZString.Empty);
					}

					query.AddToFilter(subQuery, JoinCondition.Or);
				}
			}

			return query;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString(Constants.AddressSourceCode, AddressSourceCode);
			writer.WriteElementString(Constants.JobNumber, JobNumber);
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == Constants.AddressSourceCode)
			{
				AddressSourceCode = reader.ReadElementString(Constants.AddressSourceCode);
			}

			if (reader.Name == Constants.JobNumber)
			{
				JobNumber = reader.ReadElementString(Constants.JobNumber);
			}
		}

		#endregion

		#region Implementation

		protected override bool IsEmptyCore => base.IsEmptyCore && AddressSourceCode.IsEmpty && JobNumber.IsEmpty;

		protected override void ClearCore()
		{
			base.ClearCore();
			AddressSourceCode = ZString.Empty;
			JobNumber = ZString.Empty;
		}

		#endregion

		static class Constants
		{
			public const string AddressSourceCode = "AddressSourceCode";
			public const string JobNumber = "JobNumber";
		}
	}
}
