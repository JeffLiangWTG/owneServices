using System;
namespace CargoWise.eHub.DataAccess.Integration
{
	public class TransformDetail
	{
		public TransformDetail(Guid transformSetID, string typeName, string targetMessageType)
		{
            TransformSetID = transformSetID;
			MapTypeName = typeName;
			TargetMessageType = targetMessageType;
		}

        public readonly Guid TransformSetID;
		public readonly string MapTypeName;
		public readonly string TargetMessageType;

		public static implicit operator TransformDetail(eServices.eHubDataAccess.Integration.TransformDetail transformDetail)
			=> transformDetail is null ? null : new TransformDetail(transformDetail.TransformSetID, transformDetail.MapTypeName, transformDetail.TargetMessageType);
	}
}
