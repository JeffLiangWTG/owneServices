using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefPackTypeSchema.Constants.F3_Code), DescriptionProperty(RefPackTypeSchema.Constants.F3_Description)]
	public class RefPackType : AutoRefPackType, IDocManagerSupport, ICanDelete, IZUnit
	{
		#region Ctor

		public RefPackType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			F3_IsSystem = false;
		}

		#region Properties

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[List("Lookups.LengthUnitList")]
		public override ZString F3_UnitOfDimension
		{
			get { return base.F3_UnitOfDimension; }
			set { base.F3_UnitOfDimension = value; }
		}

		[List("Lookups.WeightUnitList")]
		public override ZString F3_UnitOfWeight
		{
			get { return base.F3_UnitOfWeight; }
			set { base.F3_UnitOfWeight = value; }
		}

		[TranslatableDataField(Schema.TableName, Schema.F3_Description, DataXmlFilePaths.RefPackType, MaxLength = Schema.F3_DescriptionMaxLength, Type = typeof(RefPackType), SecurityCheckpoint = "RefPackTypeModify", Asmid = ResString.AssemblyId)]
		public override ZString F3_Description
		{
			get { return base.F3_Description; }
			set { base.F3_Description = value; }
		}

		public MultilingualString F3_DescriptionMultilingual
		{
			get { return GetMultilingual(F3_DescriptionInfo); }
		}

		[List("Lookups.UOMPackTypeList")]
		public override ZString F3_UOMType
		{
			get { return base.F3_UOMType; }
			set { base.F3_UOMType = value; }
		}

		public override bool ReadOnly
		{
			get { return IsReservedType; }
		}

		public bool IsReservedType
		{
			get { return F3_Code == RefPackTypeCollection.ReservedContainerType; }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("9CA66EC6-F285-4B4F-9BF0-E30413293927", "Package Type - {0}", CalculateShortcutName());

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.PackType);
				}
				return docManagerInfo;
			}
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return F3_Code != "CNT"; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("0de3d627-0d09-426f-8648-66895c85cff0", "Cannot delete this reserved Package Type."); }
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			UpdateEDICodeMapping();
		}

		void UpdateEDICodeMapping()
		{
			if ((ZString)F3_CodeInfo.OriginalValue != F3_Code)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.PackageType);
				query.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalCode, F3_CodeInfo.OriginalValue);
				OrgPatternMatchOverride[] matchOverrides = Factory.Load<OrgPatternMatchOverride>(query);
				foreach (OrgPatternMatchOverride matchOverride in matchOverrides)
				{
					matchOverride.OO_LocalCode = F3_Code;
				}
			}
		}

		#endregion

		#region ZUnit

		ZString IZUnit.Code => F3_Code;
		ZUnitType IZUnit.Type => ZUnitType.RefPackType;

		public static implicit operator ZUnit(RefPackType packType) => packType == null ? default(ZUnit) : new ZUnit(packType);
		public ZUnit ToZUnit() => this;

		#endregion
	}
}
