using System;

namespace Hawking.Elk.Common.Model
{
    public interface IEntity
    {
        Guid TrackingId { get; }
    }
}
