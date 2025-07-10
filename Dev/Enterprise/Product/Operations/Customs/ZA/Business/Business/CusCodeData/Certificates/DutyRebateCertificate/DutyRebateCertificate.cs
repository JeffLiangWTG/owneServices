using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class DutyRebateCertificate : CertificateCusCodeData
	{
		public DutyRebateCertificate(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Override Methods

		public override void Delete()
		{
			EntryInstruction?.DutyRebateCertificateLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			base.Delete();
		}

		protected override ZString CusCodeDataTypeCode => CusCodeDataTypeList.Codes.DRC;

		#endregion

		#region Lookups / Validation

		public new DutyRebateCertificateLookups Lookups
		{
			get { return (DutyRebateCertificateLookups)base.Lookups; }
		}

		public new DutyRebateCertificateValidation Validation
		{
			get { return (DutyRebateCertificateValidation)base.Validation; }
		}

		protected override IEnumerable<CertificateCusCodeData> GetCertificateCollectionCore(CusEntryInstruction instruction) => instruction.DutyRebateCertificates.OfType<CertificateCusCodeData>();

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new DutyRebateCertificateValidation(this);
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new DutyRebateCertificateLookups(this);
		}

		protected override void RecalculateWhenAboutToBeDetachedOrDeleted(CusEntryInstruction instruction)
		{
			instruction?.DutyRebateCertificateLineNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		protected override void RecalculateWhenAdded()
		{
			EntryInstruction?.DutyRebateCertificateLineNumberGenerator.RecalculateWhenAdded(this);
		}

		protected override void RecalculateWhenRenumbered(ZShort oldValue)
		{
			EntryInstruction?.DutyRebateCertificateLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
		}

		#endregion
	}
}
