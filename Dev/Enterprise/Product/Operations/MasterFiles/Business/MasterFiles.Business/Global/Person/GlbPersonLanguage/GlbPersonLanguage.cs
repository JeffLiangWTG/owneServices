using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonLanguage : AutoGlbPersonLanguage
	{
		public GlbPersonLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region G7_Language
		[List("Lookups.Languages")]
		public override ZString G7_Language
		{
			get
			{
				return base.G7_Language;
			}
			set
			{
				base.G7_Language = value;
			}
		}
		#endregion

		#region G7_SkillLevel
		[List("Lookups.SkillLevels")]
		public override ZString G7_SkillLevel
		{
			get
			{
				return base.G7_SkillLevel;
			}
			set
			{
				base.G7_SkillLevel = value;
			}
		}
		#endregion

		#region Language Description

		public ZString LanguageDescription
		{
			get { return Lookups.Languages.GetDescriptionFromCode(G7_Language); }
		}

		public ZPropertyInfo LanguageDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(LanguageDescription)); }
		}

		#endregion

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DataRow row = ((IBusinessObjectInternals)this).Row;

			row[GlbPersonLanguageSchema.Constants.G7_Language] = "";
			row[GlbPersonLanguageSchema.Constants.G7_SkillLevel] = "";
		}

		#endregion
	}
}
