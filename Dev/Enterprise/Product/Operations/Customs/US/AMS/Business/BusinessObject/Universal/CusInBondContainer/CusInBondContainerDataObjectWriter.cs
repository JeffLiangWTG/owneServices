using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondContainerDataObjectWriter : DataTransfer.Universal.CusInBondContainerDataObjectWriter
	{
		public CusInBondContainerDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper)
			: base(writeManager, helper)
		{
		}

		protected override void PopulateInBondSpecificData(Customs.Business.CusInBondContainer containerBO, Container containerData)
		{
			base.PopulateInBondSpecificData(containerBO, containerData);
			var amsContainerBO = (CusInBondContainer)containerBO;
			containerData.IsEmptyContainer = containerBO.BC_IsEmpty;
			PopulateAddInfosData(amsContainerBO, containerData);
			PopulateCustomsReferences(amsContainerBO, containerData);
		}

		void PopulateAddInfosData(CusInBondContainer containerBO, Container containerData)
		{
			var list = containerData.AddInfoCollection ?? new List<AddInfo>();
			list.Add(new AddInfo() { Key = Constants.Container.AddInfo.ForeignPort, Value = containerBO.BC_RL_NKForeignPort });
			list.Add(new AddInfo() { Key = Constants.Container.AddInfo.ForeignPortScheduleK, Value = containerBO.BC_ForeignPortKCode });
			list.Add(new AddInfo() { Key = Constants.Container.AddInfo.TypeOfService, Value = containerBO.BC_TypeOfService });
			containerData.AddInfoCollection = list;
		}

		void PopulateCustomsReferences(CusInBondContainer containerBO, Container containerData)
		{
			var customsReferenceCollection = containerData.CustomsReferenceCollection ?? new List<UniversalCustoms.CustomsReference>();
			foreach (var customsReferenceBO in containerBO.Vehicles)
			{
				customsReferenceCollection.Add(new UniversalCustoms.CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = Constants.VehicleReference.Type, Description = Constants.VehicleReference.TypeDescription },
					Reference = customsReferenceBO.BV_VIN
				});
			}
			containerData.CustomsReferenceCollection = customsReferenceCollection;
		}
	}
}
