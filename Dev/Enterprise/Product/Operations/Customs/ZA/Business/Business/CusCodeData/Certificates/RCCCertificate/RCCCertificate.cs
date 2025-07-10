using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class RCCCertificate : CertificateCusCodeData
	{
		public RCCCertificate(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Override Methods

		public override void Delete()
		{
			EntryInstruction?.RCCCertificateLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			base.Delete();
		}

		protected override ZString CusCodeDataTypeCode => PermitTypeList.Codes.RCC;

		#endregion

		#region Lookups / Validation

		public new RCCCertificateLookups Lookups
		{
			get { return (RCCCertificateLookups)base.Lookups; }
		}

		public new RCCCertificateValidation Validation
		{
			get { return (RCCCertificateValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new RCCCertificateValidation(this);
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new RCCCertificateLookups(this);
		}

		protected override IEnumerable<CertificateCusCodeData> GetCertificateCollectionCore(CusEntryInstruction instruction) => instruction.RCCCertificates.OfType<CertificateCusCodeData>();

		protected override void RecalculateWhenAboutToBeDetachedOrDeleted(CusEntryInstruction instruction)
		{
			instruction?.RCCCertificateLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		protected override void RecalculateWhenAdded()
		{
			EntryInstruction?.RCCCertificateLineNumberGenerator.RecalculateWhenAdded(this);
		}

		protected override void RecalculateWhenRenumbered(ZShort oldValue)
		{
			EntryInstruction?.RCCCertificateLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
		}

		#endregion
	}
}
