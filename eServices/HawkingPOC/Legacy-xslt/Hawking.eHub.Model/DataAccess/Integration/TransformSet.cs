using System.Collections.Generic;

namespace Hawking.eHub.Model.DataAccess.Integration
{
    public class TransformSet
    {
        public TransformSet(List<TransformDetail> transforms, string xpathPredicate)
        {
            Transforms = transforms;
            XpathPredicate = xpathPredicate;
        }

        public readonly List<TransformDetail> Transforms;
        public readonly string XpathPredicate;
    }
}
