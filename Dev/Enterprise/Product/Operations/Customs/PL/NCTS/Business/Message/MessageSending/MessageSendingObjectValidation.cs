using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class MessageSendingObjectValidation : NctsHeaderMessageSendingObjectValidation
{
	public MessageSendingObjectValidation(MessageSendingObject parent) : base(parent)
	{ }

	public new MessageSendingObject Parent => (MessageSendingObject)base.Parent;

	protected new ValidationRuleConfiguration ValidationRuleConfiguration => (ValidationRuleConfiguration)base.ValidationRuleConfiguration;

	public override Type AutoValidationType => typeof(MessageSendingObjectValidation);

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateAmendmentType();
		ValidateGoodsLocation();
		ValidatePresentationDateTime();
		ValidateTirPageNumber();
		ValidateTirUnloadingNumber();
		ValidateTransportIdentification();
		ValidatePlaceOfLoading();
	}

	public void ValidateAmendmentType()
	{
		ValidateCalculatedProperty(Parent.AmendmentTypeInfo);
	}

	public void ValidateGoodsLocation()
	{
		ValidateCalculatedProperty(Parent.GoodsLocationInfo);
	}

	public void ValidatePresentationDateTime()
	{
		ValidateCalculatedProperty(Parent.PresentationDateTimeInfo);
	}

	public void ValidateTirPageNumber()
	{
		ValidateCalculatedProperty(Parent.TirPageNumberInfo);
	}

	public void ValidateTirUnloadingNumber()
	{
		ValidateCalculatedProperty(Parent.TirUnloadingNumberInfo);
	}

	public void ValidateTransportIdentification()
	{
		ValidateCalculatedProperty(Parent.TransportIdentificationInfo);
	}

	public void ValidatePlaceOfLoading()
	{
		ValidateCalculatedProperty(Parent.PlaceOfLoadingInfo);
	}

	protected override void CheckJustification()
	{
		var parent = Parent;

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.JustificationInfo, parent.MessageTypeInfo, (ZString)DepartureMessageSendingObjectTypeList.Codes.INV);
	}

	protected void CheckAmendmentType()
	{
		var parent = Parent;
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.AmendmentTypeInfo, parent.MessageTypeInfo, (ZString)DepartureMessageSendingObjectTypeList.Codes.AMD);
	}

	protected void CheckGoodsLocation()
	{
		var parent = Parent;

		if (ValidationRuleConfiguration.IsRuleNR0032Active && PRNTypeValidationWithSubmittedType015() && parent.GoodsLocation.IsEmpty)
		{
			parent.GoodsLocationInfo.AddMessageError(Res.GetString("C1F58A34-946F-4F9A-AA93-5EF023B8E153", "[NR0032] You have not entered Location Of Goods."));
		}
	}

	protected void CheckPresentationDateTime()
	{
		var parent = Parent;
		var presentationDateTime = parent.PresentationDateTime;
		var info = parent.PresentationDateTimeInfo;
		if (parent.MessageType == DepartureMessageSendingObjectTypeList.Codes.AMD
			&& !presentationDateTime.IsEmpty
			&& presentationDateTime < ZDateTime.Now)
		{
			info.AddMessageError(Res.GetString("A6311E6F-71BB-41C9-BCEB-E5F94E28B331", "{0} cannot be in the past", info.HumanReadableName));
		}
	}

	protected void CheckPlaceOfLoading()
	{
		var parent = Parent;

		if (parent.MessageType == DepartureMessageSendingObjectTypeList.Codes.PRN)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.PlaceOfLoadingInfo, propertyDescription: parent.PlaceOfLoadingInfo.HumanReadableName, messagePrefix: "[C0404] ");
		}
	}

	protected override void CheckDepartureOfficeOfEnquiry()
	{
		base.CheckDepartureOfficeOfEnquiry();
		var parent = Parent;

		if (!parent.DepartureOfficeOfEnquiryInfo.ReadOnly)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.DepartureOfficeOfEnquiryInfo);
		}
	}

	protected void CheckTirPageNumber()
	{
		CheckTirField(value: Parent.TirPageNumber,
			info: Parent.TirPageNumberInfo);
	}

	protected void CheckTirUnloadingNumber()
	{
		CheckTirField(value: Parent.TirUnloadingNumber,
			info: Parent.TirUnloadingNumberInfo);
	}

	protected void CheckTirField(ZString value, ZPropertyInfo info)
	{
		var parent = Parent;
		var isArrivalMovement = parent.NctsHeader.IsArrivalMovement;
		var messageType = parent.MessageType;
		var arrivalMovementHeader = parent.NctsHeader.ArrivalMovementHeader;

		if (isArrivalMovement
			&& messageType == ArrivalMessageSendingObjectTypeList.Codes.URM)
		{
			if (!arrivalMovementHeader.BM_UnloadingCompleted)
			{
				ListValidation.MessageErrorIfInvalidCode(info);
				return;
			}

			if (arrivalMovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.TIR
				&& string.IsNullOrEmpty(value))
			{
				info.AddMessageError(Res.GetString("712BD850-CEFA-436D-834A-925BA4909ED8", "[RP22] You have not entered {0}.", info.HumanReadableName));
			}
			else if (string.IsNullOrEmpty(arrivalMovementHeader.BM_InBondEntryType)
				&& string.IsNullOrEmpty(value))
			{
				info.AddWarning(Res.GetString("7D7C5C72-89DB-4A85-A2DB-0E0F22299789", "[RP22] – for TIR declaration type, it’s required to enter {0}.", info.HumanReadableName));
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(info);
			}
		}
	}

	protected void CheckTransportIdentification()
	{
		var parent = Parent;
		var transportIdentification = parent.TransportIdentification;

		if (ValidationRuleConfiguration.IsRuleNR0033Active && PRNTypeValidationWithSubmittedType015() && ((string)transportIdentification).Any(char.IsLower))
		{
			parent.TransportIdentificationInfo.AddMessageError(Res.GetString("2EEED07B-ABEC-4815-A28B-5A8D2082CD4F", "[NR0033] Data must be in Capital Letter."));
		}
	}

	bool PRNTypeValidationWithSubmittedType015()
	{
		var parent = Parent;
		var nctsHeader = parent.NctsHeader;
		return parent.MessageType == DepartureMessageSendingObjectTypeList.Codes.PRN
				&& nctsHeader.IsDepartureMovement
				&& nctsHeader.MovementHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D
				&& nctsHeader.Messages.Cast<EDIMessage>().Any(m => m.EM_MessageSubType == Constants.MessageSubTypeCodes.IE015);
	}
}
