using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.Models;

public enum TransactionType
{
    Income,
    Expense
}

public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public TransactionType Type { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 