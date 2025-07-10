using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.RQT_Code), DescriptionProperty(Schema.RQT_Description)]
	public class ExternalRequestType : AutoExternalRequestType, IJobNumber
	{
		public ExternalRequestType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public string JobNumber => RQT_Code;

		[List("Lookups.JobTypeList")]
		public override ZString RQT_JobType { get => base.RQT_JobType; set => base.RQT_JobType = value; }

		protected bool RQT_Code_ReadOnly => IsInDatabase;

		protected bool RQT_JobType_ReadOnly => IsInDatabase && HasRequests;

		protected bool RQT_IsSystem_ReadOnly => true;

		protected override ZString HumanReadableNameCore => Res.GetString("d3c36ab5-8445-48b4-af16-17f0a074d49c", "Request Type {0}", this.RQT_Code);

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[ResourceStringData("ExternalRequestType|RQT_RIT_Template", Caption = "Form Type")]
		[List("Lookups.FormTypeList")]
		public override ZGuid RQT_RIT_Template { get => base.RQT_RIT_Template; set => base.RQT_RIT_Template = value; }

		protected bool HasRequests => Factory.LoadTop1<ExternalRequest>(new ZQuery(ExternalRequestSchema.REQ_RQT_Type,
			SQLComparisonOperator.Equal, base.PK)) != null;

		[List("Lookups.AssigneeAddressTypes")]
		public override ZString RQT_Assignee { get => base.RQT_Assignee; set => base.RQT_Assignee = value; }

		[List("Lookups.ReviewerAddressTypes")]
		public override ZString RQT_Reviewer { get => base.RQT_Reviewer; set => base.RQT_Reviewer = value; }
	}
}
