using System.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(TrackingOrderFilterValueObjectDataAdapter))]
	sealed class TrackingOrderFilterValueObjectDataAdapterTest : ValueObjectDataAdapterTest<TrackingOrderFilterBusinessObject_Old_ForWeb, Xsd.WebOrderFilter>
	{
		#region Test business objects

		OrgHeader fTestOrg;
		OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.New<OrgHeader>();
					fTestOrg.OH_Code = "TEST";
					fTestOrg.OH_FullName = "Test Company";
					fTestOrg.OH_RL_NKClosestPort = "AUSYD";
				}
				return fTestOrg;
			}
		}

		RefVessel fVessel;
		RefVessel Vessel
		{
			get
			{
				if (fVessel == null)
				{
					fVessel = Factory.New<RefVessel>();
					fVessel.RV_Code = "TESTVESSEL";
				}
				return fVessel;
			}
		}

		#endregion

		protected override string ExpectedRootCollectionElementName
		{
			get { return "WebOrderFilters"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "WebOrderFilter"; }
		}

		protected override ValueObjectDataAdapter<TrackingOrderFilterBusinessObject_Old_ForWeb, Xsd.WebOrderFilter> GetNewBizObjXmlDataAdapter()
		{
			return new TrackingOrderFilterValueObjectDataAdapter();
		}

		protected override TrackingOrderFilterBusinessObject_Old_ForWeb NewBusinessObject()
		{
			WebFilterBusinessObjectFactory filterFactory = new WebFilterBusinessObjectFactory(Factory);
			return filterFactory.New<TrackingOrderFilterBusinessObject_Old_ForWeb>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Base tests have SOURCE_CODE")]
		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			TrackingOrderFilterBusinessObject_Old_ForWeb filterBO = NewBusinessObject();
			return new BusinessObjectAndExpectedOutputFileName(filterBO, Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Tracking\Tracking.Business\Test\EmptyTrackingOrderFilter.xml"), ValidationKind.None, "Empty TrackingOrder Filter Business Object");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Base tests have SOURCE_CODE")]
		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			TrackingOrderFilterBusinessObject_Old_ForWeb filterBO = NewBusinessObject();
			filterBO.JD_DateFilterType = "ALL";
			filterBO.JD_FromDate = new ZDateTime(2004, 04, 11);
			filterBO.JD_ToDate = new ZDateTime(2007, 10, 03);

			return new BusinessObjectAndExpectedOutputFileName(filterBO, Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Tracking\Tracking.Business\Test\EmptyTrackingOrderFilter.xml"), ValidationKind.None, "Empty TrackingOrder Filter Business Object");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Base tests have SOURCE_CODE")]
		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}
	}
}
