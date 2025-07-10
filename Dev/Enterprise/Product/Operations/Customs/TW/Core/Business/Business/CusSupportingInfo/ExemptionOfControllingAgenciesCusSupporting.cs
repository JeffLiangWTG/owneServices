using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class ExemptionOfControllingAgenciesCusSupporting : CusSupportingInfo
	{
		public ExemptionOfControllingAgenciesCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.PermitExemptionCodes;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new ExemptionOfControllingAgenciesCusSupportingValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new ExemptionOfControllingAgenciesCusSupportingLookups(this);

		public new ExemptionOfControllingAgenciesCusSupportingLookups Lookups => (ExemptionOfControllingAgenciesCusSupportingLookups)base.Lookups;

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		[List(nameof(Lookups) + "." + nameof(ExemptionOfControllingAgenciesCusSupportingLookups.SpecialCodeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.ExemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumber", Caption = "Code", FullDescription = "The special code for exemption provided by the controlling agency. Entered in the Permit Number field.")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = CSI_ReferenceNumber;
				base.CSI_ReferenceNumber = value;
				var parent = Parent;
				if (parent != null && value != oldValue && !IsCopying && !((ISupportDataImporting)parent).IsImportingData)
				{
					var infoArray = new ZPropertyInfo[] { parent.PermitExemptionCode1Info, parent.PermitExemptionCode2Info, parent.PermitExemptionCode3Info, parent.PermitExemptionCode4Info, parent.PermitExemptionCode5Info };
					var matchIndex = 0;
					foreach (var permitExemption in parent.ExemptionOfControllingAgenciesCusSupportings)
					{
						if (permitExemption.PK == PK && matchIndex < 5)
						{
							infoArray[matchIndex].RefreshBinding(oldValue);
							break;
						}
						matchIndex++;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.ExemptionOfControllingAgenciesCusSupporting.SpecialCodeDescription", Caption = "Description")]
		public ZString SpecialCodeDescription
		{
			get
			{
				var result = ZString.Empty;
				if (!CSI_ReferenceNumber.IsEmpty)
				{
					var list = (TWSpecialCodeCollection)Lookups.SpecialCodeList;
					list.Load(new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, CSI_ReferenceNumber));
					result = list.Cast<TWSpecialCode>().FirstOrDefault()?.SC_Description ?? ZString.Empty;
				}
				return result;
			}
		}

		public ZPropertyInfo SpecialCodeDescriptionInfo => GetZPropertyInfo(nameof(SpecialCodeDescription));

		public static IBusinessObjectCollection GetSpecialCodeList(BusinessObjectFactory factory) => factory.GetCachedValue("Enterprise.Customs.TW.Business.ExemptionOfControllingAgenciesCusSupporting.SpecialCodeList", () =>
		{
			return new TWSpecialCodeCollection(factory);
		});
	}
}
