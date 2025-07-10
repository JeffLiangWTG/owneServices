
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyPackLocation : CFSPackLocation
	{
		public TallyPackLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Business Objects

		public new TallyPackLine PackLine
		{
			get { return (TallyPackLine)GetParentPackLine(); }
		}

		protected override PackLine GetParentPackLine()
		{
			return Factory.Load<TallyPackLine>(JQ_JL);
		}

		#endregion

		#region Property Overrides

		public override ZInt JQ_NoPackages
		{
			get
			{
				return base.JQ_NoPackages;
			}
			set
			{
				base.JQ_NoPackages = value;

				if (PackLine != null)
				{
					PackLine.MarkAsNeedingValidation();
				}
			}
		}

		#endregion
	}
}
