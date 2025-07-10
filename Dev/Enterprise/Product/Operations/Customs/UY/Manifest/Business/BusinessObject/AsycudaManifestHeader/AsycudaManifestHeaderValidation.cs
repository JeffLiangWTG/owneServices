using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected override void CheckAMA_DateAtCustomsOffice()
		{
			base.CheckAMA_DateAtCustomsOffice();

			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.AMA_DateAtCustomsOfficeInfo);
		}

		protected override void CheckAMA_OA_Carrier()
		{
			base.CheckAMA_OA_Carrier();

			if (!Parent.AMA_OA_Carrier.IsEmpty)
			{
				var carrierRUT = Parent.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == UruguayOrgCusCodeInfo.OrgCusCodes.RUT)?.OK_CustomsRegNo ?? ZString.Empty;

				if (carrierRUT == ZString.Empty)
				{
					Parent.AMA_OA_CarrierInfo.AddMessageError(ResString.GetMultilingualString("A47818AB-60CD-4F04-9487-3D8FEB3AE994", "The selected Carrier should have RUT"));
				}
			}
		}

		protected override void CheckAMA_ManifestType()
		{
			base.CheckAMA_ManifestType();

			if (GlbCompany.CurrentCompany.GC_BusinessRegNo.IsEmpty)
			{
				Parent.AMA_ManifestTypeInfo.AddMessageError(ResString.GetMultilingualString("4806FC19-38BE-4B2F-8B18-3255E7B5D592", "The current company should have Registration Number Entered"));
			}
		}
	}
}
