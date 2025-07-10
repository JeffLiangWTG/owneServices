using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master)
			: base(master)
		{ }

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			using (child.SuspendSettingHasChanges())
			{
				if (Master != null)
				{
					var bill = (AsycudaBill)child;
					if (Master.NeedPreviousDeclarationNumbers)
					{
						bill.CustomsEntryNumberType = CusEntryNumberTypes.Turkey.PRV;
					}

					if (Master.IsSea)
					{
						var result = ZString.Empty;

						switch (Master.AMA_ManifestType)
						{
							case TRManifestTypes.Codes.DENITH:
								result = ABLLocationInformationDefaultTextGemi;
								break;
							case TRManifestTypes.Codes.DENIHR:
							case TRManifestTypes.Codes.CIKONC:
								result = ABLLocationInformationDefaultTextLiman;
								break;
							default:
								break;
						}

						bill.ABL_LocationInformation = result;
					}

					bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;

					bill.ABL_SpecialCargoCode = Universal.CodeDescriptionPairLists.YesNoList.Codes.No;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Localised strings")]
		const string ABLLocationInformationDefaultTextLiman = "LİMAN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Localised strings")]
		const string ABLLocationInformationDefaultTextGemi = "GEMİ";
	}
}
