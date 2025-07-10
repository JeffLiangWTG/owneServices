using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class DeclarationMessageSendingObject : AdditionalDocumentMessageSendingObject
	{
		public DeclarationMessageSendingObject(CusEntryHeader header)
			: base(header)
		{
		}

		protected override void SetMessageSendingObjectDefaultValues()
		{
			base.SetMessageSendingObjectDefaultValues();
			if (MessageStatus.IsEmpty)
			{
				if (OnlyHasDeclarationEntryNumber || !Header.HasBeenLodgedAtCustoms)
				{
					Action = ActionCodeList.Codes.Create;
				}
				else
				{
					Action = ActionCodeList.Codes.Update;
				}
			}
		}

		protected override void SetShouldSendtDefaultValue()
		{
			if (MessageStatus.IsEmpty)
			{
				ShouldSend = true;
			}
		}

		#region Lookup Lists
		public override CodeDescriptionPairList ActionList
		{
			get
			{
				var createFlag = EntryStatus.IsEmpty;
				var onlyHasDeclarationEntryNumber = OnlyHasDeclarationEntryNumber;
				return Factory.GetCachedValue(string.Concat(nameof(DeclarationMessageSendingObject), nameof(ActionList), createFlag, onlyHasDeclarationEntryNumber), () =>
				 {
					 var list = new ActionCodeList();
					 list.RemoveCode(ActionCodeList.Codes.Delete);
					 if (onlyHasDeclarationEntryNumber)
					 {
						 list.RemoveCode(ActionCodeList.Codes.Update);
					 }
					 return list;
				 });
			}
		}
		#endregion

		public override ZString GetMessageOwner()
		{
			return ZString.Empty;
		}

		bool OnlyHasDeclarationEntryNumber => Header.EntryNumber.IsEmpty && !Header.DeclarationNumber.IsEmpty;

		public override ZString FriendlyNameForMessageManager => Res.GetString("F7370575-7595-4CD4-BDFF-42FA5752FD08", "Send Customs Declaration");

		protected virtual IAdditionalInformation GetAdditionalInformation()
		{
			var sumCopy = entryInstruction.DeclarationDuplicates.Cast<DeclarationDuplicate>().Sum(c => c.Copy);
			return sumCopy > 0 ? new AdditionalInformationWrapper(sumCopy) : null;
		}

		protected IEnumerable<ZString> GetGovernmentProcedureDescriptions()
		{
			var result = entryInstruction.TW_TradersRemarks;

			if (result.Length > 256)
			{
				var part1 = result.Substring(0, 256).TrimEndIncludingWhiteSpace('\r');
				yield return part1;
				var part2 = result.Substring(part1.Length).TrimStart(new char[] { '\r', '\n' }).SubstringSafe(0, 256).TrimEndIncludingWhiteSpace('\r');
				if (!part2.IsEmpty)
				{
					yield return part2;
				}
			}
			else
			{
				yield return result;
			}
		}
	}
}
