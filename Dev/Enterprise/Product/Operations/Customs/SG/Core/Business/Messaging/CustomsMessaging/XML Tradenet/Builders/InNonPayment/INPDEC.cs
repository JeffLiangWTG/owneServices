using System.Globalization;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class INPDEC : BaseTradeNetMessage<InNonPayment>
	{
		public INPDEC(IINPDEC cusDec)
			: base(cusDec)
		{
		}

		public new IINPDEC CusDec => (IINPDEC)base.CusDec;

		public override string MessageType => CommonAccessReferenceCodeList.Codes.INPDEC;
		public override string MessageSubType => CUSDECEDIMessage.Declaration;

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			inboundMessage.InNonPayment = BuildDeclaration();
		}

		#region Cargo
		protected override Cargo BuildCargoCore()
		{
			var cargo = base.BuildCargoCore();
			BuildExhibitionTemporaryImportPeriod(cargo);

			if (CusDec.Is2bStoredBWCY || CusDec.IsStorageInFTZ)
			{
				BuildStorageLocation(cargo);
			}

			return cargo;
		}
		#endregion

		#region Transport
		protected override OutwardTransportAdditionalVesselInformation BuildOutwardTransportAdditionalVesselInformationCore()
		{
			var additionalVesselInformation = base.BuildOutwardTransportAdditionalVesselInformationCore();
			var hasData = additionalVesselInformation != null;

			if (CusDec.HasOutwardTransport)
			{
				additionalVesselInformation = additionalVesselInformation ?? new OutwardTransportAdditionalVesselInformation();

				if (CusDec.IsSeaStoreDeclaration)
				{
					hasData |= BuildLoadingNextPortCore(additionalVesselInformation);

					if (CusDec.HasLiquorOrTobacco)
					{
						hasData |= BuildLoadingFinalPortCore(additionalVesselInformation);
					}
				}
			}

			return hasData ? additionalVesselInformation : null;
		}

		protected override OutwardTransport BuildOutwardTransportCore()
		{
			var outwardTransport = base.BuildOutwardTransportCore();

			if (CusDec.HasOutwardTransport && !CusDec.IsSeaStoreDeclaration && (CusDec.DeclarationType == DeclarationTypeCodeList.Codes.REX || CusDec.DeclarationType == DeclarationTypeCodeList.Codes.SFZ))
			{
				SetValueIfNotEmpty(CusDec.CountryOfFinalDestination, (c) => outwardTransport.FinalDestinationCountry = c);
			}

			return outwardTransport;
		}

		protected override ExhibitionTemporaryImportPeriod BuildExhibitionTemporaryImportPeriodCore()
		{
			var result = base.BuildExhibitionTemporaryImportPeriodCore();

			if (CusDec.IsTemporaryConsignment)
			{
				var startDate = CusDec.StartDateOfTemporaryImport;
				var endDate = CusDec.EndDateOfTemporaryImport;

				if (!startDate.IsEmpty)
				{
					result = result ?? new ExhibitionTemporaryImportPeriod();
					result.StartDate = startDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
				}

				if (!endDate.IsEmpty)
				{
					result = result ?? new ExhibitionTemporaryImportPeriod();
					result.EndDate = endDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
				}
			}

			return result;
		}
		#endregion

		#region Invoice

		protected override bool SupportsInvoiceNumberSegment => true;

		#endregion

		#region Tariff

		protected override bool SupportsOtherTax => true;

		#endregion

		#region MotorVehicle
		protected override bool SupportsRegistrationDateSection => true;
		#endregion
	}
}
