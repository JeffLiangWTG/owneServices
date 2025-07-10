using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class ImportRelatedActivityPromptUserDeciderFactory : ImportRelatedActivityDeciderFactory
	{
		public ImportRelatedActivityPromptUserDeciderFactory(KForm parentForm, string reason)
		{
			this.parentForm = parentForm;
			this.reason = reason;
		}

		readonly KForm parentForm;

		public override string Reason
		{
			get { return reason; }
		}
		readonly string reason;

		protected override T GetIfAvailableCore<T>()
		{
			var attribute = TypeDescriptor.GetAttributes(typeof(T))[typeof(ImportRelatedActivityPromptUserDeciderAttribute)] as ImportRelatedActivityPromptUserDeciderAttribute;
			if (attribute != null)
			{
				var deciderType = Type.GetType(attribute.TypeName);
				var result = (T)Activator.CreateInstance(deciderType, new object[] { parentForm });
				result.DecideReason = Reason;
				return result;
			}

			return null;
		}
	}

	public class ImportRelatedActivityPromptUserDecider : ImportRelatedActivityDecider
	{
		public ImportRelatedActivityPromptUserDecider(KForm parentForm)
		{
			ParentForm = parentForm;
		}

		public readonly KForm ParentForm;
	}
}
