using System;

namespace Hawking.eHub.Mapping.Config.Services.Model
{
    internal class RankedTransformSet
    {
        internal Guid? SenderPK { get; set; }
        internal Guid? RecipientPK { get; set; }
        internal Guid? TransformSetId { get; set; }
        internal string XPathPredicate { get; set; }
        internal int MatchRank { get; set; }
    }
}
