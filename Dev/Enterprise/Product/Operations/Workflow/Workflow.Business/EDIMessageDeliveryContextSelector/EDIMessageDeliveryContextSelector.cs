using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageDeliveryContextSelector : AutoEDIMessageDeliveryContextSelector, IEDIMessageDeliveryContextSelector
	{
		public EDIMessageDeliveryContextSelector(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString HumanReadableShortcutNameCore => Res.GetString("EDIMessageDeliveryContextSelector|HumanReadableName", "[{0}] [{1}]", ECS_ProcessType, ECS_Code);

		#endregion

		#region Properties

		[List("Lookups.ProcessTypes")]
		public override ZString ECS_ProcessType
		{
			get => base.ECS_ProcessType;
			set => base.ECS_ProcessType = value;
		}

		#endregion

		#region Collections

		[ChildEditable(true)]
		public EDIMessageDeliveryContextLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new EDIMessageDeliveryContextLineCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		EDIMessageDeliveryContextLineCollection lines;

		#endregion

		#region Delete

		public override void Delete()
		{
			lines?.DeleteAll();
			base.Delete();
		}

		#endregion
	}
}
