using System.ComponentModel.DataAnnotations;

namespace server.Models 
{
  public class Coin
  {
    public int Id { get; set; }
    [ConcurrencyCheck]
    public int Value { get; set; }
    [ConcurrencyCheck]
    public int Quantity { get; set; }
    public bool IsBlocked { get; set; }
    public Device Device { get; set; }
  }
}