using System;

namespace Foralla.KISS.Repository
{
    public interface IEntityBase
    {
        Guid Id { get; set; }
    }
}
