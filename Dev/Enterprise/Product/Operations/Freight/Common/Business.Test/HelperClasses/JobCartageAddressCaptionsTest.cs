using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class JobCartageAddressCaptionsTest : TestCaseWithFactory
	{
		#region TestGetRelevantDocAddressType

		public void TestGetRelevantDocAddressType()
		{
			var cartageType = Factory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, Constants.CartageJobType.NEW_AirExport));
			AssertEquals(DocAddressType.LocalCartageExporter, CommonCartageAddressHelper.GetRelevantDocAddressType(cartageType.LooseBooking));

			cartageType = Factory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, Constants.CartageJobType.NEW_AirImport));
			AssertEquals(DocAddressType.LocalCartageImporter, CommonCartageAddressHelper.GetRelevantDocAddressType(cartageType.LooseBooking));

			cartageType = Factory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, Constants.CartageJobType.NEW_FCLExportToSHP));
			AssertEquals(DocAddressType.LocalCartageExporter, CommonCartageAddressHelper.GetRelevantDocAddressType(cartageType.ContainerizedBooking));

			cartageType = Factory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, Constants.CartageJobType.NEW_EmptyCYDtoCFS));
			AssertEquals(DocAddressType.LocalCartageCFS, CommonCartageAddressHelper.GetRelevantDocAddressType(cartageType.ContainerizedBooking));

			cartageType = Factory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, Constants.CartageJobType.NEW_FCLUnpackLooseToCNE));
			AssertEquals(DocAddressType.LocalCartageImporter, CommonCartageAddressHelper.GetRelevantDocAddressType(cartageType.LooseBooking));
			AssertEquals(DocAddressType.LocalCartageCFS, CommonCartageAddressHelper.GetRelevantDocAddressType(cartageType.ContainerizedBooking));

			var list = new List<DocAddressType>() { DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageCFS };
			AssertEquals(DocAddressType.LocalCartageImporter, CommonCartageAddressHelper.GetRelevantDocAddressType(list));

			list = new List<DocAddressType>() { DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCFS };
			AssertEquals(DocAddressType.LocalCartageExporter, CommonCartageAddressHelper.GetRelevantDocAddressType(list));

			list = new List<DocAddressType>() { DocAddressType.LocalCartageCFS };
			AssertEquals(DocAddressType.LocalCartageCFS, CommonCartageAddressHelper.GetRelevantDocAddressType(list));
		}

		#endregion
		#region TestGetOrgTypeDescriptionFromCartageDocAddressType

		public void TestGetOrgTypeDescriptionFromCartageDocAddressType()
		{
			AssertEquals(LocalCartageJobOrgTypeList.Descriptions.CFS, CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.LocalCartageCFS));
			AssertEquals(LocalCartageJobOrgTypeList.Descriptions.CTO, CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.LocalCartageCTO));
			AssertEquals(LocalCartageJobOrgTypeList.Descriptions.CNE, CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.LocalCartageImporter));
			AssertEquals(LocalCartageJobOrgTypeList.Descriptions.CNR, CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.LocalCartageExporter));
			AssertEquals(LocalCartageJobOrgTypeList.Descriptions.CYD, CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.LocalCartageYard));
			AssertEquals(LocalCartageJobOrgTypeList.Descriptions.SRV, CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.LocalCartageService));
			AssertEquals(LocalCartageJobOrgTypeList.Descriptions.WHS, CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.LocalCartageWarehouse));
			AssertEquals(LocalCartageJobOrgTypeList.Descriptions.MSC, CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.LocalCartageMSC));
			AssertEquals("", CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.NonPersistent));
			AssertEquals("", CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(DocAddressType.None));
		}

		#endregion
	}
}
