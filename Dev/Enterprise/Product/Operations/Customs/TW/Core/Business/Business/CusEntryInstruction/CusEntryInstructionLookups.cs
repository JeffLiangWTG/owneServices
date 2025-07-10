using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction parent)
			: base(parent)
		{
		}

		public CusEntryInstruction EntryInstruction => Parent;

		public new BaseJobDeclaration JobDeclaration => EntryInstruction?.JobDeclaration;

		public OrganisationsFindBoxCollection OrganisationsFindBoxCollection => new WarehouseOrganisationsFindBoxCollection(Parent.Factory);

		protected new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		public override CodeDescriptionPairList StyleList
		{
			get
			{
				var jobDeclaration = JobDeclaration;
				if (jobDeclaration != null)
				{
					var cacheKey = string.Concat("TW.CusEntryInstructionLookups.StyleList_", jobDeclaration.JE_MessageType);
					var effectiveDate = Parent.DateOfValuation;

					return Factory.GetCachedValue(cacheKey, () =>
					{
						var styleList = new CodeDescriptionPairList();
						if (jobDeclaration.IsImport)
						{
							styleList.AddPairsIfNotExist(TWRefCusCodeListLoader.GetImportDeclarationType(Factory, effectiveDate));
						}
						else if (jobDeclaration.IsExport)
						{
							styleList.AddPairsIfNotExist(TWRefCusCodeListLoader.GetExportDeclarationType(Factory, effectiveDate));
						}
						styleList.Sort();
						return styleList;
					});
				}
				return Factory.GetCachedValue<CodeDescriptionPairList>();
			}
		}

		#region ExaminationZoneList

		public CodeDescriptionPairList ExaminationZoneList
		{
			get
			{
				var officeAreaCode = Parent.CEI_CustomsOffice;
				var entryDate = Parent.EntryHeader?.DeclarationDate ?? ZDateTime.Empty;
				var effectiveDate = !entryDate.IsEmpty ? entryDate : ZDate.Today;
				var cacheKey = string.Concat("TW.CusEntryInstructionLookups.ExaminationZoneList_", officeAreaCode, "_", effectiveDate.ToISO8601ShortDateString());

				return Factory.GetCachedValue(cacheKey, () =>
				{
					var examList = new CodeDescriptionPairList();
					examList.AddPairsIfNotExist(TWRefCusCodeListLoader.GetICIPlacesByCustomsOffice(Factory, officeAreaCode, effectiveDate));
					examList.Sort();
					return examList;
				});
			}
		}

		#endregion

		public CodeDescriptionPairList BondedWarehouseTypeList => Factory.GetCachedValue<BondedWarehouseTypeList>();

		public CodeDescriptionPairList ReasonforDutyList => Factory.GetCachedValue<ReasonforDutyList>();

		public CodeDescriptionPairList ExamModeList => Factory.GetCachedValue<ExamModeList>();

		public CodeDescriptionPairList CustomsOfficeList => TWRefCusCodeListTypes.GetCustomsOfficeList(Factory);

		public IBusinessObjectCollection GoodsLocationCollection => TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, Parent.CEI_CustomsOffice);

		public CodeDescriptionPairList BoxNumberList => RegistryHelper.GetBoxNumberList(Factory);

		public CodeDescriptionPairList RORPaymentMethodList => Factory.GetCachedValue<RORPaymentMethodList>();
	}
}
