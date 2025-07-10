using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class JobConsolAWBSpecialHandling : AutoJobConsolAWBSpecialHandling
	{
		protected JobConsolAWBSpecialHandling(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly JobConsolAWBSpecialHandlingTypeDecider TypeDecider = new JobConsolAWBSpecialHandlingTypeDecider();

		protected override ZString HumanReadableNameCore => Res.GetString("037655fc-d3db-4302-9b78-4704100e07ed", "Special Handling");

		public ForwardingConsol Consol => Factory.Load<ForwardingConsol>(JKH_JK_Consol);

		public override ZString JKH_Code
		{
			get => base.JKH_Code;
			set
			{
				if (JKH_Code != value)
				{
					base.JKH_Code = value;
					Consol?.MarkAsNeedingValidation();
				}
			}
		}
	}
}
