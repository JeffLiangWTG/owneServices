using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class CUSCAREDIMessage : SARSEDIMessage
	{
		public CUSCAREDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.CUSCAR;  // Used to construct "SARSCAR" for UNB recipient - don't change this

			var registry = ZACustomsRegistry.Instance.DefaultBranchForManifestSubmission.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			if (!string.IsNullOrEmpty(registry))
			{
				var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, registry));
				if (branch != null)
				{
					EM_GB = branch.PK;
					EM_IsTestMessage = Env.Registry.ZACustoms.GetIsTestMode(branch);
				}
			}
		}

		protected override string GetSendersReference()
		{
			return "000002";
		}

		public override ZString LocalReferenceNumber
		{
			get
			{
				if (!lrn.HasValue)
				{
					lrn = CUSCARD16AHelper?.UniqueReferenceNumber ?? ZString.Empty;
				}
				return lrn.Value;
			}
		}
		ZString? lrn;

		public D16AMessageHelper CUSCARD16AHelper => cuscarHelper ?? (cuscarHelper = D16AMessageHelper.New(this));
		D16AMessageHelper cuscarHelper;
	}
}
