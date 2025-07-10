using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class EIDOMessage : EDIMessage
	{
		public static string UsKeyPart { get { return "EDI"; } }
		public const string ThemKeyPart = "OneStop";

		public EIDOMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region New

		public static EIDOMessage New(IEDIMessageCollectionProvider parent, EIDOMessageFunction fuction, string messageText)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			EIDOMessage message = (EIDOMessage)parent.Messages.AddNew(typeof(EIDOMessage));
			message.EM_MessageText = messageText;
			message.EM_MessageSubType = SubTypeFromFunction(fuction);

			return message;
		}

		#endregion

		#region SetEventToAddOnSaving

		public void SetEventToAddOnSaving(Type objectType, Event addedEvent, params KeyValuePair<string, string>[] eventParameters)
		{
			SetEventToAddOnSaving(objectType, addedEvent, string.Empty, eventParameters);
		}

		public void SetEventToAddOnSaving(Type objectType, Event addedEvent, ZString eventReference, params KeyValuePair<string, string>[] eventParameters)
		{
			linkedObjectType = objectType;
			reference = eventReference;
			eventToAdd = addedEvent;
			parameters = eventParameters;
		}

		#endregion

		#region Overrides

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", UsKeyPart, ThemKeyPart).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_ApplicationCode = ApplicationCodes.EIDO;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			EM_MessageType = ApplicationCodes.EIDO;
			EM_Status = Status.Queued;
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new EIDOMessageTypes(); }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (eventToAdd != null && logAddedOnSaving == null)
			{
				BusinessObject linkedObject = Factory.Load(linkedObjectType, EM_LinkUniqueID);
				logAddedOnSaving = linkedObject.GetLogs().AddNew(eventToAdd, reference, parameters);
			}

			isLicenceConsumptionLogged = IsInDatabase;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (logAddedOnSaving != null)
			{
				if (!saveSucceeded)
				{
					logAddedOnSaving.Delete();
				}
				else
				{
					eventToAdd = null;
					reference = "";
				}
				logAddedOnSaving = null;
			}

			if (saveSucceeded && !isLicenceConsumptionLogged && EM_MessageSubType == EIDOMessageTypes.Codes.Original)
			{
				ObjectFactory.Get<ILicenceConsumptionLogCreator>().CreateLog(Env.Licence.ShippingManagerEIDOMessagingPerTransaction, true);
			}
		}

		#endregion

		#region Implementation

		static string SubTypeFromFunction(EIDOMessageFunction function)
		{
			switch (function)
			{
				case EIDOMessageFunction.Original:
					return EIDOMessageTypes.Codes.Original;
				case EIDOMessageFunction.Cancelation:
					return EIDOMessageTypes.Codes.Cancellation;
				default:
					throw new ArgumentOutOfRangeException(nameof(function), function, "dont know how to handle this message function");
			}
		}

		#endregion

		Type linkedObjectType;
		Event eventToAdd;
		ZString reference;
		StmALog logAddedOnSaving;
		bool isLicenceConsumptionLogged;
		KeyValuePair<string, string>[] parameters;
	}
}
