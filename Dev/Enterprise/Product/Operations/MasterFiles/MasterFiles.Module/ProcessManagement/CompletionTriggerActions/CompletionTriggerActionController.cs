using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class CompletionTriggerActionController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException(NotImplementedExceptionMessage);
		}

		public override ControllerID ID => ControllerIDs.CompletionTriggerAction;

		public override ModuleIdentifier ModuleID => ModuleIDs.CompletionTriggerAction;

		public override Type TypeOfTopLevelBusinessObject => typeof(ProcessTaskNotification);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override SecurityCheckpoint CheckPointForView
			=> throw new CompletionTriggerActionFormNotSupportedException(NotImplementedExceptionMessage);

		protected override SecurityCheckpoint CheckPointForNew
			=> throw new CompletionTriggerActionFormNotSupportedException(NotImplementedExceptionMessage);

		protected override SecurityCheckpoint CheckPointForEdit
			=> throw new CompletionTriggerActionFormNotSupportedException(NotImplementedExceptionMessage);

		protected override SecurityCheckpoint CheckPointForDelete
			=> throw new CompletionTriggerActionFormNotSupportedException(NotImplementedExceptionMessage);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception message string")]
		const string NotImplementedExceptionMessage = "We may implement this in the future to open the action's trigger's/milestone's job form, but this is not currently supported.";

		[Serializable]
		class CompletionTriggerActionFormNotSupportedException : ModuleFeatureNotSupportedException
		{
			public CompletionTriggerActionFormNotSupportedException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected CompletionTriggerActionFormNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}
	}
}
