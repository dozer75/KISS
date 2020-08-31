using MongoDB.Driver;

namespace Foralla.KISS.Repository
{
    public class MongoOptions
    {
        internal MongoOptions()
        {
        }

        public ClientSessionOptions ClientSessionOptions { get; set; }

        public string DatabaseName { get; set; }

        public MongoDatabaseSettings DatabaseSettings { get; set; }

        public MongoClientSettings ServerSettings { get; set; }

        public TransactionOptions TransactionOptions { get; set; }
    }
}
