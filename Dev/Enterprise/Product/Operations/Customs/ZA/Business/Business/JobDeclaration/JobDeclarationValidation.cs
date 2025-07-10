using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	using CargoWise.Types;
	using MasterFiles.Business;
	using Universal;
	public class JobDeclarationValidation : AutoZAJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		public new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		#region Implementation

		#region Override
		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAgentCode();
			ValidateJE_OH_AgentOverride();
		}

		#region ValidateAgentCode

		public void ValidateAgentCode()
		{
			ValidateCalculatedProperty(Parent.AgentCodeInfo);
		}

		protected void CheckAgentCode()
		{
			if (Parent?.AgentCode.IsEmpty ?? false)
			{
				Parent.AgentCodeInfo.AddMessageError(ResString.GetMultilingualString("585A8BEA-4CF1-4B1D-964A-D969DFC41E1A", "Please select an Agent with valid South Africa Agent Code on Misc. tab"));
			}
		}

		#endregion

		#region ValidateJE_OH_AgentOverride

		public void ValidateJE_OH_AgentOverride()
		{
			ValidateCalculatedProperty(Parent.JE_OH_AgentOverrideInfo);
		}

		protected virtual void CheckJE_OH_AgentOverride()
		{
			var targetInfo = Parent.JE_OH_AgentOverrideInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			var agent = Parent.AgentOverride;
			var country = Parent.Country;
			if (agent != null && country != null && agent.GetAgentCode(country).Length == 0)
			{
				targetInfo.AddMessageError("The selected agent does not have an Agent Code.");
			}
		}

		#endregion

		#region MasterBill
		public override bool IsMasterBillMandatory
		{
			get { return MessageDataProviderInstruction.ShouldOutputTransportDocumentNumber(MessageKeyFactor); }
		}

		protected override void CheckJE_MasterBill()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				base.CheckJE_MasterBill();
				if (Parent.JE_MasterBill.StartsWith("000") && Parent.IsAir)
				{
					Parent.JE_MasterBillInfo.AddWarning(MasterBillShouldNotPrefixByThreeZeros);
				}
				if (!Parent.JE_MasterBill.IsEmpty && !MessageDataProviderInstruction.ShouldOutputTransportDocumentNumber(MessageKeyFactor))
				{
					Parent.JE_MasterBillInfo.AddMessageError(MasterBillMustBeEmpty(Parent.JE_MasterBillInfo.HumanReadableName));
				}
			}
		}

		public static string MasterBillMustBeEmpty(string label)
		{
			return Res.GetString("4c119660-5e2e-4103-984c-8ad955005f10", "{0} must not be captured.", label);
		}

		public static string MasterBillShouldNotPrefixByThreeZeros
		{
			get { return Res.GetString("fc2aa9d8-6270-4fbc-ba5e-60ae09b54d71", "Use of Air Waybill prefix code 000 may result in a Customs Penalty"); }
		}

		#endregion

		#region VoyageFlightNo
		protected override void CheckJE_VoyageFlightNo()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				base.CheckJE_VoyageFlightNo();
				if (Parent.JE_VoyageFlightNo.IsEmpty && (MessageDataProviderInstruction.ShouldOutputVoyageFlightNo(MessageKeyFactor) || MessageDataProviderInstruction.ShouldOutputRoadVehicle(MessageKeyFactor)))
				{
					Parent.JE_VoyageFlightNoInfo.AddMessageError(VoyageFlightNoMandatory(Parent.JE_VoyageFlightNoInfo.Description));
				}
			}
		}

		public static string VoyageFlightNoMandatory(ZString voyageFlightNoLabel)
		{
			return Res.GetString("e4d4cc16-c7ea-4823-8491-054651707c09", "{0} must be set when sending the message", voyageFlightNoLabel);
		}
		#endregion

		#region MergeBy
		protected override void CheckJE_MergeBy()
		{
			base.CheckJE_MergeBy();

			if (Parent.InvoiceLines?.Count > 0 && !Parent.IsMergeByValidForPreviousProcedureCode)
			{
				Parent.JE_MergeByInfo.AddMessageError(ValidationConstants.InvoiceLine.MergingNotRecommendedWherePreviousProcedureCodeIsNonZero);
			}
		}
		#endregion

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsOfficeInfo);
			ValidateJE_LocationOfGoods();
		}

		protected override void CheckJE_LocationOfGoods()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				var targetInfo = Parent.JE_LocationOfGoodsInfo;
				if (Parent.JE_LocationOfGoods.IsEmpty)
				{
					if (!Parent.IsExWarehouse && (Parent.IsAir || Parent.IsSea))
					{
						targetInfo.AddMessageError(LocationOfGoodsMandatory);
					}
				}
				else
				{
					var locationOfGoods = Parent.LocationOfGoods;
					if (locationOfGoods == null)
					{
						targetInfo.AddMessageError(ListValidation.GetNotificationMessage(targetInfo).ToString());
					}
					else
					{
						if (!locationOfGoods.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.DistrictOffices, Parent.JE_CustomsOffice))
						{
							targetInfo.AddWarning(ValidationConstants.Declaration.LocationOfGoodsDoesNotBelongToCustomsOffice(Parent.JE_LocationOfGoods, Parent.JE_CustomsOffice));
						}
						if (Parent.HasCusContainers && !locationOfGoods.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.DepotType, UniversalReferenceConstants.RefCusCodeListAttributes.Values.Containerised))
						{
							targetInfo.AddMessageError(ValidationConstants.Declaration.LocationOfGoodsNotForContainerisedCargo);
						}
						if (!locationOfGoods.MatchTransportMode(Parent.TransportMode))
						{
							targetInfo.AddMessageError(ValidationConstants.Declaration.LocationOfGoodsNotForTransportMode);
						}
					}
				}
			}
		}

		public static string LocationOfGoodsMandatory
		{
			get { return Res.GetString("17aa15da-698a-47f2-a232-89e6a64f3107", "Location of Goods is required."); }
		}

		protected override void CheckJE_CarrierCode()
		{
			if (!(Parent.IsExWarehouse || Parent.IsImportByExternalBroker) && Parent.IsSea)
			{
				var includeSeaExportValidation = !(Parent.IsExport && Parent.JE_CarrierCode.IsEmpty);
				if (includeSeaExportValidation)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CarrierCodeInfo);
				}
			}
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			if (!(Parent.IsExWarehouse || Parent.IsImportByExternalBroker))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_RL_NKOriginInfo, Parent.Lookups.Origins);
			}
		}
		public static string CountryOfOriginMandatory
		{
			get { return Res.GetString("0E6D12D3-909C-4E26-B252-DBC0A75ABA59", "Port of Origin is mandatory"); }
		}

		#region Vessel
		protected override void CheckJE_VesselName()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_VesselNameInfo);
				if (MessageDataProviderInstruction.ShouldOutputVessel(MessageKeyFactor))
				{
					if (Parent.JE_VesselName.IsEmpty)
					{
						Parent.JE_VesselNameInfo.AddMessageError(VesselMandatory);
					}
				}
			}
		}

		public static string VesselMandatory
		{
			get { return Res.GetString("dfa4cbb0-38a3-44ee-82ec-6cde71d1b435", "You must select a vessel."); }
		}

		#endregion

		public static string FinalDestMandatory
		{
			get { return Res.GetString("c2313df4-5348-40e4-9984-e827865b7bc4", "Final Destination needs to be entered for this declaration."); }
		}

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();
			if (Parent.JE_ApplicationCode.IsEmpty)
			{
				Parent.JE_ApplicationCodeInfo.AddError(EmptyApplicationCode);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_ApplicationCodeInfo);
		}

		public static string EmptyApplicationCode
		{
			get { return Res.GetString("00F3B5EC-E189-42AB-802B-5D573DEB7E70", "You must select a Message Mode."); }
		}

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_GoodsOriginInfo);
		}

		protected override void CheckJE_TotalWeight()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				base.CheckJE_TotalWeight();
				if (Parent.IsImport && !Parent.IsExWarehouse || Parent.IsExport)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalWeightInfo);
				}
			}
		}

		protected override void CheckJE_ContainerMode()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				base.CheckJE_ContainerMode();
			}
		}

		protected override void CheckJE_DateOfArrival()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				base.CheckJE_DateOfArrival();
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			ValidateJE_Carrier();
			ValidateJE_RadioCallSign();
		}

		protected override void CheckJE_TransportModeMandatory()
		{
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();
			ValidateJE_OH_Supplier();
			ValidateJE_Carrier();
			ValidateJE_RadioCallSign();
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			if (!MessageKeyFactor.IsExBond())
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_SupplierInfo);
			}
		}

		protected override void CheckJE_MarksAndNumbersShort()
		{
			base.CheckJE_MarksAndNumbersShort();

			ValidationHelper.ValidateBlankLinesForEDIFACT(Parent.JE_MarksAndNumbersShortInfo, Parent.JE_MarksAndNumbers);
		}

		internal MessageDataProviderKeyFactor MessageKeyFactor
		{
			get
			{
				if (messageKeyFactor == null)
				{
					messageKeyFactor = new MessageDataProviderKeyFactor()
					{
						ShipmentType = Parent.JE_MessageType,
						TransportMode = Parent.JE_TransportMode,
						RemovalTransportMode = Parent.JE_RemovalTransportCode,
						PPC = ZString.Empty,
						CPC = ZString.Empty,
						MessageType = ZString.Empty,
						CountryOfDestination = Parent.FinalDestination?.Country,
						CountryOfOrigin = Parent.Origin?.Country
					};
				}
				return messageKeyFactor;
			}
		}
		MessageDataProviderKeyFactor messageKeyFactor;

		#endregion

		protected void CheckUnregisteredTrader(OrgHeader org, ZPropertyInfo propertyInfo, string unregisteredTraderType)
		{
			var checkValue = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.IDNumber);

			if (checkValue.IsEmpty)
			{
				checkValue = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.TaxFileCode);
			}

			if (checkValue.IsEmpty)
			{
				if (!OrgHelper.UsePassport)
				{
					propertyInfo.AddMessageError(ValidationConstants.Declaration.UnregisteredTraderDoesNotHaveIDOrGTXCode(unregisteredTraderType));
				}
				else
				{
					checkValue = OrgHelper.GetPassportNumber(org);

					if (checkValue.IsEmpty)
					{
						propertyInfo.AddMessageError(ValidationConstants.Declaration.UnregisteredTraderDoesNotHaveIDPassportOrGTXCode(unregisteredTraderType));
					}
				}
			}
		}

		#endregion

		#region AddInfo

		protected override void CheckJE_MasterBillIssuedDate()
		{
			if (Parent.JE_MasterBillIssuedDate.IsEmpty && !Parent.JE_MasterBill.IsEmpty && MessageDataProviderInstruction.ShouldOutputTransportDocumentNumber(MessageKeyFactor))
			{
				Parent.JE_MasterBillIssuedDateInfo.AddMessageError(ValidationConstants.Declaration.MasterBillIssuedDateCannotBeEmpty(Parent.JE_MasterBillInfo.HumanReadableName));
			}
		}

		protected override void CheckJE_RL_NKMasterBillIssuedAt()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RL_NKMasterBillIssuedAtInfo, Parent.Lookups.MasterBillIssuedAts);

			if (Parent.JE_RL_NKMasterBillIssuedAt.IsEmpty)
			{
				if (!Parent.JE_MasterBill.IsEmpty && !Parent.IsExWarehouse && !Parent.IsPost && Parent.JE_TransportMode != Core.Constants.TransportModes.FixedTransportInstallations && !Parent.IsUnknownOrNotApplicable)
				{
					Parent.JE_RL_NKMasterBillIssuedAtInfo.AddMessageError(ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmpty(Parent.JE_MasterBillInfo.HumanReadableName));
				}
			}
			else if (!Parent.JE_RL_NKOrigin.IsEmpty && (Parent.JE_RL_NKOrigin.Left(2) != Parent.JE_RL_NKMasterBillIssuedAt.Left(2)))
			{
				Parent.JE_RL_NKMasterBillIssuedAtInfo.AddWarning(ValidationConstants.Declaration.MasterBillIsseudAtCountryDifferent);
			}
		}

		protected override void CheckJE_VATClaimBackIndicator()
		{
			base.CheckJE_VATClaimBackIndicator();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_VATClaimBackIndicatorInfo, Parent.Lookups.VATClaimBackIndicator);
		}

		protected override void CheckJE_ValuationDate()
		{
			base.CheckJE_ValuationDate();
			if (Parent.JE_MessageType == ZAJobMessageTypeList.Codes.Import && Parent.JE_ValuationDate.IsEmpty)
			{
				Parent.JE_ValuationDateInfo.AddError(ValidationConstants.Declaration.ExchangeRateDateIsRequired);
			}
		}

		protected override void CheckJE_ROOCert()
		{
			base.CheckJE_ROOCert();
			ValidateJE_ROOType();
		}

		protected override void CheckJE_ROOType()
		{
			base.CheckJE_ROOType();
			if (Parent.IsExport)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_ROOTypeInfo, Parent.Lookups.ROOTypesList);
				if (Parent.JE_ROOType.IsEmpty && !Parent.JE_ROOCert.IsEmpty)
				{
					Parent.JE_ROOTypeInfo.AddMessageError(ValidationConstants.InvoiceLine.NoROOTypeEnteredForCert);
				}
			}
		}

		protected override void CheckJE_Carrier()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				var includeSeaExportValidation = !(Parent.IsSea && Parent.IsExport && Parent.JE_Carrier.IsEmpty);
				if (includeSeaExportValidation)
				{
					base.CheckJE_Carrier();
					if (MessageDataProviderInstruction.ShouldOutputVessel(MessageKeyFactor))
					{
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CarrierInfo);
					}
				}
			}
		}

		protected override void CheckJE_RadioCallSign()
		{
			if (!Parent.IsImportByExternalBroker)
			{
				base.CheckJE_RadioCallSign();
				if (MessageDataProviderInstruction.ShouldOutputVessel(MessageKeyFactor))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RadioCallSignInfo);
				}
			}
		}

		protected override void CheckJE_Trailer2()
		{
			base.CheckJE_Trailer2();
			if (!Parent.JE_Trailer2.IsEmpty && Parent.JE_Trailer1.IsEmpty)
			{
				Parent.JE_Trailer2Info.AddMessageError(Trailer1NotEntered);
			}
		}

		protected override void CheckJE_RemovalTransportCode()
		{
			base.CheckJE_RemovalTransportCode();

			if (Parent.JE_RemovalTransportCode.IsEmpty && MessageDataProviderInstruction.IsBlns(Parent.FinalDestination?.Country) && MessageDataProviderInstruction.IsShipmentImpOrExw(Parent.JE_MessageType))
			{
				Parent.JE_RemovalTransportCodeInfo.AddWarning(ValidationConstants.AdditionalInformation.RemovalTransportModeRequired);
			}
		}

		public static string Trailer1NotEntered
		{
			get { return Res.GetString("994d41b3-56a5-43a0-949a-5082b069e5e0", "Trailer 1 has not been entered."); }
		}

		#endregion
	}
}
