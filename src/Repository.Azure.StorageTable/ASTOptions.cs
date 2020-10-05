using Foralla.KISS.Repository.Extensions;
using Microsoft.Azure.Cosmos.Table;

namespace Foralla.KISS.Repository
{
    /// <summary>
    ///     Configuration used by <see cref="ASTExtensions.AddAzureStorageTableRepository"/> to configure the underlying Azure Storage Table.
    /// </summary>
    public class ASTOptions
    {
        /// <summary>
        ///     Gets or sets the connection string for the Azure Storage Table or Cosmos DB Table storage.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets any custom configuration when working with Azure Cosmos DB Table storage.
        /// </summary>
        /// <remarks>
        ///     Used to do special configuration for the Azure Cosmos DB.
        /// </remarks>
        public TableClientConfiguration CosmosClientConfiguration { get; set; }
    }
}
