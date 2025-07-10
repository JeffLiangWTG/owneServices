using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.SFT_FeatureName)]
	[DescriptionProperty(nameof(GroupDescription))]
	public class StmFeatureTest : AutoStmFeatureTest
	{
		public const string WinzorFeatureCode = "WINZOR";
		public const string WinzorAllFeaturesCode = "WINZORALL";
		public const string WinzorMainFormFeatureCode = "WINZORMAINFORM";
		public const string WebVersion = "WEBVERSION";
		public const string ForceCW1HomeScreen = "ForceCW1HomeScreen";
		public const string NetCoreVersion = "NETCOREVERSION";

		public StmFeatureTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("44d024a2-9e49-4f5a-87a8-10481d676c97", "Feature Test - {0}", CalculateShortcutName());

		protected override ZString HumanReadableShortcutNameCore => HumanReadableNameCore;

		public ZString GroupDescription => Group?.GG_Desc ?? string.Empty;

		[List("Lookups.Features")]
		public override ZString SFT_FeatureName { get => base.SFT_FeatureName; set => base.SFT_FeatureName = value; }
	}
}
