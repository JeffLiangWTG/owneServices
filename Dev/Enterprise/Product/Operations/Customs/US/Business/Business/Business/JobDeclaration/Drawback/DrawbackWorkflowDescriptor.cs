using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.DrawBackWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("FDFA5BA8-D112-433E-8A25-9B3F50564B8E", "Drawback declaration job"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(JobDeclaration); }
		}

		public override bool SupportsBufferManagement => false;

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.US.Drawback; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool RequiresPort1
		{
			get { return false; }
		}

		public override bool RequiresPort2
		{
			get { return false; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				USAddInfoSchema.US_EntryType,
				USAddInfoSchema.US_DRWRejectedMerchandiseReason,
				USAddInfoSchema.US_DRWDatePeriodFrom,
				USAddInfoSchema.US_DRWDatePeriodTo,
				USAddInfoSchema.US_EstimatedEntryDate,
				USAddInfoSchema.US_DRWFilingMethod,
				USAddInfoSchema.US_DRWPurpose,
				USAddInfoSchema.US_BondType,
				USAddInfoSchema.US_SuretyCode
			};
		}

		public override string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			var fieldName = fieldColumn.Name;
			var fieldColumnDescription = string.Empty;
			switch (fieldName)
			{
				case USAddInfoSchema.Constants.US_EntryType:
					fieldColumnDescription = Res.GetString("142270E8-7F54-40FD-B33A-928898DB3CEA", "Claim Type");
					break;
				case USAddInfoSchema.Constants.US_DRWRejectedMerchandiseReason:
					fieldColumnDescription = Res.GetString("C7139162-84ED-465D-849D-0D8A8C3C96D1", "Rejected Merchandise Reason");
					break;
				case USAddInfoSchema.Constants.US_DRWDatePeriodFrom:
					fieldColumnDescription = Res.GetString("50622FD9-A8FD-4002-B1F7-E6338DFEB5A2", "Drawback Period Covered From");
					break;
				case USAddInfoSchema.Constants.US_DRWDatePeriodTo:
					fieldColumnDescription = Res.GetString("C21244BB-380F-46C5-9807-D8FE0447EC24", "Drawback Period Covered To");
					break;
				case USAddInfoSchema.Constants.US_EstimatedEntryDate:
					fieldColumnDescription = Res.GetString("6C3476DA-249D-4E66-84FB-F030937E8BE5", "Estimated Claim Date");
					break;
				case USAddInfoSchema.Constants.US_DRWFilingMethod:
					fieldColumnDescription = Res.GetString("F78704EF-7DD4-465F-81D7-93942B31342F", "Method of Filing");
					break;
				case USAddInfoSchema.Constants.US_DRWPurpose:
					fieldColumnDescription = Res.GetString("138B95A4-C7F6-44BC-A3FE-3101F60FE444", "Purpose");
					break;
				case USAddInfoSchema.Constants.US_BondType:
					fieldColumnDescription = Res.GetString("4A3FE131-34F0-4750-B8F6-4B6679CD66E6", "Bond Type");
					break;
				case USAddInfoSchema.Constants.US_SuretyCode:
					fieldColumnDescription = Res.GetString("C8D04A79-4083-4953-A4C7-56AC571AAF42", "Surety Code");
					break;
				default:
					fieldColumnDescription = base.GetFieldColumnDescription(factory, fieldColumn);
					break;
			}
			return fieldColumnDescription;
		}
	}
}
