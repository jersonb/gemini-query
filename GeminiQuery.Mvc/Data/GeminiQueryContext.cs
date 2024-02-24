using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeminiQuery.Mvc.Models;

namespace GeminiQuery.Mvc.Data
{
    public class GeminiQueryContext : DbContext
    {
        public GeminiQueryContext (DbContextOptions<GeminiQueryContext> options)
            : base(options)
        {
        }

        public DbSet<GeminiQuery.Mvc.Models.Question> Question { get; set; } = default!;
    }
}
