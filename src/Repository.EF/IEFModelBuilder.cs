using Microsoft.EntityFrameworkCore;

namespace Foralla.KISS.Repository
{
    public interface IEFModelBuilder
    {
        void CreateModel(ModelBuilder builder);
    }
}
