using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ZA.Business
{
	[CodeProperty(CusCodeData.Schema.CY_Data)]
	[DescriptionProperty(CusCodeData.Schema.Description)]
	public class CaseNumber : CusCodeData
	{
		public CaseNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				return new TypeLoaderCollection(typeof(CusEntryInstruction),
					ObjectFactory.GetType<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaBill>(),
					ObjectFactory.GetType<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>());
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CaseNumber;
			CY_Code = CaseNumberTypeList.Codes.DocumentInspectionCases;
		}

		protected override CusCodeDataLookups GetNewLookups() => new CaseNumberLookups(this);

		public new CaseNumberLookups Lookups => (CaseNumberLookups)base.Lookups;

		public ICaseNumberCollectionProvider ParentAsCollectionProvider => (ICaseNumberCollectionProvider)Parent;

		protected override CusCodeDataValidation GetNewValidation() => new CaseNumberValidation(this);

		public bool Description_ReadOnly => true;

		public override ZString Description => CY_Code.IsEmpty ? "" : Lookups.DocumentStatusCodeList.GetDescriptionFromCode(Document_Status);

		[CargoWise.ComponentModel.MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		[System.ComponentModel.ReadOnly(true)]
		public ZString Document_Status
		{
			get
			{
				if (genAddOnForDocument_Status == null || genAddOnForDocument_Status.IsDeleted)
				{
					Customs.Business.GenAddOnHelper.FindOrMakeNewAddOn("ZA_DocumentStatus", this, out genAddOnForDocument_Status);
				}
				return genAddOnForDocument_Status.XA_Data;
			}
			set
			{
				if (value.IsEmpty && genAddOnForDocument_Status != null)
				{
					genAddOnForDocument_Status.Delete();
				}
				else
				{
					if (genAddOnForDocument_Status == null || genAddOnForDocument_Status.IsDeleted)
					{
						Customs.Business.GenAddOnHelper.FindOrMakeNewAddOn("ZA_DocumentStatus", this, out genAddOnForDocument_Status);
					}
					genAddOnForDocument_Status.XA_Data = value;
				}
			}
		}
		GenAddOnColumn genAddOnForDocument_Status;

		public ZPropertyInfo Document_StatusInfo
		{
			get
			{
				if (genAddOnForDocument_Status == null)
				{
					Customs.Business.GenAddOnHelper.FindOrMakeNewAddOn("ZA_DocumentStatus", this, out genAddOnForDocument_Status);
				}
				return GetWrappedZPropertyInfo(nameof(Document_Status), x => genAddOnForDocument_Status.XA_DataInfo);
			}
		}
	}
}
