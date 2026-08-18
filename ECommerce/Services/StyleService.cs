using ECommerce.DBContext;
using ECommerce.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services
{
    public interface IStyleService
    {
        Task<List<Style>> GetAllAsync();
        Task<Style?> GetByIdAsync(int id);
    }

    public class StyleService : IStyleService
    {
        private readonly AppDbContext _context;

        public StyleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Style>> GetAllAsync()
        {
            return await _context.Styles
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Style?> GetByIdAsync(int id)
        {
            return await _context.Styles.FindAsync(id);
        }
    }
}
