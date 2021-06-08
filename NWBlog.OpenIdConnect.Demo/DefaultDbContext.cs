using Microsoft.EntityFrameworkCore;

namespace NWBlog.OpenIdConnect.Demo
{
    public class DefaultDbContext : DbContext
    {
        // entity sets here...

        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options) { }

        // ...
    }
}
