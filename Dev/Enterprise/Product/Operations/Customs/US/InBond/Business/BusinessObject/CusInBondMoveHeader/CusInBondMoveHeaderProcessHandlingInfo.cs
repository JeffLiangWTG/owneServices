using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveHeaderProcessHandlingInfo : ProcessHandlingInfo
	{
		public CusInBondMoveHeaderProcessHandlingInfo(CusInBondMoveHeader moveHeader)
			: base(moveHeader)
		{ }

		CusInBondMoveHeader MoveHeader
		{
			get { return (CusInBondMoveHeader)LogParent; }
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return PopulateCascadingTargets(MoveHeader.Header, logBeingAdded);
		}

		internal static IEnumerable<CascadingLink> PopulateCascadingTargets(CusInBondHeader header, IStmALog logBeingAdded)
		{
			var headerLink = PopulateCascadingTarget(header, logBeingAdded);
			if (headerLink != null)
			{
				yield return headerLink;
			}
			var headerParent = header != null ? header.Parent : null;
			var headerParentLink = PopulateCascadingTarget((IWorkflowProvider)headerParent, logBeingAdded);
			if (headerParentLink != null)
			{
				yield return headerParentLink;
			}
		}

		static CascadingLink PopulateCascadingTarget(IWorkflowProvider provider, IStmALog logBeingAdded)
		{
			Func<ProcessTask, bool> matchedProcessTask = x => x.P9_RespondToCascadedEvents && x.P9_SE_NKMilestoneEvent == logBeingAdded.SL_SE_NKEvent;
			if (provider != null && provider.WorkflowItems.Cast<ProcessTask>().Any(matchedProcessTask))
			{
				return new CascadingLink()
				{
					Parent = (IStmALogParent)provider,
					Triggers = provider.WorkflowItems.Where(matchedProcessTask).ToArray()
				};
			}
			return null;
		}
	}
}
