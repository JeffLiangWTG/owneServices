using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.BR;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentCustomsEntryNumberValidation : ShipmentCustomsEntryNumberValidation
	{
		public ForwardingShipmentCustomsEntryNumberValidation(ForwardingShipmentCustomsEntryNumber parent)
			: base(parent)
		{
		}

		protected new ForwardingShipmentCustomsEntryNumber Parent { get { return (ForwardingShipmentCustomsEntryNumber)base.Parent; } }

		protected override void CheckEntryNumber()
		{
			base.CheckEntryNumber();

			if (Parent.IsEntryNumberForShipmentBinding && Parent.Shipment.CusEntryNumbers.Count > 1)
			{
				Parent.Shipment.CusEntryNumbers.Select(n => new ForwardingShipmentCusEntryNumberProxy(Parent.Shipment, n)).ForEach(p =>
				{
					p.Validation.ValidateEntryNumber();
					if (p.HasWarnings)
					{
						p.EntryNumberInfo.GetWarnings().ForEach(e => Parent.EntryNumberInfo.AddWarning(e.Message));
					}
					if (p.HasMessageErrors)
					{
						p.EntryNumberInfo.GetMessageErrors().ForEach(e => Parent.EntryNumberInfo.AddMessageError(e.Message));
					}
					if (p.HasErrors)
					{
						p.EntryNumberInfo.GetErrors().ForEach(e => Parent.EntryNumberInfo.AddError(e.Message));
					}
				});
			}
			else
			{
				CheckEntryNumberCore();
			}
		}

		void CheckEntryNumberCore()
		{
			var parent = Parent;
			var shipment = parent.Shipment;
			var entryType = parent.EntryType;
			var entryNumber = parent.EntryNumber;
			var entryNumberIsEmpty = entryNumber.IsEmpty;
			var propertyInfo = parent.EntryNumberInfo;
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			propertyInfo.RunAdditionalValidation();

			if (shipment.Consols.Count == 0)
			{
				propertyInfo.AddWarning(Res.GetString("b2172dee-e830-4adf-8ae4-8b5c245175f5", "This Shipment is not attached to a Consolidation"));
			}

			if (entryType == CusEntryNumberTypes.UnitedStates.ITN && Core.Constants.CountryCodes.IsUsaOrTerritory(countryCode) && Core.Constants.CountryCodes.IsUsaOrTerritory(shipment.JS_RL_NKOrigin.Left(2)))
			{
				var isNoEEI = IsNoEEI(entryNumber) || IsNoEEI(shipment.DocsAndCartage.Lookups.JP_ExportStatementList.GetDescriptionFromCode(shipment.DocsAndCartage.JP_ExportStatement));
				var shouldValidate = !shipment.IsDomesticFreight && !isNoEEI;

				if (shouldValidate)
				{
					if (entryNumberIsEmpty)
					{
						propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(CusEntryNumberTypes.UnitedStates.ITN));
					}
					else if (!IsValidITNNumber(entryNumber))
					{
						propertyInfo.AddMessageError(Res.GetString("c339bce2-c276-494c-b709-d4f7a452f787", "The number must start with the letter \"X\", followed by the year, month and day of acceptance in the AES, and six randomly assigned digits."));
					}
				}
			}

			if (entryType == CusEntryNumberTypes.Australia.CAN && (!HasCoLoadMasterWithCAN || !entryNumberIsEmpty) && (ChildCoLoadsWithNoCan || !entryNumberIsEmpty))
			{
				new CANValidation().ValidateCANField(propertyInfo);
			}

			if (entryType == CusEntryNumberTypes.Brazil.DUE)
			{
				ValidateDUEEntryNumber();
			}

			if (countryCode == Core.Constants.CountryCodes.NewZealand)
			{
				if (!entryNumberIsEmpty && !ValidNZEntryNumber)
				{
					propertyInfo.AddWarning(Res.GetString("00E90D55-694C-4E87-9393-204784532587", "NZ Customs Entry Number can contain numbers only. Non-numeric characters will be stripped out of any NZ message sent."));
				}
			}

			if (entryType == CusEntryNumberTypes.Standard.MovementReferenceNumber && !entryNumberIsEmpty)
			{
				if (parent.Shipment.OuterPackLines.Any(packLine => !((PackLine)packLine).JL_ExportRefNumber.IsEmpty))
				{
					propertyInfo.AddWarning(Res.GetString("a46f918b-36fd-49bd-89d2-61f7a6542a97", "Another Export Reference Number exists on a pack line level. This shipment level MRN will only apply to packs that have no MRN entered. If you are planning on obtaining different MRN numbers for the remaining packs, please remove MRN from the shipment level."));
				}

				if (countryCode == Core.Constants.CountryCodes.SouthAfrica)
				{
					if (entryNumber.Length < 18)
					{
						propertyInfo.AddMessageError(Customs.Common.ZA.ZAValidationConstants.InvalidMRNLength);
					}
				}
				else if (!(countryCode == Core.Constants.CountryCodes.Germany && entryNumber.StartsWith("AT")))
				{
					var errors = MRNValidationHelper.CheckMRNFormat(entryNumber, parent.Factory);
					errors.ForEach(error => propertyInfo.AddMessageError(error));
				}
			}

			if (countryCode == Core.Constants.CountryCodes.Germany
					&& entryType == CusEntryNumberTypes.Standard.LocalReferenceNumber
					&& !entryNumberIsEmpty
					&& entryNumber.Length > 22)
			{
				propertyInfo.AddError(Res.GetString("F27BB9EA-B82C-4629-8BE7-E2AC618D60DD", "Local Reference Number has a maximum limit of 22 characters."));
			}
		}

		void ValidateDUEEntryNumber()
		{
			if (!BRCusEntryNumValidationHelper.CheckValidDUEEntryNumberFormat(Parent.EntryNumber))
			{
				Parent.EntryNumberInfo.AddMessageError(Res.GetString("185f491f-2e3e-4f3a-9b21-2b3134788082", "The entered DUE code does not match the required format: <year, 2>BR<Number, 9><CheckDigit, 1>."));
			}
			else if (!BRCusEntryNumValidationHelper.CheckValidDUEEntryNumberCheckDigit(Parent.EntryNumber, out var correctCheckDigit))
			{
				Parent.EntryNumberInfo.AddMessageError(Res.GetString("ad126f88-cd39-402a-8507-9402f41aae66", "The check digit for the entered DUE code is incorrect, it should be {0}.", correctCheckDigit));
			}
		}

		bool ChildCoLoadsWithNoCan
		{
			get
			{
				ForwardingShipment[] shipments = Parent.Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, Parent.Shipment.PK));
				bool result = shipments.Length == 0;
				foreach (ForwardingShipment coLoad in shipments)
				{
					result = coLoad.CustomsEntryNumberType == CusEntryNumberTypes.Australia.CAN && coLoad.CustomsEntryNumber.IsEmpty;
					if (result)
					{
						break;
					}
				}
				return result;
			}
		}

		bool HasCoLoadMasterWithCAN
		{
			get
			{
				bool result = false;
				if (Parent.Shipment.CoLoadMasterShipment != null)
				{
					result = Parent.Shipment.CoLoadMasterShipment.CustomsEntryNumberType == CusEntryNumberTypes.Australia.CAN && !Parent.Shipment.CoLoadMasterShipment.CustomsEntryNumber.IsEmpty;
				}
				return result;
			}
		}

		bool ValidNZEntryNumber
		{
			get
			{
				bool result = true;
				if (Parent.Shipment.Origin != null && Parent.Shipment.Origin.RL_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
				{
					result = Parent.EntryNumber.IsNumbersOnlyOrEmpty;
				}

				return result;
			}
		}

		bool IsValidITNNumber(ZString itnNumber) => itnNumber.Length == 15 && itnNumber.StartsWith("X") && itnNumber.SubstringSafe(1).IsNumbersOnlyOrEmpty && ZDateTime.TryParseExact(itnNumber.SubstringSafe(1, 8), out _, "yyyyMMdd");

		bool IsNoEEI(ZString value) => value.Replace(" ", string.Empty).StartsWith("NOEEI");
	}
}
