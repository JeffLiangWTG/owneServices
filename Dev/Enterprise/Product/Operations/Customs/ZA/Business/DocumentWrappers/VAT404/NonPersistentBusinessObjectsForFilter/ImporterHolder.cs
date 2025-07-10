using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ImporterHolder : AutoImporterHolder
	{
		public ImporterHolder(VAT404DocumentInstruction parent) : base(parent.Factory)
		{
			Parent = parent;
		}
		internal VAT404DocumentInstruction Parent;

		#region Importer

		[RelatedBusinessObject("Importer")]
		[List(nameof(Importers))]
		public override ZGuid ImporterPK
		{
			get { return base.ImporterPK; }
			set { base.ImporterPK = value; }
		}

		public OrgHeader Importer
		{
			get { return Factory.Load<OrgHeader>(ImporterPK); }
		}

		public OrgHeaderCollection Importers => new OrgHeaderCollection(Factory);

		#endregion
	}

	public class ImporterHolderValidation : AutoImporterHolderValidation
	{
		public ImporterHolderValidation(AutoImporterHolder parent) : base(parent)
		{
		}

		VAT404DocumentInstruction ParentInstruction => (Parent as ImporterHolder).Parent;

		protected override void CheckImporterPK()
		{
			base.CheckImporterPK();
			var targetInfo = Parent.ImporterPKInfo;
			MandatoryValidation.WarnIfNotEntered(targetInfo);
			if (ParentInstruction.ImporterForFilter.OfType<ImporterHolder>().Any(x => x != Parent && x.ImporterPK == Parent.ImporterPK))
			{
				targetInfo.AddWarning(Res.GetString("A91C5D1B-8BD9-4D5B-9D8B-568FC4A88DF0", "This value has already been entered."));
			}
		}
	}
}
