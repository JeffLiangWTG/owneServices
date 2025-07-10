using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5203
{
	class Exporter : PartyDetails
	{
		public Exporter(JobDeclaration declaration, OrgHeader orgHeader, TWJobDocAddress jobDocAddress)
			: base(declaration, orgHeader, jobDocAddress?.Address ?? orgHeader.MainAddress)
		{
			supplierDocumentaryAddress = Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
			entryInstruction = Argument.NotNull(declaration.CusEntryInstruction, nameof(entryInstruction));
		}

		readonly CusEntryInstruction entryInstruction;

		readonly TWJobDocAddress supplierDocumentaryAddress;

		protected override ZString CustomsControlIDCore => supplierDocumentaryAddress.CBPCode;

		protected override ZString PaymentOnAccountBusinessIDCore => supplierDocumentaryAddress.TPCCode;

		protected override IEnumerable<ICommunication> CommunicationsCore
		{
			get
			{
				if (!Communications1IdCore.IsEmpty)
				{
					yield return new CommunicationWrapper(Communications1IdCore, Communications1TypeID);
				}

				if (!Communications2IdCore.IsEmpty)
				{
					yield return new CommunicationWrapper(Communications2IdCore, Communications2TypeID);
				}
			}
		}

		ZString Communications1IdCore => orgHeader?.MainAddress?.OA_Phone ?? ZString.Empty;

		ZString Communications1TypeID => MessageConstants.CommunicationTypeIDs.TE;

		ZString Communications2IdCore => orgHeader?.MainAddress?.OA_Email ?? ZString.Empty;

		ZString Communications2TypeID => MessageConstants.CommunicationTypeIDs.MA;

		protected override AddressData GetEnglishAddress()
		{
			return new ExporterAddressData(orgHeader, supplierDocumentaryAddress, SharedHelper.GetEnglishLanguageCodes());
		}

		protected override AddressData GetChineseTraditionalAddress()
		{
			var jobDocAddress = supplierDocumentaryAddress.E2_AddressOverride ? supplierDocumentaryAddress.LocalAddress : supplierDocumentaryAddress;
			return new ExporterAddressData(orgHeader, jobDocAddress, Core.SharedConstants.Languages.ChineseTraditional);
		}

		protected override ZString LPCOAuthorizedPartyIDCore => FormatAEONumber(supplierDocumentaryAddress.AEOCode);

		protected override ZString TypeCodeCore => supplierDocumentaryAddress.TypeCode;

		protected override ZString IDCore => SharedHelper.GetIDStartWithNO(supplierDocumentaryAddress.IDCode, TypeCodeCore);
	}
}
