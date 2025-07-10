using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	public abstract partial class InBondDataObjectWriterTest : InBondHelperTest
	{
		public void TestMergeInBondWithDeclaration()
		{
			var declaration = CreateBizObjWithParent(out var header);
			if (declaration != null)
			{
				var universalShipment = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
				AssertNotEquals("universalShipment.VesselName", "BOB VESSEL", universalShipment.VesselName.GetValueOrDefault());
				AssertEquals("universalShipment.SubShipmentCollection", 1, universalShipment.SubShipmentCollection.Count);
				var subShipment = universalShipment.SubShipmentCollection[0];
				var dataSource = subShipment.GetMatchingDataSource(GetDataContextType);
				AssertEquals(header.BH_JobReference, dataSource.Key.GetValueOrDefault());
				AssertEquals("subShipment.VesselName", "BOB VESSEL", subShipment.VesselName);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDataObjectWriterSchemaVersion()
		{
			var bizObj = CreateBizObjWithParent(out var _);
			if (bizObj != null)
			{
				Factory.SaveForTesting();
				using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
				{
					var universalShipment = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, bizObj);
					AssertNotEquals("universalShipment.VesselName", "BOB VESSEL", universalShipment.VesselName.GetValueOrDefault());
					AssertEquals("universalShipment.SubShipmentCollection", 1, universalShipment.SubShipmentCollection.Count);
					var subShipment = universalShipment.SubShipmentCollection[0];
					AssertEquals("Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.DataContext", subShipment.DataContext.GetType().FullName);
				}

				using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
				{
					var universalShipment = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, bizObj);
					AssertNotEquals("universalShipment.VesselName", "BOB VESSEL", universalShipment.VesselName.GetValueOrDefault());
					AssertEquals("universalShipment.SubShipmentCollection", 1, universalShipment.SubShipmentCollection.Count);
					var subShipment = universalShipment.SubShipmentCollection[0];
					AssertEquals("Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext", subShipment.DataContext.GetType().FullName);
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected void AssertContainDate(List<Date> dateCollection, DateType dateType, ZBool isEtimated, ZDateTime? date)
		{
			var dateData = dateCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == dateType && x.IsEstimate.GetValueOrDefault() == isEtimated);
			AssertNotNull(string.Format("Precondition: dateCollection should contain DateType({0})", dateType.ToString()), dateData);
			AssertEquals("dateData.Value", date, dateData.Value);
		}

		protected void AssertContainNote(DataObjectList<Note> noteCollection, ZBool isCustomDescription, ZString description, ZString noteText)
		{
			var noteData = noteCollection.FirstOrDefault(x => x.Description.GetValueOrDefault() == description);
			AssertNotNull(string.Format("Precondition: notteCollection should contain Description({0})", description), noteData);
			AssertEquals("noteData.IsCustomDescription", isCustomDescription, noteData.IsCustomDescription);
			AssertEquals("noteData.Description", description, noteData.Description);
			AssertEquals("noteData.NoteText", noteText, noteData.NoteText);
		}

		protected void AssertInBondDisposition(UniversalCustoms.AddInfoGroup dispositionData, ZString? code, ZDateTime? date, ZShort? order)
		{
			AssertNotNull("dispositionData.Type", dispositionData.Type);
			AssertEquals("dispositionData.Type.Code", CusAddInfoTypeAttribute.Codes.USDisposition, dispositionData.Type.Code);
			AssertEquals("dispositionData.Type.Description", "Disposition", dispositionData.Type.Description);
			AssertEquals("dispositionData.Code", code, dispositionData.AddInfoCollection.GetZStringValue(USDispositionDataAddInfoSchema.US_Code.Name.Substring(3)));
			AssertEquals("dispositionData.Date", date, dispositionData.AddInfoCollection.GetZDateTimeValue(USDispositionDataAddInfoSchema.US_DispositionDate.Name.Substring(3)));
			AssertEquals("dispositionData.Order", order, dispositionData.AddInfoCollection.GetZShortValue(USDispositionDataAddInfoSchema.US_Order.Name.Substring(3)));
		}

		protected DispositionData SetupDisposition(DispositionData dispositionData, ZString code, ZDateTime date, ZShort order)
		{
			dispositionData.US_Code = code;
			dispositionData.US_DispositionDate = date;
			dispositionData.US_Order = order;
			return dispositionData;
		}

		protected abstract BusinessObject CreateBizObjWithParent(out CusInBondHeader header);
		protected abstract DataContextType GetDataContextType { get; }
	}
}
