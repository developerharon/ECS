using Ecs.Contexts;

namespace Ecs.Services
{
    public class TimestampServices : ITimestampServices
    {
        private readonly ApplicationDbContext _context;

        public TimestampServices(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
