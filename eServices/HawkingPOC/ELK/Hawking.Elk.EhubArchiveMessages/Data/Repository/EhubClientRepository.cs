using System.Diagnostics.Contracts;
using Hawking.Elk.Common;
using Hawking.Elk.EhubArchiveMessages.Model;
using Microsoft.EntityFrameworkCore;

namespace Hawking.Elk.EhubArchiveMessages.Data.Repository
{
    public class EhubClientRepository : Repository<EhubClient>
    {
        public EhubClientRepository(DbContext dbContext) : base(dbContext)
        {
            Contract.Requires(dbContext != null);
        }
    }
}
