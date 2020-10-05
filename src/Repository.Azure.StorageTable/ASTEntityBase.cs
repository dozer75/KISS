using System;
using Microsoft.Azure.Cosmos.Table;

namespace Foralla.KISS.Repository
{
    public abstract class ASTEntityBase : TableEntity, IEntityBase<string>
    {
        [IgnoreProperty]
        public string Id
        {
            get => PartitionKey != null && RowKey != null ? $"{PartitionKey}-{RowKey}" : null;
            set
            {
                if (value is null)
                {
                    PartitionKey = null;
                    RowKey = null;

                    return;
                }

                var sections = value.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);

                if (sections.Length != 2)
                {
                    throw new ArgumentException($"{value} is not valid. The id must be in format {{PartitionKey}}-{{RowKey}}.", nameof(value));
                }

                PartitionKey = sections[0];
                RowKey = sections[1];
            }
        }
    }
}
