using Microsoft.EntityFrameworkCore;
using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.API.Services
{
    public class TransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ApplicationDbContext context,
            ILogger<TransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(string userId)
        {
            try
            {
                return await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetFilteredTransactionsAsync(
            string userId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? month = null,
            int? year = null)
        {
            try
            {
                var query = _context.Transactions
                    .Where(t => t.UserId == userId)
                    .AsNoTracking();

                if (startDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt <= endDate.Value);
                }

                if (month.HasValue)
                {
                    query = query.Where(t => t.CreatedAt.Month == month.Value);
                }

                if (year.HasValue)
                {
                    query = query.Where(t => t.CreatedAt.Year == year.Value);
                }

                return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered transactions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetUserBalanceAsync(string userId)
        {
            try
            {
                return await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .SumAsync(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating balance for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Transaction?> GetTransactionAsync(string id)
        {
            try
            {
                return await _context.Transactions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction {TransactionId}", id);
                throw;
            }
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                
                // Reload the transaction to ensure we have the latest data
                return await _context.Transactions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == transaction.Id) 
                    ?? throw new InvalidOperationException("Failed to create transaction");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction for user {UserId}", transaction.UserId);
                throw;
            }
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            try
            {
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == transaction.Id);

                if (existingTransaction == null)
                {
                    throw new InvalidOperationException($"Transaction {transaction.Id} not found");
                }

                existingTransaction.Amount = transaction.Amount;
                existingTransaction.Description = transaction.Description;
                existingTransaction.Type = transaction.Type;
                existingTransaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId}", transaction.Id);
                throw;
            }
        }

        public async Task DeleteTransactionAsync(string id)
        {
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transaction == null)
                {
                    throw new InvalidOperationException($"Transaction {id} not found");
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId}", id);
                throw;
            }
        }
    }
} 