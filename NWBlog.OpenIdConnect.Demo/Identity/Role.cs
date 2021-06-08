using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWBlog.OpenIdConnect.Demo.Identity
{
    [Table(nameof(Role))]
    public class Role : IdentityRole<long>
    {
        public List<UserRole> UserRoles { get; set; }
    }
}
