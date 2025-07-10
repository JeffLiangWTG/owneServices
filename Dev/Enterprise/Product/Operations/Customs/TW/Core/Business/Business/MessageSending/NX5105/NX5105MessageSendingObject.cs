using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX5105MessageSendingObject : DeclarationMessageSendingObject, INX5105Declaration
	{
		readonly JobDeclaration declaration;

		public NX5105MessageSendingObject(CusEntryHeader header, bool includeControllingMessageInformation = false, bool isAirCraftParts = false)
			: base(header)
		{
			IncludeControllingMessageInformation = includeControllingMessageInformation;
			declaration = Argument.NotNull(header.Declaration, "declaration");
			this.isAirCraftParts = isAirCraftParts;
		}

		protected readonly ZBool IncludeControllingMessageInformation;

		readonly bool isAirCraftParts;

		public override ZString MessageType => isAirCraftParts ? MessageTypeCodeList.Codes.CAA : MessageTypeCodeList.Codes.ICD;

		ZDate INX5105Declaration.AcceptanceDateTime
		{
			get
			{
				var result = entryInstruction.CEI_DateForDuty.Date;
				return result.IsValid ? result : ZDate.Today;
			}
		}

		ZString INX5105Declaration.Authentication => ZString.Empty;

		ZString INX5105Declaration.ID => Header.EntryNumberForSendingObject;

		ZDecimal INX5105Declaration.InvoiceAmount => Header.CH_TotalCustomsValueInInvoiceCurrency;

		ZDecimal INX5105Declaration.TotalGrossMassMeasure => declaration.GrossWeightInKilos();

		ZInt INX5105Declaration.TotalPackageQuantity => declaration.JE_TotalNoOfPacks;

		ZString INX5105Declaration.AssociatedGovernmentProcedureCode => entryInstruction.CEI_ExamMode;

		ZString INX5105Declaration.CombinedNote
		{
			get
			{
				var result = ZString.Empty;
				if (IncludeControllingMessageInformation && !isAirCraftParts)
				{
					result = Customs.Business.YesNoList.Codes.Yes;
				}
				return result;
			}
		}

		ZString INX5105Declaration.TypeCode => entryInstruction.CEI_Style;

		IAdditionalInformation INX5105Declaration.AdditionalInformation => GetAdditionalInformation();

		IPartyDetails INX5105Declaration.Agent => PartyHelper.GetAgent(declaration);

		ITransportMeans INX5105Declaration.BorderTransportMeans => new NX5105Declaration_BorderTransportMeans(Header);

		ICurrencyExchange INX5105Declaration.CurrencyExchange => new NX5105Declaration_CurrencyExchange(Header);

		IDutyTaxFee INX5105Declaration.DutyTaxFee => new NX5105Declaration_DutyTaxFee(Header);

		IGoodsShipment INX5105Declaration.GoodsShipment => GetGoodsShipment();

		protected virtual IGoodsShipment GetGoodsShipment()
		{
			return new NX5105Declaration_GoodsShipment(Header, SupportingDocuments, GetAllEDocs(), IncludeControllingMessageInformation);
		}

		IEnumerable<ZString> INX5105Declaration.GovernmentProcedureDescriptions => GetGovernmentProcedureDescriptions();

		public IPartyDetails Importer => PartyHelper.GetImporter(declaration);

		IDeclarationPackaging INX5105Declaration.Packaging => new NX5105Declaration_DeclarationPackaging(Header);

		ZString INX5105Declaration.RepresentativePersonName => Header.Declaration.CusAgentCertificateNumber;

		#region Applications
		IEnumerable<IApplication> INX5105Declaration.Applications => IncludeControllingMessageInformation ? ControllingMessageApplications : null;

		IEnumerable<IApplication> ControllingMessageApplications
		{
			get
			{
				var validCMHeaders = new Dictionary<CusTWControllingMessageHeader, List<CusEntryLine>>();
				foreach (var invoiceLine in Header.InvoiceLines.Cast<JobComInvoiceLine>())
				{
					foreach (CusTWControllingMessageHeader controllingMessageHeader in entryInstruction.ControllingMessageHeaders)
					{
						if (invoiceLine.InvoiceLineRelatedControllingMsgHeadersGenPivots.Contains(controllingMessageHeader))
						{
							if (!validCMHeaders.ContainsKey(controllingMessageHeader))
							{
								validCMHeaders.Add(controllingMessageHeader, new List<CusEntryLine>());
							}
							validCMHeaders[controllingMessageHeader].Add(invoiceLine.CusEntryLine);
						}
					}
				}
				foreach (var cmHeader in validCMHeaders.Keys)
				{
					yield return new NX5105CMApplication(cmHeader, validCMHeaders[cmHeader]);
				}
			}
		}
		#endregion

		public override ZString GetMessageOwner()
		{
			return Importer?.ID ?? ZString.Empty;
		}

		public override ZString SerializeToMessageString()
		{
			return new NX5105MessageBuilder().SerializeToMessageString(this, Action);
		}
	}
}
