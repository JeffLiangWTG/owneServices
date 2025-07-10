using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using static Enterprise.MasterFiles.Business.TriggerActionCommunicationModeSubstitutor;

namespace Enterprise.MasterFiles.Business
{
	public sealed class WorkflowTriggerNotification : IMessageProcessor
	{
		public ExtraDataSubstitutionDelegate ExtraDataSubstitution;
		public ExtraDataSubstitutionDelegate ExtraPostMacroDataSubstitution;
		public WorkflowTriggerNotification(Lazy<MessageProcessorCommunicationModesResult> modes, ProcessTaskNotification action, BusinessObject parent, Lazy<IStmALog> eventProvider)
		{
			this.lazyModes = modes;
			this.Action = action;
			this.Parent = parent;
			this.eventProvider = eventProvider;
		}

		public static bool IsUserEmailMacro(ZString message)
		{
			return getHasMacroRegex.IsMatch(message);
		}

		#region Properties

		ZString JobNumber
		{
			get
			{
				if (jobNumber.IsEmpty)
				{
					jobNumber = TriggerActionCommunicationModeSubstitutor.GetJobNumber(Parent);
				}

				return jobNumber;
			}
		}

		ZString jobNumber;

		#endregion

		#region IProcessor Members

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			var modes = Modes.CommunicationModes;
			if (modes.Count > 0 && modes.All(m => (m.EK_MessagePurpose != Action.PQ_MessagePurpose) && (!string.IsNullOrWhiteSpace(m.EK_MessagePurpose))))
			{
				ErrorReporter.ReportOnce("WorkflowTriggerNotification.Process", string.Format(CultureInfo.InvariantCulture, "Trigger/Milestone action message purpose [{0}] is not part of EDICommunicationsMode's message purpose of [{1}]. There are {2} modes currently present.", Action.PQ_MessagePurpose, string.Join(", ", modes.Select(m => m.EK_MessagePurpose)), modes.Count));
			}

			var message = Substitute(CommunicationModeSubstitutorProperty.EmailText, Action.PQ_EmailTextFallbackToTemplate);
			var context = ConvertToContext(Action);
			context.Notifications = notifications;
			using (var stream = GetStream(message))
			{
				var delivery = new EDIMessageDelivery(JobNumber);
				delivery.ExtraDataSubstitution = Substitute;
				delivery.Deliver(context, modes, new DeliveryStreamWrapperUXML(stream, context.ParentInfo));
			}
		}

		internal DeliveryContext ConvertToContext(ProcessTaskNotification action)
		{
			var context = new DeliveryContext(action.Factory);
			var parent = Parent;
			context.ParentInfo = EntityInfo.New(parent);
			if (parent != null)
			{
				context.ParentHumanReadableName = parent.HumanReadableName;
			}
			context.ActionDescription = action.Description;
			return context;
		}

		public ZString Substitute(CommunicationModeSubstitutorProperty property, ZString value) => TriggerActionCommunicationModeSubstitutor.Substitute(Action, Parent, eventProvider?.Value, property, value, ExtraDataSubstitution, ExtraPostMacroDataSubstitution);

		SubStreamableStream GetStream(ZString msg)
		{
			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(msg));
			stream.Position = 0;
			return (SubStreamableStream)stream;
		}

		#endregion

		IMessageProcessorCommunicationModesResult IMessageProcessor.GetDestinations() => Modes;

		readonly Lazy<MessageProcessorCommunicationModesResult> lazyModes;
		public MessageProcessorCommunicationModesResult Modes => lazyModes.Value;
		public readonly ProcessTaskNotification Action;
		public readonly BusinessObject Parent;
		readonly Lazy<IStmALog> eventProvider;
		static readonly Regex getHasMacroRegex = new Regex(@".*\(\*.+\*\).*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
