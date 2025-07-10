using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class CensusWarningQuery : AutoCensusWarningQuery, IACECensusWarningQuery
	{
		public CensusWarningQuery(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region IACECensusWarningQuery Members

		ZString IQueryMessageAttachee.ProcessingDistrictPort
		{
			get { return ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(GlbBranch.CurrentBranch); }
		}

		ZString IQueryMessageAttachee.ProcessingOfficeCode
		{
			get { return USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(GlbBranch.CurrentBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty); }
		}

		EDIMessageCollection IQueryMessageAttachee.Messages
		{
			get { return null; }
		}

		IEnumerable<(ZString, ZString)> IQueryMessageAttachee.EntryFilerCodesAndNumbers
		{
			get
			{
				yield return (EntryFilerCode, EntryNumber);
			}
		}

		ZDateTime IQueryMessageAttachee.DateFrom
		{
			get { return DateFrom; }
		}

		ZDateTime IQueryMessageAttachee.DateTo
		{
			get { return DateTo; }
		}

		ZString IACECensusWarningQuery.DistrictPortOfEntry
		{
			get { return DistrictPortCode; }
		}

		Guid IQueryMessageAttachee.CompanyPK
		{
			get { return GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		GlbBranch IACECensusWarningQuery.Branch
		{
			get { return Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EntryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(GlbBranch.CurrentBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;
		}
	}
}
