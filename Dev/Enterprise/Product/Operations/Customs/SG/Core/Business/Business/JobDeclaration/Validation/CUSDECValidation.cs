using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class CUSDECValidation : JobDeclarationValidation
	{
		public CUSDECValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		#region CheckMessageSubtype

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_MessageSubTypeInfo, "Declaration Type");

			if (MessageSubTypeList != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_MessageSubTypeInfo, MessageSubTypeList, (NoResString)"Please enter a valid Declaration Type");
			}
		}

		protected abstract ICodeDescriptionPairList MessageSubTypeList { get; }

		#endregion

		protected override void CheckJE_ContainerMode()
		{
			base.CheckJE_ContainerMode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ContainerModeInfo, "Cargo Packing Type");

			if (Parent.JE_ContainerCount > 0 && !Parent.IsContainerised)
			{
				var errorMsg = Parent.IsTradeNet4Point1 ? "Cargo Packing Type must be 9 if containers have been entered" : "Cargo Packing Type must be 3 if containers have been entered";
				Parent.JE_ContainerModeInfo.AddMessageError(errorMsg);
			}
		}

		protected override void CheckJE_ContainerCount()
		{
			base.CheckJE_ContainerCount();
			if (Parent.JE_ContainerCount == 0 && Parent.IsContainerised)
			{
				Parent.JE_ContainerCountInfo.AddMessageError("Total number of containers should be greater than 0 for Containerised Cargo.");
			}

			if (Parent.JE_ContainerCount > 99)
			{
				Parent.JE_ContainerCountInfo.AddMessageError("Total number of containers must be less than 100. Singapore Customs messaging only allows for upto 99 containers to be sent in any 1 message.");
			}
		}

		protected override void CheckJE_TotalNoOfPacksDecimal()
		{
			CompareValidation.CheckNumberNotNegative(Parent.JE_TotalNoOfPacksDecimalInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.JE_TotalNoOfPacksDecimalInfo, 99999999);

			if (Parent.JE_TotalNoOfPacksDecimal == 0)
			{
				Parent.JE_TotalNoOfPacksDecimalInfo.AddMessageError("Outer Packs Count should be greater than 0.");
			}

			ValidateJE_TotalNoOfPacksPackType();
		}

		protected override void CheckJE_TotalNoOfPacksPackType()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalNoOfPacksPackTypeInfo, "Outer Packs Unit");
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TotalNoOfPacksPackTypeInfo, Parent.Lookups.JE_TotalNoOfPacksPackType_List);

			ValidateJE_TotalNoOfPacksDecimal();
		}

		protected override void CheckJE_TotalWeight()
		{
			base.CheckJE_TotalWeight();
			if (Parent.JE_TotalWeight <= 0)
			{
				Parent.JE_TotalWeightInfo.AddMessageError("Total Weight should be greater than 0.");
			}

			ValidateJE_TotalWeightUnit();
		}

		protected override void CheckJE_TotalWeightUnit()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TotalWeightUnitInfo, Parent.Lookups.WeightUnitList);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalWeightUnitInfo, "Weight Unit");

			ValidateJE_TotalWeight();
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			if (!Parent.JE_TransportMode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKPortOfLoadingInfo, "Port of Loading");
			}
		}

		protected void CheckForwarder()
		{
			base.CheckJE_OH_Forwarder();
			if (!Parent.JE_HouseBill.IsEmpty || !Parent.SG_OutwardHAWB.IsEmpty || HasHouseBillOnLines)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_ForwarderInfo, "Forwarder");
			}
		}

		bool HasHouseBillOnLines
		{
			get
			{
				foreach (JobComInvoiceHeader invoiceHeader in Parent.Invoices)
				{
					foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
					{
						if (!invoiceLine.SG_InwardHAWB.IsEmpty || !invoiceLine.SG_OutwardHAWB.IsEmpty)
						{
							return true;
						}
					}
				}

				return false;
			}
		}
	}
}
