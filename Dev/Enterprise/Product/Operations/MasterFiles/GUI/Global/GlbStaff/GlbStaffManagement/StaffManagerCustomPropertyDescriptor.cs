using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI
{
	internal class StaffManagerCustomPropertyDescriptor : ZCustomPropertyDescriptor
	{
		public StaffManagerCustomPropertyDescriptor(ICustomProperty property) : base(property.Identifier, property.Info.Type) { }

		protected override object GetValueCore(object component)
		{
			var staff = component as GlbStaff;
			if (staff != null)
			{
				var managerNames = (ZString)string.Join(", ", staff.Managers.Where(x => x.GSM_ManagerType == Name && x.IsCurrentManager).Select(y => y.Manager.GS_FullName).OrderBy(x => x));
				return Converter.ConvertFrom(managerNames);
			}
			return null;
		}

		protected override void SetValueCore(object component, object value)
		{
			throw new NotSupportedException(FormattableString.Invariant($"StaffManagerCustomPropertyDescriptor for property '{Name}' is read only"));
		}

		public override bool IsReadOnly
		{
			get { return true; }
		}
	}
}
