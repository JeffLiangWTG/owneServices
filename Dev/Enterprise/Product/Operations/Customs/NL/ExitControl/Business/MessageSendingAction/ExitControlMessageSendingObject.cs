using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.NL.ExitControl.Business;

public class ExitControlMessageSendingObject : EU.ExitControl.Business.ExitControlMessageSendingObject
{
	public ExitControlMessageSendingObject(CusExitReport messagingObject) : base(messagingObject)
	{
	}

	public static class Schema
	{
		public const string EntryType = nameof(ExitControlMessageSendingObject.EntryType);
	}

	[List(nameof(Lookups) + "." + nameof(ExitControlMessageSendingObjectLookups.EntryTypeList))]
	[ResourceStringData("Enterprise.Customs.NL.ExitControl.Business.ExitControlMessageSendingObject|EntryType", Caption = "Entry Type")]
	public ZString EntryType
	{
		get => entryType;
		set
		{
			SetNonPersistentPropertyValue(EntryTypeInfo, ref entryType, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateEntryType();
			}
		}
	}

	ZString entryType;

	public ZPropertyInfo EntryTypeInfo => GetZPropertyInfo(nameof(Schema.EntryType));

	#region Lookups

	public ExitControlMessageSendingObjectLookups Lookups => lookups ?? (lookups = GetNewLookups());
	ExitControlMessageSendingObjectLookups lookups;

	protected ExitControlMessageSendingObjectLookups GetNewLookups() => new ExitControlMessageSendingObjectLookups(this);

	#endregion

	#region Validation

	public ExitControlMessageSendingActionValidation Validation => new ExitControlMessageSendingActionValidation(this);

	#endregion
}
