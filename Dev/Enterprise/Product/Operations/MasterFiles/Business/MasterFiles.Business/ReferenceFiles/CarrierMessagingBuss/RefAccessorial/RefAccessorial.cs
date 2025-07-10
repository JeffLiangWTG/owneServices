using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefAccessorialSchema.Constants.ASI_Code), DescriptionProperty(RefAccessorialSchema.Constants.ASI_Description)]
	public class RefAccessorial : AutoRefAccessorial
	{
		public RefAccessorial(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("RefAccessorial|ASI_Code", Caption = "Accessorial Code")]
		public override ZString ASI_Code => base.ASI_Code;

		[ResourceStringData("RefAccessorial|ASI_Description", Caption = "Accessorial Description")]
		public override ZString ASI_Description => base.ASI_Description;

		protected override ZString HumanReadableNameCore => Res.GetString("95817189-b58b-42bd-80d4-1038d9334d84", "Accessorial - {0}", CalculateShortcutName());
	}
}
