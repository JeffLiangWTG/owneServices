using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105ImporterWrapper : NX5105PartyDetailsWrapper
	{
		public NX5105ImporterWrapper(OrgAddress orgAddress, ZString customsControlId, TWJobDocAddress jobDocAddress, params string[] codesToLookFor)
			: base(orgAddress, orgAddress, codesToLookFor)
		{
			this.customsControlId = customsControlId;
			importerDocumentaryAddress = Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
		}

		readonly TWJobDocAddress importerDocumentaryAddress;
		readonly ZString customsControlId;

		protected override ZString CustomsControlIDCore => customsControlId;

		protected override ZString PaymentOnAccountBusinessIDCore => importerDocumentaryAddress.TPCCode;

		protected override ZString TypeCodeCore => importerDocumentaryAddress.TypeCode;

		protected override ZString Communications1IdCore => importerDocumentaryAddress.E2_Phone;

		protected override ZString Communications2IdCore => importerDocumentaryAddress.E2_Email;

		protected override ZString LPCOAuthorizedPartyIDCore => FormatAEONumber(importerDocumentaryAddress.AEOCode);

		protected override ZString IDCore => SharedHelper.GetIDStartWithNO(importerDocumentaryAddress.IDCode, TypeCodeCore);

		internal override ZBool ShouldAllowShowNoEnglishName => true;

		protected override AddressData GetEnglishAddress()
		{
			return new AddressData(importerDocumentaryAddress, false, false, SharedHelper.GetEnglishLanguageCodes());
		}

		protected override AddressData GetChineseTraditionalAddress()
		{
			if (importerDocumentaryAddress.E2_AddressOverride)
			{
				return new AddressData(importerDocumentaryAddress.LocalAddress, Core.SharedConstants.Languages.ChineseTraditional);
			}
			else
			{
				return base.GetChineseTraditionalAddress();
			}
		}
	}
}
