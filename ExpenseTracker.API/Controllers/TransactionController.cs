using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Services;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            TransactionService transactionService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Transaction>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions");
                return StatusCode(500, new { message = "An error occurred while retrieving transactions" });
            }
        }

        [HttpGet("filter")]
        [ProducesResponseType(typeof(IEnumerable<Transaction>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetFilteredTransactions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                {
                    return BadRequest(new { message = "Start date cannot be after end date" });
                }

                var transactions = await _transactionService.GetFilteredTransactionsAsync(
                    userId, startDate, endDate, month, year);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered transactions");
                return StatusCode(500, new { message = "An error occurred while retrieving filtered transactions" });
            }
        }

        [HttpGet("balance")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<decimal>> GetBalance()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var balance = await _transactionService.GetUserBalanceAsync(userId);
                return Ok(new { balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance");
                return StatusCode(500, new { message = "An error occurred while retrieving balance" });
            }
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(TransactionSummary), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TransactionSummary>> GetSummary()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                var summary = new TransactionSummary
                {
                    TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    TotalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                    Balance = transactions.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction summary");
                return StatusCode(500, new { message = "An error occurred while retrieving transaction summary" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Transaction), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] Transaction transaction)
        {
            try
            {
                _logger.LogInformation("Received transaction creation request: {@Transaction}", transaction);

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                transaction.UserId = userId;
                transaction.CreatedAt = DateTime.UtcNow;

                var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
                _logger.LogInformation("Transaction created successfully: {@Transaction}", createdTransaction);

                return CreatedAtAction(nameof(GetTransactions), new { id = createdTransaction.Id }, createdTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                return StatusCode(500, new { message = "An error occurred while creating the transaction" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTransaction(string id, [FromBody] Transaction transaction)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingTransaction = await _transactionService.GetTransactionAsync(id);
                if (existingTransaction == null || existingTransaction.UserId != userId)
                {
                    return NotFound(new { message = "Transaction not found" });
                }

                transaction.Id = id;
                transaction.UserId = userId;
                transaction.CreatedAt = existingTransaction.CreatedAt;
                transaction.UpdatedAt = DateTime.UtcNow;

                await _transactionService.UpdateTransactionAsync(transaction);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the transaction" });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var transaction = await _transactionService.GetTransactionAsync(id);
                if (transaction == null || transaction.UserId != userId)
                {
                    return NotFound(new { message = "Transaction not found" });
                }

                await _transactionService.DeleteTransactionAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the transaction" });
            }
        }
    }
} 