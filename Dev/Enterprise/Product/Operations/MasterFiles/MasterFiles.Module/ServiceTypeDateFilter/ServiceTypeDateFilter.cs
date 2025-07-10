using System;
using System.Windows.Forms;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ServiceTypeDateFilter : ModuleDateFilter
	{
		public ServiceTypeDateFilter(FilterStripBusinessObject filterBusinessObject, Type job, bool isDateBooked) : base(GetDescription(isDateBooked), delegate
		{ return new ZQuery(); })
		{
			this.job = job;
			this.isDateBooked = isDateBooked;
			this.filterBusinessObject = filterBusinessObject;
			MultilingualDescription = GetMultilingualDescription();
		}

		readonly Type job;
		readonly bool isDateBooked;
		readonly FilterStripBusinessObject filterBusinessObject;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public const string ServiceTypeDateBooked = "Service Type / Date Booked";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public const string ServiceTypeDateCompleted = "Service Type / Date Completed";

		#region  Description

		ResourceString GetMultilingualDescription()
		{
			return isDateBooked
				? ResString.GetMultilingualString("A12C1E56-7900-4861-B3FC-216F74DFA57D", ServiceTypeDateBooked)
				: ResString.GetMultilingualString("D556DDEA-551B-44CE-9573-54AA890102AE", ServiceTypeDateCompleted);
		}

		static ZString GetDescription(bool isBooked)
		{
			return new ZString(isBooked ? ServiceTypeDateBooked : ServiceTypeDateCompleted);
		}

		#endregion

		#region Get Controls

		public static Control GetServiceTypeDateFilterControl(ZFilterStrip filterStripControl, ZBindingSource bindingSource)
		{
			var control = new ServiceTypeDateFilterControl(filterStripControl);
			ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
			bindingSource.SetBindingMember(control, ".");

			return control;
		}

		#endregion

		#region Properties

		#region DateTimeColumn

		SchemaDateTimeColumn DateTimeColumn => isDateBooked ? JobServiceSchema.ES_Booked : JobServiceSchema.ES_Completed;

		#endregion

		#region JobService

		[MaxLength(3)]
		[BusinessObjectTestExclude]
		public ZString JobServiceType
		{
			get { return jobServiceType; }
			set
			{
				if (jobServiceType != value)
				{
					jobServiceType = value;
					if (!IsValidationSuspended)
					{
						CheckMaximumLength(JobServiceTypeInfo, value);
						Validation.ValidateJobServiceType();
					}
					JobServiceTypeInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo JobServiceTypeInfo
		{
			get { return GetZPropertyInfo(nameof(JobServiceType)); }
		}

		ZString jobServiceType;

		#endregion

		#region JobServiceType_List
		public CodeDescriptionPairList JobServiceType_List
		{
			get
			{
				if (jobServiceType_List == null)
				{
					var jobService = filterBusinessObject.Factory.GetNull<JobService>(); // use null factory to crate the bizo in order to ensure we are using same List
					jobServiceType_List = jobService.Lookups.JobServiceType_List;
				}

				return jobServiceType_List;
			}
		}
		CodeDescriptionPairList jobServiceType_List;

		#endregion

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.ModesAndTypes; }
		}

		#endregion

		#region Validation

		public new ServiceTypeDateFilterValidation Validation
		{
			get { return (ServiceTypeDateFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ServiceTypeDateFilterValidation(this);
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("JobServiceType", JobServiceType);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "JobServiceType")
			{
				JobServiceType = reader.ReadElementString("JobServiceType");
			}
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			JobServiceType = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && jobServiceType.IsEmpty;

		#endregion

		#region GetQuery

		protected override ZQuery GetQuery()
		{
			var result = new ZDBOnlyQuery(job);
			var comparisonOperator = GetComparisonOperator();

			var jobServiceSubQuery = new ZDBOnlySubQuery(typeof(JobService), JobServiceSchema.ES_ParentID);
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				jobServiceSubQuery.AddToFilter(JoinCondition.And, DateTimeColumn, ZDateTime.Empty);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				jobServiceSubQuery.AddToFilter(JoinCondition.And, DateTimeColumn, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			}
			else
			{
				filterBusinessObject.AddDateTimeRange(jobServiceSubQuery, comparisonOperator, JoinCondition.And, DateTimeColumn, FromDate, ToDate);
			}

			if (!jobServiceType.IsEmpty)
			{
				jobServiceSubQuery.AddToFilter(JobServiceSchema.ES_ServiceCode, jobServiceType);
			}

			if (!jobServiceSubQuery.IsEmpty)
			{
				result.AddSubQuery(jobServiceSubQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion
	}
}
