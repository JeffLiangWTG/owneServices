using System;
using System.ServiceModel;

namespace CargoWise.eHub.Products.NZCustoms.Client
{
    public static class CommunicationObjectExtension
    {
        public static void RunAndClose<T>(this T client, Action<T> work)
            where T : ICommunicationObject
        {
            try
            {
                work(client);
                client.Close();
            }
            catch (Exception)
            {
                client.Abort();
                throw;
            }
        }
        
        public static U RunAndClose<T, U>(this T client, Func<T,U> work)
            where T : ICommunicationObject
        {
            try
            {
                U result = work(client);
                client.Close();
                return result;
            }
            catch (Exception)
            {
                client.Abort();
                throw;
            }
        }
    }
}
