using System.ComponentModel.DataAnnotations;
using IHM_Distribution.Data.Repository;
using IHM_Distribution.Models;
using IHM_Distribution.Models.Common;
using IHM_Distribution.Services;
using Microsoft.EntityFrameworkCore;

namespace IHM_Distribution.Data
{
	public class AppDbContext : DbContextBase, ITruckContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IIdentityService identityService)
            : base(options, identityService)
        {
        }
    }
}