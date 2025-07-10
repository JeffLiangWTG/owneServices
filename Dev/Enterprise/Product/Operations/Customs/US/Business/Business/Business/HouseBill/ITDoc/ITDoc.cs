using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ITDoc : AutoITDoc
	{
		public ITDoc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "ITDoc"; }
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (US_7512OpenArea.IsEmpty)
			{
				Delete();
			}
		}

		#endregion
	}
}
