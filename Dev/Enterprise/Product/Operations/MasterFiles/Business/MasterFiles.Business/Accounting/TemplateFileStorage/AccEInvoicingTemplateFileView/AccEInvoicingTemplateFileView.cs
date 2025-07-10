using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used in GUI, included in another work item.")]
	public class AccEInvoicingTemplateFileView : AutoAccEInvoicingTemplateFileView, IJobConfiguration
	{
		public AccEInvoicingTemplateFileView(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.JobTypesList")]
		public override ZString ETF_JobType { get => base.ETF_JobType; set => base.ETF_JobType = value; }

		[List("Lookups.DirectionsList")]
		public override ZString ETF_ServiceDirection { get => base.ETF_ServiceDirection; set => base.ETF_ServiceDirection = value; }

		[List("Lookups.TransportModesList")]
		public override ZString ETF_TransportMode { get => base.ETF_TransportMode; set => base.ETF_TransportMode = value; }

		[List("Lookups.TemplateCodesList")]
		public override ZString ETF_TemplateCode { get => base.ETF_TemplateCode; set => base.ETF_TemplateCode = value; }

		public AccEInvoicingTemplateFileLevelEnum Level => ToTESLevel(ETF_ParentTableCode);

		public ZString ETF_TemplateConfigLevel => Level.ToString();

		public ZPropertyInfo ETF_TemplateConfigLevelInfo
		{
			get { return GetZPropertyInfo(nameof(ETF_TemplateConfigLevel)); }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("ce2c7cb8-9da0-4d6c-9c8a-c7e954de2bf1", "XSLT File Configuration");

		public bool IsDuplicateOf(AccEInvoicingTemplateFileView config)
		{
			return ETF_GC == config.ETF_GC
				&& ETF_ParentTableCode == config.ETF_ParentTableCode
				&& ETF_ParentID == config.ETF_ParentID
				&& ETF_JobType == config.ETF_JobType
				&& ETF_TransportMode == config.ETF_TransportMode;
		}

		public bool IsDefault => ETF_JobType == "ALL" && ETF_TransportMode == "ALL";

		public ZString LevelName
		{
			get
			{
				var captions = new[]
				{
					Res.GetString("c3019e85-fc90-4857-99d8-92b5f2b15d01", "Company"),
					Res.GetString("29ee4c19-7b03-4342-882c-e489db6cb4c3", "Branch"),
					Res.GetString("be028ecf-b8dc-4ff3-8b05-910cb4b35f19", "Organization")
				};

				return captions[(int)Level];
			}
		}

		public override bool ReadOnly
		{
			get
			{
				var parentCollection = (AccEInvoicingTemplateFileViewCollection)((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(pc => pc is AccEInvoicingTemplateFileViewCollection);

				if (parentCollection == null)
				{
					return base.ReadOnly;
				}

				if (parentCollection.ReadOnly)
				{
					return true;
				}

				return parentCollection.Level != Level;
			}
			set => base.ReadOnly = value;
		}

		public ZPropertyInfo LevelNamePropertyInfo => GetZPropertyInfo(nameof(LevelName));

		public ZString JobTypeDescription => Lookups.JobTypesList.GetDescriptionFromCode(ETF_JobType);

		public ZString TemplateFileName => TemplateFile?.TFS_FileName ?? ZString.Empty;

		public ZString TemplateFileDesc => TemplateFile?.TFS_Description ?? ZString.Empty;

		AccTemplateFileStorage TemplateFile
		{
			get
			{
				var filter = new ZQuery(AccTemplateFileStorageSchema.TFS_GC, ETF_GC);
				filter.AddToFilter(AccTemplateFileStorageSchema.TFS_Ledger, LedgerTypes.AccountsReceivable);
				filter.AddToFilter(AccTemplateFileStorageSchema.TFS_Code, ETF_TemplateCode);
				return Factory.LoadTop1<AccTemplateFileStorage>(filter);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DataRow row = ((IBusinessObjectInternals)this).Row;

			row[AccEInvoicingTemplateFileViewSchema.Constants.ETF_ConfigType] = JobConfiguration.TypeCodes.EInvoicingTemplateFile;
			row[AccEInvoicingTemplateFileViewSchema.Constants.ETF_GC] = Env.CurrentCompanyPK;
			row[AccEInvoicingTemplateFileViewSchema.Constants.ETF_Ledger] = "AR";
			row[AccEInvoicingTemplateFileViewSchema.Constants.ETF_ParentTableCode] = "";
			row[AccEInvoicingTemplateFileViewSchema.Constants.ETF_ServiceDirection] = "ALL";
			row[AccEInvoicingTemplateFileViewSchema.Constants.ETF_JobType] = "ALL";
			row[AccEInvoicingTemplateFileViewSchema.Constants.ETF_TransportMode] = "ALL";
			row[AccEInvoicingTemplateFileViewSchema.Constants.ETF_TemplateCode] = "";
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		public static string ToTablePrefix(AccEInvoicingTemplateFileLevelEnum level)
		{
			if (level == AccEInvoicingTemplateFileLevelEnum.Branch)
			{
				return GlbBranchSchema.Constants.Prefix;
			}
			if (level == AccEInvoicingTemplateFileLevelEnum.Organisation)
			{
				return OrgHeaderSchema.Constants.Prefix;
			}
			if (level == AccEInvoicingTemplateFileLevelEnum.Company)
			{
				return string.Empty;
			}

			throw new InvalidOperationException($"Unknown level value {level}");
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		public static AccEInvoicingTemplateFileLevelEnum ToTESLevel(ZString tablePrefix)
		{
			if (tablePrefix == GlbBranchSchema.Constants.Prefix)
			{
				return AccEInvoicingTemplateFileLevelEnum.Branch;
			}
			if (tablePrefix == OrgHeaderSchema.Constants.Prefix)
			{
				return AccEInvoicingTemplateFileLevelEnum.Organisation;
			}
			if (string.IsNullOrEmpty(tablePrefix))
			{
				return AccEInvoicingTemplateFileLevelEnum.Company;
			}

			throw new InvalidOperationException($"Unknown table prefix value {tablePrefix}");
		}

		ZString IJobConfiguration.JobType => ETF_JobType;
		ZString IJobConfiguration.ServiceDirection => ETF_ServiceDirection;
		ZString IJobConfiguration.TransportMode => ETF_TransportMode;
		bool IJobConfiguration.IncludeOptionsForAllJobTypes => true;
	}
}
