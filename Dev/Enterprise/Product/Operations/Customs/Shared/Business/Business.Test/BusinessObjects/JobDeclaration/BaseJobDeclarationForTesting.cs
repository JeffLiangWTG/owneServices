using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobDeclarationForTesting : BaseJobDeclaration
	{
		public BaseJobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCodeCoreExposed
		{
			get { return LocalCurrencyCodeCore; }
		}

		protected override bool SupportInwardProcessingCore => true;

		protected override IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListCore
		{
			get { return new ZString[] { JE_ApplicationCode, "XXX", "YYY" }; }
		}

		[ReadOnlyMember(nameof(PortsAreReadOnly))]
		public override ZString JE_RL_NKFinalDestination
		{
			get { return base.JE_RL_NKFinalDestination; }
			set { base.JE_RL_NKFinalDestination = value; }
		}

		[ReadOnlyMember(nameof(PortsAreReadOnly))]
		public override ZString JE_RL_NKPortOfLoading
		{
			get { return base.JE_RL_NKPortOfLoading; }
			set { base.JE_RL_NKPortOfLoading = value; }
		}

		protected override ZQuery GetDiscardedMessagesFilter()
		{
			return new ZQuery(EDIMessageSchema.EM_ApplicationCode, "ZZZ");
		}

		public bool PortsAreReadOnly { get; set; }

		public ZString? GetImporterEquipment_Exposed()
		{
			return GetImporterEquipment();
		}

		public ZString? GetSupplierEquipment_Exposed()
		{
			return GetSupplierEquipment();
		}

		internal bool IsDPSFreightMovementRestrictedCore_Exposed()
		{
			return IsDPSFreightMovementRestrictedCore();
		}

		public override bool UseImporterAddress => true;
		public override bool UseSupplierAddress => true;

		protected  override ZBool ShouldDefaultContainerModeAndIsContainerisedCore(ZString containerMode)
		{
			return containerMode == "FCL";
		}
	}
}
