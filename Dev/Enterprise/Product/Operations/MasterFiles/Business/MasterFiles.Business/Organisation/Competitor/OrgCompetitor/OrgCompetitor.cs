using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompetitor : AutoOrgCompetitor
	{
		public OrgCompetitor(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		new class Schema : AutoOrgCompetitor.Schema
		{
			public const string CompanyLevel = "CompanyLevel";
		}

		protected override ZString HumanReadableNameCore => Res.GetString("23eb2537-7436-40b1-a573-f2168529caeb", "Competitor");

		#region Properties

		#region Description

		[ResourceStringData("A28F2311-BF8F-402D-881F-8A70BDF64065", Caption = "Description")]
		public ZString Description
		{
			get
			{
				return OrganisationsDataRegistry.Instance.CompetitorType.Value.GetDescriptionFromCode(OCP_Type);
			}
		}

		#endregion

		#region Competitor Type Code

		[ResourceStringData("3A68C6EA-E565-42FB-B9F4-98CD0AB2B072", Caption = "Competitor Type Code")]
		[List("Lookups.ActiveCompetitorTypes")]
		public override ZString OCP_Type
		{
			get
			{
				return base.OCP_Type;
			}
			set
			{
				base.OCP_Type = value;
				Validation.ValidateOCP_OH_Competitor();
			}
		}

		#endregion

		#region Organisation Code

		[ResourceStringData("7092339E-5F47-4AFA-AC5E-3499D6B93605", Caption = "Organization Code")]
		[List("Lookups.Organisations")]
		public override ZGuid OCP_OH_Competitor
		{
			get
			{
				return base.OCP_OH_Competitor;
			}
			set
			{
				base.OCP_OH_Competitor = value;
				Validation.ValidateOCP_Type();
			}
		}

		#endregion

		#region Organisation Name

		[ResourceStringData("ACEB2CBE-B724-40EB-921C-FDD056A9D825", Caption = "Organization Name")]
		public ZString OrganisationName
		{
			get
			{
				return Competitor?.OH_FullName ?? ZString.Empty;
			}
		}

		#endregion

		#region CompanyLevel

		public override ZGuid OCP_GC_Company
		{
			get { return base.OCP_GC_Company; }
			set
			{
				base.OCP_GC_Company = value;

				MarkAsNeedingValidation();
				CompanyLevelInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude()]
		[List("Lookups.CompanyLevelList")]
		[MaxLength(3)]
		public ZString CompanyLevel
		{
			get { return OCP_GC_Company == ZGuid.Empty ? CompanyLevelList.Codes.ENT : CompanyLevelList.Codes.COM; }
			set { OCP_GC_Company = (value == CompanyLevelList.Codes.COM) ? GlbCompany.CurrentCompany.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo CompanyLevelInfo
		{
			get { return GetZPropertyInfo(Schema.CompanyLevel); }
		}

		#endregion

		#endregion
	}
}
