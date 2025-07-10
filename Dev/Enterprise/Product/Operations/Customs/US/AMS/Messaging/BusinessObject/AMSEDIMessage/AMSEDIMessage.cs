using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSEDIMessage : CBPEDIMessage
	{
		public AMSEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public readonly static new TypeDecider TypeDecider = new EDIMessageTypeDecider();
		public const string AMSMessageNumberPlaceHolder = "<MSG PLACEHOLDER>"; //17 (9 System Identifier + 8 Numberic) characters long
		public const string InBondNumberPlaceHolder = "<ITNOPLC>";//9 characters long

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				var messageAttachee = EM_LinkedObject as IMessageAttacheeInHeader;

				if (messageAttachee != null)
				{
					var header = Factory.Load<IMessageActionHeader>(messageAttachee.HeaderPK);
					if (header != null)
					{
						header.RebuildMessageAttacheesRelatedRecords(messageAttachee);
					}
				}
			}
		}

		protected override string GetMessageReferenceNumber()
		{
			var messageAttachee = EM_LinkedObject as IManifestMessageAttachee;
			GlbCompany company = null;
			if (messageAttachee != null)
			{
				var branch = messageAttachee.Branch;
				if (branch != null)
				{
					company = branch.Company;
				}
			}

			if (company == null)
			{
				company = Company ?? Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			}

			string sender = company.LicenceKeyIdentifier;
			var receiver = CBPEDIInterchange.InterchangePartyIDs.AMSMailbox;
			return sender + Env.NumberFountains.EDIFACTNumberFountain("M", sender, receiver).GetNextFormatted(Factory);
		}

		protected override bool AllowExceeding9999Limit
		{
			get { return false; }
		}

		protected override BlockControlGenerator GetMessageBlock()
		{
			BlockControlGenerator result = null;

			if (IsTransmitMessage)
			{
				result = new AMSInputBlockControlGenerator(GetMessageBlockApplicationCode());
			}
			else
			{
				result = new AMSOutputBlockControlGenerator(GetMessageBlockApplicationCode());
			}

			var messageText = EM_MessageText;
			if (!messageText.IsEmpty)
			{
				result.Deserialise(BlockPadder.Pad(messageText));
			}

			return result;
		}

		bool IsACE
		{
			get { return EM_MessageOwner == Constants.ACE; }
		}

		protected override string GetMessageBlockApplicationCodeCore()
		{
			var result = base.GetMessageBlockApplicationCodeCore();
			if (IsACE)
			{
				result = Constants.ACE;
			}
			else if (!IsTransmitMessage)
			{
				var originalMessage = OriginalMessage;
				if (originalMessage != null)
				{
					result = originalMessage.GetMessageBlockApplicationCode();
				}
			}
			return result;
		}

		protected override string MessageNumberPlaceHolderOverride
		{
			get { return AMSMessageNumberPlaceHolder; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.AMS;
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			var result = new List<Type>();
			result.Add(ObjectFactory.GetType<Integration.Customs.US.USAMS.ICusInBondMoveHeader>());
			return result;
		}

		protected override ZArchitecture.Core.CodeDescriptionPairList MessageSubTypeList
		{
			get { return Factory.GetCachedValue<AMSMessageSubTypeList>(); }
		}
	}
}
