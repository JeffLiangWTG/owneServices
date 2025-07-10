using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class TradersRemark : CusSupportingInfo
	{
		public TradersRemark(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("AEC33AD4-5F74-42E3-AC03-2A9F7239899F", "Traders Remark");

		public override bool SupportsNotes => false;

		[BusinessObjectTestExclude]
		[ResourceStringData("036307D4-F102-48B3-B5FA-82D1FCEA144E", Caption = "Traders Remark")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set
			{
				var newValue = value.Trim();
				if (newValue.IsEmpty)
				{
					Delete();
				}
				else
				{
					base.CSI_Description = newValue;
				}
			}
		}

		public new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new TradersRemarkValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.TradersRemarks;
			CSI_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
		}
	}
}
