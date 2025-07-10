using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	public class MessageChooserNonPersistent : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string MessagesToSend = "MessagesToSend";
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "identifier")]
		public MessageChooserNonPersistent(SingleMessageManager[] managers, ZString question) : this(managers, question, "Send")
		{
		}

		public MessageChooserNonPersistent(SingleMessageManager[] managers, ZString question, ZString action)
		{
			this.Managers = managers;
			this.Question = question;
			this.Action = action;
			foreach (SingleMessageManager manager in managers)
			{
				MessagesToSend.Add(new ZBoolDescriptionPair(manager.MessageFriendlyName, false));
			}
		}

		#region MessagesToSend

		ZBoolDescriptionPairList messagesToSend;
		public ZBoolDescriptionPairList MessagesToSend
		{
			get
			{
				if (messagesToSend == null)
				{
					messagesToSend = new ZBoolDescriptionPairList();
				}
				return messagesToSend;
			}
		}

		//		public ZPropertyInfo MessagesToSendInfo
		//		{
		//			get
		//			{
		//				return GetZPropertyInfo(Schema.MessagesToSend);
		//			}
		//		}

		#endregion

		public virtual SingleMessageManager[] SelectedManagers
		{
			get
			{
				ArrayList result = new ArrayList();
				for (int i = 0; i < Managers.Length; i++)
				{
					if (MessagesToSend[i].Value)
					{
						result.Add(Managers[i]);
					}
				}
				return (SingleMessageManager[])result.ToArray(typeof(SingleMessageManager));
			}
		}

		public virtual void SelectAll()
		{
			foreach (ZBoolDescriptionPair pair in MessagesToSend)
			{
				pair.Value = true;
			}
		}

		public virtual void DelselectAll()
		{
			foreach (ZBoolDescriptionPair pair in MessagesToSend)
			{
				pair.Value = false;
			}
		}

		public virtual void SelectDefaultToSendManagers(ShouldManagerDefaultToSendDelegate selectManagerDelegate)
		{
			for (int i = 0; i < Managers.Length; i++)
			{
				MessagesToSend[i].Value = selectManagerDelegate(Managers[i]);
			}
		}

		public readonly ZString Question;
		public readonly ZString Action;
		public SingleMessageManager[] Managers;
		public delegate ZBool ShouldManagerDefaultToSendDelegate(SingleMessageManager manager);
	}
}
