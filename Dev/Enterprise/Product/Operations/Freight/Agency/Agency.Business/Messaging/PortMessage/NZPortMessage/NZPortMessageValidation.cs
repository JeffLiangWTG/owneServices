using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class NZPortMessageValidation : PortMessageValidation
	{
		public NZPortMessageValidation(NZPortMessage parent)
			: base(parent) { }

		protected override void CheckCTO()
		{
			base.CheckCTO();

			if (Parent.CTO.IsEmpty)
			{
				Parent.CTOInfo.AddError(Res.GetString("cf002523-21aa-4da6-a667-3de47ffde9a1", "There is no CTO registered for this port on the schedule."));
			}
		}

		protected override void CheckPrincipalPK()
		{
			base.CheckPrincipalPK();

			MandatoryValidation.CheckEntered(Parent.PrincipalPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.PrincipalPKInfo);
		}

		protected override void CheckMessageType()
		{
			var lastMessageSent = Parent.LastMessageSent;
			var message = ZString.Empty;

			switch (Parent.MessageType)
			{
				case PortMessageTypeList.Codes.Cancellation:
					if (lastMessageSent == ZString.Empty)
					{
						message = Res.GetString("64910a09-acd3-4ff6-8080-624bee3fadf0", "No messages have been sent for this port/direction, so there is nothing to cancel.");
					}
					else if (lastMessageSent == PortMessageTypeList.Codes.Cancellation)
					{
						message = Res.GetString("71c95574-ed03-4235-8a93-716f397ef43d", "A cancellation has already been sent for this port/direction.");
					}
					break;

				case PortMessageTypeList.Codes.Replace:
					if (lastMessageSent == ZString.Empty)
					{
						message = Res.GetString("ceade5cc-07ca-4645-8aec-e8d81e5f5554", "No messages have been sent for this port/direction, so there is nothing to replace.");
					}
					else if (lastMessageSent == PortMessageTypeList.Codes.Cancellation)
					{
						message = Res.GetString("769cd673-3977-4b7b-af90-7cfd6180c149", "The last message sent for this port/direction was a cancellation, so there is nothing to replace.");
					}
					break;

				case PortMessageTypeList.Codes.Original:
					if (lastMessageSent != ZString.Empty && lastMessageSent != PortMessageTypeList.Codes.Cancellation)
					{
						message = Res.GetString("bb18d4f1-b801-4e00-82da-4da61838cd10", "An uncanceled message has already been sent for this port/direction.");
					}
					break;
			}

			if (!message.IsEmpty)
			{
				Parent.MessageTypeInfo.AddWarning(message);
			}
		}

		protected new NZPortMessage Parent
		{
			get
			{
				return (NZPortMessage)base.Parent;
			}
		}
	}
}
