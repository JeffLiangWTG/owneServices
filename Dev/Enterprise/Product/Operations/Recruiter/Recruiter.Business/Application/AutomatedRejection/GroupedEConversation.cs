using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class GroupedEConversation : NonPersistentBusinessObject, IConversation
	{
		readonly BusinessObject parent;
		readonly string defaultMessage;

		public ActiveBusinessObjectCollection<JobConversation> Conversations { get; }
		public JobConversation RootConversation { get; }

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GroupedEConversation(BusinessObject parent, string defaultMessage)
			: base(parent.Factory)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
			this.defaultMessage = defaultMessage;

			Conversations = new ActiveBusinessObjectCollection<JobConversation>(Factory, new ZQuery(JobConversationSchema.JCC_ParentID, parent.PK));
			RootConversation = FindOrCreateRoot();

			RegisterEditableChildObject(Conversations);
			parent.RegisterEditableChildObject(this);
		}

		JobConversation FindOrCreateRoot()
		{
			var convo = Conversations.FirstOrDefault(IsRootConversation);
			if (convo == null)
			{
				var pk = JobConversation.CreateAndSaveNewConversationOnNewFactory(parent, defaultMessage);
				convo = parent.Factory.Load<JobConversation>(pk);
			}

			return convo;
		}

		bool IsRootConversation(JobConversation convo) => Equals(convo.Messages.LastOrDefault()?.JCM_Body, defaultMessage);

		public bool IsEmpty => Conversations.Any(c => c.Messages.Any());
		public bool AnyLocalMessageContains(string text) => Conversations.Any(c => c.AnyLocalMessageContains(text));
		public IList<IConversationMessage> GetTimeOrderedMessages() => Conversations.SelectMany(c => c.Messages).OrderByDescending(m => m.JCM_PostedTimeUtc).ToList<IConversationMessage>();
	}
}
