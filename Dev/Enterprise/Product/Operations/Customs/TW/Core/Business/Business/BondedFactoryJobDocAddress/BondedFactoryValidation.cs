using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class BondedFactoryValidation : JobDocAddressValidation
	{
		public BondedFactoryValidation(AutoJobDocAddress parent) : base(parent)
		{
		}

		public new BondedFactory Parent => (BondedFactory)base.Parent;

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			var propertyInfo = Parent.E2_OA_AddressInfo;

			if (!OrgHeaderHelper.CheckHasVatInTW(Parent.Organisation))
			{
				propertyInfo.AddMessageError(taiwanVatIsNotExistWarning);
			}

			if (!AddressHasTaiwaneseCustomsCode(new string[] { OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.FTZ }))
			{
				var errorMessage = Parent.Declaration?.IsImport ?? false ? ValidationConstants.BondedFactory.ImpCusCodenotCBFnorEPZnorFTZ : ValidationConstants.BondedFactory.ExpCusCodenotCBFnorEPZnorFTZ;
				propertyInfo.AddMessageError(errorMessage);
			}

			CheckCanNotCoExistWithSupplierAndImporterBondedId(propertyInfo);
		}

		void CheckCanNotCoExistWithSupplierAndImporterBondedId(ZPropertyInfo propertyInfo)
		{
			var jobDeclaration = Parent.Declaration;
			if (jobDeclaration != null && !propertyInfo.Value.IsEmpty)
			{
				var declarationType = jobDeclaration.CusEntryInstruction.CEI_Style;
				if (!jobDeclaration.SupplierDocumentaryAddress.CBPCode.IsEmpty && SupplierAddressRequirement.SupplierBondedIdCanNotCoExistOrEmptyWithToBondedWarehouseList.Contains(declarationType))
				{
					propertyInfo.AddMessageError(ValidationConstants.TWJobDocAddress.SupplierBondedIdCanNotCoExistOrEmptyWithBondedFactories);
				}
				else if (!jobDeclaration.ImporterDocumentaryAddress.CBPCode.IsEmpty && declarationType == Constants.DeclarationTypes.Import.G7)
				{
					propertyInfo.AddMessageError(ValidationConstants.TWJobDocAddress.ImporterBondedIdCanNotCoExistWithBondedFactories);
				}
			}
		}

		bool AddressHasTaiwaneseCustomsCode(string[] codesToLookFor)
		{
			return !Parent.Address.GetCustomsRegNo(codesToLookFor).IsEmpty;
		}

		protected override void CheckE2_AddressSequence()
		{
			base.CheckE2_AddressSequence();
			var bondedFactory = Parent;
			var maxNumberOfEntries = bondedFactory?.Declaration?.IsImport ?? false ? BondedFactoryCollection.GetMaximumNumberOfImportBondedFactories()
							: BondedFactoryCollection.GetMaximumNumberOfExportBondedFactories();

			if (Parent.E2_AddressSequence >= maxNumberOfEntries)
			{
				Parent.AddRowMessageError(string.Format(CultureInfo.CurrentCulture, tooManyRecordsWarning, bondedFactory.Declaration.JE_MessageType, maxNumberOfEntries));
			}
		}

		readonly MultilingualString taiwanVatIsNotExistWarning = ResString.GetMultilingualString("5D6525A9-A18C-4EF7-A92D-E186FC3B22AC", "The selected Organization does not have a Government VAT Code.");
		readonly MultilingualString tooManyRecordsWarning = ResString.GetMultilingualString("EDEBF22D-22DD-41D7-B9CE-DC936B8C0873", "There are too many records for this Shipment Type ({0}), you can have a maximum of {1}");
	}
}
