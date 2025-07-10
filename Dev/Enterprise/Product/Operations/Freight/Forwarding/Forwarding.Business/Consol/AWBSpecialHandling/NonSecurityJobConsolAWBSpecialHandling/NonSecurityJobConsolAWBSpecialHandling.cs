using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	[DependentBusinessObject(typeof(ForwardingConsol), "AWBSpecialHandlingItems")]
	public class NonSecurityJobConsolAWBSpecialHandling : JobConsolAWBSpecialHandling
	{
		public NonSecurityJobConsolAWBSpecialHandling(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.SpecialHandlingCodeDescriptionList")]
		public override ZString JKH_Code
		{
			get => base.JKH_Code;
			set => base.JKH_Code = value;
		}

		public new NonSecurityJobConsolAWBSpecialHandlingLookups Lookups => (NonSecurityJobConsolAWBSpecialHandlingLookups)base.Lookups;

		protected override JobConsolAWBSpecialHandlingLookups GetNewLookups()
		{
			return new NonSecurityJobConsolAWBSpecialHandlingLookups(this);
		}

		public new NonSecurityJobConsolAWBSpecialHandlingValidation Validation => (NonSecurityJobConsolAWBSpecialHandlingValidation)base.Validation;

		protected override JobConsolAWBSpecialHandlingValidation GetNewValidation()
		{
			return new NonSecurityJobConsolAWBSpecialHandlingValidation(this);
		}

		public ZString SpecialHandlingDescription => Lookups.SpecialHandlingCodeDescriptionList[JKH_Code, System.StringComparison.OrdinalIgnoreCase]?.Description;
	}
}
