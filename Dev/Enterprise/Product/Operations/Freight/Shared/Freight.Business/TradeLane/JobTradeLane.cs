using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business
{
	[System.Diagnostics.DebuggerDisplay("Trade Lane={EJ_Code}")]
	public class JobTradeLane : AutoJobTradeLane, IJobTradeLane
	{
		public JobTradeLane(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region EJ_Location1

		[List("Lookups.Locations")]
		public override ZString EJ_Location1
		{
			get { return base.EJ_Location1; }
			set { base.EJ_Location1 = value; }
		}

		#endregion

		#region EJ_Location2

		[List("Lookups.Locations")]
		public override ZString EJ_Location2
		{
			get { return base.EJ_Location2; }
			set { base.EJ_Location2 = value; }
		}

		#endregion

		#region EJ_Direction

		[List("Lookups.DirectionTypes")]
		public override ZString EJ_Direction
		{
			get { return base.EJ_Direction; }
			set { base.EJ_Direction = value; }
		}

		#endregion

		#endregion

		#region BusinessObject Overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				string result = Res.GetString("2587403e-fb68-48b2-a261-27ff2a539205", "Trade Lane");
				if (!EJ_Code.IsEmpty)
				{
					result += " " + EJ_Code;
				}
				return result;
			}
		}

		#endregion

	}
}
