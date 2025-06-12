namespace Hawking.eHub.Model.DataAccess.Integration
{
    public class TransformDetail
    {
        public TransformDetail(string typeName, string targetMessageType)
        {
            MapTypeName = typeName;
            TargetMessageType = targetMessageType;
        }

        public readonly string MapTypeName;
        public readonly string TargetMessageType;
    }
}
