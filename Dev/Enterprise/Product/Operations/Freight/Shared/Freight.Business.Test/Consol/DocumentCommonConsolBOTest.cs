using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DocumentCommonConsol))]
	sealed class DocumentCommonConsolBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return DocumentCommonConsol;
		}

		#region Consol

		CommonConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<CommonConsol>();
					fConsol.JK_TransportMode = Constants.TransportModes.Air;
					fConsol.JK_MasterBillNum = "08112345678";

					Transport transport = Consol.Transports[0];
					transport.JW_IsLinked = true;
					transport.JW_RL_NKLoadPort = "AUSYD";

					fConsol.Shipments.AddNew();
					fConsol.Shipments[0].JS_OuterPacks = 7;
				}
				return fConsol;
			}
		}
		CommonConsol fConsol;

		#endregion

		#region DocumentCommonConsol

		DocumentCommonConsol DocumentCommonConsol
		{
			get
			{
				if (fDocumentCommonConsol == null)
				{
					fDocumentCommonConsol = new DocumentCommonConsol(Consol, Constants.DataContext.LoadListConsol);
				}

				return fDocumentCommonConsol;
			}
		}
		DocumentCommonConsol fDocumentCommonConsol;

		#endregion

		protected override void SetUp()
		{
			CurrentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CurrentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CurrentCompanyCountryCode;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = CurrentBranchPort;
			base.TearDown();
		}

		ZString CurrentCompanyCountryCode;
		ZString CurrentBranchPort;

		#endregion
	}
}
