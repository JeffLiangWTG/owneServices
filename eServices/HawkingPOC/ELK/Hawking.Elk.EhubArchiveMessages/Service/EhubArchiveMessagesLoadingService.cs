using System;
using System.Collections.Generic;
using System.Linq;
using Hawking.Elk.EhubArchiveMessages.Config;
using Hawking.Elk.EhubArchiveMessages.Data.Context;
using Hawking.Elk.EhubArchiveMessages.Data.Repository;
using Hawking.Elk.EhubArchiveMessages.Model;
using Microsoft.EntityFrameworkCore;

namespace Hawking.Elk.EhubArchiveMessages.Service
{
    public class EhubArchiveMessagesLoadingService : IEhubArchiveMessagesLoadingService
    {
        static EhubArchiveMessageRepository ehubArchiveMessageRepository;
        static EhubClientRepository ehubClientRepository;
        static IEnumerable<EhubClient> ehubClients;

        public EhubArchiveMessagesLoadingService(IKafkaConfig kafkaConfig)
        {
            var archiveOnlineOptions =
                SqlServerDbContextOptionsExtensions.UseSqlServer(
                    new DbContextOptionsBuilder(),
                    kafkaConfig.ArchiveDbConnectionString).Options;

            var ehubClientptions =
                SqlServerDbContextOptionsExtensions.UseSqlServer(
                    new DbContextOptionsBuilder(),
                    kafkaConfig.EhubClientDbConnectionString).Options;

            ehubArchiveMessageRepository = new EhubArchiveMessageRepository(new EhubArchiveOnlineDbContext(archiveOnlineOptions));
            ehubClientRepository = new EhubClientRepository(new EhubClientDbContext(ehubClientptions));
        }

        public IEnumerable<EhubArchiveMessage> LoadEhubArchiveMessages(DateTime fromArchivedUtc, int batchSize)
        {
            var query = ehubArchiveMessageRepository
                .FindWhere(x => x.ArchivedUTC > fromArchivedUtc)
                .OrderBy(x => x.ArchivedUTC)
                .AsNoTracking()
                .Take(batchSize);

            return query.ToArray();
        }

        public static IEnumerable<EhubClient> EhubClients
        {
            get
            {
                if (ehubClients == null)
                {
                    ehubClients = ehubClientRepository.FindWhere().AsNoTracking().ToArray();
                }

                return ehubClients;
            }
        }
    }
}
