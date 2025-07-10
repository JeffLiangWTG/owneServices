using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public sealed class LetterofAuthorizationPersonalDocumentWrapper : DocumentWrapper, IDocumentWrapper
	{
		public LetterofAuthorizationPersonalDocumentWrapper(JobDeclaration declaration, BusinessObjectFactory factory) : base(declaration, factory)
		{
			this.declaration = declaration;
			IsImport = this.declaration.IsImport;
			IsExport = this.declaration.IsExport;
			entryHeader = this.declaration.EntryHeader;
			agent = PartyHelper.GetAgent(this.declaration);
			consignor = IsImport ? PartyHelper.GetImporter(this.declaration) : PartyHelper.GetExporter(this.declaration);
		}

		#region Fields
		readonly JobDeclaration declaration;

		readonly CusEntryHeader entryHeader;

		readonly IPartyDetails agent;

		readonly IPartyDetails consignor;

		public ZBool IsImport { get; private set; }

		public ZBool IsExport { get; private set; }

		public ZString ConsignorChineseAddress => consignor?.Address?.ChineseLine ?? ZString.Empty;

		public ZString ConsignorChineseName => consignor?.ChineseName ?? ZString.Empty;

		public ZString ConsignorTaxID => consignor?.ID ?? ZString.Empty;

		public ZString ConsignorPhone => consignor?.Communications?.FirstOrDefault(x => x.TypeID == CommunicationTypeIDs.TE)?.ID ?? ZString.Empty;

		public ZString CustomsControlID => consignor?.CustomsControlID ?? ZString.Empty;

		public ZString ImportDeclarationID => IsImport ? DeclarationID : ZString.Empty;

		public ZString ExportDeclarationID => IsExport ? DeclarationID : ZString.Empty;

		public ZString DeclarationID
		{
			get
			{
				var result = declaration.EntryNumber;
				if (result.IsEmpty)
				{
					result = entryHeader?.DeclarationNumber ?? ZString.Empty;
				}
				return result;
			}
		}

		public ZString ImportGoodsShipmentID => IsImport ? GetShipmentId() : ZString.Empty;

		public ZString ExportGoodsShipmentID => IsExport ? GetShipmentId() : ZString.Empty;

		ZString GetShipmentId()
		{
			return declaration.GetTransportContractDocumentsWithMasterBillSegmentID((id, typeCode) => new TransportContractDocumentWrapper(id, typeCode)).FirstOrDefault(x => x.TypeCode == TransportContractDocumentTypeCodes._703 || x.TypeCode == TransportContractDocumentTypeCodes._714)?.ID ?? ZString.Empty;
		}

		public ZString BrokerageBoxNumber => agent?.ID ?? ZString.Empty;

		public ZString AgentChineseName => agent?.ChineseName ?? ZString.Empty;

		public ZString AgentChineseAddress => agent?.Address?.ChineseLine ?? ZString.Empty;

		public ZString AgentPhone => AgentAddress != null && (AgentAddress.Language == Core.SharedConstants.Languages.ChineseTraditional || AgentAddress.TranslatedAddresses.Any(x => x.Language == Core.SharedConstants.Languages.ChineseTraditional)) ? AgentAddress.OA_Phone : ZString.Empty;

		ZDateTime DateForDuty => Factory.GetValue(ref dateForDutyCached, () => declaration.CusEntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty);

		CachedProperty<ZDateTime> dateForDutyCached;

		public ZString AcceptanceDateTimeYear => DateForDuty.ToTaiWanYear();

		public ZString AcceptanceDateTimeMonth
		{
			get
			{
				var result = ZString.Empty;
				if (!DateForDuty.IsEmpty)
				{
					result = DateForDuty.Month.ToString("D2", CultureInfo.InvariantCulture);
				}
				return result;
			}
		}

		public ZString AcceptanceDateTimeDay
		{
			get
			{
				var result = ZString.Empty;
				if (!DateForDuty.IsEmpty)
				{
					result = DateForDuty.Day.ToString("D2", CultureInfo.InvariantCulture);
				}
				return result;
			}
		}

		public OrgAddress AgentAddress => declaration.DeclarantAddress;

		public ZString CustomsOffice
		{
			get
			{
				var customsOffice = declaration.CusEntryInstruction?.CEI_CustomsOffice.Left(1) ?? ZString.Empty;
				ZString result = new TaiwanCustomsDistrictList().GetDescriptionFromCode(customsOffice);
				if (!result.IsEmpty)
				{
					result = result.Left(2);
				}
				return result;
			}
		}
		#endregion
	}
}
