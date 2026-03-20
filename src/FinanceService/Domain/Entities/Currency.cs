namespace FinanceService.Domain.Entities;

public class Currency
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}
