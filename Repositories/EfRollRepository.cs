using WarehouseRolls.Models;
using WarehouseRolls.Data;
using Microsoft.EntityFrameworkCore;

namespace WarehouseRolls.Repositories
{
    public class EfRollRepository : IRollRepository
    {
        private readonly WarehouseRollsDbContext _context;

        public EfRollRepository(WarehouseRollsDbContext context)
        {
            _context = context;
        }

        public async Task<Roll> CreateAsync(Roll roll, CancellationToken ct = default)
        {
            await _context.AddAsync(roll);
            await _context.SaveChangesAsync();
            return roll;
        }

        public async Task<Roll?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Rolls.FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<Roll> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var roll = await _context.Rolls.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (roll == null)
            {
                return null;
            }

            roll.DeletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
            return roll;
        }

        public IQueryable<Roll> Query()
        {
            return _context.Rolls.AsQueryable();
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }
    }
}