using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgARTermsInstallmentCollection : DependentBusinessObjectCollection<OrgARTermsInstallment, OrgARTerms>
	{
		public OrgARTermsInstallmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgARTermsInstallmentCollection(OrgARTerms master)
			: base(master)
		{
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var aRTermsInstallment = (OrgARTermsInstallment)dependent;
			aRTermsInstallment.ML_PY_Terms = Master.PK;
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject dependent, bool forDelete)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject newElement)
		{
			base.SetDefaultsForNewChild(newElement);
			var aRTermsInstallment = (OrgARTermsInstallment)newElement;
			aRTermsInstallment.ML_SequenceNumber = (ZByte)(Master.ARTermsInstallments.Count + 1);
			aRTermsInstallment.ML_PY_Terms = Master.PK;
			aRTermsInstallment.ML_AgreedPaymentMethod = Master.PY_AgreedPaymentMethod;

			if (ReadOnly)
			{
				newElement.SetReadOnlyIncludingChildren(true);
			}
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgARTermsInstallmentSchema.ML_PY_Terms; }
		}
	}
}
