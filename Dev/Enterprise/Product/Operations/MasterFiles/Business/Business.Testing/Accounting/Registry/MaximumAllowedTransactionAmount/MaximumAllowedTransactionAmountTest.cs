using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MaximumAllowedTransactionAmount))]
	class MaximumAllowedTransactionAmountTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new MaximumAllowedTransactionAmount();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MaximumAllowedTransactionAmount();
		}

		protected new MaximumAllowedTransactionAmount BizObj
		{
			get { return (MaximumAllowedTransactionAmount)base.BizObj; }
		}

		#endregion
	}
}
